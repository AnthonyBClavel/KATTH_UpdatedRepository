using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Linq;

public class TileMovementController : MonoBehaviour
{
    [Header("Bools")]
    [SerializeField]
    private bool canMove = true;
    [SerializeField]
    private bool canInteract = true;
    [SerializeField]
    private bool canRestartPuzzle = true;
    private bool hasFinishedZone = false;
    private bool hasAlreadyPopedOut = false;
    private bool hasMovedPuzzleView = false;
    private bool hasSetCheckpoint = false;

    private float lerpLength; // 0.2f - The time it takes to move from its current position to the destination
    private float rayLength = 1f; // The length of the ray used by raycasts
    private float resetPuzzleDelay;
    private int bridgeTileCount;
    private int bridgeNumber;
    private int puzzleNumber;

    [Header("Vectors")]
    private Vector3 nextPos;
    private Vector3 destination;
    private Vector3 currentDirection;

    Vector3 north = Vector3.zero,
    east = new Vector3(0, 90, 0),
    south = new Vector3(0, 180, 0),
    west = new Vector3(0, 270, 0);

    [Header("Current Puzzle/Checkpoint/Bridge")]
    private GameObject checkpoint;
    private GameObject puzzle;
    private GameObject bridge;

    private GameObject currentTile;
    private GameObject previousTile;

    private GameObject bridgeTileCheck;
    private GameObject tileCheck;
    private GameObject dialogueViewsHolder;
    private GameObject destroyedRockParticle;
    private Animator playerAnimator;

    private IEnumerator playerFootstepsCoroutine;
    private IEnumerator playerMovementCoroutine;

    [Header("Variables for Grass Material")]
    private Material grassMat;
    private Vector3 grassMatPosition;

    [Header("Scripts")]
    private TorchMeter torchMeterScript;
    private SaveManager saveManagerScript;
    private CharacterDialogue characterDialogueScript;
    private CameraController cameraScript;
    private FidgetController playerFidgetScript;
    private AudioManager audioManagerScript;
    private GameManager gameManagerScript;
    private ObjectShakeController objectShakeScript;
    private PuzzleManager puzzleManagerScript;
    private TransitionFade transitionFadeScript;
    private EndCredits endCreditsScript;
    private TutorialDialogue tutorialDialogueScript;

    public GameObject CurrentPuzzle
    {
        get { return puzzle; }
        set { puzzle = value; }
    }

    public GameObject CurrentCheckpoint
    {
        get { return checkpoint; }
        set { checkpoint = value; }
    }

    public Vector3 Destination
    {
        get { return destination; }
        set { destination = value; }
    }

    public int PuzzleNumber
    {
        get { return puzzleNumber; }
        set { puzzleNumber = value; }
    }

    public float LerpLength
    {
        get { return lerpLength; }
        set { lerpLength = value; }
    }

    public float ResetPuzzleDelay
    {
        get { return resetPuzzleDelay; }
        set { resetPuzzleDelay = value; }
    }

    public bool HasFinishedZone
    {
        get { return hasFinishedZone; }
        set { hasFinishedZone = value; }
    }

    public bool CanMove
    {
        get { return canMove; }
    }

    public bool CanRestartPuzzle
    {
        get { return canRestartPuzzle; }
    }

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElemenets();
    }

    // Start is called before the first frame update
    void Start()
    {
        currentDirection = north;
        nextPos = Vector3.forward;
        destination = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    // Sets the input-related bools to false
    public void SetPlayerBoolsFalse()
    {
        canMove = false;
        canRestartPuzzle = false;
        canInteract = false;
    }

    // Sets the input-related bools to true
    public void SetPlayerBoolsTrue()
    {
        canMove = true;
        canRestartPuzzle = true;
        canInteract = true;
    }

    // Draws a ray forward - returns the collider of the object that it hits, null otherwise 
    public Collider GetCollider()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), transform.TransformDirection(Vector3.forward));
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        if (Physics.Raycast(myRay, out hit, rayLength))
            return hit.collider;

        return null;
    }

    /*** MAIN MOVEMENT METHOD ***/
    // Checks when the player can move when to reset the puzzle
    private void Move()
    {
        // Before the player reaches it's destination (when the player is within 0.00001 units of its destination)
        if (Vector3.Distance(destination, transform.position) <= 0.00001f)
        {
            // Checks if the torch meter ran out - resest the puzzle and plays the appropriate sfx if so
            if (torchMeterScript.CurrentVal <= 0 && canMove && !OnBridge())
            {   
                puzzleManagerScript.ResetPuzzle(resetPuzzleDelay); // Note: canMove is set to false within this method, so the sfx below only play once  
                audioManagerScript.PlayTorchFireExtinguishSFX();
                audioManagerScript.PlayFreezeingSFX();
            }
        }

        // Checks if a movement key is pressed
        if (RecievedMovementInput())
        {
            playerFidgetScript.SetIdleCountToZero();
            transform.localEulerAngles = currentDirection;
            AlertBubbleCheck();

            // Checks for no colliders and no edges
            if (!ColliderCheck() && !EdgeCheck())
            {
                destination = transform.position + nextPos;
                MoveToNextPosition();
                SubractFromTorchMeter(1);
            }
        }
    }

    // Checks if movement input was recieved - returns true if so, false otherwise 
    private bool RecievedMovementInput()
    {
        // Cannot update input while the player is on a bridge, or while exiting a scene!
        if (!OnBridge() && !transitionFadeScript.IsChangingScenes) 
        {
            /*** Movement input STARTS here ***/
            if (canMove)
            {
                // Move North
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    nextPos = Vector3.forward;
                    currentDirection = north;
                    return true;
                }

                // Move South
                else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    nextPos = Vector3.back;
                    currentDirection = south;
                    return true;
                }

                // Move East
                else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    nextPos = Vector3.right;
                    currentDirection = east;
                    return true;
                }

                // Move West
                else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    nextPos = Vector3.left;
                    currentDirection = west;
                    return true;
                }
            }
            /*** Movement input ENDS here ***/

            // Reset puzzle
            if (Input.GetKeyDown(KeyCode.R) && canRestartPuzzle)
            {
                puzzleManagerScript.ResetPuzzle(0f);
                playerFidgetScript.SetIdleCountToZero();
            }

            // Interact/Break
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (torchMeterScript.CurrentVal > 0 && canInteract)
                {
                    Collider collider = GetCollider();

                    if (collider == null)
                        return false;

                    switch (collider.tag)
                    {
                        case ("DestroyableBlock"):
                            DestroyBlock(collider);
                            break;
                        case ("Firestone"):
                            UseFirestone(collider);
                            break;
                        case ("Generator"):
                            TurnOnGenerator(collider);
                            break;
                        case ("NPC"):
                            InteractWithNPC(collider);
                            break;
                        case ("Artifact"):
                            InteractWithArtifact(collider);
                            break;
                        default:
                            //Debug.Log("Unrecognizable tag");
                            break;
                    }
                }
            }
        }

        return false;
    }

    // Checks for colliders - return true if so, false otherwise
    private bool ColliderCheck()
    {
        Collider collider = GetCollider();
        PushBlockCheck(collider);

        // Returns false if there's no collider (no obstacles)
        if (collider == null)
            return false;

        // Set objectToShake only if the collider object has children
        if (collider.transform.childCount > 0)
        {
            GameObject objectToShake = collider.transform.GetChild(0).gameObject;
            string tag = collider.tag;

            if (tag == "StaticBlock" || tag == "DestroyableBlock")
                ChangeAnimationState("Pushing");

            switch (objectToShake.name)
            {
                case ("Tree"):
                    objectShakeScript.ShakeObject(objectToShake);
                    audioManagerScript.PlayTreeHitSFX();
                    break;
                case ("SnowTree"):
                    objectShakeScript.ShakeObject(objectToShake);
                    audioManagerScript.PlaySnowTreeHitSFX();
                    break;
                case ("BarrenTree"):
                    objectShakeScript.ShakeObject(objectToShake);
                    audioManagerScript.PlaySnowTreeHitSFX();
                    break;
                case ("Rock"):
                    objectShakeScript.ShakeObject(objectToShake);
                    audioManagerScript.PlayRockHitSFX();
                    break;
                case ("GasBarrel"):
                    objectShakeScript.ShakeObject(objectToShake);
                    audioManagerScript.PlayMetalHitSFX();
                    break;
                case ("Crystal"):
                    objectShakeScript.ShakeObject(objectToShake);
                    collider.GetComponent<Crystal>().PlayCrystalLightAnim();
                    audioManagerScript.PlayCrystalHitSFX();
                    break;
                default:
                    //Debug.Log("Unrecognizable object name");
                    break;
            }
        }

        return true;
    }

    // Checks if the object can be pushed
    private void PushBlockCheck(Collider collider)
    {
        if (collider != null && collider.tag == "PushableBlock")
        {
            BlockMovementController blockMovementScript = collider.GetComponent<BlockMovementController>();
            gameManagerScript.CheckForBlockMovementDebug(blockMovementScript);

            if (blockMovementScript.MoveBlock())
                SubractFromTorchMeter(1);

            ChangeAnimationState("Pushing");
        }
    }

    // Sets the object inactive
    private void DestroyBlock(Collider collider)
    {
        GameObject destroyableBlock = collider.gameObject;
        GameObject newDestroyedRockParticle = Instantiate(destroyedRockParticle, destroyableBlock.transform.position + new Vector3(0, -0.25f, 0), destroyableBlock.transform.rotation);
        newDestroyedRockParticle.transform.parent = gameManagerScript.transform;

        SubractFromTorchMeter(1);
        audioManagerScript.PlayRockBreakSFX();
        destroyableBlock.SetActive(false);

        ChangeAnimationState("Interacting");
        //Debug.Log("Destroyed Block");
    }

    // Turns off the firestone's light
    private void UseFirestone(Collider collider)
    {
        Light firestoneLight = collider.gameObject.GetComponentInChildren<Light>();

        if (firestoneLight.enabled == true)
        {
            torchMeterScript.CurrentVal = torchMeterScript.MaxVal;
            audioManagerScript.PlayTorchFireIgniteSFX();
            firestoneLight.enabled = false;

            ChangeAnimationState("Interacting");
            //Debug.Log("Firestone has been used");
        }
    }

    // Turns on the generator
    private void TurnOnGenerator(Collider collider)
    {
        Generator generatorScript = collider.gameObject.GetComponent<Generator>();
        audioManagerScript.SetGeneratorScript(generatorScript);

        if (generatorScript.IsGenActive == false)
        {
            generatorScript.TurnOnGenerator();
            SubractFromTorchMeter(1);

            ChangeAnimationState("Interacting");
            //Debug.Log("Generator has been turned on");
        }
    }

    // Calls the methods to begin/trigger the NPC dialogue
    private void InteractWithNPC(Collider collider)
    {
        dialogueViewsHolder.transform.position = collider.transform.position;
        collider.GetComponent<NonPlayerCharacter>().SetVariablesForCharacterDialogueScript();

        //Debug.Log("Interacted with NPC");
    }

    // Calls the methods to begin/trigger the Artifact dialogue
    private void InteractWithArtifact(Collider collider)
    {
        Artifact artifactScript = collider.GetComponent<Artifact>();

        if (!artifactScript.HasCollectedArtifact)
        {
            dialogueViewsHolder.transform.position = collider.transform.position;
            artifactScript.StartArtifactDialogue();
            artifactScript.OpenChest();
            ChangeAnimationState("Interacting");

            //Debug.Log("Interacted with Artifact");
        }
    }

    // Sets the current tile and bridge the player is on
    public void CurrentTileCheck()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.cyan);

        if (Physics.Raycast(myRay, out hit, rayLength))
        {
            currentTile = hit.collider.gameObject;
            OnCheckpoint();
            AlertBubbleCheck();
            TorchMeterAnimCheck();
            EnablePlayerInputCheck();
            TutorialDialogueCheck();
        }
    }

    // Checks if the player is on a bridge tile - returns true if so, false otherwise
    public bool OnBridge()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;
        //Debug.DrawRay(myRay.origin, myRay.direction, Color.yellow);

        if (Physics.Raycast(myRay, out hit, rayLength))
        {
            currentTile = hit.collider.gameObject;
            string tag = currentTile.tag;

            if (tag == "BridgeTile")
            {
                bridge = currentTile.transform.parent.gameObject;
                bridgeNumber = ConvertObjectNameToNumber(bridge);
                return true;
            }
        }

        return false;
    }

    // Checks if there is an edge in front of the player - returns true if so, false otherwise
    private bool EdgeCheck()
    {
        Ray myEdgeRay = new Ray(transform.position + new Vector3(0, -0.1f, 0), transform.TransformDirection(Vector3.forward));
        RaycastHit hit;
        Debug.DrawRay(myEdgeRay.origin, myEdgeRay.direction, Color.red);

        if (Physics.Raycast(myEdgeRay, out hit, rayLength))
        {
            string tag = hit.collider.tag;

            // The player cannot walk over holes - holes count as an "edge"
            if (tag != "EmptyBlock")
                return false;
        }

        return true;
    }

    // Determines if the player is on a checkpoint tile - returns true if so, false otherwise
    public bool OnCheckpoint()
    {
        if (!OnBridge())
        {
            string tag = currentTile.tag;

            // Checks if the checkpoint has been set
            if (!hasSetCheckpoint)
            {
                if (tag == "Checkpoint")
                    checkpoint = currentTile;
                else
                {
                    // If the first tile the player lands on is not a checkpoint, find it (called when moving to a previous puzzle)...
                    for (int i = 0; i < currentTile.transform.parent.childCount; i++)
                    {
                        GameObject child = currentTile.transform.parent.GetChild(i).gameObject;

                        if (child.tag == "Checkpoint")
                            checkpoint = child;
                    }
                }

                puzzle = currentTile.transform.parent.parent.gameObject;
                puzzleNumber = ConvertObjectNameToNumber(puzzle);
                CheckpointManager checkpointManagerScript = checkpoint.GetComponent<CheckpointManager>();

                torchMeterScript.MaxVal = checkpointManagerScript.GetNumMovements(); // Sets the new max value for the torch meter
                torchMeterScript.ResetTorchMeterElements();

                puzzleManagerScript.UpdateParentObjects();
                checkpointManagerScript.LastBridgeTileCheck(); // Note: the player's rotation is saved in this method
                saveManagerScript.SavePlayerPosition(checkpoint);
                saveManagerScript.SaveCameraPosition();

                bridgeTileCount = 0;
                previousTile = null;
                hasMovedPuzzleView = false;
                hasSetCheckpoint = true;
            }
            
            if (tag == "Checkpoint")
                return true;
        }
        return false;
    }

    // Checks if the player is on the last tile of the puzzle - returns true if so, false otherwise
    public bool OnLastTileBlock()
    {
        for (int i = 0; i < 360; i += 90)
        {
            tileCheck.transform.localEulerAngles = new Vector3(0, i, 0);

            Ray myRay = new Ray(tileCheck.transform.position + new Vector3(0, -0.1f, 0), tileCheck.transform.TransformDirection(Vector3.forward));
            RaycastHit hit;
            //Debug.DrawRay(myRay.origin, myRay.direction, Color.blue);

            if (Physics.Raycast(myRay, out hit, rayLength) && !OnCheckpoint())
            {
                string tag = hit.collider.tag;

                if (tag == "BridgeTile")
                {
                    //Debug.Log("You are on the last tile");
                    return true;
                }
            }
        }

        //Debug.Log("You are NOT on the last tile");
        return false;
    }

    // Checks if the player is on the first or final bridge in the zone
    private bool OnFirstOrLastBridge()
    {
        if (OnBridge())
        {
            string bridgeName = bridge.name;

            // Note: make sure the first bridge is called "EntryBridge" the final bridge is called "EndBridge"
            if (bridgeName == "EndBridge" || bridgeName == "EntryBridge")
                return true;
        }

        return false;
    }

    // Determines the next bridge tile to move towards - Returns true if found, false otherwise
    private bool NextBridgeTileCheck()
    {
        // Looks for the next bridge tile and sets it as the destination
        if (transform.position == currentTile.transform.position && previousTile != currentTile && bridgeTileCount != bridge.transform.childCount) // Note: doesn't check for bridge tiles during intro
        {
            bridgeTileCount++;

            for (int i = 0; i < 360; i += 90)
            {
                bridgeTileCheck.transform.localEulerAngles = new Vector3(0, i, 0);

                Ray myBridgeRay = new Ray(bridgeTileCheck.transform.position + new Vector3(0, -0.1f, 0), bridgeTileCheck.transform.TransformDirection(Vector3.forward));
                RaycastHit hit02;
                Debug.DrawRay(myBridgeRay.origin, myBridgeRay.direction, Color.red);

                if (Physics.Raycast(myBridgeRay, out hit02, rayLength))
                {
                    GameObject nextBridgeTile = hit02.collider.gameObject;

                    if (nextBridgeTile != previousTile)
                    {
                        //Debug.Log("Next Bridge Tile Found");
                        transform.eulerAngles = new Vector3(0, bridgeTileCheck.transform.eulerAngles.y, 0);
                        destination = nextBridgeTile.transform.position;
                        StartPlayerCoroutines();

                        NextPuzzleViewCheck();
                        SetPlayerBoolsFalse();
                        previousTile = currentTile;
                        hasSetCheckpoint = false;
                        return true;
                    }
                }
            }
            //Debug.Log("Next Bridge Tile NOT Found");

            // If the bridge abruptly ends and no bridge tile is found...
            // Stops the player's walking animation
            if (bridge.name == "EndBridge")
                playerAnimator.SetTrigger("Idle");
            // Otherwise, move back to the previous puzzle
            else
            {
                bridgeTileCount = 0;
                previousTile = null;
                NextBridgeTileCheck();
            }
        }

        return false;
    }

    // Checks when to set the alert bubble active/inactive
    public bool AlertBubbleCheck()
    {
        Collider collider = GetCollider();

        if (collider != null && torchMeterScript.CurrentVal != 0f) // Note: Alert bubble doesn't pop up when frozen
        {
            string tag = collider.tag;

            if (tag == "NPC")
            {
                characterDialogueScript.SetAlertBubbleActive();
                return true;
            }
            else if (tag == "Artifact")
            {
                Artifact artifactScript = collider.GetComponent<Artifact>();

                // If the player hasn't collected the artifact within an artifact chest
                if (!artifactScript.HasCollectedArtifact)
                {
                    characterDialogueScript.SetAlertBubbleActive();
                    return true;
                }
            }
        }

        characterDialogueScript.SetAlertBubbleInactive();
        return false;
    }

    // Checks when the torch meter can pop in/out
    public void TorchMeterAnimCheck()
    {
        if (OnBridge() && !hasAlreadyPopedOut) // Torch meter pops out when the player is on a bridge tile
        {
            torchMeterScript.TorchMeterPopOut();
            hasAlreadyPopedOut = true;
        }
        else if (!OnBridge() && hasAlreadyPopedOut) // Torch meter pops in when the player is not on a bridge tile
        {
            torchMeterScript.TorchMeterPopIn();
            hasAlreadyPopedOut = false;
        }
    }

    // Subracts a value from the torch meter's current total
    private void SubractFromTorchMeter(int valueToSubract)
    {
        float newTorchMeterVal = torchMeterScript.CurrentVal - valueToSubract;

        if (!OnBridge())
            torchMeterScript.CurrentVal = newTorchMeterVal;
    }

    // Converts the name of an object to an integer
    private int ConvertObjectNameToNumber(GameObject theObject)
    {
        // If the object has a number in its name
        if (theObject.name.Any(char.IsDigit))
        {
            string newObjectName = Regex.Replace(theObject.name, "[A-Za-z ]", "");
            return int.Parse(newObjectName);
        }

        //Debug.Log("Object does NOT have a number in its name");
        return 0;
    }

    // Plays a new animation state
    public void ChangeAnimationState(string newState)
    {
        switch (newState)
        {
            case ("Interacting"):
                audioManagerScript.PlaySwooshSFX();
                playerAnimator.SetTrigger("Interacting");
                break;
            case ("Pushing"):
                playerAnimator.SetTrigger("Pushing");
                break;
            default:
                playerAnimator.Play(newState);
                break;
        }

        playerFidgetScript.SetIdleCountToZero();
    }

    // Checks if the camera can move to the next puzzle view
    private void NextPuzzleViewCheck()
    {
        // The camera will not lerp if the player is on the first/last bridge
        if (!OnFirstOrLastBridge() && !hasMovedPuzzleView)
        {
            //Debug.Log("Next Puzzle View Activated");
            if (bridgeNumber != puzzleNumber)
                cameraScript.PreviousPuzzleView();
            else
                cameraScript.NextPuzzleView();

            hasMovedPuzzleView = true;
        }
    }

    // Checks for the methods to call depending on the tile the player landed/is on
    private void EnablePlayerInputCheck()
    {
        // Checks if the player can recieve input
        if (!OnBridge())
        {
            playerAnimator.SetTrigger("Idle");
            SetPlayerBoolsTrue();
        }
        else if (OnBridge())
        {
            NextBridgeTileCheck();

            // Checks if the player has completed the zone - by stepping on the final bridge
            if (bridge.name == "EndBridge" && !hasFinishedZone)
            {
                SetFinishedZoneElements();
                audioManagerScript.PlayChimeSFX();
                hasFinishedZone = true;
            }
        }
    }

    // Calls the appropriate methods before transitioning to the next zone
    public void SetFinishedZoneElements()
    {
        // Plays the end credits after completing the final zone
        if (SceneManager.GetActiveScene().name == "FifthMap")
            endCreditsScript.StartEndCredits();

        gameManagerScript.ResetCollectedArtifactsCheck();
        transitionFadeScript.GameFadeOut();
        SetPlayerBoolsFalse();
    }

    // Checks which footstep sfx to play - determined by the tag/name of the tile the player is on
    private void PlayerFootstepSFXCheck()
    {
        // Doesnt play the footstep sfx after crossing a certain amount of tiles on the final bridge
        if (hasFinishedZone && bridgeTileCount >= 8)
            return;
        else
        {
            Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
            RaycastHit hit;
            //Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

            if (Physics.Raycast(myRay, out hit, rayLength))
            {
                string tag = hit.collider.tag;
                string name = hit.collider.name;

                // Footstep sfx for grass tiles
                if (tag == "GrassTile" || name == "Checkpoint_GrassTile")
                    audioManagerScript.PlayGrassFootstepSFX();

                // Footstep sfx for snow tiles
                else if (tag == "SnowTile" || name == "Checkpoint_SnowTile" || name == "Checkpoint_SnowTile02")
                    audioManagerScript.PlaySnowFootstepSFX();

                // Footstep sfx for stone tiles
                else if (tag == "StoneTile" || name == "Checkpoint_StoneTile")
                    audioManagerScript.PlayStoneFootstepSFX();

                // Footstep sfx for metal tiles
                else if (tag == "MetalTile" || name == "Checkpoint_MetalTile")
                    audioManagerScript.PlayMetalFootstepSFX();

                // Footstep sfx for wood (bridge tiles)
                else if (tag == "BridgeTile" || name == "BridgeTile")
                    audioManagerScript.PlayWoodFootstepSFX();

                // Footstep sfx for crates (wooden crates)
                else if (tag == "PushableBlock")
                    audioManagerScript.PlayCrateFootstepSFX();
            }
        }
    }

    // Checks which tutorial dialogue to play - for tutorial zone ONLY
    private void TutorialDialogueCheck()
    {
        if (tutorialDialogueScript != null && !transitionFadeScript.IsChangingScenes && torchMeterScript.CurrentVal > 0 && canMove)
        {
            Collider collider = GetCollider();

            if (!OnBridge() && OnLastTileBlock())
                tutorialDialogueScript.PlayBridgeDialogueCheck();

            if (collider != null)
            {
                switch (collider.tag)
                {
                    case ("PushableBlock"):
                        tutorialDialogueScript.PlayPushDialogueCheck();
                        break;
                    default:
                        //Debug.Log("Unrecognizable tag");
                        break;
                }
            }

            switch (puzzleNumber)
            {
                case (1): // Puzzle 1
                    tutorialDialogueScript.PlayWelcomeDialogueCheck();
                    break;
                case (2): // Puzzle 2
                    tutorialDialogueScript.PlayBreakDialogueCheck();
                    break;
                case (3): // Puzzle 3
                    tutorialDialogueScript.PlayHoleDialogueCheck();
                    break;
                case (4): // Puzzle 4
                    tutorialDialogueScript.PlayFirstoneDialogueCheck();
                    break;
                case (5): // Puzzle 5
                    tutorialDialogueScript.PlayInteractablesDialogueCheck();
                    break;
                case (6): // Puzzle 6
                    tutorialDialogueScript.PlayFinishedTutorialDialogueCheck();
                    break;
                default: // Any other puzzle
                    //Debug.Log("Unrecognizable tutorial puzzle");
                    break;
            }
        }
    }

    // Checks how to move the player's position (lerps over a duration, or instantly)
    private void MoveToNextPosition()
    {
        gameManagerScript.CheckForPlayerScriptDebug();

        // Lerps the player's position only if there's a duration
        if (lerpLength > 0)
        {
            // Only move if at destination and not on bridge
            if (canMove)
            {
                StartPlayerCoroutines();
                canInteract = false;
                canMove = false;
            }
        }
        // Moves the player's position instantly...
        else
            transform.position = destination;
    }

    // Sets the player's position to the grass material's vector position
    public void WriteToGrassMaterial()
    {
        if (grassMatPosition != transform.position)
        {
            grassMatPosition = transform.position;
            grassMat.SetVector("_position", grassMatPosition);
        }
    }

    // Stops the player's movement and footstep coroutines
    public void StopPlayerCoroutines()
    {
        if (playerMovementCoroutine != null)
            StopCoroutine(playerMovementCoroutine);

        if (playerFootstepsCoroutine != null)
            StopCoroutine(playerFootstepsCoroutine);
    }

    // Starts/overrides the player's movement and footstep coroutines
    private void StartPlayerCoroutines()
    {
        if (playerMovementCoroutine != null)
            StopCoroutine(playerMovementCoroutine);

        playerMovementCoroutine = MovePlayerPosition(destination, lerpLength);
        StartCoroutine(playerMovementCoroutine);

        if (playerFootstepsCoroutine != null)
            StopCoroutine(playerFootstepsCoroutine);

        playerFootstepsCoroutine = PlayerFootsteps(lerpLength);
        StartCoroutine(playerFootstepsCoroutine);
    }

    // Moves the player's position to a new position over a duration (endPosition = position to move to, duration = seconds)
    private IEnumerator MovePlayerPosition(Vector3 endPosition, float duration)
    {
        float time = 0f;
        Vector3 startPosition = transform.position;

        while (time < duration)
        {
            if (!playerFidgetScript.IsPlayingAnimation("Walking"))
                ChangeAnimationState("Walking");

            WriteToGrassMaterial();
            transform.position = Vector3.MoveTowards(startPosition, endPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        CurrentTileCheck();
    }

    // Plays sfx for the player's footsteps - determined by lerpLength
    // Note: lerpLength is the time it takes for the player to move from its current position to its destination
    private IEnumerator PlayerFootsteps(float duration)
    {
        float fourthOfDuration = duration / 4f;

        // Plays a footstep sfx when the player has traveled for 1/4th of the lerpLength
        yield return new WaitForSeconds(fourthOfDuration);
        PlayerFootstepSFXCheck();

        // Plays a footstep sfx when the player has traveled for 3/4th of the lerpLength 
        yield return new WaitForSeconds(fourthOfDuration * 2f);
        if (!OnFirstOrLastBridge())
            PlayerFootstepSFXCheck();
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        torchMeterScript = FindObjectOfType<TorchMeter>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        cameraScript = FindObjectOfType<CameraController>();
        saveManagerScript = FindObjectOfType<SaveManager>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        gameManagerScript = FindObjectOfType<GameManager>();
        objectShakeScript = FindObjectOfType<ObjectShakeController>();
        puzzleManagerScript = FindObjectOfType<PuzzleManager>();
        transitionFadeScript = FindObjectOfType<TransitionFade>();
        endCreditsScript = FindObjectOfType<EndCredits>();
        playerFidgetScript = GetComponentInChildren<FidgetController>();
        tutorialDialogueScript = (SceneManager.GetActiveScene().name == "TutorialMap") ? FindObjectOfType<TutorialDialogue>() : null;
    }

    // Sets private variables, objects, and components
    private void SetElemenets()
    {
        // Sets them by looking at the names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            string childName = child.name;

            if (childName == "PlayerModel")
                playerAnimator = child.GetComponent<Animator>();
            if (childName == "TileCheck")
                tileCheck = child;
            if (childName == "BridgeTileCheck")
                bridgeTileCheck = child;
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
        grassMat = gameManagerScript.grassMaterial;
    }

}
