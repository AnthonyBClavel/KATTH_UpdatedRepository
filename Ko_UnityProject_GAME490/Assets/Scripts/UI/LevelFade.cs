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
    private LevelManager levelManager;
    private TileMovementController playerScript;
    private PlayerSounds playerSoundsScript;
    private GameHUD gameHUDScript;

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
            pauseMenuScript.canPause = true; // Might need to refine later - works in tutorial because pause menu script component is disabled during dialogue
            playerSoundsScript.canCheckBridgeTiles = true; // This is only false when the player enters and leaves a scene
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
        playerScript.canSetBoolsTrue = false;
        pauseMenuScript.isChangingScenes = true;
        disableMenuInputs();
        Time.timeScale = 1f;
        animator.SetTrigger("FadeOutLevel");
    }

    // Triggers the "FadeOutToNextLevel" animation (fade)
    public void FadeOutToNextLevel()
    {
        pauseMenuScript.isChangingScenes = true;
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
        pauseMenuScript.QuitToMain();                                
    }

    // Calls the "LoadNextLevel" coroutine in the pause menu
    public void OnFadeCompleteForLevel()
    {
        levelManager.StartCoroutine("LoadNextLevelAsync");
    }


    // Re-enables all inputs
    public void enableMenuInputs()
    {
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = true;
        //gameCanvas.GetComponentInChildren<CanvasGroup>().blocksRaycasts = true;
    }

    // Disables all inputs
    public void disableMenuInputs()
    {
        UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = false;      
        //gameCanvas.GetComponentInChildren<CanvasGroup>().blocksRaycasts = false;
    }

    private void SetScripts()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName != "MainMenu")
        {
            levelManager = FindObjectOfType<LevelManager>();
            playerScript = FindObjectOfType<TileMovementController>();
            pauseMenuScript = FindObjectOfType<PauseMenu>();
            playerSoundsScript = FindObjectOfType<PlayerSounds>();
            gameHUDScript = FindObjectOfType<GameHUD>();
        }
        if (sceneName == "MainMenu")
        {
            mainMenuScript = FindObjectOfType<MainMenu>();
        }
    }

}
