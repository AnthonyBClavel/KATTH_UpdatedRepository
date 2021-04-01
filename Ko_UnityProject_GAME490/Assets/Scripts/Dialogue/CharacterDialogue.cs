using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CharacterDialogue : MonoBehaviour
{
    public float typingSpeed = 0.03f;
    private float speechBubbleAnimationDelay;

    private int playerIndex;
    private int nPCIndex;

    Vector3 up = Vector3.zero,
    right = new Vector3(0, 90, 0),
    down = new Vector3(0, 180, 0),
    left = new Vector3(0, 270, 0);

    private Vector2 playerBubbleOrigPos;
    private Vector2 npcBubbleOrigPos;
    private Vector2 moveBubbleRight;
    private Vector2 moveBubbleLeft;
    
    private Vector2 playerOriginalPivot;
    private Vector2 npcOriginalPivot;
    private Vector2 movePivotRight;
    private Vector2 movePivotLeft;

    [Header("Audio")]
    public AudioSource charNoise;
    public AudioClip dialoguePopUpSFX;
    private AudioSource audioSource;

    [Header("Animator")]
    public Animator playerBubbleAnim;
    public Animator nPCBubbleAnim;

    [Header("TextMeshPro")]
    // Note: the white text is used to "calculate" the size of the bubble (to fit the text), dialogue text will overlay white text once size is found
    public TextMeshProUGUI whitePlayerText; 
    public TextMeshProUGUI whiteNPCText;
    public TextMeshProUGUI playerDialogueText;
    public TextMeshProUGUI nPCDialogueText;

    [Header("GameObjects")]
    public GameObject playerDialgueBubble;
    public GameObject nPCDialgueBubble;
    public GameObject continueButton;

    [Header("GameObjects")]
    public RectTransform playerSpeechBubbleHolder;
    public RectTransform nPCSpeechBubbleHolder;

    [Header("Bools")]
    public bool isPlayerSpeaking;
    public bool canPlayBubbleAnim = false;
    public bool canStartDialogue = true;
    private bool hasStartedDialogue = false;
    private bool hasPlayedPopUpSFX = false;
    private bool hasSetDialogueBars = false;
    private bool hasSetPivot = false;

    [Header("NPC Bools")]
    public bool isVillageEdler;
    public bool isFisherman;
    public bool isVillageExplorer01;
    public bool isFriendlyGhost;
    public bool isVillageExplorer02;

    [Header("Dialogue Sentences")]
    [TextArea]
    public string[] playerDialogueSetences;
    [TextArea]
    public string[] nPCDialogueSetences;

    private DialogueBars dialogueBarsScript;
    private TileMovementController playerScript;
    private PauseMenu pauseMenuScript;
    private NonPlayerCharacter nPCScript;

    void Awake()
    {
        dialogueBarsScript = FindObjectOfType<DialogueBars>();
        playerScript = FindObjectOfType<TileMovementController>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        nPCScript = FindObjectOfType<NonPlayerCharacter>();
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        continueButton.SetActive(false);
        nPCDialgueBubble.SetActive(false);
        playerDialgueBubble.SetActive(false);

        playerBubbleOrigPos = new Vector2(0, 78);
        npcBubbleOrigPos = new Vector2(0, 78);
        moveBubbleRight = new Vector2(100f, 78);
        moveBubbleLeft = new Vector2(-100f, 78);

        playerOriginalPivot = new Vector2(0.5f, 0);
        npcOriginalPivot = new Vector2(0.5f, 0);
        movePivotRight = new Vector2(1, 0);
        movePivotLeft = new Vector2(0, 0);
    }
        

    // Update is called once per frame
    void Update()
    {
        /*if (pauseMenuScript.isPaused)
        {
            nPCDialgueBubble.transform.localScale = new Vector3(0, 0, 0);
            playerDialgueBubble.transform.localScale = new Vector3(0, 0, 0);
        }
        else if (!pauseMenuScript.isPaused)
        {
            nPCDialgueBubble.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            playerDialgueBubble.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        }*/

        /*if (Input.GetKeyDown(KeyCode.L))
        {
            //StartNPCDialogue();             
        }

        if(Input.GetKeyDown(KeyCode.R) && canStartDialogue)
        {
            StopCoroutine("TypePlayerDialogue");
            StopCoroutine("TypeNPCDialogue");

            playerDialgueBubble.SetActive(false);
            nPCDialgueBubble.SetActive(false);
            continueButton.SetActive(false);
            nPCScript.ResetRotationNPC();

            EmptyTextComponents();
            playerIndex = 0;
            nPCIndex = 0;

            isPlayerSpeaking = true;
            hasStartedDialogue = false;
            canPlayBubbleAnim = false;
            hasPlayedPopUpSFX = false;
            hasSetPivot = false;
        }

        AnimBubbleDelayCheck();
        ContinueButtonCheck();*/
    }

    // Starts the dialogue with an npc
    public void StartNPCDialogue()
    {
        if (canStartDialogue)
        {
            StartCoroutine("StartDialogueDelay");
            nPCScript.SetRotationNPC();
            canStartDialogue = false;
        }
    }

    // Checks for when the player can load the next dialogue sentence
    private void ContinueButtonCheck()
    {
        if (Input.GetKeyDown(KeyCode.Return) && continueButton.activeSelf && !pauseMenuScript.isChangingScenes && !pauseMenuScript.isPaused)
        {
            EndDialogueCheck();
            ContinuePlayerDialogueCheck();
            ContinueNPCDialogueCheck();
            continueButton.SetActive(false);
        }
    }

    // Checks if the player dialogue continues
    private void ContinuePlayerDialogueCheck()
    {
        if (playerIndex < playerDialogueSetences.Length - 1 && isPlayerSpeaking)
        {
            if (hasStartedDialogue)
                playerIndex++;
            else
                hasStartedDialogue = true;

            StartCoroutine("TypePlayerDialogue");
        }
    }

    // Checks oif the npc dilaogue continues
    private void ContinueNPCDialogueCheck()
    {
        if (nPCIndex < nPCDialogueSetences.Length - 1 && !isPlayerSpeaking)
        {
            if (hasStartedDialogue)
                nPCIndex++;
            else
                hasStartedDialogue = true;

            StartCoroutine("TypeNPCDialogue");
        }
    }

    //Check when the dialogue has ended
    private void EndDialogueCheck()
    {
        if (playerIndex == playerDialogueSetences.Length - 1)
        {
            StartCoroutine("EndDialogueDelay");
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

        else if (hasSetDialogueBars && !hasStartedDialogue)
        {
            dialogueBarsScript.ToggleDialogueBars();
            hasSetDialogueBars = false;
        }
    }

    // Checks if the dialogue pop up sfx can be played - played only once when the dialogue bubble is activated/re-activated
    private void PlayDialogueBubbleSFXCheck()
    {
        if (!hasPlayedPopUpSFX && isPlayerSpeaking)
        {
            audioSource.volume = 0.24f;
            audioSource.pitch = 0.55f;
            audioSource.PlayOneShot(dialoguePopUpSFX);
            hasPlayedPopUpSFX = true;
        }
        if (!hasPlayedPopUpSFX && !isPlayerSpeaking)
        {
            audioSource.volume = 0.24f;
            audioSource.pitch = 0.6f;
            audioSource.PlayOneShot(dialoguePopUpSFX);
            hasPlayedPopUpSFX = true;
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

    // Checks for when the player and npc can talk
    private void DialogueOrderCheck()
    {
        if (isPlayerSpeaking)
        {
            if (playerIndex == 0 || playerIndex == 1 || playerIndex == 3 || playerIndex == 4)
            {
                isPlayerSpeaking = false;
                hasPlayedPopUpSFX = false;
            }            
        }
        else if (!isPlayerSpeaking)
        {
            if (nPCIndex == 1 || nPCIndex == 4 || nPCIndex == 10 || nPCIndex == 13)
            {
                isPlayerSpeaking = true;
                hasPlayedPopUpSFX = false;
            }             
        }       
    }

    // Sets a specific text color for each npc
    private void SetDialogueTextColor()
    {
        if (isVillageEdler)
            nPCDialogueText.color = new Color32(58, 78, 112, 255);
        if (isFisherman)
            nPCDialogueText.color = new Color32(194, 130, 104, 255);
        if (isVillageExplorer01)
            nPCDialogueText.color = new Color32(115, 106, 142, 255);
        if (isFriendlyGhost)
            nPCDialogueText.color = Color.black;
        if(isVillageExplorer02)
            nPCDialogueText.color = new Color32(155, 162, 125, 255);
    }

    private void SetDialogueBubblePivot()
    {
        if(!hasSetPivot)
        {
            if (playerScript.playerDirection == up || playerScript.playerDirection == down)
            {
                playerSpeechBubbleHolder.pivot = playerOriginalPivot; 
                playerSpeechBubbleHolder.localPosition = playerBubbleOrigPos;
                nPCSpeechBubbleHolder.pivot = npcOriginalPivot;
                nPCSpeechBubbleHolder.localPosition = npcBubbleOrigPos;
            }

            if (playerScript.playerDirection == left)
            {
                playerSpeechBubbleHolder.pivot = movePivotLeft;
                playerSpeechBubbleHolder.localPosition = moveBubbleLeft;
                nPCSpeechBubbleHolder.pivot = movePivotRight;
                nPCSpeechBubbleHolder.localPosition = moveBubbleRight;
            }

            if (playerScript.playerDirection == right)
            {
                playerSpeechBubbleHolder.pivot = movePivotRight;
                playerSpeechBubbleHolder.localPosition = moveBubbleRight;
                nPCSpeechBubbleHolder.pivot = movePivotLeft;
                nPCSpeechBubbleHolder.localPosition = moveBubbleLeft;
            }

            hasSetPivot = true;
        }
           
    }

    // Clears the text within all text components - before new text is set
    private void EmptyTextComponents()
    {
        nPCDialogueText.text = string.Empty;
        playerDialogueText.text = string.Empty;
        whitePlayerText.text = string.Empty;
        whiteNPCText.text = string.Empty;
    }

    // Checks if the player is speaking first or not
    private void StartDialogueCheck()
    {
        if (isPlayerSpeaking)
            StartCoroutine("TypePlayerDialogue");
        else
            StartCoroutine("TypeNPCDialogue");
    }

    // Starts the dialogue after a delay
    private IEnumerator StartDialogueDelay()
    {
        SetDialogueBarsCheck();
        dialogueBarsScript.TurnOffHUD();
        playerScript.SetPlayerBoolsFalse();
        SetDialogueTextColor();

        yield return new WaitForSeconds(0.5f);
        StartDialogueCheck();
    }

    // Ends the dialogue (you can interact with npc again after a small delay)
    private IEnumerator EndDialogueDelay()
    {
        playerDialgueBubble.SetActive(false);
        nPCDialgueBubble.SetActive(false);
        continueButton.SetActive(false);
        hasStartedDialogue = false;
        canPlayBubbleAnim = false;
        canStartDialogue = false;
        hasPlayedPopUpSFX = false;
        hasSetPivot = false;

        SetDialogueBarsCheck();
        dialogueBarsScript.TurnOnHUD();
        nPCScript.ResetRotationNPC();

        yield return new WaitForSeconds(0.5f);
        playerScript.SetPlayerBoolsTrue();
        canStartDialogue = true;
    }

    // Types the next sentence for the player
    private IEnumerator TypePlayerDialogue()
    {
        if (canPlayBubbleAnim && playerDialgueBubble.activeSelf)
            playerBubbleAnim.SetTrigger("NextSentence");
        else
            canPlayBubbleAnim = true;

        nPCDialgueBubble.SetActive(false);
        playerDialgueBubble.SetActive(true);
        PlayDialogueBubbleSFXCheck();
        SetDialogueBubblePivot();

        yield return new WaitForSeconds(speechBubbleAnimationDelay);
        EmptyTextComponents();
        DialogueOrderCheck();
        whitePlayerText.text = playerDialogueSetences[playerIndex];

        foreach (char letter in playerDialogueSetences[playerIndex].ToCharArray())
        {
            playerDialogueText.text += letter;
            charNoise.Play();          
            yield return new WaitForSeconds(typingSpeed);
        }

        continueButton.SetActive(true);
    }

    // Types the next sentence for the npc
    private IEnumerator TypeNPCDialogue()
    {
        if (canPlayBubbleAnim && nPCDialgueBubble.activeSelf)
            nPCBubbleAnim.SetTrigger("NextSentence");
        else
            canPlayBubbleAnim = true;
        
        playerDialgueBubble.SetActive(false);
        nPCDialgueBubble.SetActive(true);
        PlayDialogueBubbleSFXCheck();
        SetDialogueBubblePivot();

        yield return new WaitForSeconds(speechBubbleAnimationDelay);
        EmptyTextComponents();
        DialogueOrderCheck();
        whiteNPCText.text = nPCDialogueSetences[nPCIndex];

        foreach (char letter in nPCDialogueSetences[nPCIndex].ToCharArray())
        {
            nPCDialogueText.text += letter;
            charNoise.Play();
            yield return new WaitForSeconds(typingSpeed);
        }

        continueButton.SetActive(true);
    }

    //NOTES
    //Player
    //0 = 446
    //1 = 610
    //2 = 210
    //3 = 702
    //4 = 336
    //5 = 282

    //NPC
    //0 = 190
    //1 = 320
    //2 = 815
    //3 = 190
    //4 = 866
    //5 = 200
    //6 = 742
    //7 = 190
    //8 = 240
    //9 = 425
    //10 = 295
    //11 = 230
    //12 = 290


}
