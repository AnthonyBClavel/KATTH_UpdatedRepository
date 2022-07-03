using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour
{
    private float lerpDuration = 0.5f; // Original Value = 0.5f
    private float glowDuration = 3f; // Original Value = 3f
    private float minLightIntensity = 0.1f; // Original Value = 0.1f
    private float maxLightIntensity = 1f; // Original Value = 1f

    private bool canLerpToMax = true;
    private bool canResetGlowLength = false;
    private bool isLerpingToMin = false;
    private bool allCrystalsLit = false;

    private GameObject crystalParent;
    private Light crystalLight;
    private IEnumerator crystalLightCoroutine;
    private PuzzleManager puzzleManagerScript;

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Checks to play the crystal's light animation
    // Note: the method will return if the crystal is lerping to its max
    public void PlayLightAnimation()
    {       
        if (!canLerpToMax && !canResetGlowLength) return;

        // Checks to lerp to its max intensity or to "reset" its glow duration
        float duration = !canResetGlowLength ? lerpDuration : glowDuration;
        if (canLerpToMax) canLerpToMax = false;
        StartCrystalLightCoroutine(maxLightIntensity, duration);
    }

    // Sets the crytsal's light intensity to its min - RESETS the crystal
    public void SetMinIntensity()
    {
        canLerpToMax = true;
        canResetGlowLength = false;
        isLerpingToMin = false;
        allCrystalsLit = false;

        StopAllCoroutines();
        crystalLight.intensity = minLightIntensity;
    }

    // Set the crystal's light intensity to its max - PREVENTS the crystal from lerping/resetting
    public void SetMaxIntensity()
    {
        canLerpToMax = false;
        canResetGlowLength = false;
        allCrystalsLit = true;

        if (!isLerpingToMin) return;
        StopAllCoroutines();
        StartCrystalLightCoroutine(maxLightIntensity, lerpDuration);
    }

    // Checks if the crystal's light intensity is greater than its min - returns true if so, false otherwise
    public bool LitCheck()
    {
        if (crystalLight.intensity > minLightIntensity) return true;

        return false;
    }

    // Checks if the crystal's light has lerped to its minimum intensity
    private void MinLightIntensityCheck()
    {
        if (allCrystalsLit || crystalLight.intensity != minLightIntensity) return;

        canLerpToMax = true;
        isLerpingToMin = false;
        canResetGlowLength = false;
    }

    // Checks if the crystal's light has lerped to its maximum intensity
    private void MaxLightIntensityCheck()
    {
        if (allCrystalsLit || crystalLight.intensity != maxLightIntensity) return;

        // Checks to "glow" (to stay lit for duration length) or to lerp to its min intesnity
        float endIntensity = !canResetGlowLength ? maxLightIntensity : minLightIntensity;
        float duration = !canResetGlowLength ? glowDuration : lerpDuration;

        if (!canResetGlowLength) canResetGlowLength = true;
        else if (canResetGlowLength) isLerpingToMin = true;
        StartCrystalLightCoroutine(endIntensity, duration);
    }

    // Starts the coroutine that lerps the crystal's light intensity
    private void StartCrystalLightCoroutine(float endIntensity, float duration)
    {
        if (crystalLightCoroutine != null) StopCoroutine(crystalLightCoroutine);

        crystalLightCoroutine = LerpLightIntensity(endIntensity, duration);
        StartCoroutine(crystalLightCoroutine);
    }

    // Lerps the crystal's light intensity over a specific duration (endIntensity = intensity to lerp to, duration = seconds)
    private IEnumerator LerpLightIntensity(float endIntensity, float duration)
    {
        float startIntesity = canResetGlowLength ? maxLightIntensity : crystalLight.intensity;
        float time = 0;

        while (time < duration)
        {
            if (!allCrystalsLit) puzzleManagerScript.AllCrystalsLitCheck(crystalParent);
            crystalLight.intensity = Mathf.Lerp(startIntesity, endIntensity, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        crystalLight.intensity = endIntensity;
        MinLightIntensityCheck();
        MaxLightIntensityCheck();
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        puzzleManagerScript = FindObjectOfType<PuzzleManager>();
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "CrystalLight":
                    crystalLight = child.GetComponent<Light>();
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
        crystalParent = transform.parent.gameObject;
    }

}
