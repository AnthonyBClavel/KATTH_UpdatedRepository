using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IceMaterialScript : MonoBehaviour
{
    public Material iceMaterial;
    public GameObject coldUI;
   
    private Image image;
    private float iceMaterialAlpha;
    private float coldUIAlpha = 0f;

    // Start is called before the first frame update
    void Start()
    {
        image = coldUI.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        image.color = new Color(1, 1, 1, coldUIAlpha);

        iceMaterial.SetFloat("Vector1_FCC70E1D", iceMaterialAlpha);
    }

    // Increases the alpha of the ice material over time until it reaches its max value
    public IEnumerator FadeMaterialToFullAlpha()
    {
        for (float i = 0f; i <= 1; i += 0.05f)
        {
            i = iceMaterialAlpha;
            iceMaterialAlpha += 0.05f;
            yield return new WaitForSeconds(0.05f);
        }
    }

    // Sets the alpha of the ice material back to 0 after a delay
    public IEnumerator ResetPlayerMaterial()
    {
        yield return new WaitForSeconds(1.5f);
        iceMaterialAlpha = 0f;
    }

    // Sets the alpha of the ice material back to 0 immediately
    public void ResetPlayerMaterial02()
    {
        iceMaterialAlpha = 0f;
    }

    // Increases the alpha of the Cold UI over time until it reaches its max value
    public IEnumerator IncreaseAlpha_ColdUI()
    {
        for (float i = 0f; i <= 1; i += 0.02f)
        {
            i = coldUIAlpha;
            coldUIAlpha += 0.025f;
            yield return new WaitForSeconds(0.02f);
        }
    }

    // Sets the alpha of the Cold UI back to 0 after a delay
    public IEnumerator ResetUIAlpha_ColdUI()
    {
        yield return new WaitForSeconds(1.5f);
        coldUIAlpha = 0f;
    }

    // Sets the alpha of the Cold UI back to 0 immediately
    public void ResetUIAlpha_ColdUI02()
    {
        coldUIAlpha = 0f;
    }

}
