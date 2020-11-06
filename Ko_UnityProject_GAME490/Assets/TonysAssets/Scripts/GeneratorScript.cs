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

    private float duration;

    private Animator anim;
    private string currentState;

    // Start is called before the first frame update
    void Start()
    {
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

    public void TurnOnGenerator()
    {
        turnOnGeneratorSFX.SetActive(true);
        initialGeneratorLoopSFX.SetActive(true);

        StartCoroutine("DelayGeneratorParticles");
        StartCoroutine("PlayGeneratorLoopAfterInitialSFX");

        generatorLightMat.EnableKeyword("_EMISSION");
        ChangeAnimationState("GeneratorGears");
    }

    public void TurnOffGenerator()
    {
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


    private void ChangeAnimationState(string newState)
    {
        anim.Play(newState);
        currentState = newState;
    }

    private IEnumerator DelayGeneratorParticles()
    {
            yield return new WaitForSeconds(0.7f);
            heaterDoorMat.EnableKeyword("_EMISSION");
            yield return new WaitForSeconds(0.2f);
            steamParticle01.SetActive(true);
            steamParticle02.SetActive(true);
    }

    private IEnumerator PlayGeneratorLoopAfterInitialSFX()
    {
            yield return new WaitForSeconds(duration);
            initialGeneratorLoopSFX.SetActive(false);
            generatorLoopSFX.SetActive(true);
    }
}
