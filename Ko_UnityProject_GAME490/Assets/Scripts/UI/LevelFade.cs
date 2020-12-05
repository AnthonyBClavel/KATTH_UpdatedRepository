using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFade : MonoBehaviour
{ 
    public Animator animator;                                

    private MainMenu mainMenu;                                  
    private PauseMenu01 pauseMenu;
    private LevelManager levelManager;

    // Start is called before the first frame update
    void Start()
    {
        mainMenu = FindObjectOfType<MainMenu>();               

        pauseMenu = FindObjectOfType<PauseMenu01>();

        levelManager = FindObjectOfType<LevelManager>();
    }

    //function that triggers the "FadeOutMain" animation (fade)
    public void FadeOutOfGame()
    {
        animator.SetTrigger("FadeOutGame");
    }

    //function that triggers the "FadeOutContinue" animation (fade)
    public void FadeOutContinueGame()
    {
        animator.SetTrigger("FadeOutContinue");
    }

    //function that triggers the "FadeOutMain" animation (fade)
    public void FadeOutOfMainMenu()                             
    {
        animator.SetTrigger("FadeOutMain");                     
    }

    //function that triggers the "FadeOutOfLevel" animation (fade)
    public void FadeOutOfLevel()
    {
        Time.timeScale = 1f;
        animator.SetTrigger("FadeOutLevel");
    }

    //function that triggers the "FadeOutToNextLevel" animation (fade)
    public void FadeOutToNextLevel()
    {
        animator.SetTrigger("FadeOutNextLevel");
    }

    //calls the "QuitGame" function in the main menu script
    public void OnFadeCompleteForGame()
    {
        mainMenu.QuitGame();
    }

    //calls the "ContinueGame" function in the main menu script
    public void OnFadeCompleteContinueButton()
    {
        mainMenu.ContinueGame();
    }

    //calls the "NewGame" function in the main menu script
    public void OnFadeCompleteForMain()                         
    {
        mainMenu.NewGame();                                     
    }

    //calls the "QuitToMain" function in the pause menu
    public void OnFadeCompleteForPause()                       
    {
        pauseMenu.QuitToMain();                                
    }

    //calls the "LoadNextLevel" coroutine in the pause menu
    public void OnFadeCompleteForLevel()
    {
        levelManager.StartCoroutine("LoadNextLevelAsync");
    }

}
