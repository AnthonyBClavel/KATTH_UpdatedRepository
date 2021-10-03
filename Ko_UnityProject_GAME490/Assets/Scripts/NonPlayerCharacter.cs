using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayerCharacter : MonoBehaviour
{
    [Header("Bools")]
    public bool hasPlayedOptionOne = false;
    public bool hasPlayedOptionTwo = false;
    public bool hasLoadedInitialDialogue = false;

    private string characterName;
    private Color32 nPCTextColor;

    private GameObject characterHolder;
    private GameObject nPCDialogueCheck;
    private Vector3 originalRotation;

    Vector3 up = Vector3.zero,
    right = new Vector3(0, 90, 0),
    down = new Vector3(0, 180, 0),
    left = new Vector3(0, 270, 0);

    [Header("NPC Dialogue Array")]
    public TextAsset[] nPCDialogueFiles;
    public TextAsset[] playerDialogueFiles;
    public TextAsset dialogueOptionsFile;

    private TileMovementController playerScript;
    private CharacterDialogue characterDialogueScript;
    private NPCFidgetController nPCFidgetScript;

    void Awake()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        nPCFidgetScript = GetComponent<NPCFidgetController>();

        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        originalRotation = characterHolder.transform.localEulerAngles;
        SetDialogueTextColor();
    }

    // Returns the name of the NPC
    public string ReturnCharacterName()
    {
        return characterName;
    }

    // Returns the text color to use for the NPC's dialogue bubble
    public Color32 ReturnTextColor()
    {
        return nPCTextColor;
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
        if (playerScript.playerDirection == up)
            characterHolder.transform.localEulerAngles = down;

        if (playerScript.playerDirection == left)
            characterHolder.transform.localEulerAngles = right;

        if (playerScript.playerDirection == down)
            characterHolder.transform.localEulerAngles = up;

        if (playerScript.playerDirection == right)
            characterHolder.transform.localEulerAngles = left;
    }

    // Sets the name of the NPC and the text color
    private void SetDialogueTextColor()
    {
        characterName = gameObject.name;
        Color32 textColor;

        if (characterName == "VillageElder")
            textColor = new Color32(58, 78, 112, 255);

        else if (characterName == "Fisherman")
            textColor = new Color32(194, 130, 104, 255);

        else if (characterName == "VillageExplorer01")
            textColor = new Color32(115, 106, 142, 255);

        else if (characterName == "FriendlyGhost")
            textColor = new Color32(96, 182, 124, 255);

        else if (characterName == "VillageExplorer02")
            textColor = new Color32(155, 162, 125, 255);

        else if (characterName == "BabyMammoth")
            textColor = new Color32(196, 146, 102, 255);

        else
            textColor = Color.black;

        nPCTextColor = textColor;
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
    }

}
