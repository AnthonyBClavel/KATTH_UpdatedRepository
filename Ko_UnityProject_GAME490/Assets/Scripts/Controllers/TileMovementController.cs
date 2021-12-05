using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TileMovementController : MonoBehaviour
{
    //private bool hasDied = false;
    [Header("Bools")]
    private bool canSetBoolsTrue = true;
    private bool canMove = true;
    private bool canInteract = true;
    private bool canRestartPuzzle = true;
    private bool hasFinishedZone = false;
    private bool alreadyPlayedSFX = false;
    private bool hasAlreadyPopedOut = false;
    private bool hasMovedPuzzleView = false; 
    private bool canCheckForNextBridgeTile = true;
    private bool hasFoundNextBridgeTile = false;
    private bool canTorchMeter = true;
    private bool canPlayFootsteps = true;
    private bool canPlaySecondFootstep = false;
    private bool hasSetCheckpoint = false;

    private float lerpLength; // 0.2f - The time it takes to move from its current position to the destination
    private float rayLength = 1f; // The length of the ray used by raycasts
    private float resetPuzzleDelay;
    private int bridgeTileCount;
    private int bridgeNumber;
    private int puzzleNumber;
    private new string tag;
    private new string name;
    private string objectToShakeName;

    [Header("Vectors")]
    private Vector3 nextPos;
    private Vector3 destination;
    private Vector3 currentDirection;

    Vector3 up = Vector3.zero, // Look North
    right = new Vector3(0, 90, 0), // Look East
    down = new Vector3(0, 180, 0), // Look South
    left = new Vector3(0, 270, 0); // Look West

    private GameObject playerEdgeCheck;
    private GameObject nextBridgeTileCheck;
    private GameObject dialogueViewsHolder;
    private GameObject previousBridgeTile;
    private GameObject objectToShake;
    private GameObject destroyedRockParticle;
    private Animator playerAnimator;

    [Header("Current Puzzle/Checkpoint/Bridge")]
    private GameObject checkpoint;
    private GameObject puzzle;
    private GameObject bridge;

    private IEnumerator playerFootstepsCoroutine;
    private IEnumerator playerMovementCoroutine;

    [Header("Scripts")]
    private TorchMeter torchMeterScript;
    private SaveManager saveManagerScript;
    private CharacterDialogue characterDialogueScript;
    private CameraController cameraScript;
    private PlayerFidgetController playerFidgetScript;
    private AudioManager audioManagerScript;
    private GameManager gameManagerScript;
    private IntroManager introManagerScript;
    private ObjectShakeController objectShakeScript;
    private PuzzleManager puzzleManagerScript;
    private TransitionFade transitionFadeScript;
    private EndCredits endCreditsScript;

    void Awake()
    {
        SetScripts();
        SetElemenets();
    }

    // Start is called before the first frame update
    void Start()
    {
        currentDirection = up;
        nextPos = Vector3.forward;
        destination = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        checkIfOnCheckpoint();
        checkForNextBridgeTile();
        AlertBubbleCheck();
    }

    // Returns or sets the current puzzle
    public GameObject CurrentPuzzle
    {
        get
        {
            return puzzle;
        }
        set
        {
            puzzle = value;
        }
    }

    // Returns or sets the current checkpoint
    public GameObject CurrentCheckpoint
    {
        get
        {
            return checkpoint;
        }
        set
        {
            checkpoint = value;
        }
    }

    // Returns the value of puzzleNumber
    public int PuzzleNumber
    {
        get
        {
            return puzzleNumber;
        }
        set
        {
            puzzleNumber = value;
        }
    }

    // Returns or sets the value of LerpLength
    public float LerpLength
    {
        get
        {
            return lerpLength;
        }
        set
        {
            lerpLength = value;
        }
    }

    // Returns or sets the value of resetPuzzleDelay
    public float ResetPuzzleDelay
    {
        get
        {
            return resetPuzzleDelay;
        }
        set
        {
            resetPuzzleDelay = value;
        }
    }

    // Returns or sets the value of hasFinishedZone
    public bool HasFinishedZone
    {
        get
        {
            return hasFinishedZone;
        }
        set
        {
            hasFinishedZone = value;
        }
    }

    // Returns the value of canMove
    public bool CanMove
    {
        get
        {
            return canMove;
        }
    }

    // Returns the value of canRestartPuzzle
    public bool CanRestartPuzzle
    {
        get
        {
            return canRestartPuzzle;
        }
    }

    // Sets the value of canSetBoolsTrue
    public bool CanSetBoolsTrue
    {
        set
        {
            canSetBoolsTrue = value;
        }
    }

    // The main movement function for the player - tile movement
    void Move()
    {
        /*** Old Movement Code ***/
        // transform.position = Vector3.MoveTowards(transform.position, destination, 5f * Time.deltaTime);

        // Before the player reaches it's destination (or when player is within 0.00001 units of its destination)
        if (Vector3.Distance(destination, transform.position) <= 0.00001f)
        {
            //checkIfOnCheckpoint();
            //checkForNextBridgeTile();

            // Resets the SFX for the torch meter back to false
            if (torchMeterScript.CurrentVal > 0 && alreadyPlayedSFX)
                alreadyPlayedSFX = false;

            // Checks of the torch meter ran out - resest the puzzle and plays torch meter sfx
            if (torchMeterScript.CurrentVal <= 0 && canTorchMeter && !alreadyPlayedSFX)
            {
                if (SceneManager.GetActiveScene().name == "TutorialMap")
                    resetPuzzleWithDelayInTutorial();
                else
                {
                    checkpoint.GetComponent<CheckpointManager>().ResetPlayerDelay(resetPuzzleDelay);

                    if (!gameManagerScript.canDeathScreen)
                        puzzleManagerScript.ResetPuzzle(resetPuzzleDelay);
                }

                audioManagerScript.PlayTorchFireExtinguishSFX();
                audioManagerScript.PlayFreezeingSFX();
                alreadyPlayedSFX = true;
            }
        }

        if (!updateKeyboardInput()) return; // Returns if there's no movement input - code below doesnt get called
        transform.localEulerAngles = currentDirection;

        // Checks for no colliders
        if (!colliderCheck())
        {
            // Checks for no edges or holes
            if (!edgeCheck())
            {
                destination = transform.position + nextPos;
                MoveToNextPosition();

                // Checks if the player is not on a bridge tile
                if (!onBridge())
                    SubractFromTorchMeter();
            }
        }
    }

    // Updates the keyboard input - returns true if input is recieved, false otherwise 
    bool updateKeyboardInput()
    {
        if (!onBridge()) // Note: cannot update input while on bridge!
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
                else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    nextPos = Vector3.left;
                    currentDirection = left;
                    return true;
                }
            }
            /*** Movement inputs END here ***/

            // Hit R to reset puzzle
            if (canRestartPuzzle && Input.GetKeyDown(KeyCode.R))
            {
                checkpoint.GetComponent<CheckpointManager>().ResetPlayer();
                puzzleManagerScript.ResetPuzzle(0f);
            }

            // Hit Return/Space/Enter to break or interact with block
            if (canInteract && torchMeterScript.CurrentVal > 0)
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
        }
        return false;
    }

    // Checks for colliders - return true if so, false otherwise
    bool colliderCheck()
    {
        Collider collider = getCollider();

        // Returns if there's no collider (no obstacles)
        if (collider == null)
            return false;

        // Set objectToShake only if the collider object has children
        if (collider.transform.childCount > 0)
        {
            objectToShake = collider.transform.GetChild(0).gameObject;
            objectToShakeName = objectToShake.name;
        }

        switch (collider.tag)
        {
            case ("StaticBlock"):
                objectShakeScript.ShakeObject(objectToShake);
                ChangeAnimationState("Pushing");
                if (objectToShakeName == "Tree")
                    audioManagerScript.PlayTreeHitSFX();
                else if (objectToShakeName == "SnowTree" || objectToShakeName == "BarrenTree")
                    audioManagerScript.PlaySnowTreeHitSFX();
                else if (objectToShakeName == "GasBarrel")
                    audioManagerScript.PlayMetalHitSFX();
                else if (objectToShakeName == "Crystal")
                {
                    collider.GetComponent<Crystal>().PlayCrystalLightAnim();
                    audioManagerScript.PlayCrystalHitSFX();
                }
                break;

            case ("PushableBlock"):
                BlockMovementController blockMovementScript = collider.GetComponent<BlockMovementController>();
                gameManagerScript.CheckForBlockMovementDebug(blockMovementScript);
                if (blockMovementScript.MoveBlock())
                    SubractFromTorchMeter();
                ChangeAnimationState("Pushing");
                break;

            case ("DestroyableBlock"):
                objectShakeScript.ShakeObject(objectToShake);
                audioManagerScript.PlayRockHitSFX();
                ChangeAnimationState("Pushing");
                break;

            default:
                //Debug.Log("Unrecognizable Tag");
                playerFidgetScript.SetIdleIndexToZero();
                break;
        }
        return true;
    }

    // Draws a ray forward - returns the collider of the object that it hits, null otherwise 
    public Collider getCollider()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), transform.TransformDirection(Vector3.forward));
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

        GameObject newDestroyedRockParticle = Instantiate(destroyedRockParticle, destroyableBlock.transform.position + new Vector3(0, -0.25f, 0), destroyableBlock.transform.rotation);
        newDestroyedRockParticle.transform.parent = gameManagerScript.transform;

        SubractFromTorchMeter();
        audioManagerScript.PlayRockBreakSFX();
        destroyableBlock.SetActive(false);

        ChangeAnimationState("Interacting");
    }

    // Turns on the generator if the player is colliding/interacting with it
    public void turnOnGenerator(Collider collider)
    {
        Generator generatorScript = collider.gameObject.GetComponent<Generator>();
        audioManagerScript.SetGeneratorScript(generatorScript);

        if (generatorScript.IsActive == false)
        {
            //Debug.Log("Turned On Generator");
            generatorScript.TurnOnGenerator();
            SubractFromTorchMeter();

            ChangeAnimationState("Interacting");
        }
    }

    // Disables the firestone's light if the player is interacting with it
    public void getFirestone(Collider collider)
    {
        Light firestoneLight = collider.gameObject.GetComponentInChildren<Light>();

        if (firestoneLight.enabled == true)
        {
            //Debug.Log("Has Interacted With Firestone");
            torchMeterScript.CurrentVal = torchMeterScript.MaxVal;
            audioManagerScript.PlayTorchFireIgniteSFX();

            ChangeAnimationState("Interacting");
        }

        firestoneLight.enabled = false;
    }

    // Calls the scripts and functions to begin/trigger the NPC dialogue
    public void interactWithNPC(Collider collider)
    {
        //Debug.Log("Player has interacted with NPC");
        NonPlayerCharacter nonPlayerCharacterScript = collider.GetComponent<NonPlayerCharacter>();

        dialogueViewsHolder.transform.position = collider.transform.position;
        nonPlayerCharacterScript.SetVariablesForCharacterDialogueScript();
    }

    // Calls the scripts and functions to begin/trigger the Artifact dialogue
    public void interactWithArtifact(Collider collider)
    {
        Artifact artifactScript = collider.GetComponent<Artifact>();

        if (!artifactScript.HasCollectedArtifact)
        {
            //Debug.Log("Player has interacted with Artifact");
            dialogueViewsHolder.transform.position = collider.transform.position;
            artifactScript.SetVariablesForCharacterDialogueScript();

            ChangeAnimationState("Interacting");
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
        torchMeterScript.CurrentVal = torchMeterScript.MaxVal;
        torchMeterScript.ResetTorchMeterElements();
    }

    // Determines if the player is on a bridge tile - returns true if so, false otherwise
    public bool onBridge()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;
        //Debug.DrawRay(myRay.origin, myRay.direction, Color.yellow);

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
        //Debug.DrawRay(myRay.origin, myRay.direction, Color.green);

        Physics.Raycast(myRay, out hit, rayLength);
        GameObject currentTile = hit.collider.gameObject;
        string tag = currentTile.tag;           

        if (!hasSetCheckpoint && !onBridge())
        {
            if (tag != "Checkpoint")
            {
                for (int i = 0; i < currentTile.transform.parent.childCount; i++)
                {
                    GameObject child = currentTile.transform.parent.GetChild(i).gameObject;

                    if (child.tag == "Checkpoint")
                        checkpoint = child;
                }
            }
            if (tag == "Checkpoint")
                checkpoint = currentTile;        

            if (!canPlaySecondFootstep)
                canPlaySecondFootstep = true;

            puzzle = currentTile.transform.parent.parent.gameObject;
            CheckpointManager checkpointManagerScript = checkpoint.GetComponent<CheckpointManager>();
            ConvertObjectNameToNumber(puzzle, "Puzzle", ref puzzleNumber);

            int newNumMovements = checkpointManagerScript.getNumMovements();
            torchMeterScript.MaxVal = newNumMovements; // Sets the new max value for the torch meter

            bridgeTileCount = 0;
            ResetTorchMeter();
            PopInTorchMeterCheck();
            checkpointManagerScript.LastBridgeTileCheck(); // Player's rotation is saved in this function
            saveManagerScript.SavePlayerPosition(checkpoint);
            saveManagerScript.SaveCameraPosition();

            previousBridgeTile = null;
            hasMovedPuzzleView = false;
            canSetBoolsTrue = true;
            hasSetCheckpoint = true;
        }

        if (tag == "Checkpoint")
            return true;

        return false;
    }

    // Determines if the player on the last tile block in a puzzle - returns true if so, false otherwise - ONLY USED DURING TUTORIAL
    public bool onLastTileBlock()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;
        //Debug.DrawRay(myRay.origin, myRay.direction, Color.cyan);

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

    // Determines where the player can't move to - returns true if there's an edge, false otherwise
    private bool edgeCheck()
    {
        Ray myEdgeRay = new Ray(playerEdgeCheck.transform.position, Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(myEdgeRay.origin, myEdgeRay.direction, Color.red);

        if (Physics.Raycast(myEdgeRay, out hit, rayLength))
        {
            string tag = hit.collider.tag;

            // Cannot walk over holes - counts as an "edge"
            if (tag == "EmptyBlock")
                return true;
            else
                return false;
        }

        // If the ray doesn't hit anything, then there's an edge
        return true;
    }

    // Determines the next bridge tile to move towards
    private bool checkForNextBridgeTile()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        Physics.Raycast(myRay, out hit, rayLength);
        GameObject currentBridgeTile = hit.collider.gameObject;
        GameObject bridgeTileParentObject = currentBridgeTile.transform.parent.gameObject;
        string tag = currentBridgeTile.tag;
        string name = currentBridgeTile.name;

        if (name == "BridgeTile")
        {
            // Torch meter pops out when the player gets on a bridge
            PopOutTorchMeterCheck();
            canTorchMeter = false;

            if (previousBridgeTile != currentBridgeTile)
                hasFoundNextBridgeTile = false;

            // Updates the bridge the player is/was on
            if (bridge != bridgeTileParentObject)
                bridge = bridgeTileParentObject;

            if (bridge.name == "EndBridge") // Make sure the parent object for the final bridge is called "EndBridge"
            {
                // Checks if the player has completed the zone - by landing on the final bridge
                if (!hasFinishedZone)
                {
                    SetExitZoneElements();
                    audioManagerScript.PlayChimeSFX();
                    hasFinishedZone = true;
                }

                // Disables the SFX for the player's footsteps - ONLY disabled when transitioning to another zone/scene
                if (bridgeTileCount == 8 && canPlayFootsteps)
                    canPlayFootsteps = false;

                // Doesn't look for the next bridge tile - ONLY occurs when the player is at the end of the final bridge (EndBridge)
                if (bridgeTileCount == bridgeTileParentObject.transform.childCount)
                {
                    canCheckForNextBridgeTile = false;
                    StopPlayerCoroutines();
                }
            }

            // Looks for the next bridge tile and sets it as the destination
            if (transform.position == currentBridgeTile.transform.position && canCheckForNextBridgeTile && !hasFoundNextBridgeTile && !introManagerScript.isActiveAndEnabled) // Doesn't check for bridge tiles during intro
            {
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

                        //if (name02 == "BridgeTile" || tag02 == "Checkpoint" || name02.Contains("Tile"))
                        if (nextBridgeTile != previousBridgeTile)
                        {
                            //Debug.Log("Next Bridge Tile Found");
                            destination = nextBridgeTile.transform.position;
                            StartMovementCoroutine();

                            transform.eulerAngles = new Vector3(0, nextBridgeTileCheck.transform.eulerAngles.y, 0);
                            hasFoundNextBridgeTile = true;
                            break;
                        }
                    }

                    //Debug.Log("Bridge Tile Not Found");
                    i = rayDirection;
                    rayDirection += 90;
                    nextBridgeTileCheck.transform.localEulerAngles = new Vector3(0, rayDirection, 0);
                }

                previousBridgeTile = currentBridgeTile;
                NextPuzzleViewCheck();
                SetPlayerBoolsFalse();
                hasSetCheckpoint = false;
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

    // Checks when to set the alert bubble active/inactive
    private bool AlertBubbleCheck()
    {
        Ray myAlertBubbleRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), transform.TransformDirection(Vector3.forward));
        RaycastHit hit;
        //Debug.DrawRay(myAlertBubbleRay.origin, myAlertBubbleRay.direction, Color.red);

        if (Physics.Raycast(myAlertBubbleRay, out hit, rayLength) && torchMeterScript.CurrentVal != 0f) // Note: Alert bubble doesn't pop up when frozen
        {
            string tag = hit.collider.tag;

            // If the player collides with an NPC
            if (tag == "NPC")
            {
                characterDialogueScript.SetAlertBubbleActive();
                return true;
            }
            // If the player hasn't collected an artifact within an artifact chest
            if (tag == "Artifact")
            {
                Artifact artifactScript = hit.collider.GetComponent<Artifact>();

                if (!artifactScript.HasCollectedArtifact)
                {
                    characterDialogueScript.SetAlertBubbleActive();
                    return true;
                }
            }
            else
                return false;
        }

        characterDialogueScript.SetAlertBubbleInactive();
        return false;
    }

    // Resets the current puzzle after a delay (determined by current checkpoint) - USED ONLY IN TUTORIAL
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

        //hasDied = true;
    }

    public void SetExitZoneElements()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Plays the end credits after completing the final zone
        if (currentScene == "FifthMap")
            endCreditsScript.StartEndCredits();

        gameManagerScript.ResetCollectedArtifactsCheck();
        transitionFadeScript.GameFadeOut();
        SetPlayerBoolsFalse();
        canSetBoolsTrue = false;
        canPlaySecondFootstep = false;
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
        if (canTorchMeter)
            torchMeterScript.CurrentVal--;
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

    // Removes the string (prefixToRemove) from the parent object's name and converst what left into 
    private void ConvertObjectNameToNumber(GameObject parentObject, string prefixToRemove, ref int integer)
    {
        // Checks if the parent object's name has a number in it (returns true if so)
        string parentObjectName = parentObject.name;
        bool hasNumberInName = parentObjectName.Any(char.IsDigit);

        if (parentObjectName.Contains(prefixToRemove) && hasNumberInName)
        {
            parentObjectName = parentObjectName.Replace(prefixToRemove, "");
            integer = int.Parse(parentObjectName);
        }
    }

    // Checks if the camera can move to the next puzzle view - does not move if on final puzzle
    private void NextPuzzleViewCheck()
    {
        // Note: make sure the parent objects for the first and last bridge are correct (first bridge = "EntryBridge", last bridge = "EndBridge")
        // The camera doesnt lerp if you land on these bridges...
        if (bridge.name != "EndBridge" && bridge.name != "EntryBridge" && !hasMovedPuzzleView)
        {
            ConvertObjectNameToNumber(bridge, "Bridge", ref bridgeNumber);

            //Debug.Log("Next Puzzle View Activated");
            if (bridgeNumber != puzzleNumber)
                cameraScript.PreviousPuzzleView();
            else
                cameraScript.NextPuzzleView();

            canSetBoolsTrue = false;
            hasMovedPuzzleView = true;
        }
    }

    // Plays a new animation state
    public void ChangeAnimationState(string newState)
    {
        if (newState == "Interacting")
            audioManagerScript.PlaySwooshSFX();

        playerAnimator.Play(newState);
        playerFidgetScript.SetIdleIndexToZero();
    }

    // Checks which footstep sfx to play for the PLAYER - determined by the tag/name of object the player walks on
    private void PlayerFootstepSFXCheck()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;
        //Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        if (Physics.Raycast(myRay, out hit, rayLength) && canPlayFootsteps)
        {
            if (tag != hit.collider.tag)
                tag = hit.collider.tag;

            if (name != hit.collider.name)
                name = hit.collider.name;

            // Checks to play a footstep sfx for grass
            if (tag == "GrassTile" || name == "Checkpoint_GrassTile")
                audioManagerScript.PlayGrassFootstepSFX();

            // Checks to play a footstep sfx for snow
            else if (tag == "SnowTile" || name == "Checkpoint_SnowTile" || name == "Checkpoint_SnowTile02")
                audioManagerScript.PlaySnowFootstepSFX();

            // Checks to play a footstep sfx for stone
            else if (tag == "StoneTile" || name == "Checkpoint_StoneTile")
                audioManagerScript.PlayStoneFootstepSFX();

            // Checks to play a footstep sfx for metal
            else if (tag == "MetalTile" || name == "Checkpoint_MetalTile")
                audioManagerScript.PlayMetalFootstepSFX();

            // Checks to play a footstep sfx for wood (bridge tiles)
            else if (tag == "BridgeTile" || name == "BridgeTile")
                audioManagerScript.PlayWoodFootstepSFX();

            // Checks to play a footstep sfx for crates (wooden crates)
            else if (tag == "PushableBlock")
                audioManagerScript.PlayCrateFootstepSFX();
        }
    }

    // Checks to move the player's position (over a duration, or instantly)
    private void MoveToNextPosition()
    {
        gameManagerScript.CheckForPlayerScriptDebug();

        // Moves position over duration...
        if (lerpLength != 0)
        {
            // Only move if at destination and not on bridge
            if (canMove)
            {
                StartMovementCoroutine();
                canMove = false;
            }
        }
        // Moves position instantly...
        else
            transform.position = destination;
    }

    // Stops the player's movement and footstep coroutines
    public void StopPlayerCoroutines()
    {
        ChangeAnimationState("Idle");

        if (playerMovementCoroutine != null)
            StopCoroutine(playerMovementCoroutine);

        if (playerFootstepsCoroutine != null)
            StopCoroutine(playerFootstepsCoroutine);
    }

    // Starts/overrides the coroutine for the player movement - PRIMARILY FOR FINDING NEXT BRIDGE TILE
    private void StartMovementCoroutine()
    {
        // Stops the player movement coroutine if active
        if (playerMovementCoroutine != null)
            StopCoroutine(playerMovementCoroutine);

        playerMovementCoroutine = MovePlayerPosition(destination, lerpLength);
        StartCoroutine(playerMovementCoroutine);

        // Plays the sfx for the player's footsteps
        PlayerFootstepsSFX();
    }

    // Plays the footsteps coroutine for the player
    private void PlayerFootstepsSFX()
    {
        if (playerFootstepsCoroutine != null)
            StopCoroutine(playerFootstepsCoroutine);

        playerFootstepsCoroutine = PlayerFootsteps();
        StartCoroutine(playerFootstepsCoroutine);
    }

    // Moves the player's position to a new position over a duration (endPosition = position to move to, duration = seconds)
    private IEnumerator MovePlayerPosition(Vector3 endPosition, float duration)
    {
        float time = 0f;
        Vector3 startPosition = transform.position;

        while (time < duration)
        {
            if (playerFidgetScript.CurrentAnimPlaying != "Walking")
                ChangeAnimationState("Walking");
            transform.position = Vector3.MoveTowards(startPosition, endPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;

        if (!onBridge() && canSetBoolsTrue)
        {
            playerAnimator.SetTrigger("Idle");

            if (!canMove)
                canMove = true;

            if (!canRestartPuzzle)
                canRestartPuzzle = true;

            if (!canInteract)
                canInteract = true;
        }         
    }

    // Plays the footsteps sfx based on the player's lerpLength (time it takes to move from its current position to the destination)
    private IEnumerator PlayerFootsteps()
    {
        float duration = lerpLength / 4f;

        // Plays a footstep sfx when the player has traveled for 1/4th of the lerpLength
        yield return new WaitForSeconds(duration);
        PlayerFootstepSFXCheck();

        // Plays a footstep sfx when the player has traveled for 3/4th of the lerpLength 
        yield return new WaitForSeconds(duration * 2f);
        if (canPlaySecondFootstep)
            PlayerFootstepSFXCheck();
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        torchMeterScript = FindObjectOfType<TorchMeter>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        playerFidgetScript = FindObjectOfType<PlayerFidgetController>();
        cameraScript = FindObjectOfType<CameraController>();
        saveManagerScript = FindObjectOfType<SaveManager>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        gameManagerScript = FindObjectOfType<GameManager>();
        introManagerScript = FindObjectOfType<IntroManager>();
        objectShakeScript = FindObjectOfType<ObjectShakeController>();
        puzzleManagerScript = FindObjectOfType<PuzzleManager>();
        transitionFadeScript = FindObjectOfType<TransitionFade>();
        endCreditsScript = FindObjectOfType<EndCredits>();
    }

    // Sets private variables, objects, and components
    private void SetElemenets()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.name == "PlayerModel")
                playerAnimator = child.GetComponent<Animator>();
            if (child.name == "EdgeCheck")
                playerEdgeCheck = child;
            if (child.name == "NextBridgeTileCheck")
                nextBridgeTileCheck = child;
        }

        for (int i = 0; i < cameraScript.transform.parent.childCount; i++)
        {
            GameObject child = cameraScript.transform.parent.GetChild(i).gameObject;

            if (child.name == "DialogueViewsHolder")
                dialogueViewsHolder = child;
        }

        destroyedRockParticle = gameManagerScript.destroyedRockParticle;
        lerpLength = gameManagerScript.playerLerpLength;
        resetPuzzleDelay = gameManagerScript.resetPuzzleDelay;
    }

}
