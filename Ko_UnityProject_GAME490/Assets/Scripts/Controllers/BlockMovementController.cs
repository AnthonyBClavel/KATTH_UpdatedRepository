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

        if (!canMoveBlock || ColliderCheck() || EdgeCheck()) return false;

        audioManagerScript.PlayPushCrateSFX();
        audioManagerScript.PlayCrateSlideSFX();

        destination = transform.position + nextBlockPos;
        canMoveBlock = false;

        StartMoveBlockCoroutine();
        return true;
    }

    // Move the block to its starting position
    public void ResetBlock()
    {
        StopAllCoroutines();

        destination = (gameManagerScript.isDebugging) ? debugStartPosition : startPosition;
        transform.position = destination;

        if (emptyBlock != null) emptyBlock.SetActive(true);
        hasUpdatedStartPosition = false;
        canMoveBlock = true;
    }

    // Sets the next block position
    private void SetNextBlockPosition()
    {
        Vector3 playerDirection = playerScript.transform.eulerAngles;

        switch (playerDirection.y)
        {
            case 0: // Looking North
                nextBlockPos = Vector3.forward;
                break;
            case 90: // Looking East
                nextBlockPos = Vector3.right;
                break;
            case 180: // Looking South
                nextBlockPos = Vector3.back;
                break;
            case 270: // Looking West
                nextBlockPos = Vector3.left;
                break;
            default:
                //Debug.Log("Unrecognizable direction");
                break;
        }
    }

    // Updates the block's starting position - For Debugging Purposes ONLY
    // Note: after restarting a puzzle, you can adjust the block's starting position
    private void UpdateStartPosition()
    {
        if (!gameManagerScript.isDebugging || hasUpdatedStartPosition) return;

        debugStartPosition = transform.position;
        destination = debugStartPosition;
        hasUpdatedStartPosition = true;
    }

    // Checks for a collider at the next block position - return true if so, false otherwise
    private bool ColliderCheck()
    {
        Ray myRay = new Ray(transform.position, nextBlockPos);
        //RaycastHit hit;
        //Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        if (!Physics.Raycast(myRay, rayLength)) return false;

        audioManagerScript.PlayCantPushCrateSFX();
        return true;
    }

    // Checks for an edge at the next block position - returns true if so, false otherwise
    private bool EdgeCheck()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, -0.6f, 0), nextBlockPos);
        RaycastHit hit;
        //Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        if (!Physics.Raycast(myRay, out hit, rayLength) || hit.collider.CompareTag("BridgeTile") || hit.collider.name.Contains("BridgeTile"))
        {
            audioManagerScript.PlayCantPushCrateSFX();
            return true;
        }

        return false;
    }

    // Checks for a hole at the block's current position - returns true and falls into the hole if so, false otherwise
    private bool HoleCheck()
    {
        Ray myRay = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        //Debug.DrawRay(myRay.origin, myRay.direction, Color.red);

        if (Physics.Raycast(myRay, out hit, rayLength) && hit.collider.CompareTag("EmptyBlock"))
        {
            emptyBlock = hit.collider.gameObject;
            emptyBlock.SetActive(false);

            destination = emptyBlock.transform.position;
            isFalling = true;

            StartMoveBlockCoroutine();
            return true;
        }

        return false;
    }

    // Starts the coroutine that moves the block
    private void StartMoveBlockCoroutine()
    {
        if (moveBlockCoroutine != null) StopCoroutine(moveBlockCoroutine);

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
        nextBlockPos = Vector3.forward;
        startPosition = transform.position;
        debugStartPosition = startPosition;
        destination = startPosition;

        lerpDuration = gameManagerScript.crateLerpDuration;
    }

}
