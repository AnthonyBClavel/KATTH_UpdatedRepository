using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KimsBreakableBlocks : MonoBehaviour
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
        Debug.Log("Num of breakable blocks: " + transform.childCount);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }
}
