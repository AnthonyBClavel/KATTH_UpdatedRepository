using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameHUD : MonoBehaviour
{
    private bool canToggleHUD;

    public TextMeshProUGUI puzzleBubbleColorText;
    public TextMeshProUGUI artifactBubbleColorText;
    private TextMeshProUGUI puzzleForegroundText;
    private TextMeshProUGUI artifactForegroundText;

    private GameObject torchMeter;
    private GameObject deathScreen;
    private GameObject notificationBubblesHolder;

    private PauseMenu pauseMenuScript;
    private TileMovementController playerScript;
    private TorchMeter torchMeterScript;
    private AudioManager audioManagerScript;
    private NotificationBubbles notificationBubblesScript;
    private PuzzleManager puzzleManagerScript;
    private TransitionFade transitionFadeScript;
    private SkipSceneButton skipSceneButtonScript;

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

    // Returns or sets the value of the bool canToggleHUD
    public bool CanToggleHUD
    {
        get
        {
            return canToggleHUD;
        }
        set
        {
            canToggleHUD = value;
        }
    }

    // Turns the HUD on
    public void TurnOnHUD()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        torchMeter.SetActive(true);
        torchMeterScript.PlayTorchFlameIconAnim();
        notificationBubblesHolder.SetActive(true);

        if (sceneName == "TutorialMap")
            skipSceneButtonScript.SetSkipSceneButtonActive();
    }

    // Turns the HUD off
    public void TurnOffHUD()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        torchMeter.SetActive(false);
        notificationBubblesHolder.SetActive(false);

        if (sceneName == "TutorialMap")
            skipSceneButtonScript.SetSkipSceneButtonInactive();
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
        audioManagerScript.PlayDeathScreenSFX();
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
                torchMeterScript.IsTorchMeterCheck();
            }
        }
    }

    // Checks when the HUD elements can be toggled
    private void CanToggleCheck()
    {
        if (!transitionFadeScript.IsChangingScenes && !pauseMenuScript.IsPaused && pauseMenuScript.CanPause && pauseMenuScript.isActiveAndEnabled && !playerScript.OnBridge())
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
        if (deathScreen.activeSelf && !pauseMenuScript.IsChangingMenus)
        {
            if (Input.GetKeyDown(KeyCode.Q) && !pauseMenuScript.IsSafetyMenu)
                pauseMenuScript.OpenSafetyMenu();

            if (Input.GetKeyDown(KeyCode.R))
            {
                puzzleManagerScript.ResetPuzzle(0f);
                deathScreen.SetActive(false);
            }           
        }
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        playerScript = FindObjectOfType<TileMovementController>();
        torchMeterScript = FindObjectOfType<TorchMeter>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        notificationBubblesScript = FindObjectOfType<NotificationBubbles>();
        puzzleManagerScript = FindObjectOfType<PuzzleManager>();
        transitionFadeScript = FindObjectOfType<TransitionFade>();
        skipSceneButtonScript = FindObjectOfType<SkipSceneButton>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            string childName = child.name;

            if (childName == "OptionalDeathScreen")
                deathScreen = child;
            if (childName == "NotificationBubblesHolder")
                notificationBubblesHolder = child;
            if (childName == "TorchMeter")
                torchMeter = child;
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
    }

}
