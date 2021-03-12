using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SafetyMenuAnimEvent : MonoBehaviour
{
    public GameObject mainCanvas;
    private PauseMenu pauseMenuScript;
    private MainMenu mainMenuScript;
    private GameHUD gameHUDScript;

    // Start is called before the first frame update
    void Awake()
    {
        if(SceneManager.GetActiveScene().name == "MainMenu")
           mainMenuScript = FindObjectOfType<MainMenu>();
        else
        {
            gameHUDScript = FindObjectOfType<GameHUD>();
            pauseMenuScript = FindObjectOfType<PauseMenu>();
        }            
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // For an animation event in the saftey menu
    public void SetSafetyMenuInactive()
    {
        gameObject.SetActive(false);

        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            if (!mainMenuScript.isQuitingGame)
                mainCanvas.SetActive(true);
        }

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            if (!pauseMenuScript.isChangingScenes && !gameHUDScript.isDeathScreen)
                mainCanvas.SetActive(true);
            if (!pauseMenuScript.isChangingScenes && gameHUDScript.isDeathScreen)
            {
                gameHUDScript.safetyMenuDeathScreenText.SetActive(false);
                gameHUDScript.safetyMenuText.SetActive(true);
                pauseMenuScript.deathScreenAnim.SetTrigger("DS_PopIn");
            }            
        }

    }
    
}
