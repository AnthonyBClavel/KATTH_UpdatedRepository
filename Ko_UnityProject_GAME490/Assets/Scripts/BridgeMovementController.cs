using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeMovementController : MonoBehaviour
{
    public GameObject blockToMoveTo;
    public bool hasSteppedOnTile = false;

    [Header("0 = up, 90 = right, 180 = down, 270 = left")]
    public int newDirection;

    private TileMovementController playerScript;

    void Awake()
    {
        playerScript = FindObjectOfType<TileMovementController>();
    }

    // Sets a destination for the player to move towards
    public void MoveToNextBlock()
    {
        playerScript.setDestination(blockToMoveTo.transform.position);
    }

}
