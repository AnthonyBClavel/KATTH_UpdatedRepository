using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuAnimEvent : MonoBehaviour
{
    public Animator logoAnim;
    //public GameObject optionsScreen;
    public GameObject safetyMenu;
    private MainMenu mainMenuScript;

    // Start is called before the first frame update
    void Awake()
    {
        mainMenuScript = FindObjectOfType<MainMenu>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetInactiveMMB()
    {
        gameObject.SetActive(false);

        //if (mainMenuScript.isOptionsMenu == true)
            //optionsScreen.SetActive(true);

        if (mainMenuScript.isSafetyMenu == true)
            safetyMenu.SetActive(true);
    }

    public void FadeInLogo()
    {
        if(mainMenuScript.canFadeLogo == true)
            logoAnim.SetTrigger("TC_FadeIn");
    }
    public void FadeOutLogo()
    {
        if (mainMenuScript.canFadeLogo == false)
            logoAnim.SetTrigger("TC_FadeOut");
    }

}
