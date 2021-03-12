using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuSounds : MonoBehaviour
{
    private PauseMenu pauseMenuScript;
    private MainMenu mainMenuScript;
    private AudioSource audioSource;
    private bool pressedSFX; // To prevent spamming the buttonSFX
    private bool hasClicked;

    void Awake()
    {
        SetAudioSourceAndScripts();
    }

    // Start is called before the first frame update
    void Start()
    {
        pressedSFX = true;
        hasClicked = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Resets the pressedSFX bool back to false if any input is recieved
        /*if (Input.anyKey || Input.anyKeyDown)
        {
            pressedSFX = false;
        }*/

        DeterminePressedSFX();
    }

    // Function for On Pointer Enter component - when the mouse hovers over button
    public void SetHasClickedToTrue()
    {
        hasClicked = true;
    }

    // Just for the credits button in main menu - this will get removed when we add a credits
    public void SetHasClickedToFalse()
    {
        hasClicked = false;
    }

    // Plays an sfx if the bool is false, this function plays via animation event (in the button's animation)
    // AudioClip determined in the animation event as well
    public void PlayMenuSound(AudioClip whichSound)
    {
        if (!pressedSFX && !hasClicked)
            audioSource.PlayOneShot(whichSound);
    }
    public void PlayMenuSound02(AudioClip whichSound)
    {
        if (!pressedSFX)
            audioSource.PlayOneShot(whichSound);
    }

    // Plays the button click sfx from the correct audio source in each scene
    public void PlayButtonClickSFX()
    {
        if (!pressedSFX)
        {
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                audioSource.PlayOneShot(mainMenuScript.buttonClickSFX);
            }
            else
            {
                audioSource.PlayOneShot(pauseMenuScript.buttonClickSFX);
            }
        }
    }


    // Sets the audioSource to the appropriate correct audio component in each scene
    private void SetAudioSourceAndScripts()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            mainMenuScript = FindObjectOfType<MainMenu>();
            audioSource = FindObjectOfType<MainMenu>().gameObject.GetComponent<AudioSource>();
        }
        else
        {
            pauseMenuScript = FindObjectOfType<PauseMenu>();
            audioSource = FindObjectOfType<PauseMenu>().gameObject.GetComponent<AudioSource>();
        }
    }

    // Determines the states at which pressedSFX is true and false - false while changing menus or in options menu
    private void DeterminePressedSFX()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            if (!mainMenuScript.canPlayButtonSFX)
            {
                pressedSFX = true;
                hasClicked = false;
            }             
            else
                pressedSFX = false;
        }
        else
        {
            if (!pauseMenuScript.canPlayButtonSFX)
            {
                pressedSFX = true;
                hasClicked = false;
            }               
            else
                pressedSFX = false;
        }
    }

}
