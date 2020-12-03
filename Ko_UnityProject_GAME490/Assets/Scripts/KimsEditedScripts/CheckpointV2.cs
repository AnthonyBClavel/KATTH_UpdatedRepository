using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointV2 : MonoBehaviour
{
    public int numMovements;
    private GameObject player; // Player object
    private IceMaterialScript iceMaterialScript;
    private TileMovementV2 tileMovementScript;
    private Animator playerAnimator;
    Vector3 p; // Player position for debugging
    Vector3 blockPosition; // Block position
    private bool hit; // True if we hit it before, false otherwise

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        blockPosition = transform.position;
        hit = false;

        iceMaterialScript = player.GetComponent<IceMaterialScript>();
        tileMovementScript = player.GetComponent<TileMovementV2>();
        playerAnimator = player.GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            p = player.transform.position;
            Debug.Log("Player Position: " + p);
        }
    }

    // Returns the number of movements for the checkpoint
    public int getNumMovements() { return numMovements; }

    /**
     * Resets the player's position back to the checkpoint
     **/
    public void resetPlayerPosition()
    {
        player.transform.position = blockPosition;
        tileMovementScript.setDestination(blockPosition);
    }

    public IEnumerator resetPlayerPositionWithDelay(float seconds)
    {
        iceMaterialScript.StartCoroutine("IncreaseAlpha_ColdUI");
        iceMaterialScript.StartCoroutine("ResetUIAlpha_ColdUI");
        iceMaterialScript.StartCoroutine("FadeMaterialToFullAlpha");
        iceMaterialScript.StartCoroutine("ResetPlayerMaterial");
        playerAnimator.enabled = false;
        tileMovementScript.enabled = false;
        yield return new WaitForSeconds(seconds);
        playerAnimator.enabled = true;
        player.transform.position = blockPosition;
        tileMovementScript.setDestination(blockPosition);
        tileMovementScript.ResetTorchMeter();
        tileMovementScript.enabled = true;
    }

    public void setCheckpoint()
    {
        hit = true;
    }

    public bool hitCheckpoint()
    {
        return hit;
    }


}
