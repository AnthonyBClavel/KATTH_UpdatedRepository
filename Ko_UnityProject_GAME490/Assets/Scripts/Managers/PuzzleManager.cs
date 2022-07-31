using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    static readonly string treeHitParticle = "TreeHitParticle";
    static readonly string treeShaderColor = "Color_6566A18B";
    private bool hasLitAllCrystals = false;

    private Transform currentPuzzle;
    private Transform staticBlocks;
    private Transform pushableBlocks;
    private Transform breakableBlocks;
    private Transform firestones;
    private Transform generators;

    public GameObject[] particleEffects;
    private ParticleSystem.MainModule treeHitparticleSystem;

    private TorchMeter torchMeterScript;
    private CameraController cameraScript;
    private DeathScreen deathScreenScript;
    private AudioManager audioManagerScript;
    private TileMovementController playerScript;
    private CheckpointManager checkpointManagerScript;

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
        DestroyAllPuzzleParticles(duration);

        // The puzzle blocks don't reset during the death screen
        if (deathScreenScript.CanDeathScreen() && !playerScript.CanRestartPuzzle) return;
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

        foreach (Transform child in currentPuzzle)
        {
            if (!child.gameObject.activeInHierarchy) continue;

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
        checkpointManagerScript = checkpoint.GetComponent<CheckpointManager>();
        checkpointManagerScript.SetSavedBlockPosition(); // The player rotation is saved in this method

        torchMeterScript.MaxVal = checkpointManagerScript.MaxTileMoves; // Sets the max tile moves for the puzzle
        torchMeterScript.ResetTorchMeter();

        currentPuzzle = checkpoint.transform.parent.parent;
        cameraScript.HasMovedPuzzleView = false;

        StartCoroutine(ResetGenerator(0f)); // Resets the generator from the previous puzzle if applicable
        UpdateParentObjects();
    }

    // Instantiates a particle effect on an object
    // Note: no offset will be added to the object's y position if the float parameter is not set
    public void InstantiateParticleEffect(GameObject theObject, string particleName, float? offsetPosY = 0f)
    {
        Vector3 offsetPos = new Vector3(0f, offsetPosY ?? 0f, 0f);
        GameObject particle = ParticleEffect(particleName);

        // Sets the particle's color to the tree shader's color if applicable
        if (particle.name == treeHitParticle)
        {          
            Material treeShader = theObject.transform.GetChild(0).GetComponent<MeshRenderer>().material;
            treeHitparticleSystem.startColor = treeShader.GetColor(treeShaderColor); // The color component's reference name in shader
        }

        GameObject particleEffect = Instantiate(particle, theObject.transform.position + offsetPos, theObject.transform.rotation);
        particleEffect.transform.parent = transform;
    }

    // Returns the desired particle effect within the array
    private GameObject ParticleEffect(string particleName)
    {
        foreach (GameObject child in particleEffects)
        {
            if (child.name != particleName) continue;
            return child;
        }
        return new GameObject();
    }

    // Destroys all active particle effects within a puzzle after a duration
    // Note: all particles are instantiated as children of the puzzle manager
    // Note: no duration will be added if the parameter is not set
    private void DestroyAllPuzzleParticles(float duration = 0f)
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject, duration);
    }

    // Checks if all crystals within a puzzle are lit - play the chime sfx if so
    // Note: crystalBlocks is the parent object that holds the crystals
    public void AllCrystalsLitCheck(GameObject crystalBlocks)
    {
        if (hasLitAllCrystals) return;
        bool allCrystalsLit = true;

        foreach (Transform child in crystalBlocks.transform)
        {
            if (!child.gameObject.activeInHierarchy || !child.name.Contains("Crystal")) continue;
            else if (!child.GetComponent<Crystal>().LitCheck()) allCrystalsLit = false;
        }

        // Returns if any crystal is not lit
        if (!allCrystalsLit) return;
        foreach (Transform child in crystalBlocks.transform)
        {
            if (!child.gameObject.activeInHierarchy || !child.name.Contains("Crystal")) continue;
            child.GetComponent<Crystal>().SetMaxIntensity();
        }

        audioManagerScript.PlayChimeSFX();
        hasLitAllCrystals = true;
    }

    // Checks to reset the postion for all pushable blocks after a delay (duration = seconds)
    private IEnumerator ResetPushableBlocks(float duration)
    {
        if (pushableBlocks == null) yield break;
        else if (duration != 0) yield return new WaitForSeconds(duration);

        foreach (Transform child in pushableBlocks)
            child.GetComponent<BlockMovementController>().ResetBlock();
    }

    // Checks to set all breakable blocks active after a delay (duration = seconds)
    private IEnumerator ResetBreakableBlocks(float duration)
    {
        if (breakableBlocks == null) yield break;
        else if (duration != 0) yield return new WaitForSeconds(duration);

        foreach (Transform child in breakableBlocks)
            child.gameObject.SetActive(true); 
    }

    // Checks to reset all crystals after a delay (duration = seconds)
    public IEnumerator ResetCrystals(float duration)
    {
        if (staticBlocks == null) yield break;
        else if (duration != 0) yield return new WaitForSeconds(duration);

        foreach (Transform child in staticBlocks)
        {
            if (!child.gameObject.activeInHierarchy || !child.name.Contains("Crystal")) continue;
            child.GetComponent<Crystal>().SetMinIntensity();
        }
        hasLitAllCrystals = false;
    }

    // Checks to reset the firestone after a delay (duration = seconds)
    private IEnumerator ResetFirestone(float duration)
    {
        if (firestones == null) yield break;
        else if (duration != 0) yield return new WaitForSeconds(duration);

        foreach (Transform child in firestones)
            child.GetComponentInChildren<Light>().enabled = true;
    }

    // Checks to resets the generator after a delay (duration = seconds)
    private IEnumerator ResetGenerator(float duration)
    {
        if (generators == null) yield break;
        else if (duration != 0) yield return new WaitForSeconds(duration);

        foreach (Transform child in generators)
            child.GetComponent<Generator>().TurnOffGenerator();
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        cameraScript = FindObjectOfType<CameraController>();
        deathScreenScript = FindObjectOfType<DeathScreen>();
        torchMeterScript = FindObjectOfType<TorchMeter>();
        checkpointManagerScript = null;
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        treeHitparticleSystem = ParticleEffect("TreeHitParticle").GetComponent<ParticleSystem>().main;
    }

}
