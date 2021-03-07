using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeMovementController : MonoBehaviour
{
    public GameObject blockToMoveTo;
    public int newDirection;

    private TileMovementController playerScript;

    private void Awake()
    {
        playerScript = FindObjectOfType<TileMovementController>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void MoveToNextBlock()
    {
        playerScript.setDestination(blockToMoveTo.transform.position);
    }

}
