using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuAnimEvent : MonoBehaviour
{
    private Animator logoAnim;
    //public GameObject safetyMenu;
    //public GameObject optionsScreen;

    private MainMenu mainMenuScript;

    void Awake()
    {
        mainMenuScript = FindObjectOfType<MainMenu>();
        logoAnim = mainMenuScript.titleScreenLogoAnim;
    }

    // Sets the main menu button canvas inactive if the continue button is active - for anim event
    public void SetInactiveMMB()
    {
        if (mainMenuScript.canShowContinueButton)
        {            
            /*if (mainMenuScript.isSafetyMenu)
                safetyMenu.SetActive(true);

            if (mainMenuScript.isOptionsMenu)
                optionsScreen.SetActive(true);*/

            gameObject.SetActive(false);
        }
    }

    // Sets the main menu button canvas inactive if the continue button is inactive - for anim event
    public void SetInactiveMMB02()
    {
        if (!mainMenuScript.canShowContinueButton)
        {
            /*if (mainMenuScript.isSafetyMenu)
                safetyMenu.SetActive(true);

            if (mainMenuScript.isOptionsMenu)
                optionsScreen.SetActive(true);*/

            gameObject.SetActive(false);
        }
    }

    // Checks if the game logo can fade in - for anim event
    public void FadeInLogo()
    {
        if (mainMenuScript.canFadeLogo)
            logoAnim.SetTrigger("TC_FadeIn");
    }

    // Checks if the game logo can fade out - for anim event
    public void FadeOutLogo()
    {
        if (!mainMenuScript.canFadeLogo)
            logoAnim.SetTrigger("TC_FadeOut");
    }

}
