using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class NotificationBubbles : MonoBehaviour
{
    [SerializeField] [Range(0.1f, 1f)]
    private float animDuration = 0.3f; // Original Value = 0.3f
    [SerializeField] [Range(1f, 10f)]
    private float notificationDuration = 3f; // Original Value = 3f
    [SerializeField] [Range(0, 270f)]
    private float animDistanceNB = 30f; // Original Value = 30f
    [SerializeField] [Range(80f, 270f)]
    private float animDistanceKB = 100f; // Original Value = 100f
    private float bubbleScale = 0.8f;
    private float totalArtifacts;
    private float totalPuzzles;

    static readonly string tutorialZone = "TutorialMap";
    private string sceneName;

    private bool canMoveInPN = true;
    private bool canMoveOutPN = false;   
    private bool canMoveInAN = true;   
    private bool canMoveOutAN = false;   

    private bool isPlayingPN = false;
    private bool isPlayingAN = false;

    private GameObject notificationBubblesHolder;
    private GameObject artifactNotification;
    private GameObject puzzleNotification;
    private GameObject artifactKeybind;
    private GameObject puzzleKeybind;

    private RectTransform artifactNotificationRT;
    private RectTransform puzzleNotificationRT;
    private RectTransform artifactKeybindRT;
    private RectTransform puzzleKeybindRT;

    private Vector2 pn_OriginalPos; // PN = puzzle notification
    private Vector2 pn_Destination;

    private Vector2 pk_OriginalPos; // PK = puzzle keybind
    private Vector2 pk_Destination;

    private Vector2 an_OriginalPos; // AN = artifact notification
    private Vector2 an_Destination;

    private Vector2 ak_OriginalPos; // AK = artifact keybind
    private Vector2 ak_Destination;

    private TextMeshProUGUI puzzleNotificaionText;
    private TextMeshProUGUI artifactNotificationText;

    private SaveManager saveManagerScript;
    private PauseMenu pauseMenuScript;

    // Awake is called before Start()
    void Awake()
    {
        sceneName = SceneManager.GetActiveScene().name;

        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetNotificationBubbbles();
    }

    // Sets/toggles the keybind bubbles active/inactive
    public void ToggleKeybindBubbles()
    {
        bool isActive = puzzleKeybind.activeSelf ? false : true;

        artifactKeybind.SetActive(isActive);
        puzzleKeybind.SetActive(isActive);
    }

    // Sets the text for the puzzle notification bubble 
    // Note: the default value for checkToPlay will always be true if the parameter is not set
    public void SetsPuzzleNotificationText(int puzzleNumber, bool checkToPlayPN = true)
    {
        puzzleNotificaionText.text = $"{puzzleNumber}/{totalPuzzles}";

        if (!checkToPlayPN) return;
        PlayPuzzleNotificationCheck();
    }

    // Sets the text for the artifact notification bubble
    // Note: the default value for checkToPlay will always be true if the parameter is not set
    public void SetsArtifactNotificationText(int artifactCount, bool checkToPlayAN = true)
    {
        artifactNotificationText.text = $"{artifactCount}/{totalArtifacts}";

        if (!checkToPlayAN) return;
        PlayArtifactNotificationCheck();
    }

    // Checks to play the puzzle notification
    // Note: you cannot manually move the notification bubble during its sequence
    [ContextMenu("Play Puzzle Notification")]
    private void PlayPuzzleNotificationCheck()
    {
        if (!pauseMenuScript.CanPause || !puzzleNotification.activeSelf) return;

        if (!canMoveInPN || isPlayingPN) return;

        PuzzleKeybind_UpdateVectors();
        PuzzleNotification_UpdateVectors();
        StartCoroutine(PlayPuzzleNotificationSequence());
        //Debug.Log("Puzzle notification has been played");
    }

    // Checks to play the artifact notification
    // Note: you cannot manually move the notification bubble during its sequence
    [ContextMenu("Play Artifact Notification")]
    private void PlayArtifactNotificationCheck()
    {
        if (!pauseMenuScript.CanPause || !artifactNotification.activeSelf) return;

        if (!canMoveInAN || isPlayingAN) return;

        ArtifactKeybind_UpdateVectors();
        ArtifactNotification_UpdateVectors();
        StartCoroutine(PlayArtifactNotificationSequence());
        //Debug.Log("Artifact notification has been played");
    }

    // Checks to manually move the notification bubbles in/out of the screen
    public void MoveBubblesInputCheck()
    {
        //if (!gameHUDScript.CanToggleHUD()) return;
        if (!notificationBubblesHolder.activeInHierarchy) return;

        if (Input.GetKeyDown(KeyCode.Q))
            LerpArtifactNotifcation();

        if (Input.GetKeyDown(KeyCode.E))
            LerpPuzzleNotifcation();
    }

    // Checks to move the puzzle notification bubble in/out of the screen
    private void LerpPuzzleNotifcation()
    {
        if (isPlayingPN || !canMoveInPN && !canMoveOutPN) return;

        PuzzleNotification_UpdateVectors();
        PuzzleKeybind_UpdateVectors();

        if (canMoveInPN) // If PK has lerped to its original position
            StartCoroutine(PuzzleNotification_MoveIn());

        else if (canMoveOutPN) // If PN has lerped to its destination
            StartCoroutine(PuzzleNotification_MoveOut());
    }

    // Checks to move the artifact notification bubble in/out of the screen
    private void LerpArtifactNotifcation()
    {
        if (isPlayingAN || !canMoveInAN && !canMoveOutAN) return;

        ArtifactNotification_UpdateVectors();
        ArtifactKeybind_UpdateVectors();

        if (canMoveInAN) // If AK has lerped to its original position
            StartCoroutine(ArtifactNotification_MoveIn());
      
        else if (canMoveOutAN) // If AN has lerped to its destination
            StartCoroutine(ArtifactNotification_MoveOut());
    }

    // Updates the vectors for the puzzle notification bubble
    private void PuzzleNotification_UpdateVectors()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(puzzleNotificationRT);
        float currentWidth = puzzleNotificationRT.rect.width;
        float currentPosY = puzzleNotificationRT.anchoredPosition.y;

        pn_Destination = new Vector2(-animDistanceNB, currentPosY);
        pn_OriginalPos = new Vector2((currentWidth * bubbleScale) + 20f, currentPosY);
    }

    // Updates the vectors for the puzzle keybind bubble
    private void PuzzleKeybind_UpdateVectors()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(puzzleKeybindRT);
        float currentPosY = puzzleKeybindRT.anchoredPosition.y;

        pk_OriginalPos = new Vector2(0f, currentPosY);
        pk_Destination = new Vector2(animDistanceKB, currentPosY);
    }

    // Updates the vectors for the artifact notification bubble
    private void ArtifactNotification_UpdateVectors()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(artifactNotificationRT);
        float currentWidth = artifactNotificationRT.rect.width;
        float currentPosY = artifactNotificationRT.anchoredPosition.y;

        an_Destination = new Vector2(animDistanceNB, currentPosY);
        an_OriginalPos = new Vector2(-(currentWidth * bubbleScale) - 20f, currentPosY);
    }

    // Updates the vectors for the artifact keybind bubble
    private void ArtifactKeybind_UpdateVectors()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(artifactKeybindRT);
        float currentPosY = artifactKeybindRT.anchoredPosition.y;

        ak_OriginalPos = new Vector2(0f, currentPosY);
        ak_Destination = new Vector2(-animDistanceKB, currentPosY);
    }

    // Returns the adjusted animation duration for appropriate rect transform
    // Note: 0.7f + 0.3f = 1f = the full value of animDuration
    private float AnimDuration(RectTransform objectRT)
    {
        // animDuration * 0.7f = duration for notification bubbles
        // animDuration * 0.3f = duration for keybind bubbles
        return objectRT.name.Contains("Notification") ? animDuration * 0.7f : animDuration * 0.3f;
    }

    // Lerps the position of the object to another over a duration (duration = seconds)
    private IEnumerator LerpPosition(RectTransform objectRT, Vector2 startPos, Vector2 endPos)
    {
        objectRT.anchoredPosition = startPos;
        float duration = AnimDuration(objectRT);
        float time = 0f;

        while (time < duration)
        {
            objectRT.anchoredPosition = Vector2.Lerp(startPos, endPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        objectRT.anchoredPosition = endPos;
    }

    // Moves the puzzle notification bubble onto the screen
    private IEnumerator PuzzleNotification_MoveIn()
    {
        canMoveInPN = false;
        puzzleNotificationRT.anchoredPosition = pn_OriginalPos;
        StartCoroutine(LerpPosition(puzzleKeybindRT, pk_OriginalPos, pk_Destination)); // Lerps PK to destination

        yield return new WaitForSeconds(AnimDuration(puzzleKeybindRT));
        StartCoroutine(LerpPosition(puzzleNotificationRT, pn_OriginalPos, pn_Destination)); // Lerps PN to destination

        yield return new WaitForSeconds(AnimDuration(puzzleNotificationRT));
        canMoveOutPN = true;
    }

    // Moves the puzzle notification bubble out of the screen
    private IEnumerator PuzzleNotification_MoveOut()
    {
        canMoveOutPN = false;
        StartCoroutine(LerpPosition(puzzleNotificationRT, pn_Destination, pn_OriginalPos)); // Lerps PN to original position

        yield return new WaitForSeconds(AnimDuration(puzzleNotificationRT));
        StartCoroutine(LerpPosition(puzzleKeybindRT, pk_Destination, pk_OriginalPos)); // Lerps PK to original position

        yield return new WaitForSeconds(AnimDuration(puzzleKeybindRT));
        canMoveInPN = true;
    }

    // Moves the artifact notification bubble onto the screen
    private IEnumerator ArtifactNotification_MoveIn()
    {
        canMoveInAN = false;
        artifactKeybindRT.anchoredPosition = an_OriginalPos;
        StartCoroutine(LerpPosition(artifactKeybindRT, ak_OriginalPos, ak_Destination)); // lerps AK to destination

        yield return new WaitForSeconds(AnimDuration(artifactKeybindRT));
        StartCoroutine(LerpPosition(artifactNotificationRT, an_OriginalPos, an_Destination)); // Lerps AN to destination

        yield return new WaitForSeconds(AnimDuration(artifactNotificationRT));
        canMoveOutAN = true;
    }

    // Moves the artifact notification bubble out of the screen
    private IEnumerator ArtifactNotification_MoveOut()
    {
        canMoveOutAN = false;
        StartCoroutine(LerpPosition(artifactNotificationRT, an_Destination, an_OriginalPos)); // Lerps AN to original position

        yield return new WaitForSeconds(AnimDuration(artifactNotificationRT));
        StartCoroutine(LerpPosition(artifactKeybindRT, ak_Destination, ak_OriginalPos)); // Lerps AK to original position

        yield return new WaitForSeconds(AnimDuration(artifactKeybindRT));
        canMoveInAN = true;
    }

    // Plays the puzzle notification sequence
    private IEnumerator PlayPuzzleNotificationSequence()
    {
        isPlayingPN = true;
        StartCoroutine(PuzzleNotification_MoveIn());

        yield return new WaitForSeconds(animDuration + notificationDuration);
        StartCoroutine(PuzzleNotification_MoveOut());

        yield return new WaitForSeconds(animDuration);
        isPlayingPN = false;
    }

    // Plays the artifact notification sequence
    private IEnumerator PlayArtifactNotificationSequence()
    {
        isPlayingAN = true;
        StartCoroutine(ArtifactNotification_MoveIn());

        yield return new WaitForSeconds(animDuration + notificationDuration);
        StartCoroutine(ArtifactNotification_MoveOut());

        yield return new WaitForSeconds(animDuration);
        isPlayingAN = false;
    }

    // Sets the initial value for each notification bubble
    private void SetNotificationBubbbles()
    {
        SetsArtifactNotificationText(saveManagerScript.ArtifactCount, false);
        SetsPuzzleNotificationText(saveManagerScript.CameraIndex + 1, false);
    }

    // Sets the vectors to use
    private void SetVectors()
    {
        an_OriginalPos = artifactNotificationRT.anchoredPosition;
        pn_OriginalPos = puzzleNotificationRT.anchoredPosition;

        ak_OriginalPos = artifactKeybindRT.anchoredPosition;
        pk_OriginalPos = puzzleKeybindRT.anchoredPosition;

        an_Destination = new Vector2(animDistanceNB, an_OriginalPos.y);
        pn_Destination = new Vector2(-animDistanceNB, pn_OriginalPos.y);

        ak_Destination = new Vector2(-animDistanceKB, ak_OriginalPos.y);
        pk_Destination = new Vector2(animDistanceKB, pk_OriginalPos.y);
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        saveManagerScript = FindObjectOfType<SaveManager>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "NB_Holder":
                    notificationBubblesHolder = child.gameObject;
                    break;
                case "PuzzleKeybind":
                    puzzleKeybindRT = child.GetComponent<RectTransform>();
                    puzzleKeybind = child.gameObject;
                    break;
                case "ArtifactKeybind":
                    artifactKeybindRT = child.GetComponent<RectTransform>();
                    artifactKeybind = child.gameObject;
                    break;
                case "PuzzleNotification":
                    puzzleNotificationRT = child.GetComponent<RectTransform>();
                    puzzleNotification = child.gameObject;
                    break;
                case "ArtifactNotification":
                    artifactNotificationRT = child.GetComponent<RectTransform>();
                    artifactNotification = child.gameObject;
                    break;
                case "PN_Text":
                    puzzleNotificaionText = child.GetComponent<TextMeshProUGUI>();
                    break;
                case "AN_Text":
                    artifactNotificationText = child.GetComponent<TextMeshProUGUI>();
                    break;
                default:
                    break;
            }

            SetVariables(child);
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        SetVariables(transform);
        SetVectors();

        totalPuzzles = GameObject.FindGameObjectsWithTag("Checkpoint").Length;
        totalArtifacts = (sceneName != tutorialZone) ? 15 : 1;
    }

    // Checks to update the destination for the notification bubbles - For Debugging Purposes ONLY
    public void DebuggingCheck()
    {
        if (canMoveOutPN && !isPlayingPN)
        {
            PuzzleNotification_UpdateVectors();
            puzzleNotificationRT.anchoredPosition = pn_Destination;

            PuzzleKeybind_UpdateVectors();
            puzzleKeybindRT.anchoredPosition = pk_Destination;
        }
        if (canMoveOutAN && !isPlayingAN)
        {
            ArtifactNotification_UpdateVectors();
            artifactNotificationRT.anchoredPosition = an_Destination;

            ArtifactKeybind_UpdateVectors();
            artifactKeybindRT.anchoredPosition = ak_Destination;
        }
    }

}
