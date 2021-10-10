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

    private IEnumerator shakeTreeCoroutine;
    private IEnumerator shakeSnowTreeCoroutine;
    private IEnumerator shakeCrystalCoroutine;
    private IEnumerator shakeBarrelCoroutine;
    private IEnumerator shakeRockCoroutine;

    private ParticleSystem.MainModule treeHitparticleSystem;
    private GameManager gameManagerScript;

    void Awake()
    {
        gameManagerScript = FindObjectOfType<GameManager>();

        treeHitParticle = gameManagerScript.treeHitParticle;
        snowTreeHitParticle = gameManagerScript.snowTreeHitParticle;
        treeHitparticleSystem = treeHitParticle.GetComponent<ParticleSystem>().main;
    }

    // Resets/calls the coroutine to shake an object
    public void ShakeObject(GameObject objectToShake)
    {
        if (objectToShake.name == "Tree")
        {
            if (shakeTreeCoroutine != null)
                StopCoroutine(shakeTreeCoroutine);

            // Gets the color of the shader material by acquiring the reference name of the color component in the tree shader
            if (objectToShake != lastTree)
            {
                GameObject treeTop = objectToShake.transform.GetChild(0).gameObject;
                Color treeColor = treeTop.GetComponent<MeshRenderer>().material.GetColor("Color_6566A18B");
                treeHitparticleSystem.startColor = treeColor;
                lastTree = objectToShake;
            }

            Instantiate(treeHitParticle, objectToShake.transform.position, objectToShake.transform.rotation);
            shakeTreeCoroutine = StartShake(objectToShake, treeShakePower, treeShakeDuration);
            StartCoroutine(shakeTreeCoroutine);
        }

        else if (objectToShake.name == "SnowTree" || objectToShake.name == "BarrenTree")
        {
            if (shakeSnowTreeCoroutine != null)
                StopCoroutine(shakeSnowTreeCoroutine);

            Instantiate(snowTreeHitParticle, objectToShake.transform.position, objectToShake.transform.rotation);
            shakeSnowTreeCoroutine = StartShake(objectToShake, treeShakePower, treeShakeDuration);
            StartCoroutine(shakeSnowTreeCoroutine);
        }

        else if (objectToShake.name == "Crystal")
        {
            if (shakeCrystalCoroutine != null)
                StopCoroutine(shakeCrystalCoroutine);

            shakeCrystalCoroutine = StartShake(objectToShake, crystalShakePower, crystalShakeDuration);
            StartCoroutine(shakeCrystalCoroutine);
        }

        else if (objectToShake.name == "GasBarrel")
        {
            if (shakeBarrelCoroutine != null)
                StopCoroutine(shakeBarrelCoroutine);

            shakeBarrelCoroutine = StartShake(objectToShake, barrelShakePower, barrelShakeDuration);
            StartCoroutine(shakeBarrelCoroutine);
        }

        else if (objectToShake.name == "Rock")
        {
            if (shakeRockCoroutine != null)
                StopCoroutine(shakeRockCoroutine);

            shakeRockCoroutine = StartShake(objectToShake, rockShakePower, rockShakeDuration);
            StartCoroutine(shakeRockCoroutine);
        }

    }

    // Shakes the rotation of an object over a period of time (duration = seconds)
    private IEnumerator StartShake(GameObject objectToShake, float power, float duration)
    {
        float time = 0;
        float shakePower = power;
        float shakeFadeTime = power / duration;
        float shakeRotation = power * rotationMultiplier;

        while (time < duration)
        {
            // Ignore commented lines below unless needed - moves objectToShake to a random/new position after it shakes
            //float xAmount = Random.Range(-1f, 1f) * shakePower;
            //float yAmount = Random.Range(-1f, 1f) * shakePower;
            //objectToShake.transform.position += new Vector3(xAmount, yAmount, 0);

            shakePower = Mathf.MoveTowards(shakePower, 0f, shakeFadeTime * Time.deltaTime);
            shakeRotation = Mathf.MoveTowards(shakeRotation, 0f, shakeFadeTime * rotationMultiplier * Time.deltaTime);

            if (objectToShake.transform.rotation != Quaternion.Euler(0f, 0f, shakeRotation * Random.Range(-1f, 1f)))
                objectToShake.transform.rotation = Quaternion.Euler(0f, 0f, shakeRotation * Random.Range(-1f, 1f));

            time += Time.deltaTime;
            yield return null;
        }

        objectToShake.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    // Function for the shake itself (length is for how long the shake will last in seconds, power is the shake's intensity)
    /*public void StartShake(GameObject objectToShake, float length, float power)
    {
        //StaticBlockSFX();                                                                                                   

        //Instantiate(particleEffect, objectToShake.transform.position, objectToShake.transform.rotation);                          

        shakeTimeRemaining = length;
        shakePower = power;

        shakeFadeTime = power / length;

        shakeRotation = power * rotationMultiplier;
    }*/

}
