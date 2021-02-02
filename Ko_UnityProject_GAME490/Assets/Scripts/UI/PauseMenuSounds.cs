using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenuSounds : MonoBehaviour
{
    private PauseMenu01 pauseMenuScript;

    //prevents spamming the button sfx sounds
    private bool pressedSFX;

    // Start is called before the first frame update
    void Start()
    {
        pauseMenuScript = FindObjectOfType<PauseMenu01>();
    }

    // Update is called once per frame
    void Update()
    {
        //this is for reseting the pressed sfx when exiting and then immediately returning to the options menu, after pressing return, and left clicking
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Mouse0))
        {
            pressedSFX = false;
        }
        if (pauseMenuScript.isOptionsMenu == true)
        {
            pressedSFX = true;
        }

    }

    //function for On Pointer Enter (when mouse hovers over button)
    public void SetPressedSFXToFalse()
    {
        pressedSFX = false;
    }


    //While in a level(any level) scene...
    /* the function plays a sound whenever you havn't pressed enter
     * if you have, then the function doesnt play a sound
     * this is done to prevent two sounds from playing at the same time when mutiple animation events are played */
    public void PlayPauseMenuSound(AudioClip whichSound)
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Escape))
            pressedSFX = false;

        if (!pressedSFX)
            pauseMenuScript.GetComponent<AudioSource>().PlayOneShot(whichSound);

        else return;

        pressedSFX = true;
    }

}
