using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetBreakableBlocks : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Sets the inactive breakable blocks back to active immediately
    public void resetBlocks()
    {
        Debug.Log("Num of breakable blocks: " + transform.childCount);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    // Sets the inactive breakable blocks back to active after a certain amount of seconds
    public IEnumerator resetBlocksWithDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Debug.Log("Num of breakable blocks: " + transform.childCount);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }

}
