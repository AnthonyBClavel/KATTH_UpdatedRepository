﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuSounds : MonoBehaviour
{
    private MainMenu mainMenuScript;
    private bool pressedSFX; //prevents spamming the button sfx sounds

    // Start is called before the first frame update
    void Awake()
    {
        mainMenuScript = FindObjectOfType<MainMenu>();
    }

    // Update is called once per frame
    void Update()
    {
        // Resets the pressedSFX bool back to false based on these inputs
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Mouse0))
        {
            pressedSFX = false;
        }
        if (mainMenuScript.isOptionsMenu == true || mainMenuScript.canPlayButtonSFX == false)
        {
            pressedSFX = true;
        }

    }

    // Function for On Pointer Enter component - when the mouse hovers over button
    public void SetPressedSFXToFalse()
    {
        pressedSFX = false;
    }
   
    // While in the main menu scene...
    /* the function plays a sound whenever you havn't pressed enter
     * if you have, then the function doesnt play a sound
     * this is done to prevent two sounds from playing at the same time when mutiple animation events are played or repeated */
    public void PlayMainMenuSound(AudioClip whichSound)
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
            pressedSFX = false;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            pressedSFX = false;

        if (!pressedSFX)
            mainMenuScript.GetComponent<AudioSource>().PlayOneShot(whichSound);

        else return;

        pressedSFX = true;
    }

}
