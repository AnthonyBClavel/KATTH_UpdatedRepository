using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public GameObject player;
    public GameObject dialogueManager;

    private DialogueManager dialogueScript;

    public TextAsset startupDialogue;
    public TextAsset pushDialogue;
    public TextAsset breakDialogue;
    public TextAsset holeDialogue;
    public TextAsset firestoneDialogue;

    public TextAsset bridgeDialogue;
    public TextAsset torchMeterDialogue;
    public TextAsset deathDialogue;

    private bool hasPlayedStart = false;
    private bool hasPlayedTorch = false;
    private bool hasDied = false;
    private bool hasPlayedPush = false;
    private bool hasPlayedBreak = false;
    private bool hasPlayedHole = false;
    private bool hasPlayedFirestone = false;
    private bool hasPassedBridge = false;

    private TextAsset currentDialogue;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void setDialogue(TextAsset dialogue)
    {
        dialogueManager.GetComponent<DialogueManager>().setDialogue(dialogueManager.GetComponent<DialogueManager>().readTextFile(dialogue));
    }

    private void startDialogue()
    {
        dialogueManager.GetComponent<DialogueManager>().startDialogue();
    }

    // Update is called once per frame
    void Update()
    { 

        if (dialogueManager.GetComponent<DialogueManager>().hasStarted && !hasPlayedStart)
        {
            setDialogue(startupDialogue);
            currentDialogue = startupDialogue;
            startDialogue();
            hasPlayedStart = true;
        }

        else if (!player.GetComponent<TileMovementV2>().isWalking)
        {
            Collider collider = player.GetComponent<TileMovementV2>().getCollider();
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

            else if (player.GetComponent<TileMovementV2>().onBridge() && !hasPassedBridge)
            {
                setDialogue(bridgeDialogue);
                currentDialogue = bridgeDialogue;
                startDialogue();
                hasPassedBridge = true;
            }

            else if (player.GetComponent<TileMovementV2>().checkIfOnCheckpoint() && player.GetComponent<TileMovementV2>().puzzle.name == "Puzzle3" && !hasPlayedHole)
            {
                setDialogue(holeDialogue);
                currentDialogue = holeDialogue;
                startDialogue();
                hasPlayedHole = true;
            }
        }

        if (player.GetComponent<TileMovementV2>().hasDied)
        {
            player.GetComponent<TileMovementV2>().enabled = false;
            if (!dialogueManager.GetComponent<DialogueManager>().inDialogue)
            {
                setDialogue(deathDialogue);
                currentDialogue = deathDialogue;
                startDialogue();
            }
        }
    }
}
