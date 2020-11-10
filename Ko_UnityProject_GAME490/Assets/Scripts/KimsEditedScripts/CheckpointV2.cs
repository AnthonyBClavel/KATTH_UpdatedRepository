using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointV2 : MonoBehaviour
{
    public int numMovements;
    private GameObject player; // Player object
    Vector3 p; // Player position for debugging
    Vector3 blockPosition; // Block position
    private bool hit; // True if we hit it before, false otherwise

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        blockPosition = transform.position;
        hit = false;
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
        player.GetComponent<TileMovementV2>().setDestination(blockPosition);
    }

    public IEnumerator resetPlayerPositionWithDelay(float seconds)
    {
        player.GetComponent<IceMaterialScript>().StartCoroutine("FadeMaterialToFullAlpha", 1.5f);
        player.GetComponent<IceMaterialScript>().StartCoroutine("ResetPlayerMaterial", 1.5f);
        player.GetComponentInChildren<Animator>().enabled = false;
        player.GetComponentInChildren<TileMovementV2>().enabled = false;
        yield return new WaitForSeconds(seconds);
        player.GetComponentInChildren<Animator>().enabled = true;
        player.GetComponentInChildren<TileMovementV2>().enabled = true;
        player.transform.position = blockPosition;
        player.GetComponent<TileMovementV2>().setDestination(blockPosition);
        player.GetComponent<TileMovementV2>().ResetTorchMeter();
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
