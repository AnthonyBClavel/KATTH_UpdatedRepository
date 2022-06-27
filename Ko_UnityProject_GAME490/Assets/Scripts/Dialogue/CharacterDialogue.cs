using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;

public class CharacterDialogue : MonoBehaviour
{
    [Header("Dialogue Arrow Animation Variables")]
    [SerializeField]
    [Range(0.1f, 2.0f)]
    private float animDuration = 1f; // Original Value = 1f
    [SerializeField]
    [Range(1f, 90f)]
    private float animDistance = 15f; // Original Value = 15f

    [Header("Character Dialogue Variables")]
    [SerializeField] [Range(0.005f, 0.1f)]
    private float typingSpeed = 0.03f; // Original Value = 0.03f
    private float originalTypingSpeed; // Original Value = 0.03f
    private float sbhOffsetPositionY; // sbh = speech bubble holder
    private float sbhOffsetPositionX; // sbh = speech bubble holder

    private int sentenceIndex;
    private int artifactIndex;
    private int dialogueOptionsIndex;

    private string nextSentence;
    private string currentlyTalking;

    private bool isPlayerSpeaking = false;
    private bool isInteractingWithNPC = false;
    private bool isInteractingWithArtifact = false;
    private bool inDialogue = false;
    private bool inDialogueOptions = false;
    private bool hasSelectedDialogueOption = false;

    private GameObject playerDialogueBubble;
    private GameObject nPCDialogueBubble;
    private GameObject alertBubble;
    private GameObject continueButton;
    private GameObject playerDialogueCheck;
    private GameObject nPCDialogueCheck;
    private GameObject dialogueArrow;
    private GameObject dialogueOptionButtons;

    private Image smallArrowSprite;
    private Image bigArrowSprite;
    private Image playerBubbleSprite;
    private Image playerTailSprite;
    private Image nPCBubbleSprite;
    private Image nPCTailSprite;
    private Image alertBubbleSprite;
    private Image alertIconSprite;

    private RectTransform playerDB; // DB = dialogue bubble
    private RectTransform nPCDB; // DB = dialogue bubble
    private RectTransform playerSBH; // SBH = speech bubble holder
    private RectTransform nPCSBH; // SBH = speech bubble holder
    private RectTransform alertBubbleRT;
    private RectTransform dialogueArrowRT;
    private RectTransform canvasRT;
    private RectTransform characterDialogueRT;

    private VerticalLayoutGroup playerSpeechBubbleVLG;
    private AudioSource charNoiseSFX;
    private Animator playerBubbleAnim;
    private Animator nPCBubbleAnim;
    private Camera dialogueCamera;

    private Vector2 bubbleHolderMidPos;
    private Vector2 bubbleHolderRightPos;
    private Vector2 bubbleHolderLeftPos;

    private Vector2 middlePivot;
    private Vector2 rightPivot;
    private Vector2 leftPivot;
    private Vector2 dialogueArrowOrigPos;
    private Vector2 dialogueArrowDestination;

    private Vector2 pdcAnchorPos; // pdc = player dialogue check
    private Vector2 npcdcAnchorPos; // npcdc = npc dialogue check

    public TextAsset[] emptyChestDialogue;
    private TextAsset[] artifactDialogue;
    private TextAsset[] nPCDialogue;
    private string[] dialogueSentences;
    private string[] dialogueOptions;
    private List<GameObject> doGameObjects = new List<GameObject>();
    private List<TextMeshProUGUI> doTextComponenets = new List<TextMeshProUGUI>();

    [Header("Text Variables")]
    private StringBuilder textToColor = new StringBuilder();
    private TextMeshProUGUI nPCDialogueText;
    private TextMeshProUGUI playerDialogueText;
    private Color32 nPCTextColor;
    private Color32 nPCBubbleColor;
    private Color32 playerTextColor = new Color32(128, 160, 198, 255);
    private Color32 playerBubbleColor = Color.white;
    private Color32 unselectedTextColor = Color.gray;

    [Header("Scriptable Objects")]
    private Artifact_SO artifact;
    private NonPlayerCharacter_SO nonPlayerCharacter;

    private IEnumerator dialogueInputCoroutine;
    private IEnumerator dialogueOptionsInputCoroutine;
    private IEnumerator dialogueArrowCoroutine;
    private IEnumerator dialogueOptionsCoroutine;

    private Artifact artifactScript;
    private NonPlayerCharacter nPCScript;
    private FidgetController nPCFidgetScript;
    private FidgetController playerFidgetScript;
    private TileMovementController playerScript;
    private CameraController cameraScript;
    private AudioManager audioManagerScript;
    private BlackBars blackBarsScript;
    private GameHUD gameHUDScript;
    private TransitionFade transitionFadeScript;

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
    private void SetCharacterDialogue(TextAsset characterDialogue) => dialogueSentences = characterDialogue.ReturnSentences();

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
            FadeOutDialogueMusic();
            //Debug.Log("Dialogue has ended");
        }
        else
        {
            OpenDialogueOptions();       
            //Debug.Log("Opened dialogue options");
        }

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
        else
            EndDialogue();
    }

    // Checks to set the dialogue options active/inactive
    private void ShowDialgoueOptionsCheck()
    {
        for (int i = 0; i < dialogueOptions.Length; i++)
        {        
            doTextComponenets[i].text = inDialogueOptions ? dialogueOptions[i] : string.Empty;
            doGameObjects[i].SetActive(inDialogueOptions ? true : false);
            doTextComponenets[i].color = unselectedTextColor;
        }

        // Note: ONLY shows the "Collect" dialogue option AFTER the player has inspected the artifact
        if (!isInteractingWithArtifact || artifactScript.HasInspectedArtifact) return;

        for (int i = 0; i < dialogueOptions.Length; i++)
        {
            if (!dialogueOptions[i].Contains("Collect")) continue;

            doTextComponenets[i].text = string.Empty;
            doGameObjects[i].SetActive(false);
            break;
        }
    }

    // "Opens" the dialogue options
    public void OpenDialogueOptions()
    {
        if (dialogueOptionsCoroutine != null)
            StopCoroutine(dialogueOptionsCoroutine);

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
        ShowDialgoueOptionsCheck(); // This method MUST be called last!
    }

    // Checks to play the selected dialgoue option (NPC)
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

        CloseDialogueOptions(); // This method MUST be called before StartDialogue()
        StartDialogue();
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
            FadeOutDialogueMusic();
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

    // Fades in the dialogue music
    private void FadeInDialogueMusic()
    {
        audioManagerScript.FadeInDialogueMusic();
        audioManagerScript.FadeOutBackgroundMusic();
        audioManagerScript.FadeOutGeneratorLoopCheck();
    }

    // Fades out the dialogue music
    private void FadeOutDialogueMusic()
    {
        audioManagerScript.FadeOutDialogueMusic();
        audioManagerScript.FadeInBackgroundMusic();
        audioManagerScript.FadeInGeneratorLoopCheck();
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

    // Set the position of the dialogue arrow
    private void SetDialoguArrowPosition(int doIndex)
    {
        dialogueArrow.transform.SetParent(doGameObjects[doIndex].transform);
        dialogueArrowRT.anchoredPosition = dialogueArrowOrigPos;
        smallArrowSprite.color = playerTextColor; // Sets arrow color
        audioManagerScript.PlayArrowSelectSFX(); // Plays selected sfx

        // Highlights the current selected dialogue option
        for (int i = 0; i < doTextComponenets.Count; i++)
        {
            Color textColor = i == doIndex ? playerTextColor : unselectedTextColor;
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

        Vector3 playerDirection = playerScript.transform.eulerAngles;
        float maxScreenPosX = (canvasRT.rect.width / 2f) - 66f;
        float maxScreenPosY = (canvasRT.rect.height / 2f) - blackBarsScript.FinalHeight;
        // Note: 66f = 96f (length of bigArrowSprite) - 40f (left padding in VerticaLayouGroup) - 5f (x position in dialogueArrowOrigPos) + 15f (dialogue arrow animDistance)

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

        // Make sure to check if the continue button cant be pressed when the player pauses the game and returns to the main menu!
        if (continueButton.activeInHierarchy)
            NextSentenceCheck();

        else if (!continueButton.activeInHierarchy && typingSpeed > originalTypingSpeed / 2)
            typingSpeed /= 2;
    }

    // Checks for the input that selects a dialogue option
    private void SelectInputCheck()
    {
        if (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.KeypadEnter)) return;

        if (isInteractingWithNPC)
            SelectedDialogueCheckForNPC();

        else if (isInteractingWithArtifact)
            SelectedDialogueCheckForArtifact();
    }

    // Checks for the input that moves the dialogue arrow
    private void ArrowInputCheck()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            // Finds the next ACTIVE dialogue option
            for (int i = dialogueOptionsIndex - 1; i >= 0; i--)
            {
                if (!doGameObjects[i].activeInHierarchy) continue;

                dialogueOptionsIndex = i;
                SetDialoguArrowPosition(i);
                break;
            }
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            // Finds the next ACTIVE dialogue option
            for (int i = dialogueOptionsIndex + 1; i < doGameObjects.Count; i++)
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
        playerScript.SetPlayerBoolsFalse();
        cameraScript.LerpToDialogueView();
        blackBarsScript.MoveBarsIn();
        gameHUDScript.TurnOffHUD();

        SetInitialDialogue();
        FadeInDialogueMusic();
        SetSpeechBubblePivots();
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

        cameraScript.LerpToPuzzleView();
        nPCScript.ResetRotation();
        blackBarsScript.MoveBarsOut();
        gameHUDScript.TurnOnHUD();

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
            charNoiseSFX.Play();
            yield return new WaitForSeconds(typingSpeed);
        }

        typingSpeed = originalTypingSpeed;
        continueButton.SetActive(true);
    }

    // Checks for the character dialogue input
    private IEnumerator CharacterDialogueInputCheck()
    {
        yield return new WaitForSeconds(0.01f);

        while (inDialogue && !inDialogueOptions && !transitionFadeScript.IsChangingScenes)
        {
            if (Time.deltaTime > 0) ContinueInputCheck();
            yield return null;
        }
        //Debug.Log("Input check for CHARACTER DIALOGUE has finished!");
    }

    // Checks for the dialogue options input
    private IEnumerator DialogueOptionsInputCheck()
    {
        while (dialogueArrow.activeInHierarchy && !transitionFadeScript.IsChangingScenes)
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
        cameraScript = FindObjectOfType<CameraController>();
        artifactScript = FindObjectOfType<Artifact>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        blackBarsScript = FindObjectOfType<BlackBars>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        transitionFadeScript = FindObjectOfType<TransitionFade>();
        nPCFidgetScript = nPCScript.GetComponentInChildren<FidgetController>();
        playerFidgetScript = playerScript.GetComponentInChildren<FidgetController>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets them by looking at the names of children
        for (int i = 0; i < gameHUDScript.transform.parent.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.parent.GetChild(i).gameObject;

            if (child.name == "CharacterDialogue")
            {
                characterDialogueRT = child.GetComponent<RectTransform>();
                canvasRT = characterDialogueRT.parent.GetComponent<RectTransform>();
            }
        }

        for (int i = 0; i < characterDialogueRT.transform.childCount; i++)
        {
            GameObject child = characterDialogueRT.transform.GetChild(i).gameObject;
           
            if (child.name == "PlayerDialogueBubble")
            {
                playerDialogueBubble = child;
                playerDB = playerDialogueBubble.GetComponent<RectTransform>();
                playerBubbleAnim = playerDialogueBubble.GetComponent<Animator>();

                for (int j = 0; j < playerDialogueBubble.transform.childCount; j++)
                {
                    GameObject child02 = playerDialogueBubble.transform.GetChild(j).gameObject;

                    if (child02.name == "SpeechBubbleHolder")
                    {
                        playerSBH = child02.GetComponent<RectTransform>();

                        GameObject playerSpeechBubble = child02.transform.GetChild(0).gameObject;
                        playerSpeechBubbleVLG = playerSpeechBubble.GetComponent<VerticalLayoutGroup>();
                        playerBubbleSprite = playerSpeechBubble.GetComponent<Image>();                        

                        for (int k = 0; k < playerSpeechBubble.transform.childCount; k++)
                        {
                            GameObject child03 = playerSpeechBubble.transform.GetChild(k).gameObject;

                            if (child03.name.Contains("Option"))
                            {
                                doGameObjects.Add(child03);
                                doTextComponenets.Add(child03.GetComponent<TextMeshProUGUI>());
                            }
                            if (child03.name == "Text")
                                playerDialogueText = child03.GetComponent<TextMeshProUGUI>();
                        }

                        dialogueArrow = doGameObjects[0].transform.GetChild(0).gameObject;
                        dialogueArrowRT = dialogueArrow.GetComponent<RectTransform>();                       
                    }

                    if (child02.name == "Tail")
                        playerTailSprite = child02.GetComponent<Image>();
                }
            }

            if (child.name == "NPCDialogueBubble")
            {
                nPCDialogueBubble = child;
                nPCDB = nPCDialogueBubble.GetComponent<RectTransform>();
                nPCBubbleAnim = nPCDialogueBubble.GetComponentInChildren<Animator>();

                for (int j = 0; j < nPCDialogueBubble.transform.childCount; j++)
                {
                    GameObject child02 = nPCDialogueBubble.transform.GetChild(j).gameObject;

                    if (child02.name == "SpeechBubbleHolder")
                    {
                        nPCSBH = child02.GetComponent<RectTransform>();

                        GameObject nPCSpeechBubble = child02.transform.GetChild(0).gameObject;
                        nPCBubbleSprite = nPCSpeechBubble.GetComponent<Image>();

                        for (int k = 0; k < nPCSpeechBubble.transform.childCount; k++)
                        {
                            GameObject child03 = nPCSpeechBubble.transform.GetChild(k).gameObject;

                            if (child03.name == "Text")
                                nPCDialogueText = child03.GetComponent<TextMeshProUGUI>();
                        }
                    }
                    if (child02.name == "Tail")
                        nPCTailSprite = child02.GetComponent<Image>();
                }
            }
            
            if (child.name == "AlertBubble")
            {
                alertBubble = child;
                alertBubbleRT = alertBubble.GetComponent<RectTransform>();
                playerScript.AlertBubble = alertBubble;

                for (int j = 0; j < alertBubble.transform.childCount; j++)
                {
                    GameObject child02 = alertBubble.transform.GetChild(j).gameObject;

                    if (child02.name == "SpeechBubble")
                        alertBubbleSprite = child02.GetComponent<Image>();
                    if (child02.name == "Icon")
                        alertIconSprite = child02.GetComponent<Image>();
                }
            }
        }

        for (int i = 0; i < gameHUDScript.transform.parent.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.parent.GetChild(i).gameObject;

            if (child.name == "KeybindButtons")
            {
                GameObject keybindButtons = child;

                for (int j = 0; j < keybindButtons.transform.childCount; j++)
                {
                    GameObject child02 = keybindButtons.transform.GetChild(j).gameObject;

                    if (child02.name == "ContinueButton")
                        continueButton = child02;
                    if (child02.name == "DialogueOptionButtons")
                        dialogueOptionButtons = child02;
                }
            }
        }

        for (int i = 0; i < cameraScript.transform.childCount; i++)
        {
            GameObject child = cameraScript.transform.GetChild(i).gameObject;

            if (child.name == "DialogueCamera")
                dialogueCamera = child.GetComponent<Camera>();
        }

        for (int i = 0; i < playerScript.transform.childCount; i++)
        {
            GameObject child = playerScript.transform.GetChild(i).gameObject;

            if (child.name == "DialogueCheck")
                playerDialogueCheck = child;
        }

        for (int i = 0; i < nPCScript.transform.childCount; i++)
        {
            GameObject child = nPCScript.transform.GetChild(i).gameObject;

            if (child.name == "DialogueCheck")
                nPCDialogueCheck = child;                      
        }

        for (int i = 0; i < dialogueArrow.transform.childCount; i++)
        {
            GameObject child = dialogueArrow.transform.GetChild(i).gameObject;

            if (child.name == "SmallArrow")
                smallArrowSprite = child.GetComponent<Image>();
            if (child.name == "BigArrow")
                bigArrowSprite = child.GetComponent<Image>();
        }

        charNoiseSFX = audioManagerScript.charNoiseAS;
        originalTypingSpeed = typingSpeed;
        SetVectors();
    }

}
