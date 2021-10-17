using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour
{
    private bool canFadeInCrystalLight = true;
    private bool canResetCrystalLight = false;
    private bool canStayLit = false;
    private bool isFadingOut = false;

    //[Range(0.1f, 3f)]
    private float fadeLength = 0.5f;
    //[Range(0f, 5f)]
    private float glowLength = 3f;
    //[Range(0f, 1f)]
    private float minLightIntesity = 0.1f;
    //[Range(0f, 1f)]
    private float maxLightIntesity = 1f;

    private GameObject parentObject;
    private Light crystalLight;
    private IEnumerator crystalLightCoroutine;
    private PuzzleManager puzzleManagerScript;

    void Awake()
    {
        puzzleManagerScript = FindObjectOfType<PuzzleManager>();
        SetElements();
    }

    // Checks if the crystal's light component has an intesity value of 1f
    public bool CrystalLightCheck()
    {
        if (crystalLight.intensity > minLightIntesity)
            return true;
        else
            return false;
    }

    // Checks to fade in or reset the crystal light
    public void PlayCrystalLightAnim()
    {
        if (canFadeInCrystalLight)
        {
            StartCrystalCoroutine(maxLightIntesity, fadeLength);
            canFadeInCrystalLight = false;
        }
        if (canResetCrystalLight)
            StartCrystalCoroutine(maxLightIntesity, glowLength);
    }

    // Resets the crytsal's light component and bools
    public void ResetCrystalLight()
    {
        StopAllCoroutines();
        crystalLight.intensity = minLightIntesity;
        canFadeInCrystalLight = true;
        canResetCrystalLight = false;
        canStayLit = false;
        isFadingOut = false;
    }

    // Prevents the crystal light from fading in or resetting - keeps it lit
    public void LeaveCrystalLightOn()
    {
        canFadeInCrystalLight = false;
        canResetCrystalLight = false;

        if (isFadingOut)
        {
            StopAllCoroutines();
            crystalLight.intensity = maxLightIntesity;
        }
    }

    // Checks if all crystals within a puzzle are lit
    private void AllCrystalsLitCheck()
    {
        if (!canStayLit)
        {
            bool allCrystalsLit = true;

            // If any crystal's light is not greater than the minLightIntesity, then allCrystalsLit is false
            for (int j = 0; j < parentObject.transform.childCount; j++)
            {
                GameObject child = parentObject.transform.GetChild(j).gameObject;

                if (child.activeSelf && child.name.Contains("Crystal") && child.GetComponent<Crystal>().CrystalLightCheck() == false)
                    allCrystalsLit = false;
            }
            // If every crystal's light is greater than the minLightIntesity
            if (allCrystalsLit)
            {
                //Debug.Log("Have lit all crystals!");
                puzzleManagerScript.PlayAllCrystalsLitSFX();

                for (int k = 0; k < parentObject.transform.childCount; k++)
                {
                    GameObject child = parentObject.transform.GetChild(k).gameObject;

                    if (child.activeSelf && child.name.Contains("Crystal"))
                        child.GetComponent<Crystal>().LeaveCrystalLightOn();

                    canStayLit = true;
                }
            }
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
        if (canResetCrystalLight)
            crystalLight.intensity = maxLightIntesity;

        float time = 0;
        float startIntesity = crystalLight.intensity;

        while (time < duration)
        {
            crystalLight.intensity = Mathf.Lerp(startIntesity, endIntensity, time / duration);
            time += Time.deltaTime;
            AllCrystalsLitCheck();
            yield return null;
        }

        crystalLight.intensity = endIntensity;
    
        if (!canStayLit)
        {
            if (time < glowLength)
            {
                // Checks to reset the crystal bools
                if (crystalLight.intensity == minLightIntesity)
                {
                    canFadeInCrystalLight = true;
                    canResetCrystalLight = false;
                    isFadingOut = false;
                }
                // Sets the crystal light to glow over a duration (crystalGlowLength)
                if (crystalLight.intensity == maxLightIntesity)
                {
                    StartCrystalCoroutine(maxLightIntesity, glowLength);
                    canResetCrystalLight = true;
                }
            }
            // Checks to fade out the crystal light over a duration (crystalFadeLength)
            else if (time >= glowLength)
            {
                StartCrystalCoroutine(minLightIntesity, fadeLength);
                isFadingOut = true;
            }
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
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

        parentObject = transform.parent.gameObject;
        // OR USE crystalLight = GetComponentInChildren<Light>();
    }

}
