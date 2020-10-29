using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetFireStone : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void resetFireStone()
    {
        Debug.Log("FireStone light enabled");
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.GetComponentInChildren<Light>().enabled = true;
        }
    }
}
