using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Linq;

public class PuzzleManager : MonoBehaviour
{
    private int? puzzleNumber;
    private int? bridgeNumber;
    private bool hasLitAllCrystals = false;

    private GameObject currentPuzzle;
    private GameObject currentBridge;
    private GameObject currentCheckpoint;

    private GameObject staticBlocks;
    private GameObject pushableBlocks;
    private GameObject breakableBlocks;
    private GameObject firestones;
    private GameObject generators;
    private GameObject deathScreen;

    public GameObject[] particleEffects;
    private ParticleSystem.MainModule treeHitparticleSystem;

    private GameHUD gameHUDScript;
    private TorchMeter torchMeterScript;
    private CameraController cameraScript;
    private GameManager gameManagerScript;
    private AudioManager audioManagerScript;
    private TileMovementController playerScript;
    private CheckpointManager checkpointManagerScript;

    public int? PuzzleNumber
    {
        get { return puzzleNumber; }
        set { puzzleNumber = value; }
    }

    public int? BridgeNumber
    {
        get { return bridgeNumber; }
        set { bridgeNumber = value; }
    }

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Resets both the puzzle and the player after a delay (duration = seconds)
    // Note: no delay will be added if the duration parameter is not set
    public void ResetPuzzle(float duration = 0f)
    {
        checkpointManagerScript.ResetPlayer(duration);
        DestroyAllPuzzleParticles();

        // The puzzle blocks don't reset during the death screen
        if (gameManagerScript.canDeathScreen && !playerScript.CanRestartPuzzle) return;
        ResetPuzzleBlocks(duration);
    }

    // Resets all of the blocks within a puzzle after a delay (duration = seconds)
    // Note: no delay will be added if the duration parameter is not set
    private void ResetPuzzleBlocks(float duration = 0f)
    {
        StartCoroutine(ResetPushableBlocks(duration));
        StartCoroutine(ResetBreakableBlocks(duration));
        StartCoroutine(ResetCrystals(duration));
        StartCoroutine(ResetFirestone(duration));
        StartCoroutine(ResetGenerator(duration));
    }

    // Sets all parent objects equal to null
    private void ResetParentObjects()
    {
        staticBlocks = null;
        pushableBlocks = null;
        breakableBlocks = null;
        firestones = null;
        generators = null;
    }

    // Updates the parent objects that hold the puzzle blocks (trees, rocks, crates, crystals, etc.)
    public void UpdateParentObjects()
    {
        ResetParentObjects();

        foreach (Transform childTransform in currentPuzzle.transform)
        {
            GameObject child = childTransform.gameObject;

            if (!child.activeInHierarchy)
                continue;

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
                    //Debug.Log("Unrecognizable child name");
                    break;
            }
        }
        //Debug.Log("Updated parent objects");
    }

    // Sets the current checkpoint
    public void SetCurrentCheckpoint(GameObject checkpoint)
    {
        currentCheckpoint = checkpoint;
        currentPuzzle = currentCheckpoint.transform.parent.parent.gameObject;
        puzzleNumber = ConvertObjectNameToNumber(currentPuzzle);

        checkpointManagerScript = currentCheckpoint.GetComponent<CheckpointManager>();
        checkpointManagerScript.SetSavedBlockPosition(); // Note: player rotation is saved in this method

        torchMeterScript.MaxVal = checkpointManagerScript.MaxTileMoves; // The max tile moves for the puzzle
        torchMeterScript.ResetTorchMeterElements();

        cameraScript.HasMovedPuzzleView = false;
        //ResetPuzzleBlocks();
        UpdateParentObjects();
    }

    // Sets the current bridge
    public void SetCurrentBridge(GameObject bridge)
    {
        currentBridge = bridge;
        bridgeNumber = ConvertObjectNameToNumber(currentBridge);
    }

    // Instantiates a particle effect on an object
    // Note: no offset will be added to the object's y position if the float parameter is not set
    public void InstantiateParticleEffect(GameObject theObject, string particleName, float? offsetPosY = 0f)
    {
        Vector3 offsetPos = new Vector3(0f, offsetPosY ?? 0f, 0f);
        GameObject particle = ParticleEffect(particleName);

        // Sets the particle's color to the tree shader's color if applicable
        if (particle.name == "TreeHitParticle")
        {          
            Material treeShader = theObject.transform.GetChild(0).GetComponent<MeshRenderer>().material;
            treeHitparticleSystem.startColor = treeShader.GetColor("Color_6566A18B"); // The color component's reference name in shader
        }

        GameObject particleEffect = Instantiate(particle, theObject.transform.position + offsetPos, theObject.transform.rotation);
        particleEffect.transform.parent = transform;
    }

    // Returns the desired particle effect within the array
    private GameObject ParticleEffect(string particleName)
    {
        foreach (GameObject child in particleEffects)
        {
            if (child.name == particleName)
                return child;
        }

        return new GameObject();
    }

    // Destroys all active particle effects within a puzzle
    private void DestroyAllPuzzleParticles()
    {
        // Note: all particles are instantiated as children of the puzzle manager
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }

    // Converts the name of an object to an integer - removes all letters in the name
    private int? ConvertObjectNameToNumber(GameObject theObject)
    {
        // If the object has a number in its name
        if (theObject.name.Any(char.IsDigit))
        {
            string newObjectName = Regex.Replace(theObject.name, "[A-Za-z ]", "");
            return int.Parse(newObjectName);
        }

        return null;
    }

    // Checks if all crystals within a puzzle are lit - play the chime sfx if so
    // Note: crystalBlocks is the parent object that holds the crystals
    public void AllCrystalsLitCheck(GameObject crystalBlocks)
    {
        if (hasLitAllCrystals) return;
        bool allCrystalsLit = true;

        foreach (Transform child in crystalBlocks.transform)
        {
            // If the light intesnity within ANY crystal is NOT greater than the minLightIntesity
            if (child.gameObject.activeInHierarchy && child.name.Contains("Crystal") && !child.GetComponent<Crystal>().LitCheck())
                allCrystalsLit = false;
        }

        if (allCrystalsLit)
        {
            foreach (Transform child in crystalBlocks.transform)
            {          
                if (child.gameObject.activeInHierarchy && child.name.Contains("Crystal"))
                    child.GetComponent<Crystal>().SetMaxIntensity();
            }

            audioManagerScript.PlayChimeSFX();
            hasLitAllCrystals = true;
        }
    }

    // Checks to fade out the audio for all generators
    public void FadeOutGenAudioCheck()
    {
        if (generators == null) return;

        // Note: the generator will turn off after its audio fades to zero
        foreach (Transform child in generators.transform)
            child.GetComponent<Generator>().FadeOutGeneratorAudio();
    }

    // Checks to reset the postion for all pushable blocks after a delay (duration = seconds)
    private IEnumerator ResetPushableBlocks(float duration)
    {
        if (pushableBlocks == null) yield break;
        else if (duration != 0) yield return new WaitForSeconds(duration);

        foreach (Transform child in pushableBlocks.transform)
            child.GetComponent<BlockMovementController>().ResetBlock();
    }

    // Checks to set all breakable blocks active after a delay (duration = seconds)
    private IEnumerator ResetBreakableBlocks(float duration)
    {
        if (breakableBlocks == null) yield break;
        else if (duration != 0) yield return new WaitForSeconds(duration);

        foreach (Transform child in breakableBlocks.transform)
            child.gameObject.SetActive(true); 
    }

    // Checks to reset all crystals after a delay (duration = seconds)
    public IEnumerator ResetCrystals(float duration)
    {
        if (staticBlocks == null) yield break;
        else if (duration != 0) yield return new WaitForSeconds(duration);

        foreach (Transform child in staticBlocks.transform)
        {
            if (child.gameObject.activeInHierarchy && child.name.Contains("Crystal"))
                child.GetComponent<Crystal>().SetMinIntensity();
        }
        hasLitAllCrystals = false;
    }

    // Checks to reset the firestone after a delay (duration = seconds)
    private IEnumerator ResetFirestone(float duration)
    {
        if (firestones == null) yield break;
        else if (duration != 0) yield return new WaitForSeconds(duration);

        foreach (Transform child in firestones.transform)
            child.GetComponentInChildren<Light>().enabled = true;
    }

    // Checks to resets the generator after a delay (duration = seconds)
    private IEnumerator ResetGenerator(float duration)
    {
        if (generators == null) yield break;
        else if (duration != 0) yield return new WaitForSeconds(duration);

        foreach (Transform child in generators.transform)
            child.GetComponent<Generator>().TurnOffGenerator();
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        checkpointManagerScript = null;
        cameraScript = FindObjectOfType<CameraController>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        gameManagerScript = FindObjectOfType<GameManager>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        torchMeterScript = FindObjectOfType<TorchMeter>();
        playerScript = FindObjectOfType<TileMovementController>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets them by looking at the names of children
        for (int i = 0; i < gameHUDScript.transform.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.GetChild(i).gameObject;

            if (child.name == "OptionalDeathScreen")
                deathScreen = child;
        }

        treeHitparticleSystem = ParticleEffect("TreeHitParticle").GetComponent<ParticleSystem>().main;
    }

}
