using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KimsCheckpoint : MonoBehaviour
{
    private GameObject player; // Player object
    Vector3 p; // Player position for debugging
    Vector3 blockPosition; // Block position

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        blockPosition = transform.position;
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

    /**
     * Resets the player's position back to the checkpoint
     **/
    public void resetPlayerPosition()
    {
        player.transform.position = blockPosition;
        player.GetComponent<KimsTileMovement>().setDestination(blockPosition);
    }




}
