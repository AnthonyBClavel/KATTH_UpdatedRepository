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
    public GameObject genertaorSFX02;

    public bool canInteract;
    private float loopingGeneratorSFX;
    private float loopingGeneratorSFX02;
    private string currentState;
    private Animator anim;


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
        genertaorSFX02.GetComponent<AudioSource>().volume = loopingGeneratorSFX02;

        /*** For Debugging purposes ***/
        // Note: all generators in the scene will turn on when use this debug - sounds will be loud
        /*if (Input.GetKeyDown(KeyCode.L))
        {
            TurnOnGenerator();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            TurnOffGenerator();
        }
        /*** End Debugging ***/

    }

    // Turns on the generator immediately
    public void TurnOnGenerator()
    {
        canInteract = false;
        turnOnGeneratorSFX.SetActive(true);
        generatorSFX.SetActive(true);
        genertaorSFX02.SetActive(true);
        loopingGeneratorSFX = 0.0f;
        loopingGeneratorSFX02 = 0.0f;
        StartCoroutine("FadeInGeneratorVolume");
        StartCoroutine("DelayGeneratorParticles");

        generatorLightMat.EnableKeyword("_EMISSION");
        ChangeAnimationState("GeneratorGears");
    }

    // Turns off the generator immediately
    public void TurnOffGenerator()
    {
        canInteract = true;
        StopCoroutine("DelayGeneratorParticles");
        StopCoroutine("FadeInGeneratorVolume");

        turnOnGeneratorSFX.SetActive(false);
        generatorSFX.SetActive(false);
        genertaorSFX02.SetActive(false);
        steamParticle01.SetActive(false);
        steamParticle02.SetActive(false);

        heaterDoorMat.DisableKeyword("_EMISSION");
        generatorLightMat.DisableKeyword("_EMISSION");
        ChangeAnimationState("GeneratorIdle");
    }

    // Resets the generator's emmisive textures to default settings after a delay
    public void TurnOffEmisionAndVolume()
    {
        StopCoroutine("FadeInGeneratorVolume");
        StartCoroutine("TurnOffEmissionAndVolumeDelay");
        StartCoroutine("FadeOutGeneratorVolume");
    }

    // Changes the animation state of the generator
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

    // Increases the generator's volume until it reaches its max value
    private IEnumerator FadeInGeneratorVolume()
    {
        for (float i = 0f; i <= 1; i += 0.02f)
        {
            i = loopingGeneratorSFX;
            loopingGeneratorSFX += 0.02f;
            yield return new WaitForSeconds(0.025f);
        }
    }

    // Decreases the generator's volume until it reaches its min value
   private IEnumerator FadeOutGeneratorVolume()
    {
        for (float i = 1f; i >= 0; i -= 0.02f)
        {
            i = loopingGeneratorSFX02;
            loopingGeneratorSFX02 -= 0.02f;
            yield return new WaitForSeconds(0.025f);
        }
    }

    private IEnumerator TurnOffEmissionAndVolumeDelay()
    {
        generatorSFX.SetActive(false);
        loopingGeneratorSFX = 0f;
        loopingGeneratorSFX02 = 1f;
        yield return new WaitForSeconds(1.6f);
        heaterDoorMat.DisableKeyword("_EMISSION");
        generatorLightMat.DisableKeyword("_EMISSION");
        genertaorSFX02.SetActive(false);
    }

}
