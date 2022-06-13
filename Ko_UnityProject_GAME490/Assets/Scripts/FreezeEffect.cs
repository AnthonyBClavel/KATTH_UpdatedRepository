using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FreezeEffect : MonoBehaviour
{
    private Image frostedBorder;
    private Material iceMaterial;

    private IEnumerator iceMatCoroutine;
    private IEnumerator frostedBorderCoroutine;

    private GameManager gameManagerScript;
    private GameHUD gameHUDScript;

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        iceMaterial.SetFloat("Vector1_FCC70E1D", 0f);
        frostedBorder.SetImageAlpha(0f);
    }

    // Starts the ice-related coroutines
    public void LerpIceAlphas()
    {
        StartIceMatCoroutine();
        StartFrostedBorderCoroutine();
    }

    // Stops the ice-related coroutines and sets their alphas to zero
    public void ResetIceAlphas()
    {
        if (iceMatCoroutine != null)
            StopCoroutine(iceMatCoroutine);

        if (frostedBorderCoroutine != null)
            StopCoroutine(frostedBorderCoroutine);

        iceMaterial.SetFloat("Vector1_FCC70E1D", 0f);
        frostedBorder.SetImageAlpha(0f);
    }

    // Starts the coroutine that lerps the alpha of the ice material
    private void StartIceMatCoroutine()
    {
        if (iceMatCoroutine != null)
            StopCoroutine(iceMatCoroutine);

        iceMatCoroutine = LerpIceMatAlpha(1f);
        StartCoroutine(iceMatCoroutine);
    }

    // Starts the coroutine that lerps the alpha of the frosted border
    private void StartFrostedBorderCoroutine()
    {
        if (frostedBorderCoroutine != null)
            StopCoroutine(frostedBorderCoroutine);

        frostedBorderCoroutine = LerpFrostedBorderAlpha(1f);
        StartCoroutine(frostedBorderCoroutine);
    }

    // Lerps the alpha of the ice material to another over a specific duration (endAlpha = alpha to lerp to)
    // Note: 1f = full alpha, 0f = zero alpha
    private IEnumerator LerpIceMatAlpha(float endAlpha)
    {
        float startAlpha = iceMaterial.GetFloat("Vector1_FCC70E1D");
        float duration = gameManagerScript.resetPuzzleDelay - 0.5f;
        float time = 0;

        while (time < duration)
        {
            iceMaterial.SetFloat("Vector1_FCC70E1D", Mathf.Lerp(startAlpha, endAlpha, time / duration));
            time += Time.deltaTime;
            yield return null;
        }

        iceMaterial.SetFloat("Vector1_FCC70E1D", endAlpha);
    }

    // Lerps the alpha of the frosted border to another over a specific duration (endAlpha = alpha to lerp to)
    // Note: 1f = full alpha, 0f = zero alpha
    private IEnumerator LerpFrostedBorderAlpha(float endAlpha)
    { 
        Color startColor = frostedBorder.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, endAlpha);
        float duration = gameManagerScript.resetPuzzleDelay - 0.5f;
        float time = 0;

        while (time < duration)
        {
            frostedBorder.color = Color.Lerp(startColor, endColor, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        frostedBorder.color = endColor;
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        gameManagerScript = FindObjectOfType<GameManager>();
        gameHUDScript = FindObjectOfType<GameHUD>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets them by looking at the names of children
        for (int i = 0; i < gameHUDScript.transform.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.GetChild(i).gameObject;

            if (child.name == "FrostedBorder")
                frostedBorder = child.GetComponent<Image>();
        }

        iceMaterial = gameManagerScript.iceMaterial;
    }

}
