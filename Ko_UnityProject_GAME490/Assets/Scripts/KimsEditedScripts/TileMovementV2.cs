using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TileMovementV2 : MonoBehaviour
{
    public GameObject torchFireIgniteSFX;
    public GameObject torchFireExtinguishSFX;
    public GameObject freezingSFX;

    Vector3 up = Vector3.zero,                                  // Object look North
    right = new Vector3(0, 90, 0),                              // Object look East
    down = new Vector3(0, 180, 0),                              // Object look South
    left = new Vector3(0, 270, 0),                                // Object look West
    currentDirection = Vector3.zero;                            // Default state

    Vector3 nextPos, destination, direction;

    float speed = 5f;                                           // The speed at which the object will move from its current position to the destination
    float rayLength = 1f;                                       // The ray length for the tile movement
    float rayLengthEdgeCheck = 1f;                              // The ray length for the edge check

    public GameObject edgeCheck;

    private bool isWalking;                                     // Used to determine when to play an object's animation
    private bool isPushing;                                     // Used to determine when the object can move
    private bool isInteracting;
    private bool alreadyPlayedSFX;

    public Animator Anim;
    private string currentState;

    public GameObject destroyedBlockParticle;

    public Camera main_camera;

    public TorchMeterStat torchMeterMoves;

    public GameObject checkpoint;
    public GameObject puzzle;

    private void Awake()
    {
        torchMeterMoves.Initialize();
    }

    void Start()
    {
        currentDirection = up;
        nextPos = Vector3.forward; // The next block postion is equal to the object's forward axis (it will move along the direction it is facing)
        destination = transform.position; // The point where the object is currently at
    }

    void Update()
    {
        Move();
        Anim.SetBool("isWalking", isWalking);
        Anim.SetBool("isPushing", isPushing);
        Anim.SetBool("isInteracting", isInteracting);

        /*** For Debugging purposes ***/
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            torchMeterMoves.CurrentVal--;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            torchMeterMoves.CurrentVal++;
        }
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
        // Moving the character to the destination
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        // Checks if we're on a checkpoint AFTER moving
        if (Vector3.Distance(destination, transform.position) <= 0.00001f)
        {
            isWalking = false;
            checkIfOnCheckpoint();
            // If player runs out of the meter
            if (torchMeterMoves.CurrentVal <= 0 && !alreadyPlayedSFX)
            {
                Instantiate(torchFireExtinguishSFX, transform.position, transform.rotation); // Play audio clip
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

        if (!updateKeyboardInput()) return; // Returns if we have no movement keyboard input

        transform.localEulerAngles = currentDirection;

        // Checking for colliders
        if (Valid())
        {
            if (EdgeCheck()) // If no colliders, but we hit an edge
            {
                isWalking = true;
                PlayerSounds.instance.TileCheck();
                destination = transform.position + nextPos;
                direction = nextPos;
                if (!onBridge()) torchMeterMoves.CurrentVal -= 1;
                else ResetTorchMeter();
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
        /*** Movement inputs ***/
        // W key (North)
        if (Input.GetKeyDown(KeyCode.W))
        {
            nextPos = Vector3.forward;
            currentDirection = up;
            return true;
        }

        // S key (South)
        else if (Input.GetKeyDown(KeyCode.S))
        {
            nextPos = Vector3.back;
            currentDirection = down;
            return true;
        }

        // D key (East)
        else if (Input.GetKeyDown(KeyCode.D))
        {
            nextPos = Vector3.right;
            currentDirection = right;
            return true;
        }

        // A key (West)
        else if (Input.GetKeyDown(KeyCode.A))
        {
            nextPos = Vector3.left;
            currentDirection = left;
            return true;
        }

        /*** Non-Movement Inputs ***/

        // Hit R (Reset puzzle)
        else if (Input.GetKeyDown(KeyCode.R)) resetPuzzle();

        // Hit Return (Break block)
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            Collider collider = getCollider();
            if (collider.tag != "DestroyableBlock") return false;
            Debug.Log("Destroyed Block");
            torchMeterMoves.CurrentVal--;
            Instantiate(destroyedBlockParticle, collider.gameObject.transform.position, collider.gameObject.transform.rotation);
            collider.gameObject.SetActive(false);
            isWalking = false;
        }
        return false;
    }

    /***
     * Draws a ray forward and returns the collider if it hits, or null otherwise
     ***/
    Collider getCollider()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), transform.forward);
        RaycastHit hit;

        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        if (Physics.Raycast(myRay, out hit, rayLength)) return hit.collider;
        return null;
    }

    /***
     * The bool function that checks to see if the next position is valid or not
     ***/
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

            case ("Obstacle"):
                if (collider.gameObject.GetComponent<BlockMovement>().MoveBlock(currentDirection))
                    torchMeterMoves.CurrentVal--;
                isWalking = false;
                CheckToPlayAnims();
                break;

            case ("DestroyableBlock"):
                collider.gameObject.GetComponentInChildren<ObjectShakeController>().StartShake(0.2f, 0.25f);
                isWalking = false;
                CheckToPlayAnims();
                break;

            case ("FireStone"):
                if (collider.gameObject.GetComponentInChildren<Light>().enabled == true)
                {
                    torchMeterMoves.CurrentVal = torchMeterMoves.MaxVal;
                    Instantiate(torchFireIgniteSFX, transform.position, transform.rotation); // Spwans the particle effect on the object's position and rotation
                }
                collider.gameObject.GetComponentInChildren<Light>().enabled = false;
                isWalking = false;
                CheckToPlayAnims();
                break;

            case ("Generator"):
                Debug.Log("Turned On Generator");
                torchMeterMoves.CurrentVal--;
                collider.gameObject.GetComponentInChildren<GeneratorScript>().TurnOnGenerator();
                isWalking = false;
                CheckToPlayAnims();
                break;

            default:
                Debug.Log("Unrecognizable Tag");
                isWalking = false;
                CheckToPlayAnims();
                break;
        }
        return false;
    }

    /*** Checks if there's an edge ***/
    bool EdgeCheck()
    {
        Ray myEdgeRay = new Ray(edgeCheck.transform.position, -transform.up);
        RaycastHit hit;
        Debug.DrawRay(myEdgeRay.origin, myEdgeRay.direction, Color.red);

        if (Physics.Raycast(myEdgeRay, out hit, rayLengthEdgeCheck))
        {
            string tag = hit.collider.tag;
            if (tag == "MoveCameraBlock")
            {
                torchMeterMoves.CurrentVal = torchMeterMoves.MaxVal;
                return true;
            }
            else if (tag == "EmptyBlock")
            {
                isWalking = false;
                return false;
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

    /***
     * Resets the current value of the torch meter to the maximum value
     ***/
    public void ResetTorchMeter()
    {
        torchMeterMoves.CurrentVal = torchMeterMoves.MaxVal;
    }

    /***
     * Draws a ray below the character and returns true if player is standing on a checkpoint, returns false otherwise
     ***/
    private bool checkIfOnCheckpoint()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        Physics.Raycast(myRay, out hit, rayLength);
        if (hit.collider.tag != "Checkpoint") return false; // If we did not hit a checkpoint

        checkpoint = hit.collider.gameObject;
        puzzle = hit.collider.transform.parent.parent.gameObject;

        // Setting new puzzle view
        GameObject view = puzzle.transform.Find("View").gameObject;
        main_camera.GetComponent<CameraController>().NextPuzzleView(view.transform);

        // Setting new torch meter value
        int newNumMovements = checkpoint.GetComponent<CheckpointV2>().getNumMovements();
        torchMeterMoves.setMaxValue(newNumMovements);

        // If this is the first time we visited this checkpoint
        if (!checkpoint.GetComponent<CheckpointV2>().hitCheckpoint())
        {
            checkpoint.GetComponent<CheckpointV2>().setCheckpoint();
            ResetTorchMeter();
            main_camera.GetComponent<CameraController>().WindGush();
        }
        return true;
    }

    /***
     * Draws a ray below the character - Returns true of the player is standing on a bridge, false otherwise
     ***/
    private bool onBridge()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        Physics.Raycast(myRay, out hit, rayLength);
        if (hit.collider.tag == "WoodTiles" || hit.collider.tag == "MoveCameraBlock") return true;
        return false;
    }

    /***
     * Resets the current puzzle the player is at (from last checkpoint)
     ***/
    private void resetPuzzle()
    {
        checkpoint.GetComponent<CheckpointV2>().resetPlayerPosition();
        ResetTorchMeter();

        Debug.Log("Pushable blocks child count: " + puzzle.transform.childCount);
        for (int i = 0; i < puzzle.transform.childCount; i++)
        {
            GameObject child = puzzle.transform.GetChild(i).gameObject;
            if (child.name == "Pushable Blocks")
                child.GetComponent<ResetPushableBlocks>().resetBlocks();

            else if (child.name == "Breakable Blocks")
                child.GetComponent<ResetBreakableBlocks>().resetBlocks();

            else if (child.name == "Fire Stones")
                child.GetComponent<ResetFireStone>().resetFirestone();

            else if (child.name == "Generator Blocks")
                child.GetComponent<ResetGeneratorBlocks>().resetGenerator();
        }
    }

    /***
     * Resets the puzzle the player is currently at (from the last checkpoint) (with delay)
     ***/
    private void resetPuzzleWithDelay()
    {
        checkpoint.GetComponent<CheckpointV2>().StartCoroutine("resetPlayerPositionWithDelay", 1.5f);

        Debug.Log("Pushable blocks child count: " + puzzle.transform.childCount);
        for (int i = 0; i < puzzle.transform.childCount; i++)
        {
            GameObject child = puzzle.transform.GetChild(i).gameObject;
            if (child.name == "Pushable Blocks")
                child.GetComponent<ResetPushableBlocks>().StartCoroutine("resetBlocksWithDelay", 1.5f);
            
            else if (child.name == "Breakable Blocks")
                child.GetComponent<ResetBreakableBlocks>().StartCoroutine("resetBlocksWithDelay", 1.5f);

            else if (child.name == "Fire Stones")
                child.GetComponent<ResetFireStone>().StartCoroutine("resetFirestoneWithDelay", 1.5f);

            else if (child.name == "Generator Blocks")
                child.GetComponent<ResetGeneratorBlocks>().StartCoroutine("resetGeneratorWithDelay", 1.5f);
        }

    }

    private void ChangeAnimationState(string newState)
    {
        Anim.Play(newState);
        currentState = newState;
    }

    private void CheckToPlayAnims()
    {
        if (isPushing) ChangeAnimationState("Pushing");
        else if (isInteracting) ChangeAnimationState("Interacting");
        isPushing = false;
        isInteracting = false;
    }
}
