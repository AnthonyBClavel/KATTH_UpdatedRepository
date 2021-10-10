using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    private GameObject pushableBlocks;
    private GameObject breakableBlocks;
    private GameObject crystalBlocks;
    private GameObject firestones;
    private GameObject generators;
    private GameObject currentPuzzle;

    private TileMovementController playerScript;

    void Awake()
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

    // Resets all the blocks and game objects within the current puzzle (set float to zero for an instant reset)
    public void ResetPuzzle(float delayDuration)
    {
        currentPuzzle = playerScript.ReturnCurrentPuzzle();

        for (int i = 0; i < currentPuzzle.transform.childCount; i++)
        {
            GameObject child = currentPuzzle.transform.GetChild(i).gameObject;

            if (child.name == "PushableBlocks")
            {
                pushableBlocks = child;
                StartCoroutine(ResetPushableBlocks(delayDuration));
            }
            if (child.name == "BreakableBlocks")
            {
                breakableBlocks = child;
                StartCoroutine(ResetBreakableBlocks(delayDuration));
            }
            if (child.name == "CrystalBlocks")
            {
                crystalBlocks = child;
                //StartCoroutine(ResetCrystals(delayDuration));
            }
            if (child.name == "Firestones")
            {
                firestones = child;
                StartCoroutine(ResetFirestone(delayDuration));
            }
            if (child.name == "Generators")
            {
                generators = child;
                StartCoroutine(ResetGenerator(delayDuration));
            }
        }
    }

    // Resets the postion of each pushable block after delay
    private IEnumerator ResetPushableBlocks(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        Debug.Log("Number of pushable blocks: " + pushableBlocks.transform.childCount);
        for (int i = 0; i < pushableBlocks.transform.childCount; i++)
        {
            pushableBlocks.transform.GetChild(i).GetComponent<BlockMovementController>().ResetBlockPosition();
        }
    }

    // Sets all inactive breakable blocks to active after a delay
    private IEnumerator ResetBreakableBlocks(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        Debug.Log("Num of breakable blocks: " + breakableBlocks.transform.childCount);
        for (int i = 0; i < breakableBlocks.transform.childCount; i++)
        {
            breakableBlocks.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    // Resets the firestone after a delay
    private IEnumerator ResetFirestone(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        Debug.Log("Firestone light enabled");
        for (int i = 0; i < firestones.transform.childCount; i++)
        {
            firestones.transform.GetChild(i).GetComponentInChildren<Light>().enabled = true;
        }
    }

    // Resets the generator after a delay
    private IEnumerator ResetGenerator(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        Debug.Log("Generator has been reseted");
        for (int i = 0; i < generators.transform.childCount; i++)
        {
            generators.transform.GetChild(i).GetComponentInChildren<GeneratorScript>().TurnOffGenerator();
        }
    }

    // Sets private variables, objects, and components
    /*private void SetElements()
    {
        
    }*/

}
