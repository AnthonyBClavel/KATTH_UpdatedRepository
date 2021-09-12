using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenAnimEvent : MonoBehaviour
{

    private PauseMenu pauseMenuScript;
    private GameHUD gameHUDScript;

    void Awake()
    {
        pauseMenuScript = FindObjectOfType<PauseMenu>();

        if (SceneManager.GetActiveScene().name != "TutorialMap")
            gameHUDScript = FindObjectOfType<GameHUD>();

        SetElements();  
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetSafetyMenuActive()
    {
        if (pauseMenuScript.isSafetyMenu && !gameHUDScript.isDeathScreen)
            //pauseMenuScript.safetyMenu.SetActive(true);
        if (pauseMenuScript.isSafetyMenu && gameHUDScript.isDeathScreen)
        {
            //pauseMenuScript.safetyMenu.SetActive(true);
            //gameHUDScript.safetyMenuText.SetActive(false);
            //gameHUDScriptsafetyMenuDeathScreenText.SetActive(true);
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {


    }

}
