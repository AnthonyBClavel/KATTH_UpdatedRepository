using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockMovementController : MonoBehaviour
{
    private float lerpDuration = 0.1f; // Original Value = 0.1f
    private float rayLength = 1f;

    private bool isFalling = false;
    private bool canMoveBlock = true;
    private bool hasUpdatedStartPosition = false;

    private GameObject edgeCheck;
    private GameObject emptyBlock;

    private Vector3 nextBlockPos;
    private Vector3 destination;
    private Vector3 startPosition;
    private Vector3 debugStartPosition;

    private IEnumerator moveBlockCoroutine;
    
    private TileMovementController playerScript;
    private AudioManager audioManagerScript;
    private GameManager gameManagerScript;


    // Returns or sets the value of lerpLength
    public float LerpDuration
    {
        get { return lerpDuration; }
        set { lerpDuration = value; }
    }

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Checks if the block can move - returns true if so, false otherwise
    public bool MoveBlock()
    {
        UpdateStartPosition();
        SetNextBlockPosition();

        // Checks for colliders and edges
        if (canMoveBlock && !ColliderCheck() && !EdgeCheck())
        {
            destination = transform.position + nextBlockPos;
            StartMoveBlockCoroutine();

            audioManagerScript.PlayPushCrateSFX();
            audioManagerScript.PlayCrateSlideSFX();
            canMoveBlock = false;
            return true;
        }

        return false;
    }

    // Move the block to its starting position
    public void ResetBlock()
    {
        StopAllCoroutines();

        if (emptyBlock != null) emptyBlock.SetActive(true);
        hasUpdatedStartPosition = false;
        canMoveBlock = true;

        Vector3 startPos = (gameManagerScript.isDebugging) ? debugStartPosition : startPosition;
        transform.position = startPos;
        destination = startPos;
    }

    // Updates the block's starting position - For Debugging Purposes ONLY
    private void UpdateStartPosition()
    {
        // Note: after restarting a puzzle, you can adjust the block's starting position
        // Note: the player has to touch the block in order to save its new starting position
        if (gameManagerScript.isDebugging && !hasUpdatedStartPosition)
        {
            debugStartPosition = transform.position;
            destination = debugStartPosition;
            hasUpdatedStartPosition = true;
        }
    }

    // Sets the block position and moves the edgeCheck to that position
    private void SetNextBlockPosition()
    {
        Vector3 playerDirection = playerScript.transform.eulerAngles;
        Vector3 blockPos = transform.position;

        switch (playerDirection.y)
        {
            case 0: // Looking North
                nextBlockPos = Vector3.forward;
                edgeCheck.transform.position = (blockPos + nextBlockPos);
                break;
            case 90: // Looking East
                nextBlockPos = Vector3.right;
                edgeCheck.transform.position = (blockPos + nextBlockPos);
                break;
            case 180: // Looking South
                nextBlockPos = Vector3.back;
                edgeCheck.transform.position = (blockPos + nextBlockPos);
                break;
            case 270: // Looking West
                nextBlockPos = Vector3.left;
                edgeCheck.transform.position = (blockPos + nextBlockPos);
                break;
            default:
                //Debug.Log("Unrecognizable direction");
                break;
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
            if (hit.collider == true)
            {
                audioManagerScript.PlayCantPushCrateSFX();
                return true;
            }
        }
        return false;
    }

    // Checks if the theres an edge at the next block position - returns true if so, false otherwise
    private bool EdgeCheck()
    {
        Ray myEdgeRay = new Ray(edgeCheck.transform.position, Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(myEdgeRay.origin, myEdgeRay.direction, Color.red);

        if (Physics.Raycast(myEdgeRay, out hit, rayLength))
        {
            // Blocks cannot move onto bridge tiles - counts as an "edge"
            if (hit.collider.tag == "BridgeTile" || hit.collider.name == "BridgeTile")
            {
                audioManagerScript.PlayCantPushCrateSFX();
                return true;
            }

            // If the ray hits anything, then there's no edge
            return false;
        }

        // If the ray doesn't hit anything, then there is an edge
        audioManagerScript.PlayCantPushCrateSFX();
        return true;
    }

    // Checks if there is a hole - returns true if so, false otherwise
    private bool HoleCheck()
    {
        Ray myRay = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        if (Physics.Raycast(myRay, out hit, rayLength))
        {
            GameObject colliderObject = hit.collider.gameObject;

            if (colliderObject.tag == "EmptyBlock" || colliderObject.name == "EmptyBlock")
            {
                emptyBlock = colliderObject;
                emptyBlock.SetActive(false);
                isFalling = true;

                // Note: falls into the hole if true
                destination = colliderObject.transform.position;
                StartMoveBlockCoroutine();
                return true;
            }
        }

        return false;
    }

    // Starts the coroutine that moves the block
    private void StartMoveBlockCoroutine()
    {
        if (moveBlockCoroutine != null)
            StopCoroutine(moveBlockCoroutine);

        moveBlockCoroutine = MoveBlockPosition(destination, lerpDuration);
        StartCoroutine(moveBlockCoroutine);
    }

    // Moves the block's position to another over a duration (endPosition = position to move to, duration = seconds)
    private IEnumerator MoveBlockPosition(Vector3 endPosition, float duration)
    {
        float time = 0f;
        Vector3 startPosition = transform.position;

        while (time < duration)
        {
            // Plays the crate thump SFX after reaching more than half its destination - ONLY when the block is falling
            if (isFalling && time > (duration / 2))
            {
                audioManagerScript.PlayCrateThumpSFX();
                isFalling = false;
            }

            transform.position = Vector3.MoveTowards(startPosition, endPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        canMoveBlock = HoleCheck() ? false : true;
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        gameManagerScript = FindObjectOfType<GameManager>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets them by looking at the names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "EdgeCheck")
                edgeCheck = child;
        }

        nextBlockPos = Vector3.forward;
        startPosition = transform.position;
        debugStartPosition = startPosition;
        destination = startPosition;

        lerpDuration = gameManagerScript.crateLerpDuration;
    }

}
