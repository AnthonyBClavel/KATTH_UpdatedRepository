using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetGeneratorBlocks : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void resetGenerator()
    {
        Debug.Log("Generator has been reseted");
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.GetComponentInChildren<GeneratorScript>().TurnOffGenerator();
        }
    }
    public IEnumerator resetGeneratorWithDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Debug.Log("Generator has been reseted");
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.GetComponentInChildren<GeneratorScript>().TurnOffGenerator();
        }
    }
}
