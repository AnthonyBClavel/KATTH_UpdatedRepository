using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.IO;

public class CharacterDialogue : MonoBehaviour
{
    public string talkingTo;
    private string lastTalkedTo;
    private int playerIndex;
    private int nPCIndex;
    private int dialogueOptionsIndex;

    Vector3 up = Vector3.zero,
    right = new Vector3(0, 90, 0),
    down = new Vector3(0, 180, 0),
    left = new Vector3(0, 270, 0);

    //public float sentenceLength;
    public float typingSpeed = 0.03f;
    public float sentenceDelay = 1f;
    private float speechBubbleAnimationDelay;
    private float dialogueBubbleScale = 0.8f; // the scale of the parent object affects the child object's positioning

    private float screenLeftEdgePosX = -885; // -960f 
    private float screenRightEdgePosX = 885; // 960f
    private float screenTopEdgePosY = 415; 
    private float screenLeftEdgePosX02 = -885;
    private float screenRightEdgePosX02 = 885;

    private float playerRightEdgePosX; // right side of the player's speech bubble 
    private float playerLeftEdgePosX; // left side of of the player's speech bubble
    private float playerTopEdgePosY;

    private float nPCRightEdgePosX; // right side of the npc's speech bubble
    private float nPCLeftEdgePosX; // left side of the npc's speech bubble 
    private float nPCTopEdgePosY;

    private float dialogueOptionsLeftEdgePosX; // right side of the dialogue options bubble 
    private float dialogueOptionsRightEdgePosX; // left side of the dialogue options bubble
    private float dialogueOptionsTopEdgePosY;

    private float playerBubbleWidth;
    private float playerBubbleHeight;
    private float playerBubbleHolderLocalPosX;
    private float playerBubbbleHolderLocalPoxY;  
    private float playerDialogueBubbleLocalPosX;
    private float playerDialogueBubbleLocalPosY;

    private float nPCBubbleWidth;
    private float nPCBubbleHeight;
    private float nPCBubbleHolderLocalPosX;
    private float nPCBubbleHolderLocalPosY;
    private float nPCDialogueBubbleLocalPosX;
    private float nPCDialogueBubbleLocalPosY;

    private float dialogueOptionsBubbleWidth;
    private float dialogueOptionsBubbleHeight;
    private float dialogueOptionsHolderLocalPosX;
    private float dialogueOptionsHolderLocalPosY;
    private float dialogueOptionsBubbleLocalPosX;
    private float dialogueOptionsBubbleLocalPosY;

    private Color selectedTextColor = new Color32(128, 160, 198, 255);
    private Color unselectedTextColor = Color.gray;
    private Vector3 dialogueArrowDefaultPos = new Vector3(5, 5, 0);

    private Vector2 bubbleAnimOrigPos;
    private Vector2 bubbleHolderOrigPos;
    private Vector2 moveBubbleRight;
    private Vector2 moveBubbleLeft;

    private Vector2 originalPivot;
    private Vector2 movePivotRight;
    private Vector2 movePivotLeft;

    [Header("Bools")]
    public bool isPlayerSpeaking;
    public bool canPlayBubbleAnim = false;
    public bool canStartDialogue = true;
    public bool hasStartedDialoguePlayer = false;
    public bool hasStartedDialogueNPC = false;
    public bool isInteractingWithNPC = false;
    public bool isInteractingWithArtifact = false;
    private bool canCheckBubbleBounds = false;
    private bool hasSetBubbleDefaultPosX = false;
    private bool hasSetBubbleDefaultPosY = false;
    private bool hasPlayedPopUpSFX = false;
    private bool hasSetDialogueBars = false;
    private bool hasSetPivot = false;
    private bool hasAlertBubble = false;
    private bool canAlertBubble = true;
    private bool hasMovedDialogueArrow = false;
    private bool canMoveDialogueArrow = false;
    //private bool hasSetIndex = false;
    //private bool hasLoadedInitialDialogue = false;
    //private bool hasPlayedOptionOne = false;
    //private bool hasPlayedOptionTwo = false;
    private bool hasSelectedDialogueOption = false;

    [Header("Audio")]
    public AudioClip[] dialogueMusicTracks;
    public GameObject dialogueMusic;
    public AudioSource charNoise;
    private AudioSource dialogueMusicAudioSource;
    private AudioSource audioSource;  
    public AudioClip dialoguePopUpSFX;
    public AudioClip dialogueArrowSFX;   
    private AudioClip lastTrack;
    private float dialogueMusicVol;

    [Header("Animator")]
    public Animator playerBubbleAnim;
    public Animator nPCBubbleAnim;
    public Animator dialogueOptionsBubbleAnim;
    //public Animator nPCAnimator;

    [Header("TextMeshPro")]
    // Note: the white text is used to "calculate" the size of the bubble (to fit the text), dialogue text will overlay white text once size is found
    public TextMeshProUGUI whitePlayerText; 
    public TextMeshProUGUI whiteNPCText;
    public TextMeshProUGUI playerDialogueText;
    public TextMeshProUGUI nPCDialogueText;
    private TextMeshProUGUI optionOneText;
    private TextMeshProUGUI optionTwoText;
    private TextMeshProUGUI optionThreeText;

    [Header("GameObjects")]
    public GameObject playerDialgueBubble;
    public GameObject nPCDialgueBubble;
    public GameObject dialogueOptionsBubble;
    public GameObject playerAlertBubble;
    public GameObject continueButton;
    public GameObject fadeTransition;
    public Camera dialogueCamera;

    [Header("Dialogue Arrow GameObjects")]
    public GameObject dialogueArrow;
    public GameObject dialogueOptionOne;
    public GameObject dialogueOptionTwo;
    public GameObject dialogueOptionThree;

    [Header("RectTransforms")]
    public Transform playerDialogueCheck;
    public Transform nPCDialogueCheck;
    public RectTransform playerSpeechBubbleHolder;
    public RectTransform nPCSpeechBubbleHolder;
    public RectTransform dialogueOptionsHolder;

    [Header("Dialogue Text Files")]
    public TextAsset[] playerDialogueFiles;

    [Header("Dialogue Setences")]
    private string[] playerDialogueSentences;
    private string[] nPCDialogueSentences;
    private string[] dialogueQuestions;

    [Header("Scripts")]
    public NonPlayerCharacter nPCScript;
    public FidgetAnimControllerNPC fidgetAnimControllerNPC;
    private DialogueBars dialogueBarsScript;
    private TileMovementController playerScript;
    private PauseMenu pauseMenuScript;
    private AudioLoops audioLoopsScript;
    private CameraController cameraScript;
    private FidgetAnimControllerPlayer fidgetAnimControllerPlayer;

    void Awake()
    {
        dialogueBarsScript = FindObjectOfType<DialogueBars>();
        playerScript = FindObjectOfType<TileMovementController>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        nPCScript = FindObjectOfType<NonPlayerCharacter>();
        audioLoopsScript = FindObjectOfType<AudioLoops>();
        cameraScript = FindObjectOfType<CameraController>();
        fidgetAnimControllerNPC = FindObjectOfType<FidgetAnimControllerNPC>();
        fidgetAnimControllerPlayer = FindObjectOfType<FidgetAnimControllerPlayer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //screenRightEdgePosX = pauseMenuScript.GetComponent<RectTransform>().rect.width / 2;     
        //screenLeftEdgePosX = -screenRightEdgePosX;
        //dialogueBubbleScale = playerBubbleAnim.gameObject.transform.localScale.x;

        optionOneText = dialogueOptionOne.GetComponent<TextMeshProUGUI>();
        optionTwoText = dialogueOptionTwo.GetComponent<TextMeshProUGUI>();
        optionThreeText = dialogueOptionThree.GetComponent<TextMeshProUGUI>();

        dialogueMusicAudioSource = dialogueMusic.GetComponent<AudioSource>();
        audioSource = GetComponent<AudioSource>();
        continueButton.SetActive(false);
        nPCDialgueBubble.SetActive(false);
        playerDialgueBubble.SetActive(false);
        dialogueOptionsBubble.SetActive(false);
        playerAlertBubble.SetActive(false);

        SetVectors();        
    }
        

    // Update is called once per frame
    void Update()
    {
        AdjustDialogueBubbleCheckPlayer();
        AdjustDialogueBubbleCheckNPC();
        AdjustDialogueOptionsBubbleCheck();
        DialogueArrowCheck();

        //sentenceLength = whitePlayerText.text.Length;

        if (Input.GetKeyDown(KeyCode.R) && canStartDialogue)
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

        AnimBubbleDelayCheck();
        ContinueButtonCheck();
    }

    void FixedUpdate()
    {       
        SetPlayerBubblePosition();
        SetNPCBubblePosition();
        SetDialogueBubblePosition();
        SetAlertBubblePosition();
    }

    // Starts the dialogue with an npc
    public void StartDialogue()
    {
        if (canStartDialogue)
        {
            StartCoroutine("StartDialogueDelay");           
            canStartDialogue = false;        
        }
    }

    // Sets the alert bubble active
    public void SetAlertBubbleActive()
    {
        if(!hasAlertBubble && canAlertBubble)
        {
            playerAlertBubble.SetActive(true);
            hasAlertBubble = true;
        }
    }

    // Sets the alert bubble inactive
    public void SetAlertBubbleInactive()
    {
        if (hasAlertBubble && canAlertBubble)
        {
            playerAlertBubble.SetActive(false);           
            hasAlertBubble = false;
        }
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
        if (continueButton.activeSelf && !pauseMenuScript.isChangingScenes && !pauseMenuScript.isPaused)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                ContinueDialogueCheck();
                continueButton.SetActive(false);
            }
        }
    }

    // Checks to see if the dialogue can continue
    private void ContinueDialogueCheck()
    {
        if (!pauseMenuScript.isChangingScenes)
        {
            hasSetBubbleDefaultPosX = false;
            hasSetBubbleDefaultPosY = false;
            NextDialogueSentenceCheck();
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
        }
    }

    // Checks if the player is speaking first or not
    private void StartDialogueCheck()
    {
        if (isPlayerSpeaking)
            StartCoroutine("TypePlayerDialogue");
        else
            StartCoroutine("TypeNPCDialogue");
    }

    // Determines the next dialogue sentence do be played
    private void NextDialogueSentenceCheck()
    {
        if (playerDialogueSentences != null)
        {
            if (playerIndex < playerDialogueSentences.Length - 1 && playerDialogueSentences[playerIndex + 1] != string.Empty && isPlayerSpeaking)
            {
                if (hasStartedDialoguePlayer)
                    playerIndex++;

                if (!playerDialogueSentences[playerIndex].Contains("END DIALOGUE") && !playerDialogueSentences[playerIndex].Contains("LOAD DIALOGUE OPTIONS"))
                    StartCoroutine("TypePlayerDialogue");
                    
            }
        }

        if (nPCDialogueSentences != null)
        {
            if (nPCIndex < nPCDialogueSentences.Length - 1 && nPCDialogueSentences[nPCIndex + 1] != string.Empty && !isPlayerSpeaking)
            {
                if (hasStartedDialogueNPC)
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
            if (playerDialogueSentences[playerIndex].Contains("LOAD DIALOGUE OPTIONS"))
            {
                Debug.Log("The opened dialogue options");
                StopCoroutine("TypePlayerDialogue");
                StopCoroutine("TypeNPCDialogue");
                OpenDialogueOptionsBubble();
            }
            if (nPCDialogueSentences[nPCIndex].Contains("END DIALOGUE"))
            {
                Debug.Log("Has ended dialogue ");
                StopCoroutine("TypePlayerDialogue");
                StopCoroutine("TypeNPCDialogue");
                StartCoroutine("EndDialogueDelay");
                FadeOutDialogueMusic();
            }
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
            dialogueBarsScript.ToggleDialogueBars();
            hasSetDialogueBars = true;
        }

        else if (hasSetDialogueBars && !hasStartedDialoguePlayer || hasSetDialogueBars && !hasStartedDialogueNPC)
        {
            dialogueBarsScript.ToggleDialogueBars();
            hasSetDialogueBars = false;
        }
    }

    // Sets the player's dialogue bubble to follow the player
    private void SetPlayerBubblePosition()
    {
        Vector3 playerBubblePos = dialogueCamera.WorldToScreenPoint(playerDialogueCheck.position);

        if (playerDialgueBubble.transform.position != playerBubblePos)
            playerDialgueBubble.transform.position = playerBubblePos;
    }

    // Sets the npc's dialogue bubble to follow the player
    private void SetNPCBubblePosition()
    {
        Vector3 nPCBubblePos = dialogueCamera.WorldToScreenPoint(nPCDialogueCheck.position);

        if (nPCDialgueBubble.transform.position != nPCBubblePos)
            nPCDialgueBubble.transform.position = nPCBubblePos;
    }

    // Sets the dialogue bubble to follow the player
    private void SetDialogueBubblePosition()
    {
        Vector3 dialogueBubblePos = dialogueCamera.WorldToScreenPoint(playerDialogueCheck.position);

        if (dialogueOptionsBubble.transform.position != dialogueBubblePos)
            dialogueOptionsBubble.transform.position = dialogueBubblePos;
    }

    // Sets the alert bubble to follow the player
    private void SetAlertBubblePosition()
    {
        Vector3 alertBubblePos = dialogueCamera.WorldToScreenPoint(playerDialogueCheck.position);

        if (playerAlertBubble.transform.position != alertBubblePos)
            playerAlertBubble.transform.position = alertBubblePos;
    }

    // Checks to see when the dialogue arrow can be move and execute functions
    private void DialogueArrowCheck()
    {
        if (dialogueArrow.activeSelf && dialogueOptionsIndex < dialogueQuestions.Length && canMoveDialogueArrow && !pauseMenuScript.isChangingScenes && !pauseMenuScript.isPaused)
        {
            if (dialogueOptionsIndex != 0)
            {
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    dialogueOptionsIndex--;
                    PlayDialogueArrowSFX();
                    hasMovedDialogueArrow = false;
                }
            }

            if (dialogueOptionsIndex != 2)
            {
                if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    dialogueOptionsIndex++;
                    PlayDialogueArrowSFX();
                    hasMovedDialogueArrow = false;
                }

            }

            if (dialogueOptionsIndex == 0)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    if (!nPCScript.hasPlayedOptionOne)
                    {
                        setPlayerDialogue(playerDialogueFiles[2]);
                        setNPCDialogue(nPCScript.nPCDialogueFiles[2]);
                        nPCScript.hasPlayedOptionOne = true;
                    }
                    else
                    {
                        setPlayerDialogue(playerDialogueFiles[3]);
                        setNPCDialogue(nPCScript.nPCDialogueFiles[3]);
                    }
                    hasSelectedDialogueOption = true;
                    CloseDialogueOptionsBuble();
                    StartDialogueCheck();
                }

                if (!hasMovedDialogueArrow)
                {
                    dialogueArrow.transform.SetParent(dialogueOptionOne.transform);
                    dialogueArrow.transform.localPosition = dialogueArrowDefaultPos;

                    optionOneText.color = selectedTextColor;
                    optionTwoText.color = unselectedTextColor;
                    optionThreeText.color = unselectedTextColor;

                    hasMovedDialogueArrow = true;
                }
            }

            else if (dialogueOptionsIndex == 1)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    if (!nPCScript.hasPlayedOptionTwo)
                    {
                        setPlayerDialogue(playerDialogueFiles[4]);
                        setNPCDialogue(nPCScript.nPCDialogueFiles[4]);
                        nPCScript.hasPlayedOptionTwo = true;
                    }
                    else
                    {
                        setPlayerDialogue(playerDialogueFiles[5]);
                        setNPCDialogue(nPCScript.nPCDialogueFiles[5]);
                    }
                    hasSelectedDialogueOption = true;
                    CloseDialogueOptionsBuble();
                    StartDialogueCheck();
                }

                if (!hasMovedDialogueArrow)
                {
                    dialogueArrow.transform.SetParent(dialogueOptionTwo.transform);
                    dialogueArrow.transform.localPosition = dialogueArrowDefaultPos;

                    optionOneText.color = unselectedTextColor;
                    optionTwoText.color = selectedTextColor;
                    optionThreeText.color = unselectedTextColor;

                    hasMovedDialogueArrow = true;
                }
            }

            else if (dialogueOptionsIndex == 2)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    if (hasSelectedDialogueOption)
                    {
                        setPlayerDialogue(playerDialogueFiles[6]);
                        setNPCDialogue(nPCScript.nPCDialogueFiles[6]);
                    }
                    else
                    {
                        setPlayerDialogue(playerDialogueFiles[7]);
                        setNPCDialogue(nPCScript.nPCDialogueFiles[7]);
                    }
                    CloseDialogueOptionsBuble();
                    StartDialogueCheck();
                }

                if (!hasMovedDialogueArrow)
                {
                    dialogueArrow.transform.SetParent(dialogueOptionThree.transform);
                    dialogueArrow.transform.localPosition = dialogueArrowDefaultPos;

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
        if (canCheckBubbleBounds && dialogueOptionsBubble.activeSelf)
        {
            DialogueOptionsBubbleValuesCheck();

            dialogueOptionsTopEdgePosY = ((dialogueOptionsHolderLocalPosY + dialogueOptionsBubbleHeight) * dialogueBubbleScale) + dialogueOptionsBubbleLocalPosY /*+ dialogueOptionsBubbleAnim.gameObject.transform.localPosition.y*/;

            if (dialogueOptionsTopEdgePosY > screenTopEdgePosY)
            {
                dialogueOptionsBubbleAnim.gameObject.transform.localPosition = new Vector3(0, screenTopEdgePosY - dialogueOptionsTopEdgePosY, 0);
            }

            if (dialogueOptionsTopEdgePosY < screenTopEdgePosY && !hasSetBubbleDefaultPosY)
            {
                dialogueOptionsBubbleAnim.gameObject.transform.localPosition = bubbleAnimOrigPos;
                hasSetBubbleDefaultPosY = true;
            }

            if (playerScript.playerDirection == right)
            {
                dialogueOptionsLeftEdgePosX = dialogueOptionsBubbleLocalPosX + ((dialogueOptionsHolderLocalPosX - dialogueOptionsBubbleWidth) * dialogueBubbleScale);

                if (dialogueOptionsLeftEdgePosX < screenLeftEdgePosX02 && dialogueOptionsHolder.rect.width < ((-screenLeftEdgePosX02 * 2) / dialogueBubbleScale))
                {
                    dialogueOptionsHolder.localPosition = new Vector3(dialogueOptionsHolder.localPosition.x + ((screenLeftEdgePosX02 - dialogueOptionsLeftEdgePosX) / dialogueBubbleScale), 78, 0);
                }

                if (dialogueOptionsLeftEdgePosX > screenLeftEdgePosX02 && playerSpeechBubbleHolder.rect.width < (-screenRightEdgePosX02 / dialogueBubbleScale) && !hasSetBubbleDefaultPosX)
                {
                    dialogueOptionsHolder.localPosition = moveBubbleRight;
                    hasSetBubbleDefaultPosX = true;
                }
            }

            else if (playerScript.playerDirection == left)
            {
                dialogueOptionsRightEdgePosX = dialogueOptionsBubbleLocalPosX + ((dialogueOptionsHolderLocalPosX + dialogueOptionsBubbleWidth) * dialogueBubbleScale);

                if (dialogueOptionsRightEdgePosX > screenRightEdgePosX02 && dialogueOptionsHolder.rect.width < ((screenRightEdgePosX02 * 2) / dialogueBubbleScale))
                {
                    dialogueOptionsHolder.localPosition = new Vector3(dialogueOptionsHolder.localPosition.x - ((dialogueOptionsRightEdgePosX - screenRightEdgePosX02) / dialogueBubbleScale), 78, 0);
                }

                if (dialogueOptionsRightEdgePosX < screenRightEdgePosX02 && playerSpeechBubbleHolder.rect.width < (screenRightEdgePosX02 / dialogueBubbleScale) && !hasSetBubbleDefaultPosX)
                {
                    dialogueOptionsHolder.localPosition = moveBubbleLeft;
                    hasSetBubbleDefaultPosX = true;
                }
            }

            else if (playerScript.playerDirection == up || playerScript.playerDirection == down)
            {
                dialogueOptionsLeftEdgePosX = dialogueOptionsBubbleLocalPosX + ((dialogueOptionsHolderLocalPosX - dialogueOptionsBubbleWidth * 0.5f) * dialogueBubbleScale);
                dialogueOptionsRightEdgePosX = dialogueOptionsBubbleLocalPosX + ((dialogueOptionsHolderLocalPosX + dialogueOptionsBubbleWidth * 0.5f) * dialogueBubbleScale);

                if (dialogueOptionsLeftEdgePosX < screenLeftEdgePosX02 && dialogueOptionsHolder.rect.width < ((-screenLeftEdgePosX02 * 2) / dialogueBubbleScale))
                {
                    dialogueOptionsHolder.localPosition = new Vector3(dialogueOptionsHolder.localPosition.x + ((screenLeftEdgePosX02 - dialogueOptionsLeftEdgePosX) / dialogueBubbleScale), 78, 0);
                }

                if (dialogueOptionsRightEdgePosX > screenRightEdgePosX02 && dialogueOptionsHolder.rect.width < ((screenRightEdgePosX02 * 2) / dialogueBubbleScale))
                {
                    dialogueOptionsHolder.localPosition = new Vector3(dialogueOptionsHolder.localPosition.x - ((dialogueOptionsRightEdgePosX - screenRightEdgePosX02) / dialogueBubbleScale), 78, 0);
                }

                if (dialogueOptionsLeftEdgePosX >= screenLeftEdgePosX02 && dialogueOptionsRightEdgePosX <= screenRightEdgePosX02 && !hasSetBubbleDefaultPosX)
                {
                    dialogueOptionsHolder.localPosition = bubbleHolderOrigPos;
                    hasSetBubbleDefaultPosX = true;
                }
            }

        }
    }

    // Checks to see if the player's dialogue bubble goes outside of the screen and re-adjusts it accordingly
    private void AdjustDialogueBubbleCheckPlayer()
    {
        if (canCheckBubbleBounds && playerDialgueBubble.activeSelf)
        {
            PlayerBubbleValuesCheck();

            playerTopEdgePosY = ((playerBubbbleHolderLocalPoxY + playerBubbleHeight) * dialogueBubbleScale) + playerDialogueBubbleLocalPosY /*+ playerBubbleAnim.gameObject.transform.localPosition.y*/;

            if (playerTopEdgePosY > screenTopEdgePosY)
            {
                playerBubbleAnim.gameObject.transform.localPosition = new Vector3(0, screenTopEdgePosY - playerTopEdgePosY, 0);
            }

            if (playerTopEdgePosY < screenTopEdgePosY && !hasSetBubbleDefaultPosY)
            {
                playerBubbleAnim.gameObject.transform.localPosition = bubbleAnimOrigPos;
                hasSetBubbleDefaultPosY = true;
            }

            if (playerScript.playerDirection == right)
            {
                playerLeftEdgePosX = playerDialogueBubbleLocalPosX + ((playerBubbleHolderLocalPosX - playerBubbleWidth) * dialogueBubbleScale);

                if (playerLeftEdgePosX < screenLeftEdgePosX && playerSpeechBubbleHolder.rect.width < ((-screenLeftEdgePosX * 2) / dialogueBubbleScale))
                {
                    playerSpeechBubbleHolder.localPosition = new Vector3(playerSpeechBubbleHolder.localPosition.x + ((screenLeftEdgePosX - playerLeftEdgePosX) / dialogueBubbleScale), 78, 0);
                    //playerSpeechBubbleHolder.localPosition = new Vector3(playerSpeechBubbleHolder.localPosition.x + ((-playerLeftEdgePosX - 960) / 0.8f), 78, 0);
                }

                if (playerLeftEdgePosX > screenLeftEdgePosX && playerSpeechBubbleHolder.rect.width < (-screenLeftEdgePosX / dialogueBubbleScale) && !hasSetBubbleDefaultPosX)
                {
                    playerSpeechBubbleHolder.localPosition = moveBubbleRight;
                    hasSetBubbleDefaultPosX = true;
                }
            }

            else if (playerScript.playerDirection == left)
            {
                playerRightEdgePosX = playerDialogueBubbleLocalPosX + ((playerBubbleHolderLocalPosX + playerBubbleWidth) * dialogueBubbleScale);

                if (playerRightEdgePosX > screenRightEdgePosX && playerSpeechBubbleHolder.rect.width < ((screenRightEdgePosX * 2) / dialogueBubbleScale))
                {
                    playerSpeechBubbleHolder.localPosition = new Vector3(playerSpeechBubbleHolder.localPosition.x - ((playerRightEdgePosX - screenRightEdgePosX) / dialogueBubbleScale), 78, 0);
                    //playerSpeechBubbleHolder.localPosition = new Vector3(playerSpeechBubbleHolder.localPosition.x - ((rightEdgePosX - 960) / 0.8f), 78, 0);
                }

                if (playerRightEdgePosX < screenRightEdgePosX && playerSpeechBubbleHolder.rect.width < (screenRightEdgePosX / dialogueBubbleScale) && !hasSetBubbleDefaultPosX)
                {
                    playerSpeechBubbleHolder.localPosition = moveBubbleLeft;
                    hasSetBubbleDefaultPosX = true;
                }
            }

            else if (playerScript.playerDirection == up || playerScript.playerDirection == down)
            {
                playerLeftEdgePosX = playerDialogueBubbleLocalPosX + ((playerBubbleHolderLocalPosX - playerBubbleWidth * 0.5f) * dialogueBubbleScale);
                playerRightEdgePosX = playerDialogueBubbleLocalPosX + ((playerBubbleHolderLocalPosX + playerBubbleWidth * 0.5f) * dialogueBubbleScale);

                if (playerLeftEdgePosX < screenLeftEdgePosX && playerSpeechBubbleHolder.rect.width < ((-screenLeftEdgePosX * 2) / dialogueBubbleScale))
                {
                    playerSpeechBubbleHolder.localPosition = new Vector3(playerSpeechBubbleHolder.localPosition.x + ((screenLeftEdgePosX - playerLeftEdgePosX) / dialogueBubbleScale), 78, 0);
                }

                if (playerRightEdgePosX > screenRightEdgePosX && playerSpeechBubbleHolder.rect.width < ((screenRightEdgePosX * 2) / dialogueBubbleScale))
                {
                    playerSpeechBubbleHolder.localPosition = new Vector3(playerSpeechBubbleHolder.localPosition.x - ((playerRightEdgePosX - screenRightEdgePosX) / dialogueBubbleScale), 78, 0);
                }

                if (playerLeftEdgePosX > screenLeftEdgePosX && playerRightEdgePosX < screenRightEdgePosX && !hasSetBubbleDefaultPosX)
                {
                    playerSpeechBubbleHolder.localPosition = bubbleHolderOrigPos;
                    hasSetBubbleDefaultPosX = true;
                }
            }

        }
    }

    // Checks to see if the npc's dialogue bubble goes outside of the screen and re-adjusts it accordingly
    private void AdjustDialogueBubbleCheckNPC()
    {    
        if (canCheckBubbleBounds && nPCDialgueBubble.activeSelf)
        {
            NPCBubbleValuesCheck();

            nPCTopEdgePosY = ((nPCBubbleHolderLocalPosY + nPCBubbleHeight) * dialogueBubbleScale) + nPCDialogueBubbleLocalPosY /*+ nPCBubbleAnim.gameObject.transform.localPosition.y*/;

            if (nPCTopEdgePosY > screenTopEdgePosY)
            {
                nPCBubbleAnim.gameObject.transform.localPosition = new Vector3(0, screenTopEdgePosY - nPCTopEdgePosY, 0);
            }

            if (nPCTopEdgePosY < screenTopEdgePosY && !hasSetBubbleDefaultPosY)
            {
                nPCBubbleAnim.gameObject.transform.localPosition = bubbleAnimOrigPos;
                hasSetBubbleDefaultPosY = true;
            }

            if (playerScript.playerDirection == right)
            {
                nPCRightEdgePosX = nPCDialogueBubbleLocalPosX + ((nPCBubbleHolderLocalPosX + nPCBubbleWidth) * dialogueBubbleScale);

                if (nPCRightEdgePosX > screenRightEdgePosX && nPCSpeechBubbleHolder.rect.width < ((screenRightEdgePosX * 2) / dialogueBubbleScale))
                {
                    nPCSpeechBubbleHolder.localPosition = new Vector3(nPCSpeechBubbleHolder.localPosition.x - ((nPCRightEdgePosX - screenRightEdgePosX) / dialogueBubbleScale), 78, 0);
                    //playerSpeechBubbleHolder.localPosition = new Vector3(playerSpeechBubbleHolder.localPosition.x - ((rightEdgePosX - 960) / 0.8f), 78, 0);
                }

                if (nPCRightEdgePosX < screenRightEdgePosX && playerSpeechBubbleHolder.rect.width < (screenRightEdgePosX / dialogueBubbleScale) && !hasSetBubbleDefaultPosX)
                {
                    nPCSpeechBubbleHolder.localPosition = moveBubbleLeft;
                    hasSetBubbleDefaultPosX = true;
                }
            }

            else if (playerScript.playerDirection == left)
            {
                nPCLeftEdgePosX = nPCDialogueBubbleLocalPosX + ((nPCBubbleHolderLocalPosX - nPCBubbleWidth) * dialogueBubbleScale);

                if (nPCLeftEdgePosX < screenLeftEdgePosX && nPCSpeechBubbleHolder.rect.width < ((-screenLeftEdgePosX * 2) / dialogueBubbleScale))
                {
                    nPCSpeechBubbleHolder.localPosition = new Vector3(nPCSpeechBubbleHolder.localPosition.x + ((screenLeftEdgePosX - nPCLeftEdgePosX) / dialogueBubbleScale), 78, 0);
                    //npcSpeechBubbleHolder.localPosition = new Vector3(npcSpeechBubbleHolder.localPosition.x + ((-npcLeftEdgePosX - 960) / 0.8f), 78, 0);
                }

                if (nPCLeftEdgePosX > screenLeftEdgePosX && playerSpeechBubbleHolder.rect.width < (-screenLeftEdgePosX / dialogueBubbleScale) && !hasSetBubbleDefaultPosX)
                {
                    nPCSpeechBubbleHolder.localPosition = moveBubbleRight;
                    hasSetBubbleDefaultPosX = true;
                }
            }

            else if (playerScript.playerDirection == up || playerScript.playerDirection == down)
            {
                nPCLeftEdgePosX = nPCDialogueBubbleLocalPosX + ((nPCBubbleHolderLocalPosX - nPCBubbleWidth * 0.5f) * dialogueBubbleScale);
                nPCRightEdgePosX = nPCDialogueBubbleLocalPosX + ((nPCBubbleHolderLocalPosX + nPCBubbleWidth * 0.5f) * dialogueBubbleScale);

                if (nPCLeftEdgePosX < screenLeftEdgePosX && nPCSpeechBubbleHolder.rect.width < ((-screenLeftEdgePosX * 2) / dialogueBubbleScale))
                {
                    nPCSpeechBubbleHolder.localPosition = new Vector3(nPCSpeechBubbleHolder.localPosition.x + ((screenLeftEdgePosX - playerLeftEdgePosX) / dialogueBubbleScale), 78, 0);
                }

                if (nPCRightEdgePosX > screenRightEdgePosX && nPCSpeechBubbleHolder.rect.width < ((screenRightEdgePosX * 2) / dialogueBubbleScale))
                {
                    nPCSpeechBubbleHolder.localPosition = new Vector3(nPCSpeechBubbleHolder.localPosition.x - ((nPCRightEdgePosX - screenRightEdgePosX) / dialogueBubbleScale), 78, 0);
                }

                if (nPCLeftEdgePosX > screenLeftEdgePosX && nPCRightEdgePosX < screenRightEdgePosX && !hasSetBubbleDefaultPosX)
                {
                    nPCSpeechBubbleHolder.localPosition = bubbleHolderOrigPos;
                    hasSetBubbleDefaultPosX = true;
                }
            }

        }
    }

    // Updates the floats for the dialogue options bubble when they change
    private void DialogueOptionsBubbleValuesCheck()
    {
        if (dialogueOptionsBubbleWidth != dialogueOptionsHolder.rect.width)
            dialogueOptionsBubbleWidth = dialogueOptionsHolder.rect.width;

        if (dialogueOptionsBubbleHeight != dialogueOptionsHolder.rect.height)
            dialogueOptionsBubbleHeight = dialogueOptionsHolder.rect.height;

        if (dialogueOptionsHolderLocalPosX != dialogueOptionsHolder.localPosition.x)
            dialogueOptionsHolderLocalPosX = dialogueOptionsHolder.localPosition.x;

        if (dialogueOptionsHolderLocalPosY != dialogueOptionsHolder.localPosition.y)
            dialogueOptionsHolderLocalPosY = dialogueOptionsHolder.localPosition.y;

        if (dialogueOptionsBubbleLocalPosX != dialogueOptionsBubble.transform.localPosition.x)
            dialogueOptionsBubbleLocalPosX = dialogueOptionsBubble.transform.localPosition.x;

        if (dialogueOptionsBubbleLocalPosY != dialogueOptionsBubble.transform.localPosition.y)
            dialogueOptionsBubbleLocalPosY = dialogueOptionsBubble.transform.localPosition.y;
    }

    // Updates the floats for the player's bubble when they change
    private void PlayerBubbleValuesCheck()
    {
        if (playerBubbleHeight != playerSpeechBubbleHolder.rect.height)
            playerBubbleHeight = playerSpeechBubbleHolder.rect.height;

        if (playerBubbleWidth != playerSpeechBubbleHolder.rect.width)
            playerBubbleWidth = playerSpeechBubbleHolder.rect.width;

        if (playerBubbleHolderLocalPosX != playerSpeechBubbleHolder.localPosition.x)
            playerBubbleHolderLocalPosX = playerSpeechBubbleHolder.localPosition.x;

        if (playerBubbbleHolderLocalPoxY != playerSpeechBubbleHolder.localPosition.y)
            playerBubbbleHolderLocalPoxY = playerSpeechBubbleHolder.localPosition.y;

        if (playerDialogueBubbleLocalPosX != playerDialgueBubble.transform.localPosition.x)
            playerDialogueBubbleLocalPosX = playerDialgueBubble.transform.localPosition.x;

        if (playerDialogueBubbleLocalPosY != playerDialgueBubble.transform.localPosition.y)
            playerDialogueBubbleLocalPosY = playerDialgueBubble.transform.localPosition.y;
    }

    // Updates the floats for the npc's bubble when they change
    private void NPCBubbleValuesCheck()
    {
        if (nPCBubbleWidth != nPCSpeechBubbleHolder.rect.width)
            nPCBubbleWidth = nPCSpeechBubbleHolder.rect.width;

        if (nPCBubbleHeight != nPCSpeechBubbleHolder.rect.height)
            nPCBubbleHeight = nPCSpeechBubbleHolder.rect.height;

        if (nPCBubbleHolderLocalPosX != nPCSpeechBubbleHolder.localPosition.x)
            nPCBubbleHolderLocalPosX = nPCSpeechBubbleHolder.localPosition.x;

        if (nPCBubbleHolderLocalPosY != nPCSpeechBubbleHolder.localPosition.y)
            nPCBubbleHolderLocalPosY = nPCSpeechBubbleHolder.localPosition.y;

        if (nPCDialogueBubbleLocalPosX != nPCDialgueBubble.transform.localPosition.x)
            nPCDialogueBubbleLocalPosX = nPCDialgueBubble.transform.localPosition.x;

        if (nPCDialogueBubbleLocalPosY != nPCDialgueBubble.transform.localPosition.y)
            nPCDialogueBubbleLocalPosY = nPCDialgueBubble.transform.localPosition.y;
    }

    // Sets the pivot for both the player and npc's dialogue bubble - pivot determines which direction the dialogue bubble extends to
    private void SetDialogueBubblePivot()
    {
        if (!hasSetPivot)
        {
            if (playerScript.playerDirection == up || playerScript.playerDirection == down)
            {

                playerSpeechBubbleHolder.pivot = originalPivot;
                nPCSpeechBubbleHolder.pivot = originalPivot;
                dialogueOptionsHolder.pivot = originalPivot;
            }

            if (playerScript.playerDirection == left)
            {
                playerSpeechBubbleHolder.pivot = movePivotLeft;
                nPCSpeechBubbleHolder.pivot = movePivotRight;
                dialogueOptionsHolder.pivot = movePivotLeft;
            }

            if (playerScript.playerDirection == right)
            {
                playerSpeechBubbleHolder.pivot = movePivotRight;
                nPCSpeechBubbleHolder.pivot = movePivotLeft;
                dialogueOptionsHolder.pivot = movePivotRight;
            }

            hasSetPivot = true;
        }

    }

    // Sets the dialogue options bubble active
    private void OpenDialogueOptionsBubble()
    {
        StartCoroutine("SetDialogueArrowActiveDelay");

        if (!nPCScript.hasLoadedInitialDialogue)
        {
            dialogueOptionsIndex = 0;
            nPCScript.hasLoadedInitialDialogue = true;
        }

        playerDialgueBubble.SetActive(false);
        nPCDialgueBubble.SetActive(false);
        dialogueOptionsBubble.SetActive(true);
        dialogueOptionOne.SetActive(true);
        dialogueOptionTwo.SetActive(true);
        dialogueOptionThree.SetActive(true);

        SetDialogueBubblePivot();
        PlayDialogueBubbleSFXCheck();
        hasStartedDialoguePlayer = false;
        hasStartedDialogueNPC = false;
        canMoveDialogueArrow = true;
        fidgetAnimControllerNPC.inCharacterDialogue = false;
        fidgetAnimControllerPlayer.inCharacterDialogue = false;
        fidgetAnimControllerPlayer.FidgetAnimCheck();

        optionOneText.color = unselectedTextColor;
        optionTwoText.color = unselectedTextColor;
        optionThreeText.color = unselectedTextColor;

        optionOneText.text = dialogueQuestions[0];
        optionTwoText.text = dialogueQuestions[1];
        optionThreeText.text = dialogueQuestions[2];
    }

    // Sets the dialogue options bubble inactive
    private void CloseDialogueOptionsBuble()
    {
        optionOneText.color = unselectedTextColor;
        optionTwoText.color = unselectedTextColor;
        optionThreeText.color = unselectedTextColor;
     
        dialogueOptionsBubble.SetActive(false);
        dialogueArrow.SetActive(false);
        hasSetBubbleDefaultPosX = false;
        hasSetBubbleDefaultPosY = false;
        canMoveDialogueArrow = false;
        canPlayBubbleAnim = false;
        hasPlayedPopUpSFX = false;
        hasSetPivot = false;
        fidgetAnimControllerNPC.inCharacterDialogue = true;
        fidgetAnimControllerPlayer.inCharacterDialogue = true;

        if (dialogueOptionsIndex == 2)
        {
            dialogueArrow.transform.SetParent(dialogueOptionOne.transform);
            dialogueArrow.transform.localPosition = dialogueArrowDefaultPos;
            dialogueOptionsIndex = 0;
        }

        playerIndex = 0;
        nPCIndex = 0;
    }

    // Checks if the dialogue pop up sfx can be played - played only once when the dialogue bubble is activated/re-activated
    private void PlayDialogueBubbleSFXCheck()
    {
        if (!hasPlayedPopUpSFX)
        {
            if (isPlayerSpeaking)
            {
                audioSource.volume = 0.24f;
                audioSource.pitch = 0.5f;
                audioSource.PlayOneShot(dialoguePopUpSFX);
                hasPlayedPopUpSFX = true;
            }
            else if (!isPlayerSpeaking)
            {
                audioSource.volume = 0.24f;
                audioSource.pitch = 0.6f;
                audioSource.PlayOneShot(dialoguePopUpSFX);
                hasPlayedPopUpSFX = true;
            }
        }
    }

    // Checks to see what the speechBubbleAnimationDelay should be - delay changes due to different animation lengths
    private void AnimBubbleDelayCheck()
    {
        if (canPlayBubbleAnim)
            speechBubbleAnimationDelay = 0.05f;
        else
            speechBubbleAnimationDelay = 0.1f;
    }

    // Sets the values for the sfx and plays it
    private void PlayDialogueArrowSFX()
    {
        audioSource.volume = 1f;
        audioSource.pitch = 3f;
        audioSource.PlayOneShot(dialogueArrowSFX);
    }

    // Sets a specific text color for each npc
    private void SetDialogueTextColor()
    {
        if (talkingTo == "VillageElder")
            nPCDialogueText.color = new Color32(58, 78, 112, 255);

        else if (talkingTo == "Fisherman")
            nPCDialogueText.color = new Color32(194, 130, 104, 255);

        else if (talkingTo == "VillageExplorer01")
            nPCDialogueText.color = new Color32(115, 106, 142, 255);

        else if (talkingTo == "FriendlyGhost")
            nPCDialogueText.color = Color.black;

        else if (talkingTo == "VillageExplorer02")
            nPCDialogueText.color = new Color32(155, 162, 125, 255);

        else
            nPCDialogueText.color = Color.black;
    }

    // Sets values for all vectors
    private void SetVectors()
    {
        bubbleAnimOrigPos = new Vector2(0, 0);
        bubbleHolderOrigPos = new Vector2(0, 78);
        moveBubbleRight = new Vector2(100f, 78);
        moveBubbleLeft = new Vector2(-100f, 78);

        originalPivot = new Vector2(0.5f, 0);
        movePivotRight = new Vector2(1, 0);
        movePivotLeft = new Vector2(0, 0);
    }

    // Clears the text within all text components - before new text is set
    private void EmptyTextComponents()
    {
        nPCDialogueText.text = string.Empty;
        playerDialogueText.text = string.Empty;
        whitePlayerText.text = string.Empty;
        whiteNPCText.text = string.Empty;
    }

    // Changes the dialogue music with a randomly selected dialogue music track that's new/different from previous
    private void ChangeDialogueMusic()
    {
        int attempts = 3;
        AudioClip newDialogueMusicTrack = dialogueMusicTracks[UnityEngine.Random.Range(0, dialogueMusicTracks.Length)];

        while (newDialogueMusicTrack == lastTrack && attempts > 0)
        {
            newDialogueMusicTrack = dialogueMusicTracks[UnityEngine.Random.Range(0, dialogueMusicTracks.Length)];
            attempts--;
        }

        lastTrack = newDialogueMusicTrack;

        dialogueMusicAudioSource.Stop();
        dialogueMusicAudioSource.clip = newDialogueMusicTrack;
        dialogueMusicAudioSource.Play();
    }

    // Fades in the dialogue music
    private void FadeInDialogueMusic()
    {
        StopCoroutine("FadeOutDialogueMusicTrack");
        StartCoroutine("FadeInDialogueMusicTrack");
        audioLoopsScript.FadeOutBGMLoop();
    }

    // Fades out the dialogue music
    private void FadeOutDialogueMusic()
    {
        StopCoroutine("FadeInDialogueMusicTrack");
        StartCoroutine("FadeOutDialogueMusicTrack");
        audioLoopsScript.FadeInBGMLoop();
    }

    // Increases the dialogue music over time until it reaches its max value
    private IEnumerator FadeInDialogueMusicTrack()
    {
        dialogueMusicVol = 0.0f;
        dialogueMusicAudioSource.volume = dialogueMusicVol;
        dialogueMusic.SetActive(true);
        ChangeDialogueMusic();

        for (float i = 0f; i <= 0.8f; i += 0.02f)
        {
            i = dialogueMusicVol;
            dialogueMusicVol += 0.02f;
            dialogueMusicAudioSource.volume = dialogueMusicVol;
            yield return new WaitForSeconds(0.025f);
        }
    }

    // Decreases the dialogue music volume over time until it reaches its min value
    private IEnumerator FadeOutDialogueMusicTrack()
    {
        dialogueMusicVol = 0.8f;
        dialogueMusicAudioSource.volume = dialogueMusicVol;

        for (float i = 0.8f; i >= 0f; i -= 0.02f)
        {
            i = dialogueMusicVol;
            dialogueMusicVol -= 0.02f;
            dialogueMusicAudioSource.volume = dialogueMusicVol;
            yield return new WaitForSeconds(0.025f);
        }

        dialogueMusic.SetActive(false);
    }

    // Sets the correct dialogue files for the player, npc, artifacts, and dialogue bubble
    private void SetDialogueFilesCheck()
    {
        if (isInteractingWithNPC)
        {
            nPCScript.SetRotationNPC();
            SetDialogueTextColor();

            if (!nPCScript.hasLoadedInitialDialogue)
            {
                setPlayerDialogue(playerDialogueFiles[0]);
                setNPCDialogue(nPCScript.nPCDialogueFiles[0]);         
            }

            else if (nPCScript.hasLoadedInitialDialogue)
            {
                setPlayerDialogue(playerDialogueFiles[1]);
                setNPCDialogue(nPCScript.nPCDialogueFiles[1]);
            }
        }
    }

    // Sets the dialogue arrow active after specified time
    private IEnumerator SetDialogueArrowActiveDelay()
    {
        yield return new WaitForSeconds(0.25f);
        dialogueArrow.SetActive(true);
        PlayDialogueArrowSFX();
        hasMovedDialogueArrow = false;
    }

    // Starts the dialogue after a delay
    private IEnumerator StartDialogueDelay()
    {
        SetDialogueFilesCheck();
        FadeInDialogueMusic();
        playerAlertBubble.SetActive(false);
        canCheckBubbleBounds = true;
        canAlertBubble = false;
        cameraScript.canMoveCamera = false;
        cameraScript.canMoveToDialogueViews = true;
        cameraScript.hasCheckedDialogueViews = false;
        fidgetAnimControllerNPC.inCharacterDialogue = true;
        fidgetAnimControllerPlayer.inCharacterDialogue = true;

        SetDialogueBarsCheck();
        dialogueBarsScript.TurnOffHUD();
        playerScript.SetPlayerBoolsFalse();

        yield return new WaitForSeconds(0.5f);
        StartDialogueCheck();
    }

    // Ends the dialogue (you can interact with npc again after a small delay)
    private IEnumerator EndDialogueDelay()
    {
        playerIndex = 0;
        nPCIndex = 0;

        playerDialgueBubble.SetActive(false);
        nPCDialgueBubble.SetActive(false);
        hasStartedDialoguePlayer = false;
        hasStartedDialogueNPC = false;
        canPlayBubbleAnim = false;
        canStartDialogue = false;
        hasSetPivot = false;
        hasSetBubbleDefaultPosX = false;
        hasSetBubbleDefaultPosY = false;
        hasAlertBubble = false;
        isInteractingWithNPC = false;
        hasSelectedDialogueOption = false;
        cameraScript.canMoveCamera = true;
        cameraScript.canMoveToDialogueViews = false;
        fidgetAnimControllerNPC.inCharacterDialogue = false;
        fidgetAnimControllerPlayer.inCharacterDialogue = false;

        EmptyTextComponents();
        SetDialogueBarsCheck();
        dialogueBarsScript.TurnOnHUD();
        nPCScript.ResetRotationNPC();

        yield return new WaitForSeconds(0.4f);
        canAlertBubble = true;
        yield return new WaitForSeconds(0.1f);
        fidgetAnimControllerNPC.hasPlayedGreetAnimNPC = false;
        fidgetAnimControllerPlayer.hasPlayedGreetAnimPlayer = false;
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
            hasStartedDialoguePlayer = true;
            fidgetAnimControllerPlayer.FidgetAnimCheck();                 
        }            

        nPCDialgueBubble.SetActive(false);
        playerDialgueBubble.SetActive(true);
        PlayDialogueBubbleSFXCheck();
        SetDialogueBubblePivot();

        yield return new WaitForSeconds(speechBubbleAnimationDelay);
        EmptyTextComponents(); 
        whitePlayerText.text = playerDialogueSentences[playerIndex];

        foreach (char letter in playerDialogueSentences[playerIndex].ToCharArray())
        {
            playerDialogueText.text += letter;
            charNoise.Play();          
            yield return new WaitForSeconds(typingSpeed);
        }

        WhoSpeaksNextCheck();
        continueButton.SetActive(true);

        //yield return new WaitForSeconds(sentenceDelay);
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
            hasStartedDialogueNPC = true;
            fidgetAnimControllerNPC.FidgetAnimCheck();
        }

        playerDialgueBubble.SetActive(false);
        nPCDialgueBubble.SetActive(true);
        PlayDialogueBubbleSFXCheck();
        SetDialogueBubblePivot();

        yield return new WaitForSeconds(speechBubbleAnimationDelay);
        EmptyTextComponents();
        whiteNPCText.text = nPCDialogueSentences[nPCIndex];

        foreach (char letter in nPCDialogueSentences[nPCIndex].ToCharArray())
        {
            nPCDialogueText.text += letter;
            charNoise.Play();
            yield return new WaitForSeconds(typingSpeed);
        }

        WhoSpeaksNextCheck();
        continueButton.SetActive(true);

        //yield return new WaitForSeconds(sentenceDelay);
        //ContinueDialogueCheck();      
    }


    // Make a notification pop up for the puzzles
    // Create a unique sfx to play for each npc to give them their own personality (charNoise)
    // Come up with sample dialogue for All characters
    // Import and set up Whatever NPC's and Artifacts we have
    // Polish the artifact interactive rotation thingy
    // Make the continue game show up only when a save has been loaded (in  the main menu)

    // ***Extras***
    // Set all Bridges to gloabal illumination - colors are too dark, didn't notice due to monitors insufficient color accuracy - Move bridge far away, then bake light
    // Make the dialogue arrow shrink when you press enter and rescale it to normal size when you unpress enter
    // Look into making the dialogue arrow animation via code (tweaning)
    // Make pressing r reset the dialogue options (dialogue options disapear when already answered)?
    // Fix player movemnt script - the phasing through the blocks
    // Make a on time dilaogue prompt for when the playr is near the generator
}
