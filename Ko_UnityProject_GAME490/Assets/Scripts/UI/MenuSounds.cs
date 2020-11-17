using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSounds : MonoBehaviour
{
    //used to prevent the player from spamming the button press sound
    private bool pressedSFX;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //this is for reseting the pressed sfx when exiting and then immediately returning to the options menu
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Mouse0))
        {
            pressedSFX = false;
        }
    }

    //While in a level(any level) scene...
    /* the function plays a sound whenever you havn't pressed enter
     * if you have, then the function doesnt play a sound
     * this is done to prevent two sounds from playing at the same time when mutiple animation events are played */
    void PlayPauseMenuSound(AudioClip whichSound)
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.Escape))
        {
            pressedSFX = false;
        }
        if (!pressedSFX)
        {
            FindObjectOfType<PauseMenu01>().GetComponent<AudioSource>().PlayOneShot(whichSound);
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            pressedSFX = true;
        }
        else
        {
            return;
        }
    }
   
    //While in the main menu scene...
    /* the function plays a sound whenever you havn't pressed enter
     * if you have, then the function doesnt play a sound
     * this is done to prevent two sounds from playing at the same time when mutiple animation events are played */
    void PlayMainMenuSound(AudioClip whichSound)
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.Escape))
        {
            pressedSFX = false;
        }
        if (!pressedSFX)
        {
            FindObjectOfType<MainMenu>().GetComponent<AudioSource>().PlayOneShot(whichSound);
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            pressedSFX = true;
        }
        else
        {
            return;
        }
    }

}
