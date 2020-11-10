using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFade : MonoBehaviour
{ 
    public Animator animator;                                   //variable for the animator component

    private MainMenu mainMenu;                                  //variable for the specified script

    private PauseMenu01 pauseMenu;                              //variable for the specified script

    // Start is called before the first frame update
    void Start()
    {
        mainMenu = FindObjectOfType<MainMenu>();                //sets the variable to the object with the specified script

        pauseMenu = FindObjectOfType<PauseMenu01>();            //sets the variable to the object with the specified script
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void FadeOutOfMainMenu()                             //the function for fading out of the main menu
    {
        animator.SetTrigger("FadeOutMain");                     //trigger the "FadeOutMain" trigger within the animator window
    }

    public void FadeOutOfLevel()                                //the function for fading out of the pause menu (out of the level)
    {
        animator.SetTrigger("FadeOutLevel");                    //trigger the "FadeOutLevel" trigger within the animator window
        Time.timeScale = 1f;                                    //sets the time scale back to normal (1f = realtime)
    }

    public void OnFadeCompleteForMain()                         //the function that calls another script's function (for fading out of main menu)
    {
        mainMenu.NewGame();                                     //call the function with the specified/external script
    }

    public void OnFadeCompleteForPause()                        //the function that calls another script's function (for fading out of pause menu)
    {
        pauseMenu.QuitToMain();                                 //call the function with the specified/external script
    }
}
