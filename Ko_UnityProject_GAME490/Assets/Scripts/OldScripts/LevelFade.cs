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

    private MainMenu mainMenuScript;                                  
    private PauseMenu pauseMenuScript;
    private TileMovementController playerScript;
    private GameManager gameManagerScript;

    // Start is called before the first frame update
    void Start()
    {
        SetScripts();
    }

    // Sets the bools to true after the initial fade in is complete - for an animation event
    public void SetBoolsToTrue()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            pauseMenuScript.CanPause = true; // Might need to refine later - works in tutorial because pause menu script component is disabled during dialogue
            //playerSoundsScript.canPlaySecondFootstep = true; // This is only false when the player enters and leaves a scene
        }         
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
        mainMenuScript.DisableMenuInputMM();
        animator.SetTrigger("FadeOutContinue");
    }

    // Triggers the "FadeOutMain" animation (fade)
    public void FadeOutOfMainMenu()                             
    {
        disableMenuInputs();
        mainMenuScript.DisableMenuInputMM();
        animator.SetTrigger("FadeOutMain");                     
    }

    // Triggers the "FadeOutOfLevel" animation (fade)
    public void FadeOutOfLevel()
    {
        //playerScript.canSetBoolsTrue = false;
        //pauseMenuScript.isChangingScenes = true;
        disableMenuInputs();
        Time.timeScale = 1f;
        animator.SetTrigger("FadeOutLevel");
    }

    // Triggers the "FadeOutToNextLevel" animation (fade)
    public void FadeOutToNextLevel()
    {
        animator.SetTrigger("FadeOutNextLevel");
    }

    // Calls the "QuitGame" function in the main menu script
    public void OnFadeCompleteForGame()
    {
        mainMenuScript.QuitGame();
    }

    // Calls the "ContinueGame" function in the main menu script
    public void OnFadeCompleteContinueButton()
    {
        mainMenuScript.ContinueGame();
    }

    // Calls the "NewGame" function in the main menu script
    public void OnFadeCompleteForMain()                         
    {
        mainMenuScript.NewGame();                                     
    }

    // Calls the "QuitToMain" function in the pause menu
    public void OnFadeCompleteForPause()                       
    {
        //pauseMenuScript.QuitToMain();
        gameManagerScript.LoadNextSceneCheck();
    }

    // Calls the "LoadNextLevel" coroutine in the pause menu
    public void OnFadeCompleteForLevel()
    {
        gameManagerScript.LoadNextSceneCheck();
    }

    // Re-enables all inputs
    public void enableMenuInputs()
    {
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = true;
    }

    // Disables all inputs
    public void disableMenuInputs()
    {
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = false;      
    }

    private void SetScripts()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName != "MainMenu")
        {
            playerScript = FindObjectOfType<TileMovementController>();
            pauseMenuScript = FindObjectOfType<PauseMenu>();
            gameManagerScript = FindObjectOfType<GameManager>();
        }
        if (sceneName == "MainMenu")
        {
            mainMenuScript = FindObjectOfType<MainMenu>();
        }
    }

}
