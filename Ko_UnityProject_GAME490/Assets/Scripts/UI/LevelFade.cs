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
    private PauseMenu01 pauseMenu;
    private LevelManager levelManager;

    // Start is called before the first frame update
    void Start()
    {
        mainMenu = FindObjectOfType<MainMenu>();               

        pauseMenu = FindObjectOfType<PauseMenu01>();

        levelManager = FindObjectOfType<LevelManager>();
    }

    // Function that triggers the "FadeOutMain" animation (fade)
    public void FadeOutOfGame()
    {
        disableMenuInputs();
        animator.SetTrigger("FadeOutGame");
    }

    // Function that triggers the "FadeOutContinue" animation (fade)
    public void FadeOutContinueGame()
    {
        disableMenuInputs();
        animator.SetTrigger("FadeOutContinue");
    }

    // Function that triggers the "FadeOutMain" animation (fade)
    public void FadeOutOfMainMenu()                             
    {
        disableMenuInputs();
        animator.SetTrigger("FadeOutMain");                     
    }

    // Function that triggers the "FadeOutOfLevel" animation (fade)
    public void FadeOutOfLevel()
    {
        pauseMenu.isChangingScenes = true;
        disableMenuInputs();
        Time.timeScale = 1f;
        animator.SetTrigger("FadeOutLevel");
    }

    // Function that triggers the "FadeOutToNextLevel" animation (fade)
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

    public void enableMenuInputs()
    {
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = true;
        gameCanvas.GetComponentInChildren<CanvasGroup>().blocksRaycasts = true;
    }

    public void disableMenuInputs()
    {
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = false;
        gameCanvas.GetComponentInChildren<CanvasGroup>().blocksRaycasts = false;
    }


}
