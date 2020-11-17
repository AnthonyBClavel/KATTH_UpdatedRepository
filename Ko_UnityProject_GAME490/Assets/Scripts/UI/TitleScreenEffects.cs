using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class TitleScreenEffects : MonoBehaviour
{
    public Volume postProcessingVolume;
    private Image image;
    private float alphaLevel = 0.5f;

    private DepthOfField DOP;
    // Start is called before the first frame update
    void Start()
    {
        postProcessingVolume.profile.TryGet(out DOP);
        image = GetComponent<Image>();
       
    }

    // Update is called once per frame
    void Update()
    {
        image.color = new Color(0, 0, 0, alphaLevel);

        //for debugging
        /*if (Input.GetKeyDown(KeyCode.Return))
        {
            StartCoroutine("RemoveBlurredBG");
            StartCoroutine("ReduceBGAlpha");

        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DOP.focalLength.value = 10f;
            alphaLevel = 0.5f;
        }*/
    }

    public void SharpenBG()
    {
        StartCoroutine("RemoveBlurredBG");
        StartCoroutine("ReduceBGAlpha");
    }

    private IEnumerator RemoveBlurredBG()
    {
        for (float i = 10; i >= 1; i -= 0.02f)
        {

            i = DOP.focalLength.value;
            DOP.focalLength.value -= 0.1f;
            yield return new WaitForSeconds(0.02f);
        }
        
    }

    private IEnumerator ReduceBGAlpha()
    {
        for (float i = 0.5f; i >= 0; i -= 0.02f)
        {
            i = alphaLevel;
            alphaLevel -= 0.01f;
            yield return new WaitForSeconds(0.02f);
        }
    }
}
