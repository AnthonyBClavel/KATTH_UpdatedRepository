using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    private bool hasLitAllCrystals = false;

    private GameObject staticBlocks;
    private GameObject pushableBlocks;
    private GameObject breakableBlocks;
    private GameObject firestones;
    private GameObject generators;
    private GameObject deathScreen;

    private TileMovementController playerScript;
    private AudioManager audioManagerScript;
    private GameManager gameManagerScript;
    private GameHUD gameHUDScript;
    private TorchMeter torchMeterScript;

    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Updates the parent objects - the game objects that hold puzzle blocks (trees, rocks, crates, crystals, etc.)
    public void UpdateParentObjects()
    {
        GameObject currentPuzzle = playerScript.CurrentPuzzle;

        staticBlocks = null;
        pushableBlocks = null;
        breakableBlocks = null;
        firestones = null;
        generators = null;

        for (int i = 0; i < currentPuzzle.transform.childCount; i++)
        {
            GameObject child = currentPuzzle.transform.GetChild(i).gameObject;

            if (child.activeSelf)
            {
                switch (child.name)
                {
                    case ("StaticBlocks"):
                        staticBlocks = child;
                        break;
                    case ("PushableBlocks"):
                        pushableBlocks = child;
                        break;
                    case ("BreakableBlocks"):
                        breakableBlocks = child;
                        break;
                    case ("Firestones"):
                        firestones = child;
                        break;
                    case ("Generators"):
                        generators = child;
                        break;
                    default:
                        break;
                }
            }
        }
        //Debug.Log("The puzzle's parent objects have been updated");
    }

    // Resets both the player and the puzzle (delay = seconds, set float to zero for instant reset)
    public void ResetPuzzle(float delay)
    {
        GameObject currentCheckpoint = playerScript.CurrentCheckpoint;
        currentCheckpoint.GetComponent<CheckpointManager>().ResetPlayer(delay);
        DestroyAllPuzzleParticles();

        // Avoids reseting the puzzle elements if the following argument is true...
        if (gameManagerScript.canDeathScreen && torchMeterScript.CurrentVal <= 0 && !deathScreen.activeSelf)
            return;

        if (staticBlocks != null)
            StartCoroutine(ResetCrystals(delay));
        if (pushableBlocks != null)
            StartCoroutine(ResetPushableBlocks(delay));
        if (breakableBlocks != null)
            StartCoroutine(ResetBreakableBlocks(delay));
        if (firestones != null)
            StartCoroutine(ResetFirestone(delay));
        if (generators != null)
            StartCoroutine(ResetGenerator(delay));
    }

    // // Checks if all crystals within a puzzle are lit - plays the chimeSFX sfx if so
    public void AllCrystalsLitCheck()
    {
        if (!hasLitAllCrystals)
        {
            bool allCrystalsLit = true;

            // If the light intesnity within any crystal is not greater than the minLightIntesity, then allCrystalsLit is set to false
            for (int i = 0; i < staticBlocks.transform.childCount; i++)
            {
                GameObject child = staticBlocks.transform.GetChild(i).gameObject;

                if (child.activeSelf && child.name.Contains("Crystal") && child.GetComponent<Crystal>().CrystalLitCheck() == false)
                    allCrystalsLit = false;
            }

            // If the light intensity within all crystals is greater than the minLightIntesity
            if (allCrystalsLit)
            {
                for (int i = 0; i < staticBlocks.transform.childCount; i++)
                {
                    GameObject child = staticBlocks.transform.GetChild(i).gameObject;

                    if (child.activeSelf && child.name.Contains("Crystal"))
                        child.GetComponent<Crystal>().LeaveCrystalLightOn();
                }

                audioManagerScript.PlayChimeSFX();
                hasLitAllCrystals = true;
            }
        }
    }

    // Checks if there's a generator within the puzzle - resets the generator if so
    public void ResetGeneratorCheck()
    {
        if (generators != null)
        {
            for (int i = 0; i < generators.transform.childCount; i++)
            {
                generators.transform.GetChild(i).GetComponent<Generator>().FadeOutGeneratorLoop(0f);
            }
        }
    }

    // Destroys all of the puzzle-related particle effects that are active
    private void DestroyAllPuzzleParticles()
    {
        // NOTE: All puzzle-related particles are instantiated as children of the game manager
        foreach (Transform child in gameManagerScript.transform)
        {
            Destroy(child.gameObject);
        }
    }

    // Resets the postion of each pushable block after a delay
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

        //Debug.Log("Number of breakable blocks: " + breakableBlocks.transform.childCount);
        for (int i = 0; i < breakableBlocks.transform.childCount; i++)
        {
            breakableBlocks.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    // Resets the crystals after a delay
    public IEnumerator ResetCrystals(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        //Debug.Log("Number of crystal blocks: " + staticBlocks.transform.childCount);
        for (int i = 0; i < staticBlocks.transform.childCount; i++)
        {
            GameObject child = staticBlocks.transform.GetChild(i).gameObject;

            if (child.activeSelf && child.name.Contains("Crystal"))
                child.GetComponent<Crystal>().ResetCrystalLight();

            hasLitAllCrystals = false;
        }
    }

    // Resets the firestone after a delay
    private IEnumerator ResetFirestone(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        //Debug.Log("Firestone has been reseted");
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

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        gameManagerScript = FindObjectOfType<GameManager>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        torchMeterScript = FindObjectOfType<TorchMeter>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game object by looking at names of children
        for (int i = 0; i < gameHUDScript.transform.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.GetChild(i).gameObject;

            if (child.name == "OptionalDeathScreen")
                deathScreen = child;
        }
    }

}
