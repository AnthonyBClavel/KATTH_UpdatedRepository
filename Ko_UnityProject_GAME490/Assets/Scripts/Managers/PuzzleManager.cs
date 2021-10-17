using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    private bool canPlayChimeSFX = true;

    private GameObject staticBlocks;
    private GameObject pushableBlocks;
    private GameObject breakableBlocks;
    private GameObject firestones;
    private GameObject generators;
    private GameObject currentPuzzle;

    private TileMovementController playerScript;
    private AudioManager audioManagerScript;

    void Awake()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        audioManagerScript = FindObjectOfType<AudioManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Plays the chimeSFX when all crystals in a puzzle are lit
    public void PlayAllCrystalsLitSFX()
    {
        if (canPlayChimeSFX)
        {
            audioManagerScript.PlayChimeSFX();
            canPlayChimeSFX = false;
        }
    }

    // Resets all the blocks and game objects within the current puzzle (set float to zero for an instant reset)
    public void ResetPuzzle(float delayDuration)
    {
        currentPuzzle = playerScript.CurrentPuzzle;

        for (int i = 0; i < currentPuzzle.transform.childCount; i++)
        {
            GameObject child = currentPuzzle.transform.GetChild(i).gameObject;
            string childName = child.name;

            if (child.activeSelf)
            {
                if (childName == "StaticBlocks")
                {
                    staticBlocks = child;
                    StartCoroutine(ResetCrystals(delayDuration));
                }
                if (childName == "PushableBlocks")
                {
                    pushableBlocks = child;
                    StartCoroutine(ResetPushableBlocks(delayDuration));
                }
                if (childName == "BreakableBlocks")
                {
                    breakableBlocks = child;
                    StartCoroutine(ResetBreakableBlocks(delayDuration));
                }
                if (childName == "Firestones")
                {
                    firestones = child;
                    StartCoroutine(ResetFirestone(delayDuration));
                }
                if (childName == "Generators")
                {
                    generators = child;
                    StartCoroutine(ResetGenerator(delayDuration));
                }
            }
        }
    }

    // Fades out the generator loop (generator is reseted after the its audio fades out)
    public void ResetGeneratorCheck()
    {
        currentPuzzle = playerScript.CurrentPuzzle;

        for (int i = 0; i < currentPuzzle.transform.childCount; i++)
        {
            GameObject child = currentPuzzle.transform.GetChild(i).gameObject;

            if (child.activeSelf && child.name == "Generators")
            {
                generators = child;

                for (int j = 0; j < generators.transform.childCount; j++)
                {
                    generators.transform.GetChild(j).GetComponent<Generator>().FadeOutGeneratorLoop();
                }
            }
        }
    }

    // Resets the postion of each pushable block after delay
    private IEnumerator ResetPushableBlocks(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        //Debug.Log("Number of pushable blocks: " + pushableBlocks.transform.childCount);
        for (int i = 0; i < pushableBlocks.transform.childCount; i++)
        {
            pushableBlocks.transform.GetChild(i).GetComponent<BlockMovementController>().ResetBlockPosition();
        }
    }

    // Sets all inactive breakable blocks to active after a delay
    private IEnumerator ResetBreakableBlocks(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        //Debug.Log("Num of breakable blocks: " + breakableBlocks.transform.childCount);
        for (int i = 0; i < breakableBlocks.transform.childCount; i++)
        {
            breakableBlocks.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    // Resets the crystals after a delay
    public IEnumerator ResetCrystals(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        //Debug.Log("Num of crystal blocks: " + staticBlocks.transform.childCount);
        for (int i = 0; i < staticBlocks.transform.childCount; i++)
        {
            GameObject child = staticBlocks.transform.GetChild(i).gameObject;

            if (child.activeSelf && child.name.Contains("Crystal"))
                child.GetComponent<Crystal>().ResetCrystalLight();

            canPlayChimeSFX = true;
        }
    }

    // Resets the firestone after a delay
    private IEnumerator ResetFirestone(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        //Debug.Log("Firestone light enabled");
        for (int i = 0; i < firestones.transform.childCount; i++)
        {
            firestones.transform.GetChild(i).GetComponentInChildren<Light>().enabled = true;
        }
    }

    // Resets the generator after a delay
    private IEnumerator ResetGenerator(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        //Debug.Log("Generator has been reseted");
        for (int i = 0; i < generators.transform.childCount; i++)
        {
            generators.transform.GetChild(i).GetComponent<Generator>().TurnOffGenerator();
        }
    }

    // Sets private variables, objects, and components
    /*private void SetElements()
    {
        
    }*/

}
