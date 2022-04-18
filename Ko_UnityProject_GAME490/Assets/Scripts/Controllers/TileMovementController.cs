using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TileMovementController : MonoBehaviour
{
    [Header("Bools")]
    [SerializeField]
    private bool canSetBoolsTrue = true;
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
    private string objectToShakeName;

    [Header("Vectors")]
    private Vector3 nextPos;
    private Vector3 destination;
    private Vector3 currentDirection;

    Vector3 up = Vector3.zero, // Look North
    right = new Vector3(0, 90, 0), // Look East
    down = new Vector3(0, 180, 0), // Look South
    left = new Vector3(0, 270, 0); // Look West

    [Header("Current Puzzle/Checkpoint/Bridge")]
    private GameObject checkpoint;
    private GameObject puzzle;
    private GameObject bridge;

    private GameObject currentTile;
    private GameObject previousTile;

    private GameObject bridgeTileCheck;
    private GameObject tileCheck;
    private GameObject dialogueViewsHolder;
    private GameObject objectToShake;
    private GameObject destroyedRockParticle;
    private Animator playerAnimator;

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
    }

    // Returns or sets the current puzzle
    public GameObject CurrentPuzzle
    {
        get { return puzzle; }
        set { puzzle = value; }
    }

    // Returns or sets the current checkpoint
    public GameObject CurrentCheckpoint
    {
        get { return checkpoint; }
        set { checkpoint = value; }
    }

    // Returns the value of puzzleNumber
    public int PuzzleNumber
    {
        get { return puzzleNumber; }
        set { puzzleNumber = value; }
    }

    // Returns or sets the value of LerpLength
    public float LerpLength
    {
        get { return lerpLength; }
        set { lerpLength = value; }
    }

    // Returns or sets the value of resetPuzzleDelay
    public float ResetPuzzleDelay
    {
        get { return resetPuzzleDelay; }
        set { resetPuzzleDelay = value; }
    }

    // Returns or sets the value of hasFinishedZone
    public bool HasFinishedZone
    {
        get { return hasFinishedZone; }
        set { hasFinishedZone = value; }
    }

    // Returns the value of canMove
    public bool CanMove
    {
        get { return canMove; }
    }

    // Returns the value of canRestartPuzzle
    public bool CanRestartPuzzle
    {
        get { return canRestartPuzzle; }
    }

    // Sets the value of canSetBoolsTrue
    public bool CanSetBoolsTrue
    {
        set { canSetBoolsTrue = value; }
    }

    // Sets the destination - allows the destination to be changed by other scripts
    public void setDestination(Vector3 newDestination)
    {
        destination = newDestination;
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
            /*** Movement input ENDS here ***/

            // Reset puzzle
            if (Input.GetKeyDown(KeyCode.R) && canRestartPuzzle)
                puzzleManagerScript.ResetPuzzle(0f);

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
                            interactWithNPC(collider);
                            break;
                        case ("Artifact"):
                            interactWithArtifact(collider);
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

        // Returns false if there's no collider (no obstacles)
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
                    SubractFromTorchMeter(1);
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

    // Sets the current tile and bridge the player is on
    public void CurrentTileCheck()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.cyan);

        if (Physics.Raycast(myRay, out hit, rayLength))
        {
            currentTile = hit.collider.gameObject;
            TorchMeterAnimCheck();
            AlertBubbleCheck();
            OnCheckpoint();

            if (OnBridge())
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
        if (!OnBridge() && !hasSetCheckpoint)
        {
            string tag = currentTile.tag;

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
            ConvertObjectNameToNumber(puzzle, "Puzzle", ref puzzleNumber);
            CheckpointManager checkpointManagerScript = checkpoint.GetComponent<CheckpointManager>();

            torchMeterScript.MaxVal = checkpointManagerScript.GetNumMovements(); // Sets the new max value for the torch meter
            ResetTorchMeter();

            puzzleManagerScript.UpdateParentObjects();
            checkpointManagerScript.LastBridgeTileCheck(); // Note: the player's rotation is saved in this method
            saveManagerScript.SavePlayerPosition(checkpoint);
            saveManagerScript.SaveCameraPosition();

            bridgeTileCount = 0;
            previousTile = null;
            hasMovedPuzzleView = false;
            canSetBoolsTrue = true;
            hasSetCheckpoint = true;
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
                        StartMovementCoroutine();

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

    // Resets the current value of the torch meter to its maximum value
    public void ResetTorchMeter()
    {
        torchMeterScript.CurrentVal = torchMeterScript.MaxVal;
        torchMeterScript.ResetTorchMeterElements();
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

    public void SetFinishedZoneElements()
    {
        // Plays the end credits after completing the final zone
        if (SceneManager.GetActiveScene().name == "FifthMap")
            endCreditsScript.StartEndCredits();

        gameManagerScript.ResetCollectedArtifactsCheck();
        transitionFadeScript.GameFadeOut();
        SetPlayerBoolsFalse();
        canSetBoolsTrue = false;
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
        if (!OnFirstOrLastBridge() && !hasMovedPuzzleView)
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
        {
            audioManagerScript.PlaySwooshSFX();
            playerAnimator.SetTrigger("Interacting");
        }
        else if (newState == "Pushing")
            playerAnimator.SetTrigger("Pushing");
        else
            playerAnimator.Play(newState);

        playerFidgetScript.SetIdleIndexToZero();
    }

    // Checks which footstep sfx to play for the player - determined by the tag/name of the tile the player walks on
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
        CurrentTileCheck();

        if (!OnBridge())
        {
            playerAnimator.SetTrigger("Idle");

            if (canSetBoolsTrue)
            {
                if (!canMove)
                    canMove = true;

                if (!canRestartPuzzle)
                    canRestartPuzzle = true;

                if (!canInteract)
                    canInteract = true;
            }
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
        if (!OnFirstOrLastBridge())
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
    }

}
