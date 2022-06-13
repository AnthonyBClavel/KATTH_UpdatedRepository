using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Linq;

public class TileMovementController : MonoBehaviour
{
    private int bridgeTileCount;

    private float lerpDuration = 0.2f; // Original Value = 0.2f
    private float resetPuzzleDelay = 1.5f; // Orginal Value = 1.5f
    private float rayLength = 1f;

    [Header("Bools")]
    [SerializeField]
    private bool canMove = true;
    [SerializeField]
    private bool canInteract = true;
    [SerializeField]
    private bool canRestartPuzzle = true;
    private bool hasFinishedZone = false;
    private bool hasAlreadyPopedOut = false;
    private bool hasSetCheckpoint = false;

    [Header("Current Checkpoint/Bridge")]
    private GameObject checkpoint;
    private GameObject bridge;

    private GameObject currentTile;
    private GameObject previousTile;
    private GameObject bridgeTileCheck;
    private GameObject tileCheck;
    private GameObject dialogueViewsHolder;
    private GameObject alertBubble;
    private Animator playerAnimator;

    private Vector3 nextPos;
    private Vector3 destination;

    Vector3 north = Vector3.zero,
    east = new Vector3(0, 90, 0),
    south = new Vector3(0, 180, 0),
    west = new Vector3(0, 270, 0);

    [Header("Variables for Grass Material")]
    private Vector3 grassMatPosition;
    private Material grassMat;

    private IEnumerator playerFootstepsCoroutine;
    private IEnumerator playerMovementCoroutine;

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

    public GameObject AlertBubble
    {
        get { return alertBubble; }
        set { alertBubble = value; }
    }

    public Vector3 Destination
    {
        get { return destination; }
        set { destination = value; }
    }

    public float LerpDuration
    {
        get { return lerpDuration; }
        set { lerpDuration = value; }
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
        WriteToGrassMaterial();
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
                    transform.eulerAngles = north;
                    return true;
                }

                // Move South
                else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    nextPos = Vector3.back;
                    transform.eulerAngles = south;
                    return true;
                }

                // Move East
                else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    nextPos = Vector3.right;
                    transform.eulerAngles = east;
                    return true;
                }

                // Move West
                else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    nextPos = Vector3.left;
                    transform.eulerAngles = west;
                    return true;
                }
            }
            /*** Movement input ENDS here ***/

            // Reset puzzle
            if (Input.GetKeyDown(KeyCode.R) && canRestartPuzzle)
            {
                puzzleManagerScript.ResetPuzzle();
                playerFidgetScript.SetIdleCountToZero();
                objectShakeScript.StopAllShakingObjects();
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

    // Checks to shake the object
    private void ShakeObjectCheck(Collider collider)
    {
        // The object can shake ONLY if it has a child/children
        if (collider.transform.childCount == 0) return;

        if (collider.tag == "StaticBlock" || collider.tag == "DestroyableBlock")
        {
            objectShakeScript.ShakeObject(collider.transform.GetChild(0).gameObject);
            ChangeAnimationState("Pushing");
        }
    }

    // Checks to push the object
    private void PushBlockCheck(Collider collider)
    {
        // The object can be pushed ONLY if it has this tag...
        if (collider.tag != "PushableBlock") return;

        BlockMovementController blockMovementScript = collider.GetComponent<BlockMovementController>();
        gameManagerScript.CheckForBlockMovementDebug(blockMovementScript);
        if (blockMovementScript.MoveBlock()) SubractFromTorchMeter(1);

        ChangeAnimationState("Pushing");
    }

    // Sets the object inactive
    private void DestroyBlock(Collider collider)
    {
        GameObject destroyableBlock = collider.gameObject;

        puzzleManagerScript.InstantiateParticleEffect(destroyableBlock, "DestroyedRockParticle", -0.25f);
        audioManagerScript.PlayRockBreakSFX();
        destroyableBlock.SetActive(false);
        SubractFromTorchMeter(1);

        ChangeAnimationState("Interacting");
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

        if (!generatorScript.IsGenActive)
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
        NonPlayerCharacter nPCScript = collider.GetComponent<NonPlayerCharacter>();
        NonPlayerCharacter_SO nPC = nPCScript.nonPlayerCharacter;

        if (nPCScript.enabled)
        {        
            dialogueViewsHolder.transform.position = collider.transform.position;
            alertBubble.SetActive(false);

            characterDialogueScript.StartNPCDialogue(nPCScript, nPC);
            //Debug.Log("Interacted with NPC");
        }
    }

    // Calls the methods to begin/trigger the Artifact dialogue
    private void InteractWithArtifact(Collider collider)
    {
        Artifact artifactScript = collider.GetComponent<Artifact>();
        Artifact_SO artifact = artifactScript.artifact;

        // If the artifact script and the artifact object itself are both active
        if (artifactScript.enabled && artifactScript.ArtifactHolder.activeSelf)
        {
            dialogueViewsHolder.transform.position = collider.transform.position;
            alertBubble.SetActive(false);

            characterDialogueScript.StartArtifactDialogue(artifactScript, artifact);
            artifactScript.OpenChest();

            ChangeAnimationState("Interacting");
            //Debug.Log("Interacted with Artifact");
        }
    }

    // Checks to move the player
    public void MovePlayerCheck()
    {
        if (!OnTile()) return;

        OnCheckpoint();
        AlertBubbleCheck();
        TorchMeterAnimCheck();
        EnablePlayerInputCheck();
        TutorialDialogueCheck();
    }

    // Returns the collider of the object in front of the player 
    private Collider GetCollider()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), transform.TransformDirection(Vector3.forward));
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        if (Physics.Raycast(myRay, out hit, rayLength))
            return hit.collider;

        // If the ray doesn't hit anything, then there's no collider
        return null;
    }

    // Checks for colliders - return true if so, false otherwise
    private bool ColliderCheck()
    {
        Collider collider = GetCollider();      
        if (collider == null) return false;

        PushBlockCheck(collider);
        ShakeObjectCheck(collider);

        return true;
    }

    // Checks if there's an edge at the next position - returns true if so, false otherwise
    private bool EdgeCheck()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, -0.1f, 0), transform.TransformDirection(Vector3.forward));
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        if (Physics.Raycast(myRay, out hit, rayLength))
        {
            // The player cannot walk over holes - counts as an "edge"
            if (hit.collider.tag != "EmptyBlock")
                return false;
        }

        // If the ray doesn't hit anything, then there's an edge
        return true;
    }

    // Checks if the player is on a tile - sets the current tile and returns true if so, false otherwise
    public bool OnTile()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.cyan);

        if (Physics.Raycast(myRay, out hit, rayLength))
        {
            currentTile = hit.collider.gameObject;
            return true;
        }

        return false;
    }

    // Checks if the player is on a bridge - sets the current bridge and returns true if so, false otherwise
    public bool OnBridge()
    {
        if (!OnTile()) return false;

        if (currentTile.tag == "BridgeTile" || currentTile.name == "BridgeTile")
        {
            bridge = currentTile.transform.parent.gameObject;
            puzzleManagerScript.SetCurrentBridge(bridge);
            return true;
        }

        return false;
    }

    // Checks if the player is on a checkpoint - returns true if so, false otherwise
    public bool OnCheckpoint()
    {
        if (OnBridge()) return false;

        // Checks if the checkpoint has been set
        if (!hasSetCheckpoint)
        {
            if (currentTile.tag == "Checkpoint")
                checkpoint = currentTile;
            else
            {
                // Finds the checkpoint if it wasn't the first tile the player landed on
                foreach (Transform child in currentTile.transform.parent)
                {
                    if (child.tag == "Checkpoint")
                    {
                        checkpoint = child.gameObject;
                        break;
                    }
                }
            }

            puzzleManagerScript.SetCurrentCheckpoint(checkpoint);
            saveManagerScript.SavePlayerPosition(checkpoint);
            saveManagerScript.SaveCameraPosition();
          
            previousTile = null;
            bridgeTileCount = 0;
            hasSetCheckpoint = true;
        }

        if (currentTile.tag == "Checkpoint")
            return true;

        return false;
    }

    // Checks if the player is on the last tile of a puzzle - returns true if so, false otherwise
    private bool OnLastTileBlock()
    {
        for (int i = 0; i < 360; i += 90)
        {
            tileCheck.transform.localEulerAngles = new Vector3(0, i, 0);
            Ray myRay = new Ray(tileCheck.transform.position + new Vector3(0, -0.1f, 0), tileCheck.transform.TransformDirection(Vector3.forward));
            RaycastHit hit;
            //Debug.DrawRay(myRay.origin, myRay.direction, Color.blue);

            if (Physics.Raycast(myRay, out hit, rayLength) && !OnCheckpoint())
            {
                if (hit.collider.tag == "BridgeTile" || hit.collider.name == "BridgeTile")
                    return true;
            }
        }

        return false;
    }

    // Determines the next bridge tile to move towards - returns true if found, false otherwise
    private bool NextBridgeTileCheck()
    {
        // Looks for the next bridge tile and sets it as the destination
        if (transform.position == currentTile.transform.position && previousTile != currentTile && bridgeTileCount != bridge.transform.childCount) // Note: doesn't check for bridge tiles during intro
        {
            bridgeTileCount++;

            for (int i = 0; i < 360; i += 90)
            {
                bridgeTileCheck.transform.localEulerAngles = new Vector3(0, i, 0);

                Ray myRay = new Ray(bridgeTileCheck.transform.position + new Vector3(0, -0.1f, 0), bridgeTileCheck.transform.TransformDirection(Vector3.forward));
                RaycastHit hit;
                Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

                if (Physics.Raycast(myRay, out hit, rayLength))
                {
                    GameObject nextBridgeTile = hit.collider.gameObject;

                    if (nextBridgeTile != previousTile)
                    {            
                        transform.eulerAngles = new Vector3(0, bridgeTileCheck.transform.eulerAngles.y, 0);
                        destination = nextBridgeTile.transform.position;
                        previousTile = currentTile;

                        puzzleManagerScript.MoveCameraToNextPuzzle();
                        StartPlayerCoroutines();
                        SetPlayerBoolsFalse();

                        hasSetCheckpoint = false;
                        return true;
                        //Debug.Log("The next bridge tile WAS found!");
                    }
                }
            }
            //Debug.Log("The next bridge tile was NOT found!");

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
                alertBubble.SetActive(true);
                characterDialogueScript.SetAlertBubbleColor();
                return true;
            }
            else if (tag == "Artifact")
            {
                Artifact artifactScript = collider.GetComponent<Artifact>();

                // If the player hasn't collected the artifact within an artifact chest
                if (artifactScript.enabled)
                {
                    alertBubble.SetActive(true);
                    characterDialogueScript.SetAlertBubbleColor();
                    return true;
                }
            }
        }

        alertBubble.SetActive(false);
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

            switch (puzzleManagerScript.PuzzleNumber)
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
        if (lerpDuration > 0)
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

        playerMovementCoroutine = MovePlayerPosition(destination, lerpDuration);
        StartCoroutine(playerMovementCoroutine);

        if (playerFootstepsCoroutine != null)
            StopCoroutine(playerFootstepsCoroutine);

        playerFootstepsCoroutine = PlayerFootsteps(lerpDuration);
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
        MovePlayerCheck();
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
        if (puzzleManagerScript.BridgeNumber != null)
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

            if (child.name == "PlayerModel")
                playerAnimator = child.GetComponent<Animator>();
            if (child.name == "TileCheck")
                tileCheck = child;
            if (child.name == "BridgeTileCheck")
                bridgeTileCheck = child;
        }

        for (int i = 0; i < cameraScript.transform.parent.childCount; i++)
        {
            GameObject child = cameraScript.transform.parent.GetChild(i).gameObject;

            if (child.name == "DialogueViewsHolder")
                dialogueViewsHolder = child;
        }

        nextPos = Vector3.forward;
        destination = transform.position;

        lerpDuration = gameManagerScript.playerLerpDuration;
        resetPuzzleDelay = gameManagerScript.resetPuzzleDelay;
        grassMat = gameManagerScript.grassMaterial;
    }

}
