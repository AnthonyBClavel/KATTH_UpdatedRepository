using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayerCharacter : MonoBehaviour
{
    public NonPlayerCharacter_SO nonPlayerCharacter;

    private bool hasPlayedOptionOne = false;
    private bool hasPlayedOptionTwo = false;
    private bool hasPlayedInitialDialogue = false;

    private GameObject characterHolder;
    private GameObject nPCDialogueCheck;
    private Vector3 originalRotation;

    Vector3 up = Vector3.zero, // Look North
    right = new Vector3(0, 90, 0), // Look East
    down = new Vector3(0, 180, 0), // Look South
    left = new Vector3(0, 270, 0); // Look West

    private TileMovementController playerScript;
    private FidgetController nPCFidgetScript;

    public bool HasPlayedOptionOne
    {
        get { return hasPlayedOptionOne; }
        set { hasPlayedOptionOne = value; }
    }

    public bool HasPlayedOptionTwo
    {
        get { return hasPlayedOptionTwo; }
        set { hasPlayedOptionTwo = value; }
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

    // Start is called before the first frame update
    void Start()
    {
        originalRotation = characterHolder.transform.localEulerAngles;
    }

    // Sets the npc back to its original rotation
    public void ResetRotationNPC() => characterHolder.transform.localEulerAngles = originalRotation;

    // Rotates the npc towards the player
    public void SetRotationNPC()
    {
        Vector3 playerDirection = playerScript.transform.eulerAngles;

        switch (playerDirection.y)
        {
            case 0: // Looking north
                characterHolder.transform.eulerAngles = down;
                break;
            case 90: // Looking east
                characterHolder.transform.eulerAngles = left;
                break;
            case 180: // Looking south
                characterHolder.transform.eulerAngles = up;
                break;
            case 270: // Looking west
                characterHolder.transform.eulerAngles = right;
                break;
            default:
                break;
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

        //SetDialogueTextColor();
    }

    // Sets text color to use for the NPC's dialogue bubble - For Reference
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
