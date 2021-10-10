using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("Bools")]
    public bool isDeathScreen;
    public bool canToggleHUD;

    [Header("Text Meshes")]
    public TextMeshProUGUI puzzleBubbleColorText;
    public TextMeshProUGUI artifactBubbleColorText;
    private TextMeshProUGUI puzzleForegroundText;
    private TextMeshProUGUI artifactForegroundText;

    private GameObject torchMeter;
    private GameObject deathScreen;
    private GameObject skipTutorialButton;
    private GameObject notificationBubblesHolder;

    private PauseMenu pauseMenuScript;
    private TileMovementController playerScript;
    private TorchMeterScript torchMeterScript;
    private AudioManager audioManagerScript;
    private NotificationBubbles notificationBubblesScript;
    private GameManager gameManagerScript;
    private PuzzleManager puzzleManagerScript;

    void Awake()
    {
        SetScripts();
        SetElements();

        SetNumberOfCollectedArtifacts();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CanToggleCheck();
        DeathScreenInputCheck();
        ToggleHUDCheck();
    }

    // Turns the HUD on
    public void TurnOnHUD()
    {
        torchMeter.SetActive(true); 
        notificationBubblesHolder.SetActive(true);

        if (SceneManager.GetActiveScene().name == "TutorialMap")
            skipTutorialButton.SetActive(true);
    }

    // Turns the HUD off
    public void TurnOffHUD()
    {
        torchMeter.SetActive(false);
        notificationBubblesHolder.SetActive(false);

        if (SceneManager.GetActiveScene().name == "TutorialMap")
            skipTutorialButton.SetActive(false);
    }

    // Updates the text for the puzzle notification
    public void UpdatePuzzleBubbleText(string newPuzzleBubbleText)
    {
        puzzleBubbleColorText.text = newPuzzleBubbleText;
        puzzleForegroundText.text = newPuzzleBubbleText;
    }

    // Updates the text for the artifact notification
    public void UpdateArtifactBubbleText(string newArtifactBubbleText)
    {
        artifactBubbleColorText.text = newArtifactBubbleText;
        artifactForegroundText.text = newArtifactBubbleText;
    }

    // Sets the death screen active
    public void SetDeathScreenActive()
    {
        deathScreen.SetActive(true);
        isDeathScreen = true;
        audioManagerScript.PlayDeathScreenSFX();
    }

    // Sets the death screen inactive
    public void SetDeathScreenInactive()
    {
        deathScreen.SetActive(false);
        isDeathScreen = false;
    }

    // Updates the artifact notification bubble with the current amount of artifacts collected
    private void SetNumberOfCollectedArtifacts()
    {
        int numberOfArtifactsCollected = PlayerPrefs.GetInt("numberOfArtifactsCollected");

        if (numberOfArtifactsCollected <= 15)
        {
            string sceneName = SceneManager.GetActiveScene().name;

            if (sceneName == "TutorialMap")
            {
                PlayerPrefs.DeleteKey("listOfArtifacts");
                PlayerPrefs.DeleteKey("numberOfArtifactsCollected");
                UpdateArtifactBubbleText("0/1");
            }
            else
                UpdateArtifactBubbleText(numberOfArtifactsCollected + "/15");
        }
    }

    // Toggles the HUD on/off
    private void ToggleHUDCheck()
    {
        if (canToggleHUD && notificationBubblesHolder.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                notificationBubblesScript.KeybindBubblesCheck();
                torchMeterScript.isTorchMeterCheck();
            }
        }
    }

    // Checks when the HUD elements can be toggled
    private void CanToggleCheck()
    {
        if (pauseMenuScript.canPause && !pauseMenuScript.isPaused && !pauseMenuScript.isChangingScenes && pauseMenuScript.isActiveAndEnabled && !playerScript.onBridge())
        {
            if (canToggleHUD != true)
                canToggleHUD = true;
        }          
        else
        {
            if (canToggleHUD != false)
                canToggleHUD = false;
        }           
    }

    // Checks when death screen can recieve input
    private void DeathScreenInputCheck()
    {
        if (isDeathScreen && !pauseMenuScript.isChangingMenus)
        {
            if (Input.GetKeyDown(KeyCode.Q) && !pauseMenuScript.isSafetyMenu)
                pauseMenuScript.OpenSafetyMenu();

            if (Input.GetKeyDown(KeyCode.R))
            {
                playerScript.ReturnCurrentCheckpoint().GetComponent<CheckpointManager>().ResetPlayer();
                puzzleManagerScript.ResetPuzzle(0f);
                SetDeathScreenInactive();
            }           
        }
    }

    // Sets the death screen elements active after a delay
    private IEnumerator SetDeathScreenActiveDelay(float seconds)
    {
        yield return new WaitForSeconds(gameManagerScript.resetPuzzleDelay);
        deathScreen.SetActive(true);
        isDeathScreen = true;
        audioManagerScript.PlayDeathScreenSFX();
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        playerScript = FindObjectOfType<TileMovementController>();
        torchMeterScript = FindObjectOfType<TorchMeterScript>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        notificationBubblesScript = FindObjectOfType<NotificationBubbles>();
        gameManagerScript = FindObjectOfType<GameManager>();
        puzzleManagerScript = FindObjectOfType<PuzzleManager>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "OptionalDeathScreen")
                deathScreen = child;
            if (child.name == "NotificationBubblesHolder")
                notificationBubblesHolder = child;
        }

        for (int i = 0; i < puzzleBubbleColorText.transform.childCount; i++)
        {
            GameObject child = puzzleBubbleColorText.transform.GetChild(i).gameObject;

            if (child.name == "ForegroundText")
                puzzleForegroundText = child.GetComponent<TextMeshProUGUI>();
        }

        for (int i = 0; i < artifactBubbleColorText.transform.childCount; i++)
        {
            GameObject child = artifactBubbleColorText.transform.GetChild(i).gameObject;

            if (child.name == "ForegroundText")
                artifactForegroundText = child.GetComponent<TextMeshProUGUI>();
        }

        if (SceneManager.GetActiveScene().name == "TutorialMap")
            skipTutorialButton = FindObjectOfType<SkipButton>().gameObject;

        torchMeter = torchMeterScript.gameObject;
    }

}
