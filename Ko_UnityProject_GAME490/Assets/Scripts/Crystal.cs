﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour
{
    private float lerpDuration = 0.5f; // Original Value = 0.5f
    private float glowDuration = 3f; // Original Value = 3f
    private float minLightIntesity = 0.1f; // Original Value = 0.1f
    private float maxLightIntesity = 1f; // Original Value = 1f

    private bool canLerpToMax = true;
    private bool canResetGlowLength = false;
    private bool canStayLit = false;
    private bool islerpingToMin = false;

    private Light crystalLight;
    private IEnumerator crystalLightCoroutine;
    private PuzzleManager puzzleManagerScript;

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Checks if the crystal's light intesnity is greater than the minLightIntesity - returns true if so, false otherwise
    public bool CrystalLitCheck()
    {
        if (crystalLight.intensity > minLightIntesity)
            return true;
        else
            return false;
    }

    // Checks to lerp the crystal's light intesity or reset its glow length
    public void PlayCrystalLightAnim()
    {
        if (canLerpToMax)
        {
            StartCrystalCoroutine(maxLightIntesity, lerpDuration);
            canLerpToMax = false;
        }
        else if (canResetGlowLength)
            StartCrystalCoroutine(maxLightIntesity, glowDuration);
    }

    // Resets the crytsal's light intesnity to its minimum intesity
    public void ResetCrystalLight()
    {
        StopAllCoroutines();
        crystalLight.intensity = minLightIntesity;
        canLerpToMax = true;
        canResetGlowLength = false;
        canStayLit = false;
        islerpingToMin = false;
    }

    // Prevents the crystal light from lerping or resetting its light intensity - leaves the light at its max intesity
    public void LeaveCrystalLightOn()
    {
        canLerpToMax = false;
        canResetGlowLength = false;
        canStayLit = true;

        // When the crystal light is fading out...
        if (islerpingToMin)
        {
            StopAllCoroutines();
            crystalLight.intensity = maxLightIntesity;
        }
    }

    // Starts the coroutine for the crystal's light animation
    private void StartCrystalCoroutine(float endIntensity, float duration)
    {
        if (crystalLightCoroutine != null)
            StopCoroutine(crystalLightCoroutine);

        crystalLightCoroutine = LerpCrystalLight(endIntensity, duration);
        StartCoroutine(crystalLightCoroutine);
    }

    // Lerps the crystal's light intensity over a specific duration (endIntesity = intesity to lerp to, duration = seconds)
    private IEnumerator LerpCrystalLight(float endIntensity, float duration)
    {
        if (canResetGlowLength)
            crystalLight.intensity = maxLightIntesity;

        float time = 0;
        float startIntesity = crystalLight.intensity;

        while (time < duration)
        {
            crystalLight.intensity = Mathf.Lerp(startIntesity, endIntensity, time / duration);
            time += Time.deltaTime;

            if (!canStayLit)
                puzzleManagerScript.AllCrystalsLitCheck();

            yield return null;
        }

        crystalLight.intensity = endIntensity;
    
        // Checks what to do after the crystal light has lerped to the "endIntesity"
        if (!canStayLit) // If all crystals are not lit...
        {
            if (duration == lerpDuration)
            {
                // If the crystal light has lerped to it minimum intesity
                if (endIntensity == minLightIntesity)
                {
                    canLerpToMax = true;
                    canResetGlowLength = false;
                    islerpingToMin = false;
                }
                // If the crystal light has lerped to it maximum intesity
                else if (endIntensity == maxLightIntesity)
                {
                    StartCrystalCoroutine(maxLightIntesity, glowDuration);
                    canResetGlowLength = true;
                }
            }
            // If the crystal light has finished "glowing"
            else if (duration == glowDuration)
            {
                StartCrystalCoroutine(minLightIntesity, lerpDuration);
                islerpingToMin = true;
            }
        }
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
    }

}
