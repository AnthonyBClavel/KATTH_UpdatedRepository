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
    left = new Vector3(0,270,0),                                // Object look West
    currentDirection = Vector3.zero;                            // Default state

    Vector3 nextPos, destination, direction;

    private bool canMove;                         

    float speed = 5f;                                           // The speed at which the object will move from its current position to the destination    
    float rayLength = 1f;                                       // The ray length for the tile movement
    float rayLengthEdgeCheck = 1f;                              // The ray length for the edge check

    public GameObject edgeCheck;

    private bool isWalking;                                     // Used to determine when to play an object's animation
    private bool isPushing;                                     // Used to determine when the object can move 
    private bool isInteracting;
    private bool canPush;                                       // The bool is used to determine when the object can be pushed 
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
        currentDirection = up; // The direction the object faces when you start the game
        nextPos = Vector3.forward; // The next block postion is equal to the object's forward axis (it will move along the direction it is facing)
        destination = transform.position; // The point where the object is currently at
    }

    void Update()
    {
        Move();                                                                      
        Push();                                                                                        
        Anim.SetBool("isWalking", isWalking);                                                        
        Anim.SetBool("isPushing", isPushing);                                                             
        Anim.SetBool("isInteracting", isInteracting);

        /** For Debugging purposes **/
        if (Input.GetKeyDown(KeyCode.LeftArrow))                                               
        {
            torchMeterMoves.CurrentVal -= 1;                                         
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))                                                
        {
            torchMeterMoves.CurrentVal += 1;                                                
        }
        /** End Debugging **/

        // If player wants to reset puzzle
        if (Input.GetKeyDown(KeyCode.R))
        {
            resetPuzzle();
        }

        if(torchMeterMoves.CurrentVal > 0)                                                         
        {
            alreadyPlayedSFX = false;                                                    
        }
    }

    /**
     * The main movement function for the object.
     * When a specific button is pressed, the object is set to move along the stated axis.
     * The object can move and perform the walking animation while the button stays pressed.
     **/
    void Move()                                                                                           
    {
        // Moving the character to the destination
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
        isWalking = false;

        // Checks if we're on a checkpoint AFTER moving
        if (Vector3.Distance(destination, transform.position) <= 0.00001f) {
            checkIfOnCheckpoint();
            // If player runs out of the meter
            if (torchMeterMoves.CurrentVal <= 0 && !alreadyPlayedSFX)
            {
                Instantiate(torchFireExtinguishSFX, transform.position, transform.rotation);                  // Play audio clip
                Instantiate(freezingSFX, transform.position, transform.rotation);
                alreadyPlayedSFX = true;                                                                      // The audio clip cannot be played again
                resetPuzzleWithDelay();
            }
        }


        // Up
        if (Input.GetKeyDown(KeyCode.W))                                                                  
        {
            nextPos = Vector3.forward;                                                                   
            currentDirection = up;                                                                       
            canMove = true;         
        }

        // Down
        else if (Input.GetKeyDown(KeyCode.S))
        {
            nextPos = Vector3.back;
            currentDirection = down;
            canMove = true;          
        }
        
        // Right
        else if (Input.GetKeyDown(KeyCode.D))
        {
            nextPos = Vector3.right;
            currentDirection = right;
            canMove = true;      
        }

        // Left
        else if (Input.GetKeyDown(KeyCode.A))
        {
            nextPos = Vector3.left;
            currentDirection = left;
            canMove = true;
        }

        // Checks to see how big the distance is between the object's current position and the destination
        if (Vector3.Distance(destination, transform.position) <= 0.00001f)                                
        {
            /**
            * Rotates the actual object to the current direction 
            * (the raycast will roatate with it, along the axis its facing)
            **/
            transform.localEulerAngles = currentDirection;                                              
            if (canMove)
            {
                isWalking = true;

                if (Valid() && EdgeCheck())                                                       
                {      
                    PlayerSounds.instance.TileCheck();
                    destination = transform.position + nextPos;                                          
                    direction = nextPos;
                    if (!onBridge())
                        torchMeterMoves.CurrentVal -= 1;
                    else
                        ResetTorchMeter();
                    canMove = false;  // Prevents the object from constantly moving towards the object's current direction  
                }           
            }    
        }
    }

    void Push()                                                                                  
    {
        if(Input.GetKey(KeyCode.W))                                                   
        {
            nextPos = Vector3.forward;                                                       
            currentDirection = up;                                                                
            canPush = true;  // The object can push another object while the statement above is true
            if (!Valid()) // If the object cannot move
            {
                canMove = false;
                CheckToPlayAnims();
            }
        }

        else if (Input.GetKey(KeyCode.A))
        {
            nextPos = Vector3.left;
            currentDirection = left;
            canPush = true;
            if (!Valid())
            {
                canMove = false;
                CheckToPlayAnims();
            }
        }

        else if (Input.GetKey(KeyCode.S))
        {
            nextPos = Vector3.back;
            currentDirection = down;
            canPush = true;
            if (!Valid())
            {
                canMove = false;
                CheckToPlayAnims();
            }
        }

        else if (Input.GetKey(KeyCode.D))
        {
            nextPos = Vector3.right;
            currentDirection = right;
            canPush = true;
            if (!Valid())
            {
                canMove = false;
                CheckToPlayAnims();
            }
        }
    }

    /**
     * The bool function that checks to see if the next position is valid or not
     **/
    bool Valid()                                                                                                                                
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), transform.forward);                                                 
        RaycastHit hit;

        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);                                                                               

        if (Physics.Raycast(myRay, out hit, rayLength))                        
        {
            //if (hit.collider.tag == "Obstacle" || hit.collider.tag == "StaticBlock" || hit.collider.tag == "DestroyableBlock")                

            if (hit.collider.tag == "FireStone" && Input.GetKeyDown(KeyCode.Return) && canPush)                              
            {
                if(hit.collider.gameObject.GetComponentInChildren<Light>().enabled == true)
                {
                    torchMeterMoves.CurrentVal = torchMeterMoves.MaxVal;                                                                    
                    Instantiate(torchFireIgniteSFX, transform.position, transform.rotation); // Spwans the particle effect on the object's position and rotation
                }
                hit.collider.gameObject.GetComponentInChildren<Light>().enabled = false;
                isWalking = false;                                                                                                              
                return false;                                                                                                                 
            }

            if (hit.collider.tag == "Obstacle" && canPush)                                                                                     
            {
                bool move = hit.collider.gameObject.GetComponent<BlockMovement>().MoveBlock();
                if (Input.GetKeyDown(KeyCode.LeftShift) && move)                                                                      
                {
                    torchMeterMoves.CurrentVal -= 1;      
                }

                //hit.collider.gameObject.GetComponent<BlockMovement>().MoveBlock();                                                       
                isWalking = false;  // Cannot play its walking animation while the statement above is true
                return false; //the bool function will return as false if the statement above is true
            }

            if (hit.collider.tag == "StaticBlock" && canPush && Input.GetKeyDown(KeyCode.LeftShift))                                  
            {
                Debug.Log("Cannot Push Static Block");                                                                                   
                hit.collider.gameObject.GetComponentInChildren<ObjectShakeController>().StartShake(0.25f, 0.25f);                                                        
                isWalking = false;                                                                                                            
                return false;                                                                                                           
            }

            if (hit.collider.tag == "DestroyableBlock" && canPush && Input.GetKeyDown(KeyCode.LeftShift))                                     
            {
                Debug.Log("Cannot Push Breakable Block");                                                                                     
                hit.collider.gameObject.GetComponentInChildren<ObjectShakeController>().StartShake(0.2f, 0.25f);                                                             
                isWalking = false;                                                                                                             
                return false;                                                                                                                  
            }

            if (hit.collider.tag == "DestroyableBlock" && Input.GetKeyDown(KeyCode.Return) && canPush)                                          
            {
                Debug.Log("Destroyed Block");                                                                                                  
                torchMeterMoves.CurrentVal -= 1;                                                                                                
                Instantiate(destroyedBlockParticle, hit.collider.gameObject.transform.position, hit.collider.gameObject.transform.rotation);    
                hit.collider.gameObject.SetActive(false);
                isWalking = false;                                                                                                             
                return false;                                                                                                                   
            }
            else
            {
                isWalking = false;                                                                                                              
                return false;                                                                                                                    
            }
        }
        return true;                                                                                                                           

    }

    /** Checks if there's an edge **/
    bool EdgeCheck()                                                                                                                           
    {
        Ray myEdgeRay = new Ray(edgeCheck.transform.position, -transform.up);                                                                  
        RaycastHit hit;

        Debug.DrawRay(myEdgeRay.origin, myEdgeRay.direction, Color.red);                                                                        

        if (Physics.Raycast(myEdgeRay, out hit, rayLengthEdgeCheck))                                                                            
        {
            if(hit.collider.tag == "MoveCameraBlock")                                                                             
            {
                torchMeterMoves.CurrentVal = torchMeterMoves.MaxVal;                                                                     
                // CameraController.instance.NextPuzzleView();                                                                               
                return true;                                                                                                    
            }
            if(hit.collider.tag == "EmptyBlock")
            {
                isWalking = false;
                return false;
            }
            return true;                                                                                                                      
        }

        isWalking = false;                                                                                                                 
        return false;                                                                                                                          
    }                                                                                           

    /**
     * Sets the destination of the player to the new destination.
     * @param newDestination - The new destination to set to
     **/
    public void setDestination(Vector3 newDestination)
    {
        destination = newDestination;
    }

    public void ResetTorchMeter()
    {
        torchMeterMoves.CurrentVal = torchMeterMoves.MaxVal;
    }

    private bool checkIfOnCheckpoint()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);                                          
        RaycastHit hit;

        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);                                                              

        Physics.Raycast(myRay, out hit, rayLength);
        if (hit.collider.tag != "Checkpoint") return false; // If we did not hit a checkpoint

        //Debug.Log("On checkpoint");
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

    private bool onBridge()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;

        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        Physics.Raycast(myRay, out hit, rayLength);
        if (hit.collider.tag == "WoodTiles" || hit.collider.tag == "MoveCameraBlock")
        {
            return true;
        }
        return false;
    }

    private void resetPuzzle()
    {
        checkpoint.GetComponent<CheckpointV2>().resetPlayerPosition();
        ResetTorchMeter();

        Debug.Log("Pushable blocks child count: " + puzzle.transform.childCount);
        for (int i = 0; i < puzzle.transform.childCount; i++)
        {
            GameObject child = puzzle.transform.GetChild(i).gameObject;
            if (child.name == "Pushable Blocks")
            {
                child.GetComponent<ResetPushableBlocks>().resetBlocks();
            }

            else if (child.name == "Breakable Blocks")
            {
                child.GetComponent<ResetBreakableBlocks>().resetBlocks();
            }

            else if (child.name == "Fire Stones")
            {
                child.GetComponent<ResetFireStone>().resetFirestone();
            }
        }

    }

    private void resetPuzzleWithDelay()
    {

        checkpoint.GetComponent<CheckpointV2>().StartCoroutine("resetPlayerPositionWithDelay", 1.5f);

        Debug.Log("Pushable blocks child count: " + puzzle.transform.childCount);
        for (int i = 0; i < puzzle.transform.childCount; i++)
        {
            GameObject child = puzzle.transform.GetChild(i).gameObject;
            if (child.name == "Pushable Blocks")
            {
                child.GetComponent<ResetPushableBlocks>().StartCoroutine("resetBlocksWithDelay", 1.5f); 
            }

            else if (child.name == "Breakable Blocks")
            {
                child.GetComponent<ResetBreakableBlocks>().StartCoroutine("resetBlocksWithDelay", 1.5f);
            }

            else if (child.name == "Fire Stones")
            {
                child.GetComponent<ResetFireStone>().StartCoroutine("resetFirestoneWithDelay", 1.5f);
            }
        }

    }

    private void ChangeAnimationState (string newState)
    {
        Anim.Play(newState);
        currentState = newState;
    }

    private void CheckToPlayAnims()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            ChangeAnimationState("Pushing");
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            ChangeAnimationState("Interacting");
        }
        else
        {
            isPushing = false;
            isInteracting = false;
        }
    }

}
