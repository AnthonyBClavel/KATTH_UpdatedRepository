using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayerCharacter : MonoBehaviour
{
    public NonPlayerCharacter_SO nonPlayerCharacter;

    private bool hasPlayedInitialDialogue = false;
    private bool[] dialogueOptionBools = new bool[0];

    private GameObject characterHolder;
    private GameObject nPCDialogueCheck;

    private Vector3 originalRotation;
    Vector3 north = Vector3.zero,
    east = new Vector3(0, 90, 0),
    south = new Vector3(0, 180, 0),
    west = new Vector3(0, 270, 0);

    private TileMovementController playerScript;
    private FidgetController nPCFidgetScript;

    public bool[] DialogueOptionBools
    {
        get { return dialogueOptionBools; }
        set { dialogueOptionBools = value; }
    }

    public bool HasPlayedInitialDialogue
    {
        get { return hasPlayedInitialDialogue; }
        set { hasPlayedInitialDialogue = value; }
    }

    public FidgetController FidgetScript
    {
        get { return nPCFidgetScript; }
    }

    public GameObject DialogueCheck
    {
        get { return nPCDialogueCheck; }
    }

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Sets the npc back to its original rotation
    public void ResetRotation() => characterHolder.transform.eulerAngles = originalRotation;

    // Rotates the npc towards the player
    public void SetRotation()
    {
        Vector3 playerDirection = playerScript.transform.eulerAngles;

        switch (playerDirection.y)
        {
            case 0: // Looking north
                characterHolder.transform.eulerAngles = south;
                break;
            case 90: // Looking east
                characterHolder.transform.eulerAngles = west;
                break;
            case 180: // Looking south
                characterHolder.transform.eulerAngles = north;
                break;
            case 270: // Looking west
                characterHolder.transform.eulerAngles = east;
                break;
            default:
                //Debug.Log("Unrecognizable direction");
                break;
        }
    }

    // Creates a new array of dialogue option bools
    // Note: the bools are used to determine if a dialogue option has been played or not
    public void SetDialogueOptionBools(int arraySize)
    {
        if (dialogueOptionBools.Length != 0 || dialogueOptionBools.Length == arraySize) return;

        dialogueOptionBools = new bool[arraySize];

        for (int i = 0; i < dialogueOptionBools.Length; i++)
        {
            dialogueOptionBools[i] = false;
        }
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        nPCFidgetScript = GetComponentInChildren<FidgetController>();
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "CharacterHolder":
                    characterHolder = child.gameObject;
                    break;
                case "DialogueCheck":
                    nPCDialogueCheck = child.gameObject;
                    break;
                default:
                    break;
            }

            if (child.parent.name == "CharacterHolder") continue;
            SetVariables(child);
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        SetVariables(transform);
        originalRotation = characterHolder.transform.eulerAngles;
    }

}
