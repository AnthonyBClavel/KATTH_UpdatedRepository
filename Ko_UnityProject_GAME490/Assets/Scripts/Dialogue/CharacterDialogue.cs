using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterDialogue : MonoBehaviour
{
    private int sentenceIndex;
    private int artifactIndex;
    private int dialogueOptionsIndex;

    private float typingSpeed; // 0.03f
    private float originalTypingSpeed; // 0.03f

    private string nextSentence;
    private string currentlyTalking;

    private bool isPlayerSpeaking = false;
    private bool isInteractingWithNPC = false;
    private bool isInteractingWithArtifact = false;
    private bool inDialogue = false;
    private bool canSpeedUpDialogue = false;
    private bool selectedLastOption = false;
    private bool hasSetBubbleDefaultPosX = false;
    private bool hasSetBubbleDefaultPosY = false;

    private GameObject dialogueArrowHolder;
    private GameObject dialogueOptionButtons;

    private AudioSource charNoiseSFX;
    private Animator playerBubbleAnim;
    private Animator nPCBubbleAnim;
    private Animator dialogueOptionsBubbleAnim;

    [Header("Dialogue Variables")]
    private List<GameObject> doGameObjects = new List<GameObject>();
    private List<TextMeshProUGUI> doTextComponenets = new List<TextMeshProUGUI>();
    private TextAsset[] nPCDialogue;
    private TextAsset[] artifactDialogue;
    private string[] dialogueSentences;
    private string[] dialogueOptions;

    [Header("TextMeshPro")]
    // Note: the bubble color text is used to "calculate" the size of the bubble (to fit the text), dialogue text will overlay this text once size is found
    public TextMeshProUGUI playerBubbleColorText;
    public TextMeshProUGUI nPCBubbleColorText;
    private TextMeshProUGUI playerForegroundText;
    private TextMeshProUGUI nPCForegroundText;

    [Header("Scriptable Objects")]
    private Artifact_SO artifact;
    private NonPlayerCharacter_SO nonPlayerCharacter;

    private IEnumerator inputCoroutine;
    private IEnumerator doInputCoroutine;

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
    private DialogueArrow dialogueArrowScript;
    private TransitionFade transitionFadeScript;

    // Review variables below ///////////////////////////////////////////////////////////////////////////

    private float dialogueBubbleScale = 0.8f; // the scale of the parent object affects the child object's positioning

    private float screenLeftEdgePosX = -885; // -960f 
    private float screenRightEdgePosX = 885; // 960f
    private float screenTopEdgePosY = 415; 
    private float screenLeftEdgePosX02 = -885;
    private float screenRightEdgePosX02 = 885;

    private float psbh_RightEdgePosX;
    private float psbh_LeftEdgePosX;
    private float psbh_TopEdgePosY;

    private float npcsbh_RightEdgePosX;
    private float npcsbh_LeftEdgePosX;
    private float npcsbh_TopEdgePosY;

    private float dosph_LeftEdgePosX;
    private float dosph_RightEdgePosX;
    private float dosph_TopEdgePosY;

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

    private float dosph_Width; // dosbh = dialogue options speech bubble holder
    private float dosph_Height;
    private float dosph_LocalPosX;
    private float dosph_LocalPosY;

    private float dob_LocalPosX; // dob = dialogue options bubble
    private float dob_LocalPosY;

    private GameObject playerDialogueBubble;
    private GameObject nPCDialogueBubble;
    private GameObject dialogueOptionsBubble;
    private GameObject playerAlertBubble;
    private GameObject continueButton;
    private GameObject playerDialogueCheck;
    private GameObject nPCDialogueCheck;
    private Camera dialogueCamera;

    private RectTransform playerSpeechBubbleHolder;
    private RectTransform nPCSpeechBubbleHolder;
    private RectTransform doSpeechBubbleHolder; // do = dialogue options

    private Vector3 playerDirection;
    Vector3 up = Vector3.zero, // Looking North
    right = new Vector3(0, 90, 0), // Looking East
    down = new Vector3(0, 180, 0), // Looking South
    left = new Vector3(0, 270, 0); // Looking West

    private Vector3 bubbleAnimOrigPos;
    private Vector3 bubbleHolderOrigPos;
    private Vector3 moveBubbleRight;
    private Vector3 moveBubbleLeft;

    private Vector2 originalPivot;
    private Vector2 movePivotRight;
    private Vector2 movePivotLeft;

    private Vector3 dialogueArrowDefaultPos = new Vector3(5, 5, 0);
    private Color32 selectedTextColor = new Color32(128, 160, 198, 255);
    private Color32 unselectedTextColor = Color.gray;
    private Color32 nPCTextColor;

    public float TypingSpeed
    {
        get { return typingSpeed; }
        set { typingSpeed = value; }
    }

    public bool InDialogue
    {
        get { return inDialogue; }
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
        //dialogueBubbleScale = playerBubbleAnim.gameObject.transform.localScale.x;
    }

    // Update is called once per frame
    void Update()
    {
        // Updates the player's current direction
        if (playerDirection != playerScript.transform.eulerAngles)
            playerDirection = playerScript.transform.eulerAngles;

        AdjustDialogueBubbleCheckPlayer();
        AdjustDialogueBubbleCheckNPC();
        AdjustDialogueOptionsBubbleCheck();

        /*** For Debugging purposes ***/
        /*if (Input.GetKeyDown(KeyCode.R) && !inDialogue)
        {
           
        }
        /*** End Debugging ***/
    }

    // LateUpdate is called once per frame - after all Update() functions have been called
    void LateUpdate()
    {
        SetPlayerBubblePosition();
        SetNPCBubblePosition();
        SetDialogueBubblePosition();
        SetAlertBubblePosition();
    }

    /** Refined Code STARTS here **///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // Updates and starts the dialogue for the npc
    public void StartNPCDialogue(NonPlayerCharacter newScript, NonPlayerCharacter_SO newNPC)
    {
        if (nonPlayerCharacter != newNPC)
        {
            nonPlayerCharacter = newNPC;
            nPCScript = newScript;

            nPCTextColor = nonPlayerCharacter.nPCTextColor;
            nPCDialogue = nonPlayerCharacter.nPCDialogue;
            SetDialogueOptions(nonPlayerCharacter.dialogueOptions);

            nPCDialogueCheck = nPCScript.DialogueCheck;
            nPCFidgetScript = nPCScript.FidgetScript;
        }

        isInteractingWithNPC = true;
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
            SetDialogueOptions(artifact.dialogueOptions);
        }

        isInteractingWithArtifact = true;
        StartCoroutine(StartDialogueDelay());
    }

    // Sets the character dialogue
    private void SetCharacterDialogue(TextAsset characterDialogue) => dialogueSentences = characterDialogue.ReturnSentences();

    // Sets the dialogue options
    private void SetDialogueOptions(TextAsset dialogeOptionsFile)
    {
        dialogueOptions = dialogeOptionsFile.ReturnSentences();
        nPCScript.SetDialogueOptionBools(dialogueOptions.Length);
    }

    // Starts the character dialogue
    private void StartDialogue()
    {
        gameManagerScript.CheckForCharacterDialogueDebug();
        originalTypingSpeed = typingSpeed; //typingSpeed = originalTypingSpeed;

        sentenceIndex = 0;
        NextDialogue(dialogueSentences[0]);
    }

    // Ends the character dialogue
    private void EndDialogue()
    {
        if (selectedLastOption)
        {        
            StartCoroutine(EndDialogueDelay());
            FadeOutDialogueMusic();
            //Debug.Log("Dialogue Has Ended");
        }
        else
        {                         
            OpenDialogueOptions();
            audioManagerScript.PlayDialoguePopUpSFX01();
            //Debug.Log("Opened Dialogue Options");
        }
    }

    // Determines, sets, and plays the next dialogue sentence (theString = the next line/string within the text file)
    private void NextDialogue(string theString)
    {
        string characterName = theString.Remove(theString.IndexOf(':') + 1);
        nextSentence = theString.Replace(characterName, "").Trim();
        isPlayerSpeaking = characterName.Contains("PLAYER");
        PlayBubblePopSFX(characterName);

        StartCoroutine(TypeCharacterDialogue());
        StartInputCoroutine();
    }

    // Sets the initial dialogue to play - when initially interacting with an artifact/npc
    private void SetInitialDialogue()
    {
        if (isInteractingWithNPC)
        {
            nPCScript.SetRotationNPC();
            nPCForegroundText.color = nPCTextColor;
                
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
        continueButton.SetActive(false);
        originalTypingSpeed = typingSpeed; //typingSpeed = originalTypingSpeed;

        if (sentenceIndex < dialogueSentences.Length - 1 && dialogueSentences[sentenceIndex + 1] != string.Empty)
            NextDialogue(dialogueSentences[++sentenceIndex]);
        else
            EndDialogue();
    }

    // Checks to set the dialogue options active/inactive
    private void ShowDialgoueOptionsCheck()
    {
        GameObject dialogueOptionsHolder = doGameObjects[0].transform.parent.gameObject;

        for (int i = 0; i < dialogueOptionsHolder.transform.childCount; i++)
        {
            // Sets the dialogue options ACTIVE
            if (dialogueOptionsBubble.activeSelf)
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
            doGameObjects[1].SetActive(false);
            doTextComponenets[1].text = string.Empty;
        }
    }

    // Opens the dialogue options
    public void OpenDialogueOptions()
    {
        StartCoroutine(SetDialogueArrowActiveDelay());

        dialogueOptionsBubble.SetActive(true);
        playerDialogueBubble.SetActive(false);
        nPCDialogueBubble.SetActive(false);
        ShowDialgoueOptionsCheck();
        playerFidgetScript.Fidget(); // Note: this method MUST be called after setting dialogueOptionsBubble active!

        hasSetBubbleDefaultPosX = false;
        hasSetBubbleDefaultPosY = false;
    }

    // Closes the dialogue options
    private void CloseDialogueOptions()
    {
        dialogueArrowScript.StopDialogueArrowAnim();
        sentenceIndex = 0;

        dialogueOptionsBubble.SetActive(false);
        dialogueOptionButtons.SetActive(false);
        dialogueArrowHolder.SetActive(false);
        ShowDialgoueOptionsCheck();

        hasSetBubbleDefaultPosX = false;
        hasSetBubbleDefaultPosY = false;
    }

    // Set the position of the dialogue arrow
    private void SetDialoguArrowPosition(int doIndex)
    {
        dialogueArrowHolder.transform.SetParent(doGameObjects[doIndex].transform);
        dialogueArrowHolder.transform.localPosition = dialogueArrowDefaultPos;
        audioManagerScript.PlayButtonClick02SFX();

        // Highlights the current selected dialogue option
        for (int i = 0; i < doTextComponenets.Count; i++)
        {
            if (i == doIndex)
                doTextComponenets[i].color = selectedTextColor;
            else
                doTextComponenets[i].color = unselectedTextColor;
        }
    }

    // Clears the text within all text components
    private void EmptyTextComponents()
    {
        nPCForegroundText.text = string.Empty;
        playerForegroundText.text = string.Empty;
        playerBubbleColorText.text = string.Empty;
        nPCBubbleColorText.text = string.Empty;
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

    // Checks to play the bubble pop sfx for the character (player/npc)
    private void PlayBubblePopSFX(string characterName)
    {
        if (currentlyTalking != characterName)
        {
            if (isPlayerSpeaking)
                audioManagerScript.PlayDialoguePopUpSFX01();
            else
                audioManagerScript.PlayDialoguePopUpSFX02();

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

        isInteractingWithArtifact = false;
        isInteractingWithNPC = false;
        selectedLastOption = false;
        hasSetBubbleDefaultPosX = false;
        hasSetBubbleDefaultPosY = false;

        yield return new WaitForSeconds(0.4f);
        playerScript.AlertBubbleCheck();

        yield return new WaitForSeconds(0.1f);
        playerScript.SetPlayerBoolsTrue();
        inDialogue = false;
    }

    // Types out the character diaogue
    private IEnumerator TypeCharacterDialogue()
    {
        Animator animator = isPlayerSpeaking ? playerBubbleAnim : nPCBubbleAnim;
        FidgetController characterFidgetScript = isPlayerSpeaking ? playerFidgetScript : nPCFidgetScript;
        TextMeshProUGUI bubbleColorText = isPlayerSpeaking ? playerBubbleColorText : nPCBubbleColorText;
        TextMeshProUGUI forgroundText = isPlayerSpeaking ? playerForegroundText : nPCForegroundText;
        GameObject dialogueBubble = isPlayerSpeaking ? playerDialogueBubble : nPCDialogueBubble;
        
        hasSetBubbleDefaultPosX = false;
        hasSetBubbleDefaultPosY = false;
        canSpeedUpDialogue = false;

        if (!dialogueBubble.activeSelf)
        {
            playerDialogueBubble.SetActive(isPlayerSpeaking ? true : false);
            nPCDialogueBubble.SetActive(isPlayerSpeaking ? false : true);
            characterFidgetScript.Fidget();
        }
        else
            animator.SetTrigger("NextSentence");

        yield return new WaitForSeconds(0.05f); // use 0.05f or 0.1f
        EmptyTextComponents();
        bubbleColorText.text = nextSentence;
        canSpeedUpDialogue = true;

        foreach (char letter in nextSentence.ToCharArray())
        {
            forgroundText.text += letter;
            charNoiseSFX.Play();
            yield return new WaitForSeconds(typingSpeed);
        }

        continueButton.SetActive(true);
        canSpeedUpDialogue = false;
    }

    // Checks for the input that continues or speeds up the character dialogue
    private IEnumerator ContinueDialogueInputCheck()
    {
        while (inDialogue && !dialogueOptionsBubble.activeSelf && !transitionFadeScript.IsChangingScenes)
        {
            if (Time.deltaTime > 0)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    if (continueButton.activeSelf) // Make sure to check if the continue button cant be pressed when the player pauses the game and returns to the main menu!
                        NextSentenceCheck();

                    else if (!continueButton.activeSelf && typingSpeed > originalTypingSpeed / 2 && canSpeedUpDialogue)
                        typingSpeed /= 2;
                }
            }

            yield return null;
        }
    }

    // Checks for the input that selects a dialogue options
    private IEnumerator DialogueOptionsInputCheck()
    {
        while (dialogueArrowHolder.activeSelf && !transitionFadeScript.IsChangingScenes)
        {
            if (Time.deltaTime > 0)
            {
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    // Finds the next ACTIVE dialogue option
                    for (int i = dialogueOptionsIndex - 1; i >= 0; i--)
                    {
                        if (doGameObjects[i].activeSelf)
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
                        if (doGameObjects[i].activeSelf)
                        {
                            dialogueOptionsIndex = i;
                            SetDialoguArrowPosition(i);
                            break;
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    int dialogueChoice = (dialogueOptionsIndex * 2) + 2;
                    int dialogueChoiceVariant = dialogueChoice + 1;

                    if (isInteractingWithNPC)
                    {
                        // Note: the last dialogue option should always be the "closing" option
                        if (dialogueOptionsIndex == dialogueOptions.Length - 1)
                            selectedLastOption = true;

                        // Checks if the dialogue option has already been played
                        if (nPCScript.DialogueOptionBools[dialogueOptionsIndex] == false)
                        {
                            SetCharacterDialogue(nPCDialogue[dialogueChoice]);
                            nPCScript.DialogueOptionBools[dialogueOptionsIndex] = true;
                        }
                        else
                            SetCharacterDialogue(nPCDialogue[dialogueChoiceVariant]);

                        CloseDialogueOptions(); // This MUST be called before starting dialogue!
                        StartDialogue();
                    }

                    else if (isInteractingWithArtifact)
                    {
                        if (dialogueOptionsIndex == 0)
                        {
                            artifactScript.HasInspectedArtifact = true;
                            artifactScript.InspectArtifact();
                        }
                        else
                        {
                            if (dialogueOptions[dialogueOptionsIndex].Contains("Collect"))
                                artifactScript.CollectArtifact();

                            playerScript.ChangeAnimationState("Interacting");
                            artifactScript.CloseChest();
                            StartCoroutine(EndDialogueDelay());
                            FadeOutDialogueMusic();
                        }

                        CloseDialogueOptions();
                    }
                }
            }

            yield return null;
        }
    }

    // Sets the dialogue arrow active after a delay
    private IEnumerator SetDialogueArrowActiveDelay()
    {
        yield return new WaitForSeconds(0.25f);
        SetDialoguArrowPosition(dialogueOptionsIndex);
        dialogueOptionButtons.SetActive(true);
        dialogueArrowHolder.SetActive(true);
        dialogueArrowScript.PlayDialogueArrowAnim();
        StartDialogueOptionsInputCoroutine();
    }

    // Sets values for all vectors - REVIEW THIS STILL
    private void SetVectors()
    {
        bubbleAnimOrigPos = new Vector3(0, 0, 0);
        bubbleHolderOrigPos = new Vector3(0, 78, 0);
        moveBubbleRight = new Vector3(100f, 78, 0);
        moveBubbleLeft = new Vector3(-100f, 78, 0);

        originalPivot = new Vector2(0.5f, 0);
        movePivotRight = new Vector2(1, 0);
        movePivotLeft = new Vector2(0, 0);
    }

    // Sets the pivots for all dialogue bubbles (player/npc/dialogueOptions)
    private void SetDialogueBubblePivots()
    {
        Vector3 playerDirection = playerScript.transform.eulerAngles;

        switch (playerDirection.y)
        {
            case 0: // Looking north
                playerSpeechBubbleHolder.pivot = originalPivot;
                nPCSpeechBubbleHolder.pivot = originalPivot;
                doSpeechBubbleHolder.pivot = originalPivot;
                break;
            case 90: // Looking east
                playerSpeechBubbleHolder.pivot = movePivotRight;
                nPCSpeechBubbleHolder.pivot = movePivotLeft;
                doSpeechBubbleHolder.pivot = movePivotRight;
                break;
            case 180: // Looking south
                playerSpeechBubbleHolder.pivot = originalPivot;
                nPCSpeechBubbleHolder.pivot = originalPivot;
                doSpeechBubbleHolder.pivot = originalPivot;
                break;
            case 270: //Looking west
                playerSpeechBubbleHolder.pivot = movePivotLeft;
                nPCSpeechBubbleHolder.pivot = movePivotRight;
                doSpeechBubbleHolder.pivot = movePivotLeft;
                break;
            default:
                //Debug.Log("Unrecognizable direction");
                break;
        }
    }

    /** Refined Code ENDS here **/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // Left Off HERE!!!

    // Sets the player's dialogue bubble to follow the player
    private void SetPlayerBubblePosition()
    {
        Vector3 playerBubblePos = dialogueCamera.WorldToScreenPoint(playerDialogueCheck.transform.position);

        if (playerDialogueBubble.transform.position != playerBubblePos)
            playerDialogueBubble.transform.position = playerBubblePos;
    }

    // Sets the npc's dialogue bubble to follow the player
    private void SetNPCBubblePosition()
    {
        Vector3 nPCBubblePos = dialogueCamera.WorldToScreenPoint(nPCDialogueCheck.transform.position);

        if (nPCDialogueBubble.transform.position != nPCBubblePos)
            nPCDialogueBubble.transform.position = nPCBubblePos;
    }

    // Sets the dialogue bubble to follow the player
    private void SetDialogueBubblePosition()
    {
        Vector3 dialogueBubblePos = dialogueCamera.WorldToScreenPoint(playerDialogueCheck.transform.position);

        if (dialogueOptionsBubble.transform.position != dialogueBubblePos)
            dialogueOptionsBubble.transform.position = dialogueBubblePos;
    }

    // Sets the alert bubble to follow the player
    private void SetAlertBubblePosition()
    {
        Vector3 alertBubblePos = dialogueCamera.WorldToScreenPoint(playerDialogueCheck.transform.position);

        if (playerAlertBubble.transform.position != alertBubblePos)
            playerAlertBubble.transform.position = alertBubblePos;
    }   

    // Checks to see if the dialogue options bubble goes outside of the screen and re-adjusts it accordingly
    private void AdjustDialogueOptionsBubbleCheck()
    {
        DialogueOptionsBubbleValuesCheck();

        if (dialogueOptionsBubble.activeSelf)
        {
            dosph_TopEdgePosY = ((dosph_LocalPosY + dosph_Height) * dialogueBubbleScale) + dob_LocalPosY;

            if (dosph_TopEdgePosY > screenTopEdgePosY)
            {
                if (dialogueOptionsBubbleAnim.gameObject.transform.localPosition != new Vector3(0, screenTopEdgePosY - dosph_TopEdgePosY, 0))
                    dialogueOptionsBubbleAnim.gameObject.transform.localPosition = new Vector3(0, screenTopEdgePosY - dosph_TopEdgePosY, 0);
            }

            if (dosph_TopEdgePosY < screenTopEdgePosY && !hasSetBubbleDefaultPosY)
            {
                if (dialogueOptionsBubbleAnim.gameObject.transform.localPosition != bubbleAnimOrigPos)
                    dialogueOptionsBubbleAnim.gameObject.transform.localPosition = bubbleAnimOrigPos;

                hasSetBubbleDefaultPosY = true;
            }

            if (playerDirection == right)
            {
                dosph_LeftEdgePosX = dob_LocalPosX + ((dosph_LocalPosX - dosph_Width) * dialogueBubbleScale);

                if (dosph_LeftEdgePosX < screenLeftEdgePosX02)
                {
                    if (doSpeechBubbleHolder.localPosition != new Vector3(doSpeechBubbleHolder.localPosition.x + ((screenLeftEdgePosX02 - dosph_LeftEdgePosX) / dialogueBubbleScale), 78, 0))
                        doSpeechBubbleHolder.localPosition = new Vector3(doSpeechBubbleHolder.localPosition.x + ((screenLeftEdgePosX02 - dosph_LeftEdgePosX) / dialogueBubbleScale), 78, 0);
                }

                if (dosph_LeftEdgePosX > screenLeftEdgePosX02 && !hasSetBubbleDefaultPosX)
                {
                    if (doSpeechBubbleHolder.localPosition != moveBubbleRight)
                        doSpeechBubbleHolder.localPosition = moveBubbleRight;

                    hasSetBubbleDefaultPosX = true;
                }
            }

            else if (playerDirection == left)
            {
                dosph_RightEdgePosX = dob_LocalPosX + ((dosph_LocalPosX + dosph_Width) * dialogueBubbleScale);

                if (dosph_RightEdgePosX > screenRightEdgePosX02)
                {
                    if (doSpeechBubbleHolder.localPosition != new Vector3(doSpeechBubbleHolder.localPosition.x - ((dosph_RightEdgePosX - screenRightEdgePosX02) / dialogueBubbleScale), 78, 0))
                        doSpeechBubbleHolder.localPosition = new Vector3(doSpeechBubbleHolder.localPosition.x - ((dosph_RightEdgePosX - screenRightEdgePosX02) / dialogueBubbleScale), 78, 0);
                }

                if (dosph_RightEdgePosX < screenRightEdgePosX02 && !hasSetBubbleDefaultPosX)
                {
                    if (doSpeechBubbleHolder.localPosition != moveBubbleLeft)
                        doSpeechBubbleHolder.localPosition = moveBubbleLeft;

                    hasSetBubbleDefaultPosX = true;
                }
            }

            else if (playerDirection == up || playerDirection == down)
            {
                dosph_LeftEdgePosX = dob_LocalPosX + ((dosph_LocalPosX - dosph_Width * 0.5f) * dialogueBubbleScale);
                dosph_RightEdgePosX = dob_LocalPosX + ((dosph_LocalPosX + dosph_Width * 0.5f) * dialogueBubbleScale);

                if (dosph_LeftEdgePosX < screenLeftEdgePosX02)
                {
                    if (doSpeechBubbleHolder.localPosition != new Vector3(doSpeechBubbleHolder.localPosition.x + ((screenLeftEdgePosX02 - dosph_LeftEdgePosX) / dialogueBubbleScale), 78, 0))
                        doSpeechBubbleHolder.localPosition = new Vector3(doSpeechBubbleHolder.localPosition.x + ((screenLeftEdgePosX02 - dosph_LeftEdgePosX) / dialogueBubbleScale), 78, 0);
                }

                if (dosph_RightEdgePosX > screenRightEdgePosX02)
                {
                    if (doSpeechBubbleHolder.localPosition != new Vector3(doSpeechBubbleHolder.localPosition.x - ((dosph_RightEdgePosX - screenRightEdgePosX02) / dialogueBubbleScale), 78, 0))
                        doSpeechBubbleHolder.localPosition = new Vector3(doSpeechBubbleHolder.localPosition.x - ((dosph_RightEdgePosX - screenRightEdgePosX02) / dialogueBubbleScale), 78, 0);
                }

                if (dosph_LeftEdgePosX >= screenLeftEdgePosX02 && dosph_RightEdgePosX <= screenRightEdgePosX02 && !hasSetBubbleDefaultPosX)
                {
                    if (doSpeechBubbleHolder.localPosition != bubbleHolderOrigPos)
                        doSpeechBubbleHolder.localPosition = bubbleHolderOrigPos;

                    hasSetBubbleDefaultPosX = true;
                }
            }

        }
    }

    // Checks to see if the player's dialogue bubble goes outside of the screen and re-adjusts it accordingly
    private void AdjustDialogueBubbleCheckPlayer()
    {
        PlayerBubbleValuesCheck();

        if (playerDialogueBubble.activeSelf)
        {
            psbh_TopEdgePosY = ((psbh_LocalPoxY + psbh_Height) * dialogueBubbleScale) + pdb_LocalPosY;

            if (psbh_TopEdgePosY > screenTopEdgePosY)
            {
                if (playerBubbleAnim.gameObject.transform.localPosition != new Vector3(0, screenTopEdgePosY - psbh_TopEdgePosY, 0))
                   playerBubbleAnim.gameObject.transform.localPosition = new Vector3(0, screenTopEdgePosY - psbh_TopEdgePosY, 0);
            }

            if (psbh_TopEdgePosY < screenTopEdgePosY && !hasSetBubbleDefaultPosY)
            {
                if (playerBubbleAnim.gameObject.transform.localPosition != bubbleAnimOrigPos)
                    playerBubbleAnim.gameObject.transform.localPosition = bubbleAnimOrigPos;

                hasSetBubbleDefaultPosY = true;
            }

            if (playerDirection == right)
            {
                psbh_LeftEdgePosX = pdb_LocalPosX + ((psbh_LocalPosX - psbh_Width) * dialogueBubbleScale);

                if (psbh_LeftEdgePosX < screenLeftEdgePosX)
                {
                    if (playerSpeechBubbleHolder.localPosition != new Vector3(playerSpeechBubbleHolder.localPosition.x + ((screenLeftEdgePosX - psbh_LeftEdgePosX) / dialogueBubbleScale), 78, 0))
                        playerSpeechBubbleHolder.localPosition = new Vector3(playerSpeechBubbleHolder.localPosition.x + ((screenLeftEdgePosX - psbh_LeftEdgePosX) / dialogueBubbleScale), 78, 0);
                        //playerSpeechBubbleHolder.localPosition = new Vector3(playerSpeechBubbleHolder.localPosition.x + ((-playerLeftEdgePosX - 960) / 0.8f), 78, 0);
                }

                if (psbh_LeftEdgePosX > screenLeftEdgePosX && !hasSetBubbleDefaultPosX)
                {
                    if (playerSpeechBubbleHolder.localPosition != moveBubbleRight)
                        playerSpeechBubbleHolder.localPosition = moveBubbleRight;

                    hasSetBubbleDefaultPosX = true;
                }
            }

            else if (playerDirection == left)
            {
                psbh_RightEdgePosX = pdb_LocalPosX + ((psbh_LocalPosX + psbh_Width) * dialogueBubbleScale);

                if (psbh_RightEdgePosX > screenRightEdgePosX)
                {
                    if (playerSpeechBubbleHolder.localPosition != new Vector3(playerSpeechBubbleHolder.localPosition.x - ((psbh_RightEdgePosX - screenRightEdgePosX) / dialogueBubbleScale), 78, 0))
                        playerSpeechBubbleHolder.localPosition = new Vector3(playerSpeechBubbleHolder.localPosition.x - ((psbh_RightEdgePosX - screenRightEdgePosX) / dialogueBubbleScale), 78, 0);
                        //playerSpeechBubbleHolder.localPosition = new Vector3(playerSpeechBubbleHolder.localPosition.x - ((rightEdgePosX - 960) / 0.8f), 78, 0);
                }

                if (psbh_RightEdgePosX < screenRightEdgePosX && !hasSetBubbleDefaultPosX)
                {
                    if (playerSpeechBubbleHolder.localPosition != moveBubbleLeft)
                        playerSpeechBubbleHolder.localPosition = moveBubbleLeft;

                    hasSetBubbleDefaultPosX = true;
                }
            }

            else if (playerDirection == up || playerDirection == down)
            {
                psbh_LeftEdgePosX = pdb_LocalPosX + ((psbh_LocalPosX - psbh_Width * 0.5f) * dialogueBubbleScale);
                psbh_RightEdgePosX = pdb_LocalPosX + ((psbh_LocalPosX + psbh_Width * 0.5f) * dialogueBubbleScale);

                if (psbh_LeftEdgePosX < screenLeftEdgePosX)
                {
                    if (playerSpeechBubbleHolder.localPosition != new Vector3(playerSpeechBubbleHolder.localPosition.x + ((screenLeftEdgePosX - psbh_LeftEdgePosX) / dialogueBubbleScale), 78, 0))
                        playerSpeechBubbleHolder.localPosition = new Vector3(playerSpeechBubbleHolder.localPosition.x + ((screenLeftEdgePosX - psbh_LeftEdgePosX) / dialogueBubbleScale), 78, 0);
                }

                if (psbh_RightEdgePosX > screenRightEdgePosX)
                {
                    if (playerSpeechBubbleHolder.localPosition != new Vector3(playerSpeechBubbleHolder.localPosition.x - ((psbh_RightEdgePosX - screenRightEdgePosX) / dialogueBubbleScale), 78, 0))
                        playerSpeechBubbleHolder.localPosition = new Vector3(playerSpeechBubbleHolder.localPosition.x - ((psbh_RightEdgePosX - screenRightEdgePosX) / dialogueBubbleScale), 78, 0);
                }

                if (psbh_LeftEdgePosX > screenLeftEdgePosX && psbh_RightEdgePosX < screenRightEdgePosX && !hasSetBubbleDefaultPosX)
                {
                    if (playerSpeechBubbleHolder.localPosition != bubbleHolderOrigPos)
                        playerSpeechBubbleHolder.localPosition = bubbleHolderOrigPos;

                    hasSetBubbleDefaultPosX = true;
                }
            }

        }
    }

    // Checks to see if the npc's dialogue bubble goes outside of the screen and re-adjusts it accordingly
    private void AdjustDialogueBubbleCheckNPC()
    {
        NPCBubbleValuesCheck();

        if (nPCDialogueBubble.activeSelf)
        {
            npcsbh_TopEdgePosY = ((npcsbh_LocalPosY + npcsbh_Height) * dialogueBubbleScale) + npcdb_LocalPosY;

            if (npcsbh_TopEdgePosY > screenTopEdgePosY)
            {
                if (nPCBubbleAnim.gameObject.transform.localPosition != new Vector3(0, screenTopEdgePosY - npcsbh_TopEdgePosY, 0))
                    nPCBubbleAnim.gameObject.transform.localPosition = new Vector3(0, screenTopEdgePosY - npcsbh_TopEdgePosY, 0);
            }

            if (npcsbh_TopEdgePosY < screenTopEdgePosY && !hasSetBubbleDefaultPosY)
            {
                if (nPCBubbleAnim.gameObject.transform.localPosition != bubbleAnimOrigPos)
                    nPCBubbleAnim.gameObject.transform.localPosition = bubbleAnimOrigPos;

                hasSetBubbleDefaultPosY = true;
            }

            if (playerDirection == right)
            {
                npcsbh_RightEdgePosX = npcdb_LocalPosX + ((npcsbh_LocalPosX + npcsbh_Width) * dialogueBubbleScale);

                if (npcsbh_RightEdgePosX > screenRightEdgePosX)
                {
                    if (nPCSpeechBubbleHolder.localPosition != new Vector3(nPCSpeechBubbleHolder.localPosition.x - ((npcsbh_RightEdgePosX - screenRightEdgePosX) / dialogueBubbleScale), 78, 0))
                        nPCSpeechBubbleHolder.localPosition = new Vector3(nPCSpeechBubbleHolder.localPosition.x - ((npcsbh_RightEdgePosX - screenRightEdgePosX) / dialogueBubbleScale), 78, 0);
                }

                if (npcsbh_RightEdgePosX < screenRightEdgePosX && !hasSetBubbleDefaultPosX)
                {
                    if (nPCSpeechBubbleHolder.localPosition != moveBubbleLeft)
                        nPCSpeechBubbleHolder.localPosition = moveBubbleLeft;

                    hasSetBubbleDefaultPosX = true;
                }
            }

            else if (playerDirection == left)
            {
                npcsbh_LeftEdgePosX = npcdb_LocalPosX + ((npcsbh_LocalPosX - npcsbh_Width) * dialogueBubbleScale);

                if (npcsbh_LeftEdgePosX < screenLeftEdgePosX)
                {
                    if (nPCSpeechBubbleHolder.localPosition != new Vector3(nPCSpeechBubbleHolder.localPosition.x + ((screenLeftEdgePosX - npcsbh_LeftEdgePosX) / dialogueBubbleScale), 78, 0))
                        nPCSpeechBubbleHolder.localPosition = new Vector3(nPCSpeechBubbleHolder.localPosition.x + ((screenLeftEdgePosX - npcsbh_LeftEdgePosX) / dialogueBubbleScale), 78, 0);
                }

                if (npcsbh_LeftEdgePosX > screenLeftEdgePosX && !hasSetBubbleDefaultPosX)
                {
                    if (nPCSpeechBubbleHolder.localPosition != moveBubbleRight)
                        nPCSpeechBubbleHolder.localPosition = moveBubbleRight;

                    hasSetBubbleDefaultPosX = true;
                }
            }

            else if (playerDirection == up || playerDirection == down)
            {
                npcsbh_LeftEdgePosX = npcdb_LocalPosX + ((npcsbh_LocalPosX - npcsbh_Width * 0.5f) * dialogueBubbleScale);
                npcsbh_RightEdgePosX = npcdb_LocalPosX + ((npcsbh_LocalPosX + npcsbh_Width * 0.5f) * dialogueBubbleScale);

                if (npcsbh_LeftEdgePosX < screenLeftEdgePosX)
                {
                    if (nPCSpeechBubbleHolder.localPosition != new Vector3(nPCSpeechBubbleHolder.localPosition.x + ((screenLeftEdgePosX - psbh_LeftEdgePosX) / dialogueBubbleScale), 78, 0))
                        nPCSpeechBubbleHolder.localPosition = new Vector3(nPCSpeechBubbleHolder.localPosition.x + ((screenLeftEdgePosX - psbh_LeftEdgePosX) / dialogueBubbleScale), 78, 0);
                }

                if (npcsbh_RightEdgePosX > screenRightEdgePosX)
                {
                    if (nPCSpeechBubbleHolder.localPosition != new Vector3(nPCSpeechBubbleHolder.localPosition.x - ((npcsbh_RightEdgePosX - screenRightEdgePosX) / dialogueBubbleScale), 78, 0))
                        nPCSpeechBubbleHolder.localPosition = new Vector3(nPCSpeechBubbleHolder.localPosition.x - ((npcsbh_RightEdgePosX - screenRightEdgePosX) / dialogueBubbleScale), 78, 0);
                }

                if (npcsbh_LeftEdgePosX > screenLeftEdgePosX && npcsbh_RightEdgePosX < screenRightEdgePosX && !hasSetBubbleDefaultPosX)
                {
                    if (nPCSpeechBubbleHolder.localPosition != bubbleHolderOrigPos)
                        nPCSpeechBubbleHolder.localPosition = bubbleHolderOrigPos;

                    hasSetBubbleDefaultPosX = true;
                }
            }

        }
    }

    // Updates the floats for the dialogue options bubble when they change
    private void DialogueOptionsBubbleValuesCheck()
    {
        if (dosph_Width != doSpeechBubbleHolder.rect.width)
            dosph_Width = doSpeechBubbleHolder.rect.width;

        if (dosph_Height != doSpeechBubbleHolder.rect.height)
            dosph_Height = doSpeechBubbleHolder.rect.height;

        if (dosph_LocalPosX != doSpeechBubbleHolder.localPosition.x)
            dosph_LocalPosX = doSpeechBubbleHolder.localPosition.x;

        if (dosph_LocalPosY != doSpeechBubbleHolder.localPosition.y)
            dosph_LocalPosY = doSpeechBubbleHolder.localPosition.y;

        if (dob_LocalPosX != dialogueOptionsBubble.transform.localPosition.x)
            dob_LocalPosX = dialogueOptionsBubble.transform.localPosition.x;

        if (dob_LocalPosY != dialogueOptionsBubble.transform.localPosition.y)
            dob_LocalPosY = dialogueOptionsBubble.transform.localPosition.y;
    }

    // Updates the floats for the player's bubble when they change
    private void PlayerBubbleValuesCheck()
    {
        if (psbh_Height != playerSpeechBubbleHolder.rect.height)
            psbh_Height = playerSpeechBubbleHolder.rect.height;

        if (psbh_Width != playerSpeechBubbleHolder.rect.width)
            psbh_Width = playerSpeechBubbleHolder.rect.width;

        if (psbh_LocalPosX != playerSpeechBubbleHolder.localPosition.x)
            psbh_LocalPosX = playerSpeechBubbleHolder.localPosition.x;

        if (psbh_LocalPoxY != playerSpeechBubbleHolder.localPosition.y)
            psbh_LocalPoxY = playerSpeechBubbleHolder.localPosition.y;

        if (pdb_LocalPosX != playerDialogueBubble.transform.localPosition.x)
            pdb_LocalPosX = playerDialogueBubble.transform.localPosition.x;

        if (pdb_LocalPosY != playerDialogueBubble.transform.localPosition.y)
            pdb_LocalPosY = playerDialogueBubble.transform.localPosition.y;
    }

    // Updates the floats for the npc's bubble when they change
    private void NPCBubbleValuesCheck()
    {
        if (npcsbh_Width != nPCSpeechBubbleHolder.rect.width)
            npcsbh_Width = nPCSpeechBubbleHolder.rect.width;

        if (npcsbh_Height != nPCSpeechBubbleHolder.rect.height)
            npcsbh_Height = nPCSpeechBubbleHolder.rect.height;

        if (npcsbh_LocalPosX != nPCSpeechBubbleHolder.localPosition.x)
            npcsbh_LocalPosX = nPCSpeechBubbleHolder.localPosition.x;

        if (npcsbh_LocalPosY != nPCSpeechBubbleHolder.localPosition.y)
            npcsbh_LocalPosY = nPCSpeechBubbleHolder.localPosition.y;

        if (npcdb_LocalPosX != nPCDialogueBubble.transform.localPosition.x)
            npcdb_LocalPosX = nPCDialogueBubble.transform.localPosition.x;

        if (npcdb_LocalPosY != nPCDialogueBubble.transform.localPosition.y)
            npcdb_LocalPosY = nPCDialogueBubble.transform.localPosition.y;
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
        dialogueArrowScript = FindObjectOfType<DialogueArrow>();
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
            string chidName01 = child.name;

            if (chidName01 == "DialogueOptionsBubble")
            {
                dialogueOptionsBubble = child;
                dialogueOptionsBubbleAnim = dialogueOptionsBubble.GetComponentInChildren<Animator>();

                for (int j = 0; j < dialogueOptionsBubbleAnim.transform.childCount; j++)
                {
                    RectTransform rectTransform = dialogueOptionsBubbleAnim.transform.GetChild(j).GetComponent<RectTransform>();

                    if (rectTransform.name == "SpeechBubbleHolder")
                    {
                        doSpeechBubbleHolder = rectTransform;
                        GameObject dialogueOptionsHolder = doSpeechBubbleHolder.GetComponentInChildren<Image>().gameObject;

                        for (int k = 0; k < dialogueOptionsHolder.transform.childCount; k++)
                        {
                            GameObject child02 = dialogueOptionsHolder.transform.GetChild(k).gameObject;
                            doGameObjects.Add(child02);
                            doTextComponenets.Add(child02.GetComponent<TextMeshProUGUI>());
                        }

                        dialogueArrowHolder = doGameObjects[0].transform.GetChild(0).gameObject;
                        dialogueArrowScript.SetDialogueArrow(dialogueArrowHolder);
                    }                    
                }
            }             
            if (chidName01 == "PlayerDialogueBubble")
            {
                playerDialogueBubble = child;
                playerBubbleAnim = playerDialogueBubble.GetComponentInChildren<Animator>();

                for (int g = 0; g < playerBubbleAnim.transform.childCount; g++)
                {
                    RectTransform rectTransform = playerBubbleAnim.transform.GetChild(g).GetComponent<RectTransform>();

                    if (rectTransform.name == "SpeechBubbleHolder")
                        playerSpeechBubbleHolder = rectTransform;
                }
            }
            if (chidName01 == "NPCDialogueBubble")
            {
                nPCDialogueBubble = child;
                nPCBubbleAnim = nPCDialogueBubble.GetComponentInChildren<Animator>();

                for (int f = 0; f < nPCBubbleAnim.transform.childCount; f++)
                {
                    RectTransform rectTransform = nPCBubbleAnim.transform.GetChild(f).GetComponent<RectTransform>();

                    if (rectTransform.name == "SpeechBubbleHolder")
                        nPCSpeechBubbleHolder = rectTransform;
                }
            }              
            if (chidName01 == "PlayerAlertBubble")
                playerAlertBubble = child;
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

        for (int i = 0; i < playerBubbleColorText.transform.childCount; i++)
        {
            GameObject child = playerBubbleColorText.transform.GetChild(i).gameObject;

            if (child.name == "ForegroundText")
                playerForegroundText = child.GetComponent<TextMeshProUGUI>();
        }

        for (int i = 0; i < nPCBubbleColorText.transform.childCount; i++)
        {
            GameObject child = nPCBubbleColorText.transform.GetChild(i).gameObject;

            if (child.name == "ForegroundText")
                nPCForegroundText = child.GetComponent<TextMeshProUGUI>();
        }

        for (int i = 0; i < nPCScript.transform.childCount; i++)
        {
            GameObject child = nPCScript.transform.GetChild(i).gameObject;

            if (child.name == "DialogueCheck")
                nPCDialogueCheck = child;                      
        }

        charNoiseSFX = audioManagerScript.charNoiseAS;
        typingSpeed = gameManagerScript.typingSpeed;
        originalTypingSpeed = gameManagerScript.typingSpeed;
    }

}
