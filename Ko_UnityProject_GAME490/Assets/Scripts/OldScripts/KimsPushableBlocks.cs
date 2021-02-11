using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KimsPushableBlocks : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void resetBlocks()
    {
        Debug.Log("Num of pushable blocks: " + transform.childCount);
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            child.GetComponent<BlockMovementController>().resetPosition();
        }
    }
}
