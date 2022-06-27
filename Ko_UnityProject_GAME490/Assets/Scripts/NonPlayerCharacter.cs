using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayerCharacter : MonoBehaviour
{
    public NonPlayerCharacter_SO nonPlayerCharacter;

    private bool hasPlayedInitialDialogue = false;
    private bool[] dialogueOptionBools;

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

    // Creates a new array of bools - for determining whether a dialogue option has already been played or not
    public void SetDialogueOptionBools(int arraySize)
    {
        if (dialogueOptionBools == null || dialogueOptionBools.Length != arraySize)
        {
            dialogueOptionBools = new bool[arraySize];

            for (int i = 0; i < dialogueOptionBools.Length; i++)
            {
                dialogueOptionBools[i] = false;
            }
        }
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        nPCFidgetScript = GetComponentInChildren<FidgetController>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets them by looking at the names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "CharacterHolder")
                characterHolder = child;

            if (child.name == "DialogueCheck")
                nPCDialogueCheck = child;
        }

        originalRotation = characterHolder.transform.eulerAngles;
    }

    // Sets text color to use for the NPC's dialogue bubble - For Reference (shows the original text colors for each npc)
    /*private void SetDialogueTextColor()
    {
        switch (nonPlayerCharacter.nPCName)
        {
            case ("BabyMammoth"):
                nPCTextColor = new Color32(196, 146, 102, 255);
                break;
            case ("FirstVillageExplorer"):
                nPCTextColor = new Color32(115, 106, 142, 255);
                break;
            case ("Fisherman"):
                nPCTextColor = new Color32(194, 130, 104, 255);
                break;
            case ("FriendlyGhost"):
                nPCTextColor = new Color32(96, 182, 124, 255);
                break;
            case ("SecondVillageExplorer"):
                nPCTextColor = new Color32(155, 162, 125, 255);
                break;
            case ("VillageElder"):
                nPCTextColor = new Color32(58, 78, 112, 255);
                break;
            default:
                nPCTextColor = Color.black;
                break;
        }
    }*/

}
