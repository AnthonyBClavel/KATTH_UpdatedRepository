using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceMaterialScript : MonoBehaviour
{

    public Material iceMaterial;
    private float iceMaterialAlpha;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
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
}
