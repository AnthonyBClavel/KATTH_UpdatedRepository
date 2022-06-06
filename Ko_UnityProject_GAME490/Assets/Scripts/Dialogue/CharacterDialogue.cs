using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;

public class CharacterDialogue : MonoBehaviour
{
    [SerializeField]
    private Vector2 test;
    [SerializeField]
    private Vector2 test02;

    [Header("Dialogue Arrow Animation Variables")]
    //[Range(0.1f, 2.0f)]
    private float animDuration = 1f;
    //[Range(1f, 90f)]
    private float animDistance = 15f;

    private int sentenceIndex;
    private int artifactIndex;
    private int dialogueOptionsIndex;

    private float typingSpeed = 0.03f; // 0.03f
    private float originalTypingSpeed; // 0.03f
    private float speechBubbleHolderOrigPosY;
    private float speechBubbleHolderOrigWidth;

    private string nextSentence;
    private string currentlyTalking;

    private bool isPlayerSpeaking = false;
    private bool isInteractingWithNPC = false;
    private bool isInteractingWithArtifact = false;
    private bool inDialogue = false;
    private bool inDialogueOptions = false;
    private bool hasSetBubbleDefaultPosX = false;
    private bool hasSetBubbleDefaultPosY = false;
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

    private RectTransform playerDB; // DB = dialogue bubble
    private RectTransform nPCDB;
    private RectTransform playerSBH; // SBH = speech bubble holder
    private RectTransform nPCSBH;
    private RectTransform alertBubbleRT;
    private RectTransform dialogueArrowRT;
    private RectTransform rectTransform;

    private VerticalLayoutGroup playerSpeechBubbleVLG;
    private AudioSource charNoiseSFX;
    private Animator playerBubbleAnim;
    private Animator nPCBubbleAnim;
    private Camera dialogueCamera;

    private Vector3 playerDirection;
    Vector3 up = Vector3.zero, // Looking North
    right = new Vector3(0, 90, 0), // Looking East
    down = new Vector3(0, 180, 0), // Looking South
    left = new Vector3(0, 270, 0); // Looking West

    private Vector2 bubbleHolderOrigPos;
    private Vector2 bubbleHolderRightPos;
    private Vector2 bubbleHolderLeftPos;

    private Vector2 originalPivot;
    private Vector2 rightPivot;
    private Vector2 leftPivot;
    private Vector2 dialogueArrowOrigPos;
    private Vector2 dialogueArrowDestination;

    private Vector2 playerDialogueCheckAnchorPos;
    private Vector2 nPCDialogueCheckAnchorPos;

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

    private IEnumerator inputCoroutine;
    private IEnumerator doInputCoroutine;
    private IEnumerator dialogueArrowCoroutine;
    private IEnumerator dialogueOptionsCoroutine;

    private Artifact artifactScript;
    private NonPlayerCharacter nPCScript;
    private FidgetController nPCFidgetScript;
    private FidgetController playerFidgetScript;
    private TileMovementController playerScript;
    private CameraController cameraScript;
    private GameManager gameManagerScript;
    private AudioManager audioManagerScript;
    private BlackBars blackBarsScript;
    private GameHUD gameHUDScript;
    private TransitionFade transitionFadeScript;

    // Review variables below ///////////////////////////////////////////////////////////////////////////

    private float screenLeftEdgePosX = -885; // -960f 
    private float screenRightEdgePosX = 885; // 960f
    private float screenTopEdgePosY = 415; 

    private float psbh_RightEdgePosX;
    private float psbh_LeftEdgePosX;
    private float psbh_TopEdgePosY;

    private float npcsbh_RightEdgePosX;
    private float npcsbh_LeftEdgePosX;
    private float npcsbh_TopEdgePosY;

    private float psbh_Width; // psbh = player speech bubble holder
    private float psbh_Height;
    private float psbh_LocalPosX;
    private float psbh_LocalPoxY;

    private float pdb_LocalPosX; // pdb = player dialogue bubble
    private float pdb_LocalPosY;

    private float npcsbh_Width; // npcbh = npc speech bubble holder
    private float npcsbh_Height;
    private float npcsbh_LocalPosX;
    private float npcsbh_LocalPosY;

    private float npcdb_LocalPosX; // npcdb = npc dialogue bubble
    private float npcdb_LocalPosY;

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
        SetVectors();
    }

    // Start is called before the first frame update
    void Start()
    {
        //screenRightEdgePosX = pauseMenuScript.GetComponent<RectTransform>().rect.width / 2;   
        //screenLeftEdgePosX = -screenRightEdgePosX;
    }

    // Update is called once per frame
    void Update()
    {
        // Updates the player's current direction
        if (playerDirection != playerScript.transform.eulerAngles)
            playerDirection = playerScript.transform.eulerAngles;

        AdjustDialogueBubbleCheckPlayer();
        AdjustDialogueBubbleCheckNPC();

        /*** For Debugging purposes ***/
        /*if (Input.GetKeyDown(KeyCode.T))
        {
            
        }
        /*** End Debugging ***/

        //ClampTest();
    }

    // LateUpdate is called once per frame - after all Update() functions have been called
    void LateUpdate()
    {
        SetPlayerBubblePosition();
        SetNPCBubblePosition();
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
        gameManagerScript.CheckForCharacterDialogueDebug();
        originalTypingSpeed = typingSpeed; //typingSpeed = originalTypingSpeed;

        sentenceIndex = 0;
        NextDialogue(dialogueSentences[0]);
        StartInputCoroutine();
    }

    // Ends the character dialogue
    private void EndDialogue()
    {
        // Note: the dialogue will end after choosing/playing the last dialogue option
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
    private void NextDialogue(string theString)
    {
        string characterName = theString.Remove(theString.IndexOf(':') + 1);
        nextSentence = theString.Replace(characterName, "").Trim();
        isPlayerSpeaking = characterName.Contains("PLAYER");
        PlayFidgetAndBubblePopSFX(characterName);

        StartCoroutine(TypeCharacterDialogue());
    }

    // Sets the initial dialogue to play - when initially interacting with an artifact/npc
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

    // Checks for the next sentence to play
    private void NextSentenceCheck()
    {
        gameManagerScript.CheckForCharacterDialogueDebug();
        originalTypingSpeed = typingSpeed; //typingSpeed = originalTypingSpeed;
        continueButton.SetActive(false);

        if (sentenceIndex < dialogueSentences.Length - 1 && dialogueSentences[sentenceIndex + 1] != string.Empty)
            NextDialogue(dialogueSentences[++sentenceIndex]);
        else
            EndDialogue();
    }

    // Checks to set the dialogue options active/inactive
    private void ShowDialgoueOptionsCheck()
    {
        for (int i = 0; i < dialogueOptions.Length; i++)
        {
            // Sets the dialogue options ACTIVE
            if (inDialogueOptions)
            {
                doTextComponenets[i].text = dialogueOptions[i];
                doGameObjects[i].SetActive(true);
            }
            // Sets the dialogue options INACTIVE
            else
            {
                doTextComponenets[i].text = string.Empty;
                doGameObjects[i].SetActive(false);
            }

            doTextComponenets[i].color = unselectedTextColor;
        }

        // Only shows the "Collect" dialogue option only alfter the player has inspected the artifact
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

        hasSetBubbleDefaultPosX = false;
        hasSetBubbleDefaultPosY = false;
    }

    // Opens the dialogue options
    public void OpenDialogueOptions()
    {
        if (dialogueOptionsCoroutine != null)
            StopCoroutine(dialogueOptionsCoroutine);

        dialogueOptionsCoroutine = OpenDialogueOptionsDelay();
        StartCoroutine(dialogueOptionsCoroutine);
    }

    // Closes the dialogue options
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

        // Checks which closing option to play
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

    // Clears the text within all text components
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
        bigArrowSprite.color = color;
        playerDialogueText.color = color;
        playerBubbleSprite.color = color;
        playerTailSprite.color = color;
    }

    // Sets the color for the npc dialogue bubble
    private void SetNPCBubbleColor(Color32 color)
    {
        nPCDialogueText.color = color;
        nPCBubbleSprite.color = color;
        nPCTailSprite.color = color;
    }

    // Checks to set the color for the appropriate dialoge bubble and text
    private void DialogueBubbleColorCheck()
    {
        if (isPlayerSpeaking)
        {
            if (playerDialogueText.color != playerBubbleColor)
            {
                SetPlayerBubbleColor(playerBubbleColor);
                //Debug.Log("Updated Bubble Color For Player");
            }
        }
        else
        {
            if (nPCDialogueText.color != nPCBubbleColor)
            {
                SetNPCBubbleColor(nPCBubbleColor);
                //Debug.Log("Updated Bubble Color For NPC");
            }
        }
    }

    // Checks to play a fidget animation and bubble pop sfx for the character (player/npc)
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

    // Starts the coroutine that checks for the character dialogue input
    private void StartInputCoroutine()
    {
        if (inputCoroutine != null)
            StopCoroutine(inputCoroutine);

        inputCoroutine = ContinueDialogueInputCheck();
        StartCoroutine(inputCoroutine);
    }

    // Starts the coroutine that checks for the dialogue options input
    private void StartDialogueOptionsInputCoroutine()
    {
        if (doInputCoroutine != null)
            StopCoroutine(doInputCoroutine);

        doInputCoroutine = DialogueOptionsInputCheck();
        StartCoroutine(doInputCoroutine);
    }

    // Starts the dialogue arrow corouitne
    private void PlayDialogueArrowAnim()
    {
        if (dialogueArrowCoroutine != null)
            StopCoroutine(dialogueArrowCoroutine);

        dialogueArrowCoroutine = LerpDialogueArrow();
        StartCoroutine(dialogueArrowCoroutine);
    }

    // Stops the dialogue arrow corouitne and sets resets its position
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

        SetDialogueBubblePivots();
        FadeInDialogueMusic();
        SetInitialDialogue();
        inDialogue = true;

        yield return new WaitForSeconds(0.5f); // maybe wait for when the camera to finish transforming here 
        StartDialogue();
    }

    // Ends the dialogue after a delay (cant interact with npc after a small delay)
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
        hasSetBubbleDefaultPosX = false;
        hasSetBubbleDefaultPosY = false;

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

    // Types out the character diaogue
    private IEnumerator TypeCharacterDialogue()
    {
        Animator animator = isPlayerSpeaking ? playerBubbleAnim : nPCBubbleAnim;
        GameObject currentdialogueBubble = isPlayerSpeaking ? playerDialogueBubble : nPCDialogueBubble;
        GameObject previousDialogueBubble = isPlayerSpeaking ? nPCDialogueBubble : playerDialogueBubble;
        TextMeshProUGUI dialogueText = isPlayerSpeaking ? playerDialogueText : nPCDialogueText;
        string textHexColor = isPlayerSpeaking ? playerTextColor.ReturnHexColor() : nPCTextColor.ReturnHexColor();

        // Note: the bools below MUST be called here!
        hasSetBubbleDefaultPosX = false;
        hasSetBubbleDefaultPosY = false;

        if (!currentdialogueBubble.activeInHierarchy)
        {
            currentdialogueBubble.SetActive(true);
            previousDialogueBubble.SetActive(false);
        }
        else
            animator.SetTrigger("NextSentence");

        yield return new WaitForSeconds(0.05f); // use 0.05f or 0.1f
        EmptyTextComponents();
        DialogueBubbleColorCheck();
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

    // Checks for the input that selects a dialogue options
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

    // Lerps the position of the dialogue arrow
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

    // Sets the pivots for all dialogue bubbles (player/npc/dialogueOptions)
    private void SetDialogueBubblePivots()
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

    /** Refined Code ENDS here **/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // Sets values for all vectors - REVIEW THIS STILL
    private void SetVectors()
    {
        // Note: The speech bubble holders for the player and npc are identical
        speechBubbleHolderOrigPosY = playerSBH.anchoredPosition.y;
        speechBubbleHolderOrigWidth = playerSBH.rect.width / 2f;

        bubbleHolderOrigPos = new Vector2(0, speechBubbleHolderOrigPosY);
        bubbleHolderRightPos = new Vector2(speechBubbleHolderOrigWidth, speechBubbleHolderOrigPosY); // Note: 80 is half of the bubbles original width
        bubbleHolderLeftPos = new Vector2(-speechBubbleHolderOrigWidth, speechBubbleHolderOrigPosY);

        originalPivot = new Vector2(0.5f, 0);
        rightPivot = new Vector2(1, 0);
        leftPivot = new Vector2(0, 0);
    }

    // Sets the player's dialogue bubble and alert bubble to follow the player
    private void SetPlayerBubblePosition()
    {
        /*Vector3 dcScreenPoint = dialogueCamera.WorldToScreenPoint(playerDialogueCheck.transform.position);

        if (playerDialogueBubble.transform.position != dcScreenPoint)
            playerDialogueBubble.transform.position = dcScreenPoint;

        if (alertBubble.transform.position != dcScreenPoint)
            alertBubble.transform.position = dcScreenPoint;*/

        Vector2 dialogueCheckScreenPoint = dialogueCamera.WorldToScreenPoint(playerDialogueCheck.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, dialogueCheckScreenPoint, null, out playerDialogueCheckAnchorPos);

        if (playerDB.anchoredPosition != playerDialogueCheckAnchorPos)
            playerDB.anchoredPosition = playerDialogueCheckAnchorPos;

        if (alertBubbleRT.anchoredPosition != playerDialogueCheckAnchorPos)
            alertBubbleRT.anchoredPosition = playerDialogueCheckAnchorPos;
    }

    // Sets the npc's dialogue bubble to follow the player
    private void SetNPCBubblePosition()
    {
        /*Vector3 dcScreenPoint = dialogueCamera.WorldToScreenPoint(nPCDialogueCheck.transform.position);

        if (nPCDialogueBubble.transform.position != dcScreenPoint)
            nPCDialogueBubble.transform.position = dcScreenPoint;*/

        Vector2 dialogueCheckScreenPoint = dialogueCamera.WorldToScreenPoint(nPCDialogueCheck.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, dialogueCheckScreenPoint, null, out nPCDialogueCheckAnchorPos);

        if (nPCDB.anchoredPosition != nPCDialogueCheckAnchorPos)
            nPCDB.anchoredPosition = nPCDialogueCheckAnchorPos;
    }

    private void ClampTest()
    {
        PlayerBubbleValuesCheck();

        // Grab the length and width of the screen, make sure to adjuct accrodingly to wide screens as well (possibly make 1920pixels the max width before indenting the sentences)
        float halfOfScreenWidth = 960f;
        float halfOfScreenHeight = 540f;

        float clampX;
        float clampY;
        RectTransform playerBubble = playerDialogueBubble.GetComponent<RectTransform>();

        // remeber that anchored position is a Vetcor2 (it does not take a z value)
        if (playerDirection == up || playerDirection == down)
        {
            // Note: 80 is the length and witdh of the bubble origin image

            float bubbleWidth = (psbh_Width / 2);
            float bubbleHieght = (psbh_Height + 80);
   
            clampX = Mathf.Clamp(playerBubble.anchoredPosition.x, -halfOfScreenWidth + bubbleWidth, halfOfScreenWidth - bubbleWidth);
            clampY = Mathf.Clamp(playerBubble.anchoredPosition.y, -halfOfScreenHeight, halfOfScreenHeight - bubbleHieght);

            // Rather than adjusting the player bubble, adjust the speech bubble image itself
            playerBubble.anchoredPosition = new Vector2(clampX, clampY);
        }

    }


    // Checks to see if the player's dialogue bubble goes outside of the screen and re-adjusts it accordingly
    private void AdjustDialogueBubbleCheckPlayer()
    {
        PlayerBubbleValuesCheck();

        if (playerDialogueBubble.activeInHierarchy)
        {
            psbh_TopEdgePosY = ((psbh_LocalPoxY + psbh_Height)) + pdb_LocalPosY;
            float playerBubblePosX = playerDB.anchoredPosition.x;
            float playerBubblePosY = playerDB.anchoredPosition.y;

            if (psbh_TopEdgePosY > screenTopEdgePosY)
            {
                if (playerDB.anchoredPosition != new Vector2(playerBubblePosX, screenTopEdgePosY - psbh_TopEdgePosY))
                    playerDB.anchoredPosition = new Vector2(playerBubblePosX, screenTopEdgePosY - psbh_TopEdgePosY);
            }

            if (psbh_TopEdgePosY < screenTopEdgePosY && !hasSetBubbleDefaultPosY)
            {
                // need to return to the worldspace point here - the one that follows the character's dialogue bubble check
                if (playerDB.anchoredPosition != new Vector2(playerBubblePosX, playerBubblePosY)) //
                    playerDB.anchoredPosition = new Vector2(playerBubblePosX, playerBubblePosY); //

                hasSetBubbleDefaultPosY = true;
            }

            if (playerDirection == right)
            {
                psbh_LeftEdgePosX = pdb_LocalPosX + ((psbh_LocalPosX - psbh_Width));

                if (psbh_LeftEdgePosX < screenLeftEdgePosX)
                {
                    if (playerSBH.anchoredPosition != new Vector2(playerSBH.anchoredPosition.x + ((screenLeftEdgePosX - psbh_LeftEdgePosX)), speechBubbleHolderOrigPosY))
                        playerSBH.anchoredPosition = new Vector2(playerSBH.anchoredPosition.x + ((screenLeftEdgePosX - psbh_LeftEdgePosX)), speechBubbleHolderOrigPosY);
                        //playerSpeechBubbleHolder.localPosition = new Vector3(playerSpeechBubbleHolder.localPosition.x + ((-playerLeftEdgePosX - 960) / 0.8f), 78, 0);
                }

                if (psbh_LeftEdgePosX > screenLeftEdgePosX && !hasSetBubbleDefaultPosX)
                {
                    if (playerSBH.anchoredPosition != bubbleHolderRightPos)
                        playerSBH.anchoredPosition = bubbleHolderRightPos;

                    hasSetBubbleDefaultPosX = true;
                }
            }

            else if (playerDirection == left)
            {
                psbh_RightEdgePosX = pdb_LocalPosX + ((psbh_LocalPosX + psbh_Width));

                if (psbh_RightEdgePosX > screenRightEdgePosX)
                {
                    if (playerSBH.anchoredPosition != new Vector2(playerSBH.anchoredPosition.x - ((psbh_RightEdgePosX - screenRightEdgePosX)), speechBubbleHolderOrigPosY))
                        playerSBH.anchoredPosition = new Vector2(playerSBH.anchoredPosition.x - ((psbh_RightEdgePosX - screenRightEdgePosX)), speechBubbleHolderOrigPosY);
                        //playerSpeechBubbleHolder.localPosition = new Vector3(playerSpeechBubbleHolder.localPosition.x - ((rightEdgePosX - 960) / 0.8f), 78, 0);
                }

                if (psbh_RightEdgePosX < screenRightEdgePosX && !hasSetBubbleDefaultPosX)
                {
                    if (playerSBH.anchoredPosition != bubbleHolderLeftPos)
                        playerSBH.anchoredPosition = bubbleHolderLeftPos;

                    hasSetBubbleDefaultPosX = true;
                }
            }

            else if (playerDirection == up || playerDirection == down)
            {
                psbh_LeftEdgePosX = pdb_LocalPosX + ((psbh_LocalPosX - psbh_Width * 0.5f));
                psbh_RightEdgePosX = pdb_LocalPosX + ((psbh_LocalPosX + psbh_Width * 0.5f));

                if (psbh_LeftEdgePosX < screenLeftEdgePosX)
                {
                    if (playerSBH.anchoredPosition != new Vector2(playerSBH.anchoredPosition.x + ((screenLeftEdgePosX - psbh_LeftEdgePosX)), speechBubbleHolderOrigPosY))
                        playerSBH.anchoredPosition = new Vector2(playerSBH.anchoredPosition.x + ((screenLeftEdgePosX - psbh_LeftEdgePosX)), speechBubbleHolderOrigPosY);
                }

                if (psbh_RightEdgePosX > screenRightEdgePosX)
                {
                    if (playerSBH.anchoredPosition != new Vector2(playerSBH.anchoredPosition.x - ((psbh_RightEdgePosX - screenRightEdgePosX)), speechBubbleHolderOrigPosY))
                        playerSBH.anchoredPosition = new Vector2(playerSBH.anchoredPosition.x - ((psbh_RightEdgePosX - screenRightEdgePosX)), speechBubbleHolderOrigPosY);
                }

                if (psbh_LeftEdgePosX > screenLeftEdgePosX && psbh_RightEdgePosX < screenRightEdgePosX && !hasSetBubbleDefaultPosX)
                {
                    if (playerSBH.anchoredPosition != bubbleHolderOrigPos)
                        playerSBH.anchoredPosition = bubbleHolderOrigPos;

                    hasSetBubbleDefaultPosX = true;
                }
            }

        }
    }

    // Checks to see if the npc's dialogue bubble goes outside of the screen and re-adjusts it accordingly
    private void AdjustDialogueBubbleCheckNPC()
    {
        NPCBubbleValuesCheck();

        if (nPCDialogueBubble.activeInHierarchy)
        {
            npcsbh_TopEdgePosY = ((npcsbh_LocalPosY + npcsbh_Height)) + npcdb_LocalPosY;
            float nPCBubblePosX = nPCDB.anchoredPosition.x;
            float nPCBubblePosY = nPCDB.anchoredPosition.y;

            if (npcsbh_TopEdgePosY > screenTopEdgePosY)
            {
                if (nPCDB.anchoredPosition != new Vector2(nPCBubblePosX, screenTopEdgePosY - npcsbh_TopEdgePosY))
                    nPCDB.anchoredPosition = new Vector2(nPCBubblePosX, screenTopEdgePosY - npcsbh_TopEdgePosY);
            }

            if (npcsbh_TopEdgePosY < screenTopEdgePosY && !hasSetBubbleDefaultPosY)
            {
                if (nPCDB.anchoredPosition != new Vector2(nPCBubblePosX, nPCBubblePosY))
                    nPCDB.anchoredPosition = new Vector2(nPCBubblePosX, nPCBubblePosY);

                hasSetBubbleDefaultPosY = true;
            }

            if (playerDirection == right)
            {
                npcsbh_RightEdgePosX = npcdb_LocalPosX + ((npcsbh_LocalPosX + npcsbh_Width));

                if (npcsbh_RightEdgePosX > screenRightEdgePosX)
                {
                    if (nPCSBH.anchoredPosition != new Vector2(nPCSBH.anchoredPosition.x - ((npcsbh_RightEdgePosX - screenRightEdgePosX)), speechBubbleHolderOrigPosY))
                        nPCSBH.anchoredPosition = new Vector2(nPCSBH.anchoredPosition.x - ((npcsbh_RightEdgePosX - screenRightEdgePosX)), speechBubbleHolderOrigPosY);
                }

                if (npcsbh_RightEdgePosX < screenRightEdgePosX && !hasSetBubbleDefaultPosX)
                {
                    if (nPCSBH.anchoredPosition != bubbleHolderLeftPos)
                        nPCSBH.anchoredPosition = bubbleHolderLeftPos;

                    hasSetBubbleDefaultPosX = true;
                }
            }

            else if (playerDirection == left)
            {
                npcsbh_LeftEdgePosX = npcdb_LocalPosX + ((npcsbh_LocalPosX - npcsbh_Width));

                if (npcsbh_LeftEdgePosX < screenLeftEdgePosX)
                {
                    if (nPCSBH.anchoredPosition != new Vector2(nPCSBH.anchoredPosition.x + ((screenLeftEdgePosX - npcsbh_LeftEdgePosX)), speechBubbleHolderOrigPosY))
                        nPCSBH.anchoredPosition = new Vector2(nPCSBH.anchoredPosition.x + ((screenLeftEdgePosX - npcsbh_LeftEdgePosX)), speechBubbleHolderOrigPosY);
                }

                if (npcsbh_LeftEdgePosX > screenLeftEdgePosX && !hasSetBubbleDefaultPosX)
                {
                    if (nPCSBH.anchoredPosition != bubbleHolderRightPos)
                        nPCSBH.anchoredPosition = bubbleHolderRightPos;

                    hasSetBubbleDefaultPosX = true;
                }
            }

            else if (playerDirection == up || playerDirection == down)
            {
                npcsbh_LeftEdgePosX = npcdb_LocalPosX + ((npcsbh_LocalPosX - npcsbh_Width * 0.5f));
                npcsbh_RightEdgePosX = npcdb_LocalPosX + ((npcsbh_LocalPosX + npcsbh_Width * 0.5f));

                if (npcsbh_LeftEdgePosX < screenLeftEdgePosX)
                {
                    if (nPCSBH.anchoredPosition != new Vector2(nPCSBH.anchoredPosition.x + ((screenLeftEdgePosX - psbh_LeftEdgePosX)), speechBubbleHolderOrigPosY))
                        nPCSBH.anchoredPosition = new Vector2(nPCSBH.anchoredPosition.x + ((screenLeftEdgePosX - psbh_LeftEdgePosX)), speechBubbleHolderOrigPosY);
                }

                if (npcsbh_RightEdgePosX > screenRightEdgePosX)
                {
                    if (nPCSBH.anchoredPosition != new Vector2(nPCSBH.anchoredPosition.x - ((npcsbh_RightEdgePosX - screenRightEdgePosX)), speechBubbleHolderOrigPosY))
                        nPCSBH.anchoredPosition = new Vector2(nPCSBH.anchoredPosition.x - ((npcsbh_RightEdgePosX - screenRightEdgePosX)), speechBubbleHolderOrigPosY);
                }

                if (npcsbh_LeftEdgePosX > screenLeftEdgePosX && npcsbh_RightEdgePosX < screenRightEdgePosX && !hasSetBubbleDefaultPosX)
                {
                    if (nPCSBH.anchoredPosition != bubbleHolderOrigPos)
                        nPCSBH.anchoredPosition = bubbleHolderOrigPos;

                    hasSetBubbleDefaultPosX = true;
                }
            }

        }
    }

    // Updates the floats for the player's bubble when they change
    private void PlayerBubbleValuesCheck()
    {
        if (psbh_Height != playerSBH.rect.height)
            psbh_Height = playerSBH.rect.height;

        if (psbh_Width != playerSBH.rect.width)
            psbh_Width = playerSBH.rect.width;

        if (psbh_LocalPosX != playerSBH.anchoredPosition.x)
            psbh_LocalPosX = playerSBH.anchoredPosition.x;

        if (psbh_LocalPoxY != playerSBH.anchoredPosition.y)
            psbh_LocalPoxY = playerSBH.anchoredPosition.y;

        if (pdb_LocalPosX != playerDB.anchoredPosition.x)
            pdb_LocalPosX = playerDB.anchoredPosition.x;

        if (pdb_LocalPosY != playerDB.anchoredPosition.y)
            pdb_LocalPosY = playerDB.anchoredPosition.y;
    }

    // Updates the floats for the npc's bubble when they change
    private void NPCBubbleValuesCheck()
    {
        if (npcsbh_Width != nPCSBH.rect.width)
            npcsbh_Width = nPCSBH.rect.width;

        if (npcsbh_Height != nPCSBH.rect.height)
            npcsbh_Height = nPCSBH.rect.height;

        if (npcsbh_LocalPosX != nPCSBH.anchoredPosition.x)
            npcsbh_LocalPosX = nPCSBH.anchoredPosition.x;

        if (npcsbh_LocalPosY != nPCSBH.anchoredPosition.y)
            npcsbh_LocalPosY = nPCSBH.anchoredPosition.y;

        if (npcdb_LocalPosX != nPCDB.anchoredPosition.x)
            npcdb_LocalPosX = nPCDB.anchoredPosition.x;

        if (npcdb_LocalPosY != nPCDB.anchoredPosition.y)
            npcdb_LocalPosY = nPCDB.anchoredPosition.y;
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        nPCScript = FindObjectOfType<NonPlayerCharacter>();
        cameraScript = FindObjectOfType<CameraController>();
        artifactScript = FindObjectOfType<Artifact>();
        gameManagerScript = FindObjectOfType<GameManager>();
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
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
           
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
                        dialogueArrowOrigPos = dialogueArrowRT.anchoredPosition;
                        dialogueArrowDestination = new Vector2(dialogueArrowOrigPos.x - animDistance, dialogueArrowOrigPos.y);
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
                    string childName02 = child02.name;

                    if (childName02 == "ContinueButton")
                        continueButton = child02;
                    if (childName02 == "DialogueOptionButtons")
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

        rectTransform = GetComponent<RectTransform>();
        charNoiseSFX = audioManagerScript.charNoiseAS;
        typingSpeed = gameManagerScript.typingSpeed;
        originalTypingSpeed = typingSpeed;
    }

}
