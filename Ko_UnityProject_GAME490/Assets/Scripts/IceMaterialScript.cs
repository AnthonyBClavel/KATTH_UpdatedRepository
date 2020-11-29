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

    public IEnumerator FadeMaterialToFullAlpha()
    {
        for (float i = 0f; i <= 1; i += 0.05f)
        {
            i = iceMaterialAlpha;
            iceMaterialAlpha += 0.05f;
            yield return new WaitForSeconds(0.05f);
        }
    }

    public IEnumerator ResetPlayerMaterial()
    {
        yield return new WaitForSeconds(1.5f);
        iceMaterialAlpha = 0f;
    }

    public IEnumerator IncreaseAlpha_ColdUI()
    {
        for (float i = 0f; i <= 1; i -= 0.02f)
        {
            i = coldUIAlpha;
            coldUIAlpha += 0.025f;
            yield return new WaitForSeconds(0.02f);
        }
    }

    public IEnumerator ResetUIAlpha_ColdUI()
    {
        yield return new WaitForSeconds(1.5f);
        coldUIAlpha = 0f;
    }

}
