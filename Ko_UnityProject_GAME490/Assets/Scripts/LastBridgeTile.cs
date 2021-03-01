using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastBridgeTile : MonoBehaviour
{
    private TileMovementController tileMovementScript;

    private bool hit; // True if we hit it before, false otherwise

    // Start is called before the first frame update
    void Start()
    {     
        tileMovementScript = FindObjectOfType<TileMovementController>();
        hit = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setLastBridgeTile()
    {
        hit = true;
    }

    public bool hitLastBridgeTile()
    {
        return hit;
    }


}
