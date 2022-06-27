using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TileMovementController : MonoBehaviour
{
    private int bridgeTileCount;

    private float lerpDuration = 0.2f; // Original Value = 0.2f
    private float resetPuzzleDelay = 1.5f; // Orginal Value = 1.5f
    private float rayLength = 1f;

    private bool canMove = true;
    private bool canInteract = true;
    private bool canRestartPuzzle = true;
    private bool hasFinishedZone = false;
    private bool hasSetCheckpoint = false;
    private bool hasPopedOutTM = false;

    private GameObject checkpoint; // Current checkpoint
    private GameObject bridge; // Current bridge
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

    private Vector3 grassMatPosition;
    private Material grassMat;

    private IEnumerator playerFootstepsCoroutine;
    private IEnumerator playerMovementCoroutine;

    private TorchMeter torchMeterScript;
    private SaveManager saveManagerScript;

    private EndCredits endCreditsScript;
    private CameraController cameraScript;
    private GameManager gameManagerScript;
    private AudioManager audioManagerScript;
    private PuzzleManager puzzleManagerScript;
    private TransitionFade transitionFadeScript;
    private FidgetController playerFidgetScript;
    private ObjectShakeController objectShakeScript;
    private TutorialDialogue tutorialDialogueScript;
    private CharacterDialogue characterDialogueScript;

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
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Note: The destination is set in Start() since the player's position loads/changes in awake
        destination = transform.position;
        nextPos = Vector3.forward;
    }

    // FixedUpdate is called at a fixed interval (independent from the game's framerate)
    // Update is called once per frame
    void Update()
    {
        PlayerInput();
    }

    // Sets all input-related bools to false
    public void SetPlayerBoolsFalse()
    {
        canMove = false;
        canRestartPuzzle = false;
        canInteract = false;
    }

    // Sets all input-related bools to true
    public void SetPlayerBoolsTrue()
    {
        canMove = true;
        canRestartPuzzle = true;
        canInteract = true;
    }

    // Checks for the player input
    private void PlayerInput()
    {
        // No input is recieved while the player is on a bridge or while exiting a scene
        if (OnBridge() || FreezePlayer() || transitionFadeScript.IsChangingScenes) return;

        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
            Interact();

        else if (Input.GetKeyDown(KeyCode.R))
            ResetPuzzle();

        else if (MovementInput())
            MovePlayer();
    }

    // Checks for the input to move the player
    private bool MovementInput()
    {
        if (!canMove) return false;
        bool recievedInput = false;

        // Move North
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            nextPos = Vector3.forward;
            transform.eulerAngles = north;
            recievedInput = true;
        }

        // Move South
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            nextPos = Vector3.back;
            transform.eulerAngles = south;
            recievedInput = true;
        }

        // Move East
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            nextPos = Vector3.right;
            transform.eulerAngles = east;
            recievedInput = true;
        }

        // Move West
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            nextPos = Vector3.left;
            transform.eulerAngles = west;
            recievedInput = true;
        }

        return recievedInput;
    }

    // Checks to restart the current puzzle
    private void ResetPuzzle()
    {
        if (!canRestartPuzzle) return;

        puzzleManagerScript.ResetPuzzle();
        playerFidgetScript.SetIdleCountToZero();
        objectShakeScript.StopAllShakingObjects();
    }

    // Checks to interact with the object in front of the player
    private void Interact()
    {
        Collider collider = GetCollider();
        if (collider == null || torchMeterScript.CurrentVal <= 0 || !canInteract) return;

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

    // Checks to move the player to the next position
    private void MovePlayer()
    {
        playerFidgetScript.SetIdleCountToZero();
        AlertBubbleCheck();

        // The player doesn't move if there's a collider or an edge
        if (ColliderCheck() || EdgeCheck()) return;

        canMove = false;
        canInteract = false;

        gameManagerScript.CheckForPlayerScriptDebug();
        SubractFromTorchMeter();
  
        destination = transform.position + nextPos;
        previousTile = currentTile;
        StartPlayerCoroutines();
    }

    // Checks to freeze the player - to restart the puzzle after a delay
    private bool FreezePlayer()
    {
        // Returns false if the the player is NOT within 0.00001f units of its destination
        if (Vector3.Distance(destination, transform.position) > 0.00001f || torchMeterScript.CurrentVal > 0 || !canMove) return false;

        // Note: the player bools are set to false within the method below
        puzzleManagerScript.ResetPuzzle(resetPuzzleDelay);
        audioManagerScript.PlayExtinguishFireSFX();
        audioManagerScript.PlayFreezeingSFX();
        return true;
    }

    // Checks to shake the object
    private void ShakeObjectCheck(Collider collider)
    {
        if (collider.transform.childCount == 0 || !collider.CompareTag("StaticBlock") && !collider.CompareTag("DestroyableBlock")) return;

        objectShakeScript.ShakeObject(collider.transform.GetChild(0).gameObject);
        ChangeAnimationState("Pushing");
    }

    // Checks to push the object
    private void PushBlockCheck(Collider collider)
    {
        if (!collider.CompareTag("PushableBlock")) return;

        BlockMovementController blockMovementScript = collider.GetComponent<BlockMovementController>();
        gameManagerScript.CheckForBlockMovementDebug(blockMovementScript);
        if (blockMovementScript.MoveBlock()) SubractFromTorchMeter();

        ChangeAnimationState("Pushing");
        //Debug.Log("Interacted with PUSHABLE BLOCK");
    }

    // Sets the object inactive
    private void DestroyBlock(Collider collider)
    {
        GameObject destroyableBlock = collider.gameObject;

        puzzleManagerScript.InstantiateParticleEffect(destroyableBlock, "DestroyedRockParticle", -0.25f);
        audioManagerScript.PlayBreakRockSFX();
        destroyableBlock.SetActive(false);
        SubractFromTorchMeter();

        ChangeAnimationState("Interacting");
        //Debug.Log("Interacted with DESTROYABLE BLOCK");
    }

    // Checks to turn off the firestone's light
    private void UseFirestone(Collider collider)
    {
        Light firestoneLight = collider.gameObject.GetComponentInChildren<Light>();
        if (!firestoneLight.enabled) return;

        torchMeterScript.CurrentVal = torchMeterScript.MaxVal;
        audioManagerScript.PlayIgniteFireSFX();
        firestoneLight.enabled = false;

        ChangeAnimationState("Interacting");
        //Debug.Log("Interacted with FIRESTONE");
    }

    // Checks to turn on the generator
    private void TurnOnGenerator(Collider collider)
    {
        Generator generatorScript = collider.gameObject.GetComponent<Generator>();
        if (generatorScript.IsGenActive) return;

        audioManagerScript.SetGeneratorScript(generatorScript);
        generatorScript.TurnOnGenerator();
        SubractFromTorchMeter();

        ChangeAnimationState("Interacting");
        //Debug.Log("Interacted with GENERATOR");
    }

    // Checks to interact with the npc - starts the npc dialogue if so
    private void InteractWithNPC(Collider collider)
    {
        NonPlayerCharacter nPCScript = collider.GetComponent<NonPlayerCharacter>();
        NonPlayerCharacter_SO nPC = nPCScript.nonPlayerCharacter;
        if (!nPCScript.enabled) return;

        dialogueViewsHolder.transform.position = collider.transform.position;
        alertBubble.SetActive(false);

        characterDialogueScript.StartNPCDialogue(nPCScript, nPC);
        //Debug.Log("Interacted with NPC");
    }

    // Checks to interact with the artifact - starts the artifact dialogue if so
    private void InteractWithArtifact(Collider collider)
    {
        Artifact artifactScript = collider.GetComponent<Artifact>();
        Artifact_SO artifact = artifactScript.artifact;
        if (!artifactScript.enabled) return;

        dialogueViewsHolder.transform.position = collider.transform.position;
        alertBubble.SetActive(false);

        characterDialogueScript.StartArtifactDialogue(artifactScript, artifact);
        artifactScript.OpenChest();

        ChangeAnimationState("Interacting");
        //Debug.Log("Interacted with ARTIFACT");
    }

    // Returns the collider of the object in front of the player 
    private Collider GetCollider()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), transform.TransformDirection(Vector3.forward));
        RaycastHit hit;
        //Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        // If the ray doesn't hit anything, then there's NO collider
        if (!Physics.Raycast(myRay, out hit, rayLength)) return null;

        return hit.collider;
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
        //Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        // If the ray hits anything and it's not an empty block, then there's NO edge
        if (Physics.Raycast(myRay, out hit, rayLength) && !hit.collider.CompareTag("EmptyBlock")) return false;

        return true;
    }

    // Checks if the player is on a tile - sets the current tile and returns true if so, false otherwise
    private bool OnTile()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;
        //Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        // If the ray doesn't hit anything, then there's NO tile
        if (!Physics.Raycast(myRay, out hit, rayLength)) return false;

        currentTile = hit.collider.gameObject;
        return true;
    }

    // Checks if the player is on a bridge - sets the current bridge and returns true if so, false otherwise
    public bool OnBridge()
    {
        if (!OnTile() || !currentTile.CompareTag("BridgeTile") && !currentTile.name.Contains("BridgeTile")) return false;

        bridge = currentTile.transform.parent.gameObject;
        puzzleManagerScript.SetCurrentBridge(bridge);
        return true;
    }

    // Checks if the player is on a checkpoint - returns true if so, false otherwise
    public bool OnCheckpoint()
    {
        if (OnBridge() || !currentTile.CompareTag("Checkpoint")) return false;

        return true;
    }

    // Checks if the player is on the last tile of a puzzle - returns true if so, false otherwise
    private bool OnLastTile()
    {
        if (OnCheckpoint()) return false;
        bool onLastTile = false;

        for (int i = 0; i < 360; i += 90)
        {
            tileCheck.transform.localEulerAngles = new Vector3(0, i, 0);
            Ray myRay = new Ray(tileCheck.transform.position + new Vector3(0, -0.1f, 0), tileCheck.transform.TransformDirection(Vector3.forward));
            RaycastHit hit;
            //Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

            // If the ray doesn't hit anything or if it doesn't hit a bridge tile, then CONTINUE the loop
            if (!Physics.Raycast(myRay, out hit, rayLength) || !hit.collider.CompareTag("BridgeTile") && !hit.collider.name.Contains("BridgeTile")) continue;

            onLastTile = true;
            break;
        }

        return onLastTile;
    }

    // Returns the position of the next bridge tile
    private Vector3? NextBridgeTile()
    {
        bridgeTileCount++;

        for (int i = 0; i < 360; i += 90)
        {
            bridgeTileCheck.transform.localEulerAngles = new Vector3(0, i, 0);

            Ray myRay = new Ray(bridgeTileCheck.transform.position + new Vector3(0, -0.1f, 0), bridgeTileCheck.transform.TransformDirection(Vector3.forward));
            RaycastHit hit;
            //Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

            // If the ray doesn't hit anything or if it hits the previous tile, then CONTINUE the loop
            if (!Physics.Raycast(myRay, out hit, rayLength) || hit.collider.gameObject == previousTile) continue;

            transform.eulerAngles = new Vector3(0, bridgeTileCheck.transform.eulerAngles.y, 0);
            previousTile = currentTile;

            return hit.collider.transform.position;
        }

        // Stop the player's walking animation at the end of the last bridge
        if (bridge.name == "EndBridge") ChangeAnimationState("Idle");
        else transform.eulerAngles -= new Vector3(0, 180, 0);

        previousTile = currentTile;
        bridgeTileCount = 0;
        return null;
    }

    // Checks to enable the player input
    public void EnablePlayerInputCheck()
    {
        if (!OnTile()) return;

        if (!OnBridge())
        {
            SetCheckpointCheck();
            SetPlayerBoolsTrue();
            ChangeAnimationState("Idle");
        }
        else
        {
            hasSetCheckpoint = false;
            FinishedZoneCheck();
            SetPlayerBoolsFalse();
            MoveToNextBridgeTile();
        }

        AlertBubbleCheck();
        TutorialDialogueCheck();
        TorchMeterAnimationCheck();
    }

    // Checks to set the alert bubble active/inactive
    public void AlertBubbleCheck()
    {
        bool canSetActive = false;
        Collider collider = GetCollider();

        if (collider == null || torchMeterScript.CurrentVal == 0f)
            canSetActive = false;

        else if (collider.CompareTag("NPC"))
            canSetActive = collider.GetComponent<NonPlayerCharacter>().enabled;

        else if (collider.CompareTag("Artifact"))
            canSetActive = collider.GetComponent<Artifact>().enabled;

        if (canSetActive) characterDialogueScript.SetAlertBubbleColor();
        alertBubble.SetActive(canSetActive);
    }

    // Checks when the torch meter can pop in/out
    public void TorchMeterAnimationCheck()
    {
        if (lerpDuration == 0f) return;
        bool onBridge = OnBridge();

        if (onBridge && !hasPopedOutTM)
        {
            torchMeterScript.TorchMeterPopOut();
            hasPopedOutTM = true;
        }
        else if (!onBridge && hasPopedOutTM)
        {
            torchMeterScript.TorchMeterPopIn();
            hasPopedOutTM = false;
        }
    }

    // Checks to set the new/current checkpoint
    private void SetCheckpointCheck()
    {
        if (hasSetCheckpoint) return;

        checkpoint = FindTile("Checkpoint");
        puzzleManagerScript.SetCurrentCheckpoint(checkpoint);
        saveManagerScript.SavePlayerPosition(checkpoint);
        saveManagerScript.SaveCameraPosition();

        bridgeTileCount = 0;
        hasSetCheckpoint = true;
    }

    // Checks if the player has completed the zone
    private void FinishedZoneCheck()
    {
        if (bridge.name != "EndBridge" || hasFinishedZone) return;

        gameManagerScript.ResetCollectedArtifactsCheck();
        transitionFadeScript.GameFadeOut();
        audioManagerScript.PlayChimeSFX();
        hasFinishedZone = true;

        // Plays the end credits after completing the fifth/last zone
        if (SceneManager.GetActiveScene().name != "FifthMap") return;
        endCreditsScript.StartEndCredits();
    }

    // Checks to play the appropriate tutorial dialogue
    private void TutorialDialogueCheck()
    {
        // Returns if the player is not within the tutorial zone, or if the player is frozen
        if (tutorialDialogueScript == null || tutorialDialogueScript.tutorialDialogue.Count == 0 ||
            torchMeterScript.CurrentVal <= 0 || transitionFadeScript.IsChangingScenes) return;

        switch (puzzleManagerScript.PuzzleNumber)
        {
            case (1): // Puzzle 1
                tutorialDialogueScript.PlayBridgeDialogueCheck(OnLastTile());
                tutorialDialogueScript.PlayPushDialogueCheck(GetCollider());
                tutorialDialogueScript.PlayWelcomeDialogueCheck();
                break;
            case (2): // Puzzle 2
                tutorialDialogueScript.PlayDialogueCheck("Break");
                break;
            case (3): // Puzzle 3
                tutorialDialogueScript.PlayDialogueCheck("Hole");
                break;
            case (4): // Puzzle 4
                tutorialDialogueScript.PlayDialogueCheck("Firestone");
                break;
            case (5): // Puzzle 5
                tutorialDialogueScript.PlayDialogueCheck("Interactables");
                break;
            case (6): // Puzzle 6
                tutorialDialogueScript.PlayDialogueCheck("FinishedTutorial");
                break;
            default: // Any other puzzle
                //Debug.Log("Unrecognizable tutorial puzzle");
                break;
        }
    }

    // Subracts a value from the torch meter's current total
    // Note: the default value to subtract will always be one if the parameter is not set
    private void SubractFromTorchMeter(int valueToSubract = 1)
    {
        if (OnBridge()) return;
        torchMeterScript.CurrentVal -= (valueToSubract != 1) ? valueToSubract : 1;
    }

    // Checks to move the player to the next/previous bridge tile
    private void MoveToNextBridgeTile()
    {
        if (previousTile == null) previousTile = currentTile;
        Vector3 previousTilePos = previousTile.transform.position;
        Vector3? nextTilePos = NextBridgeTile();

        if (nextTilePos == null && bridge.name == "EndBridge") return;

        destination = nextTilePos ?? previousTilePos;
        cameraScript.NextPuzzleViewCheck();
        StartPlayerCoroutines();
    }

    // Finds and returns the tile with a specific tag
    private GameObject FindTile(string tagName)
    {
        // Checks the current tile the player is on
        if (currentTile.CompareTag(tagName))
            return currentTile;

        // Looks through all tiles within the puzzle
        foreach (Transform child in currentTile.transform.parent)
        {
            if (child.CompareTag(tagName))
                return child.gameObject;
        }

        return null;
    }

    // Plays a new animation state for the player
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
            case ("Idle"):
                playerAnimator.SetTrigger("Idle");
                break;
            default:
                playerAnimator.Play(newState);
                break;
        }

        playerFidgetScript.SetIdleCountToZero();
    }

    // Checks which footstep sfx to play
    private void PlayFootstepSFX()
    {
        // Note: the footstep sfx will stop playing after crossing 8 tiles on the last bridge
        if (!OnTile() || hasFinishedZone && bridgeTileCount > 7) return;

        string tag = currentTile.tag.ToLower();
        string name = currentTile.name.ToLower();

        if (tag.Contains("grass") || name.Contains("grass"))
            audioManagerScript.PlayGrassFootstepSFX();

        else if (tag.Contains("snow") || name.Contains("snow"))
            audioManagerScript.PlaySnowFootstepSFX();

        else if (tag.Contains("stone") || name.Contains("stone"))
            audioManagerScript.PlayStoneFootstepSFX();

        else if (tag.Contains("metal") || name.Contains("metal"))
            audioManagerScript.PlayMetalFootstepSFX();

        else if (tag.Contains("bridge") || name.Contains("bridge"))
            audioManagerScript.PlayWoodFootstepSFX();

        else if (tag.Contains("pushableblock") || name.Contains("crate"))
            audioManagerScript.PlayCrateFootstepSFX();
    }

    // Sets the player's position to the grass material's vector position
    public void WriteToGrassMaterial()
    {
        if (grassMatPosition == transform.position) return;

        grassMatPosition = transform.position;
        grassMat.SetVector("_position", grassMatPosition);
    }

    // Stops the player's movement and footstep coroutine
    public void StopPlayerCoroutines()
    {
        if (playerMovementCoroutine != null)
            StopCoroutine(playerMovementCoroutine);

        if (playerFootstepsCoroutine != null)
            StopCoroutine(playerFootstepsCoroutine);
    }

    // Starts the player's movement and footstep coroutine
    private void StartPlayerCoroutines()
    {
        StopPlayerCoroutines();

        playerMovementCoroutine = LerpPlayerPosition(destination, lerpDuration);
        StartCoroutine(playerMovementCoroutine);

        playerFootstepsCoroutine = PlayerFootsteps(lerpDuration);
        StartCoroutine(playerFootstepsCoroutine);
    }

    // Lerps the player's position to another over a duration (endPosition = position to move to, duration = seconds)
    private IEnumerator LerpPlayerPosition(Vector3 endPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        float time = 0f;

        while (time < duration)
        {
            ChangeAnimationState("Walking");
            WriteToGrassMaterial();

            transform.position = Vector3.Lerp(startPosition, endPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        EnablePlayerInputCheck();
    }

    // Plays sfx for the player's footsteps over a duration (duration = seconds)
    private IEnumerator PlayerFootsteps(float duration)
    {
        if (duration == 0f) yield break;

        // When the player has traveled for 1/4th of the lerpDuration
        yield return new WaitForSeconds(duration * 0.25f);
        PlayFootstepSFX();

        // When the player has traveled for 3/4th of the lerpDuration
        yield return new WaitForSeconds(duration * 0.5f);

        // Note: doesn't play the second footstep while on the first/last bridge
        if (OnBridge() && puzzleManagerScript.BridgeNumber == null) yield break;
        PlayFootstepSFX();
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
    private void SetElements()
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

        lerpDuration = gameManagerScript.playerLerpDuration;
        resetPuzzleDelay = gameManagerScript.resetPuzzleDelay;
        grassMat = gameManagerScript.grassMaterial;
        WriteToGrassMaterial();
    }

}
