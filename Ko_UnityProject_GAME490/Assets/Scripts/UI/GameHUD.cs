using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameHUD : MonoBehaviour
{
    public GameObject keybindIcons, levelInfo, deathScreen;
    public GameObject safetyMenuText, safetyMenuDeathScreenText;
    public TextMeshProUGUI puzzleNumber;
    public TextMeshProUGUI worldName;

    public AudioClip deathScreenSFX;
    private AudioSource audioSource;

    public bool canToggleHUD;
    public bool canDeathScreen;
    public bool isDeathScreen;
    private bool isKeybindIcons;
    private bool isLevelInfo;

    private PauseMenu pauseMenuScript;
    private TileMovementController playerScript;

    void Awake()
    {
        CheckWorld();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        playerScript = FindObjectOfType<TileMovementController>();
    }
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        canDeathScreen = false; //
        isKeybindIcons = true;
        isLevelInfo = true;
    }

    // Update is called once per frame
    void Update()
    {
        CheckWhenToToggle();
        CheckDeathScreenInput();

        /*** For Debuging ***/
        if (Input.GetKeyDown(KeyCode.I) && canToggleHUD)
        {
            isKeybindIcons = !isKeybindIcons;
            isLevelInfo = !isLevelInfo;
            //canDeathScreen = !canDeathScreen;
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

    // Sets the death screen active ONLY IF canDeathScreen true - this will be an options to toggle in the option screen
    public void SetDeathScreenActiveCheck()
    {
        if(canDeathScreen)
        {
            StartCoroutine("SetDeathScreenActiveDelay");
        }
    }

    public void SetDeathScreenInactive()
    {
        deathScreen.SetActive(false);
        isDeathScreen = false;
    }

    // Sets the restarting puzzle bool to true
    public void CanRestartPuzzle()
    {
        playerScript.canRestartPuzzle = true;
    }

    // Sets the restarting puzzle bool to false
    public void CannotRestartPuzzle()
    {
        playerScript.canRestartPuzzle = false;
    }

    // Checks when the player can toggle the Game HUD
    private void CheckWhenToToggle()
    {
        if (pauseMenuScript.canPause && !pauseMenuScript.isPaused && !pauseMenuScript.isChangingScenes && pauseMenuScript.isActiveAndEnabled)
            canToggleHUD = true;
        else
            canToggleHUD = false;
    }

    // Checks when the player can perform the inputs for the death screen
    private void CheckDeathScreenInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) && isDeathScreen && !pauseMenuScript.isSafetyMenu && !pauseMenuScript.isChangingMenus)
        {
            pauseMenuScript.OpenSafetyMenu();

        }
        // ESC is an alternative for closing the saftey menu during the death screen
        if (Input.GetKeyDown(KeyCode.Escape) && isDeathScreen && pauseMenuScript.isSafetyMenu && !pauseMenuScript.isChangingMenus)
        {
            pauseMenuScript.CloseSafetyMenu();
        }
    }

    // Checks to see which zone the player is in, and sets the HUD with the correct info
    private void CheckWorld()
    {
        if (SceneManager.GetActiveScene().name == "FirstMap")
            worldName.text = "Zone: Boreal Forest";
        else if (SceneManager.GetActiveScene().name == "SecondMap")
            worldName.text = "Zone: Frozen Forest";
        else if (SceneManager.GetActiveScene().name == "ThirdMap")
            worldName.text = "Zone: Crystal Cave";
        else if (SceneManager.GetActiveScene().name == "FourthMap")
            worldName.text = "Zone: Barren Lands";
        else if (SceneManager.GetActiveScene().name == "FifthMap")
            worldName.text = "Zone: Power Station";
        else if (SceneManager.GetActiveScene().name == "TutorialMap")
            worldName.text = "Zone: Tutorial";
    }

    // Plays the SFX for the optional death screen
    private void PlayDeathScreenSFX()
    {
        audioSource.volume = 0.5f;
        audioSource.pitch = 1f;
        audioSource.PlayOneShot(deathScreenSFX);
    }

    // Sets the death screen elements active after specified time
    private IEnumerator SetDeathScreenActiveDelay()
    {
        yield return new WaitForSeconds(1.25f);
        deathScreen.SetActive(true);
        isDeathScreen = true;
        CanRestartPuzzle();
        PlayDeathScreenSFX();
    }

}
