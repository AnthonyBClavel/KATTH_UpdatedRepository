using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectShakeController : MonoBehaviour
{
    [Header("Floats")]
    [Range(0, 20f)]
    public float rotationMultiplier = 7.5f;

    [Header("Rocks")]
    [Range(0, 3f)]
    public float rockShakePower = 0.25f;
    [Range(0, 3f)]
    public float rockShakeDuration = 0.2f;

    [Header("Trees")]
    [Range(0, 3f)]
    public float treeShakePower = 0.25f;
    [Range(0, 3f)]
    public float treeShakeDuration = 0.25f;

    [Header("Crystals")]
    [Range(0, 3f)]
    public float crystalShakePower = 0.25f;
    [Range(0, 3f)]
    public float crystalShakeDuration = 0.25f;

    [Header("Barrel")]
    [Range(0, 3f)]
    public float barrelShakePower = 0.25f;
    [Range(0, 3f)]
    public float barrelShakeDuration = 0.25f;

    private GameObject treeHitParticle;
    private GameObject snowTreeHitParticle;
    private GameObject lastTree;
    private GameObject lastSnowTree;
    private GameObject lastCrystal;
    private GameObject lastBarrel;
    private GameObject lastRock;

    private IEnumerator shakeTreeCoroutine;
    private IEnumerator shakeSnowTreeCoroutine;
    private IEnumerator shakeCrystalCoroutine;
    private IEnumerator shakeBarrelCoroutine;
    private IEnumerator shakeRockCoroutine;

    private Quaternion zeroRotation = Quaternion.Euler(0, 0, 0);
    private ParticleSystem.MainModule treeHitparticleSystem;
    private GameManager gameManagerScript;
    private TileMovementController playerScript;

    void Awake()
    {
        gameManagerScript = FindObjectOfType<GameManager>();
        playerScript = FindObjectOfType<TileMovementController>();

        treeHitParticle = gameManagerScript.treeHitParticle;
        snowTreeHitParticle = gameManagerScript.snowTreeHitParticle;
        treeHitparticleSystem = treeHitParticle.GetComponent<ParticleSystem>().main;
    }

    // Resets/calls the coroutine to shake an object
    public void ShakeObject(GameObject objectToShake)
    {
        if (objectToShake.name == "Tree")
        {
            if (objectToShake == lastTree)// OLD VERSION == (shakeTreeCoroutine != null)
            {
                StopCoroutine(shakeTreeCoroutine);
                lastTree.transform.rotation = zeroRotation;
            }

            // Gets the color of the shader material by acquiring the reference name of the color component in the tree shader
            if (objectToShake != lastTree)
            {
                GameObject treeTop = objectToShake.transform.GetChild(0).gameObject;
                Color treeColor = treeTop.GetComponent<MeshRenderer>().material.GetColor("Color_6566A18B");
                treeHitparticleSystem.startColor = treeColor;
                lastTree = objectToShake;
            }

            GameObject newTreeHitParticle = Instantiate(treeHitParticle, objectToShake.transform.position, objectToShake.transform.rotation);
            newTreeHitParticle.transform.parent = gameManagerScript.transform;

            shakeTreeCoroutine = StartShake(objectToShake, treeShakePower, treeShakeDuration);
            StartCoroutine(shakeTreeCoroutine);
        }

        else if (objectToShake.name == "SnowTree" || objectToShake.name == "BarrenTree")
        {
            if (objectToShake == lastSnowTree)
            {
                StopCoroutine(shakeSnowTreeCoroutine);
                lastSnowTree.transform.rotation = zeroRotation;
            }
            if (objectToShake != lastSnowTree)
                lastSnowTree = objectToShake;

            GameObject newSnowTreeHitParticle = Instantiate(snowTreeHitParticle, objectToShake.transform.position, objectToShake.transform.rotation);
            newSnowTreeHitParticle.transform.parent = gameManagerScript.transform;

            shakeSnowTreeCoroutine = StartShake(objectToShake, treeShakePower, treeShakeDuration);
            StartCoroutine(shakeSnowTreeCoroutine);
        }

        else if (objectToShake.name == "Crystal")
        {
            if (objectToShake == lastCrystal)
            {
                StopCoroutine(shakeCrystalCoroutine);
                lastCrystal.transform.rotation = zeroRotation;
            }
            if (objectToShake != lastCrystal)
                lastCrystal = objectToShake;

            shakeCrystalCoroutine = StartShake(objectToShake, crystalShakePower, crystalShakeDuration);
            StartCoroutine(shakeCrystalCoroutine);
        }

        else if (objectToShake.name == "GasBarrel")
        {
            if (objectToShake == lastBarrel)
            {
                StopCoroutine(shakeBarrelCoroutine);
                lastBarrel.transform.rotation = zeroRotation;
            }
            if (objectToShake != lastBarrel)
                lastBarrel = objectToShake;

            shakeBarrelCoroutine = StartShake(objectToShake, barrelShakePower, barrelShakeDuration);
            StartCoroutine(shakeBarrelCoroutine);
        }

        else if (objectToShake.name == "Rock")
        {
            if (objectToShake == lastRock)
            {
                StopCoroutine(shakeRockCoroutine);
                lastRock.transform.rotation = zeroRotation;
            }
            if (objectToShake != lastRock)
                lastRock = objectToShake;

            shakeRockCoroutine = StartShake(objectToShake, rockShakePower, rockShakeDuration);
            StartCoroutine(shakeRockCoroutine);
        }
    }

    // Shakes the rotation of an object over a period of time (duration = seconds)
    private IEnumerator StartShake(GameObject objectToShake, float power, float duration)
    {
        float time = 0;
        float shakeFadeTime = power / duration;
        float shakeRotation = power * rotationMultiplier;
        //float shakePower = power;

        while (time < duration)
        {
            // Ignore commented lines below unless you want the objectToShake to move to a random/new position after it shakes
            //shakePower = Mathf.MoveTowards(shakePower, 0f, shakeFadeTime * Time.deltaTime);
            //objectToShake.transform.position += new Vector3((Random.Range(-1f, 1f) * shakePower), (Random.Range(-1f, 1f) * shakePower), 0);

            // Pauses the coroutine if the game is paused (WaitForSeconds respects timeScale)
            if (Time.timeScale == 0f)
                yield return new WaitForSeconds(0.01f);

            // Stops the shake (while loop) if the puzzle was restarted
            if (playerScript.CanRestartPuzzle && Input.GetKeyDown(KeyCode.R))
                break;

            shakeRotation = Mathf.MoveTowards(shakeRotation, 0f, shakeFadeTime * rotationMultiplier * Time.deltaTime);
            objectToShake.transform.rotation = Quaternion.Euler(0f, 0f, shakeRotation * Random.Range(-1f, 1f));

            time += Time.deltaTime;
            yield return null;
        }

        objectToShake.transform.rotation = zeroRotation;
    }

}
