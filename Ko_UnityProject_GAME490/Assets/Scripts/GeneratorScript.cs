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
    public GameObject initialGeneratorLoopSFX;
    public GameObject generatorLoopSFX;

    public bool canInteract;

    private float duration;

    private Animator anim;
    private string currentState;

    // Start is called before the first frame update
    void Start()
    {
        canInteract = true;
        duration = initialGeneratorLoopSFX.GetComponent<AudioSource>().clip.length;

        heaterDoorMat.DisableKeyword("_EMISSION");
        generatorLightMat.DisableKeyword("_EMISSION");
        anim = GetComponentInChildren<Animator>();     
    }

    // Update is called once per frame
    void Update()
    {
        //use lines below for generator debugging
        /*
        if (Input.GetKeyDown(KeyCode.L))
        {
            TurnOnGenerator();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            TurnOffGenerator();
        } */

    }

    //turns on the generator
    public void TurnOnGenerator()
    {
        canInteract = false;
        turnOnGeneratorSFX.SetActive(true);
        initialGeneratorLoopSFX.SetActive(true);

        StartCoroutine("DelayGeneratorParticles");
        StartCoroutine("PlayGeneratorLoopAfterInitialSFX");

        generatorLightMat.EnableKeyword("_EMISSION");
        ChangeAnimationState("GeneratorGears");
    }

    //turns off the generator
    public void TurnOffGenerator()
    {
        canInteract = true;
        StopCoroutine("DelayGeneratorParticles");
        StopCoroutine("PlayGeneratorLoopAfterInitialSFX");

        turnOnGeneratorSFX.SetActive(false);
        initialGeneratorLoopSFX.SetActive(false);
        generatorLoopSFX.SetActive(false);
        steamParticle01.SetActive(false);
        steamParticle02.SetActive(false);

        heaterDoorMat.DisableKeyword("_EMISSION");
        generatorLightMat.DisableKeyword("_EMISSION");
        ChangeAnimationState("GeneratorIdle");
    }

    //function that changes the animation state of the generator
    private void ChangeAnimationState(string newState)
    {
        anim.Play(newState);
        currentState = newState;
    }

    //delays the generator's particles from starting after generator is turned on
    private IEnumerator DelayGeneratorParticles()
    {
            yield return new WaitForSecondsRealtime(0.7f);
            heaterDoorMat.EnableKeyword("_EMISSION");
            yield return new WaitForSecondsRealtime(0.2f);
            steamParticle01.SetActive(true);
            steamParticle02.SetActive(true);
    }

    //plays the clean looping sfx after the intitial looping sfx (the initial one's volume fades in; woudln't be ideal to loop that)
    private IEnumerator PlayGeneratorLoopAfterInitialSFX()
    {
            yield return new WaitForSecondsRealtime(duration);
            initialGeneratorLoopSFX.SetActive(false);
            generatorLoopSFX.SetActive(true);
    }
}
