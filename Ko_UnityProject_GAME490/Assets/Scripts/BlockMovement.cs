using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BlockMovement : MonoBehaviour
{
    Vector3 up = Vector3.zero,                                 
    right = new Vector3(0, 90, 0),                           
    down = new Vector3(0, 180, 0),                             
    left = new Vector3(0, 270, 0),                       
    currentDirection = Vector3.zero;                 

    Vector3 nextBlockPos, destination, direction, startingPosition;                              

    float speed = 10f;                                             
    float rayLength = 1f;                                      
    float rayLengthEdgeCheck = 1f;                              
    float rayFalleCheck = 1f;

    public GameObject crateEdgeCheck;                         

    private AudioSource audioSource;                           
    public AudioClip pushCrateSFX;                             
    public AudioClip cantPushCrateSFX; 

    void Start()
    {
        currentDirection = up;
        nextBlockPos = Vector3.forward;
        destination = transform.position;
        audioSource = GetComponent<AudioSource>(); 
        startingPosition = transform.position;
    }

    void Update()
    {                                                                                      
        FallCheck();                                                                                            
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime); 
    }

    public bool MoveBlock(Vector3 direction)
    {
        setDirection(direction);
        if (Vector3.Distance(destination, transform.position) <= 0.00001f)
        {
            if (Valid() && EdgeCheck())
            {
                destination = transform.position + nextBlockPos;
                direction = nextBlockPos;
                audioSource.PlayOneShot(pushCrateSFX);
                return true;
            }
        }
        return false;
    }

    void setDirection(Vector3 direction)
    {
        
        if (direction == up)
        {
            nextBlockPos = Vector3.forward;
            currentDirection = up;
            crateEdgeCheck.transform.position = (transform.position + nextBlockPos);
        }

        else if (direction == down)
        {
            nextBlockPos = Vector3.back;
            currentDirection = down;
            crateEdgeCheck.transform.position = (transform.position + nextBlockPos);
        }

        else if (direction == right)
        {
            nextBlockPos = Vector3.right;
            currentDirection = right;
            crateEdgeCheck.transform.position = (transform.position + nextBlockPos);
        }

        else if (direction == left)
        {
            nextBlockPos = Vector3.left;
            currentDirection = left;
            crateEdgeCheck.transform.position = (transform.position + nextBlockPos);
        }
    }

    bool Valid()                                                                                                                
    {
        Ray myRay = new Ray(transform.position, nextBlockPos);                                                             
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);                                                               

        if (Physics.Raycast(myRay, out hit, rayLength))                                                                         
        {
            string tag = hit.collider.tag;
            if (tag == "Obstacle" | tag == "StaticBlock" | tag == "DestroyableBlock" | tag == "FireStone" | tag == "Generator" | tag == "InvisibleBlock") 
            {
                audioSource.PlayOneShot(cantPushCrateSFX);                                                                 
                return false;                                                                                                  
            }
        }
        return true;                                                                                                           

    }
    bool EdgeCheck()                                                                                                           
    {
        Ray myEdgeRay = new Ray(crateEdgeCheck.transform.position, -transform.up);                                              
        RaycastHit hit;
        Debug.DrawRay(myEdgeRay.origin, myEdgeRay.direction, Color.red);

        if (Physics.Raycast(myEdgeRay, out hit, rayLengthEdgeCheck))
        {
            string tag = hit.collider.tag;
            //Prevents block from moving onto bridge
            if (tag == "ResetCameraBool")
            {
                audioSource.PlayOneShot(cantPushCrateSFX);
                return false;
            }
            return true;
        }
        
        audioSource.PlayOneShot(cantPushCrateSFX);                                                                               
        return false;                                                                                                      
    }

    // Checks to see if the object can "fall" or not 
    void FallCheck()                                                                                                            
    {
        Ray myFallCheckRay = new Ray(transform.position, -transform.up);                                                        
        RaycastHit hit;
        Debug.DrawRay(myFallCheckRay.origin, myFallCheckRay.direction, Color.red);                                             

        if (Physics.Raycast(myFallCheckRay, out hit, rayFalleCheck) && hit.collider.tag == "EmptyBlock")
             destination = hit.collider.gameObject.transform.position;                                                                                                                                                                                        
    }

    // Resets the block to where it originally was placed
    public void resetPosition()
    {
        transform.position = startingPosition;
        Start();
    }
}
