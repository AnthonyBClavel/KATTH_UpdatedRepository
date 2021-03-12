using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LevelFade : MonoBehaviour
{ 
    public Animator animator;
    public GameObject gameCanvas;

    private MainMenu mainMenu;                                  
    private PauseMenu pauseMenu;
    private LevelManager levelManager;

    // Start is called before the first frame update
    void Start()
    {
        mainMenu = FindObjectOfType<MainMenu>();               

        pauseMenu = FindObjectOfType<PauseMenu>();

        levelManager = FindObjectOfType<LevelManager>();
    }


    // Triggers the "FadeOutMain" animation (fade)
    public void FadeOutOfGame()
    {
        disableMenuInputs();
        animator.SetTrigger("FadeOutGame");
    }

    // Triggers the "FadeOutContinue" animation (fade)
    public void FadeOutContinueGame()
    {
        disableMenuInputs();
        animator.SetTrigger("FadeOutContinue");
    }

    // Triggers the "FadeOutMain" animation (fade)
    public void FadeOutOfMainMenu()                             
    {
        disableMenuInputs();
        animator.SetTrigger("FadeOutMain");                     
    }

    // Triggers the "FadeOutOfLevel" animation (fade)
    public void FadeOutOfLevel()
    {
        pauseMenu.isChangingScenes = true;
        disableMenuInputs();
        Time.timeScale = 1f;
        animator.SetTrigger("FadeOutLevel");
    }

    // Triggers the "FadeOutToNextLevel" animation (fade)
    public void FadeOutToNextLevel()
    {
        pauseMenu.isChangingScenes = true;
        animator.SetTrigger("FadeOutNextLevel");
    }

    // Calls the "QuitGame" function in the main menu script
    public void OnFadeCompleteForGame()
    {
        mainMenu.QuitGame();
    }

    // Calls the "ContinueGame" function in the main menu script
    public void OnFadeCompleteContinueButton()
    {
        mainMenu.ContinueGame();
    }

    // Calls the "NewGame" function in the main menu script
    public void OnFadeCompleteForMain()                         
    {
        mainMenu.NewGame();                                     
    }

    // Calls the "QuitToMain" function in the pause menu
    public void OnFadeCompleteForPause()                       
    {
        pauseMenu.QuitToMain();                                
    }

    // Calls the "LoadNextLevel" coroutine in the pause menu
    public void OnFadeCompleteForLevel()
    {
        levelManager.StartCoroutine("LoadNextLevelAsync");
    }

    
    // Disables all inputs
    public void enableMenuInputs()
    {
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = true;
        gameCanvas.GetComponentInChildren<CanvasGroup>().blocksRaycasts = true;
    }

    // Re-enables all inputs
    public void disableMenuInputs()
    {
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = false;
        gameCanvas.GetComponentInChildren<CanvasGroup>().blocksRaycasts = false;
    }

}
