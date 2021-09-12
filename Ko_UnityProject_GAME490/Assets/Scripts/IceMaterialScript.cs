using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IceMaterialScript : MonoBehaviour
{
    public bool isFrozen = false;

    private GameObject frostedBorder;
    private Material iceMaterial;
    private Image frostedBorderSprite;

    private float iceMaterialAlpha = 0f;
    private float frostedBorderAlpha = 0f;

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
        iceMaterial = gameManagerScript.iceMaterial;
        frostedBorderSprite = frostedBorder.GetComponent<Image>();

        frostedBorderSprite.color = new Color(1, 1, 1, frostedBorderAlpha);
        iceMaterial.SetFloat("Vector1_FCC70E1D", iceMaterialAlpha);
    }

    // Sets the alpha of the ice material back to zero
    public void ResetIceMaterial()
    {
        isFrozen = false;
        iceMaterialAlpha = 0f;
        iceMaterial.SetFloat("Vector1_FCC70E1D", iceMaterialAlpha);
    }

    // Sets the frosted border's alpha back to zero
    public void ResetFrostedBorderAlpha()
    {
        isFrozen = false;
        frostedBorderAlpha = 0f;
        frostedBorderSprite.color = new Color(1, 1, 1, frostedBorderAlpha);
    }

    // Increases the ice material's alpha over time until it reaches its max value
    public IEnumerator IncreaseIceMaterialAlpha()
    {
        isFrozen = true;

        for (float i = 0f; i <= 1; i += 0.05f)
        {
            i = iceMaterialAlpha;
            iceMaterialAlpha += 0.05f;
            iceMaterial.SetFloat("Vector1_FCC70E1D", iceMaterialAlpha);
            yield return new WaitForSeconds(0.05f);
        }
    }

    // Increases the frosted border's alpha over time until it reaches its max value
    public IEnumerator IncreaseFrostedBorderAlpha()
    {
        isFrozen = true;

        for (float i = 0f; i <= 1; i += 0.02f)
        {
            i = frostedBorderAlpha;
            frostedBorderAlpha += 0.025f;
            frostedBorderSprite.color = new Color(1, 1, 1, frostedBorderAlpha);
            yield return new WaitForSeconds(0.02f);
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < gameHUDScript.transform.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.GetChild(i).gameObject;

            if (child.name == "FrostedBorder")
                frostedBorder = child;
        }
    }

    // Sets the ice material's alpha back to zero after a delay
    /*public IEnumerator ResetIceMaterial02()
    {
        yield return new WaitForSeconds(1.5f);
        isFrozen = false;
        iceMaterialAlpha = 0f;
        iceMaterial.SetFloat("Vector1_FCC70E1D", iceMaterialAlpha);
    }*/

    // Sets the frosted border's alpha back to zero after a delay
    /*public IEnumerator ResetFrostedBorderAlpha02()
    {
        yield return new WaitForSeconds(1.5f);
        isFrozen = false;
        frostedBorderAlpha = 0f;
        frostedBorderSprite.color = new Color(1, 1, 1, frostedBorderAlpha);
    }*/

}
