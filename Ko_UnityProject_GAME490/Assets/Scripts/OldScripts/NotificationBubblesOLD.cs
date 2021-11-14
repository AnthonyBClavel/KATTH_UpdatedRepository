using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class NotificationBubblesOLD : MonoBehaviour
{
    [Header("Bubble Speeds")]
    public float notificationBubbleSpeed = 1500f;
    public float keybindBubbleSpeed = 2000f;
    [Header("Anim Distances")]
    public float notificationAnimDistance = 30f;
    public float keybindAnimDistance = 110f;

    private float rightScreenPosX = 960f;
    private float leftScreenPosX = -960f;
    private float notificationBubbleScale = 0.8f;
    private float pnbWidth; //pnb = puzzle notification bubble
    private float anbWidth; //anb = artifact notification bubble

    private bool isKeybindBubbles = true; // Set this to true in Start() if buggy
    private bool hasPuzzleNotification = false;
    private bool hasArtifactNotification = false;
    private bool canTogglePuzzleBubble = false;
    private bool canToggleArtifactBubble = false;
    private bool isPuzzleNotification = false;
    private bool isArtifactNotification = false;
    private bool hasPlayedPuzzleNotif = false;
    private bool hasPlayedArtifactNotif = false;

    private GameObject notificationBubblesHolder;
    private GameObject puzzleNotificationBubble;
    private GameObject artifactNotificationBubble;
    private GameObject puzzleKeybindBubble;
    private GameObject artifactKeybindBubble;

    private RectTransform pkbRectTransform; //pkb = puzzle keybind bubble 
    private RectTransform akbRectTransform; //akb = artifact keybind bubble
    private RectTransform pnbRectTransform; //pnb = puzzle notification bubble
    private RectTransform anbRectTransform; //anb = artifact notification bubble

    private Vector3 pnbOriginalPosition;
    private Vector3 pnbDestination;
    private Vector3 pkbOriginalPosition;
    private Vector3 pkbDestination;

    private Vector3 anbOriginalPosition;
    private Vector3 anbDestination;
    private Vector3 akbOriginalPosition;
    private Vector3 akbDestination;

    private PauseMenu pauseMenuScript;
    private GameHUD gameHUDScript;
    private GameManager gameManagerScript;

    void Awake()
    {
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        gameManagerScript = FindObjectOfType<GameManager>();

        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetBubbleVectors();

        Invoke("UpdateOriginalPositionsCheck", 0.01f);
    }

    // Update is called once per frame
    void Update()
    {
        ToggleNotificationBubblesCheck();
    }

    void LateUpdate()
    {
        DebuggingCheck();

        ArtifactNotificationCheck();
        PuzzleNotificationCheck();
        UpdateOriginalPositionsCheck();
    }

    // Sets the toggle bools to false - enables toggle
    public void EnableNotificationsToggle()
    {
        isPuzzleNotification = false;
        isArtifactNotification = false;
    }

    // Sets the toggle bools to true - disables toggle
    public void DisableNotificationsToggle()
    {
        isPuzzleNotification = true;
        isArtifactNotification = true;
    }

    // Checks if the puzzle notififcation can be played
    public void PlayPuzzleNotificationCheck()
    {
        if (!hasPuzzleNotification)
            PlayPuzzleNotification();
    }

    // Checks if the artifact notififcation can be played
    public void PlayArtifactNotificationCheck()
    {
        if (!hasArtifactNotification)
            PlayArtifactNotification();
    }

    // Toggles the keybind bubbles on/off
    public void KeybindBubblesCheck()
    {
        isKeybindBubbles = !isKeybindBubbles;

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

    // Plays the puzzle notification
    private void PlayPuzzleNotification()
    {
        if (puzzleNotificationBubble.activeSelf && canTogglePuzzleBubble && !isPuzzleNotification && pauseMenuScript.CanPause)
        {
            if (pkbRectTransform.localPosition == pkbOriginalPosition)
            {
                //Debug.Log("Has Played Puzzle Notification");
                StartCoroutine("TriggerPuzzleNotification");
                hasPlayedPuzzleNotif = true;
                isPuzzleNotification = true;
            }
        }
    }

    // Plays the artifact notification
    private void PlayArtifactNotification()
    {
        if (artifactNotificationBubble.activeSelf && canToggleArtifactBubble && !isArtifactNotification && pauseMenuScript.CanPause)
        {
            if (akbRectTransform.localPosition == akbOriginalPosition)
            {
                //Debug.Log("Has Played Artifact Notification");
                StartCoroutine("TriggerArtifactNotification");
                hasPlayedArtifactNotif = true;
                isArtifactNotification = true;
            }
        }
    }

    // Checks if the notifcation bubbles can be toggled
    private void ToggleNotificationBubblesCheck()
    {
        if (gameHUDScript.canToggleHUD && notificationBubblesHolder.activeSelf)
        {
            // Toggle puzzle notification bubble
            if (puzzleNotificationBubble.activeSelf && canTogglePuzzleBubble && !isPuzzleNotification && !hasPlayedPuzzleNotif)
            {
                if (Input.GetKeyDown(KeyCode.E))
                    hasPuzzleNotification = !hasPuzzleNotification;
            }

            // Toggle artifact notification bubble
            if (artifactNotificationBubble.activeSelf && canToggleArtifactBubble && !isArtifactNotification && !hasPlayedArtifactNotif)
            {
                if (Input.GetKeyDown(KeyCode.Q))
                    hasArtifactNotification = !hasArtifactNotification;
            }
        }
    }

    // Checks if the puzzle notification is moving towards its destination or original position
    private void PuzzleNotificationCheck()
    {
        CanTogglePuzzleBubbleCheck();

        if (hasPuzzleNotification)
        {
            if (pkbRectTransform.localPosition != pkbDestination)
                pkbRectTransform.localPosition = Vector3.MoveTowards(pkbRectTransform.localPosition, pkbDestination, keybindBubbleSpeed * Time.deltaTime);

            if (pkbRectTransform.localPosition == pkbDestination)
            {
                if (pnbRectTransform.localPosition != pnbDestination)
                    pnbRectTransform.localPosition = Vector3.MoveTowards(pnbRectTransform.localPosition, pnbDestination, notificationBubbleSpeed * Time.deltaTime);
            }
        }

        if (!hasPuzzleNotification)
        {
            if (pnbRectTransform.localPosition != pnbOriginalPosition)
                pnbRectTransform.localPosition = Vector3.MoveTowards(pnbRectTransform.localPosition, pnbOriginalPosition, notificationBubbleSpeed * Time.deltaTime);

            if (pnbRectTransform.localPosition == pnbOriginalPosition)
            {
                if (pkbRectTransform.localPosition != pkbOriginalPosition)
                    pkbRectTransform.localPosition = Vector3.MoveTowards(pkbRectTransform.localPosition, pkbOriginalPosition, keybindBubbleSpeed * Time.deltaTime);
            }
        }
    }

    // Checks if the artifact notification is moving towards its destination or original position
    private void ArtifactNotificationCheck()
    {
        CanToggleArtifactBubbleCheck();

        if (hasArtifactNotification)
        {
            if (akbRectTransform.localPosition != akbDestination)
                akbRectTransform.localPosition = Vector3.MoveTowards(akbRectTransform.localPosition, akbDestination, keybindBubbleSpeed * Time.deltaTime);

            if (akbRectTransform.localPosition == akbDestination)
            {
                if (anbRectTransform.localPosition != anbDestination)
                    anbRectTransform.localPosition = Vector3.MoveTowards(anbRectTransform.localPosition, anbDestination, notificationBubbleSpeed * Time.deltaTime);
            }
        }

        if (!hasArtifactNotification)
        {
            if (anbRectTransform.localPosition != anbOriginalPosition)
                anbRectTransform.localPosition = Vector3.MoveTowards(anbRectTransform.localPosition, anbOriginalPosition, notificationBubbleSpeed * Time.deltaTime);

            if (anbRectTransform.localPosition == anbOriginalPosition)
            {
                if (akbRectTransform.localPosition != akbOriginalPosition)
                    akbRectTransform.localPosition = Vector3.MoveTowards(akbRectTransform.localPosition, akbOriginalPosition, keybindBubbleSpeed * Time.deltaTime);
            }
        }
    }

    // Checks when the bool is true/false - canToggleArtifactBubble
    private void CanToggleArtifactBubbleCheck()
    {
        if (akbRectTransform.localPosition == akbOriginalPosition || anbRectTransform.localPosition == anbDestination)
        {
            if (canToggleArtifactBubble != true)
                canToggleArtifactBubble = true;
        }
        else
        {
            if (canToggleArtifactBubble != false)
                canToggleArtifactBubble = false;
        }
    }

    // Checks when the bool is true/false - canTogglePuzzleBubble
    private void CanTogglePuzzleBubbleCheck()
    {
        if (pkbRectTransform.localPosition == pkbOriginalPosition || pnbRectTransform.localPosition == pnbDestination)
        {
            if (canTogglePuzzleBubble != true)
                canTogglePuzzleBubble = true;
        }
        else
        {
            if (canTogglePuzzleBubble != false)
                canTogglePuzzleBubble = false;
        }
    }

    // Updates the original positions for the notification bubbles
    private void UpdateOriginalPositionsCheck()
    {
        if (pnbWidth != pnbRectTransform.rect.width)
            pnbWidth = pnbRectTransform.rect.width;

        if (anbWidth != anbRectTransform.rect.width)
            anbWidth = anbRectTransform.rect.width;

        // 35 was added/subracted from each x position to compensate for the bubble's twitch if the text gets updated out of the screen's bounds
        if (pnbOriginalPosition != new Vector3(rightScreenPosX + (pnbWidth * notificationBubbleScale) + 35, pnbRectTransform.localPosition.y, 0))
            pnbOriginalPosition = new Vector3(rightScreenPosX + (pnbWidth * notificationBubbleScale) + 35, pnbRectTransform.localPosition.y, 0);

        if (anbOriginalPosition != new Vector3(leftScreenPosX - (anbWidth * notificationBubbleScale) - 35, anbRectTransform.localPosition.y, 0))
            anbOriginalPosition = new Vector3(leftScreenPosX - (anbWidth * notificationBubbleScale) - 35, anbRectTransform.localPosition.y, 0);
    }

    // Sets the vector variables for the notification bubbles
    private void SetBubbleVectors()
    {
        pnbOriginalPosition = pnbRectTransform.localPosition;
        anbOriginalPosition = anbRectTransform.localPosition;

        pkbOriginalPosition = pkbRectTransform.localPosition;
        akbOriginalPosition = akbRectTransform.localPosition;

        pkbDestination = new Vector3(pkbOriginalPosition.x + (keybindAnimDistance * notificationBubbleScale), pkbOriginalPosition.y, 0);
        akbDestination = new Vector3(akbOriginalPosition.x - (keybindAnimDistance * notificationBubbleScale), akbOriginalPosition.y, 0);

        pnbDestination = new Vector3(rightScreenPosX - notificationAnimDistance, pnbOriginalPosition.y, 0);
        anbDestination = new Vector3(leftScreenPosX + notificationAnimDistance, anbOriginalPosition.y, 0);
    }


    // Triggers the puzzle notification - cannot toggle while notification is active/played
    private IEnumerator TriggerPuzzleNotification()
    {
        hasPuzzleNotification = !hasPuzzleNotification;
        yield return new WaitForSeconds(3f);
        hasPuzzleNotification = !hasPuzzleNotification;
        hasPlayedPuzzleNotif = false;
        isPuzzleNotification = false;
    }

    // Triggers the artifact notification - cannot toggle while notification is active/played
    private IEnumerator TriggerArtifactNotification()
    {
        hasArtifactNotification = !hasArtifactNotification;
        yield return new WaitForSeconds(3f);
        hasArtifactNotification = !hasArtifactNotification;
        hasPlayedArtifactNotif = false;
        isArtifactNotification = false;
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i <  gameHUDScript.transform.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.GetChild(i).gameObject;

            if (child.name == "NotificationBubblesHolder")
            {
                notificationBubblesHolder = child;

                for (int j = 0; j < notificationBubblesHolder.transform.childCount; j++)
                {
                    GameObject child02 = notificationBubblesHolder.transform.GetChild(j).gameObject;
                    RectTransform rectTransform = child02.GetComponent<RectTransform>();

                    if (child02.name == "PuzzleKeybindBubble")
                    {
                        puzzleKeybindBubble = child02.GetComponentInChildren<Image>().gameObject;
                        pkbRectTransform = rectTransform;
                    }
                    if (child02.name == "ArtifactKeybindBubble")
                    {
                        artifactKeybindBubble = child02.GetComponentInChildren<Image>().gameObject;
                        akbRectTransform = rectTransform;
                    }
                    if (child02.name == "PuzzleNotificationBubble")
                    {
                        puzzleNotificationBubble = child02;
                        pnbRectTransform = rectTransform;
                    }
                    if (child02.name == "ArtifactNotificationBubble")
                    {
                        artifactNotificationBubble = child02;
                        anbRectTransform = rectTransform;
                    }
                }
            }
        }
    }

    // Updates the destinations if the animation distances are changed - For Debugging Purposes ONLY
    private void DebuggingCheck()
    {
        if (gameManagerScript.isDebugging)
        {
            if (pkbDestination != new Vector3(pkbOriginalPosition.x + (keybindAnimDistance * notificationBubbleScale), pkbOriginalPosition.y, 0))
                pkbDestination = new Vector3(pkbOriginalPosition.x + (keybindAnimDistance * notificationBubbleScale), pkbOriginalPosition.y, 0);

            if (akbDestination != new Vector3(akbOriginalPosition.x - (keybindAnimDistance * notificationBubbleScale), akbOriginalPosition.y, 0))
                akbDestination = new Vector3(akbOriginalPosition.x - (keybindAnimDistance * notificationBubbleScale), akbOriginalPosition.y, 0);

            if (pnbDestination != new Vector3(rightScreenPosX - notificationAnimDistance, pnbOriginalPosition.y, 0))
                pnbDestination = new Vector3(rightScreenPosX - notificationAnimDistance, pnbOriginalPosition.y, 0);

            if (anbDestination != new Vector3(leftScreenPosX + notificationAnimDistance, anbOriginalPosition.y, 0))
                anbDestination = new Vector3(leftScreenPosX + notificationAnimDistance, anbOriginalPosition.y, 0);
        }
    }

}
