using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    private bool isGenActive = false;

    private float fadeDuration = 1f; // Original Value = 1f
    private float rotationDuration = 2f; // Original Value = 2f
    private float maxVolumeGLSFX;
    private float minVolumeGLSFX;
    private float originalVolumeGLSFX; // GLSFX = generator loop sfx

    private GameObject steamFX;
    private GameObject gearsHolder;
    private GameObject gear01;

    private AudioSource generatorLoopAS;
    private AudioSource turnOnGeneratorAS;

    private Material heatDoorMat;
    private Material generatorLightMat01;
    private Material generatorLightMat02;

    private IEnumerator gearsCoroutine;
    private IEnumerator audioCoroutine;
    private IEnumerator effectsCoroutine;
    private AudioManager audioManagerScript;

    public bool IsGenActive
    {
        get { return isGenActive; }
    }

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        // The three lines below MUST be called in start!
        turnOnGeneratorAS = audioManagerScript.TurnOnGeneratorAS;
        generatorLoopAS = audioManagerScript.GeneratorLoopAS;
        originalVolumeGLSFX = generatorLoopAS.volume;
    }

    // Turns on the generator
    public void TurnOnGenerator()
    {
        isGenActive = true;

        turnOnGeneratorAS.Play();
        generatorLoopAS.Play();

        FadeInGeneratorAudio();
        StartEffectsCoroutine();
    }

    // Turns off the generator
    public void TurnOffGenerator()
    {
        isGenActive = false;
        steamFX.SetActive(false);

        StopAllCoroutines();
        DisableMaterialEmissions();

        turnOnGeneratorAS.Stop();
        generatorLoopAS.Stop();

        minVolumeGLSFX = 0f;
        generatorLoopAS.volume = 0f;
    }

    // Fades in the generator audio (endVolume = volume to lerp to)
    public void FadeInGeneratorAudio(float? endVolume = null)
    {
        maxVolumeGLSFX = endVolume ?? originalVolumeGLSFX;

        if (audioCoroutine != null) StopCoroutine(audioCoroutine);
        audioCoroutine = LerpGeneratorAudio(endVolume ?? originalVolumeGLSFX, fadeDuration);
        StartCoroutine(audioCoroutine);
    }

    // Fades out the generator audio (endVolume = volume to lerp to)
    public void FadeOutGeneratorAudio(float? endVolume = null)
    {
        if (!isGenActive) return;
        minVolumeGLSFX = endVolume ?? 0f;

        if (audioCoroutine != null) StopCoroutine(audioCoroutine);
        audioCoroutine = LerpGeneratorAudio(endVolume ?? 0f, fadeDuration);
        StartCoroutine(audioCoroutine);
    }

    // Disables the emission within the materials
    private void DisableMaterialEmissions()
    {
        heatDoorMat.DisableKeyword("_EMISSION");
        generatorLightMat01.DisableKeyword("_EMISSION");
        generatorLightMat02.DisableKeyword("_EMISSION");
    }

    // Starts the coroutine that enables the generator effects
    private void StartEffectsCoroutine()
    {
        if (effectsCoroutine != null)
            StopCoroutine(effectsCoroutine);

        effectsCoroutine = EnableEffects();
        StartCoroutine(effectsCoroutine);
    }

    // Starts the coroutine that rotates the generator gears
    private void StartGearsCoroutine()
    {
        if (gearsCoroutine != null)
            StopCoroutine(gearsCoroutine);

        gearsCoroutine = RotateGears(rotationDuration);
        StartCoroutine(gearsCoroutine);
    }

    // Lerps the volume of the generator over a specific duration (endVolume = volume to lerp to, duration = seconds)
    private IEnumerator LerpGeneratorAudio(float endVolume, float duration)
    {
        generatorLoopAS.volume = (endVolume == maxVolumeGLSFX) ? minVolumeGLSFX : maxVolumeGLSFX;
        float startVolume = generatorLoopAS.volume;
        float time = 0;

        while (time < duration)
        {
            generatorLoopAS.volume = Mathf.Lerp(startVolume, endVolume, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        generatorLoopAS.volume = endVolume;
        if (generatorLoopAS.volume == 0f) TurnOffGenerator();
    }

    // Lerps the rotation of each generator gear to another over a specific duration (duration = seconds)
    private IEnumerator RotateGears(float duration)
    {
        Vector3 originalRotation = gear01.transform.localEulerAngles;
        float startRotationY = originalRotation.y;
        float endRotationY = startRotationY + 360f;
        float time = 0;

        while (time < duration)
        {
            float rotY = Mathf.Lerp(startRotationY, endRotationY, time / duration);

            foreach (Transform child in gearsHolder.transform)
            {
                float newRotY = (child.name == "Gear02" || child.name == "Gear04") ? -rotY : rotY;
                child.localEulerAngles = new Vector3(originalRotation.x, newRotY, originalRotation.z);
            }

            time += Time.deltaTime;
            yield return null;
        }

        gear01.transform.localEulerAngles = originalRotation;
        StartGearsCoroutine();
    }

    // Enables the generator's visual effects (emissive materials, gear animation, particles)
    private IEnumerator EnableEffects()
    {
        generatorLightMat01.EnableKeyword("_EMISSION");
        generatorLightMat02.EnableKeyword("_EMISSION");

        yield return new WaitForSeconds(0.7f);
        heatDoorMat.EnableKeyword("_EMISSION");
        StartGearsCoroutine();

        yield return new WaitForSeconds(0.2f);
        steamFX.SetActive(true);
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        audioManagerScript = FindObjectOfType<AudioManager>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets them by looking at the names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "Generator")
            {
                GameObject generator = child;

                for (int j = 0; j < generator.transform.childCount; j++)
                {
                    GameObject child02 = generator.transform.GetChild(j).gameObject;

                    if (child02.name == "SteamFX")
                        steamFX = child02;

                    if (child02.name == "Lights")
                    {
                        GameObject lightsHolder = child02;

                        for (int k = 0; k < lightsHolder.transform.childCount; k++)
                        {
                            GameObject child03 = lightsHolder.transform.GetChild(k).gameObject;

                            if (child03.name == "LightBulb01")
                                generatorLightMat01 = child03.GetComponent<MeshRenderer>().material;

                            if (child03.name == "LightBulb02")
                                generatorLightMat02 = child03.GetComponent<MeshRenderer>().material;
                        }
                    }

                    if (child02.name == "Furnace")
                    {
                        GameObject furnaceHolder = child02;

                        for (int k = 0; k < furnaceHolder.transform.childCount; k++)
                        {
                            GameObject child03 = furnaceHolder.transform.GetChild(k).gameObject;

                            if (child03.name == "HeatDoor")
                                heatDoorMat = child03.GetComponent<MeshRenderer>().material;
                        }
                    }

                    if (child02.name == "Gears")
                    {
                        gearsHolder = child02;

                        for (int k = 0; k < gearsHolder.transform.childCount; k++)
                        {
                            GameObject child03 = gearsHolder.transform.GetChild(k).gameObject;

                            if (child03.name == "Gear01")
                                gear01 = child03;
                        }
                    }
                }
            }
        }
        DisableMaterialEmissions();
    }

}