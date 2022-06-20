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
    public void PlayLightAnimation()
    {
        // Lerps the crystal's light intensity to its max 
        if (canLerpToMax)
        {
            StartCrystalLightCoroutine(maxLightIntensity, lerpDuration);
            canLerpToMax = false;
        }
        // Resets its glow duration otherwise (if applicable)
        else if (canResetGlowLength)
            StartCrystalLightCoroutine(maxLightIntensity, glowDuration);
    }

    // Resets the crytsal's light intensity to its min - RESETS the crystal
    public void SetMinIntensity()
    {
        canLerpToMax = true;
        canResetGlowLength = false;
        isLerpingToMin = false;
        allCrystalsLit = false;

        StopAllCoroutines();
        crystalLight.intensity = minLightIntensity;
    }

    // Checks to set the crystal's light intensity to its max - PREVENTS the crystal from lerping/resetting its light intensity
    public void SetMaxIntensity()
    {
        canLerpToMax = false;
        canResetGlowLength = false;
        allCrystalsLit = true;

        if (!isLerpingToMin) return;
        StopAllCoroutines();
        StartCrystalLightCoroutine(maxLightIntensity, lerpDuration);
        //crystalLight.intensity = maxLightIntensity;
    }

    // Checks if the crystal's light intensity is greater than its min - returns true if so, false otherwise
    public bool LitCheck()
    {
        if (crystalLight.intensity > minLightIntensity)
            return true;

        return false;
    }

    // Checks if the crystal's light has lerped to its minimum intensity
    private void MinLightIntensityCheck()
    {
        if (crystalLight.intensity != minLightIntensity) return;

        canLerpToMax = true;
        isLerpingToMin = false;
        canResetGlowLength = false;
    }

    // Checks if the crystal's light has lerped to its maximum intensity
    private void MaxLightIntensityCheck()
    {
        if (crystalLight.intensity != maxLightIntensity) return;

        // Checks to "glow" (to stay lit for the length of the duration)
        if (!canResetGlowLength)
        {
            StartCrystalLightCoroutine(maxLightIntensity, glowDuration);
            canResetGlowLength = true;
        }
        // Lerps to its min intensity otherwise
        else
        {
            StartCrystalLightCoroutine(minLightIntensity, lerpDuration);
            isLerpingToMin = true;
        }
    }

    // Starts the coroutine that lerps the crystal's light intensity
    private void StartCrystalLightCoroutine(float endIntensity, float duration)
    {
        if (crystalLightCoroutine != null)
            StopCoroutine(crystalLightCoroutine);

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
        if (allCrystalsLit) yield break;

        MinLightIntensityCheck();
        MaxLightIntensityCheck();
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        puzzleManagerScript = FindObjectOfType<PuzzleManager>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets them by looking at the names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            for (int j = 0; j < child.transform.childCount; j++)
            {
                GameObject child02 = child.transform.GetChild(j).gameObject;

                if (child02.name.Contains("CrystalLight"))
                    crystalLight = child02.GetComponent<Light>();
            }
        }

        crystalParent = transform.parent.gameObject;
    }

}
