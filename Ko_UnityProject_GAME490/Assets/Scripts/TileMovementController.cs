using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TileMovementController : MonoBehaviour
{
    public Camera main_camera;
    public Animator Anim;
    public GameObject edgeCheck;   
    private AudioSource audioSource;
    private GameObject savedInvisibleBlock;
    private string currentState;

    Vector3 up = Vector3.zero,                                  // Object look North
    right = new Vector3(0, 90, 0),                              // Object look East
    down = new Vector3(0, 180, 0),                              // Object look South
    left = new Vector3(0, 270, 0),                              // Object look West
    currentDirection = Vector3.zero;                            // Default state
    public Vector3 playerDirection;                             // Only used in Character Dialogue script

    Vector3 nextPos, destination, direction;

    float speed = 5f;                                           // The speed at which the object will move from its current position to the destination
    float rayLength = 1f;                                       // The ray length for the tile movement
    float rayLengthEdgeCheck = 1f;                              // The ray length for the edge check

    [Header("Sounds")]
    public GameObject torchFireIgniteSFX;
    public GameObject torchFireExtinguishSFX;
    public GameObject freezingSFX;
    public AudioClip[] swooshClips;

    [Header("Prefabs")]
    public GameObject destroyedBlockParticle;
    public GameObject invisibleBlock; 

    [Header("Scripts")]
    public TorchMeterStat torchMeterMoves;
    private LevelManager levelManagerScript;
    private TorchMeterScript torchMeterScript;
    private SaveManagerScript saveManagerScript;
    private GameHUD gameHUDScript;
    private CharacterDialogue characterDialogueScript;
    private PlayerSounds playerSoundsScript;
    private CameraController cameraScript;

    [Header("Save Slot Elements")]
    public GameObject checkpoint;
    public GameObject puzzle;
    //public string sceneName;

    [Header("Bools")]
    public bool isWalking;                                      // Determines when to play an object's animation
    public bool hasDied = false;
    public bool canMove = true;
    public bool canInteract = true;
    public bool canRestartPuzzle;                               // Determines when the player can press "R" (restart puzzle)
    public bool canSetBoolsTrue;
    private bool isPushing;                                     // Determines when the object can move
    private bool isInteracting;
    private bool alreadyPlayedSFX;
    private bool hasAlreadyPopedOut;                            // Determines when the torch meter can scale in/out
    private bool hasMovedPuzzleView = false;                            // Determines when the camera can switch puzzle views   
    private bool hasStartedTutorial = false;
    private bool hasDisabledFootsteps = false;


    void Awake()
    {
        torchMeterMoves.Initialize();
        savedInvisibleBlock = GameObject.Find("SavedInvisibleBlock");

        levelManagerScript = FindObjectOfType<LevelManager>();
        torchMeterScript = FindObjectOfType<TorchMeterScript>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        playerSoundsScript = FindObjectOfType<PlayerSounds>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        cameraScript = FindObjectOfType<CameraController>();

        saveManagerScript = FindObjectOfType<SaveManagerScript>();
        saveManagerScript.LoadPlayerPosition(); //
        saveManagerScript.LoadBlockPosition(); //
    }

    void Start()
    {
        /*SaveSlot save = SaveManager.LoadGame();
        if (save != null)
        {
            transform.position = new Vector3(save.getPosition()[0], save.getPosition()[1], save.getPosition()[2]);
            puzzle = GameObject.Find(save.getPuzzleName());
            main_camera.GetComponent<CameraController>().currentIndex = save.getCameraIndex();
        }
        else
        {
            //transform.position = new Vector3(0, 0, 0);
            main_camera.GetComponent<CameraController>().currentIndex = 0;
            puzzle = GameObject.Find("Puzzle01");
        }*/

        canSetBoolsTrue = true;
        audioSource = GetComponent<AudioSource>();

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
        AlertBubbleCheck();
        checkIfOnBridgeController();
        checkIfCompletedLevel();

        Anim.SetBool("isWalking", isWalking);
        Anim.SetBool("isPushing", isPushing);
        Anim.SetBool("isInteracting", isInteracting);

        /*** For Debugging purposes ***/
        if (Input.GetKeyDown(KeyCode.LeftBracket))
            torchMeterMoves.CurrentVal--;
        if (Input.GetKeyDown(KeyCode.RightBracket))
            torchMeterMoves.CurrentVal++;
        /*** End Debugging ***/
        if (torchMeterMoves.CurrentVal > 0) alreadyPlayedSFX = false;
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
            checkIfOnCheckpoint();
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
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
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
        isWalking = false;
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
            isWalking = false;
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
        }

        collider.gameObject.GetComponentInChildren<Light>().enabled = false;
        isWalking = false;
        CheckToPlayAnims();
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
        cameraScript.dialogueViews.transform.position = collider.gameObject.transform.position;
        characterDialogueScript.isInteractingWithNPC = true;      
        characterDialogueScript.StartDialogue();

        isInteracting = false;
        isWalking = false;
        CheckToPlayAnims();
    }

    public void interactWithArtifact(Collider collider)
    {
        string name = collider.name;

        Debug.Log("Player has interacted with Artifact");
        if (name == "ArtifactOne")
            characterDialogueScript.isArtifactOne = true;
        if (name == "ArtifactTwo")
            characterDialogueScript.isArtifactTwo = true;
        if (name == "ArtifactThree")
            characterDialogueScript.isArtifactThree = true;

        cameraScript.dialogueViews.transform.position = collider.gameObject.transform.position;
        characterDialogueScript.StartDialogue();

        isInteracting = false;
        isWalking = false;
        CheckToPlayAnims();
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
        isPushing = false;
        Collider collider = getCollider();
        if (collider == null) return true; // If there's no collider (no obstacles)

        isPushing = true;
        switch (collider.tag)
        {
            case ("StaticBlock"):
                collider.gameObject.GetComponentInChildren<ObjectShakeController>().StartShake(0.25f, 0.25f);
                isWalking = false;
                CheckToPlayAnims();
                break;

            case ("Crystal"):
                collider.gameObject.GetComponentInChildren<ObjectShakeController>().StartShake(0.25f, 0.25f);
                collider.gameObject.GetComponentInChildren<CrystalsManager>().SetGlowActive();
                collider.gameObject.GetComponentInChildren<CrystalsManager>().ResetCrystalIdleAnim();
                isWalking = false;
                isPushing = true;
                CheckToPlayAnims();
                break;

            case ("Obstacle"):
                if (collider.gameObject.GetComponent<BlockMovementController>().MoveBlock(currentDirection))
                    torchMeterMoves.CurrentVal--;
                isWalking = false;
                CheckToPlayAnims();
                break;

            case ("DestroyableBlock"):
                collider.gameObject.GetComponentInChildren<ObjectShakeController>().StartShake(0.2f, 0.25f);
                isWalking = false;
                CheckToPlayAnims();
                break;

            case ("InvisibleBlock"):
                isWalking = false;
                isPushing = false;
                break;

            /*case ("FireStone"):
                if (collider.gameObject.GetComponentInChildren<Light>().enabled == true)
                {
                    collider.gameObject.GetComponentInChildren<ObjectShakeController>().StartShake(0.2f, 0.25f);
                    isPushing = true;                  
                }
                else
                    isPushing = false; 
                
                isWalking = false;
                CheckToPlayAnims();
                break;*/

            /*case ("Generator"):
                Debug.Log("Turned On Generator");
                if(collider.gameObject.GetComponent<GeneratorScript>().canInteract == true) torchMeterMoves.CurrentVal--;
                collider.gameObject.GetComponentInChildren<GeneratorScript>().TurnOnGenerator();
                isWalking = false;
                isInteracting = true;
                CheckToPlayAnims();
                break;*/

            default:
                Debug.Log("Unrecognizable Tag");
                isWalking = false;
                isPushing = false;
                CheckToPlayAnims();
                break;
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
                isWalking = false;
                return false;
            }
            else
            {
                PopInTorchMeterCheck();
                canRestartPuzzle = true;
            }
            return true;
        }
        isWalking = false;
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

        if (Physics.Raycast(myAlertBubbleRay, out hit, rayLength))
        {
            string tag = hit.collider.tag;

            if (tag == "NPC" || tag == "Artifact")
            {
                characterDialogueScript.SetAlertBubbleActive();
                return true;
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

        //SaveManager.SaveGame(makeSaveSlot());
        checkpoint = hit.collider.gameObject;
        puzzle = hit.collider.transform.parent.parent.gameObject;

        // Sets the new puzzle view
        //GameObject view = puzzle.transform.Find("View").gameObject;
        //main_camera.GetComponent<CameraController>().NextPuzzleView(view.transform);

        // Sets the new torch meter value based on the checkpoint's value
        int newNumMovements = checkpoint.GetComponent<CheckpointManager>().getNumMovements();
        torchMeterMoves.setMaxValue(newNumMovements);    

        // If this is the first time we visited this checkpoint
        if (!checkpoint.GetComponent<CheckpointManager>().hitCheckpoint())
        {
            if (canSetBoolsTrue)
                SetPlayerBoolsTrue(); //Enable Player Movement

            checkpoint.GetComponent<CheckpointManager>().setCheckpoint();
            ResetTorchMeter();
            PopInTorchMeterCheck();
            saveManagerScript.SavePlayerPosition();
            saveManagerScript.SaveCameraPosition();
            //main_camera.GetComponent<CameraController>().WindGush();
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
                //SaveManager.DeleteGame();
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
            //Debug.Log("Player Footsteps Have Been Disabled");
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
        // Sets the savedInvisibleBlock's position the last tile on a bridge and saves its position
        if (tag == "LastBridgeTile" && hasMovedPuzzleView)
        {
            Debug.Log("Invisible Block Position has been saved");
            savedInvisibleBlock.transform.position = hit.collider.transform.position + new Vector3(0, 1, 0);
            saveManagerScript.SaveBlockPosition();
            hasMovedPuzzleView = false;
        }

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
        {          
            //string tag = hit.collider.tag;
            // For the if statements below, the torch meter icon has to be invisible for the bridge blocks to instatiate the invisible blocks!
            /*if (tag == "MoveCameraBlock" && !hasMovedPuzzleView && hasAlreadyPopedOut)
            {
                Debug.Log("Switched To Next Puzzle View");
                main_camera.GetComponent<CameraController>().NextPuzzleView02();
                main_camera.GetComponent<CameraController>().WindGush();
                Instantiate(invisibleBlock, hit.collider.transform.position + new Vector3(0, 1, 0), hit.collider.transform.rotation);
                hasMovedPuzzleView = true;
            }
            if (tag == "InstantiateBlock" && isWalking)
            {
                Instantiate(invisibleBlock, hit.collider.transform.position + new Vector3(0, 1, 0), hit.collider.transform.rotation);
            }
            if (tag == "ResetCameraBool" && hasAlreadyPopedOut && isWalking)
            {
                Instantiate(invisibleBlock, hit.collider.transform.position + new Vector3(0, 1, 0), hit.collider.transform.rotation);
            }
            if (tag == "ResetCameraBool" && hasMovedPuzzleView && hasAlreadyPopedOut)
            {
                hasMovedPuzzleView = false;
            }
            /*if (tag == "LastBridgeTile" && isWalking)
            {
                Debug.Log("Invisible Block Position has been saved");
                savedInvisibleBlock.transform.position = hit.collider.transform.position + new Vector3(0, 1, 0);
                saveManagerScript.SaveBlockPosition();
            }*/
            return true;
        }
        return false;
    }

    // Determines if the player on the last tile block in a puzzle - returns true if so, false otherwise - ONLY USED DURING TUTORIAL
    public bool onFirstOrLastTileBlock()
    {
        if(SceneManager.GetActiveScene().name == "TutorialMap")
        {
            Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
            RaycastHit hit;
            Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

            Physics.Raycast(myRay, out hit, rayLength);
            string name = hit.collider.name;

            // Checks to see if the player has landed on the last green block in the first puzzle of tutorial
            if (name == "PatchyGrassBlockTutorial")
            {
                canSetBoolsTrue = true;
                return true;
            }
            // Checks to see if the player has landed on the first green block in the tutorial
            if (name == "Checkpoint_GrassTiles" && !hasStartedTutorial)
            {
                hasStartedTutorial = true;
                return true;             
            }
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

    // Sets the destination to the first block in each world
    public void WalkIntoScene()
    {
        destination = new Vector3(0, 0, 0);
    }

    //Checks to see if the function for moving the camera has been called
    private void NextPuzzleViewCheck()
    {
        if (!hasMovedPuzzleView)
        {
            Debug.Log("Switched To Next Puzzle View");
            main_camera.GetComponent<CameraController>().NextPuzzleView02();
            main_camera.GetComponent<CameraController>().WindGush();
            hasMovedPuzzleView = true;
        }
    }

    // Plays a new animation state
    private void ChangeAnimationState(string newState)
    {
        Anim.Play(newState);
        currentState = newState;
    }

    // Determines which animation state to play
    private void CheckToPlayAnims()
    {
        if (isPushing) ChangeAnimationState("Pushing");
        else if (isInteracting)
        {
            ChangeAnimationState("Interacting");
            SwooshSFX();
        }
        isPushing = false;
        isInteracting = false;
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

    /*private SaveSlot makeSaveSlot()
    {
        Vector3 position = transform.position;
        float x = position.x;
        float y = position.y;
        float z = position.z;
        float[] playerPosition = { x, y, z };

        string puzzleName = puzzle.name;

        int currCameraIndex = main_camera.GetComponent<CameraController>().currentIndex;

        return new SaveSlot(sceneName, playerPosition, puzzleName, currCameraIndex);
    }*/


}
