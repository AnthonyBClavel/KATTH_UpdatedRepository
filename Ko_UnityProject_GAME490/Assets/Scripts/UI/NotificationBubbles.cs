using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationBubbles : MonoBehaviour
{
    [Range(0.1f, 1f)]
    public float animLength = 0.3f;
    [Range(1f, 10f)]
    public float notificationLength = 3f;
    [Range(0, 270f)]
    public float animDistanceNB = 30f;
    [Range(100f, 270f)]
    public float animDistanceKB = 125f; // 100f

    private float notificationBubbleScale = 0.8f;
    private float pnbWidth; //pnb = puzzle notification bubble
    private float anbWidth; //anb = artifact notification bubble

    private bool isKeybindBubbles = true; // Set this to true in Start() if buggy
    private bool isPuzzleNotification = false;
    private bool isArtifactNotification = false;
    private bool hasLerpedPuzzleNotif = false;
    private bool hasLerpedArtifactNotif = false;
    private bool canTogglePuzzleBubble = true;
    private bool canToggleArtifactBubble = true;
    private bool hasPlayedPuzzleNotif = false;
    private bool hasPlayedArtifactNotif = false;

    private GameObject notificationBubblesHolder;
    private GameObject puzzleNotificationBubble;
    private GameObject artifactNotificationBubble;
    private GameObject puzzleKeybindBubble;
    private GameObject artifactKeybindBubble;

    private Vector2 pnbOriginalPosition; //pnb = puzzle notification bubble
    private Vector2 pnbDestination;
    private Vector2 pkbOriginalPosition; //pkb = puzzle keybind bubble 
    private Vector2 pkbDestination;

    private Vector2 anbOriginalPosition; //anb = artifact notification bubble
    private Vector2 anbDestination;
    private Vector2 akbOriginalPosition; //akb = artifact keybind bubble
    private Vector2 akbDestination;

    private RectTransform pnbRectTransform;
    private RectTransform anbRectTransform;
    private RectTransform pkbRectTransform;
    private RectTransform akbRectTransform;

    private PauseMenu pauseMenuScript;
    private GameHUD gameHUDScript;

    void Awake()
    {
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        gameHUDScript = FindObjectOfType<GameHUD>();

        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetBubbleVectors();
}

    // Update is called once per frame
    void Update()
    {
        ToggleNotificationBubblesCheck();
    }

    void LateUpdate()
    {
        UpdateOriginalPositionsCheck();
    }

    // Sets the toggle bools to true - enables toggle
    public void EnableNotificationsToggle()
    {
        canTogglePuzzleBubble = true;
        canToggleArtifactBubble = true;
    }

    // Sets the toggle bools to false - disables toggle
    public void DisableNotificationsToggle()
    {
        canTogglePuzzleBubble = false;
        canToggleArtifactBubble = false;
    }

    // Plays the puzzle notification
    public void PlayPuzzleNotificationCheck()
    {
        if (pauseMenuScript.canPause && puzzleNotificationBubble.activeSelf)
        {
            if (!isPuzzleNotification && canTogglePuzzleBubble && !hasPlayedPuzzleNotif)
            {
                //Debug.Log("Has Played Puzzle Notification");
                StartCoroutine(LerpNotificationBubble(puzzleKeybindBubble, pkbDestination, puzzleNotificationBubble, pnbDestination, animLength));
                canTogglePuzzleBubble = false;
                hasPlayedPuzzleNotif = true;
            }
        }
    }

    // Plays the artifact notification
    public void PlayArtifactNotificationCheck()
    {
        if (pauseMenuScript.canPause && artifactNotificationBubble.activeSelf)
        {
            if (!isArtifactNotification && canToggleArtifactBubble && !hasPlayedArtifactNotif)
            {
                Debug.Log("Has Played Artifact Notification");
                StartCoroutine(LerpNotificationBubble(artifactKeybindBubble, akbDestination, artifactNotificationBubble, anbDestination, animLength));
                canToggleArtifactBubble = false;
                hasPlayedArtifactNotif = true;
            }
        }
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

    // Checks if the notifcation bubbles can be toggled
    private void ToggleNotificationBubblesCheck()
    {
        if (gameHUDScript.canToggleHUD && notificationBubblesHolder.activeSelf)
        {
            // Toggle puzzle notification bubble
            if (puzzleNotificationBubble.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.E) && canTogglePuzzleBubble && !hasPlayedPuzzleNotif)
                {
                    if (!hasLerpedPuzzleNotif)
                    {
                        StartCoroutine(LerpNotificationBubble(puzzleKeybindBubble, pkbDestination, puzzleNotificationBubble, pnbDestination, animLength));
                        isPuzzleNotification = true;
                        hasLerpedPuzzleNotif = true;
                        canTogglePuzzleBubble = false;
                    }                                     
                    else if (hasLerpedPuzzleNotif)
                    {
                        StartCoroutine(LerpNotificationBubble(puzzleNotificationBubble, pnbOriginalPosition, puzzleKeybindBubble, pkbOriginalPosition, animLength));
                        isPuzzleNotification = false;
                        hasLerpedPuzzleNotif = false;
                        canTogglePuzzleBubble = false;
                    }       
                }
            }

            // Toggle artifact notification bubble
            if (artifactNotificationBubble.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Q) && canToggleArtifactBubble && !hasPlayedArtifactNotif)
                {
                    if (!hasLerpedArtifactNotif)
                    {
                        StartCoroutine(LerpNotificationBubble(artifactKeybindBubble, akbDestination, artifactNotificationBubble, anbDestination, animLength));
                        isArtifactNotification = true;
                        hasLerpedArtifactNotif = true;
                        canToggleArtifactBubble = false;
                    }
                    else if (hasLerpedArtifactNotif)
                    {
                        StartCoroutine(LerpNotificationBubble(artifactNotificationBubble, anbOriginalPosition, artifactKeybindBubble, akbOriginalPosition, animLength));
                        isArtifactNotification = false;
                        hasLerpedArtifactNotif = false;
                        canToggleArtifactBubble = false;
                    }
                }
            }
        }
    }

    // Lerps the position of objects over a specific duration (bubble = notification or keybind bubble, endPosition = position to lerp to, duration = seconds)
    private IEnumerator LerpNotificationBubble(GameObject bubble01, Vector2 endPosition01, GameObject bubble02, Vector2 endPosition02, float duration)
    {
        float scaledDuration;
        float time01 = 0;
        Vector2 startPosition01 = bubble01.GetComponent<RectTransform>().anchoredPosition;

        // Checks how long the bubble has to lerp to its end position - different for keybind bubbles due to shorter lerp distance (endPosition - startPosition)
        if (bubble01 == puzzleKeybindBubble || bubble01 == artifactKeybindBubble)
            scaledDuration = duration * 0.3f;
        else
            scaledDuration = duration * 0.7f;

        // Lerps the bubble from its start position to its end position
        while (time01 < scaledDuration)
        {
            bubble01.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(startPosition01, endPosition01, time01 / scaledDuration);
            time01 += Time.deltaTime;
            yield return null;
        }

        bubble01.GetComponent<RectTransform>().anchoredPosition = endPosition01;
        float time02 = 0;
        Vector2 startPosition02 = bubble02.GetComponent<RectTransform>().anchoredPosition;

        if (bubble02 == puzzleKeybindBubble || bubble02 == artifactKeybindBubble)
            scaledDuration = duration * 0.3f;
        else
            scaledDuration = duration * 0.7f;

        while (time02 < scaledDuration)
        {
            bubble02.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(startPosition02, endPosition02, time02 / scaledDuration);
            time02 += Time.deltaTime;
            yield return null;
        }

        bubble02.GetComponent<RectTransform>().anchoredPosition = endPosition02;

        // Checks if the second half of the animation can be played - if the notification can reset itself
        if (hasPlayedPuzzleNotif && bubble02 == puzzleNotificationBubble || hasPlayedArtifactNotif && bubble02 == artifactNotificationBubble)
        {
            yield return new WaitForSeconds(notificationLength);
            time01 = 0;

            if (bubble02 == puzzleKeybindBubble || bubble02 == artifactKeybindBubble)
                scaledDuration = duration * 0.3f;
            else
                scaledDuration = duration * 0.7f;

            while (time01 < scaledDuration)
            {
                bubble02.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(endPosition02, startPosition02, time01 / scaledDuration);
                time01 += Time.deltaTime;
                yield return null;
            }

            bubble02.GetComponent<RectTransform>().anchoredPosition = startPosition02;
            time02 = 0;

            if (bubble01 == puzzleKeybindBubble || bubble01 == artifactKeybindBubble)
                scaledDuration = duration * 0.3f;
            else
                scaledDuration = duration * 0.7f;

            while (time02 < scaledDuration)
            {
                bubble01.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(endPosition01, startPosition01, time02 / scaledDuration);
                time02 += Time.deltaTime;
                yield return null;
            }

            bubble01.GetComponent<RectTransform>().anchoredPosition = startPosition01;

            if (bubble01 == puzzleKeybindBubble)
                hasPlayedPuzzleNotif = false;
            if (bubble01 == artifactKeybindBubble)
                hasPlayedArtifactNotif = false;
        }

        // Checks if the bubble can be toggled
        if (bubble02 == puzzleNotificationBubble || bubble02 == puzzleKeybindBubble)
        {
            if (canTogglePuzzleBubble != true)
                canTogglePuzzleBubble = true;
        }
        if (bubble02 == artifactNotificationBubble || bubble02 == artifactKeybindBubble)
        {
            if (canToggleArtifactBubble != true)
                canToggleArtifactBubble = true;
        } 
    }

    // Sets the vector variables for the notification bubbles
    private void SetBubbleVectors()
    {
        pnbOriginalPosition = pnbRectTransform.anchoredPosition;
        anbOriginalPosition = anbRectTransform.anchoredPosition;

        pkbOriginalPosition = pkbRectTransform.anchoredPosition;
        akbOriginalPosition = akbRectTransform.anchoredPosition;

        pnbDestination = new Vector2(-animDistanceNB, pnbOriginalPosition.y);
        anbDestination = new Vector2(animDistanceNB, anbOriginalPosition.y);

        pkbDestination = new Vector2((animDistanceKB * notificationBubbleScale), pkbOriginalPosition.y);
        akbDestination = new Vector2((-animDistanceKB * notificationBubbleScale), akbOriginalPosition.y);
    }

    // Updates the original positions for the notification bubbles
    private void UpdateOriginalPositionsCheck()
    {
        if (pnbWidth != pnbRectTransform.rect.width)
            pnbWidth = pnbRectTransform.rect.width;

        if (anbWidth != anbRectTransform.rect.width)
            anbWidth = anbRectTransform.rect.width;

        // 37.5 was added/subracted from each x position to compensate for the bubble's twitch if the text gets updated out of the screen's bounds
        if (pnbOriginalPosition != new Vector2((pnbWidth * notificationBubbleScale) + 37.5f, pnbRectTransform.anchoredPosition.y))
            pnbOriginalPosition = new Vector2((pnbWidth * notificationBubbleScale) + 37.5f, pnbRectTransform.anchoredPosition.y);

        if (anbOriginalPosition != new Vector2((-anbWidth * notificationBubbleScale) - 37.5f, anbRectTransform.anchoredPosition.y))
            anbOriginalPosition = new Vector2((-anbWidth * notificationBubbleScale) - 37.5f, anbRectTransform.anchoredPosition.y);

        // If their original position changes, move the bubbles to their new original position
        if (!isPuzzleNotification && canTogglePuzzleBubble && !hasPlayedPuzzleNotif)
        {
            if (pnbRectTransform.anchoredPosition != pnbOriginalPosition)
                pnbRectTransform.anchoredPosition = pnbOriginalPosition;
        }
        if (!isArtifactNotification && canToggleArtifactBubble && !hasPlayedArtifactNotif)
        {
            if (anbRectTransform.anchoredPosition != anbOriginalPosition)
                anbRectTransform.anchoredPosition = anbOriginalPosition;
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < gameHUDScript.transform.childCount; i++)
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
                        puzzleKeybindBubble = child02;
                        pkbRectTransform = rectTransform;
                    }
                    if (child02.name == "ArtifactKeybindBubble")
                    {
                        artifactKeybindBubble = child02;
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
    public void DebuggingCheck(GameManager gameManager)
    {
        if (gameManager.isDebugging)
        {
            if (pnbDestination != new Vector2(-animDistanceNB, pnbOriginalPosition.y))
                pnbDestination = new Vector2(-animDistanceNB, pnbOriginalPosition.y);

            if (anbDestination != new Vector2(animDistanceNB, anbOriginalPosition.y))
                anbDestination = new Vector2(animDistanceNB, anbOriginalPosition.y);

            if (pkbDestination != new Vector2((animDistanceKB * notificationBubbleScale), pkbOriginalPosition.y))
                pkbDestination = new Vector2((animDistanceKB * notificationBubbleScale), pkbOriginalPosition.y);

            if (akbDestination != new Vector2((-animDistanceKB * notificationBubbleScale), akbOriginalPosition.y))
                akbDestination = new Vector2((-animDistanceKB * notificationBubbleScale), akbOriginalPosition.y);

            // If their destination changes, move the bubbles to their new destination
            if (isPuzzleNotification && canTogglePuzzleBubble)
            {
                if (pnbRectTransform.anchoredPosition != pnbDestination)
                    pnbRectTransform.anchoredPosition = pnbDestination;

                if (pkbRectTransform.anchoredPosition != pkbDestination)
                    pkbRectTransform.anchoredPosition = pkbDestination;
            }
            if (isArtifactNotification && canToggleArtifactBubble)
            {
                if (anbRectTransform.anchoredPosition != anbDestination)
                    anbRectTransform.anchoredPosition = anbDestination;

                if (akbRectTransform.anchoredPosition != akbDestination)
                    akbRectTransform.anchoredPosition = akbDestination;
            }
        }
    }

}
