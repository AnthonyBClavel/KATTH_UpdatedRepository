using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayerCharacter : MonoBehaviour
{
    private bool hasPlayedOptionOne = false;
    private bool hasPlayedOptionTwo = false;
    private bool hasPlayedInitialDialogue = false;

    private string characterName;
    private Color32 nPCTextColor;

    private GameObject characterHolder;
    private GameObject nPCDialogueCheck;
    private Vector3 originalRotation;

    Vector3 up = Vector3.zero, // Look North
    right = new Vector3(0, 90, 0), // Look East
    down = new Vector3(0, 180, 0), // Look South
    left = new Vector3(0, 270, 0); // Look West

    public TextAsset dialogueOptionsFile;
    public TextAsset[] nPCDialogueFiles;
    public TextAsset[] playerDialogueFiles;

    private TileMovementController playerScript;
    private CharacterDialogue characterDialogueScript;
    private FidgetController nPCFidgetScript;

    void Awake()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        nPCFidgetScript = GetComponentInChildren<FidgetController>();

        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        originalRotation = characterHolder.transform.localEulerAngles;
    }

    // Returns or sets the value of the bool hasPlayedOptionOne
    public bool HasPlayedOptionOne
    {
        get
        {
            return hasPlayedOptionOne;
        }
        set
        {
            hasPlayedOptionOne = value;
        }
    }

    // Returns or sets the value of the bool hasPlayedOptionTwo
    public bool HasPlayedOptionTwo
    {
        get
        {
            return hasPlayedOptionTwo;
        }
        set
        {
            hasPlayedOptionTwo = value;
        }
    }

    // Returns or sets the value of the bool hasPlayedInitialDialogue
    public bool HasPlayedInitialDialogue
    {
        get
        {
            return hasPlayedInitialDialogue;
        }
        set
        {
            hasPlayedInitialDialogue = value;
        }
    }

    // Returns the name of the NPC
    public string CharacterName
    {
        get
        {
            return characterName;
        }
    }

    // Returns the text color to use for the NPC's dialogue bubble
    public Color32 DialogueTextColor
    {
        get
        {
            return nPCTextColor;
        }
    }

    // Updates all npc elements in the character dialogue script - also starts dialogue
    public void SetVariablesForCharacterDialogueScript()
    {
        characterDialogueScript.UpdateDialogueCheckForNPC(nPCDialogueCheck);
        characterDialogueScript.UpdateScriptForNPC(this);
        characterDialogueScript.UpdateFidgetScriptForNPC(nPCFidgetScript);

        characterDialogueScript.UpdateNonPlayerCharacterName(characterName);
        characterDialogueScript.isInteractingWithNPC = true;
        characterDialogueScript.setDialogueQuestions(dialogueOptionsFile);

        characterDialogueScript.StartDialogue();
    }

    // Rotates the npc towards the player
    public void SetRotationNPC()
    {
        Vector3 playerDirection = playerScript.transform.eulerAngles;

        if (playerDirection == up)
            characterHolder.transform.localEulerAngles = down;

        else if (playerDirection == right)
            characterHolder.transform.localEulerAngles = left;

        else if (playerDirection == down)
            characterHolder.transform.localEulerAngles = up;

        else if (playerDirection == left)
            characterHolder.transform.localEulerAngles = right;
    }

    // Sets text color to use for the NPC's dialogue bubble
    private void SetDialogueTextColor()
    {
        if (characterName == "VillageElder")
            nPCTextColor = new Color32(58, 78, 112, 255);

        else if (characterName == "Fisherman")
            nPCTextColor = new Color32(194, 130, 104, 255);

        else if (characterName == "VillageExplorer01")
            nPCTextColor = new Color32(115, 106, 142, 255);

        else if (characterName == "FriendlyGhost")
            nPCTextColor = new Color32(96, 182, 124, 255);

        else if (characterName == "VillageExplorer02")
            nPCTextColor = new Color32(155, 162, 125, 255);

        else if (characterName == "BabyMammoth")
            nPCTextColor = new Color32(196, 146, 102, 255);

        else
            nPCTextColor = Color.black;
    }

    // Sets the npc back to its original rotation
    public void ResetRotationNPC()
    {
        characterHolder.transform.localEulerAngles = originalRotation;
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at the names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "CharacterHolder")
                characterHolder = child;

            if (child.name == "DialogueCheck")
                nPCDialogueCheck = child;
        }

        characterName = gameObject.name;
        SetDialogueTextColor();
    }

}
