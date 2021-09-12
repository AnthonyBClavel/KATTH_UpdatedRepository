using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayerCharacter : MonoBehaviour
{
    public string characterName;

    [Header("Bools")]
    public bool hasPlayedOptionOne = false;
    public bool hasPlayedOptionTwo = false;
    public bool hasLoadedInitialDialogue = false;

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
    private FidgetAnimControllerNPC fidgeControllerScriptNPC;

    void Awake()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();

        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        originalRotation = characterHolder.transform.localEulerAngles;
    }

    // Updates all npc elements in the character dialogue script - also starts dialogue
    public void SetVariablesForCharacterDialogueScript()
    {
        characterDialogueScript.UpdateDialogueCheckForNPC(nPCDialogueCheck);
        characterDialogueScript.UpdateScriptForNPC(this);
        characterDialogueScript.UpdateFidgetScriptForNPC(fidgeControllerScriptNPC);

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
            {
                characterHolder = child;
                fidgeControllerScriptNPC = characterHolder.GetComponent<FidgetAnimControllerNPC>();

                for (int j = 0; j < characterHolder.transform.childCount; j++)
                {
                    GameObject child02 = characterHolder.transform.GetChild(j).gameObject;

                    if (child02.name == "DialogueCheck")
                        nPCDialogueCheck = child02;
                }
            }
        }
    }

}
