using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuSounds : MonoBehaviour
{
    private MainMenu mainMenuScript;

    //prevents spamming the button sfx sounds
    private bool pressedSFX;

    // Start is called before the first frame update
    void Start()
    {
        mainMenuScript = FindObjectOfType<MainMenu>();
    }

    // Update is called once per frame
    void Update()
    {
        //this is for reseting the pressed sfx when exiting and then immediately returning to the options menu, after pressing return, and left clicking
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Mouse0))
        {
            pressedSFX = false;
        }
        if (FindObjectOfType<MainMenu>().isOptionsMenu == true)
        {
            pressedSFX = true;
        }

    }

    //function for On Pointer Enter (when mouse hovers over button)
    public void SetPressedSFXToFalse()
    {
        pressedSFX = false;
    }
   
    //While in the main menu scene...
    /* the function plays a sound whenever you havn't pressed enter
     * if you have, then the function doesnt play a sound
     * this is done to prevent two sounds from playing at the same time when mutiple animation events are played */
    public void PlayMainMenuSound(AudioClip whichSound)
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.Escape))
            pressedSFX = false;

        if (!pressedSFX)
            FindObjectOfType<MainMenu>().GetComponent<AudioSource>().PlayOneShot(whichSound);

        else return;

        pressedSFX = true;
    }

}
