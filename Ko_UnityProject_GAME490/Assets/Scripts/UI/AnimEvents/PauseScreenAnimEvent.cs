using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseScreenAnimEvent : MonoBehaviour
{
    public GameObject safetyMenu;
    public GameObject optionsScreen;
    private PauseMenu pauseMenuScript;

    // Start is called before the first frame update
    void Awake()
    {
        pauseMenuScript = FindObjectOfType<PauseMenu>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetPauseMenuInactive()
    {
        gameObject.SetActive(false);

        if (pauseMenuScript.isOptionsMenu == true)
            optionsScreen.SetActive(true); 
        
        if (pauseMenuScript.isSafetyMenu == true)
            safetyMenu.SetActive(true);

    }

}
