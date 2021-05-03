using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IceMaterialScript : MonoBehaviour
{
    public bool isFrozen = false;
    public GameObject coldUI;
    public Material iceMaterial;

    private Image coldUISprite;
    private float iceMaterialAlpha = 0f;
    private float coldUIAlpha = 0f;

    // Start is called before the first frame update
    void Start()
    {
        coldUISprite = coldUI.GetComponent<Image>();
        coldUISprite.color = new Color(1, 1, 1, coldUIAlpha);
        iceMaterial.SetFloat("Vector1_FCC70E1D", iceMaterialAlpha);
    }

    // Increases the alpha of the ice material over time until it reaches its max value
    public IEnumerator FadeMaterialToFullAlpha()
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

    // Sets the alpha of the ice material back to zero
    public void ResetPlayerMaterial02()
    {
        isFrozen = false;
        iceMaterialAlpha = 0f;
        iceMaterial.SetFloat("Vector1_FCC70E1D", iceMaterialAlpha);
    }

    // Increases the alpha of the Cold UI over time until it reaches its max value
    public IEnumerator IncreaseAlpha_ColdUI()
    {
        isFrozen = true;

        for (float i = 0f; i <= 1; i += 0.02f)
        {
            i = coldUIAlpha;
            coldUIAlpha += 0.025f;
            coldUISprite.color = new Color(1, 1, 1, coldUIAlpha);
            yield return new WaitForSeconds(0.02f);
        }
    }

    // Sets the alpha of the Cold UI back to zero
    public void ResetUIAlpha_ColdUI02()
    {
        isFrozen = false;
        coldUIAlpha = 0f;
        coldUISprite.color = new Color(1, 1, 1, coldUIAlpha);
    }

    // Sets the alpha of the ice material back to 0 after a delay
    /*public IEnumerator ResetPlayerMaterial()
    {
        yield return new WaitForSeconds(1.5f);
        isFrozen = false;
        iceMaterialAlpha = 0f;
        iceMaterial.SetFloat("Vector1_FCC70E1D", iceMaterialAlpha);
    }*/

    // Sets the alpha of the Cold UI back to 0 after a delay
    /*public IEnumerator ResetUIAlpha_ColdUI()
    {
        yield return new WaitForSeconds(1.5f);
        isFrozen = false;
        coldUIAlpha = 0f;
        image.color = new Color(1, 1, 1, coldUIAlpha);
    }*/

}
