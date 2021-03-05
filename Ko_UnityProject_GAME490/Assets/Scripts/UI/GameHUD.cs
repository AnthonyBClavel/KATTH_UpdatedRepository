using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameHUD : MonoBehaviour
{
    public GameObject keybindIcons, levelInfo;
    public TextMeshProUGUI puzzleNumber;
    public TextMeshProUGUI worldName;

    public bool canToggleHUD;
    private bool isKeybindIcons;
    private bool isLevelInfo;

    private PauseMenu pauseMenuScript;

    void Awake()
    {
        CheckWorld();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
    }
    // Start is called before the first frame update
    void Start()
    {
        isKeybindIcons = true;
        isLevelInfo = true;
    }

    // Update is called once per frame
    void Update()
    {
        CheckWhenToToggle();

        /*** For Debuging ***/
        if (Input.GetKeyDown(KeyCode.I) && canToggleHUD)
        {
            isKeybindIcons = !isKeybindIcons;
            isLevelInfo = !isLevelInfo;
        }
        /*** Debugging Ends Here **/

        if (isKeybindIcons)
            keybindIcons.SetActive(true);
        if (!isKeybindIcons)
            keybindIcons.SetActive(false);

        if (isLevelInfo)
            levelInfo.SetActive(true);
        if (!isLevelInfo)
            levelInfo.SetActive(false);
    }

    public void TurnOffHUD()
    {
        isKeybindIcons = false;
        isLevelInfo = false;
    }

    public void TurnOnHUD()
    {
        isKeybindIcons = true;
        isLevelInfo = true;
    }

    private void CheckWhenToToggle()
    {
        if (pauseMenuScript.canPause && !pauseMenuScript.isPaused && !pauseMenuScript.isChangingScenes && pauseMenuScript.isActiveAndEnabled)
            canToggleHUD = true;
        else
            canToggleHUD = false;
    }

    private void CheckWorld()
    {
        if (SceneManager.GetActiveScene().name == "FirstMap")
            worldName.text = "World: Boreal Forest";
        else if (SceneManager.GetActiveScene().name == "SecondMap")
            worldName.text = "World: Frozen Forest";
        else if (SceneManager.GetActiveScene().name == "ThirdMap")
            worldName.text = "World: Crystal Cave";
        else if (SceneManager.GetActiveScene().name == "FourthMap")
            worldName.text = "World: Ember City";
        else if (SceneManager.GetActiveScene().name == "FifthMap")
            worldName.text = "World: Power Station";
        else if (SceneManager.GetActiveScene().name == "TutorialMap")
            worldName.text = "World: Tutorial";
    }


}
