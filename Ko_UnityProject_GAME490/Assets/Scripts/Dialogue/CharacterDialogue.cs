using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;

public class CharacterDialogue : MonoBehaviour
{
    [Header("Character Dialogue Variables")]
    [SerializeField] [Range(0.005f, 0.1f)]
    private float typingSpeed = 0.03f; // Original Value = 0.03f
    private float originalTypingSpeed; // Original Value = 0.03f
    private float sbhOffsetPositionY; // sbh = speech bubble holder
    private float sbhOffsetPositionX; // sbh = speech bubble holder

    [Header("Dialogue Arrow Animation Variables")]
    [SerializeField] [Range(0.1f, 2.0f)]
    private float animDuration = 1f; // Original Value = 1f
    [SerializeField] [Range(1f, 90f)]
    private float animDistance = 15f; // Original Value = 15f

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

    private Vector2 bubbleHolderOrigPos;
    private Vector2 bubbleHolderRightPos;
    private Vector2 bubbleHolderLeftPos;

    private Vector2 originalPivot;
    private Vector2 rightPivot;
    private Vector2 leftPivot;
    private Vector2 dialogueArrowOrigPos;
    private Vector2 dialogueArrowDestination;

    private Vector2 pdcAnchorPos; // pdc = player dialogue check
    private Vector2 npcdcAnchorPos; // npcdc = npc dialogue check

    [Header("Dialogue Variables")]
    private List<GameObject> doGameObjects = new List<GameObject>();
    private List<TextMeshProUGUI> doTextComponenets = new List<TextMeshProUGUI>();
    private TextAsset[] nPCDialogue;
    private TextAsset[] artifactDialogue;
    private string[] dialogueSentences;
    private string[] dialogueOptions;

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

    public float TypingSpeed
    {
        get { return typingSpeed; }
        set { typingSpeed = value; }
    }

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
            artifactDialogue = artifact.artifactDialogue;          
        }

        isInteractingWithArtifact = true;
        SetDialogueOptions(artifact.dialogueOptions);
        StartCoroutine(StartDialogueDelay());
    }

    // Sets the character dialogue
    private void SetCharacterDialogue(TextAsset characterDialogue) => dialogueSentences = characterDialogue.ReturnSentences();

    // Sets the dialogue options
    private void SetDialogueOptions(TextAsset dialogeOptionsFile)
    {
        if (dialogueOptions != dialogeOptionsFile.ReturnSentences())
            dialogueOptions = dialogeOptionsFile.ReturnSentences();

        if (isInteractingWithNPC)
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
        if (dialogueOptionsIndex == dialogueOptions.Length - 1)
        {        
            StartCoroutine(EndDialogueDelay());
            FadeOutDialogueMusic();
            //Debug.Log("Dialogue Has Ended");
        }
        else
        {
            OpenDialogueOptions();       
            //Debug.Log("Opened Dialogue Options");
        }

        currentlyTalking = string.Empty;
    }

    // Determines, sets, and plays the next dialogue sentence (theString = the next line/string within the text file)
    private void NextDialogueSentence(string theString)
    {
        string characterName = theString.Remove(theString.IndexOf(':') + 1);
        nextSentence = theString.Replace(characterName, "").Trim();
        isPlayerSpeaking = characterName.Contains("PLAYER");
        PlayFidgetAndBubblePopSFX(characterName);

        StartCoroutine(TypeCharacterDialogue());
    }

    // Sets the dialogue for initially interacting with an npc/artifact
    private void SetInitialDialogue()
    {
        if (isInteractingWithNPC)
        {
            nPCScript.SetRotationNPC();
            nPCDialogueText.color = nPCTextColor;
                
            if (!nPCScript.HasPlayedInitialDialogue && !isInteractingWithArtifact)
            {
                nPCScript.HasPlayedInitialDialogue = true;
                SetCharacterDialogue(nPCDialogue[0]);
            }              
            else
                SetCharacterDialogue(nPCDialogue[1]);
        }
        else
            SetCharacterDialogue(ReturnRandomArtifactDialogue());
    }

    // Returns a randomly selected text asset for the artifact dialogue
    private TextAsset ReturnRandomArtifactDialogue()
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

    // Checks to play the next dialogue sentence if applicable - ends the dialogue otherwise
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
            if (inDialogueOptions) // Set ACTIVE
            {
                doTextComponenets[i].text = dialogueOptions[i];
                doGameObjects[i].SetActive(true);
            }
            else // Set INACTIVE
            {
                doTextComponenets[i].text = string.Empty;
                doGameObjects[i].SetActive(false);
            }

            doTextComponenets[i].color = unselectedTextColor;
        }

        // Note: ONLY shows the "Collect" dialogue option AFTER the player has inspected the artifact
        if (isInteractingWithArtifact && !artifactScript.HasInspectedArtifact)
        {
            for (int i = 0; i < dialogueOptions.Length; i++)
            {
                if (dialogueOptions[i].Contains("Collect"))
                {
                    doGameObjects[i].SetActive(false);
                    doTextComponenets[i].text = string.Empty;
                    break;
                }
            }        
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
        StopDialogueArrowAnim();
        inDialogueOptions = false;
        sentenceIndex = 0;

        dialogueOptionButtons.SetActive(false);
        dialogueArrow.SetActive(false);

        playerDialogueText.gameObject.SetActive(true);
        playerSpeechBubbleVLG.childAlignment = TextAnchor.MiddleCenter;
        playerSpeechBubbleVLG.padding.left = 20;

        ShowDialgoueOptionsCheck(); // This method MUST be called last!
    }

    // Determines which version of the selected dialogue option to play
    private void DailogueOptionCheckForNPC()
    {
        int original = (dialogueOptionsIndex * 2) + 2;
        int variant = original + 1;

        // Checks which closing version to play
        if (dialogueOptionsIndex == dialogueOptions.Length - 1)
        {
            if (hasSelectedDialogueOption)
                SetCharacterDialogue(nPCDialogue[original]);
            else
                SetCharacterDialogue(nPCDialogue[variant]);
        }
        // Checks to play the original version - if the dialogue option has NOT been played
        else if (nPCScript.DialogueOptionBools[dialogueOptionsIndex] == false)
        {
            SetCharacterDialogue(nPCDialogue[original]);
            nPCScript.DialogueOptionBools[dialogueOptionsIndex] = true;
            hasSelectedDialogueOption = true;
        }
        // Plays the variant version otherwise - if the dialogue option HAS already been played
        else
        {
            SetCharacterDialogue(nPCDialogue[variant]);
            hasSelectedDialogueOption = true;
        }

        CloseDialogueOptions(); // This method MUST be called before StartDialogue()
        StartDialogue();
    }

    // Determines which methods to call based on the selected dialogue option
    private void DialogueOptionCheckForArtifact()
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

            playerScript.ChangeAnimationState("Interacting");
            artifactScript.CloseChest();
            StartCoroutine(EndDialogueDelay());
            FadeOutDialogueMusic();
        }

        playerDialogueBubble.SetActive(false);
        CloseDialogueOptions();
    }

    // Set the position of the dialogue arrow
    private void SetDialoguArrowPosition(int doIndex)
    {
        dialogueArrow.transform.SetParent(doGameObjects[doIndex].transform);
        dialogueArrowRT.anchoredPosition = dialogueArrowOrigPos;
        audioManagerScript.PlayButtonClick02SFX();

        // Highlights the current selected dialogue option
        for (int i = 0; i < doTextComponenets.Count; i++)
        {
            if (i == doIndex)
            {
                doTextComponenets[i].color = playerTextColor;
                smallArrowSprite.color = playerTextColor;
            }
            else
                doTextComponenets[i].color = unselectedTextColor;
        }
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

    // Sets the color for the player dialogue bubble
    private void SetPlayerBubbleColor(Color32 color)
    {
        if (playerDialogueText.color == color)
            return;

        playerDialogueText.color = color;
        playerBubbleSprite.color = color;
        playerTailSprite.color = color;
        bigArrowSprite.color = color;
        //Debug.Log("Updated Bubble Color For Player");
    }

    // Sets the color for the npc dialogue bubble
    private void SetNPCBubbleColor(Color32 color)
    {
        if (nPCDialogueText.color == color)
            return;

        nPCDialogueText.color = color;
        nPCBubbleSprite.color = color;
        nPCTailSprite.color = color;
        //Debug.Log("Updated Bubble Color For NPC");
    }

    // Sets the color for the alert bubble
    public void SetAlertBubbleColor(Color32? bubbleColor = null, Color32? iconColor = null)
    {
        alertBubbleSprite.color = bubbleColor ?? playerBubbleColor;
        alertIconSprite.color = iconColor ?? playerTextColor;
    }

    // Checks to play the character's fidget animation and bubble pop sfx
    private void PlayFidgetAndBubblePopSFX(string characterName)
    {
        if (currentlyTalking != characterName)
        {
            if (isPlayerSpeaking)
            {
                playerFidgetScript.Fidget();
                audioManagerScript.PlayDialoguePopUpSFX01();
            }
            else
            {
                nPCFidgetScript.Fidget();
                audioManagerScript.PlayDialoguePopUpSFX02();
            }
            currentlyTalking = characterName;
        }
    }

    // Sets the pivots for the speech bubbles
    private void SetSpeechBubblePivots()
    {
        Vector3 playerDirection = playerScript.transform.eulerAngles;

        switch (playerDirection.y)
        {
            case 0: // Looking north
                playerSBH.pivot = originalPivot;
                nPCSBH.pivot = originalPivot;
                break;
            case 90: // Looking east
                playerSBH.pivot = rightPivot;
                nPCSBH.pivot = leftPivot;
                break;
            case 180: // Looking south
                playerSBH.pivot = originalPivot;
                nPCSBH.pivot = originalPivot;
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

    // Adjusts the dialogue bubbles to stay within the screen bounds if applicable
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

        // Note: sbhOffsetPositionX = half of the speech bubble holder's original width
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

        // Note: sbhOffsetPositionX = half of the speech bubble holder's original width
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
            speechBubbleHolder.anchoredPosition = bubbleHolderOrigPos;
    }

    // Sets the dialogue bubble's postion to the dialogue check's screen position - with respect to the upper screen bound
    private void SetDialogueBubblePosition(RectTransform dailogueBubble, RectTransform speechBubbleHolder, GameObject dialogueCheck, ref Vector2 dcAnchorPos, float maxScreenPosY)
    {
        if (!dailogueBubble.gameObject.activeInHierarchy) return;

        // Note: sbhOffsetPositionY = the speech bubble holder's original y position
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

        if (alertBubbleRT.anchoredPosition != pdcAnchorPos)
            alertBubbleRT.anchoredPosition = pdcAnchorPos;
    }

    // Starts the coroutine that checks for the character dialogue input
    private void StartDialogueInputCoroutine()
    {
        if (dialogueInputCoroutine != null)
            StopCoroutine(dialogueInputCoroutine);

        dialogueInputCoroutine = ContinueDialogueInputCheck();
        StartCoroutine(dialogueInputCoroutine);
    }

    // Starts the coroutine that checks for the dialogue options input
    private void StartDialogueOptionsInputCoroutine()
    {
        if (dialogueOptionsInputCoroutine != null)
            StopCoroutine(dialogueOptionsInputCoroutine);

        dialogueOptionsInputCoroutine = DialogueOptionsInputCheck();
        StartCoroutine(dialogueOptionsInputCoroutine);
    }

    // Starts the dialogue arrow coroutine
    private void PlayDialogueArrowAnim()
    {
        if (dialogueArrowCoroutine != null)
            StopCoroutine(dialogueArrowCoroutine);

        dialogueArrowCoroutine = LerpDialogueArrow();
        StartCoroutine(dialogueArrowCoroutine);
    }

    // Stops the dialogue arrow's coroutine and sets resets its position
    public void StopDialogueArrowAnim()
    {
        if (dialogueArrowCoroutine != null)
            StopCoroutine(dialogueArrowCoroutine);

        dialogueArrowRT.anchoredPosition = dialogueArrowOrigPos;
    }

    // Starts the dialogue after a delay
    private IEnumerator StartDialogueDelay()
    {
        playerScript.SetPlayerBoolsFalse();
        cameraScript.LerpToDialogueView();
        blackBarsScript.MoveBlackBarsIn();
        gameHUDScript.TurnOffHUD();

        SetInitialDialogue();
        FadeInDialogueMusic();
        SetSpeechBubblePivots();
        inDialogue = true;

        yield return new WaitForSeconds(0.5f); // maybe wait for when the camera to finish transforming here 
        StartDialogue();
    }

    // Ends the dialogue after a delay
    private IEnumerator EndDialogueDelay()
    {
        sentenceIndex = 0;
        dialogueOptionsIndex = 0;

        nPCFidgetScript.HasPlayedInitialFidget = false;
        playerFidgetScript.HasPlayedInitialFidget = false;

        cameraScript.LerpToPuzzleView();
        nPCScript.ResetRotationNPC();
        blackBarsScript.MoveBlackBarsOut();
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
        if (playerDialogueBubble.activeInHierarchy)
        {
            playerBubbleAnim.SetTrigger("NextSentence");
            yield return new WaitForSeconds(0.05f);
        }
        else
            playerDialogueBubble.SetActive(true);

        StartCoroutine(SetDialogueArrowActiveDelay());
        SetPlayerBubbleColor(playerBubbleColor);
        inDialogueOptions = true;

        nPCDialogueBubble.SetActive(false);
        playerDialogueText.gameObject.SetActive(false);
        playerSpeechBubbleVLG.childAlignment = TextAnchor.MiddleLeft;
        playerSpeechBubbleVLG.padding.left = 40;
        EmptyTextComponents();

        audioManagerScript.PlayDialoguePopUpSFX01();
        playerFidgetScript.Fidget();
        ShowDialgoueOptionsCheck(); // This method MUST be called last!
    }

    // Types out the dailgoue for the appropriate character
    private IEnumerator TypeCharacterDialogue()
    {
        Animator animator = isPlayerSpeaking ? playerBubbleAnim : nPCBubbleAnim;
        GameObject currentdialogueBubble = isPlayerSpeaking ? playerDialogueBubble : nPCDialogueBubble;
        GameObject previousDialogueBubble = isPlayerSpeaking ? nPCDialogueBubble : playerDialogueBubble;
        TextMeshProUGUI dialogueText = isPlayerSpeaking ? playerDialogueText : nPCDialogueText;
        string textHexColor = isPlayerSpeaking ? playerTextColor.ReturnHexColor() : nPCTextColor.ReturnHexColor();

        if (!currentdialogueBubble.activeInHierarchy)
        {
            currentdialogueBubble.SetActive(true);
            previousDialogueBubble.SetActive(false);
        }
        else
            animator.SetTrigger("NextSentence");

        yield return new WaitForSeconds(0.05f); // use 0.05f or 0.1f
        EmptyTextComponents();    
        SetNPCBubbleColor(nPCBubbleColor);
        SetPlayerBubbleColor(playerBubbleColor);
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

    // Checks for the input that continues or speeds up the character dialogue
    private IEnumerator ContinueDialogueInputCheck()
    {
        yield return new WaitForSeconds(0.01f);

        while (inDialogue && !inDialogueOptions && !transitionFadeScript.IsChangingScenes)
        {
            if (Time.deltaTime > 0)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    // Make sure to check if the continue button cant be pressed when the player pauses the game and returns to the main menu!
                    if (continueButton.activeInHierarchy)
                        NextSentenceCheck();

                    else if (!continueButton.activeInHierarchy && typingSpeed > originalTypingSpeed / 2)
                        typingSpeed /= 2;
                }
            }

            yield return null;
        }
        //Debug.Log("Dialogue Input Check Has Finished!");
    }

    // Checks for the input that selects a dialogue option
    private IEnumerator DialogueOptionsInputCheck()
    {
        while (dialogueArrow.activeInHierarchy && !transitionFadeScript.IsChangingScenes)
        {
            if (Time.deltaTime > 0)
            {
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    // Finds the next ACTIVE dialogue option
                    for (int i = dialogueOptionsIndex - 1; i >= 0; i--)
                    {
                        if (doGameObjects[i].activeInHierarchy)
                        {
                            dialogueOptionsIndex = i;
                            SetDialoguArrowPosition(i);
                            break;
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    // Finds the next ACTIVE dialogue option
                    for (int i = dialogueOptionsIndex + 1; i < doGameObjects.Count; i++)
                    {
                        if (doGameObjects[i].activeInHierarchy)
                        {
                            dialogueOptionsIndex = i;
                            SetDialoguArrowPosition(i);
                            break;
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    if (isInteractingWithNPC)
                        DailogueOptionCheckForNPC();

                    else if (isInteractingWithArtifact)
                        DialogueOptionCheckForArtifact();
                }
            }

            yield return null;
        }
    }

    // Lerps the position of the dialogue arrow (in a back and forth motion)
    private IEnumerator LerpDialogueArrow()
    {
        /*** For Debugging Purposes ONLY ***/
        //dialogueArrowDestination = new Vector2(dialogueArrowOrigPos.x - animDistance, dialogueArrowOrigPos.y);

        float halfDuration = animDuration / 2f;
        float time = 0;

        while (time < halfDuration)
        {
            dialogueArrowRT.anchoredPosition = Vector2.Lerp(dialogueArrowOrigPos, dialogueArrowDestination, time / halfDuration);
            time += Time.deltaTime;
            yield return null;
        }

        dialogueArrowRT.anchoredPosition = dialogueArrowDestination;
        time = 0;

        while (time < halfDuration)
        {
            dialogueArrowRT.anchoredPosition = Vector2.Lerp(dialogueArrowDestination, dialogueArrowOrigPos, time / halfDuration);
            time += Time.deltaTime;
            yield return null;
        }

        dialogueArrowRT.anchoredPosition = dialogueArrowOrigPos;
        PlayDialogueArrowAnim();
    }

    // Sets the dialogue arrow active after a delay
    private IEnumerator SetDialogueArrowActiveDelay()
    {
        yield return new WaitForSeconds(0.25f);
        SetDialoguArrowPosition(dialogueOptionsIndex);
        dialogueOptionButtons.SetActive(true);
        dialogueArrow.SetActive(true);
        PlayDialogueArrowAnim();
        StartDialogueOptionsInputCoroutine();
    }

    // Sets values for all appropriate vectors
    private void SetVectors()
    {
        // Note: The speech bubble holders for the player and npc are identical
        sbhOffsetPositionY = playerSBH.anchoredPosition.y;
        sbhOffsetPositionX = playerSBH.rect.width / 2f;

        bubbleHolderOrigPos = new Vector2(0, sbhOffsetPositionY);
        bubbleHolderRightPos = new Vector2(sbhOffsetPositionX, sbhOffsetPositionY);
        bubbleHolderLeftPos = new Vector2(-sbhOffsetPositionX, sbhOffsetPositionY);

        originalPivot = new Vector2(0.5f, 0);
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
