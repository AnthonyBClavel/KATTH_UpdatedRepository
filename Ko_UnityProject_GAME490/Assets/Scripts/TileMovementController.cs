using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TileMovementController : MonoBehaviour
{
    [Header("Bools")]
    public bool isWalking;                                      
    public bool hasDied = false;
    public bool canMove = true;
    public bool canInteract = true;
    public bool canRestartPuzzle;                               // Determines when the player can restart puzzle
    public bool canSetBoolsTrue;
    private bool isPushing;                                     
    private bool isInteracting;
    private bool alreadyPlayedSFX;
    private bool hasAlreadyPopedOut;                            // Determines when the torch meter can scale in/out
    private bool hasMovedPuzzleView = false;                    // Determines if the camera can move to a new puzzle view  
    private bool hasDisabledFootsteps = false;

    float speed = 5f;                                           // The speed at which the object will move from its current position to the destination
    float rayLength = 1f;                                       // The ray length for the tile movement
    float rayLengthEdgeCheck = 1f;

    public Vector3 playerDirection;
    Vector3 nextPos, destination, direction;

    Vector3 up = Vector3.zero,                                  // Object look North
    right = new Vector3(0, 90, 0),                              // Object look East
    down = new Vector3(0, 180, 0),                              // Object look South
    left = new Vector3(0, 270, 0),                              // Object look West
    currentDirection = Vector3.zero;                            // Default state (North)

    public GameObject edgeCheck;
    private GameObject dialogueViewsHolder;
    private Animator playerAnim;
    //private string currentState;

    [Header("Audio")]
    public AudioClip[] swooshClips;
    private AudioSource audioSource;

    [Header("Prefabs")]
    public GameObject destroyedBlockParticle;
    public GameObject torchFireIgniteSFX;
    public GameObject torchFireExtinguishSFX;
    public GameObject freezingSFX;

    [Header("Save Slot Elements")]
    public GameObject checkpoint;
    public GameObject puzzle;
    //public string sceneName;

    [Header("Scripts")]
    public TorchMeterStat torchMeterMoves;
    private LevelManager levelManagerScript;
    private TorchMeterScript torchMeterScript;
    private SaveManagerScript saveManagerScript;
    private IceMaterialScript iceMaterialScript;
    private GameHUD gameHUDScript;
    private CharacterDialogue characterDialogueScript;
    private PlayerSounds playerSoundsScript;
    private DialogueCameraViews dialogueCameraViewsScript;
    private CameraController cameraScript;
    private FidgetAnimControllerPlayer playerFidgetScript;

    void Awake()
    {
        torchMeterMoves.Initialize();

        levelManagerScript = FindObjectOfType<LevelManager>();
        torchMeterScript = FindObjectOfType<TorchMeterScript>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        iceMaterialScript = FindObjectOfType<IceMaterialScript>();
        playerSoundsScript = FindObjectOfType<PlayerSounds>();
        dialogueCameraViewsScript = FindObjectOfType<DialogueCameraViews>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        playerFidgetScript = FindObjectOfType<FidgetAnimControllerPlayer>();
        cameraScript = FindObjectOfType<CameraController>();
        saveManagerScript = FindObjectOfType<SaveManagerScript>();

        dialogueViewsHolder = dialogueCameraViewsScript.gameObject;
        playerAnim = playerSoundsScript.GetComponent<Animator>();
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        canSetBoolsTrue = true;

        currentDirection = up;
        nextPos = Vector3.forward; // The next block postion is equal to the object's forward axis (it will move along the direction it is facing)
        destination = transform.position; // The point where the object is currently at
    }

    void Update()
    {
        //sceneName = SceneManager.GetActiveScene().name;
        if (playerDirection != currentDirection)
            playerDirection = currentDirection;

        Move();
        checkIfOnCheckpoint();
        checkIfOnBridgeController();
        checkIfCompletedLevel();
        AlertBubbleCheck();

        playerAnim.SetBool("isWalking", isWalking);
        playerAnim.SetBool("isPushing", isPushing);
        playerAnim.SetBool("isInteracting", isInteracting);

        /*** For Debugging purposes ***/
        /*if (Input.GetKeyDown(KeyCode.LeftBracket))
            torchMeterMoves.CurrentVal--;
        if (Input.GetKeyDown(KeyCode.RightBracket))
            torchMeterMoves.CurrentVal++;
        /*** End Debugging ***/

        if (torchMeterMoves.CurrentVal > 0 && alreadyPlayedSFX != false)
            alreadyPlayedSFX = false;
    }

    /***
     * The main movement function for the object.
     * When a specific button is pressed, the object is set to move along the stated axis.
     * The object can move and perform the walking animation while the button stays pressed.
     ***/
    void Move()
    {
        // Moves the object (player) to the destination
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        // Checks if we're on a checkpoint AFTER moving
        if (Vector3.Distance(destination, transform.position) <= 0.00001f)
        {
            isWalking = false;
            //checkIfOnCheckpoint();
            //checkIfOnBridgeController();
            //checkIfCompletedLevel();

            // If player's torch meter runs out...
            if (torchMeterMoves.CurrentVal <= 0 && !alreadyPlayedSFX)
            {
                if (SceneManager.GetActiveScene().name == "TutorialMap")
                    resetPuzzleWithDelayInTutorial();
                else
                    resetPuzzleWithDelay();

                Instantiate(torchFireExtinguishSFX, transform.position, transform.rotation);
                Instantiate(freezingSFX, transform.position, transform.rotation);
                alreadyPlayedSFX = true; // The audio clip cannot be played again
            }
        }
        else
        {
            isWalking = true;
            return;
        }

        if (!updateKeyboardInput()) return; // Returns if we have no movement (keyboard input)

        transform.localEulerAngles = currentDirection;

        // Checks for colliders
        if (Valid())
        {
            // If no colliders, but we hit an edge
            if (EdgeCheck()) 
            {
                isWalking = true;
                playerSoundsScript.TileCheck();
                destination = transform.position + nextPos;
                direction = nextPos;

                if (!onBridge())
                    torchMeterMoves.CurrentVal -= 1;
                else
                    ResetTorchMeter();
            }
        }

        CheckToPlayAnims();
    }

    /***
     * Updates the keyboard input
     * Returns true if there's a movement input, false otherwise
     ***/
    private bool updateKeyboardInput()
    {
        /*** Movement inputs start here ***/
        if (canMove)
        {
            // W key (North)
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                nextPos = Vector3.forward;
                currentDirection = up;
                return true;
            }

            // S key (South)
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                nextPos = Vector3.back;
                currentDirection = down;
                return true;
            }

            // D key (East)
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                nextPos = Vector3.right;
                currentDirection = right;
                return true;
            }

            // A key (West)
            else if (Input.GetKeyDown(KeyCode.A) ||Input.GetKeyDown(KeyCode.LeftArrow))
            {
                nextPos = Vector3.left;
                currentDirection = left;
                return true;
            }
        }
        /*** Movement inputs end here ***/

        // Hit R to reset puzzle
        if (Input.GetKeyDown(KeyCode.R)) resetPuzzle();

        // Hit Return to interact/break block
        if (canInteract)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Collider collider = getCollider();
                if (collider == null) return false;
                if (collider.tag == "DestroyableBlock") destroyBlock(collider);
                if (collider.tag == "Generator") turnOnGenerator(collider);
                if (collider.tag == "FireStone") getFirestone(collider);
                if (collider.tag == "NPC") interactWithNPC(collider);
                else if (collider.tag == "Artifact") interactWithArtifact(collider);
            }
        }
        return false;
    }

    /*** 
     * Function for destroying a breakable block
     * When given a collider object, destroys the GameObject of that collider
     ***/
    public void destroyBlock(Collider collider)
    {
        Debug.Log("Destroyed Block");
        torchMeterMoves.CurrentVal--;
        Instantiate(destroyedBlockParticle, collider.gameObject.transform.position, collider.gameObject.transform.rotation);
        collider.gameObject.SetActive(false);

        isInteracting = true;
        CheckToPlayAnims();
    }

    // Turns on the generator if the player is colliding/interacting with it
    public void turnOnGenerator(Collider collider)
    {    
        if (collider.gameObject.GetComponent<GeneratorScript>().canInteract == true)
        {
            Debug.Log("Turned On Generator");
            collider.gameObject.GetComponentInChildren<GeneratorScript>().TurnOnGenerator();
            torchMeterMoves.CurrentVal--;

            isInteracting = true;
            CheckToPlayAnims();
        }
    }

    // Disables firestone light if the player is colliding/interacting with it
    public void getFirestone(Collider collider)
    {      
        if (collider.gameObject.GetComponentInChildren<Light>().enabled == true)
        {
            Debug.Log("Firestone has been used");
            torchMeterMoves.CurrentVal = torchMeterMoves.MaxVal;
            Instantiate(torchFireIgniteSFX, transform.position, transform.rotation);

            isInteracting = true;
            CheckToPlayAnims();
        }

        collider.gameObject.GetComponentInChildren<Light>().enabled = false;
    }

    /***
     * Function for interacting with an NPC (with dialogue)
     * Given a collider (whose GameObject is an NPC)
     * Calls the dialogue manager to play the NPC's dialogue
     ***/
    public void interactWithNPC(Collider collider)
    {
        //GameObject npc = collider.gameObject;
        //npc.GetComponent<Interactable>().Interact();

        Debug.Log("Player has interacted with NPC");
        NonPlayerCharacter nonPlayerCharacterScript = collider.GetComponent<NonPlayerCharacter>();
        FidgetAnimControllerNPC nPCFidgetScript = collider.GetComponentInChildren<FidgetAnimControllerNPC>();

        dialogueViewsHolder.transform.position = collider.gameObject.transform.position;
        characterDialogueScript.fidgetAnimControllerNPC = nPCFidgetScript;
        characterDialogueScript.nPCScript = nonPlayerCharacterScript;
        characterDialogueScript.nPCDialogueCheck = nonPlayerCharacterScript.nPCDialogueCheck;
        characterDialogueScript.talkingTo = nonPlayerCharacterScript.characterName;
        characterDialogueScript.isInteractingWithNPC = true;
        characterDialogueScript.setDialogueQuestions(nonPlayerCharacterScript.dialogueOptionsFile);
        characterDialogueScript.StartDialogue();

        isInteracting = false;
        CheckToPlayAnims();
    }

    public void interactWithArtifact(Collider collider)
    {    
        ArtifactScript artifactScript = collider.GetComponent<ArtifactScript>();

        if (!artifactScript.hasCollectedArtifact)
        {
            Debug.Log("Player has interacted with Artifact");
            dialogueViewsHolder.transform.position = collider.gameObject.transform.position;
            collider.GetComponent<ArtifactScript>().SetArtifactDialogue();
            collider.GetComponent<ArtifactScript>().OpenChest();
            characterDialogueScript.artifactScript = artifactScript;
            characterDialogueScript.isInteractingWithArtifact = true;
            characterDialogueScript.setDialogueQuestions(characterDialogueScript.artifactDialogueOptions);
            characterDialogueScript.StartDialogue();

            isInteracting = true;
            CheckToPlayAnims();
        }
    }

    // Draws a ray forward and returns the collider if it hits, or null otherwise 
    public Collider getCollider()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), transform.forward);
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        if (Physics.Raycast(myRay, out hit, rayLength)) return hit.collider;
        return null;
    }

    // Checks to see if the next position is valid or not
    bool Valid()
    {       
        Collider collider = getCollider();
        if (collider == null)
        {
            isWalking = false;
            isPushing = false;
            return true; // If there's no collider (no obstacles)
        }

        isWalking = false;
        isPushing = true;
        switch (collider.tag)
        {
            case ("StaticBlock"):
                collider.gameObject.GetComponentInChildren<ObjectShakeController>().StartShake(0.25f, 0.25f);
                CheckToPlayAnims();
                break;

            case ("Crystal"):
                collider.gameObject.GetComponentInChildren<ObjectShakeController>().StartShake(0.25f, 0.25f);
                collider.gameObject.GetComponentInChildren<CrystalsManager>().SetGlowActive();
                collider.gameObject.GetComponentInChildren<CrystalsManager>().ResetCrystalIdleAnim();
                CheckToPlayAnims();
                break;

            case ("Obstacle"):
                if (collider.gameObject.GetComponent<BlockMovementController>().MoveBlock(currentDirection))
                    torchMeterMoves.CurrentVal--;
                CheckToPlayAnims();
                break;

            case ("DestroyableBlock"):
                collider.gameObject.GetComponentInChildren<ObjectShakeController>().StartShake(0.2f, 0.25f);
                CheckToPlayAnims();
                break;

            default:
                Debug.Log("Unrecognizable Tag");
                isPushing = false;
                CheckToPlayAnims();
                break;

                /*case ("FireStone"):
                    if (collider.gameObject.GetComponentInChildren<Light>().enabled == true)
                    {
                        collider.gameObject.GetComponentInChildren<ObjectShakeController>().StartShake(0.2f, 0.25f);
                        isPushing = true;                  
                    }
                    else
                        isPushing = false; 

                    CheckToPlayAnims();
                    break;

                case ("Generator"):
                    Debug.Log("Turned On Generator");
                    if(collider.gameObject.GetComponent<GeneratorScript>().canInteract == true) torchMeterMoves.CurrentVal--;
                    collider.gameObject.GetComponentInChildren<GeneratorScript>().TurnOnGenerator();
                    isInteracting = true;
                    CheckToPlayAnims();
                    break;*/
        }
        return false;
    }

    // Checks if there's an edge - determines where the player cant move towards
    bool EdgeCheck()
    {
        Ray myEdgeRay = new Ray(edgeCheck.transform.position, Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(myEdgeRay.origin, myEdgeRay.direction, Color.red);

        if (Physics.Raycast(myEdgeRay, out hit, rayLengthEdgeCheck))
        {
            string tag = hit.collider.tag;
            string name = hit.collider.name;

            if (name == "BridgeBlock" || tag == "EndOfLevel")
            {
                PopOutTorchMeterCheck();
                torchMeterMoves.CurrentVal = torchMeterMoves.MaxVal;
                canRestartPuzzle = false;
                return true;
            } 
            else if (tag == "EmptyBlock")
            {
                // The player cannot walk over ditches - the areas where blocks can fall in
                //isWalking = false;
                return false;
            }
            else
            {
                PopInTorchMeterCheck();
                canRestartPuzzle = true;
            }
            return true;
        }
        //isPushing = false;
        //isWalking = false;
        return false;
    }

    /***
     * Sets the destination of the player to the new destination.
     * @param newDestination - The new destination to set to
     ***/
    public void setDestination(Vector3 newDestination)
    {
        destination = newDestination;
    }

    // Resets the current value of the torch meter to the maximum value
    public void ResetTorchMeter()
    {
        torchMeterMoves.CurrentVal = torchMeterMoves.MaxVal;
        torchMeterScript.ResetTorchMeterElements();
    }

    bool AlertBubbleCheck()
    {   
        Ray myAlertBubbleRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), nextPos);
        RaycastHit hit;
        Debug.DrawRay(myAlertBubbleRay.origin, myAlertBubbleRay.direction, Color.blue);

        if (Physics.Raycast(myAlertBubbleRay, out hit, rayLength) && !iceMaterialScript.isFrozen)
        {
            string tag = hit.collider.tag;

            if (tag == "NPC")
            {
                characterDialogueScript.SetAlertBubbleActive();
                return true;
            }        
            else if (tag == "Artifact")
            {
                ArtifactScript artifactScript = hit.collider.GetComponent<ArtifactScript>();

                if (!artifactScript.hasCollectedArtifact)
                {
                    characterDialogueScript.SetAlertBubbleActive();
                    return true;
                }
            }
        }
        characterDialogueScript.SetAlertBubbleInactive();
        return false;
    }

    // Draws a ray below the player and returns true if player is standing on a checkpoint, returns false otherwise
    public bool checkIfOnCheckpoint()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        Physics.Raycast(myRay, out hit, rayLength);
        string tag = hit.collider.tag;

        if (tag != "Checkpoint") return false; // If we did not hit a checkpoint

        checkpoint = hit.collider.gameObject;
        CheckpointManager checkpointManagerScript = checkpoint.GetComponent<CheckpointManager>();
        puzzle = hit.collider.transform.parent.parent.gameObject;

        // Sets the new torch meter value based on the checkpoint's value
        int newNumMovements = checkpointManagerScript.getNumMovements();
        torchMeterMoves.setMaxValue(newNumMovements);    

        // If this is the first time we visited this checkpoint
        if (!checkpointManagerScript.hitCheckpoint())
        {
            if (canSetBoolsTrue)
                SetPlayerBoolsTrue(); //Enable Player Movement

            checkpointManagerScript.setCheckpoint();
            checkpointManagerScript.LastBridgeTileCheck();
            ResetTorchMeter();
            PopInTorchMeterCheck();
            saveManagerScript.SavePlayerPosition(checkpoint);
            saveManagerScript.SavePlayerRotation();
            saveManagerScript.SaveCameraPosition();
            hasMovedPuzzleView = false;
        }
        return true;
    }

    // Determines wether the player has touched this object 
    public bool checkIfCompletedLevel()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.blue);

        if (Physics.Raycast(myRay, out hit, rayLength))
        {
            string tag = hit.collider.tag;

            if (tag == "EndOfLevel")
            {
                ResetTorchMeter();
                levelManagerScript.gameObject.GetComponent<BridgeMovementController>().MoveToNextBlock();
                levelManagerScript.DisablePlayer();
                levelManagerScript.SetLevelCompletedEffects();
                return true;
            }
        }
        return false;
    }

    // Determines when the player lands on a bridge tile with the BridgeMovementController
    public bool checkIfOnBridgeController()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.green);

        Physics.Raycast(myRay, out hit, rayLength);
        string tag = hit.collider.tag;
        string name = hit.collider.name;

        // Disables the player's footstep sfx - ONLY disabled when the player transitions to another level/scene (on the end bridge)
        if (tag == "DisableFootsteps" && !hasDisabledFootsteps)
        {
            playerSoundsScript.canPlayFootsteps = false;
            hasDisabledFootsteps = true;
        }

        // Finds the object with the bridge controller script
        if (tag == "BridgeController" && gameObject.transform.position == hit.collider.transform.position)
        {
            Debug.Log("Bridge Controller Found");
            SetPlayerBoolsFalse();
            hit.collider.gameObject.GetComponent<BridgeMovementController>().MoveToNextBlock();
            transform.localEulerAngles = new Vector3(0, hit.collider.gameObject.GetComponent<BridgeMovementController>().newDirection, 0);
            NextPuzzleViewCheck();
            return true;
        }

        // Sets the savedInvisibleBlock's position the last tile on a bridge and saves its position - OLD VERSION
        /*if (tag == "LastBridgeTile" && hasMovedPuzzleView)
        {
            Debug.Log("Invisible Block Position has been saved");
            saveManagerScript.savedInvisibleBlock.transform.position = hit.collider.transform.position + new Vector3(0, 1, 0);
            saveManagerScript.SaveBlockPosition();
            hasMovedPuzzleView = false;
        }*/

        return false;
    }

    // Determines if the player on the bridge - returns true of the player is standing on a bridge tile, false otherwise
    public bool onBridge()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        Physics.Raycast(myRay, out hit, rayLength);
        string name = hit.collider.name;

        if (name == "BridgeBlock")
            return true;

        return false;
    }

    // Determines if the player on the last tile block in a puzzle - returns true if so, false otherwise - ONLY USED DURING TUTORIAL
    public bool onLastTileBlock()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        Physics.Raycast(myRay, out hit, rayLength);
        string name = hit.collider.name;

        // Checks to see if the player has landed on the last green block in the first puzzle of tutorial
        if (name == "LastTileBlockInPuzzle")
        {
            canSetBoolsTrue = true;
            return true;
        }
        return false;
    }

    // Resets the current puzzle the player is on (determined by last checkpoint)
    private void resetPuzzle()
    {
        // You cant restart a puzzle while on a bridge tile (determined in EdgeCheck)
        if (canRestartPuzzle)
        {
            checkpoint.GetComponent<CheckpointManager>().resetPlayerPosition();

            Debug.Log("Pushable blocks child count: " + puzzle.transform.childCount);
            for (int i = 0; i < puzzle.transform.childCount; i++)
            {
                GameObject child = puzzle.transform.GetChild(i).gameObject;
                if (child.name == "Pushable Blocks")
                    child.GetComponent<ResetPushableBlocks>().resetBlocks();

                else if (child.name == "Crystal Blocks")
                    child.GetComponent<ResetCrystalBlocks>().ResetCrystals();

                else if (child.name == "Breakable Blocks")
                    child.GetComponent<ResetBreakableBlocks>().resetBlocks();

                else if (child.name == "Fire Stones")
                    child.GetComponent<ResetFireStone>().resetFirestone();

                else if (child.name == "Generator Blocks")
                    child.GetComponent<ResetGeneratorBlocks>().resetGenerator();
            }
        }

    }

    // Resets the current puzzle the player is on (determined by last checkpoint) BUT with a delay - USED ONLY IN TUTORIAL
    private void resetPuzzleWithDelayInTutorial()
    {
        checkpoint.GetComponent<CheckpointManager>().StartCoroutine("resetPlayerPositionInTutorialWithDelay", 1.5f);

        // Debug.Log("Pushable blocks child count: " + puzzle.transform.childCount);
        for (int i = 0; i < puzzle.transform.childCount; i++)
        {
            GameObject child = puzzle.transform.GetChild(i).gameObject;
            if (child.name == "Pushable Blocks")
                child.GetComponent<ResetPushableBlocks>().StartCoroutine("resetBlocksWithDelay", 1.5f);

            else if (child.name == "Crystal Blocks")
                child.GetComponent<ResetCrystalBlocks>().StartCoroutine("ResetCrystalsWithDelay", 1.5f);

            else if (child.name == "Breakable Blocks")
                child.GetComponent<ResetBreakableBlocks>().StartCoroutine("resetBlocksWithDelay", 1.5f);

            else if (child.name == "Fire Stones")
                child.GetComponent<ResetFireStone>().StartCoroutine("resetFirestoneWithDelay", 1.5f);

            else if (child.name == "Generator Blocks")
                child.GetComponent<ResetGeneratorBlocks>().StartCoroutine("resetGeneratorWithDelay", 1.5f);
        }

        hasDied = true;
    }

    // Resets the current puzzle the player is on (determined by last checkpoint) BUT with a delay
    private void resetPuzzleWithDelay()
    {
        checkpoint.GetComponent<CheckpointManager>().StartCoroutine("resetPlayerPositionWithDelay", 1.5f);

        // Debug.Log("Pushable blocks child count: " + puzzle.transform.childCount);
        for (int i = 0; i < puzzle.transform.childCount; i++)
        {
            if (!gameHUDScript.canDeathScreen)
            {
                GameObject child = puzzle.transform.GetChild(i).gameObject;
                if (child.name == "Pushable Blocks")
                    child.GetComponent<ResetPushableBlocks>().StartCoroutine("resetBlocksWithDelay", 1.5f);

                else if (child.name == "Crystal Blocks")
                    child.GetComponent<ResetCrystalBlocks>().StartCoroutine("ResetCrystalsWithDelay", 1.5f);

                else if (child.name == "Breakable Blocks")
                    child.GetComponent<ResetBreakableBlocks>().StartCoroutine("resetBlocksWithDelay", 1.5f);

                else if (child.name == "Fire Stones")
                    child.GetComponent<ResetFireStone>().StartCoroutine("resetFirestoneWithDelay", 1.5f);

                else if (child.name == "Generator Blocks")
                    child.GetComponent<ResetGeneratorBlocks>().StartCoroutine("resetGeneratorWithDelay", 1.5f);
            }
        }

        hasDied = true;
    }

    // Sets the bools to false - prevents the player from moving, restarting puzzles, and interacting with anything
    public void SetPlayerBoolsFalse()
    {
        canRestartPuzzle = false;
        canMove = false;
        canInteract = false;
    }

    // Sets the bools to true - allows the player to move, restart puzzles, and interact with anything
    public void SetPlayerBoolsTrue()
    {
        canRestartPuzzle = true;
        canMove = true;
        canInteract = true;
    }

    // Makes the torch meter pop out
    public void PopOutTorchMeterCheck()
    {
        if (!hasAlreadyPopedOut)
        {
            torchMeterScript.TorchMeterPopOut();
            hasAlreadyPopedOut = true;
        }
    }

    // Makes the torch meter pop in
    public void PopInTorchMeterCheck()
    {
        if (hasAlreadyPopedOut)
        {
            torchMeterScript.TorchMeterPopIn();
            hasAlreadyPopedOut = false;
        }
    }

    // Sets the destination to the first block in each world - ONLY for when the player enters a new scene/zone
    public void WalkIntoScene()
    {
        destination = new Vector3(0, 0, 0);
    }

    // Checks if the camera can move to the next puzzle view
    private void NextPuzzleViewCheck()
    {
        if (!hasMovedPuzzleView)
        {
            Debug.Log("Switched To Next Puzzle View");
            cameraScript.NextPuzzleView02();
            cameraScript.PlayWindGushSFX();
            hasMovedPuzzleView = true;
        }
    }

    // Plays a new animation state
    private void ChangeAnimationState(string newState)
    {
        playerAnim.Play(newState);
        //currentState = newState;
    }

    // Plays the interaction animation for Ko 
    public void PlayInteractionAnim()
    {
        isInteracting = true;
        CheckToPlayAnims();
    }

    // Determines which animation state to play
    private void CheckToPlayAnims()
    {
        if (isPushing) 
            ChangeAnimationState("Pushing");

        else if (isInteracting)
        {
            ChangeAnimationState("Interacting");
            SwooshSFX();
        }

        isPushing = false;
        isInteracting = false;
        playerFidgetScript.SetIdleIndexToZero();
    }

    // Plays the random audio clip it aquired
    private void SwooshSFX()
    {
        AudioClip swooshClips = GetRandomSwooshSFX();
        audioSource.volume = 0.36f;
        audioSource.PlayOneShot(swooshClips);
    }

    // Gets a random audio clip from its respective array
    private AudioClip GetRandomSwooshSFX()
    {
        return swooshClips[UnityEngine.Random.Range(0, swooshClips.Length)];
    }

}
