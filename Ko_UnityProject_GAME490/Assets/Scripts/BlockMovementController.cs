using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BlockMovementController : MonoBehaviour
{
    private float lerpLength;
    private float rayLength = 1f;

    private bool canMoveBlock = true;
    private bool canPlayThumpSFX = false;

    private GameObject edgeCheck;
    private GameObject emptyBlock;

    private Vector3 nextBlockPos;
    private Vector3 destination;
    private Vector3 originalPosition;

    Vector3 up = Vector3.zero, // Look North
    right = new Vector3(0, 90, 0), // Look East
    down = new Vector3(0, 180, 0), // Look South
    left = new Vector3(0, 270, 0); // Look West

    private IEnumerator moveBlockCoroutine;                                      
    private TileMovementController playerScript;
    private AudioManager audioManagerScript;
    private GameManager gameManagerScript;

    void Awake()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        gameManagerScript = FindObjectOfType<GameManager>();

        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        nextBlockPos = Vector3.forward;
        destination = transform.position;
        originalPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        DebuggingCheck();
    }

    // Moves the block to its original position
    public void ResetBlockPosition()
    {
        StopAllCoroutines();
        transform.position = originalPosition;
        destination = originalPosition;
        canMoveBlock = true;

        if (emptyBlock != null)
            emptyBlock.SetActive(true);
    }

    // Checks of the block can move - returns true if so, false otherwise
    public bool MoveBlock()
    {
        /*** Old Movement Code ***/
        //transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        // If the block is within 0.00001 units of its destination
        if (Vector3.Distance(destination, transform.position) <= 0.00001f && canMoveBlock)
        {
            SetNextBlockPosition();

            // Checks for colliders and edges
            if (!ColliderCheck() && !EdgeCheck())
            {              
                destination = transform.position + nextBlockPos;
                StartMoveBlockCoroutine();
                audioManagerScript.PlayPushCrateSFX();
                audioManagerScript.PlayCrateSlideSFX();
                canMoveBlock = false;
                return true;
            }
        }
        return false;
    }

    // Sets the position to move to and moves the edgeCheck to that position
    private void SetNextBlockPosition()
    {
        Vector3 playerDirection = playerScript.transform.eulerAngles;

        if (playerDirection == up)
        {
            nextBlockPos = Vector3.forward;
            edgeCheck.transform.position = (transform.position + nextBlockPos);
        }

        else if (playerDirection == down)
        {
            nextBlockPos = Vector3.back;
            edgeCheck.transform.position = (transform.position + nextBlockPos);
        }

        else if (playerDirection == left)
        {
            nextBlockPos = Vector3.left;
            edgeCheck.transform.position = (transform.position + nextBlockPos);
        }

        else if (playerDirection == right)
        {
            nextBlockPos = Vector3.right;
            edgeCheck.transform.position = (transform.position + nextBlockPos);
        }
    }

    // Checks for colliders - return true if so, false otherwise
    private bool ColliderCheck()                                                                                                                
    {
        Ray myRay = new Ray(transform.position, nextBlockPos);                                                             
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);                                                           

        if (Physics.Raycast(myRay, out hit, rayLength))                                                                         
        {
            // If the ray hits anything, then there's a collider
            if (hit.collider == true)
            {
                audioManagerScript.PlayCantPushCrateSFX();
                return true;
            }
        }

        // If the ray doesn't anything, then there's no collider
        return false;
    }

    // Determines where the block can't move to - returns true if there's an edge, false otherwise
    private bool EdgeCheck()                                                            
    {
        Ray myEdgeRay = new Ray(edgeCheck.transform.position, Vector3.down);                                  
        RaycastHit hit;
        Debug.DrawRay(myEdgeRay.origin, myEdgeRay.direction, Color.red);

        if (Physics.Raycast(myEdgeRay, out hit, rayLength))
        {
            string name = hit.collider.name;

            if (name == "BridgeTile") // Prevents the block from moving onto a bridge tile
            {
                audioManagerScript.PlayCantPushCrateSFX();
                return true;
            }
            else
                return false;
        }

        // If the ray doesn't hit anything, then there's an edge
        audioManagerScript.PlayCantPushCrateSFX();
        return true;                                                                                                      
    }

    // Checks if the block can fall into a hole - returns true if so, false otherwise
    private bool FallCheck()                                                                                                       
    {
        Ray myRay = new Ray(transform.position, Vector3.down);                                                        
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);                                             

        if (Physics.Raycast(myRay, out hit, rayLength))
        {
            GameObject colliderObject = hit.collider.gameObject;
            string tag = colliderObject.tag;

            if (tag == "EmptyBlock")
            {
                destination = colliderObject.transform.position;
                StartMoveBlockCoroutine();
                emptyBlock = colliderObject;
                emptyBlock.SetActive(false);
                canPlayThumpSFX = true;
                return true;
            }
        }
        return false;
    }

    // Starts the coroutine for moving the block
    private void StartMoveBlockCoroutine()
    {
        if (moveBlockCoroutine != null)
            StopCoroutine(moveBlockCoroutine);

        moveBlockCoroutine = MoveBlockPosition(destination, lerpLength);
        StartCoroutine(moveBlockCoroutine);
    }

    // Moves the block's position to a new position over a duration (endPosition = position to move to, duration = seconds)
    private IEnumerator MoveBlockPosition(Vector3 endPosition, float duration)
    {
        float time = 0f;
        Vector3 startPosition = transform.position;

        while (time < duration)
        {
            // Checks if the crateThumpSFX can be played (only after the block fell = after reaching half its destination)
            if (canPlayThumpSFX && time > (duration / 2))
            {
                audioManagerScript.PlayCrateThumpSFX();
                canPlayThumpSFX = false;
            }

            transform.position = Vector3.MoveTowards(startPosition, endPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;

        if (FallCheck())
            canMoveBlock = false;
        else
            canMoveBlock = true;
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "EdgeCheck")
                edgeCheck = child;
        }

        lerpLength = gameManagerScript.crateLerpLength;
    }

    // Updates the lerp length if the value changes
    private void DebuggingCheck()
    {
        if (gameManagerScript.isDebugging)
        {
            if (lerpLength != gameManagerScript.crateLerpLength)
                lerpLength = gameManagerScript.crateLerpLength;
        }
    }

}
