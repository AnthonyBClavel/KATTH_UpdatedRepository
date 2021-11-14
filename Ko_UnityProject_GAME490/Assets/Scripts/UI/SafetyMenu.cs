using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafetyMenu : MonoBehaviour
{
    private PauseMenu pauseMenuScript;

    void Awake()
    {
        pauseMenuScript = FindObjectOfType<PauseMenu>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

    public void SelectYesButton()
    {
        pauseMenuScript.SelectYesButton();
    }
    public void SelectNoButton()
    {
        pauseMenuScript.SelectNoButton();
    }

    public void CloseSafetyMenu()
    {
        pauseMenuScript.CloseSafetyMenu();
    }

    public void CloseSafetyMenu02()
    {
        pauseMenuScript.CloseSafetyMenu02();
    }

}
