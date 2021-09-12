using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndCreditsAnimEvent : MonoBehaviour
{
    private GameManager gameManagerScript;
    private EndCredits endCreditsScript;
    private MainMenu mainMenuScript;

    void Awake()
    {
        SetScriptsCheck();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Checks to see which function to play based on the scene - for animation event
    public void FunctionAfterEndCredits()
    {
        if (endCreditsScript.hasEndedCredits)
        {
            if (SceneManager.GetActiveScene().name == "FifthMap")
                StartCoroutine("QuitToMainDelay");
            else
                endCreditsScript.ResetEndCredits();
        }         
    }

    // Sets the main menu active - for animation event
    public void SetMainMenuActive()
    {
        mainMenuScript.canFadeLogo = true;
        mainMenuScript.mainMenuButtons.SetActive(true);
        mainMenuScript.SelectCreditsButtonAfterCredits();
        mainMenuScript.EnableInputDelay();
    }

    // Sets the second fade to inactive after fading back to main menu - for animation event
    public void SetSecondFadeInactive()
    {
        gameObject.SetActive(false);
    }

    // Checks to see which scripts can be found
    private void SetScriptsCheck()
    {
        if (SceneManager.GetActiveScene().name == "FifthMap")
            gameManagerScript = FindObjectOfType<GameManager>();
        else
            mainMenuScript = FindObjectOfType<MainMenu>();

        endCreditsScript = FindObjectOfType<EndCredits>();
    }

    // Call the function that loads the main menu asynchronously after a delay
    private IEnumerator QuitToMainDelay()
    {
        yield return new WaitForSeconds(1.5f);
        gameManagerScript.LoadNextSceneCheck();
    }
}
