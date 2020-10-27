using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TileMovement : MonoBehaviour
{
    [SerializeField]                                            //allows you to see and manipulate the variable within the Unity inspector if it's private (the variable below this line)
    public AudioClip[] clips;                                   //creates an array of audio clips 
    public GameObject torchFireIgniteSFX;                       //variable for a gameobject (either within the Unity hierarchy, or a prefab)
    public GameObject torchFireExtinguishSFX;                   //variable for a gameobject (either within the Unity hierarchy, or a prefab)

    private AudioSource audioSource;                            //establishes a variable for an audio source component
    public AudioClip pushCrateSFX;                              //variable for an audio clip

    Vector3 up = Vector3.zero,                                  //to make the object look up (north)
    right = new Vector3(0, 90, 0),                              //to make the object look right (east)
    down = new Vector3(0, 180, 0),                              //to make the object look down (south)
    left = new Vector3(0,270,0),                                //to make the player look left (west)
    currentDirection = Vector3.zero;                            //this will be its default state - the direction it'll face when you start the game

    Vector3 nextPos, destination, direction;

    private bool canMove;                                       //the bool is used to determine when the object can move

    float speed = 5f;                                           //the speed at which the object will move from its current position to the destination    
    float rayLength = 1f;                                       //the ray length for the tile movement
    float rayLengthEdgeCheck = 1f;                              //the ray length for the edge check

    public GameObject edgeCheck;

    private bool isWalking;                                     //the bool is used to determine when to play an object's animation
    private bool isPushing;                                     //the bool is used to determine when the object can move 
    private bool canPush;                                       //the bool is used to determine when the object can be pushed 
    private bool alreadyPlayedSFX;

    public Animator Anim;                                       //establishes a variable for an animator component

    public GameObject destroyedBlockParticle;                   //the particle effect that spawns when you break a block

    [SerializeField]                                            //allows you to see and manipulate the variable within the Unity inspector if it's private (the variable below this line)
    private TorchMeterStat torchMeterMoves;                     //variable for the specified script

    private void Awake()
    {
        torchMeterMoves.Initialize();                           //initializes the specified/external script so it can be accesed within this script
    }

    void Start()
    {
        currentDirection = up;                                  //the direction the object faces when you start the game
        nextPos = Vector3.forward;                              //the next block postion is equal to the object's forward axis (it will move along the direction it is facing)
        destination = transform.position;                       //the point where the object is currenlty at 
        audioSource = GetComponent<AudioSource>();              //sets the audio source variable to the object's audio source component (sets instance)
    }

    void Update()
    {
        Move();                                                                                           //calls the Move function stated below
        Push();                                                                                           //calls the Move function stated below
        Anim.SetBool("isWalking", isWalking);                                                             //sets the bool stated in this script to the corresponding bool stated within the object's animator
        Anim.SetBool("isPushing", isPushing);                                                             //sets the bool stated in this script to the corresponding bool stated within the object's animator

        if(Input.GetKeyDown(KeyCode.LeftArrow))                                                           //if the left arrow key is pressed... (this is just for debugging purposes)
        {
            torchMeterMoves.CurrentVal -= 1;                                                              //subract one from the torch meter's current value
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))                                                         //if the right arrow key is pressed...(this is just for debugging purposes)
        {
            torchMeterMoves.CurrentVal += 1;                                                              //add one to the torch meter's current value
        }

        if(torchMeterMoves.CurrentVal == 0 && !alreadyPlayedSFX)                                          //if the torch meter's current value is zero and if the audio clip hasn't been played yet...
        {
            Instantiate(torchFireExtinguishSFX, transform.position, transform.rotation);                  //play the audio clip (spawns an object prefab with an audio clip that plays on awake)
            alreadyPlayedSFX = true;                                                                      //the audio clip cannot be played again
        }
        if(torchMeterMoves.CurrentVal > 0)                                                                //when the torch meter's current value is greater than zero
        {
            alreadyPlayedSFX = false;                                                                     //the audio clip can be played again
        }

    }

    void Move()                                                                                           //the main moving function for the object
    {
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);//when the object starts moving, the object moves from its current position to the destination
        isWalking = false;

        if (Input.GetKeyDown(KeyCode.W))                                                                  //when a ceratin key is pressed...
        {
            nextPos = Vector3.forward;                                                                    //...the object is set to move along the stated axis
            currentDirection = up;                                                                        //sets the object to rotate towards the specified direction
            canMove = true;                                                                               //the object can move while the statement above is true
            isWalking = true;                                                                             //the object can perform the walking animation while the statement above is true

        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            nextPos = Vector3.back;
            currentDirection = down;
            canMove = true;
            isWalking = true;
          
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            nextPos = Vector3.right;
            currentDirection = right;
            canMove = true;
            isWalking = true;
         
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            nextPos = Vector3.left;
            currentDirection = left;
            canMove = true;
            isWalking = true;
            
        }

        if(Vector3.Distance(destination, transform.position) <= 0.00001f)                                //checks to see how big the distance is between the object's current position and the destination
        {
            transform.localEulerAngles = currentDirection;                                               //rotates the actual object to the current direction - the raycast will roatate with it, along the axis its facing
            if (canMove)
            {               
                if (Valid() && EdgeCheck())                                                              //if the bool functions below are returned as true
                {
                    Footstep();
                    destination = transform.position + nextPos;                                          //updates the destination by adding the next position to the object's current position
                    direction = nextPos;
                    torchMeterMoves.CurrentVal -= 1;
                    canMove = false;                                                                     //prevents the object from constantly moving towards the object's current direction     
                }           
            }    
        }
    }

    void Push()                                                                                          //the function that determines when the object can push another
    {
        if(Input.GetKey(KeyCode.W))                                                                      //when the specified key is pressed...
        {
            nextPos = Vector3.forward;                                                                   //...the object is set to move along the stated axis
            currentDirection = up;                                                                       //sets the object to rotate towards the specified direction
            canPush = true;                                                                              //the object can push another object while the statement above is true
            if (!Valid())                                                                                //if the bool function below is returned as false, then the object cannot move
            {
                canMove = false;
                if (Input.GetKeyDown(KeyCode.LeftShift))                                                 //when the bool function is returned as false, and you press a certain key...
                {
                    isPushing = true;                                                                    //the object can play its pushing animation
                }
                else
                {
                    isPushing = false;                                                                   //the object cannot play its pushing animation for any other possible statements
                }
            }
            
        }

        if (Input.GetKey(KeyCode.A))
        {
            nextPos = Vector3.left;
            currentDirection = left;
            canPush = true;
            if (!Valid())
            {
                canMove = false;
                if (Input.GetKeyDown(KeyCode.LeftShift))                                                                    
                {
                    isPushing = true;
                }
                else
                {
                    isPushing = false;
                }
            }
        }

        if (Input.GetKey(KeyCode.S))
        {
            nextPos = Vector3.back;
            currentDirection = down;
            canPush = true;
            if (!Valid())
            {
                canMove = false;
                if (Input.GetKeyDown(KeyCode.LeftShift))                                                                 
                {
                    isPushing = true;
                }
                else
                {
                    isPushing = false;
                }
            }
        }

        if (Input.GetKey(KeyCode.D))
        {
            nextPos = Vector3.right;
            currentDirection = right;
            canPush = true;
            if (!Valid())
            {
                canMove = false;
                if (Input.GetKeyDown(KeyCode.LeftShift))                                                                    
                {
                    isPushing = true;
                }
                else
                {
                    isPushing = false;
                }
            }
        }
    }

    bool Valid()                                                                                                                                //the bool function that checks to see if the next position is valid or not
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), transform.forward);                                                   //shoots a ray into the direction that the object is looking towards
        RaycastHit hit;

        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);                                                                                //shows a debug line of the raycast that was called previously (just for debugging purposes)

        if (Physics.Raycast(myRay, out hit, rayLength))                                                                                         //checks to see what the ray hit depending on its range - raylength
        {
            //if (hit.collider.tag == "Obstacle" || hit.collider.tag == "StaticBlock" || hit.collider.tag == "DestroyableBlock")                //this is just for future reference - ignore this line

            if (hit.collider.tag == "FireStone" && Input.GetKeyDown(KeyCode.Return) && canPush)                                                 //if the ray hits an object tagged "Firestone" and etc... (hold down correct WASD key and then press enter to interact with firestone)                
            {
                torchMeterMoves.CurrentVal = torchMeterMoves.MaxVal;                                                                            //set the torch meter to it max value (fill up the bar)
                Instantiate(torchFireIgniteSFX, transform.position, transform.rotation);                                                        //spwans the particle effect on the object's position and rotation
                isWalking = false;                                                                                                              //the object cannot play its walking animation while the statement above is true
                return false;                                                                                                                   //the bool function will return as false if the statement above is true
            }

            if (hit.collider.tag == "Obstacle" && canPush)                                                                                      //if the ray hits an object tagged "Obstacle" and etc... (hold down correct WASD key and then press left shift to push block)
            {   
                if(Input.GetKeyDown(KeyCode.LeftShift))                                                                                         //...and if the specified key is pressed... **this needs to be refined** 
                {
                    torchMeterMoves.CurrentVal -= 1;                                                                                            //subract one from the torch meter's current value
                }

                hit.collider.gameObject.GetComponent<BlockMovement>().MoveBlock();                                                              //calls the function from the hit object's script 
                isWalking = false;                                                                                                              //the object cannot play its walking animation while the statement above is true
                return false;                                                                                                                   //the bool function will return as false if the statement above is true
            }

            if (hit.collider.tag == "StaticBlock" && canPush && Input.GetKeyDown(KeyCode.LeftShift))                                            //if the ray hits an object tagged "StaticBlock" and etc... (hold down correct WASD key and then press left shift to try and push static block)  
            {
                Debug.Log("Cannot Push Static Block");                                                                                          //sends a debug message to the console (just for debugging purposes)
                hit.collider.gameObject.GetComponentInChildren<ObjectShakeController>().StartShake(0.25f, 0.25f);                               //calls the function from the script of the parent object's child                                 
                isWalking = false;                                                                                                              //the object cannot play its walking animation while the statement above is true
                return false;                                                                                                                   //the bool function will return as false if the statement above is true
            }

            if (hit.collider.tag == "DestroyableBlock" && canPush && Input.GetKeyDown(KeyCode.LeftShift))                                       //if the ray hits an object tagged "DestroyableBlock" and etc... (hold down correct WASD key and then press left shift to try and push destroyable block)
            {
                Debug.Log("Cannot Push Breakable Block");                                                                                       //sends a debug message to the console (just for debugging purposes)
                hit.collider.gameObject.GetComponentInChildren<ObjectShakeController>().StartShake(0.1f, 0.25f);                                //calls the function from the script of the hit object's child                                 
                isWalking = false;                                                                                                              //the object cannot play its walking animation while the statement above is true
                return false;                                                                                                                   //the bool function will return as false if the statement above is true
            }

            if (hit.collider.tag == "DestroyableBlock" && Input.GetKeyDown(KeyCode.Return) && canPush)                                          //if the ray hits an object tagged "DestroyableBlock" and etc... (hold down correct WASD key and then press enter to destroy block)  
            {
                Debug.Log("Destroyed Block");                                                                                                   //sends a debug message to the console (just for debugging purposes)
                torchMeterMoves.CurrentVal -= 2;                                                                                                //subract 2 from the torch meter
                Instantiate(destroyedBlockParticle, hit.collider.gameObject.transform.position, hit.collider.gameObject.transform.rotation);    //spawns the block destruction particle effect on the tagged object's position and rotation
                Destroy(hit.collider.gameObject);                                                                                               //destroys the tagged object
                isWalking = false;                                                                                                              //the object cannot play its walking animation while the statement above is true
                return false;                                                                                                                   //the bool function will return as false if the statement above is true
            }
            else
            {
                isWalking = false;                                                                                                               //the object cannot play its walking animation for any other possible if statements
                return false;                                                                                                                    //the bool function will return as false for any other possible if statements
            }
        }
        return true;                                                                                                                             //the bool function will return as true; this is needed for it to work)

    }
    bool EdgeCheck()                                                                                                                             //the bool function that checks to see if there is an edge(tile) or not
    {
        Ray myEdgeRay = new Ray(edgeCheck.transform.position, -transform.up);                                                                    //shoots a ray from below the determined object
        RaycastHit hit;

        Debug.DrawRay(myEdgeRay.origin, myEdgeRay.direction, Color.red);                                                                         //draws a debug line of the raycast that was called previously (just for debugging purposes)

        if (Physics.Raycast(myEdgeRay, out hit, rayLengthEdgeCheck))                                                                             //if the ray hits something within its range; its rayLengthEdgeCheck (float)...
        {
            if(hit.collider.tag == "MoveCameraBlock")                                                                                            //if the ray hits an object tagged with this specific tag...
            {
                torchMeterMoves.CurrentVal = torchMeterMoves.MaxVal;                                                                             //set the torch meter to it max value (fill up the bar)
                CameraController.instance.NextPuzzleView();                                                                                      //calls a function within the specified/external script
                return true;                                                                                                                     //the bool is returned as true
            }
            return true;                                                                                                                         //the bool function is returned as true
        }

        isWalking = false;                                                                                                                       //the player is not walking during any other possible if statement
        return false;                                                                                                                            //the bool function is returned as false for any other possible if statement
    }                                                                                           


    private void Footstep()                                                                                                                      //the function that plays the random audio clip
    {
        AudioClip clips = GetRandomClip();                                                                                                       //calls the random audio clip function below
        audioSource.PlayOneShot(clips);                                                                                                          //plays the audio clip (from start to end - without intturuption) through the object's audio source component
    }

    private AudioClip GetRandomClip()                                                                                                            //the function for getting a random audio clip within the array
    {
        return clips[UnityEngine.Random.Range(0, clips.Length)];                                                                                 //selects a random audio clip based on the size of the array (its length)
    }

}
