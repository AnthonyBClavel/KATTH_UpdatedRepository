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

    [Header("Save Slot Elements")]
    public GameObject checkpoint;
    public GameObject puzzle;
    public string sceneName;

    [Header("Bools")]
    public bool isWalking;                                      // Determines when to play an object's animation
    public bool hasDied = false;
    public bool canMove = true;
    public bool canInteract = true;
    public bool canRestartPuzzle;                               // Determines when the player can press "R" (restart puzzle)
    private bool isPushing;                                     // Determines when the object can move
    private bool isInteracting;
    private bool alreadyPlayedSFX;
    private bool hasAlreadyPopedOut;                            // Determines when the torch meter can scale in/out
    private bool hasMovedPuzzleView;                            // Determines when the camera can switch puzzle views
    

    void Awake()
    {
        torchMeterMoves.Initialize();
        savedInvisibleBlock = GameObject.Find("SavedInvisibleBlock");

        levelManagerScript = FindObjectOfType<LevelManager>();
        torchMeterScript = FindObjectOfType<TorchMeterScript>();

        saveManagerScript = FindObjectOfType<SaveManagerScript>();
        //saveManagerScript.LoadPlayerPosition();
        //saveManagerScript.LoadBlockPosition();
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
        audioSource = GetComponent<AudioSource>();

        currentDirection = up;
        nextPos = Vector3.forward; // The next block postion is equal to the object's forward axis (it will move along the direction it is facing)
        destination = transform.position; // The point where the object is currently at
    }

    void Update()
    {
        sceneName = SceneManager.GetActiveScene().name;
        Move();
        checkIfOnBridgeController();
        Anim.SetBool("isWalking", isWalking);
        Anim.SetBool("isPushing", isPushing);
        Anim.SetBool("isInteracting", isInteracting);

        /*** For Debugging purposes ***/
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            torchMeterMoves.CurrentVal--;
        if (Input.GetKeyDown(KeyCode.RightArrow))
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
            levelManagerScript.checkIfCompletedLevel();

            // If player's torch meter runs out...
            if (torchMeterMoves.CurrentVal <= 0 && !alreadyPlayedSFX)
            {
                Instantiate(torchFireExtinguishSFX, transform.position, transform.rotation);
                Instantiate(freezingSFX, transform.position, transform.rotation);
                alreadyPlayedSFX = true; // The audio clip cannot be played again
                resetPuzzleWithDelay();
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
                PlayerSounds.instance.TileCheck();
                destination = transform.position + nextPos;
                direction = nextPos;
                if (!onBridge())
                {
                    torchMeterMoves.CurrentVal -= 1;
                }
                else
                {
                    ResetTorchMeter();
                }
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
        // W key (North)
        if (Input.GetKeyDown(KeyCode.W) && canMove)
        {
            nextPos = Vector3.forward;
            currentDirection = up;
            return true;
        }

        // S key (South)
        else if (Input.GetKeyDown(KeyCode.S) && canMove)
        {
            nextPos = Vector3.back;
            currentDirection = down;
            return true;
        }

        // D key (East)
        else if (Input.GetKeyDown(KeyCode.D) && canMove)
        {
            nextPos = Vector3.right;
            currentDirection = right;
            return true;
        }

        // A key (West)
        else if (Input.GetKeyDown(KeyCode.A) && canMove)
        {
            nextPos = Vector3.left;
            currentDirection = left;
            return true;
        }
        /*** Movement inputs end here ***/

        // Hit R to reset puzzle
        else if (Input.GetKeyDown(KeyCode.R)) resetPuzzle();

        // Hit Return to interact/break block
        else if (Input.GetKeyDown(KeyCode.Return) && canInteract)
        {
            Collider collider = getCollider();
            if (collider == null) return false;
            if (collider.tag == "DestroyableBlock") destroyBlock(collider);
            if (collider.tag == "Generator") turnOnGenerator(collider);
            if (collider.tag == "FireStone") getFirestone(collider);
            else if (collider.tag == "NPC") interactWithNPC(collider);
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
        Debug.Log("Turned On Generator");
        if (collider.gameObject.GetComponent<GeneratorScript>().canInteract == true) torchMeterMoves.CurrentVal--;
        collider.gameObject.GetComponentInChildren<GeneratorScript>().TurnOnGenerator();
        isInteracting = true;
        isWalking = false;
        CheckToPlayAnims();
    }

    // Disables firestone light if the player is colliding/interacting with it
    public void getFirestone(Collider collider)
    {
        Debug.Log("Firestone has been used");
        if (collider.gameObject.GetComponentInChildren<Light>().enabled == true)
        {
            torchMeterMoves.CurrentVal = torchMeterMoves.MaxVal;
            Instantiate(torchFireIgniteSFX, transform.position, transform.rotation);
        }
        collider.gameObject.GetComponentInChildren<Light>().enabled = false;
        isInteracting = true;
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
        GameObject npc = collider.gameObject;
        npc.GetComponent<Interactable>().Interact();
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
                    torchMeterMoves.CurrentVal = torchMeterMoves.MaxVal;
                    Instantiate(torchFireIgniteSFX, transform.position, transform.rotation); // Spwans the particle effect on the object's position and rotation
                }
                collider.gameObject.GetComponentInChildren<Light>().enabled = false;
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
        Ray myEdgeRay = new Ray(edgeCheck.transform.position, -transform.up);
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
            checkpoint.GetComponent<CheckpointManager>().setCheckpoint();
            ResetTorchMeter();
            PopInTorchMeterCheck();
            SetPlayerBoolsTrue(); //Enable Player Movement
            saveManagerScript.SavePlayerPosition();
            saveManagerScript.SaveCameraPosition();
            //main_camera.GetComponent<CameraController>().WindGush();
        }
        return true;
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

        if (name == "BridgeBlock")
        {
            if (tag == "BridgeController" && !isWalking)
            {
                Debug.Log("You hit this object");
                SetPlayerBoolsFalse();
                hit.collider.gameObject.GetComponent<BridgeMovementController>().MoveToNextBlock();
                transform.localEulerAngles = new Vector3(0, hit.collider.gameObject.GetComponent<BridgeMovementController>().newDirection, 0);
                NextPuzzleViewCheck();
            }
            if (tag == "LastBridgeTile" && hasMovedPuzzleView)
            {
                Debug.Log("Invisible Block Position has been saved");
                savedInvisibleBlock.transform.position = hit.collider.transform.position + new Vector3(0, 1, 0);
                saveManagerScript.SaveBlockPosition();
                hasMovedPuzzleView = false;
            }
            return true;
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

        if (name == "BridgeBlock" || name == "PatchyGrassBlockTutorial")
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

    // Resets the current puzzle the player is on (determined by last checkpoint)
    private void resetPuzzle()
    {
        // You cant restart a puzzle while on a bridge tile (determined in EdgeCheck)
        if (canRestartPuzzle)
        {
            checkpoint.GetComponent<CheckpointManager>().resetPlayerPosition();
            ResetTorchMeter();

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

    // Resets the current puzzle the player is on (determined by last checkpoint) BUT with a delay
    private void resetPuzzleWithDelay()
    {
        checkpoint.GetComponent<CheckpointManager>().StartCoroutine("resetPlayerPositionWithDelay", 1.5f);

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

        //ResetTorchMeter();
        hasDied = true;
    }

    
    public void SetPlayerBoolsFalse()
    {
        canRestartPuzzle = false;
        canMove = false;
        canInteract = false;
    }

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
        if(!hasMovedPuzzleView)
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

    private SaveSlot makeSaveSlot()
    {
        Vector3 position = transform.position;
        float x = position.x;
        float y = position.y;
        float z = position.z;
        float[] playerPosition = { x, y, z };

        string puzzleName = puzzle.name;

        int currCameraIndex = main_camera.GetComponent<CameraController>().currentIndex;

        return new SaveSlot(sceneName, playerPosition, puzzleName, currCameraIndex);
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
