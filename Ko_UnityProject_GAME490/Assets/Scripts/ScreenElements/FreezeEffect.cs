using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FreezeEffect : MonoBehaviour
{
    private float maxAlpha = 1f;
    private float minAlpha = 0f;

    private Image frostedBorder;
    private Material iceMaterial;
    private Material[] playerMats;

    private IEnumerator iceMatCoroutine;
    private IEnumerator frostedBorderCoroutine;

    private TileMovementController playerScript;
    private GameManager gameManagerScript;

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Starts the coroutines that lerp the ice-related alphas
    public void LerpAlphas()
    {
        StartIceMatCoroutine();
        StartFrostedBorderCoroutine();
    }

    // Stops the coroutines that lerp the ice-related alphas and resets their alphas
    public void ResetAlphas()
    {
        StopAllCoroutines();
        iceMaterial.SetFloat("Vector1_FCC70E1D", 0f);
        frostedBorder.SetImageAlpha(0f);
    }

    // Starts the coroutine that lerps ice material's alpha
    private void StartIceMatCoroutine()
    {
        if (iceMatCoroutine != null) StopCoroutine(iceMatCoroutine);

        iceMatCoroutine = LerpIceMatAlpha();
        StartCoroutine(iceMatCoroutine);
    }

    // Starts the coroutine that lerps the frosted border's alpha
    private void StartFrostedBorderCoroutine()
    {
        if (frostedBorderCoroutine != null) StopCoroutine(frostedBorderCoroutine);

        frostedBorderCoroutine = LerpFrostedBorderAlpha();
        StartCoroutine(frostedBorderCoroutine);
    }

    // Lerps the alpha of the ice material to another over a duration
    private IEnumerator LerpIceMatAlpha()
    {
        float duration = gameManagerScript.resetPuzzleDelay - 0.5f;
        float time = 0;

        while (time < duration)
        {
            iceMaterial.SetFloat("Vector1_FCC70E1D", Mathf.Lerp(minAlpha, maxAlpha, time / duration));
            time += Time.deltaTime;
            yield return null;
        }

        iceMaterial.SetFloat("Vector1_FCC70E1D", maxAlpha);
    }

    // Lerps the alpha of the frosted border to another over a duration
    private IEnumerator LerpFrostedBorderAlpha()
    { 
        Color startColor = frostedBorder.ReturnImageColor(minAlpha);
        Color endColor = frostedBorder.ReturnImageColor(maxAlpha);
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
        playerScript = FindObjectOfType<TileMovementController>();
        gameManagerScript = FindObjectOfType<GameManager>();
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "Ko":
                    // Note: use .sharedMaterial to use the original material
                    // Note: use .material to create an instance of the material 
                    playerMats = child.GetComponent<SkinnedMeshRenderer>().sharedMaterials;
                    break;
                default:
                    break;
            }

            if (child.name == "Bip001" || child.parent.name == "Ko") continue;
            SetVariables(child);
        }

        foreach (Material mat in playerMats)
        {
            if (!mat.name.Contains("IceShader_Mat")) continue;
            iceMaterial = mat;
            break;
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        frostedBorder = GetComponent<Image>();
        SetVariables(playerScript.transform);
        ResetAlphas();
    }

}
