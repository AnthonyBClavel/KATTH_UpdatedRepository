using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;

public class CharacterDialogue : MonoBehaviour
{
    [Header("Dialogue Arrow Animation Variables")]
    [SerializeField] [Range(0.1f, 2.0f)]
    private float animDuration = 1f; // Original Value = 1f

    [SerializeField] [Range(1f, 90f)]
    private float animDistance = 15f; // Original Value = 15f

    [Header("Character Dialogue Variables")]
    [SerializeField] [Range(0.005f, 0.1f)]
    private float typingSpeed = 0.03f; // Original Value = 0.03f
    private float originalTypingSpeed; // Original Value = 0.03f
    private float sbhOffsetPositionY; // sbh = speech bubble holder
    private float sbhOffsetPositionX; // sbh = speech bubble holder

    private int dialogueOptionsIndex;
    private int sentenceIndex;
    private int artifactIndex;

    private string currentlyTalking;
    private string nextSentence;

    private bool isInteractingWithArtifact = false;
    private bool isInteractingWithNPC = false;
    private bool isPlayerSpeaking = false;

    private bool hasSelectedDialogueOption = false;
    private bool inDialogueOptions = false;
    private bool inDialogue = false;

    private GameObject playerDialogueBubble;
    private GameObject nPCDialogueBubble;
    private GameObject playerDialogueCheck;
    private GameObject nPCDialogueCheck;
    private GameObject alertBubble;
    private GameObject dialogueArrow;
    private GameObject continueButton;
    private GameObject dialogueOptionButtons;

    private Image playerBubbleSprite;
    private Image playerTailSprite;
    private Image nPCBubbleSprite;
    private Image nPCTailSprite;
    private Image alertBubbleSprite;
    private Image alertIconSprite;
    private Image smallArrowSprite;
    private Image bigArrowSprite;

    private RectTransform playerDB; // DB = dialogue bubble
    private RectTransform nPCDB; // DB = dialogue bubble
    private RectTransform playerSBH; // SBH = speech bubble holder
    private RectTransform nPCSBH; // SBH = speech bubble holder
    private RectTransform alertBubbleRT;
    private RectTransform dialogueArrowRT;
    private RectTransform characterDialogueRT;
    private RectTransform canvasRT;

    private VerticalLayoutGroup playerSpeechBubbleVLG;
    private Animator playerBubbleAnim;
    private Animator nPCBubbleAnim;
    private Camera dialogueCamera;

    private Vector2 bubbleHolderMidPos;
    private Vector2 bubbleHolderRightPos;
    private Vector2 bubbleHolderLeftPos;

    private Vector2 middlePivot;
    private Vector2 rightPivot;
    private Vector2 leftPivot;

    private Vector2 dialogueArrowDestination;
    private Vector2 dialogueArrowOrigPos;

    private Vector2 pdcAnchorPos; // pdc = player dialogue check
    private Vector2 npcdcAnchorPos; // npcdc = npc dialogue check

    [Header("Lists/Arrays")]
    private List<TextMeshProUGUI> doTextComponenets = new List<TextMeshProUGUI>();
    private List<GameObject> doGameObjects = new List<GameObject>();
    private string[] dialogueSentences;
    private string[] dialogueOptions;
    private TextAsset[] nPCDialogue;
    private TextAsset[] artifactDialogue;
    public TextAsset[] emptyChestDialogue;

    [Header("Text Variables")]
    private StringBuilder textToColor = new StringBuilder();
    private TextMeshProUGUI playerDialogueText;
    private TextMeshProUGUI nPCDialogueText;
    private Color32 playerTextColor = new Color32(128, 160, 198, 255);
    private Color32 playerBubbleColor = Color.white;
    private Color32 nPCTextColor;
    private Color32 nPCBubbleColor;
    private Color32 unselectedTextColor = Color.gray;

    [Header("Scriptable Objects")]
    private NonPlayerCharacter_SO nonPlayerCharacter;
    private Artifact_SO artifact;

    private IEnumerator dialogueOptionsInputCoroutine;
    private IEnumerator dialogueInputCoroutine;
    private IEnumerator dialogueOptionsCoroutine;
    private IEnumerator dialogueArrowCoroutine;

    private Artifact artifactScript;
    private HUD headsUpDisplayScript;
    private BlackBars blackBarsScript;
    private NonPlayerCharacter nPCScript;
    private CameraController cameraScript;
    private FidgetController nPCFidgetScript;
    private AudioManager audioManagerScript;
    private TileMovementController playerScript;
    private FidgetController playerFidgetScript;
    private BlackOverlay blackOverlayScript;

    public bool InDialogue
    {
        get { return inDialogue; }
    }

    public bool InDialogueOptions
    {
        get { return inDialogueOptions; }
    }

    public bool IsInteractingWithArtifact
    {
        get { return isInteractingWithArtifact; }
    }

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Update is called once per frame
    void Update()
    {
        AdjustDialogueBubbles();
        SetAlertBubblePosition();
    }

    // Updates and starts the dialogue for the npc
    public void StartNPCDialogue(NonPlayerCharacter newScript, NonPlayerCharacter_SO newNPC)
    {
        if (nonPlayerCharacter != newNPC)
        {
            nonPlayerCharacter = newNPC;
            nPCScript = newScript;

            nPCTextColor = nonPlayerCharacter.nPCTextColor;
            nPCBubbleColor = nonPlayerCharacter.nPCBubbleColor;
            nPCDialogue = nonPlayerCharacter.nPCDialogue;

            nPCDialogueCheck = nPCScript.DialogueCheck;
            nPCFidgetScript = nPCScript.FidgetScript;
        }

        isInteractingWithNPC = true;
        SetDialogueOptions(nonPlayerCharacter.dialogueOptions);
        StartCoroutine(StartDialogueDelay());
    }

    // Updates and starts the dialogue for the artifact
    public void StartArtifactDialogue(Artifact newScript, Artifact_SO newArtifact)
    {
        if (artifact != newArtifact)
        {
            artifact = newArtifact;
            artifactScript = newScript;      
        }

        bool hasCollectedArtifact = PlayerPrefs.GetString("listOfArtifacts").Contains(artifact.name);
        TextAsset dialogueOptions = hasCollectedArtifact ? null : artifact.dialogueOptions;
        artifactDialogue = hasCollectedArtifact ? emptyChestDialogue : artifact.artifactDialogue;
        artifactScript.ArtifactHolder.SetActive(hasCollectedArtifact ? false : true);

        isInteractingWithArtifact = true;
        SetDialogueOptions(dialogueOptions);
        StartCoroutine(StartDialogueDelay());
    }

    // Sets the character dialogue
    private void SetCharacterDialogue(TextAsset characterDialogue)
    {
        dialogueSentences = characterDialogue.ReturnSentences();
    }

    // Sets the dialogue options
    private void SetDialogueOptions(TextAsset dialogeOptionsFile)
    {
        dialogueOptions = dialogeOptionsFile.ReturnSentences();
        if (!isInteractingWithNPC) return;

        nPCScript.SetDialogueOptionBools(dialogueOptions.Length);
    }

    // Starts the character dialogue
    private void StartDialogue()
    {
        originalTypingSpeed = typingSpeed; //typingSpeed = originalTypingSpeed;
        sentenceIndex = 0;

        NextDialogueSentence(dialogueSentences[0]);
        StartDialogueInputCoroutine();
    }

    // Ends the character dialogue
    private void EndDialogue()
    {
        // Note: the dialogue will/should always end after choosing/playing the last dialogue option
        if (dialogueOptions == null || dialogueOptionsIndex == dialogueOptions.Length - 1)
        {
            if (isInteractingWithArtifact) 
                artifactScript.CloseChest();

            StartCoroutine(EndDialogueDelay());
        }
        else OpenDialogueOptions();

        currentlyTalking = string.Empty;
    }

    // Sets the dialogue for initially interacting with an npc/artifact
    private void SetInitialDialogue()
    {
        if (isInteractingWithNPC)
        {
            int initialIndex = nPCScript.HasPlayedInitialDialogue ? 1 : 0;

            nPCScript.SetRotation();
            nPCDialogueText.color = nPCTextColor;
            nPCScript.HasPlayedInitialDialogue = true;
            SetCharacterDialogue(nPCDialogue[initialIndex]);
        }
        else if (isInteractingWithArtifact)
            SetCharacterDialogue(RandomArtifactDialogue());
    }

    // Returns a randomly selected text asset for the artifact dialogue
    private TextAsset RandomArtifactDialogue()
    {
        int newArtifactIndex = Random.Range(0, artifactDialogue.Length);
        int attempts = 3;

        // Attempts to set a text asset that's different from the one previously played
        while (newArtifactIndex == artifactIndex && attempts > 0)
        {
            newArtifactIndex = Random.Range(0, artifactDialogue.Length);
            attempts--;
        }

        artifactIndex = newArtifactIndex;
        return artifactDialogue[newArtifactIndex];
    }

    // Sets and plays the next dialogue sentence
    private void NextDialogueSentence(string theString)
    {
        string characterName = theString.Remove(theString.IndexOf(':') + 1);
        nextSentence = theString.Replace(characterName, "").Trim();
        isPlayerSpeaking = characterName.Contains("PLAYER");
        PlayFidgetAndBubblePopSFX(characterName);

        StartCoroutine(TypeCharacterDialogue());
    }

    // Checks for the next dialogue sentence to play - ends the dialogue otherwise
    private void NextSentenceCheck()
    {
        originalTypingSpeed = typingSpeed; //typingSpeed = originalTypingSpeed;
        continueButton.SetActive(false);

        if (sentenceIndex < dialogueSentences.Length - 1 && dialogueSentences[sentenceIndex + 1] != string.Empty)
            NextDialogueSentence(dialogueSentences[++sentenceIndex]);

        else EndDialogue();
    }

    // Checks to set the dialogue options active/inactive
    private void ShowDialgoueOptionsCheck()
    {
        for (int i = 0; i < dialogueOptions.Length; i++)
        {
            doTextComponenets[i].color = unselectedTextColor;
            if (dialogueOptions[i].Contains("Collect") && !CanShowCollectOption()) continue;

            doTextComponenets[i].text = inDialogueOptions ? dialogueOptions[i] : string.Empty;
            doGameObjects[i].SetActive(inDialogueOptions ? true : false);
        }
    }

    // Checks if the dialogue option for collecting the artifact can be shown - returns true if so, false otherwise
    // Note: the option should only be shown AFTER the player has inspected the artifact
    private bool CanShowCollectOption()
    {
        if (isInteractingWithArtifact && !artifactScript.HasInspectedArtifact) return false;

        return true;
    }

    // "Opens" the dialogue options
    public void OpenDialogueOptions()
    {
        if (dialogueOptionsCoroutine != null) StopCoroutine(dialogueOptionsCoroutine);

        dialogueOptionsCoroutine = OpenDialogueOptionsDelay();
        StartCoroutine(dialogueOptionsCoroutine);
    }

    // "Closes" the dialogue options
    private void CloseDialogueOptions()
    {
        inDialogueOptions = false;
        sentenceIndex = 0;

        playerDialogueText.gameObject.SetActive(true);
        dialogueOptionButtons.SetActive(false);
        dialogueArrow.SetActive(false);

        playerSpeechBubbleVLG.childAlignment = TextAnchor.MiddleCenter;
        playerSpeechBubbleVLG.padding.left = 20;

        StopDialogueArrowAnim();
        ShowDialgoueOptionsCheck(); // call this LAST!
    }

    // Checks to play the selected dialgoue option (for npc)
    private void SelectedDialogueCheckForNPC()
    {
        int original = (dialogueOptionsIndex * 2) + 2;
        int variant = original + 1;

        // Checks which closing version to set - if a dialogue option was/wasn't selected previously
        if (dialogueOptionsIndex == dialogueOptions.Length - 1)
        {
            int closing = hasSelectedDialogueOption ? original : variant;
            SetCharacterDialogue(nPCDialogue[closing]);
        }
        // Checks which dialogue version to set - if the dialogue option has/hasn't been played
        else
        {
            int dialogue = !nPCScript.DialogueOptionBools[dialogueOptionsIndex] ? original : variant;
            nPCScript.DialogueOptionBools[dialogueOptionsIndex] = true;
            hasSelectedDialogueOption = true;
            SetCharacterDialogue(nPCDialogue[dialogue]);
        }

        CloseDialogueOptions();
        StartDialogue(); // call this LAST!
    }

    // Checks which methods to call based on the selected dialgoue option (for artifact)
    private void SelectedDialogueCheckForArtifact()
    {
        string dialogueOption = dialogueOptions[dialogueOptionsIndex];

        // Checks to inspect the artifact
        if (dialogueOption.Contains("Inspect"))
        {
            artifactScript.HasInspectedArtifact = true;
            artifactScript.InspectArtifact();
        }
        // Ends the artifact dialogue otherwise
        else
        {
            // Checks to collect the artifact
            if (dialogueOption.Contains("Collect"))
                artifactScript.CollectArtifact();

            artifactScript.CloseChest();
            StartCoroutine(EndDialogueDelay());
        }

        playerDialogueBubble.SetActive(false);
        CloseDialogueOptions();
    }

    // Clears the text within all dialogue-related text components
    private void EmptyTextComponents()
    {
        playerDialogueText.text = string.Empty;
        nPCDialogueText.text = string.Empty;
        textToColor.Length = 0;
    }

    // Checks to play the character's fidget animation and bubble pop sfx
    private void PlayFidgetAndBubblePopSFX(string characterName)
    {
        if (currentlyTalking == characterName) return;

        if (isPlayerSpeaking)
        {
            playerFidgetScript.Fidget();
            audioManagerScript.PlayPlayerDialogueBubbleSFX();
        }
        else
        {
            nPCFidgetScript.Fidget();
            audioManagerScript.PlayNPCDialogueBubbleSFX();
        }
        currentlyTalking = characterName;
    }

    // Set the position of the dialogue arrow (doIndex = dialogue option index)
    private void SetDialoguArrowPosition(int doIndex)
    {
        dialogueArrow.transform.SetParent(doGameObjects[doIndex].transform);
        dialogueArrowRT.anchoredPosition = dialogueArrowOrigPos;
        smallArrowSprite.color = playerTextColor; // Sets arrow color
        audioManagerScript.PlayArrowSelectSFX(); // Plays selected sfx

        // Highlights the current selected dialogue option
        for (int i = 0; i < doTextComponenets.Count; i++)
        {
            Color textColor = (i == doIndex) ? playerTextColor : unselectedTextColor;
            doTextComponenets[i].color = textColor;
        }
    }

    // Sets the color for the player dialogue bubble
    private void SetPlayerBubbleColor(Color32 color)
    {
        if (playerDialogueText.color == color) return;

        playerDialogueText.color = color;
        playerBubbleSprite.color = color;
        playerTailSprite.color = color;
        bigArrowSprite.color = color;
        //Debug.Log("Updated bubble color for PLAYER");
    }

    // Sets the color for the npc dialogue bubble
    private void SetNPCBubbleColor(Color32 color)
    {
        if (nPCDialogueText.color == color) return;

        nPCDialogueText.color = color;
        nPCBubbleSprite.color = color;
        nPCTailSprite.color = color;
        //Debug.Log("Updated bubble color for NPC");
    }

    // Sets the color for the alert bubble
    public void SetAlertBubbleColor(Color32? bubbleColor = null, Color32? iconColor = null)
    {
        alertBubbleSprite.color = bubbleColor ?? playerBubbleColor;
        alertIconSprite.color = iconColor ?? playerTextColor;
    }

    // Sets the pivots for the speech bubble holders
    private void SetSpeechBubblePivots()
    {
        Vector3 playerDirection = playerScript.transform.eulerAngles;

        switch (playerDirection.y)
        {
            case 0: // Looking north
                playerSBH.pivot = middlePivot;
                nPCSBH.pivot = middlePivot;
                break;
            case 90: // Looking east
                playerSBH.pivot = rightPivot;
                nPCSBH.pivot = leftPivot;
                break;
            case 180: // Looking south
                playerSBH.pivot = middlePivot;
                nPCSBH.pivot = middlePivot;
                break;
            case 270: //Looking west
                playerSBH.pivot = leftPivot;
                nPCSBH.pivot = rightPivot;
                break;
            default:
                //Debug.Log("Unrecognizable direction");
                break;
        }
    }

    // Adjusts the dialogue bubbles to stay within the screen bounds
    public void AdjustDialogueBubbles()
    {
        if (!inDialogue) return;

        // Note: 66f = (96f) bigArrowSprite.rectTransform.rect.width - (40f) playerSpeechBubbleVLG.padding.left - (5f) dialogueArrowOrigPos.x + (15f) animDistance;
        float maxScreenPosX = (canvasRT.rect.width / 2f) - 66f;
        float maxScreenPosY = (canvasRT.rect.height / 2f) - blackBarsScript.FinalHeight;
        Vector3 playerDirection = playerScript.transform.eulerAngles;

        SetDialogueBubblePosition(playerDB, playerSBH, playerDialogueCheck, ref pdcAnchorPos, maxScreenPosY);
        SetDialogueBubblePosition(nPCDB, nPCSBH, nPCDialogueCheck, ref npcdcAnchorPos, maxScreenPosY);

        switch (playerDirection.y)
        {
            case 0: // Looking north
                AdjustSpeechBubbleCheck(playerDB, playerSBH, maxScreenPosX);
                if (isInteractingWithNPC) AdjustSpeechBubbleCheck(nPCDB, nPCSBH, maxScreenPosX);
                break;
            case 90: // Looking east
                AdjustSpeechBubbleRightCheck(playerDB, playerSBH, maxScreenPosX);
                if (isInteractingWithNPC) AdjustSpeechBubbleLeftCheck(nPCDB, nPCSBH, maxScreenPosX);
                break;
            case 180: // Looking south
                AdjustSpeechBubbleCheck(playerDB, playerSBH, maxScreenPosX);
                if (isInteractingWithNPC) AdjustSpeechBubbleCheck(nPCDB, nPCSBH, maxScreenPosX);
                break;
            case 270: // looking west
                AdjustSpeechBubbleLeftCheck(playerDB, playerSBH, maxScreenPosX);
                if (isInteractingWithNPC) AdjustSpeechBubbleRightCheck(nPCDB, nPCSBH, maxScreenPosX);
                break;
            default:
                //Debug.Log("Unrecognizable direction");
                break;
        }
    }

    // Adjusts the speech bubble's position to the right if applicable - with respect to left screen bound
    private void AdjustSpeechBubbleRightCheck(RectTransform dailogueBubble, RectTransform speechBubbleHolder, float maxScreenPosX)
    {
        if (!dailogueBubble.gameObject.activeInHierarchy) return;

        float minPosX = speechBubbleHolder.rect.width - maxScreenPosX;      
        float dbAnchorPosX = dailogueBubble.anchoredPosition.x;

        float newAnchorPosX = minPosX - dbAnchorPosX;
        float maxAnchorPosX = speechBubbleHolder.rect.width - sbhOffsetPositionX;

        if (dbAnchorPosX < minPosX - sbhOffsetPositionX)
            speechBubbleHolder.anchoredPosition = new Vector2((newAnchorPosX < maxAnchorPosX) ? newAnchorPosX : maxAnchorPosX, sbhOffsetPositionY);
        else
            speechBubbleHolder.anchoredPosition = bubbleHolderRightPos;
    }

    // Adjusts the speech bubble's position to the left if applicable - with respect to right screen bound
    private void AdjustSpeechBubbleLeftCheck(RectTransform dailogueBubble, RectTransform speechBubbleHolder, float maxScreenPosX)
    {
        if (!dailogueBubble.gameObject.activeInHierarchy) return;

        float maxPosX = maxScreenPosX - speechBubbleHolder.rect.width;
        float dbAnchorPosX = dailogueBubble.anchoredPosition.x;

        float newAnchorPosX = maxPosX - dbAnchorPosX;
        float minAnchorPosX = sbhOffsetPositionX - speechBubbleHolder.rect.width;

        if (dbAnchorPosX > maxPosX + sbhOffsetPositionX)
            speechBubbleHolder.anchoredPosition = new Vector2((newAnchorPosX > minAnchorPosX) ? newAnchorPosX : minAnchorPosX, sbhOffsetPositionY);
        else
            speechBubbleHolder.anchoredPosition = bubbleHolderLeftPos;
    }

    // Adjusts the speech bubble's position to the left/right if applicable - with respect to both the left/right screen bound
    private void AdjustSpeechBubbleCheck(RectTransform dailogueBubble, RectTransform speechBubbleHolder, float maxScreenPosX)
    {
        if (!dailogueBubble.gameObject.activeInHierarchy) return;

        float maxPosX = maxScreenPosX - (speechBubbleHolder.rect.width / 2);
        float minPosX = -maxPosX;
        float dbAnchorPosX = dailogueBubble.anchoredPosition.x;

        float maxAnchorPosX = (speechBubbleHolder.rect.width / 2) - sbhOffsetPositionX;
        float minAnchorPosX = -maxAnchorPosX;

        if (dbAnchorPosX < minPosX)
        {
            float newAnchorPosX = minPosX - dbAnchorPosX;
            speechBubbleHolder.anchoredPosition = new Vector2((newAnchorPosX < maxAnchorPosX) ? newAnchorPosX : maxAnchorPosX, sbhOffsetPositionY);
        }
        else if (dbAnchorPosX > maxPosX)
        {
            float newAnchorPosX = maxPosX - dbAnchorPosX;
            speechBubbleHolder.anchoredPosition = new Vector2((newAnchorPosX > minAnchorPosX) ? newAnchorPosX : minAnchorPosX, sbhOffsetPositionY);
        }
        else
            speechBubbleHolder.anchoredPosition = bubbleHolderMidPos;
    }

    // Sets the dialogue bubble's postion to the dialogue check's screen position - with respect to the upper screen bound
    private void SetDialogueBubblePosition(RectTransform dailogueBubble, RectTransform speechBubbleHolder, GameObject dialogueCheck, ref Vector2 dcAnchorPos, float maxScreenPosY)
    {
        if (!dailogueBubble.gameObject.activeInHierarchy) return;

        float maxPosY = maxScreenPosY - speechBubbleHolder.rect.height - sbhOffsetPositionY;
        Vector2 dcScreenPoint = dialogueCamera.WorldToScreenPoint(dialogueCheck.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(characterDialogueRT, dcScreenPoint, null, out dcAnchorPos);

        if (dcAnchorPos.y > maxPosY)
            dailogueBubble.anchoredPosition = new Vector2(dcAnchorPos.x, dcAnchorPos.y - (dcAnchorPos.y - maxPosY));
        else
            dailogueBubble.anchoredPosition = dcAnchorPos;
    }

    // Sets the alert bubble's position to the player's dialogue check screen position
    private void SetAlertBubblePosition()
    {
        if (!alertBubble.activeInHierarchy) return;

        Vector2 dcScreenPoint = dialogueCamera.WorldToScreenPoint(playerDialogueCheck.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(characterDialogueRT, dcScreenPoint, null, out pdcAnchorPos);

        if (alertBubbleRT.anchoredPosition != pdcAnchorPos) alertBubbleRT.anchoredPosition = pdcAnchorPos;
    }

    // Checks for the input that continues or speeds up the character dialogue
    private void ContinueInputCheck()
    {
        if (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.KeypadEnter)) return;

        if (!continueButton.activeInHierarchy && typingSpeed > originalTypingSpeed / 2)
            typingSpeed /= 2;

        else if (continueButton.activeInHierarchy)
            NextSentenceCheck();
    }

    // Checks for the input that selects a dialogue option
    private void SelectInputCheck()
    {
        if (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.KeypadEnter)) return;

        if (isInteractingWithArtifact)
            SelectedDialogueCheckForArtifact();

        else if (isInteractingWithNPC)
            SelectedDialogueCheckForNPC();
    }

    // Checks for the input that moves the dialogue arrow
    private void ArrowInputCheck()
    {
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            // Moves to the next ACTIVE dialogue option
            for (int i = dialogueOptionsIndex + 1; i < doGameObjects.Count; i++)
            {
                if (!doGameObjects[i].activeInHierarchy) continue;

                dialogueOptionsIndex = i;
                SetDialoguArrowPosition(i);
                break;
            }
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            // Moves to the next ACTIVE dialogue option
            for (int i = dialogueOptionsIndex - 1; i >= 0; i--)
            {
                if (!doGameObjects[i].activeInHierarchy) continue;

                dialogueOptionsIndex = i;
                SetDialoguArrowPosition(i);
                break;
            }
        }
    }

    // Starts the coroutine that checks for the character dialogue input
    private void StartDialogueInputCoroutine()
    {
        if (dialogueInputCoroutine != null) StopCoroutine(dialogueInputCoroutine);

        dialogueInputCoroutine = CharacterDialogueInputCheck();
        StartCoroutine(dialogueInputCoroutine);
    }

    // Starts the coroutine that checks for the dialogue options input
    private void StartDialogueOptionsInputCoroutine()
    {
        if (dialogueOptionsInputCoroutine != null) StopCoroutine(dialogueOptionsInputCoroutine);

        dialogueOptionsInputCoroutine = DialogueOptionsInputCheck();
        StartCoroutine(dialogueOptionsInputCoroutine);
    }

    // Starts the coroutine that plays the dialogue arrow animation
    private void PlayDialogueArrowAnim(Vector2 endPosition)
    {
        if (dialogueArrowCoroutine != null) StopCoroutine(dialogueArrowCoroutine);

        dialogueArrowCoroutine = LerpDialogueArrow(endPosition);
        StartCoroutine(dialogueArrowCoroutine);
    }

    // Stops the coroutine that plays the dialogue arrow animation - also resets its position
    private void StopDialogueArrowAnim()
    {
        if (dialogueArrowCoroutine != null) StopCoroutine(dialogueArrowCoroutine);
        dialogueArrowRT.anchoredPosition = dialogueArrowOrigPos;
    }

    // Starts the dialogue after a delay
    private IEnumerator StartDialogueDelay()
    {
        audioManagerScript.CrossFadeInDialogueMusic();
        playerScript.SetPlayerBoolsFalse();
        cameraScript.LerpToDialogueView();
        headsUpDisplayScript.TurnOffHUD();
        blackBarsScript.MoveBarsIn();
 
        SetSpeechBubblePivots();
        SetInitialDialogue();
        inDialogue = true;

        yield return new WaitForSeconds(0.5f);
        StartDialogue();
    }

    // Ends the dialogue after a delay
    private IEnumerator EndDialogueDelay()
    {
        if (dialogueInputCoroutine != null) 
            StopCoroutine(dialogueInputCoroutine);

        dialogueOptionsIndex = 0;
        sentenceIndex = 0;

        nPCFidgetScript.HasPlayedInitialFidget = false;
        playerFidgetScript.HasPlayedInitialFidget = false;

        audioManagerScript.CrossFadeOutDialogueMusic();
        headsUpDisplayScript.TurnOnHUD();
        cameraScript.LerpToPuzzleView();
        blackBarsScript.MoveBarsOut();
        nPCScript.ResetRotation();

        playerDialogueBubble.SetActive(false);
        nPCDialogueBubble.SetActive(false);
        EmptyTextComponents();

        hasSelectedDialogueOption = false;
        isInteractingWithArtifact = false;
        isInteractingWithNPC = false;

        yield return new WaitForSeconds(0.4f);
        playerScript.AlertBubbleCheck();

        yield return new WaitForSeconds(0.1f);
        playerScript.SetPlayerBoolsTrue();
        inDialogue = false;
    }

    // Opens the dialogue options after a delay (if applicable)
    private IEnumerator OpenDialogueOptionsDelay()
    {
        bool isActive = playerDialogueBubble.activeInHierarchy;
        float duration = isActive ? 0.05f : 0f; // 0.05f = 1/4th of anim length
        if (isActive) playerBubbleAnim.SetTrigger("NextSentence");

        yield return new WaitForSeconds(duration);
        if (!isActive) playerDialogueBubble.SetActive(true);
        playerDialogueText.gameObject.SetActive(false);
        nPCDialogueBubble.SetActive(false);
        inDialogueOptions = true;

        StartCoroutine(SetDialogueArrowActiveDelay());
        SetPlayerBubbleColor(playerBubbleColor);
        EmptyTextComponents();

        playerSpeechBubbleVLG.childAlignment = TextAnchor.MiddleLeft;
        playerSpeechBubbleVLG.padding.left = 40;

        audioManagerScript.PlayPlayerDialogueBubbleSFX();
        playerFidgetScript.Fidget();
        ShowDialgoueOptionsCheck(); // This method MUST be called last!
    }

    // Types out the dailgoue for the appropriate character
    private IEnumerator TypeCharacterDialogue()
    {
        string textHexColor = isPlayerSpeaking ? playerTextColor.ReturnHexColor() : nPCTextColor.ReturnHexColor();
        TextMeshProUGUI dialogueText = isPlayerSpeaking ? playerDialogueText : nPCDialogueText;
        GameObject currentDialogueBubble = isPlayerSpeaking ? playerDialogueBubble : nPCDialogueBubble;
        GameObject previousDialogueBubble = isPlayerSpeaking ? nPCDialogueBubble : playerDialogueBubble;
        Animator animator = isPlayerSpeaking ? playerBubbleAnim : nPCBubbleAnim;

        if (!currentDialogueBubble.activeInHierarchy)
        {
            previousDialogueBubble.SetActive(false);
            currentDialogueBubble.SetActive(true);
        }
        else
            animator.SetTrigger("NextSentence");

        yield return new WaitForSeconds(0.05f); // 0.05f = 1/4th of anim length
        SetPlayerBubbleColor(playerBubbleColor);
        SetNPCBubbleColor(nPCBubbleColor);
        EmptyTextComponents();
        dialogueText.text = nextSentence;

        foreach (char letter in nextSentence.ToCharArray())
        {
            string textToRepalce = textToColor.Append(letter).ToString();
            dialogueText.text = nextSentence.Replace(textToRepalce, $"<color=#{textHexColor}>{textToColor}</color>");
            audioManagerScript.PlayCharNoiseSFX();
            yield return new WaitForSeconds(typingSpeed);
        }

        typingSpeed = originalTypingSpeed;
        continueButton.SetActive(true);
    }

    // Checks for the character dialogue input
    private IEnumerator CharacterDialogueInputCheck()
    {
        yield return new WaitForSeconds(0.01f);

        while (inDialogue && !inDialogueOptions && !blackOverlayScript.IsChangingScenes)
        {
            if (Time.deltaTime > 0) ContinueInputCheck();
            yield return null;
        }
        //Debug.Log("Input check for CHARACTER DIALOGUE has finished!");
    }

    // Checks for the dialogue options input
    private IEnumerator DialogueOptionsInputCheck()
    {
        while (dialogueArrow.activeInHierarchy && !blackOverlayScript.IsChangingScenes)
        {
            if (Time.deltaTime > 0)
            {
                SelectInputCheck();
                ArrowInputCheck();               
            }
            yield return null;
        }
        //Debug.Log("Input check for DIALOGUE OPTIONS has finished!");
    }

    // Lerps the position of the dialogue arrow to another over a duration (loops)
    private IEnumerator LerpDialogueArrow(Vector2 endPosition)
    {
        /*** For Debugging Purposes ONLY ***/
        //dialogueArrowDestination = new Vector2(dialogueArrowOrigPos.x - animDistance, dialogueArrowOrigPos.y);

        Vector2 startPosition = (endPosition == dialogueArrowDestination) ? dialogueArrowOrigPos : dialogueArrowDestination;
        float halfDuration = animDuration / 2f;
        float time = 0;

        while (time < halfDuration)
        {
            dialogueArrowRT.anchoredPosition = Vector2.Lerp(startPosition, endPosition, time / halfDuration);
            time += Time.deltaTime;
            yield return null;
        }

        dialogueArrowRT.anchoredPosition = endPosition;
        Vector3 nextEndPosition = (endPosition == dialogueArrowDestination) ? dialogueArrowOrigPos : dialogueArrowDestination;
        PlayDialogueArrowAnim(nextEndPosition);
    }

    // Sets the dialogue arrow active after a delay
    private IEnumerator SetDialogueArrowActiveDelay()
    {
        yield return new WaitForSeconds(0.25f);

        SetDialoguArrowPosition(dialogueOptionsIndex);
        PlayDialogueArrowAnim(dialogueArrowDestination);

        dialogueOptionButtons.SetActive(true);
        dialogueArrow.SetActive(true);

        StartDialogueOptionsInputCoroutine();
    }

    // Sets the appropriate vectors
    private void SetVectors()
    {
        // Note: The speech bubble holders for the player and npc are identical
        sbhOffsetPositionY = playerSBH.anchoredPosition.y;
        sbhOffsetPositionX = playerSBH.rect.width / 2f;

        bubbleHolderMidPos = new Vector2(0, sbhOffsetPositionY);
        bubbleHolderRightPos = new Vector2(sbhOffsetPositionX, sbhOffsetPositionY);
        bubbleHolderLeftPos = new Vector2(-sbhOffsetPositionX, sbhOffsetPositionY);

        middlePivot = new Vector2(0.5f, 0);
        rightPivot = new Vector2(1, 0);
        leftPivot = new Vector2(0, 0);

        dialogueArrowOrigPos = dialogueArrowRT.anchoredPosition;
        dialogueArrowDestination = new Vector2(dialogueArrowOrigPos.x - animDistance, dialogueArrowOrigPos.y);
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        nPCScript = FindObjectOfType<NonPlayerCharacter>();

        blackOverlayScript = FindObjectOfType<BlackOverlay>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        cameraScript = FindObjectOfType<CameraController>();
        blackBarsScript = FindObjectOfType<BlackBars>();
        artifactScript = FindObjectOfType<Artifact>();
        headsUpDisplayScript = FindObjectOfType<HUD>();

        playerFidgetScript = playerScript.GetComponentInChildren<FidgetController>();
        nPCFidgetScript = nPCScript.GetComponentInChildren<FidgetController>();
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            if (child.name.Contains("PDB_Option"))
            {          
                doTextComponenets.Add(child.GetComponent<TextMeshProUGUI>());
                doGameObjects.Add(child.gameObject);
            }

            switch (child.name)
            {
                case "DialogueCamera":
                    dialogueCamera = child.GetComponent<Camera>();
                    break;
                case "CharacterDialogue":
                    characterDialogueRT = child.GetComponent<RectTransform>();
                    canvasRT = child.parent.GetComponent<RectTransform>();
                    break;
                case "ContinueButton":
                    continueButton = child.gameObject;
                    break;
                case "DialogueOptionButtons":
                    dialogueOptionButtons = child.gameObject;
                    break;
                /** Dialogue arrow variables START here **/
                case "DialogueArrow":
                    dialogueArrowRT = child.GetComponent<RectTransform>();
                    dialogueArrow = child.gameObject;
                    break;
                case "DA_SmallArrow":
                    smallArrowSprite = child.GetComponent<Image>();
                    break;
                case "DA_BigArrow":
                    bigArrowSprite = child.GetComponent<Image>();
                    break;
                /** Dialogue arrow variables END here **/
                /** Player variables START here **/
                case "PlayerDialogueBubble":
                    playerDB = child.GetComponent<RectTransform>();
                    playerBubbleAnim = child.GetComponent<Animator>();
                    playerDialogueBubble = child.gameObject;
                    break;
                case "PDB_SpeechBubbleHolder":
                    playerSBH = child.GetComponent<RectTransform>();
                    break;
                case "PDB_SpeechBubble":
                    playerSpeechBubbleVLG = child.GetComponent<VerticalLayoutGroup>();
                    playerBubbleSprite = child.GetComponent<Image>();
                    break;
                case "PDB_Tail":
                    playerTailSprite = child.GetComponent<Image>();
                    break;
                case "PDB_Text":
                    playerDialogueText = child.GetComponent<TextMeshProUGUI>();
                    break;
                /** Player variables END here **/
                /** NPC variables START here **/
                case "NPCDialogueBubble":
                    nPCDB = child.GetComponent<RectTransform>();
                    nPCBubbleAnim = child.GetComponent<Animator>();
                    nPCDialogueBubble = child.gameObject;
                    break;
                case "NDB_SpeechBubbleHolder":
                    nPCSBH = child.GetComponent<RectTransform>();
                    break;
                case "NDB_SpeechBubble":
                    nPCBubbleSprite = child.GetComponent<Image>();
                    break;
                case "NDB_Tail":
                    nPCTailSprite = child.GetComponent<Image>();
                    break;
                case "NDB_Text":
                    nPCDialogueText = child.GetComponent<TextMeshProUGUI>();
                    break;
                /** NPC variables END here **/
                /** Alert bubble variables START here **/
                case "AlertBubble":
                    alertBubbleRT = child.GetComponent<RectTransform>();
                    alertBubble = child.gameObject;       
                    playerScript.AlertBubble = alertBubble;
                    break;
                case "AB_SpeechBubble":
                    alertBubbleSprite = child.GetComponent<Image>();
                    break;
                case "AB_Icon":
                    alertIconSprite = child.GetComponent<Image>();
                    break;
                /** Alert bubble variables END here **/
                default:
                    break;
            }

            if (child.name == "ArtifactButtons" || child.name == "DialogueOptionButtons") continue;
            if (child.name == "TorchMeter" || child.name == "NotificationBubbles") continue;

            SetVariables(child);
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {     
        foreach (Transform child in playerScript.transform)
        {
            if (child.name != "DialogueCheck") continue;
            playerDialogueCheck = child.gameObject;
            break;
        }

        foreach (Transform child in nPCScript.transform)
        {
            if (child.name != "DialogueCheck") continue;
            nPCDialogueCheck = child.gameObject;
            break;
        }
    
        SetVariables(headsUpDisplayScript.transform);
        SetVariables(cameraScript.transform);
        SetVectors();
        originalTypingSpeed = typingSpeed;
    }

}
