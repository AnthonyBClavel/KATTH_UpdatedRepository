using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    private bool isActive = false;

    private float fadeAudioLength = 1f;
    private float rotateGearlength = 2f;
    private float originalVolumeGLSFX; // GLSFX = generator loop sfx
    private float finalVolumeGLSFX = 0f; // GLSFX = generator loop sfx

    private Material heatDoorMat;
    private Material generatorLightMat01;
    private Material generatorLightMat02;

    private AudioSource generatorLoopAS; // 0.8f = default volume
    private AudioSource turnOnGeneratorAS; // 1f = default volume
    private GameObject steamFX;
    private GameObject gearsHolder;
    private GameObject gear01;

    private IEnumerator rotateGearsCoroutine;
    private IEnumerator generatorLoopCoroutine;
    private IEnumerator generatorFXCoroutine;
    private AudioManager audioManagerScript;

    void Awake()
    {
        audioManagerScript = FindObjectOfType<AudioManager>();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        // 3 lines below must be called in start!
        turnOnGeneratorAS = audioManagerScript.TurnOnGeneratorAS;
        generatorLoopAS = audioManagerScript.GeneratorLoopAS;
        originalVolumeGLSFX = generatorLoopAS.volume;

        heatDoorMat.DisableKeyword("_EMISSION");
        generatorLightMat01.DisableKeyword("_EMISSION");
        generatorLightMat02.DisableKeyword("_EMISSION");
    }

    // Returns the value of isActive
    public bool IsActive
    {
        get
        {
            return isActive;
        }
    }

    // Turns on the generator immediately
    public void TurnOnGenerator()
    {
        isActive = true;

        generatorLoopAS.volume = 0f;
        turnOnGeneratorAS.Play();
        generatorLoopAS.Play();

        FadeInGeneratorLoop();
        TurnOnGeneratorEffects();
    }

    // Turns off the generator immediately
    public void TurnOffGenerator()
    {
        isActive = false;

        StopAllCoroutines();
        if (generatorLoopCoroutine != null)
            StopCoroutine(generatorLoopCoroutine);
        if (generatorFXCoroutine != null)
            StopCoroutine(generatorFXCoroutine);
        if (rotateGearsCoroutine != null)
            StopCoroutine(rotateGearsCoroutine);
        if (generatorLoopAS.volume != 0f)
            generatorLoopAS.volume = 0f;

        turnOnGeneratorAS.Stop();
        generatorLoopAS.Stop();
        steamFX.SetActive(false);

        heatDoorMat.DisableKeyword("_EMISSION");
        generatorLightMat01.DisableKeyword("_EMISSION");
        generatorLightMat02.DisableKeyword("_EMISSION");
    }

    // Fades in the generatorLoopSFX
    public void FadeInGeneratorLoop()
    {
        if (generatorLoopCoroutine != null)
            StopCoroutine(generatorLoopCoroutine);

        generatorLoopCoroutine = LerpAudio(generatorLoopAS, originalVolumeGLSFX, fadeAudioLength);

        generatorLoopAS.volume = finalVolumeGLSFX;
        StartCoroutine(generatorLoopCoroutine);
    }

    // Fades out the generatorLoopSFX (finalVolume = volume to fade out to)
    public void FadeOutGeneratorLoop(float finalVolume)
    {
        // Only fade out the audio if the generator is active
        if (isActive)
        {
            if (generatorLoopCoroutine != null)
                StopCoroutine(generatorLoopCoroutine);

            finalVolumeGLSFX = finalVolume;
            generatorLoopCoroutine = LerpAudio(generatorLoopAS, finalVolume, fadeAudioLength);

            generatorLoopAS.volume = originalVolumeGLSFX;
            StartCoroutine(generatorLoopCoroutine);
        }
    }

    // Starts the coroutine for the generator effects
    private void TurnOnGeneratorEffects()
    {
        if (generatorFXCoroutine != null)
            StopCoroutine(generatorFXCoroutine);

        generatorFXCoroutine = TurnOnGeneratorFX();
        StartCoroutine(generatorFXCoroutine);
    }

    // Starts the coroutine that rotates the generator gears
    private void RotateGeneratorGears(float duration)
    {
        if (rotateGearsCoroutine != null)
            StopCoroutine(rotateGearsCoroutine);

        rotateGearsCoroutine = LerpGeneratorGears(duration);
        StartCoroutine(rotateGearsCoroutine);
    }

    // Lerps the volume of an audio source over a specific duration (endVolume = volume to lerp to, duration = seconds)
    private IEnumerator LerpAudio(AudioSource audioSource, float endVolume, float duration)
    {
        float time = 0;
        float startVolume = audioSource.volume;

        while (time < duration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, endVolume, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = endVolume;

        // Resets the generator after the audio loop fades out
        if (audioSource == generatorLoopAS && generatorLoopAS.volume == 0f)
            TurnOffGenerator();
    }

    // Lerps the rotation of each generator gear over a specific duration (duration = seconds) - NOTE: rest of gears mimic the rotation of gear01
    private IEnumerator LerpGeneratorGears(float duration)
    {
        float time = 0;
        float startRotationY = gear01.transform.localEulerAngles.y;
        float endRotationY = gear01.transform.localEulerAngles.y + 360f;
        Vector3 originalRotation = gear01.transform.localEulerAngles;

        while (time < duration)
        {
            float yRotation = Mathf.Lerp(startRotationY, endRotationY, time / duration);
            for (int i = 0; i < gearsHolder.transform.childCount; i++)
            {
                GameObject child = gearsHolder.transform.GetChild(i).gameObject;
                string childName = child.name;
                if (childName == "Gear01" || childName == "Gear03" || childName == "Gear05")
                    child.transform.localEulerAngles = new Vector3(originalRotation.x, yRotation, originalRotation.x); // Rotate right
                if (childName == "Gear02" || childName == "Gear04")
                    child.transform.localEulerAngles = new Vector3(originalRotation.x, -yRotation, originalRotation.x); // Rotate left
            }
            time += Time.deltaTime;
            yield return null;
        }

        gear01.transform.localEulerAngles = originalRotation;
        RotateGeneratorGears(rotateGearlength);
    }

    // Turns on the generator effects after certain time intervals
    private IEnumerator TurnOnGeneratorFX()
    {
        // Turns on emission within the light bulb materials
        generatorLightMat01.EnableKeyword("_EMISSION");
        generatorLightMat02.EnableKeyword("_EMISSION");

        yield return new WaitForSeconds(0.7f);
        // Turns on emmission in the heat door material
        heatDoorMat.EnableKeyword("_EMISSION");
        RotateGeneratorGears(rotateGearlength);

        yield return new WaitForSeconds(0.2f);
        // Sets the steam particles active
        steamFX.SetActive(true);
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
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

                        for (int l = 0; l < furnaceHolder.transform.childCount; l++)
                        {
                            GameObject child04 = furnaceHolder.transform.GetChild(l).gameObject;

                            if (child04.name == "HeatDoor")
                                heatDoorMat = child04.GetComponent<MeshRenderer>().material;
                        }
                    }

                    if (child02.name == "Gears")
                    {
                        gearsHolder = child02;

                        for (int m = 0; m < gearsHolder.transform.childCount; m++)
                        {
                            GameObject child05 = gearsHolder.transform.GetChild(m).gameObject;

                            if (child05.name == "Gear01")
                                gear01 = child05;
                        }
                    }
                }
            }
        }
    }

    // Lerps the rotation of an object over a specific duration (endRotation = rotation to lerp to, duration = seconds) - OLD VERSION
    /*private IEnumerator LerpGeneratorGear(GameObject gear, float rotationAmount, float duration)
    {
        float time = 0;
        float startRotationY = gear.transform.localEulerAngles.y;
        float endRotationY = gear.transform.localEulerAngles.y + rotationAmount;
        Vector3 originalRotation = gear.transform.localEulerAngles;

        while (time < duration)
        {
            float yRotation = Mathf.Lerp(startRotationY, endRotationY, time / duration);
            gear.transform.localEulerAngles = new Vector3(originalRotation.x, yRotation, originalRotation.x);
            time += Time.deltaTime;
            yield return null;
        }

        gear.transform.localEulerAngles = originalRotation;
        RotateGeneratorGear(gear, rotationAmount, rotateGearlength);
    }*/

}