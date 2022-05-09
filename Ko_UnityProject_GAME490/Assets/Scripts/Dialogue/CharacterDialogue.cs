using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.IO;

public class CharacterDialogue : MonoBehaviour
{
    private string nPCName;
    private int playerIndex;
    private int nPCIndex;
    private int dialogueOptionsIndex;

    [Header("Bools")]
    public bool isPlayerSpeaking;
    public bool canPlayBubbleAnim = false;
    public bool canStartDialogue = true;
    public bool hasStartedPlayerDialogue = false;
    public bool hasStartedNPCDialogue = false;
    public bool isInteractingWithNPC = false;
    public bool isInteractingWithArtifact = false;
    public bool hasTransitionedToArtifactView = false;
    public bool hasSelectedDialogueOption = false;
    private bool canCheckBubbleBounds = false;
    private bool hasSetBubbleDefaultPosX = false;
    private bool hasSetBubbleDefaultPosY = false;
    private bool hasPlayedPopUpSFX = false;
    private bool hasSetDialogueBars = false;
    private bool hasSetPivot = false;
    private bool hasMovedDialogueArrow = false;
    private bool canMoveDialogueArrow = false;
    private bool canSpeedUpDialogue = false;

    //public float sentenceLength;
    private float typingSpeed; // 0.03f
    private float originalTypingSpeed; // 0.03f
    private float speechBubbleAnimationDelay;
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

    [Header("GameObjects")]
    private GameObject dialogueArrowHolder;
    private GameObject dialogueOptionOne;
    private GameObject dialogueOptionTwo;
    private GameObject dialogueOptionThree;

    private GameObject playerDialgueBubble;
    private GameObject nPCDialgueBubble;
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

    private AudioSource charNoiseSFX;
    private Animator playerBubbleAnim;
    private Animator nPCBubbleAnim;
    private Animator dialogueOptionsBubbleAnim;

    [Header("TextMeshPro")]
    // Note: the white text is used to "calculate" the size of the bubble (to fit the text), dialogue text will overlay white text once size is found
    public TextMeshProUGUI playerBubbleColorText; 
    public TextMeshProUGUI nPCBubbleColorText;
    private TextMeshProUGUI playerForegroundText;
    private TextMeshProUGUI nPCForegroundText;
    private TextMeshProUGUI optionOneText;
    private TextMeshProUGUI optionTwoText;
    private TextMeshProUGUI optionThreeText;

    private Color selectedTextColor = new Color32(128, 160, 198, 255);
    private Color unselectedTextColor = Color.gray;
    private Vector3 dialogueArrowDefaultPos = new Vector3(5, 5, 0);

    [Header("Dialogue Setences")]
    private string[] playerDialogueSentences;
    private string[] nPCDialogueSentences;
    private string[] dialogueQuestions;

    private Artifact artifactScript;
    private NonPlayerCharacter nPCScript;
    private FidgetController nPCFidgetScript;
    private FidgetController playerFidgetScript;
    private TileMovementController playerScript;
    private PauseMenu pauseMenuScript;
    private CameraController cameraScript;
    private GameManager gameManagerScript;
    private AudioManager audioManagerScript;
    private BlackBars blackBarsScript;
    private GameHUD gameHUDScript;
    private DialogueArrow dialogueArrowScript;
    private TransitionFade transitionFadeScript;

    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        //screenRightEdgePosX = pauseMenuScript.GetComponent<RectTransform>().rect.width / 2;     
        //screenLeftEdgePosX = -screenRightEdgePosX;
        //dialogueBubbleScale = playerBubbleAnim.gameObject.transform.localScale.x;

        continueButton.SetActive(false);
        nPCDialgueBubble.SetActive(false);
        playerDialgueBubble.SetActive(false);
        dialogueOptionsBubble.SetActive(false);
        playerAlertBubble.SetActive(false);

        dialogueOptionsIndex = 0;
        SetVectors();       
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
        DialogueArrowCheck();

        AnimBubbleDelayCheck();
        ContinueButtonCheck();

        //sentenceLength = whitePlayerText.text.Length;

        /*** For Debugging purposes ***/
        /*if (Input.GetKeyDown(KeyCode.R) && canStartDialogue)
        {
            StopCoroutine("TypePlayerDialogue");
            StopCoroutine("TypeNPCDialogue");

            playerDialgueBubble.SetActive(false);
            nPCDialgueBubble.SetActive(false);
            dialogueOptionsBubble.SetActive(false);
            continueButton.SetActive(false);
            nPCScript.ResetRotationNPC();

            EmptyTextComponents();
            playerIndex = 0;
            nPCIndex = 0;

            isPlayerSpeaking = true;
            hasStartedDialoguePlayer = false;
            hasStartedDialogueNPC = false;
            canPlayBubbleAnim = false;
            canCheckBubbleBounds = false;
            hasPlayedPopUpSFX = false;
            hasSetPivot = false;
            hasSetBubbleDefaultPosX = false;
            hasSetBubbleDefaultPosY = false;
            isInteractingWithNPC = false;
        }
        /*** End Debugging ***/
    }

    void FixedUpdate()
    {       
        SetPlayerBubblePosition();
        SetNPCBubblePosition();
        SetDialogueBubblePosition();
        SetAlertBubblePosition();
    }

    // Returns or sets the value of typingSpeed
    public float TypingSpeed
    {
        get
        {
            return typingSpeed;
        }
        set
        {
            typingSpeed = value;
        }
    }

    // Assigns a new game object to the nPCDialogueCheck
    public void UpdateDialogueCheckForNPC(GameObject newGameObject)
    {
        nPCDialogueCheck = newGameObject;
    }

    // Assigns a new script to the npc script
    public void UpdateScriptForNPC(NonPlayerCharacter newScript)
    {
        nPCScript = newScript;
    }

    // Assigns a new script to the fidget controller script (NPC)
    public void UpdateFidgetScriptForNPC(FidgetController newScript)
    {
        nPCFidgetScript = newScript;
    }

    // Assigns a new string to the nPCName
    public void UpdateNonPlayerCharacterName(string newString)
    {
        nPCName = newString;
    }

    // Assigns a new script to the artifact Script
    public void UpdateArtifactScript(Artifact newScript) => artifactScript = newScript;

    // Starts the dialogue with an npc
    public void StartDialogue()
    {
        if (canStartDialogue)
        {
            StartCoroutine(StartDialogueDelay());     
            canStartDialogue = false;    
        }
    }

    // Sets the alert bubble active
    public void SetAlertBubbleActive()
    {
        if (!playerAlertBubble.activeSelf)
            playerAlertBubble.SetActive(true);
    }

    // Sets the alert bubble inactive
    public void SetAlertBubbleInactive()
    {
        if (playerAlertBubble.activeSelf)
            playerAlertBubble.SetActive(false);
    }

    /*** Reading and setting dialogue text files START HERE ***/
    // Sets an array for the player dialogue (text assets) 
    public void setPlayerDialogue(TextAsset playerDialogue)
    {
        setPlayerDialogueArray(readTextFile(playerDialogue));
    }

    // Sets an array for the npc dialogue (text assets) 
    public void setNPCDialogue(TextAsset nPCDialogue)
    {
        setNPCDialogueArray(readTextFile(nPCDialogue));
    }

    // Sets an array for the dialogue questions (text asset) 
    public void setDialogueQuestions(TextAsset questions)
    {
        setDialogueQuestionsArray(readTextFile(questions));
    }

    // Sets the player's dialogue array - can be changed with new dialogue
    private void setPlayerDialogueArray(string[] playerDialogueArray)
    {
        playerDialogueSentences = playerDialogueArray;
    }

    // Sets the player's dialogue array - can be changed with new dialogue
    private void setNPCDialogueArray(string[] npcDialogueArray)
    {
        nPCDialogueSentences = npcDialogueArray;
    }

    // Sets the an array for the dialogue questions - can be changed with new questions
    private void setDialogueQuestionsArray(string[] dialogueQuestionsArray)
    {
        dialogueQuestions = dialogueQuestionsArray;
    }

    // Reads the text file
    private string[] readTextFile(TextAsset textFile)
    {
        return textFile.text.Split("\n"[0]);
    }
    /*** Reading and setting dialogue text files END HERE ***/


    // Checks for when the player can load the next dialogue sentence
    private void ContinueButtonCheck()
    {
        if (!transitionFadeScript.IsChangingScenes && !pauseMenuScript.IsPaused && pauseMenuScript.CanPause && pauseMenuScript.enabled)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (continueButton.activeSelf)
                {
                    /*if (hasTransitionedToArtifactView)
                    {
                        artifactScript.StopInspectingArtifact();
                        continueButton.SetActive(false);
                        hasTransitionedToArtifactView = false;
                    }*/

                    if (hasStartedPlayerDialogue || hasStartedNPCDialogue)
                    {
                        ContinueDialogueCheck();
                        continueButton.SetActive(false);
                    }
                }

                else if (!continueButton.activeSelf && typingSpeed > originalTypingSpeed / 2 && canSpeedUpDialogue)
                    typingSpeed /= 2;
            }
        }
    }

    // Checks to see if the dialogue can continue
    private void ContinueDialogueCheck()
    {
        if (!transitionFadeScript.IsChangingScenes)
        {
            NextDialogueSentenceCheck();
            hasSetBubbleDefaultPosX = false;
            hasSetBubbleDefaultPosY = false;
            //continueButton.SetActive(false);
        }
    }

    // Checks if the player or the npc will speak next
    private void WhoSpeaksNextCheck()
    {
        if (isPlayerSpeaking)
        {
            if (playerDialogueSentences[playerIndex + 1].Contains("SWITCH"))
            {
                //Debug.Log("detected SWITCH for player");
                playerIndex++;
                
                if (nPCDialogueSentences[nPCIndex + 1].Contains("END DIALOGUE"))
                    hasPlayedPopUpSFX = true;
                else
                    hasPlayedPopUpSFX = false;

                isPlayerSpeaking = false;
            }
            if (playerIndex < playerDialogueSentences.Length - 1 && playerDialogueSentences[playerIndex + 1].Contains("LOAD DIALOGUE OPTIONS"))
                hasPlayedPopUpSFX = false;
        }
        else if (!isPlayerSpeaking)
        {
            if (nPCDialogueSentences[nPCIndex + 1].Contains("SWITCH"))
            {
                //Debug.Log("detected SWITCH for npc");
                nPCIndex++;           

                if (playerDialogueSentences[playerIndex + 1].Contains("END DIALOGUE"))
                    hasPlayedPopUpSFX = true;

                else 
                    hasPlayedPopUpSFX = false;

                isPlayerSpeaking = true;             
            }
            if (nPCIndex < nPCDialogueSentences.Length - 1 && nPCDialogueSentences[nPCIndex + 1].Contains("LOAD DIALOGUE OPTIONS"))
                hasPlayedPopUpSFX = false;
        }      
    }

    private void WhoSpeaksFirstCheck()
    {
        if (playerDialogueSentences[playerIndex].Contains("SWITCH"))
        {
            playerIndex++;
            isPlayerSpeaking = false;
        }           
        else
            isPlayerSpeaking = true;
    }

    // Checks if the player is speaking first or not
    private void StartDialogueCheck()
    {
        gameManagerScript.CheckForCharacterDialogueDebug();
        originalTypingSpeed = typingSpeed;
        //typingSpeed = originalTypingSpeed;
        canSpeedUpDialogue = false;
        WhoSpeaksFirstCheck();

        if (isPlayerSpeaking)
            StartCoroutine("TypePlayerDialogue");
        else
            StartCoroutine("TypeNPCDialogue");
    }

    // Determines the next dialogue sentence do be played
    private void NextDialogueSentenceCheck()
    {
        gameManagerScript.CheckForCharacterDialogueDebug();
        originalTypingSpeed = typingSpeed;
        //typingSpeed = originalTypingSpeed;
        canSpeedUpDialogue = false;

        if (playerDialogueSentences != null)
        {
            if (playerIndex < playerDialogueSentences.Length - 1 && playerDialogueSentences[playerIndex + 1] != string.Empty && isPlayerSpeaking)
            {
                if (hasStartedPlayerDialogue)
                    playerIndex++;

                if (!playerDialogueSentences[playerIndex].Contains("END DIALOGUE") && !playerDialogueSentences[playerIndex].Contains("LOAD DIALOGUE OPTIONS"))
                    StartCoroutine("TypePlayerDialogue");                   
            }
        }

        if (nPCDialogueSentences != null)
        {
            if (nPCIndex < nPCDialogueSentences.Length - 1 && nPCDialogueSentences[nPCIndex + 1] != string.Empty && !isPlayerSpeaking)
            {
                if (hasStartedNPCDialogue)
                    nPCIndex++;

                if (!nPCDialogueSentences[nPCIndex].Contains("END DIALOGUE") && !playerDialogueSentences[playerIndex].Contains("LOAD DIALOGUE OPTIONS"))
                    StartCoroutine("TypeNPCDialogue");
            }
        }

        EndDialogueCheck();
    }

    // Checks when the dialogue has ended
    private void EndDialogueCheck()
    {
        if (isInteractingWithNPC)
        {
            if (nPCDialogueSentences[nPCIndex].Contains("END DIALOGUE"))
            {
                Debug.Log("Has ended dialogue ");
                StopCoroutine("TypePlayerDialogue");
                StopCoroutine("TypeNPCDialogue");
                StartCoroutine("EndDialogueDelay");
                FadeOutDialogueMusic();
            }
        }

        if (playerDialogueSentences[playerIndex].Contains("LOAD DIALOGUE OPTIONS"))
        {
            Debug.Log("Opened dialogue options");
            StopCoroutine("TypePlayerDialogue");
            StopCoroutine("TypeNPCDialogue");
            OpenDialogueOptionsBubble();
        }

        if (playerDialogueSentences[playerIndex].Contains("END DIALOGUE"))
        {
            Debug.Log("Has ended dialogue");
            StopCoroutine("TypePlayerDialogue");
            StopCoroutine("TypeNPCDialogue");
            StartCoroutine("EndDialogueDelay");
            FadeOutDialogueMusic();
        }       
    }

    // Checks when the dialogue bars should enter and exit
    private void SetDialogueBarsCheck()
    {
        if (!hasSetDialogueBars)
        {
            blackBarsScript.MoveBlackBarsIn();
            hasSetDialogueBars = true;
        }

        else if (hasSetDialogueBars && !hasStartedPlayerDialogue || hasSetDialogueBars && !hasStartedNPCDialogue)
        {
            blackBarsScript.MoveBlackBarsOut();
            hasSetDialogueBars = false;
        }
    }

    // Sets the player's dialogue bubble to follow the player
    private void SetPlayerBubblePosition()
    {
        Vector3 playerBubblePos = dialogueCamera.WorldToScreenPoint(playerDialogueCheck.transform.position);

        if (playerDialgueBubble.transform.position != playerBubblePos)
            playerDialgueBubble.transform.position = playerBubblePos;
    }

    // Sets the npc's dialogue bubble to follow the player
    private void SetNPCBubblePosition()
    {
        Vector3 nPCBubblePos = dialogueCamera.WorldToScreenPoint(nPCDialogueCheck.transform.position);

        if (nPCDialgueBubble.transform.position != nPCBubblePos)
            nPCDialgueBubble.transform.position = nPCBubblePos;
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

    // Checks to see when the dialogue arrow can be move and execute functions
    private void DialogueArrowCheck()
    {
        if (dialogueArrowHolder.activeSelf && dialogueOptionsIndex < dialogueQuestions.Length && canMoveDialogueArrow && !transitionFadeScript.IsChangingScenes && !pauseMenuScript.IsPaused && pauseMenuScript.CanPause)
        {
            if (dialogueOptionsIndex != 0)
            {
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (isInteractingWithArtifact && !artifactScript.HasInspectedArtifact)
                        dialogueOptionsIndex = 0;
                    else
                        dialogueOptionsIndex--;

                    audioManagerScript.PlayButtonClick02SFX();
                    hasMovedDialogueArrow = false;
                }
            }

            if (dialogueOptionsIndex != dialogueQuestions.Length - 1)
            {
                if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (isInteractingWithArtifact && !artifactScript.HasInspectedArtifact)
                        dialogueOptionsIndex = dialogueQuestions.Length - 1;
                    else
                        dialogueOptionsIndex++;

                    audioManagerScript.PlayButtonClick02SFX();
                    hasMovedDialogueArrow = false;
                }
            }


            if (dialogueOptionsIndex == 0)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    if (!isInteractingWithArtifact)
                    {
                        if (!nPCScript.HasPlayedOptionOne)
                        {
                            setPlayerDialogue(nPCScript.playerDialogueFiles[2]);
                            setNPCDialogue(nPCScript.nPCDialogueFiles[2]);
                            nPCScript.HasPlayedOptionOne = true;
                        }
                        else
                        {
                            setPlayerDialogue(nPCScript.playerDialogueFiles[3]);
                            setNPCDialogue(nPCScript.nPCDialogueFiles[3]);
                        }
                        hasSelectedDialogueOption = true;
                        CloseDialogueOptionsBuble();
                        StartDialogueCheck();
                    }
                    else
                    {
                        artifactScript.HasInspectedArtifact = true;
                        artifactScript.InspectArtifact();
                        CloseDialogueOptionsBuble();
                    }                                         
                }

                if (!hasMovedDialogueArrow)
                {
                    dialogueArrowHolder.transform.SetParent(dialogueOptionOne.transform);
                    dialogueArrowHolder.transform.localPosition = dialogueArrowDefaultPos;

                    optionOneText.color = selectedTextColor;
                    optionTwoText.color = unselectedTextColor;
                    optionThreeText.color = unselectedTextColor;

                    hasMovedDialogueArrow = true;
                }
            }

            else if (dialogueOptionsIndex == 1)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    if (!isInteractingWithArtifact)
                    {
                        if (!nPCScript.HasPlayedOptionTwo)
                        {
                            setPlayerDialogue(nPCScript.playerDialogueFiles[4]);
                            setNPCDialogue(nPCScript.nPCDialogueFiles[4]);
                            nPCScript.HasPlayedOptionTwo = true;                        
                        }
                        else
                        {
                            setPlayerDialogue(nPCScript.playerDialogueFiles[5]);
                            setNPCDialogue(nPCScript.nPCDialogueFiles[5]);
                        }
                        hasSelectedDialogueOption = true;
                        CloseDialogueOptionsBuble();
                        StartDialogueCheck();
                    }    
                    else
                    {
                        playerScript.ChangeAnimationState("Interacting");
                        //artifactScript.HasCollectedArtifact = true;
                        artifactScript.CollectArtifact();
                        artifactScript.CloseChest();
                        StartCoroutine("EndDialogueDelay");
                        FadeOutDialogueMusic();
                        CloseDialogueOptionsBuble();
                    }                                        
                }

                if (!hasMovedDialogueArrow)
                {
                    dialogueArrowHolder.transform.SetParent(dialogueOptionTwo.transform);
                    dialogueArrowHolder.transform.localPosition = dialogueArrowDefaultPos;

                    optionOneText.color = unselectedTextColor;
                    optionTwoText.color = selectedTextColor;
                    optionThreeText.color = unselectedTextColor;

                    hasMovedDialogueArrow = true;
                }
            }

            else if (dialogueOptionsIndex == 2)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    if (!isInteractingWithArtifact)
                    {
                        if (hasSelectedDialogueOption)
                        {
                            setPlayerDialogue(nPCScript.playerDialogueFiles[6]);
                            setNPCDialogue(nPCScript.nPCDialogueFiles[6]);
                        }
                        else
                        {
                            setPlayerDialogue(nPCScript.playerDialogueFiles[7]);
                            setNPCDialogue(nPCScript.nPCDialogueFiles[7]);
                        }
                        CloseDialogueOptionsBuble();
                        StartDialogueCheck();
                    }
                    else
                    {
                        playerScript.ChangeAnimationState("Interacting");
                        artifactScript.CloseChest();
                        StartCoroutine("EndDialogueDelay");
                        FadeOutDialogueMusic();
                        CloseDialogueOptionsBuble();
                    }
                }

                if (!hasMovedDialogueArrow)
                {
                    dialogueArrowHolder.transform.SetParent(dialogueOptionThree.transform);
                    dialogueArrowHolder.transform.localPosition = dialogueArrowDefaultPos;

                    optionOneText.color = unselectedTextColor;
                    optionTwoText.color = unselectedTextColor;
                    optionThreeText.color = selectedTextColor;

                    hasMovedDialogueArrow = true;
                }
            }
        }
    }

    // Checks to see if the dialogue options bubble goes outside of the screen and re-adjusts it accordingly
    private void AdjustDialogueOptionsBubbleCheck()
    {
        DialogueOptionsBubbleValuesCheck();

        if (canCheckBubbleBounds && dialogueOptionsBubble.activeSelf)
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

        if (canCheckBubbleBounds && playerDialgueBubble.activeSelf)
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

        if (canCheckBubbleBounds && nPCDialgueBubble.activeSelf)
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

        if (pdb_LocalPosX != playerDialgueBubble.transform.localPosition.x)
            pdb_LocalPosX = playerDialgueBubble.transform.localPosition.x;

        if (pdb_LocalPosY != playerDialgueBubble.transform.localPosition.y)
            pdb_LocalPosY = playerDialgueBubble.transform.localPosition.y;
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

        if (npcdb_LocalPosX != nPCDialgueBubble.transform.localPosition.x)
            npcdb_LocalPosX = nPCDialgueBubble.transform.localPosition.x;

        if (npcdb_LocalPosY != nPCDialgueBubble.transform.localPosition.y)
            npcdb_LocalPosY = nPCDialgueBubble.transform.localPosition.y;
    }

    // Sets the pivot for both the player and npc's dialogue bubble - pivot determines which direction the dialogue bubble extends to
    private void SetDialogueBubblePivot()
    {
        if (!hasSetPivot)
        {
            if (playerDirection == up || playerDirection == down)
            {

                playerSpeechBubbleHolder.pivot = originalPivot;
                nPCSpeechBubbleHolder.pivot = originalPivot;
                doSpeechBubbleHolder.pivot = originalPivot;
            }

            if (playerDirection == left)
            {
                playerSpeechBubbleHolder.pivot = movePivotLeft;
                nPCSpeechBubbleHolder.pivot = movePivotRight;
                doSpeechBubbleHolder.pivot = movePivotLeft;
            }

            if (playerDirection == right)
            {
                playerSpeechBubbleHolder.pivot = movePivotRight;
                nPCSpeechBubbleHolder.pivot = movePivotLeft;
                doSpeechBubbleHolder.pivot = movePivotRight;
            }

            hasSetPivot = true;
        }

    }

    // Sets the dialogue options bubble active
    public void OpenDialogueOptionsBubble()
    {
        StartCoroutine("SetDialogueArrowActiveDelay");

        if (!nPCScript.HasPlayedInitialDialogue && !isInteractingWithArtifact)
            nPCScript.HasPlayedInitialDialogue = true;

        dialogueOptionOne.SetActive(true);
        dialogueOptionThree.SetActive(true);
        optionOneText.text = dialogueQuestions[0];
        optionThreeText.text = dialogueQuestions[2];

        if (isInteractingWithArtifact && artifactScript.HasInspectedArtifact || isInteractingWithNPC)
        {
            dialogueOptionTwo.SetActive(true);
            optionTwoText.text = dialogueQuestions[1];
        }

        SetDialogueBubblePivot();
        playerDialgueBubble.SetActive(false);
        nPCDialgueBubble.SetActive(false);
        dialogueOptionsBubble.SetActive(true);

        SetTextToUnselectedTextColor();
        PlayDialogueBubbleSFXCheck();
        hasSetBubbleDefaultPosX = false;
        hasSetBubbleDefaultPosY = false;
        hasStartedPlayerDialogue = false;
        hasStartedNPCDialogue = false;
        canMoveDialogueArrow = true;
        playerFidgetScript.Fidget();
    }

    // Sets the dialogue options bubble inactive
    private void CloseDialogueOptionsBuble()
    {
        SetTextToUnselectedTextColor();
        dialogueOptionsBubble.SetActive(false);
        dialogueArrowHolder.SetActive(false);
        dialogueOptionOne.SetActive(false);
        dialogueOptionTwo.SetActive(false);
        dialogueOptionThree.SetActive(false);
        hasSetBubbleDefaultPosX = false;
        hasSetBubbleDefaultPosY = false;
        canMoveDialogueArrow = false;
        canPlayBubbleAnim = false;
        hasPlayedPopUpSFX = false;
        hasSetPivot = false;

        playerIndex = 0;
        nPCIndex = 0;

        dialogueArrowScript.StopDialogueArrowAnim();
    }

    // Sets the text for each dialogue option back to its default color
    private void SetTextToUnselectedTextColor()
    {
        optionOneText.color = unselectedTextColor;
        optionTwoText.color = unselectedTextColor;
        optionThreeText.color = unselectedTextColor;
    }

    // Checks if the dialogue pop up sfx can be played - played only once when the dialogue bubble is activated/re-activated
    private void PlayDialogueBubbleSFXCheck()
    {
        if (!hasPlayedPopUpSFX)
        {
            if (isPlayerSpeaking)
            {
                audioManagerScript.PlayDialoguePopUpSFX01();
                hasPlayedPopUpSFX = true;
            }
            else if (!isPlayerSpeaking)
            {
                audioManagerScript.PlayDialoguePopUpSFX02();
                hasPlayedPopUpSFX = true;
            }
        }
    }

    // Determines the speechBubbleAnimationDelay - changes due to different animation lengths
    private void AnimBubbleDelayCheck()
    {
        if (canPlayBubbleAnim)
        {
            if (speechBubbleAnimationDelay != 0.05f)
                speechBubbleAnimationDelay = 0.05f;
        }
        else
        {
            if (speechBubbleAnimationDelay != 0.1f)
                speechBubbleAnimationDelay = 0.1f;
        }
    }

    // Sets values for all vectors
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

    // Clears the text within all text components - before new text is set
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

    // Sets the correct dialogue files for the player, npc, artifacts, and dialogue bubble
    private void SetDialogueFilesCheck()
    {
        if (isInteractingWithNPC)
        {
            nPCScript.SetRotationNPC();
            nPCForegroundText.color = nPCScript.DialogueTextColor;

            if (!nPCScript.HasPlayedInitialDialogue)
            {
                setPlayerDialogue(nPCScript.playerDialogueFiles[0]);
                setNPCDialogue(nPCScript.nPCDialogueFiles[0]);         
            }

            else if (nPCScript.HasPlayedInitialDialogue)
            {
                setPlayerDialogue(nPCScript.playerDialogueFiles[1]);
                setNPCDialogue(nPCScript.nPCDialogueFiles[1]);
            }
        }
    }

    // Sets the dialogue arrow active after specified time
    private IEnumerator SetDialogueArrowActiveDelay()
    {
        yield return new WaitForSeconds(0.25f);
        hasMovedDialogueArrow = false;
        dialogueArrowHolder.SetActive(true);
        dialogueArrowScript.PlayDialogueArrowAnim();
        audioManagerScript.PlayButtonClick02SFX();

        if (dialogueOptionsIndex == 0)
            optionOneText.color = selectedTextColor;
    }

    // Starts the dialogue after a delay
    private IEnumerator StartDialogueDelay()
    {
        SetDialogueFilesCheck();
        FadeInDialogueMusic();
        playerAlertBubble.SetActive(false);
        canCheckBubbleBounds = true;
        cameraScript.LerpToDialogueView();

        SetDialogueBarsCheck();
        gameHUDScript.TurnOffHUD();
        playerScript.SetPlayerBoolsFalse();

        yield return new WaitForSeconds(0.5f);
        StartDialogueCheck();
    }

    // Ends the dialogue (you can interact with npc again after a small delay)
    private IEnumerator EndDialogueDelay()
    {
        dialogueArrowHolder.transform.SetParent(dialogueOptionOne.transform);
        dialogueArrowHolder.transform.localPosition = dialogueArrowDefaultPos;

        dialogueOptionsIndex = 0;
        playerIndex = 0;
        nPCIndex = 0;

        playerDialgueBubble.SetActive(false);
        nPCDialgueBubble.SetActive(false);
        hasStartedPlayerDialogue = false;
        hasStartedNPCDialogue = false;
        canPlayBubbleAnim = false;
        canStartDialogue = false;
        hasSetPivot = false;
        hasSetBubbleDefaultPosX = false;
        hasSetBubbleDefaultPosY = false;
        isInteractingWithNPC = false;
        isInteractingWithArtifact = false;
        hasSelectedDialogueOption = false;
        cameraScript.LerpToPuzzleView();

        EmptyTextComponents();
        SetDialogueBarsCheck();
        gameHUDScript.TurnOnHUD();
        nPCScript.ResetRotationNPC();

        yield return new WaitForSeconds(0.4f);
        playerScript.AlertBubbleCheck();

        yield return new WaitForSeconds(0.1f);
        nPCFidgetScript.HasPlayedInitialFidget = false;
        playerFidgetScript.HasPlayedInitialFidget = false;
        playerScript.SetPlayerBoolsTrue();
        canCheckBubbleBounds = false;
        hasPlayedPopUpSFX = false;
        canStartDialogue = true;      
    }

    // Types the next sentence for the player
    private IEnumerator TypePlayerDialogue()
    {
        if (canPlayBubbleAnim && playerDialgueBubble.activeSelf)
            playerBubbleAnim.SetTrigger("NextSentence");
        else
        {
            canPlayBubbleAnim = true;
            hasStartedPlayerDialogue = true;
            playerFidgetScript.Fidget();                 
        }

        SetDialogueBubblePivot();
        nPCDialgueBubble.SetActive(false);
        playerDialgueBubble.SetActive(true);
        PlayDialogueBubbleSFXCheck();

        yield return new WaitForSeconds(speechBubbleAnimationDelay);
        EmptyTextComponents();
        playerBubbleColorText.text = playerDialogueSentences[playerIndex];
        hasSetBubbleDefaultPosX = false;
        hasSetBubbleDefaultPosY = false;
        canSpeedUpDialogue = true;

        foreach (char letter in playerDialogueSentences[playerIndex].ToCharArray())
        {
            playerForegroundText.text += letter;
            charNoiseSFX.Play();
            yield return new WaitForSeconds(typingSpeed);
        }

        WhoSpeaksNextCheck();
        continueButton.SetActive(true);

        // For playing the dialogue automatically - OLD VERSION
        //yield return new WaitForSeconds(1f); 
        //ContinueDialogueCheck();
    }

    // Types the next sentence for the npc
    private IEnumerator TypeNPCDialogue()
    {
        if (canPlayBubbleAnim && nPCDialgueBubble.activeSelf)
            nPCBubbleAnim.SetTrigger("NextSentence");
        else
        {
            canPlayBubbleAnim = true;
            hasStartedNPCDialogue = true;
            nPCFidgetScript.Fidget();
        }

        SetDialogueBubblePivot();
        playerDialgueBubble.SetActive(false);
        nPCDialgueBubble.SetActive(true);
        PlayDialogueBubbleSFXCheck();

        yield return new WaitForSeconds(speechBubbleAnimationDelay);
        EmptyTextComponents();
        nPCBubbleColorText.text = nPCDialogueSentences[nPCIndex];
        hasSetBubbleDefaultPosX = false;
        hasSetBubbleDefaultPosY = false;
        canSpeedUpDialogue = true;

        foreach (char letter in nPCDialogueSentences[nPCIndex].ToCharArray())
        {
            nPCForegroundText.text += letter;
            charNoiseSFX.Play();
            yield return new WaitForSeconds(typingSpeed);
        }

        WhoSpeaksNextCheck();
        continueButton.SetActive(true);

        // For playing the dialogue automatically - OLD VERSION
        //yield return new WaitForSeconds(1f);
        //ContinueDialogueCheck();      
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        nPCScript = FindObjectOfType<NonPlayerCharacter>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
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
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "DialogueOptionsBubble")
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

                            if (child02.name == "DialogueOptionOne")
                            {
                                dialogueOptionOne = child02;

                                for (int l = 0; l < dialogueOptionOne.transform.childCount; l++)
                                {
                                    GameObject child03 = dialogueOptionOne.transform.GetChild(l).gameObject;

                                    if (child03.name == "DialogueArrowHolder")
                                    {
                                        dialogueArrowHolder = child03;
                                        dialogueArrowScript.SetDialogueArrow(dialogueArrowHolder);
                                    }
                                }
                            }
                               
                            if (child02.name == "DialogueOptionTwo")
                                dialogueOptionTwo = child02;
                            if (child02.name == "DialogueOptionThree")
                                dialogueOptionThree = child02;
                        }
                    }                    
                }
            }             
            if (child.name == "PlayerDialogueBubble")
            {
                playerDialgueBubble = child;
                playerBubbleAnim = playerDialgueBubble.GetComponentInChildren<Animator>();

                for (int g = 0; g < playerBubbleAnim.transform.childCount; g++)
                {
                    RectTransform rectTransform = playerBubbleAnim.transform.GetChild(g).GetComponent<RectTransform>();

                    if (rectTransform.name == "SpeechBubbleHolder")
                        playerSpeechBubbleHolder = rectTransform;
                }
            }
            if (child.name == "NPCDialogueBubble")
            {
                nPCDialgueBubble = child;
                nPCBubbleAnim = nPCDialgueBubble.GetComponentInChildren<Animator>();

                for (int f = 0; f < nPCBubbleAnim.transform.childCount; f++)
                {
                    RectTransform rectTransform = nPCBubbleAnim.transform.GetChild(f).GetComponent<RectTransform>();

                    if (rectTransform.name == "SpeechBubbleHolder")
                        nPCSpeechBubbleHolder = rectTransform;
                }
            }              
            if (child.name == "PlayerAlertBubble")
                playerAlertBubble = child;
        }

        for (int i = 0; i < gameHUDScript.transform.parent.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.parent.GetChild(i).gameObject;

            if (child.name == "ContinueButton")
                continueButton = child;
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

        optionOneText = dialogueOptionOne.GetComponent<TextMeshProUGUI>();
        optionTwoText = dialogueOptionTwo.GetComponent<TextMeshProUGUI>();
        optionThreeText = dialogueOptionThree.GetComponent<TextMeshProUGUI>();
    }

}
