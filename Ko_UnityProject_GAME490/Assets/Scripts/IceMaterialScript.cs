using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IceMaterialScript : MonoBehaviour
{
    private Material iceMaterial;
    private Image frostedBorderImage;

    private float fullAlpha = 1f;
    private float zeroAlpha = 0f;

    private Color fullAlphaColor;
    private Color zeroAlphaColor;

    private GameManager gameManagerScript;
    private GameHUD gameHUDScript;

    void Awake()
    {
        gameManagerScript = FindObjectOfType<GameManager>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        fullAlphaColor = new Color(1, 1, 1, fullAlpha);
        zeroAlphaColor = new Color(1, 1, 1, zeroAlpha);

        frostedBorderImage.color = zeroAlphaColor;
        iceMaterial.SetFloat("Vector1_FCC70E1D", zeroAlpha);
    }

    // Sets the ice material and frosted border's alpha back to zero
    public void ResetIceAlphas()
    {
        iceMaterial.SetFloat("Vector1_FCC70E1D", zeroAlpha);
        frostedBorderImage.color = zeroAlphaColor;
    }

    public void LerpIceAlphas()
    {
        StartCoroutine(LerpMaterialAlpha(iceMaterial, fullAlpha));
        StartCoroutine(LerpImageAlpha(frostedBorderImage, fullAlphaColor));
    }

    // Lerps the alpha of a material, over a specific duration (endAlpha = alpha to lerp to, duration = seconds)
    private IEnumerator LerpMaterialAlpha(Material material, float endAlpha)
    {
        float time = 0;
        float lerpDuration = gameManagerScript.resetPuzzleDelay - 0.5f;
        float startAlpha = material.GetFloat("Vector1_FCC70E1D");

        while (time < lerpDuration)
        {
            iceMaterial.SetFloat("Vector1_FCC70E1D", Mathf.Lerp(startAlpha, endAlpha, time / lerpDuration));
            time += Time.deltaTime;
            yield return null;
        }

        iceMaterial.SetFloat("Vector1_FCC70E1D", endAlpha);
    }

    // Lerps the alpha of a UI Image, over a specific duration (endColor = alpha to lerp to, duration = seconds)
    private IEnumerator LerpImageAlpha(Image image, Color endColor)
    {
        float time = 0;
        float lerpDuration = gameManagerScript.resetPuzzleDelay - 0.5f;
        Color startColor = image.color;

        while (time < lerpDuration)
        {
            image.color = Color.Lerp(startColor, endColor, time / lerpDuration);
            time += Time.deltaTime;
            yield return null;
        }

        image.color = endColor;
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < gameHUDScript.transform.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.GetChild(i).gameObject;

            if (child.name == "FrostedBorder")
                frostedBorderImage = child.GetComponent<Image>();
        }

        iceMaterial = gameManagerScript.iceMaterial;
    }

}
