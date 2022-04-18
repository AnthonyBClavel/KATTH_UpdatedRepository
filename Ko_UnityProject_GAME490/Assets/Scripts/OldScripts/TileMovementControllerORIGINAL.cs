using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TileMovementControllerORIGINAL : MonoBehaviour
{
    [Header("Bools")]
    public bool isWalking;                                      
    public bool hasDied = false;
    public bool canMove = true;
    public bool canInteract = true;
    public bool canRestartPuzzle;
    public bool canSetBoolsTrue;
    public bool hasFinishedZone = false;
    private bool isPushing;                                     
    private bool isInteracting;
    private bool alreadyPlayedSFX;
    private bool hasAlreadyPopedOut;
    private bool hasMovedPuzzleView = false; 
    private bool canCheckForNextTile = true;
    private bool canTorchMeter = true;

    [Header("Vectors")]
    public Vector3 playerDirection;
    Vector3 nextPos, destination, direction;
    Vector3 up = Vector3.zero,                      // Look North
    right = new Vector3(0, 90, 0),                  // Look East
    down = new Vector3(0, 180, 0),                  // Look South
    left = new Vector3(0, 270, 0),                  // Look West
    currentDirection = Vector3.zero;                // Default state (North)

    private float speed = 5f;                       // The speed at which the player will move
    private float rayLength = 1f;                   // the length of the ray used by raycasts

    private GameObject edgeCheck;
    private GameObject nextBridgeTileCheck;
    //private GameObject dialogueViewsHolder;
    private GameObject previousBridgeTile;
    private GameObject objectToShake;
    private Animator playerAnim;

    [Header("Particles")]
    public GameObject destroyedBlockParticle;
    public ParticleSystem torchFireParticle;

    [Header("Current Puzzle/Checkpoint/Bridge")]
    public GameObject checkpoint;
    public GameObject puzzle;
    public GameObject bridge;
    private int bridgeTileCount;

    [Header("Scripts")]
    public TorchMeterStat torchMeterMoves;
    private TorchMeter torchMeterScript;
    private SaveManager saveManagerScript;
    private GameHUD gameHUDScript;
    private CharacterDialogue characterDialogueScript;
    private FootstepsController footstepsControllerScript;
    //private DialogueCameraViews dialogueCameraViewsScript;
    private CameraController cameraScript;
    //private FidgetAnimControllerPlayer playerFidgetScript;
    private AudioManager audioManagerScript;
    private GameManager gameManagerScript;
    private IntroManager introManagerScript;
    private ObjectShakeController objectShakeScript;

    void Awake()
    {
        //torchMeterMoves.Initialize(); // Initialized in the torch meter script

        SetScripts();
        SetElemenets();
    }

    // Start is called before the first frame update
    void Start()
    {
        canSetBoolsTrue = true;

        currentDirection = up;
        nextPos = Vector3.forward;
        destination = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Updates the player's current direction
        if (playerDirection != currentDirection)
            playerDirection = currentDirection;

        // Resets the SFX for the torch meter back to false
        //if (torchMeterMoves.CurrentVal > 0 && alreadyPlayedSFX != false)
            //alreadyPlayedSFX = false;

        Move();
        checkIfOnCheckpoint();
        checkForNextBridgeTile();
        AlertBubbleCheck();
        TorchMeterDebugingCheck();

        playerAnim.SetBool("isWalking", isWalking);
        playerAnim.SetBool("isPushing", isPushing);
        playerAnim.SetBool("isInteracting", isInteracting);
    }

    // The main movement function for the player - tile movement
    void Move()
    {
        // Moves the player towards the destination
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        // Before the player reaches it's destination...
        if (Vector3.Distance(destination, transform.position) <= 0.00001f)
        {
            isWalking = false;
            //checkIfOnCheckpoint();
            //checkForNextBridgeTile();

            // Resets the puzzle if the torch meter runs out
            if (/*torchMeterMoves.CurrentVal <= 0 &&*/ !alreadyPlayedSFX && canTorchMeter)
            {
                if (SceneManager.GetActiveScene().name == "TutorialMap")
                    resetPuzzleWithDelayInTutorial();
                else
                    resetPuzzleWithDelay();

                audioManagerScript.PlayTorchFireExtinguishSFX();
                audioManagerScript.PlayFreezeingSFX();
                alreadyPlayedSFX = true;
            }
        }
        else
        {
            isWalking = true;
            return;
        }

        if (!updateKeyboardInput()) return; // Returns if there's no movement input
        transform.localEulerAngles = currentDirection;

        // Checks for colliders
        if (Valid())
        {
            // If no colliders, but we hit an edge
            if (EdgeCheck()) 
            {
                isWalking = true;
                //footstepsControllerScript.PlayFootstepsSFX();
                destination = transform.position + nextPos;
                direction = nextPos;

                if (!onBridge())
                    SubractFromTorchMeter();
                //else                   
                    //ResetTorchMeter();
            }
        }

        CheckToPlayAnims();
    }

    // Updates the keyboard input - returns true if input is recieved, false otherwise
    bool updateKeyboardInput()
    {
        /*** Movement inputs START here ***/
        if (canMove)
        {
            // Move North
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                nextPos = Vector3.forward;
                currentDirection = up;
                return true;
            }

            // Move South
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                nextPos = Vector3.back;
                currentDirection = down;
                return true;
            }

            // Move East
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                nextPos = Vector3.right;
                currentDirection = right;
                return true;
            }

            // Move West
            else if (Input.GetKeyDown(KeyCode.A) ||Input.GetKeyDown(KeyCode.LeftArrow))
            {
                nextPos = Vector3.left;
                currentDirection = left;
                return true;
            }
        }
        /*** Movement inputs END here ***/

        // Hit R to reset puzzle
        if (Input.GetKeyDown(KeyCode.R)) resetPuzzle();

        // Hit Return/Space/Enter to break/interact with block
        if (canInteract)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Collider collider = getCollider();
                if (collider == null) return false;
                if (collider.tag == "DestroyableBlock") destroyBlock(collider);
                if (collider.tag == "Generator") turnOnGenerator(collider);
                if (collider.tag == "Firestone") getFirestone(collider);
                if (collider.tag == "NPC") interactWithNPC(collider);
                else if (collider.tag == "Artifact") interactWithArtifact(collider);
            }
        }
        return false;
    }

    // Checks to see if the next position is valid or not
    bool Valid()
    {
        Collider collider = getCollider();
        // Returns if there's no collider (no obstacles)
        if (collider == null)
        {
            isWalking = false;
            isPushing = false;
            return true;
        }
        // Only if the collider object has children
        if (collider.transform.childCount > 0)
            objectToShake = collider.transform.GetChild(0).gameObject;

        isWalking = false;
        isPushing = true;
        switch (collider.tag)
        {
            case ("StaticBlock"):
                objectShakeScript.ShakeObject(objectToShake);
                audioManagerScript.PlayTreeHitSFX();
                CheckToPlayAnims();
                break;

            case ("Crystal"):
                objectShakeScript.ShakeObject(objectToShake);
                audioManagerScript.PlayCrystalHitSFX();
                //collider.gameObject.GetComponentInChildren<CrystalManager>().SetGlowActive();
                //collider.gameObject.GetComponentInChildren<CrystalManager>().ResetCrystalIdleAnim();
                CheckToPlayAnims();
                break;

            case ("PushableBlock"):
                //if (collider.gameObject.GetComponent<BlockMovementController>().MoveBlock(currentDirection))
                    //SubractFromTorchMeter();
                CheckToPlayAnims();
                break;

            case ("DestroyableBlock"):
                objectShakeScript.ShakeObject(objectToShake);
                audioManagerScript.PlayRockHitSFX();
                CheckToPlayAnims();
                break;

            default:
                //Debug.Log("Unrecognizable Tag");
                isPushing = false;
                CheckToPlayAnims();
                break;
        }
        return false;
    }

    // Draws a ray forward - returns the collider of the object that it hits, null otherwise 
    public Collider getCollider()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), transform.forward);
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        if (Physics.Raycast(myRay, out hit, rayLength)) return hit.collider;
        return null;
    }

    // Sets the object inactive - "destroys" the block
    public void destroyBlock(Collider collider)
    {
        //Debug.Log("Destroyed Block");
        GameObject destroyableBlock = collider.gameObject;

        SubractFromTorchMeter();
        Instantiate(destroyedBlockParticle, destroyableBlock.transform.position + new Vector3(0, -0.25f, 0), destroyableBlock.transform.rotation);
        audioManagerScript.PlayRockBreakSFX();
        destroyableBlock.SetActive(false);

        isInteracting = true;
        CheckToPlayAnims();
    }

    // Turns on the generator if the player is colliding/interacting with it
    public void turnOnGenerator(Collider collider)
    {
        Generator generatorScript = collider.gameObject.GetComponent<Generator>();

        if (generatorScript.IsGenActive == true)
        {
            //Debug.Log("Turned On Generator");
            generatorScript.TurnOnGenerator();
            SubractFromTorchMeter();

            isInteracting = true;
            CheckToPlayAnims();
        }
    }

    // Disables the firestone's light if the player is interacting with it
    public void getFirestone(Collider collider)
    {
        Light firestoneLight = collider.gameObject.GetComponentInChildren<Light>();

        if (firestoneLight.enabled == true)
        {
            //Debug.Log("Has Interacted With Firestone");
            //torchMeterMoves.CurrentVal = torchMeterMoves.MaxVal;
            audioManagerScript.PlayTorchFireIgniteSFX();

            isInteracting = true;
            CheckToPlayAnims();
        }

        firestoneLight.enabled = false;
    }

    // Calls the scripts and functions to begin/trigger the NPC dialogue
    public void interactWithNPC(Collider collider)
    {
        //Debug.Log("Player has interacted with NPC");
        NonPlayerCharacter nonPlayerCharacterScript = collider.GetComponent<NonPlayerCharacter>();

        //dialogueViewsHolder.transform.position = collider.transform.position;
        nonPlayerCharacterScript.SetVariablesForCharacterDialogueScript();

        isInteracting = false;
        CheckToPlayAnims();
    }

    // Calls the scripts and functions to begin/trigger the Artifact dialogue
    public void interactWithArtifact(Collider collider)
    {
        Artifact artifactScript = collider.GetComponent<Artifact>();

        if (!artifactScript.HasCollectedArtifact)
        {
            //Debug.Log("Player has interacted with Artifact");
            //dialogueViewsHolder.transform.position = collider.transform.position;
            artifactScript.SetVariablesForCharacterDialogueScript();

            isInteracting = true;
            CheckToPlayAnims();
        }
    }

    // Sets the destination - allows the destination to be changed by other scripts
    public void setDestination(Vector3 newDestination)
    {
        destination = newDestination;
    }

    // Resets the current value of the torch meter to its maximum value
    public void ResetTorchMeter()
    {
        //torchMeterMoves.CurrentVal = torchMeterMoves.MaxVal;
        torchMeterScript.ResetTorchMeterElements();
    }

    // Determines if the player is on a bridge - returns true if so, false otherwise
    public bool onBridge()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        Physics.Raycast(myRay, out hit, rayLength);
        string name = hit.collider.name;

        if (name == "BridgeTile")
            return true;

        return false;
    }

    // Determines if the player is on a checkpoint tile - returns true if so, false otherwise
    public bool checkIfOnCheckpoint()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        Physics.Raycast(myRay, out hit, rayLength);
        string tag = hit.collider.tag;

        // If we did not hit a checkpoint
        if (tag != "Checkpoint") return false;

        checkpoint = hit.collider.gameObject;
        CheckpointManager checkpointManagerScript = checkpoint.GetComponent<CheckpointManager>();
        puzzle = hit.collider.transform.parent.parent.gameObject;

        // Sets the new torch meter value based on the checkpoint's value
        //int newNumMovements = checkpointManagerScript.GetNumMovements();
        //torchMeterMoves.setMaxValue(newNumMovements);    

        // If this is the first time we visited this checkpoint
        /*if (!checkpointManagerScript.hitCheckpoint())
        {
            if (canSetBoolsTrue)
                SetPlayerBoolsTrue(); //Enable Player Movement

            //if (!footstepsControllerScript.canPlaySecondFootstep)
                //footstepsControllerScript.canPlaySecondFootstep = true;

            bridgeTileCount = 0;
            checkpointManagerScript.setCheckpoint();
            checkpointManagerScript.LastBridgeTileCheck(); // Player's rotation is also saved in this function
            ResetTorchMeter();
            PopInTorchMeterCheck();
            saveManagerScript.SavePlayerPosition(checkpoint);
            saveManagerScript.SaveCameraPosition();
            hasMovedPuzzleView = false;
        }*/
        return true;
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

    // Checks if there's an edge - determines where the player cant move towards
    private bool EdgeCheck()
    {
        Ray myEdgeRay = new Ray(edgeCheck.transform.position, Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(myEdgeRay.origin, myEdgeRay.direction, Color.red);

        if (Physics.Raycast(myEdgeRay, out hit, rayLength))
        {
            string tag = hit.collider.tag;

            if (tag == "EmptyBlock")
                return false;
            else
                return true;
        }

        return false;
    }

    // Determines the next bridge tile to move towards
    private bool checkForNextBridgeTile()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.green);

        Physics.Raycast(myRay, out hit, rayLength);
        GameObject currentBridgeTile = hit.collider.gameObject;
        GameObject bridgeTileParentObject = currentBridgeTile.transform.parent.gameObject;
        string tag = currentBridgeTile.tag;
        string name = currentBridgeTile.name;

        if (name == "BridgeTile")
        {
            //footstepsControllerScript.PlayFootstepsSFX();
            // Torch meter pops out when the player gets on a bridge
            PopOutTorchMeterCheck();
            canTorchMeter = false;

            // Updates the bridge the player is/was on
            if (bridge != bridgeTileParentObject)
                bridge = bridgeTileParentObject;

            if (bridge.name == "EndBridge") // Make sure the parent object for the final bridge is called "EndBridge"
            {
                // Checks if the player has completed the zone - by landing on the final bridge
                if (!hasFinishedZone)
                {
                    //ResetTorchMeter();
                    audioManagerScript.PlayChimeSFX();
                    //gameManagerScript.FinishedZoneCheck();
                    //footstepsControllerScript.canPlaySecondFootstep = false;
                    hasFinishedZone = true;
                }

                // Disables the SFX for the player's footsteps - ONLY disabled when transitioning to another zone/scene
                /*if (bridgeTileCount == 8 && footstepsControllerScript.canPlayFootsteps != false)
                {
                    //Debug.Log("Disabled Footsteps SFX");
                    footstepsControllerScript.canPlayFootsteps = false;
                }*/

                // Doesn't look for the next bridge tile - ONLY occurs when the player is at the end of the final bridge (EndBridge)
                if (bridgeTileCount == bridgeTileParentObject.transform.childCount)
                    canCheckForNextTile = false;
            }

            // Looks for the next bridge tile and sets it as the destination
            if (transform.position == currentBridgeTile.transform.position && canCheckForNextTile && !introManagerScript.isActiveAndEnabled) // Doesn't check for bridge tiles during intro
            {
                isWalking = true;
                bridgeTileCount++;
                int rayDirection = 0;

                for (int i = 0; i <= 270; i += 90)
                {
                    Ray myRayLBT = new Ray(nextBridgeTileCheck.transform.position + new Vector3(0, -0.1f, 0), nextBridgeTileCheck.transform.TransformDirection(Vector3.forward));
                    RaycastHit hit02;
                    Debug.DrawRay(myRayLBT.origin, myRayLBT.direction, Color.red);

                    if (Physics.Raycast(myRayLBT, out hit02, rayLength))
                    {
                        GameObject nextBridgeTile = hit02.collider.gameObject;
                        string name02 = nextBridgeTile.name;
                        string tag02 = nextBridgeTile.tag;

                        if (name02 == "BridgeTile" && nextBridgeTile != previousBridgeTile || tag02 == "Checkpoint")
                        {
                            //Debug.Log("Next Tile Found");
                            //footstepsControllerScript.PlayFootstepsSFX();
                            destination = nextBridgeTile.transform.position;

                            float playerRotation = nextBridgeTileCheck.transform.eulerAngles.y;
                            transform.eulerAngles = new Vector3(0, playerRotation, 0);
                        }
                    }

                    //Debug.Log("Bridge Tile Not Found");
                    i = rayDirection;
                    rayDirection += 90;
                    nextBridgeTileCheck.transform.localEulerAngles = new Vector3(0, rayDirection, 0);
                }

                previousBridgeTile = currentBridgeTile;
                SetPlayerBoolsFalse();
                NextPuzzleViewCheck();
                return true;
            }
        }
        else
        {
            // Torch meter pops in when the player gets off a bridge
            PopInTorchMeterCheck();
            canTorchMeter = true;
        }

        return false;
    }

    // Checks when...
    private bool AlertBubbleCheck()
    {
        Ray myAlertBubbleRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), nextPos);
        RaycastHit hit;
        Debug.DrawRay(myAlertBubbleRay.origin, myAlertBubbleRay.direction, Color.blue);

        if (Physics.Raycast(myAlertBubbleRay, out hit, rayLength) /*&& torchMeterMoves.CurrentVal != 0f*/) // Note: Bubble doesn't pop up when frozen
        {
            string tag = hit.collider.tag;

            if (tag == "NPC")
            {
                //characterDialogueScript.SetAlertBubbleActive();
                return true;
            }
            else if (tag == "Artifact")
            {
                Artifact artifactScript = hit.collider.GetComponent<Artifact>();

                if (!artifactScript.HasCollectedArtifact)
                {
                    //characterDialogueScript.SetAlertBubbleActive();
                    return true;
                }
            }
        }
        //characterDialogueScript.SetAlertBubbleInactive();
        return false;
    }

    // Resets the current puzzle (determined by CheckpointManager)
    private void resetPuzzle()
    {
        // You cant restart a puzzle while on a bridge tile (determined in EdgeCheck)
        if (canRestartPuzzle)
        {
            //checkpoint.GetComponent<CheckpointManager>().resetPlayerPosition();

            //Debug.Log("Pushable blocks child count: " + puzzle.transform.childCount);
            for (int i = 0; i < puzzle.transform.childCount; i++)
            {
                GameObject child = puzzle.transform.GetChild(i).gameObject;

                if (child.name == "PushableBlocks")
                    child.GetComponent<ResetPushableBlocks>().resetBlocks();

                else if (child.name == "CrystalBlocks")
                    child.GetComponent<ResetCrystalBlocks>().ResetCrystals();

                else if (child.name == "BreakableBlocks")
                    child.GetComponent<ResetBreakableBlocks>().resetBlocks();

                else if (child.name == "Firestones")
                    child.GetComponent<ResetFireStone>().resetFirestone();

                else if (child.name == "GeneratorBlocks")
                    child.GetComponent<ResetGeneratorBlocks>().resetGenerator();
            }
        }

    }

    // Resets the current puzzle after a delay (determined by CheckpointManager)
    private void resetPuzzleWithDelay()
    {
        float resetDuration = gameManagerScript.resetPuzzleDelay;
        checkpoint.GetComponent<CheckpointManager>().StartCoroutine("resetPlayerPositionWithDelay", resetDuration);

        // Debug.Log("Pushable blocks child count: " + puzzle.transform.childCount);
        for (int i = 0; i < puzzle.transform.childCount; i++)
        {
            /*if (!gameHUDScript.canDeathScreen)
            {
                GameObject child = puzzle.transform.GetChild(i).gameObject;

                if (child.name == "PushableBlocks")
                    child.GetComponent<ResetPushableBlocks>().StartCoroutine("resetBlocksWithDelay", resetDuration);

                else if (child.name == "CrystalBlocks")
                    child.GetComponent<ResetCrystalBlocks>().StartCoroutine("ResetCrystalsWithDelay", resetDuration);

                else if (child.name == "BreakableBlocks")
                    child.GetComponent<ResetBreakableBlocks>().StartCoroutine("resetBlocksWithDelay", resetDuration);

                else if (child.name == "Firestones")
                    child.GetComponent<ResetFireStone>().StartCoroutine("resetFirestoneWithDelay", resetDuration);

                else if (child.name == "GeneratorBlocks")
                    child.GetComponent<ResetGeneratorBlocks>().StartCoroutine("resetGeneratorWithDelay", resetDuration);
            }*/
        }

        hasDied = true;
    }

    // Resets the current puzzle after a delay (determined by CheckpointManager) - USED ONLY IN TUTORIAL
    private void resetPuzzleWithDelayInTutorial()
    {
        float resetDuration = gameManagerScript.resetPuzzleDelay;
        checkpoint.GetComponent<CheckpointManager>().StartCoroutine("resetPlayerPositionInTutorialWithDelay", resetDuration);

        // Debug.Log("Pushable blocks child count: " + puzzle.transform.childCount);
        for (int i = 0; i < puzzle.transform.childCount; i++)
        {
            GameObject child = puzzle.transform.GetChild(i).gameObject;

            if (child.name == "PushableBlocks")
                child.GetComponent<ResetPushableBlocks>().StartCoroutine("resetBlocksWithDelay", resetDuration);

            else if (child.name == "CrystalBlocks")
                child.GetComponent<ResetCrystalBlocks>().StartCoroutine("ResetCrystalsWithDelay", resetDuration);

            else if (child.name == "BreakableBlocks")
                child.GetComponent<ResetBreakableBlocks>().StartCoroutine("resetBlocksWithDelay", resetDuration);

            else if (child.name == "Firestones")
                child.GetComponent<ResetFireStone>().StartCoroutine("resetFirestoneWithDelay", resetDuration);

            else if (child.name == "GeneratorBlocks")
                child.GetComponent<ResetGeneratorBlocks>().StartCoroutine("resetGeneratorWithDelay", resetDuration);
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

    // Subracts a value from the torch meter's total - subracts 1
    private void SubractFromTorchMeter()
    {
        //if (canTorchMeter)
            //torchMeterMoves.CurrentVal--;
    }

    // Checks of the torch meter can pop out
    public void PopOutTorchMeterCheck()
    {
        if (!hasAlreadyPopedOut)
        {
            torchMeterScript.TorchMeterPopOut();
            hasAlreadyPopedOut = true;
        }
    }

    // Checks of the torch meter can pop in
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
        //destination = gameManagerScript.checkpoints[0].position;
    }

    // Checks if the camera can move to the next puzzle view - does not move if on final puzzle
    private void NextPuzzleViewCheck()
    {
        // Make sure the parent objects for the first and last bridge are correct (first bridge = "EntryBridge", last bridge = "EndBridge")
        if (bridge.name != "EndBridge" && bridge.name != "EntryBridge" && !hasMovedPuzzleView)
        {
            //Debug.Log("Next Puzzle View Activated");
            cameraScript.NextPuzzleView();
            hasMovedPuzzleView = true;
        }
    }

    // Plays a new animation state
    private void ChangeAnimationState(string newState)
    {
        playerAnim.Play(newState);
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
            audioManagerScript.PlaySwooshSFX();
        }

        isPushing = false;
        isInteracting = false;
        //playerFidgetScript.SetIdleIndexToZero();
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        torchMeterScript = FindObjectOfType<TorchMeter>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        //dialogueCameraViewsScript = FindObjectOfType<DialogueCameraViews>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        //playerFidgetScript = FindObjectOfType<FidgetAnimControllerPlayer>();
        cameraScript = FindObjectOfType<CameraController>();
        saveManagerScript = FindObjectOfType<SaveManager>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        gameManagerScript = FindObjectOfType<GameManager>();
        introManagerScript = FindObjectOfType<IntroManager>();
        objectShakeScript = FindObjectOfType<ObjectShakeController>();
        footstepsControllerScript = FindObjectOfType<FootstepsController>();
    }

    // Sets private variables, objects, and components
    private void SetElemenets()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "EdgeCheck")
                edgeCheck = child;
            if (child.name == "NextBridgeTileCheck")
                nextBridgeTileCheck = child;
        }

        //dialogueViewsHolder = dialogueCameraViewsScript.gameObject;
        playerAnim = GetComponentInChildren<Animator>();
    }

    // Enables debugging for the torch meter - For Debugging Purposes ONLY
    private void TorchMeterDebugingCheck()
    {
        if (gameManagerScript.isDebugging)
        {
            //if (Input.GetKeyDown(KeyCode.LeftBracket))
                //torchMeterMoves.CurrentVal--;
            //if (Input.GetKeyDown(KeyCode.RightBracket))
                //torchMeterMoves.CurrentVal++;
        }
    }

}
