using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;

public class PuzzleManager : MonoBehaviour
{
    private int? puzzleNumber;
    private int? bridgeNumber;

    private bool hasLitAllCrystals = false;
    private bool hasMovedPuzzleView = false;

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
    [SerializeField]
    private ParticleSystem.MainModule treeHitparticleSystem;

    private TileMovementController playerScript;
    private CameraController cameraScript;
    private AudioManager audioManagerScript;
    private GameManager gameManagerScript;
    private GameHUD gameHUDScript;
    private TorchMeter torchMeterScript;
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
    public void ResetPuzzle(float? duration = null)
    {
        float delay = duration ?? 0f; // 0f = no delay (instant reset)
        checkpointManagerScript.ResetPlayer(delay);
        DestroyAllPuzzleParticles();

        // Doesn't reset the puzzle blocks if the death screen is active
        if (gameManagerScript.canDeathScreen && torchMeterScript.CurrentVal <= 0 && !deathScreen.activeInHierarchy)
            return;

        StartCoroutine(ResetCrystals(delay));
        StartCoroutine(ResetPushableBlocks(delay));
        StartCoroutine(ResetBreakableBlocks(delay));
        StartCoroutine(ResetFirestone(delay));
        StartCoroutine(ResetGenerator(delay));
    }

    // Checks to move the camera to the next/previous puzzle view
    public void MoveCameraToNextPuzzle()
    {
        // The camera will not move if the player is on the first/last bridge
        // Note: the bridge number for the first/last bridge will be NULL since they dont have numbers in their name
        if (!playerScript.OnBridge() || bridgeNumber == null || hasMovedPuzzleView) return;

        if (bridgeNumber != puzzleNumber)
            cameraScript.PreviousPuzzleView();
        else
            cameraScript.NextPuzzleView();

        hasMovedPuzzleView = true;
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
        //Debug.Log("Updated Parent Objects");
    }

    // Updates/sets the current checkpoint
    public void SetCurrentCheckpoint(GameObject checkpoint)
    {
        currentCheckpoint = checkpoint;
        currentPuzzle = currentCheckpoint.transform.parent.parent.gameObject;
        puzzleNumber = ConvertObjectNameToNumber(currentPuzzle);

        checkpointManagerScript = currentCheckpoint.GetComponent<CheckpointManager>();
        checkpointManagerScript.LastBridgeTileCheck(); // Note: player rotation is saved in this method

        torchMeterScript.MaxVal = checkpointManagerScript.GetNumMovements(); // The max tile moves for the puzzle
        torchMeterScript.ResetTorchMeterElements();

        hasMovedPuzzleView = false;
        UpdateParentObjects();
    }

    // Updates/sets the current bridge
    public void SetCurrentBridge(GameObject bridge)
    {
        currentBridge = bridge;
        bridgeNumber = ConvertObjectNameToNumber(currentBridge);
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

    // Instantiates a particle effect on an object (offsetPosY = the optional value added to theObjects original y position)
    public void InstantiateParticleEffect(GameObject theObject, string particleName, float? offsetPosY = null)
    {
        // Note: no offset will be added if the float parameter is not set
        Vector3 offsetPos = new Vector3(0f, offsetPosY ?? 0f, 0f);
        GameObject particle = ParticleEffect(particleName);

        // Sets the particle's color the tree shader's color if applicable
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

        // Returns an empty game object otherwise
        return new GameObject();
    }

    // Converts the name of an object to an integer
    private int? ConvertObjectNameToNumber(GameObject theObject)
    {
        // If the object has a number in its name
        if (theObject.name.Any(char.IsDigit))
        {
            string newObjectName = Regex.Replace(theObject.name, "[A-Za-z ]", "");
            return int.Parse(newObjectName);
        }

        // Note: returns null if the name doesn't contain any numbers!
        return null;
    }

    // Checks to play the chime sfx - if all crystals within a puzzle are lit
    public void AllCrystalsLitCheck()
    {
        if (hasLitAllCrystals) return;
        bool allCrystalsLit = true;

        foreach (Transform child in staticBlocks.transform)
        {
            // If the light intesnity within ANY crystal is NOT GREATER than the minLightIntesity
            if (child.gameObject.activeInHierarchy && child.name.Contains("Crystal") && !child.GetComponent<Crystal>().CrystalLitCheck())
                allCrystalsLit = false;
        }

        if (allCrystalsLit)
        {
            foreach (Transform child in staticBlocks.transform)
            {          
                if (child.gameObject.activeInHierarchy && child.name.Contains("Crystal"))
                    child.GetComponent<Crystal>().LeaveCrystalLightOn();
            }

            audioManagerScript.PlayChimeSFX();
            hasLitAllCrystals = true;
        }
    }

    // Checks to fade out the generator's audio
    public void ResetGeneratorCheck()
    {
        if (generators == null) return;

        // Note: the generator will turn off after its audio fades out
        foreach (Transform child in generators.transform)
            child.GetComponent<Generator>().FadeOutGeneratorLoop();
    }

    // Destroys all active particle effects within a puzzle
    private void DestroyAllPuzzleParticles()
    {
        // Note: all particles are instantiated as children of the game manager
        foreach (Transform child in gameManagerScript.transform)
            Destroy(child.gameObject);
    }

    // Checks to reset the postion for all pushable blocks after a delay
    private IEnumerator ResetPushableBlocks(float seconds)
    {
        if (pushableBlocks == null) yield break;
        yield return new WaitForSeconds(seconds);

        foreach (Transform child in pushableBlocks.transform)
            child.GetComponent<BlockMovementController>().ResetBlock();

        //Debug.Log($"Pushable Blocks Count: {pushableBlocks.transform.childCount}"); 
    }

    // Checks to set all breakable blocks active after a delay
    private IEnumerator ResetBreakableBlocks(float seconds)
    {
        if (breakableBlocks == null) yield break;
        yield return new WaitForSeconds(seconds);

        foreach (Transform child in breakableBlocks.transform)
            child.gameObject.SetActive(true);

        //Debug.Log($"Breakable Blocks Count: {breakableBlocks.transform.childCount}");  
    }

    // Checks to reset all crystals after a delay
    public IEnumerator ResetCrystals(float seconds)
    {
        if (staticBlocks == null) yield break;
        yield return new WaitForSeconds(seconds);

        foreach (Transform child in staticBlocks.transform)
        {
            if (child.gameObject.activeInHierarchy && child.name.Contains("Crystal"))
                child.GetComponent<Crystal>().ResetCrystalLight();

            hasLitAllCrystals = false;
        }

        //Debug.Log($"Crystal Blocks Count: {staticBlocks.transform.childCount}");     
    }

    // Checks to reset the firestone after a delay
    private IEnumerator ResetFirestone(float seconds)
    {
        if (firestones == null) yield break;
        yield return new WaitForSeconds(seconds);

        foreach (Transform child in firestones.transform)
            child.GetComponentInChildren<Light>().enabled = true;

        //Debug.Log($"Firestones Count: {firestones.transform.childCount}");
    }

    // Checks to resets the generator after a delay
    private IEnumerator ResetGenerator(float seconds)
    {
        if (generators == null) yield break;
        yield return new WaitForSeconds(seconds);

        foreach (Transform child in generators.transform)
            child.GetComponent<Generator>().TurnOffGenerator();

        //Debug.Log($"Generators Count: {generators.transform.childCount}");
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        cameraScript = FindObjectOfType<CameraController>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        gameManagerScript = FindObjectOfType<GameManager>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        torchMeterScript = FindObjectOfType<TorchMeter>();
        checkpointManagerScript = null;
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
