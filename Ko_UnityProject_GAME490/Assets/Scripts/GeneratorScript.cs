using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public class GeneratorScript : MonoBehaviour
{
    public Material heaterDoorMat;
    public Material generatorLightMat;
    public GameObject steamParticle01;
    public GameObject steamParticle02;
    public GameObject turnOnGeneratorSFX;
    public GameObject generatorSFX;

    public bool canInteract;

    private float duration;
    private float loopingGeneratorSFX;

    private Animator anim;
    private string currentState;


    // Start is called before the first frame update
    void Start()
    {
        canInteract = true;

        heaterDoorMat.DisableKeyword("_EMISSION");
        generatorLightMat.DisableKeyword("_EMISSION");
        anim = GetComponentInChildren<Animator>();     
    }

    // Update is called once per frame
    void Update()
    {
        generatorSFX.GetComponent<AudioSource>().volume = loopingGeneratorSFX;

        /*** For Debugging purposes ***/
        /*if (Input.GetKeyDown(KeyCode.L))
        {
            TurnOnGenerator();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            TurnOffGenerator();
        }*/
        /*** End Debugging ***/

    }

    // Turns on the generator
    public void TurnOnGenerator()
    {
        canInteract = false;
        turnOnGeneratorSFX.SetActive(true);
        generatorSFX.SetActive(true);
        loopingGeneratorSFX = 0.0f;
        StartCoroutine("FadeInGeneratorVolume");
        StartCoroutine("DelayGeneratorParticles");

        generatorLightMat.EnableKeyword("_EMISSION");
        ChangeAnimationState("GeneratorGears");
    }

    // Turns off the generator
    public void TurnOffGenerator()
    {
        canInteract = true;
        StopCoroutine("DelayGeneratorParticles");
        StopCoroutine("FadeInGeneratorVolume");

        turnOnGeneratorSFX.SetActive(false);
        generatorSFX.SetActive(false);
        steamParticle01.SetActive(false);
        steamParticle02.SetActive(false);

        heaterDoorMat.DisableKeyword("_EMISSION");
        generatorLightMat.DisableKeyword("_EMISSION");
        ChangeAnimationState("GeneratorIdle");
    }

    // Function that changes the animation state of the generator
    private void ChangeAnimationState(string newState)
    {
        anim.Play(newState);
        currentState = newState;
    }

    // Delays the generator's particles from starting after generator is turned on
    private IEnumerator DelayGeneratorParticles()
    {
        yield return new WaitForSecondsRealtime(0.7f);
        heaterDoorMat.EnableKeyword("_EMISSION");
        yield return new WaitForSecondsRealtime(0.2f);
        steamParticle01.SetActive(true);
        steamParticle02.SetActive(true);
    }

    public IEnumerator FadeInGeneratorVolume()
    {
        for (float i = 0f; i <= 1; i += 0.02f)
        {
            i = loopingGeneratorSFX;
            loopingGeneratorSFX += 0.02f;
            yield return new WaitForSeconds(0.025f);
        }
    }

    // Reset the emmisive textures to default setting
    public void resetEmissiveTextures()
    {
        heaterDoorMat.DisableKeyword("_EMISSION");
        generatorLightMat.DisableKeyword("_EMISSION");
    }

}
