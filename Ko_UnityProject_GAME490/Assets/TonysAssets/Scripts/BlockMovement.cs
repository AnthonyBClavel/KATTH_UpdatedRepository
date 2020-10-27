using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BlockMovement : MonoBehaviour
{
    Vector3 up = Vector3.zero,                                  //to make the object look up (north)
    right = new Vector3(0, 90, 0),                              //to make the object look right (east)
    down = new Vector3(0, 180, 0),                              //to make the object look down (south)
    left = new Vector3(0, 270, 0),                              //to make the player look left (west)
    currentDirection = Vector3.zero;                            //this will be its default state - the direction it'll face when you start the game

    Vector3 nextBlockPos, destination, direction, startingPosition;

    bool canMoveBlock;                                          //the bool is used to determine when the object can move

    float speed = 5f;                                           //the speed at which the object will move from its current position to the destination               
    float rayLength = 1f;                                       //the ray length for the tile movement
    float rayLengthEdgeCheck = 1f;                              //the ray length for the edge check

    public GameObject crateEdgeCheck;                           //variable for an object - this will be used for the edge check bool function (gameobject determined in Unity inspector)

    private AudioSource audioSource;                            //establishes a variable for an audio source component
    public AudioClip pushCrateSFX;                              //variable for an audioclip (audio clip determined in Unity inspector) 
    public AudioClip cantPushCrateSFX;                          //variable for an audioclip (audio clip determined in Unity inspector) 

    void Start()
    {
        currentDirection = up;                                  //the direction the object faces when you start the game
        nextBlockPos = Vector3.forward;                         //the next block postion is equal to the object's forward axis (it will move along the direction it is facing)
        destination = transform.position;                       //the point where the object is currenlty at 
        audioSource = GetComponent<AudioSource>();              //sets the audio source variable to the object's audio source component (sets instance)
        startingPosition = transform.position;
    }

    void Update()
    {
        //MoveBlock();                                          //calls the MoveBlock function stated below - dont use this line, it's just for reference
    }

    public bool MoveBlock()
    {
        transform.position = Vector3.MoveTowards(transform.position, destination, speed /* * Time.deltaTime*/); //when the object starts moving, the object moves from its current position to the destination, **this needs to be refined** , uncheck Time.deltaTime to see what I mean...
        bool valid = false;
        bool edgeCheck = false;
        if (Input.GetKey(KeyCode.W) && Input.GetKeyDown(KeyCode.LeftShift))                                     //when the specified keys are pressed... (GetKey = hold key down, GetKeyDown = press key)
        {
            Debug.Log("Pushed Block Up");                                                                       //sends a debug message saying in the console (just for debugging purposes)
            nextBlockPos = Vector3.forward;                                                                     //...the object is set to move along the stated axis
            currentDirection = up;                                                                              //sets the object to rotate towards the specified direction
            canMoveBlock = true;                                                                                //the object can move while the if statement above is true           
        }

        if (Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.LeftShift))                    
        {
            Debug.Log("Pushed Block Down");
            nextBlockPos = Vector3.back;
            currentDirection = down;
            canMoveBlock = true;
        }

        if (Input.GetKey(KeyCode.D) && Input.GetKeyDown(KeyCode.LeftShift))
        {
            Debug.Log("Pushed Block Right");
            nextBlockPos = Vector3.right;
            currentDirection = right;
            canMoveBlock = true;
        }

        if (Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.LeftShift))
        {
            Debug.Log("Pushed Block Left");
            nextBlockPos = Vector3.left;
            currentDirection = left;
            canMoveBlock = true;
        }

        if (Vector3.Distance(destination, transform.position) <= 0.00001f)                                                      //checks to see how big the distance is between the object's current position and the destination
        {
            transform.localEulerAngles = currentDirection;                                                                      //rotates the actual object to the current direction

            if (canMoveBlock)                                                                                                   //if the object can move (if the bool is true)...
            {
                valid = Valid();
                edgeCheck = EdgeCheck();
                if (valid && edgeCheck)                                                                                     //if the bool functions below are returned as true...
                {
                    destination = transform.position + nextBlockPos;                                                            //updates the destination by adding the next position to the object's current position
                    direction = nextBlockPos;
                    audioSource.PlayOneShot(pushCrateSFX);                                                                      //plays an audio clip - the clip cannot be canceled if another is played
                    canMoveBlock = false;                                                                                       //prevents the object from constantly moving towards the object's current direction
                }
            }
        }
        return (valid && edgeCheck);

    }

    bool Valid()                                                                                                                //the bool function that checks to see if the next position is valid or not
    {
        Ray myRay = new Ray(transform.position, transform.forward);                                                             //shoots a ray into the direction that the object is looking towards
        RaycastHit hit;

        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);                                                                //shows a debug line of the raycast that was called previously (just for debugging purposes)

        if (Physics.Raycast(myRay, out hit, rayLength))                                                                         //if the ray hits something within its range - raylength (float)...
        {
            if (hit.collider.tag == "Obstacle" || hit.collider.tag == "StaticBlock" || hit.collider.tag == "DestroyableBlock" || hit.collider.tag == "FireStone")  //if the ray hits an object tagged with these specified tags...
            {
                if(Input.GetKeyDown(KeyCode.LeftShift))                                                                         //...and if the specified key is pressed...
                {
                    audioSource.PlayOneShot(cantPushCrateSFX);                                                                  //plays an audio clip - the clip cannot be canceled if another is played
                }
                return false;                                                                                                   //the bool function is returned as false
            }
        }
        return true;                                                                                                            //the bool function is returned as true for any other possible if statements

    }
    bool EdgeCheck()                                                                                                            //the bool function that checks to see if there is an edge(tile) or not
    {
        Ray myEdgeRay = new Ray(crateEdgeCheck.transform.position, -transform.up);                                              //shoots a ray from below the determined object
        RaycastHit hit;

        Debug.DrawRay(myEdgeRay.origin, myEdgeRay.direction, Color.red);                                                        //draws a debug line of the raycast that was called previously (just for debugging purposes)   

        if (Physics.Raycast(myEdgeRay, out hit, rayLengthEdgeCheck))                                                            //if the ray hits something within its range; its rayLengthEdgeCheck (float)...
        {
            return true;                                                                                                        //the bool is returned as true
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))                                                                                //if the specified key is pressed...
        {
            audioSource.PlayOneShot(cantPushCrateSFX);                                                                          //plays an audio clip - the clip cannot be canceled if another is played
        }
        return false;                                                                                                           //if the ray doesnt hit anything, the bool is returned as false
    }

    // Resets the block to where it originally was placed
    public void resetPosition()
    {
        Debug.Log("Resetting block position");
        transform.position = startingPosition;
        Start();
    }

}
