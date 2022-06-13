using System.Collections;
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

    // Checks to stop the object from shaking if applicable - stops the coroutine and removes the tuple from the list
    private void StopShakingObject(GameObject objectToShake)
    {
        for (int i = 0; i < shakingObjects.Count; i++)
        {
            (GameObject, IEnumerator) tuple = shakingObjects[i];

            if (objectToShake == tuple.Item1)
            {
                StopCoroutine(tuple.Item2);
                shakingObjects.Remove(tuple);
                objectToShake.transform.rotation = zeroRotation;
            }
        }
    }

    // Stops and clears all elements within the list of shaking objects
    public void StopAllShakingObjects()
    {
        for (int i = 0; i < shakingObjects.Count; i++)
        {
            (GameObject, IEnumerator) tuple = shakingObjects[i];
            StopCoroutine(tuple.Item2);
            tuple.Item1.transform.rotation = zeroRotation;
        }

        shakingObjects.Clear();
    }

    // Checks to play effects for object to shake (particle effects, sfx, and animations)
    private void PlayShakeFX(GameObject objectToShake)
    {
        switch (objectToShake.name)
        {
            case "Tree":
                puzzleManagerScript.InstantiateParticleEffect(objectToShake, "TreeHitParticle");
                audioManagerScript.PlayTreeHitSFX();
                break;
            case "SnowTree":
                puzzleManagerScript.InstantiateParticleEffect(objectToShake, "SnowHitParticle");
                audioManagerScript.PlaySnowTreeHitSFX();
                break;
            case "BarrenTree":
                puzzleManagerScript.InstantiateParticleEffect(objectToShake, "SnowHitParticle");
                audioManagerScript.PlaySnowTreeHitSFX();
                break;
            case "Crystal":
                audioManagerScript.PlayCrystalHitSFX();
                objectToShake.GetComponentInParent<Crystal>().PlayCrystalLightAnim();
                break;
            case "GasBarrel":
                audioManagerScript.PlayMetalHitSFX();
                break;
            case "Rock":
                audioManagerScript.PlayRockHitSFX();
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

        // Note: After the object has finished shaking, it removes itself from list of shaking objects
        StopShakingObject(objectToShake);
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        audioManagerScript = FindObjectOfType<AudioManager>();
        puzzleManagerScript = FindObjectOfType<PuzzleManager>();
    }

}
