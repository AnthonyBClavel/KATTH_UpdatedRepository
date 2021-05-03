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
    public TextAsset interactablesDialogue;
    public TextAsset artifcatsDialogue;
    public TextAsset finishedTutorialDialogue;

    public TextAsset bridgeDialogue;
    public TextAsset torchMeterDialogue;
    public TextAsset deathDialogue;
    private TextAsset currentDialogue;

    private GameObject continueButtonCD;

    private bool hasPlayedStart = false;
    private bool hasPlayedPush = false;
    private bool hasPlayedBreak = false;
    private bool hasPlayedHole = false;
    private bool hasPlayedFirestone = false;
    private bool hasPassedBridge = false;
    private bool hasPlayedInteractables = false;
    private bool hasPlayedArtifacts = false;
    private bool hasplayedFinishedTutorial = false;
    //private bool hasPlayedTorch = false;
    //private bool hasDied = false;

    private TutorialDialogueManager tutorialDialogueManagerScript;
    private TileMovementController playerScript;
    private PauseMenu pauseMenuScript;
    private ArtifactScript artifactScript;
    private SkipButton skipTurorialButtonScript;
    private CharacterDialogue characterDialogueScript;

    void Awake()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        tutorialDialogueManagerScript = FindObjectOfType<TutorialDialogueManager>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        artifactScript = FindObjectOfType<ArtifactScript>();
        skipTurorialButtonScript = FindObjectOfType<SkipButton>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();

        continueButtonCD = characterDialogueScript.continueButton;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    private void setDialogue(TextAsset dialogue)
    {
        tutorialDialogueManagerScript.setDialogue(tutorialDialogueManagerScript.readTextFile(dialogue));
    }

    private void startDialogue()
    {
        tutorialDialogueManagerScript.startDialogue();
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerScript.isWalking && !pauseMenuScript.isChangingScenes)
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

            if (playerScript.checkIfOnCheckpoint())
            {
                if (playerScript.puzzle.name == "Puzzle01" && !hasPlayedStart)
                {
                    setDialogue(startupDialogue);
                    currentDialogue = startupDialogue;
                    startDialogue();
                    hasPlayedStart = true;
                }

                else if (playerScript.puzzle.name == "Puzzle03" && !hasPlayedHole)
                {
                    setDialogue(holeDialogue);
                    currentDialogue = holeDialogue;
                    startDialogue();
                    hasPlayedHole = true;
                }

                else if (playerScript.puzzle.name == "Puzzle05" && !hasPlayedInteractables)
                {
                    setDialogue(interactablesDialogue);
                    currentDialogue = interactablesDialogue;
                    startDialogue();
                    hasPlayedInteractables = true;
                }

                else if (playerScript.puzzle.name == "Puzzle06" && !hasplayedFinishedTutorial)
                {
                    setDialogue(finishedTutorialDialogue);
                    currentDialogue = finishedTutorialDialogue;
                    startDialogue();
                    hasplayedFinishedTutorial = true;
                }
            }          
        }

        else if (playerScript.onLastTileBlock() && !hasPassedBridge)
        {
            setDialogue(bridgeDialogue);
            currentDialogue = bridgeDialogue;
            startDialogue();
            hasPassedBridge = true;
        }

        if (artifactScript.isInspectingArtifact && !hasPlayedArtifacts)
        {
            setDialogue(artifcatsDialogue);
            currentDialogue = artifcatsDialogue;
            startDialogue();

            continueButtonCD.SetActive(false);
            artifactScript.canRotateArtifact = false;
            hasPlayedArtifacts = true;
        }

        if (playerScript.hasDied)
        {
            playerScript.SetPlayerBoolsFalse();

            if (!tutorialDialogueManagerScript.inDialogue)
            {
                setDialogue(deathDialogue);
                currentDialogue = deathDialogue;
                startDialogue();
            }
        }

    }

}
