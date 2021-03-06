﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("Bools")]
    public bool canToggleHUD;
    public bool canDeathScreen;
    public bool isDeathScreen;
    private bool isKeybindBubbles;
    //private bool isKeybindIcons;
    //private bool isLevelInfo;

    public bool hasPuzzleNotification = false;
    public bool hasArtifactNotification = false;
    private bool canTogglePuzzleBubble = false;
    private bool canToggleArtifactBubble = false;
    private bool isPuzzleNotification = false;
    private bool isArtifactNotification = false;
    private bool hasPlayedPuzzleNotif = false;
    private bool hasPlayedArtifactNotif = false;

    [Header("Floats")]
    public float bubbleSpeed = 1500f; //1500
    public float keybindBubbleSpeed = 2000f; //2000
    private float puzzleBubbleWidth;
    private float artifactBubbleWidth;
    private float notificationBubbleScale = 0.8f;

    private float rightScreenPosX = 960f;
    private float leftScreenPosX = -960f;

    [Header("GameObjects")]
    public GameObject keybindIcons;
    public GameObject levelInfo;
    public GameObject deathScreen;
    public GameObject safetyMenuText;
    public GameObject safetyMenuDeathScreenText;
    public GameObject puzzleNotificationBubble;
    public GameObject artifactNotificationBubble;
    public GameObject puzzleKeybindBubble;
    public GameObject artifactKeybindBubble;
    public GameObject notificationBubblesHolder;

    [Header("RectTransforms")]
    public RectTransform puzzleKeybindBubbleRectTrans;
    public RectTransform artifactKeybindBubbleRectTrans;
    public RectTransform puzzleBubbleRectTrans;
    public RectTransform artifactBubbleRectTrans;

    [Header("Text Meshes")]
    public TextMeshProUGUI puzzleNumber;
    public TextMeshProUGUI worldName;
    public TextMeshProUGUI puzzleBubbleText;
    public TextMeshProUGUI puzzleBubbleColorText;
    public TextMeshProUGUI artifactBubbleText;
    public TextMeshProUGUI artifactBubbleColorText;

    [Header("Audio")]
    public AudioClip deathScreenSFX;
    private AudioSource audioSource;

    private Vector3 puzzleBubbleOriginalPos;
    private Vector3 puzzleBubbleDestination;
    private Vector3 puzzleKeybindBubbleOrigPos;
    private Vector3 puzzleKeybindBubbleDestination;

    private Vector3 artifactBubbleOriginalPos;
    private Vector3 artifactBubbleDestination;
    private Vector3 artifactKeybindBubbleOrigPos;
    private Vector3 artifactKeybindBubbleDestination;

    private PauseMenu pauseMenuScript;
    private TileMovementController playerScript;

    void Awake()
    {
        CheckWorld();
        SetNumberOfCollectedArtifacts();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        playerScript = FindObjectOfType<TileMovementController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetBubbleVectors();
        audioSource = GetComponent<AudioSource>();

        canDeathScreen = false; //
        isKeybindBubbles = true;
        //isKeybindIcons = false; //
        //isLevelInfo = false; //

        Invoke("UpdateBubblesOriginalPos", 0.01f);
    }

    // Update is called once per frame
    void Update()
    {
        CheckWhenToToggle();
        CheckDeathScreenInput();
        ToggleNotificationBubblesCheck();

        /*** For Debuging ***/
        /*if (Input.GetKeyDown(KeyCode.P))
        {
            //
        }
        /*** Debugging Ends Here **/

        /*if (isKeybindIcons)
            keybindIcons.SetActive(true);
        if (!isKeybindIcons)
            keybindIcons.SetActive(false);

        if (isLevelInfo)
            levelInfo.SetActive(true);
        if (!isLevelInfo)
            levelInfo.SetActive(false);*/

    }

    void LateUpdate()
    {
        //if (pauseMenuScript.isActiveAndEnabled)

        ArtifactNotificationCheck();
        PuzzleNotificationCheck();
        UpdateBubblesOriginalPos();
    }

    // Updates the artifact's notification bubble with the current amount of artifacts collected
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
            else if (sceneName == "FirstMap")
            {
                PlayerPrefs.DeleteKey("listOfArtifacts");
                PlayerPrefs.DeleteKey("numberOfArtifactsCollected");
                UpdateArtifactBubbleText("0/15");
            }
            else
                UpdateArtifactBubbleText(numberOfArtifactsCollected + "/15");

        }       
    }

    // Resets the script's bools so the notifcation bubbles can be toggled
    public void EnableNotificationsToggle()
    {
        isPuzzleNotification = false;
        isArtifactNotification = false;
    }

    public void DisableNotificationsToggle()
    {
        isPuzzleNotification = true;
        isArtifactNotification = true;
    }

    // Sets the death screen active ONLY IF canDeathScreen true - this will be an options to toggle in the option screen
    public void SetDeathScreenActiveCheck()
    {
        if (canDeathScreen)
            StartCoroutine("SetDeathScreenActiveDelay");
    }

    // Sets the death screen inactive
    public void SetDeathScreenInactive()
    {
        deathScreen.SetActive(false);
        isDeathScreen = false;
    }

    // Checks when the player can toggle the Game HUD
    private void CheckWhenToToggle()
    {
        if (pauseMenuScript.canPause && !pauseMenuScript.isPaused && !pauseMenuScript.isChangingScenes && pauseMenuScript.isActiveAndEnabled && !playerScript.onBridge())
            canToggleHUD = true;
        else
            canToggleHUD = false;
    }

    // Check when the player can toggle the notification bubbles for puzzles and artifacts
    private void ToggleNotificationBubblesCheck()
    {
        if (canToggleHUD && notificationBubblesHolder.activeSelf)
        {
            // Toggle keybind aids for the notification bubbles
            if (Input.GetKeyDown(KeyCode.C))
            {
                isKeybindBubbles = !isKeybindBubbles;
                //isKeybindIcons = !isKeybindIcons;
                //isLevelInfo = !isLevelInfo;
                //canDeathScreen = !canDeathScreen;
            }

            // Toggle puzzle notification bubble
            if (puzzleNotificationBubble.activeSelf && canTogglePuzzleBubble && !isPuzzleNotification && !hasPlayedPuzzleNotif)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    hasPuzzleNotification = !hasPuzzleNotification;
                }
            }

            // Toggle artifact notification bubble
            if (artifactNotificationBubble.activeSelf && canToggleArtifactBubble && !isArtifactNotification && !hasPlayedArtifactNotif)
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    hasArtifactNotification = !hasArtifactNotification;
                }
            }
        }

        if (isKeybindBubbles)
        {
            puzzleKeybindBubble.SetActive(true);
            artifactKeybindBubble.SetActive(true);
        }
        if (!isKeybindBubbles)
        {
            puzzleKeybindBubble.SetActive(false);
            artifactKeybindBubble.SetActive(false);
        }
    }

    public void PlayPuzzleNotificationCheck()
    {
        if (!hasPuzzleNotification)
            PlayPuzzleNotification();
    }

    public void PlayArtifactNotificationCheck()
    {
        if (!hasArtifactNotification)
            PlayArtifactNotification();
    }

    // Updates the text for the puzzle notification bubble
    public void UpdatePuzzleBubbleText(string newPuzzleBubbleText)
    {
        puzzleBubbleColorText.text = newPuzzleBubbleText;
        puzzleBubbleText.text = newPuzzleBubbleText;
    }

    // Updates the text for the artifact notification bubble
    public void UpdateArtifactBubbleText(string newArtifactBubbleText)
    {
        artifactBubbleColorText.text = newArtifactBubbleText;
        artifactBubbleText.text = newArtifactBubbleText;
    }

    // Sets the original positions for the notification bubbles
    private void UpdateBubblesOriginalPos()
    {
        if (puzzleBubbleWidth != puzzleBubbleRectTrans.rect.width)
            puzzleBubbleWidth = puzzleBubbleRectTrans.rect.width;

        if (artifactBubbleWidth != artifactBubbleRectTrans.rect.width)
            artifactBubbleWidth = artifactBubbleRectTrans.rect.width;

        // 35 was added/subracted from each x position to compensate for the bubble's twitch if the text gets updated out of the screen's bounds
        if (puzzleBubbleOriginalPos != new Vector3(rightScreenPosX + (puzzleBubbleWidth * notificationBubbleScale) + 35, puzzleBubbleRectTrans.localPosition.y, 0))
            puzzleBubbleOriginalPos = new Vector3(rightScreenPosX + (puzzleBubbleWidth * notificationBubbleScale) + 35, puzzleBubbleRectTrans.localPosition.y, 0);

        if (artifactBubbleOriginalPos != new Vector3(leftScreenPosX - (artifactBubbleWidth * notificationBubbleScale) - 35, artifactBubbleRectTrans.localPosition.y, 0))
            artifactBubbleOriginalPos = new Vector3(leftScreenPosX - (artifactBubbleWidth * notificationBubbleScale) - 35, artifactBubbleRectTrans.localPosition.y, 0);
    }

    // Sets the vector variable for the notification bubbles
    private void SetBubbleVectors()
    {
        puzzleBubbleOriginalPos = puzzleBubbleRectTrans.localPosition;
        artifactBubbleOriginalPos = artifactBubbleRectTrans.localPosition;

        puzzleKeybindBubbleOrigPos = puzzleKeybindBubbleRectTrans.localPosition;
        artifactKeybindBubbleOrigPos = artifactKeybindBubbleRectTrans.localPosition;

        puzzleKeybindBubbleDestination = new Vector3(puzzleKeybindBubbleRectTrans.localPosition.x + (110 * notificationBubbleScale), puzzleKeybindBubbleRectTrans.localPosition.y, 0);
        artifactKeybindBubbleDestination = new Vector3(artifactKeybindBubbleRectTrans.localPosition.x - (110 * notificationBubbleScale), artifactKeybindBubbleRectTrans.localPosition.y, 0);

        puzzleBubbleDestination = new Vector3(rightScreenPosX - 30, puzzleBubbleRectTrans.localPosition.y, 0);
        artifactBubbleDestination = new Vector3(leftScreenPosX + 30, artifactBubbleRectTrans.localPosition.y, 0);
    }

    // Plays the puzzle notification
    private void PlayPuzzleNotification()
    {
        if (puzzleNotificationBubble.activeSelf && canTogglePuzzleBubble && !isPuzzleNotification && pauseMenuScript.canPause)
        {
            if (puzzleKeybindBubbleRectTrans.localPosition == puzzleKeybindBubbleOrigPos)
            {
                Debug.Log("Has Played Puzzle Notification");
                StartCoroutine("TriggerPuzzleNotification");
                hasPlayedPuzzleNotif = true;
                isPuzzleNotification = true;
            }
        }
    }

    // Plays the artifact notification
    private void PlayArtifactNotification()
    {
        if (artifactNotificationBubble.activeSelf && canToggleArtifactBubble && !isArtifactNotification && pauseMenuScript.canPause)
        {
            if (artifactKeybindBubbleRectTrans.localPosition == artifactKeybindBubbleOrigPos)
            {
                Debug.Log("Has Played Artifact Notification");
                StartCoroutine("TriggerArtifactNotification");
                hasPlayedArtifactNotif = true;
                isArtifactNotification = true;
            }
        }
    }

    // Check when puzzle notification can pop in and out
    private void ArtifactNotificationCheck()
    {
        if (artifactKeybindBubbleRectTrans.localPosition == artifactKeybindBubbleOrigPos || artifactBubbleRectTrans.localPosition == artifactBubbleDestination)
            canToggleArtifactBubble = true;
        else
            canToggleArtifactBubble = false;

        if (hasArtifactNotification)
        {
            if (artifactKeybindBubbleRectTrans.localPosition != artifactKeybindBubbleDestination)
                artifactKeybindBubbleRectTrans.localPosition = Vector3.MoveTowards(artifactKeybindBubbleRectTrans.localPosition, artifactKeybindBubbleDestination, keybindBubbleSpeed * Time.deltaTime);

            if (artifactKeybindBubbleRectTrans.localPosition == artifactKeybindBubbleDestination)
            {
                if (artifactBubbleRectTrans.localPosition != artifactBubbleDestination)
                    artifactBubbleRectTrans.localPosition = Vector3.MoveTowards(artifactBubbleRectTrans.localPosition, artifactBubbleDestination, bubbleSpeed * Time.deltaTime);
            }
        }

        if (!hasArtifactNotification)
        {
            if (artifactBubbleRectTrans.localPosition != artifactBubbleOriginalPos)
                artifactBubbleRectTrans.localPosition = Vector3.MoveTowards(artifactBubbleRectTrans.localPosition, artifactBubbleOriginalPos, bubbleSpeed * Time.deltaTime);

            if (artifactBubbleRectTrans.localPosition == artifactBubbleOriginalPos)
            {
                if (artifactKeybindBubbleRectTrans.localPosition != artifactKeybindBubbleOrigPos)
                    artifactKeybindBubbleRectTrans.localPosition = Vector3.MoveTowards(artifactKeybindBubbleRectTrans.localPosition, artifactKeybindBubbleOrigPos, keybindBubbleSpeed * Time.deltaTime);
            }
        }
    }

    // Check when artifact notification can pop in and out
    private void PuzzleNotificationCheck()
    {
        if (puzzleKeybindBubbleRectTrans.localPosition == puzzleKeybindBubbleOrigPos || puzzleBubbleRectTrans.localPosition == puzzleBubbleDestination)
            canTogglePuzzleBubble = true;
        else
            canTogglePuzzleBubble = false;

        if (hasPuzzleNotification)
        {
            if (puzzleKeybindBubbleRectTrans.localPosition != puzzleKeybindBubbleDestination)
                puzzleKeybindBubbleRectTrans.localPosition = Vector3.MoveTowards(puzzleKeybindBubbleRectTrans.localPosition, puzzleKeybindBubbleDestination, keybindBubbleSpeed * Time.deltaTime);

            if (puzzleKeybindBubbleRectTrans.localPosition == puzzleKeybindBubbleDestination)
            {
                if (puzzleBubbleRectTrans.localPosition != puzzleBubbleDestination)
                    puzzleBubbleRectTrans.localPosition = Vector3.MoveTowards(puzzleBubbleRectTrans.localPosition, puzzleBubbleDestination, bubbleSpeed * Time.deltaTime);
            }
        }

        if (!hasPuzzleNotification)
        {
            if (puzzleBubbleRectTrans.localPosition != puzzleBubbleOriginalPos)
                puzzleBubbleRectTrans.localPosition = Vector3.MoveTowards(puzzleBubbleRectTrans.localPosition, puzzleBubbleOriginalPos, bubbleSpeed * Time.deltaTime);

            if (puzzleBubbleRectTrans.localPosition == puzzleBubbleOriginalPos)
            {
                if (puzzleKeybindBubbleRectTrans.localPosition != puzzleKeybindBubbleOrigPos)
                    puzzleKeybindBubbleRectTrans.localPosition = Vector3.MoveTowards(puzzleKeybindBubbleRectTrans.localPosition, puzzleKeybindBubbleOrigPos, keybindBubbleSpeed * Time.deltaTime);
            }
        }
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
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "FirstMap")
            worldName.text = "Zone: Boreal Forest";
        else if (sceneName == "SecondMap")
            worldName.text = "Zone: Frozen Forest";
        else if (sceneName == "ThirdMap")
            worldName.text = "Zone: Crystal Cave";
        else if (sceneName == "FourthMap")
            worldName.text = "Zone: Barren Lands";
        else if (sceneName == "FifthMap")
            worldName.text = "Zone: Power Station";
        else if (sceneName == "TutorialMap")
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
        playerScript.canRestartPuzzle = true;
        PlayDeathScreenSFX();
    }

    // triggers the puzzle notification - cannot toggle while notification is active/played
    private IEnumerator TriggerPuzzleNotification()
    {
        hasPuzzleNotification = !hasPuzzleNotification;
        yield return new WaitForSeconds(3f);
        hasPuzzleNotification = !hasPuzzleNotification;
        hasPlayedPuzzleNotif = false;
        isPuzzleNotification = false;
    }

    // triggers the artifact notification - cannot toggle while notification is active/played
    private IEnumerator TriggerArtifactNotification()
    {
        hasArtifactNotification = !hasArtifactNotification;
        yield return new WaitForSeconds(3f);
        hasArtifactNotification = !hasArtifactNotification;
        hasPlayedArtifactNotif = false;
        isArtifactNotification = false;
    }


}
