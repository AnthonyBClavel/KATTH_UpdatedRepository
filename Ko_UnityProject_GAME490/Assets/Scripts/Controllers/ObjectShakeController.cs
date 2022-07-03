﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectShakeController : MonoBehaviour
{
    [SerializeField] [Range(0, 20f)]
    private float rotationMultiplier = 7.5f; // Original Value = 7.5f;
    [SerializeField] [Range(0, 3f)]
    private float shakeDuration = 0.25f; // Original Value = 0.25f;
    [SerializeField] [Range(0, 3f)]
    private float shakePower = 0.25f; // Original Value = 0.25f;

    private List<(GameObject, IEnumerator)> shakingObjects = new List<(GameObject, IEnumerator)>();
    private Quaternion zeroRotation = Quaternion.Euler(0, 0, 0);

    private AudioManager audioManagerScript;
    private PuzzleManager puzzleManagerScript;

    void Awake()
    {
        SetScripts();   
    }

    // Starts a coroutine that shakes the object
    public void ShakeObject(GameObject objectToShake)
    {
        StopShakingObject(objectToShake);
        IEnumerator shakeCoroutine = StartShake(objectToShake, shakePower, shakeDuration);
        (GameObject, IEnumerator) shakeTuple = (objectToShake, shakeCoroutine);

        shakingObjects.Add(shakeTuple);
        StartCoroutine(shakeCoroutine);
        PlayShakeFX(objectToShake);
    }

    // Checks to stop the object from shaking - stops the coroutine and removes the tuple from the list
    private void StopShakingObject(GameObject objectToShake)
    {
        foreach ((GameObject, IEnumerator) tuple in shakingObjects)
        {
            if (objectToShake != tuple.Item1) continue;

            StopCoroutine(tuple.Item2);
            shakingObjects.Remove(tuple);
            objectToShake.transform.rotation = zeroRotation;
            break;
        }
    }

    // Stops and clears all elements within the list of shaking objects
    public void StopAllShakingObjects()
    {
        foreach ((GameObject, IEnumerator) tuple in shakingObjects)
        {
            StopCoroutine(tuple.Item2);
            tuple.Item1.transform.rotation = zeroRotation;
        }

        shakingObjects.Clear();
    }

    // Checks to play effects for the object to shake (particle effects, sfx, and animations)
    private void PlayShakeFX(GameObject objectToShake)
    {
        switch (objectToShake.name)
        {
            case "Tree":
                puzzleManagerScript.InstantiateParticleEffect(objectToShake, "TreeHitParticle");
                audioManagerScript.PlayHitTreeSFX();
                break;
            case "SnowTree":
                puzzleManagerScript.InstantiateParticleEffect(objectToShake, "SnowHitParticle");
                audioManagerScript.PlayHitSnowTreeSFX();
                break;
            case "BarrenTree":
                puzzleManagerScript.InstantiateParticleEffect(objectToShake, "SnowHitParticle");
                audioManagerScript.PlayHitSnowTreeSFX();
                break;
            case "Crystal":
                audioManagerScript.PlayHitCrystalSFX();
                objectToShake.GetComponentInParent<Crystal>().PlayLightAnimation();
                break;
            case "GasBarrel":
                audioManagerScript.PlayHitBarrelSFX();
                break;
            case "Rock":
                audioManagerScript.PlayHitRockSFX();
                break;
            default:
                //Debug.Log("Unrecognizable object name");
                break;
        }
    }

    // Sets the rotation of an object to another over a specific duration (duration = seconds)
    private IEnumerator StartShake(GameObject objectToShake, float power, float duration)
    {
        float shakeRotation = power * rotationMultiplier;
        float time = 0;

        while (time < duration)
        {
            // Pauses the coroutine while the game is paused - respects timeScale
            if (Time.timeScale == 0f) yield return new WaitForSeconds(0.01f);

            shakeRotation = Mathf.MoveTowards(shakeRotation, 0f, rotationMultiplier * Time.deltaTime);
            objectToShake.transform.rotation = Quaternion.Euler(0f, 0f, shakeRotation * Random.Range(-1f, 1f));
            time += Time.deltaTime;
            yield return null;
        }

        // Note: the object removes itself from the list after shaking
        StopShakingObject(objectToShake);
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        audioManagerScript = FindObjectOfType<AudioManager>();
        puzzleManagerScript = FindObjectOfType<PuzzleManager>();
    }

}
