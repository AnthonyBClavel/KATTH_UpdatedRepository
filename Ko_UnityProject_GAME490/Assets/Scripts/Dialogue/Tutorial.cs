using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public TextAsset startupDialogue;
    public TextAsset pushDialogue;
    public TextAsset breakDialogue;
    public TextAsset holeDialogue;
    public TextAsset firestoneDialogue;

    public TextAsset bridgeDialogue;
    public TextAsset torchMeterDialogue;
    public TextAsset deathDialogue;
    private TextAsset currentDialogue;

    private bool hasPlayedStart = false;
    private bool hasPlayedPush = false;
    private bool hasPlayedBreak = false;
    private bool hasPlayedHole = false;
    private bool hasPlayedFirestone = false;
    private bool hasPassedBridge = false;
    //private bool hasPlayedTorch = false;
    //private bool hasDied = false;

    private DialogueManager dialogueManagerScript;
    private TileMovementController playerScript;
    private PauseMenu pauseMenuScript;

    void Awake()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        dialogueManagerScript = FindObjectOfType<DialogueManager>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    private void setDialogue(TextAsset dialogue)
    {
        dialogueManagerScript.setDialogue(dialogueManagerScript.readTextFile(dialogue));
    }

    private void startDialogue()
    {
        dialogueManagerScript.startDialogue();
    }

    // Update is called once per frame
    void Update()
    { 
        if (dialogueManagerScript.hasStarted && !hasPlayedStart)
        {
            setDialogue(startupDialogue);
            currentDialogue = startupDialogue;
            startDialogue();
            hasPlayedStart = true;
        }

        else if (!playerScript.isWalking && !pauseMenuScript.isChangingScenes)
        {
            Collider collider = playerScript.getCollider();
            if (collider != null)
                switch (collider.tag)
                {
                    case ("Obstacle"):
                        if (!hasPlayedPush)
                        {
                            setDialogue(pushDialogue);
                            currentDialogue = pushDialogue;
                            startDialogue();
                            hasPlayedPush = true;
                        }
                        break;
                    case ("DestroyableBlock"):
                        if (!hasPlayedBreak)
                        {
                            setDialogue(breakDialogue);
                            currentDialogue = breakDialogue;
                            startDialogue();
                            hasPlayedBreak = true;
                        }
                        break;
                    case ("FireStone"):
                        if (!hasPlayedFirestone)
                        {
                            setDialogue(firestoneDialogue);
                            currentDialogue = firestoneDialogue;
                            startDialogue();
                            hasPlayedFirestone = true;
                        }
                        break;
                    default:
                        break;
                }

            else if (playerScript.onLastTileBlock() && !hasPassedBridge)
            {
                setDialogue(bridgeDialogue);
                currentDialogue = bridgeDialogue;
                startDialogue();
                hasPassedBridge = true;
            }

            else if (playerScript.checkIfOnCheckpoint() && playerScript.puzzle.name == "Puzzle03" && !hasPlayedHole)
            {
                setDialogue(holeDialogue);
                currentDialogue = holeDialogue;
                startDialogue();
                hasPlayedHole = true;
            }
        }

        if (playerScript.hasDied)
        {
            playerScript.SetPlayerBoolsFalse();

            if (!dialogueManagerScript.inDialogue)
            {
                setDialogue(deathDialogue);
                currentDialogue = deathDialogue;
                startDialogue();
            }
        }

    }

}
