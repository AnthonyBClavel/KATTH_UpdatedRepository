using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SafetyMenuAnimEvent : MonoBehaviour
{
    public GameObject canvasWithButtons;
    private PauseMenu pauseMenuScript;
    private MainMenu mainMenuScript;

    // Start is called before the first frame update
    void Awake()
    {
        if(SceneManager.GetActiveScene().name == "MainMenu")
           mainMenuScript = FindObjectOfType<MainMenu>();
        else 
           pauseMenuScript = FindObjectOfType<PauseMenu>();     
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSafetyMenuInactive()
    {
        gameObject.SetActive(false);

        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            if (mainMenuScript.isQuitingGame == false)
                canvasWithButtons.SetActive(true);
        }

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            if (pauseMenuScript.isChangingScenes == false)
                canvasWithButtons.SetActive(true);
        }
    }
    
}
