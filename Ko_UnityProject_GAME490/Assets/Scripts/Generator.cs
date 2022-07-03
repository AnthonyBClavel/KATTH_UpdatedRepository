using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    private bool isGenActive = false;
    private float rotationDuration = 2f; // Original Value = 2f

    private GameObject steamFX;
    private GameObject gearsHolder;
    private GameObject gear01;

    private Material heatDoorMat;
    private Material generatorLightMat01;
    private Material generatorLightMat02;

    private IEnumerator gearsCoroutine;
    private IEnumerator effectsCoroutine;

    public bool IsGenActive
    {
        get { return isGenActive; }
    }

    // Awake is called before Start()
    void Awake()
    {
        SetElements();
    }

    // Turns on the generator
    public void TurnOnGenerator()
    {
        isGenActive = true;
        StartEffectsCoroutine(); 
    }

    // Turns off the generator
    public void TurnOffGenerator()
    {
        isGenActive = false;
        ResetGeneratorEffects();
    }

    // Disables the generator effects
    private void ResetGeneratorEffects()
    {
        StopAllCoroutines();
        steamFX.SetActive(false);

        generatorLightMat01.DisableKeyword("_EMISSION");
        generatorLightMat02.DisableKeyword("_EMISSION");
        heatDoorMat.DisableKeyword("_EMISSION");
    }

    // Starts the coroutine that enables the generator effects
    private void StartEffectsCoroutine()
    {
        if (effectsCoroutine != null) StopCoroutine(effectsCoroutine);

        effectsCoroutine = EnableEffects();
        StartCoroutine(effectsCoroutine);
    }

    // Starts the coroutine that rotates the generator gears
    private void StartGearsCoroutine()
    {
        if (gearsCoroutine != null) StopCoroutine(gearsCoroutine);

        gearsCoroutine = RotateGears(rotationDuration);
        StartCoroutine(gearsCoroutine);
    }

    // Lerps the rotation of each generator gear to another over a specific duration (duration = seconds)
    private IEnumerator RotateGears(float duration)
    {
        Vector3 originalRotation = gear01.transform.localEulerAngles;
        float startRotationY = originalRotation.y;
        float endRotationY = startRotationY + 360f;
        float time = 0;

        while (time < duration)
        {
            float rotY = Mathf.Lerp(startRotationY, endRotationY, time / duration);

            foreach (Transform child in gearsHolder.transform)
            {
                float newRotY = (child.name == "Gear02" || child.name == "Gear04") ? -rotY : rotY;
                child.localEulerAngles = new Vector3(originalRotation.x, newRotY, originalRotation.z);
            }

            time += Time.deltaTime;
            yield return null;
        }

        gear01.transform.localEulerAngles = originalRotation;
        StartGearsCoroutine();
    }

    // Enables the generator's visual effects (emissive materials, gear animation, particles)
    private IEnumerator EnableEffects()
    {
        generatorLightMat01.EnableKeyword("_EMISSION");
        generatorLightMat02.EnableKeyword("_EMISSION");

        yield return new WaitForSeconds(0.7f);
        heatDoorMat.EnableKeyword("_EMISSION");
        StartGearsCoroutine();

        yield return new WaitForSeconds(0.2f);
        steamFX.SetActive(true);
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;
        
        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "SteamFX":
                    steamFX = child.gameObject;
                    break;
                case "LightBulb01":
                    generatorLightMat01 = child.GetComponent<MeshRenderer>().material;
                    break;
                case "LightBulb02":
                    generatorLightMat02 = child.GetComponent<MeshRenderer>().material;
                    break;
                case "HeatDoor":
                    heatDoorMat = child.GetComponent<MeshRenderer>().material;
                    break;
                case "Gears":
                    gearsHolder = child.gameObject;
                    break;
                case "Gear01":
                    gear01 = child.gameObject;
                    break;
                default:
                    break;
            }

            SetVariables(child);
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        SetVariables(transform);
        ResetGeneratorEffects();
    }
}