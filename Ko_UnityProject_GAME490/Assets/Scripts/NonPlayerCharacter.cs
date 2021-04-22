using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayerCharacter : MonoBehaviour
{
    Vector3 up = Vector3.zero,
    right = new Vector3(0, 90, 0),
    down = new Vector3(0, 180, 0),
    left = new Vector3(0, 270, 0);

    public string characterName;

    [Header("NPC Character")]
    public GameObject nPC;
    public Transform nPCDialogueCheck;
    private Vector3 originalRotation;

    [Header("NPC Dialogue Array")]
    public TextAsset[] nPCDialogueFiles;
    public TextAsset dialogueQuestionsFile;

    [Header("Bools")]
    public bool hasPlayedOptionOne = false;
    public bool hasPlayedOptionTwo = false;
    public bool hasLoadedInitialDialogue = false;

    private TileMovementController playerScript;

    void Awake()
    {
        playerScript = FindObjectOfType<TileMovementController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        originalRotation = nPC.transform.localEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Rotates the npc so that it faces the player
    public void SetRotationNPC()
    {
        if (playerScript.playerDirection == up)
            nPC.transform.localEulerAngles = down;

        if (playerScript.playerDirection == left)
            nPC.transform.localEulerAngles = right;

        if (playerScript.playerDirection == down)
            nPC.transform.localEulerAngles = up;

        if (playerScript.playerDirection == right)
            nPC.transform.localEulerAngles = left;
    }

    // Sets the npc back to its original rotation
    public void ResetRotationNPC()
    {
        nPC.transform.localEulerAngles = originalRotation;
    }

}
