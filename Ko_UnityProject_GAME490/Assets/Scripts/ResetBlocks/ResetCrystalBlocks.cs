using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetCrystalBlocks : MonoBehaviour
{
    private bool canPlaySFX;
    private AudioSource chimeAudioSource;

    private AudioLoops audioLoopsScript;

    void Awake()
    {
        audioLoopsScript = FindObjectOfType<AudioLoops>();
    }

    // Start is called before the first frame update
    void Start()
    {
        chimeAudioSource = audioLoopsScript.chimeSFX;
        canPlaySFX = false;
    }

    // Update is called once per frame
    void Update()
    {
        CheckCrystalIntesities();
    }

    // Resets the idle animation of the crystal light - crystal will not fade out until its idle animation is completed
    public void ResetCrystalIdle()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.GetComponentInChildren<CrystalsManager>().ResetCrystalIdleAnim();
        }
    }

    // Checks to see if each crystal's light component has an intesity value of 1f
    public void CheckCrystalIntesities()
    {
        bool allLit = true;

        for (int i = 0; i < transform.childCount; i++)
        {
            if (!transform.GetChild(i).gameObject.GetComponentInChildren<CrystalsManager>().crystalGlow.activeSelf)
                allLit = false;                    
        }

        if (allLit)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.GetComponentInChildren<CrystalsManager>().DisableCrystalFade();
                transform.GetChild(i).gameObject.GetComponentInChildren<CrystalsManager>().ResetCrystalIdleAnim();

                if (!canPlaySFX)
                {
                    chimeAudioSource.Play();
                    canPlaySFX = true;
                }
            }
        }
    }

    // Sets the crystal light to inactive and resets its glowing bool to false
    public void ResetCrystals()
    {
        Debug.Log("Num of Crystal blocks: " + transform.childCount);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.GetComponentInChildren<CrystalsManager>().SetGlowInactive();
            transform.GetChild(i).gameObject.GetComponentInChildren<CrystalsManager>().ResetCrystalBool();
            canPlaySFX = false;
            //transform.GetChild(i).gameObject.GetComponentInChildren<CrystalsManager>().EnableCrystalFade();
        }
    }

    // Sets the crystal light to inactive and resets its glowing bool to false after a certain amount of seconds
    public IEnumerator ResetCrystalsWithDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Debug.Log("Num of Crystal blocks: " + transform.childCount);
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.GetComponentInChildren<CrystalsManager>().SetGlowInactive();
            transform.GetChild(i).gameObject.GetComponentInChildren<CrystalsManager>().ResetCrystalBool();
            canPlaySFX = false;
            //transform.GetChild(i).gameObject.GetComponentInChildren<CrystalsManager>().EnableCrystalFade();
        }
    }

}
