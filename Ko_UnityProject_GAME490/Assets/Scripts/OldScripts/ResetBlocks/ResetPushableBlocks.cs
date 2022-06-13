using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPushableBlocks : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // Resets the postion of the pushable blocks immediately
    public void resetBlocks()
    {
        Debug.Log("Num of pushable blocks: " + transform.childCount);
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            //child.GetComponent<BlockMovementController>().ResetBlockPosition();
        }
    }

    // Resets the postion of the pushbale blocks after a certain amount of seconds
    public IEnumerator resetBlocksWithDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Debug.Log("Num of pushable blocks: " + transform.childCount);
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            //child.GetComponent<BlockMovementController>().ResetBlockPosition();
        }
    }

}
