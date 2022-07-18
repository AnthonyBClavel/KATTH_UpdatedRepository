using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialDialogue : MonoBehaviour
{
    [SerializeField] [Range(0.005f, 0.1f)]
    private float typingSpeed = 0.03f; // Original Value = 0.03f
    [SerializeField] [Range(0, 3f)]
    private float fadeDuration = 1f; // Original Value = 1f
    private float originalTypingSpeed;

    static readonly string tutorialZone = "TutorialMap";
    private string sceneName;
    private string[] sentences;
    private int sentenceIndex;

    private bool hasPlayedWelcome = false;
    private bool hasPassedBridge = false;
    private bool hasPlayedPush = false;

    private GameObject tutorialDialogueHolder;
    private GameObject artifactButtons;
    private GameObject continueButton;

    public List<TextAsset> tutorialDialogue = new List<TextAsset>();
    private TextAsset deathDialogue;

    private TextMeshProUGUI tutorialDialogueText;
    private Image blackOverlay;

    private IEnumerator dialogueInputCoroutine;
    private IEnumerator fadeOverlayCorouitne;

    private CharacterDialogue characterDialogueScript;
    private SkipSceneButton skipSceneButtonScript;
    private TileMovementController playerScript;
    private AudioManager audioManagerScript;
    private PauseMenu pauseMenuScript;
    private HUD headsUpDisplayScript;
    private Artifact artifactScript;

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
        SetTutorialDialogueInactive();
    }

    // Checks to set the tutorial dialogue game object and script inactive
    private void SetTutorialDialogueInactive()
    {
        if (sceneName == tutorialZone) return;

        gameObject.SetActive(false);
        enabled = false;
    }

    // Starts the tutorial dialogue
    private void StartDialogue()
    {
        skipSceneButtonScript.SetSkipSceneButtonInactive();
        playerScript.SetPlayerBoolsFalse();

        tutorialDialogueHolder.SetActive(true);
        tutorialDialogueText.text = string.Empty;

        originalTypingSpeed = typingSpeed; //typingSpeed = originalTypingSpeed;
        pauseMenuScript.enabled = false;
        sentenceIndex = 0;

        StartFadeOverlayCoroutine(0.5f, fadeDuration);
        StartCoroutine(TypeTutorialDialogue());
        StartDialogueInputCoroutine();
    }

    // Ends the tutorial dialogue
    private void EndDialogue()
    {
        if (characterDialogueScript.IsInteractingWithArtifact)
        {
            artifactButtons.SetActive(true);
            artifactScript.StartInputCoroutine();
        }
        else
        {
            skipSceneButtonScript.SetSkipSceneButtonActive();
            playerScript.SetPlayerBoolsTrue();
        }

        tutorialDialogueHolder.SetActive(false);
        tutorialDialogueText.text = string.Empty;

        pauseMenuScript.enabled = true;
        blackOverlay.SetImageAlpha(0f);
    }

    // Sets the array of dialogue sentences
    private void SetDialogue(TextAsset textFile)
    {
        string[] textFileSentences = textFile.text.Split("\n"[0]);
        sentences = textFileSentences;
    }

    // Plays the death dialogue
    public void PlayDeathDialogue()
    {
        SetDialogue(deathDialogue);
        StartDialogue();
    }

    // Checks to play the welcome dialogue
    public void PlayWelcomeDialogueCheck()
    {
        if (hasPlayedWelcome) return;

        PlayDialogueCheck("Welcome");
        hasPlayedWelcome = true;
    }

    // Checks to play the pushable block dialogue 
    public void PlayPushDialogueCheck(Collider collider)
    {
        if (collider == null || !collider.CompareTag("PushableBlock") || hasPlayedPush) return;

        PlayDialogueCheck("Push");
        hasPlayedPush = true;
    }

    // Checks to play the bridge dialogue
    public void PlayBridgeDialogueCheck(bool onLastBridgeTile)
    {
        if (!onLastBridgeTile || !hasPlayedWelcome || hasPassedBridge) return;

        PlayDialogueCheck("Bridge");
        hasPassedBridge = true;
    }

    // Checks if the dialogue can be played - returns true and plays the dialogue if so, false otherwise
    public bool PlayDialogueCheck(string nameOfTextAsset)
    {
        if (tutorialDialogue.Count == 0) return false;

        foreach (TextAsset textFile in tutorialDialogue)
        {
            if (textFile.name != nameOfTextAsset) continue;

            SetDialogue(textFile);
            StartDialogue();
            tutorialDialogue.Remove(textFile);
            return true;
        }
        return false;
    }

    // Checks to play the next sentence in the tutorial dialogue
    private void NextSentenceCheck()
    {
        continueButton.SetActive(false);
        originalTypingSpeed = typingSpeed; //typingSpeed = originalTypingSpeed;
        tutorialDialogueText.text = string.Empty;

        if (sentenceIndex < sentences.Length - 1 && sentences[sentenceIndex + 1] != string.Empty)
        {
            sentenceIndex++;
            StartCoroutine(TypeTutorialDialogue());
        }
        else EndDialogue();
    }

    // Checks for the input to continue or speed-up the tutorial dialogue
    private void ContinueInputCheck()
    {
        if (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.KeypadEnter)) return;

        if (!continueButton.activeSelf && typingSpeed > originalTypingSpeed / 2)
            typingSpeed /= 2;

        else if (continueButton.activeSelf) 
            NextSentenceCheck();       
    }

    // Starts the coroutine that fades the overlay
    private void StartFadeOverlayCoroutine(float endAlpha, float duration)
    {
        if (fadeOverlayCorouitne != null) StopCoroutine(fadeOverlayCorouitne);

        fadeOverlayCorouitne = FadeOverlay(endAlpha, duration);
        StartCoroutine(fadeOverlayCorouitne);
    }

    // Starts the coroutine that checks for the tutorial dialogue input
    private void StartDialogueInputCoroutine()
    {
        if (dialogueInputCoroutine != null) StopCoroutine(dialogueInputCoroutine);

        dialogueInputCoroutine = TutorialDialogueInputCheck();
        StartCoroutine(TutorialDialogueInputCheck());
    }

    // Lerps the alpha of the overlay to another over a specific duartion (duration = seconds)
    private IEnumerator FadeOverlay(float endAlpha, float duration)
    {
        Color startColor = blackOverlay.color;
        Color endColor = blackOverlay.ReturnImageColor(endAlpha);
        float time = 0;

        while (time < duration)
        {
            blackOverlay.color = Color.Lerp(startColor, endColor, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        blackOverlay.color = endColor;
    }

    // Types out the text for the tutorial dialogue
    private IEnumerator TypeTutorialDialogue()
    {
        yield return new WaitForSeconds(0.03f);

        foreach (char letter in sentences[sentenceIndex])
        {
            tutorialDialogueText.text += letter;
            audioManagerScript.PlayCharNoiseSFX();
            yield return new WaitForSeconds(typingSpeed);
        }

        typingSpeed = originalTypingSpeed;
        continueButton.SetActive(true);
    }

    // Checks for the tutorial dialogue input
    private IEnumerator TutorialDialogueInputCheck()
    {
        while (!pauseMenuScript.enabled)
        {
            if (Time.deltaTime > 0) ContinueInputCheck();
            yield return null;
        }
        //Debug.Log("Tutorial dialogue input check has ENDED");
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        skipSceneButtonScript = FindObjectOfType<SkipSceneButton>();
        playerScript = FindObjectOfType<TileMovementController>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        artifactScript = FindObjectOfType<Artifact>();
        headsUpDisplayScript = FindObjectOfType<HUD>();
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "ContinueButton":
                    continueButton = child.gameObject;
                    break;
                case "ArtifactButtons":
                    artifactButtons = child.gameObject;
                    break;
                case "TutorialDialogue":
                    tutorialDialogueHolder = child.gameObject;
                    break;
                case "TD_BlackOverlay":
                    blackOverlay = child.GetComponent<Image>();
                    break;
                case "TD_Text":
                    tutorialDialogueText = child.GetComponent<TextMeshProUGUI>();
                    break;
                default:
                    break;
            }

            if (child.parent.name == "DialogueOptionButtons" || child.parent.name == "ArtifactButtons") continue;
            if (child.name == "NotificationBubbles" || child.name == "CharacterDialogue") continue;

            SetVariables(child);
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the death dialogue and removes it from the list
        foreach (TextAsset textFile in tutorialDialogue)
        {
            if (textFile.name != "Death") continue;

            deathDialogue = textFile;
            tutorialDialogue.Remove(textFile);
            break;
        }

        SetVariables(headsUpDisplayScript.transform);
        originalTypingSpeed = typingSpeed;
    }

}
