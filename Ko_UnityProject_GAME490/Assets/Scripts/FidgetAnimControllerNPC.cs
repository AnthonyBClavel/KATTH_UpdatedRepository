using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FidgetAnimControllerNPC : MonoBehaviour
{
    [Header("Bools")]
    public bool inCharacterDialogue = false;
    public bool hasPlayedGreetAnimNPC = false;
    private bool canFidget = true;

    [Header("NPC Variables")]
    private int idleAnimIndexNPC;
    private int fidgetIndexNPC;
    private float timesToRepeatNPC = 3;

    [Header("Character Animators")]
    private Animator animNPC;
    private GameObject dialogueOptionsBubble;

    private CharacterDialogue characterDialogueScript;
    private PauseMenu pauseMenuScript;

    void Awake()
    {
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();

        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        animNPC = GetComponent<Animator>();
        idleAnimIndexNPC = 0;
        SetTimesToRepeat();
    }

    // Plays the fidget animation after the player's idle has repeated a certain amount of times
    public void AddToIdleIndex()
    {
        if (animNPC.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !inCharacterDialogue && !characterDialogueScript.hasTransitionedToArtifactView && pauseMenuScript.canPause)
        {
            if (idleAnimIndexNPC < timesToRepeatNPC)
                idleAnimIndexNPC++;

            if (idleAnimIndexNPC >= timesToRepeatNPC)
                FidgetAnimCheck();
        }         
    }

    // Plays a fidget animation when called - cannot play a fidget animation while another is playing
    public void FidgetAnimCheck()
    {
        CanNPCFidgetCheck();

        if (canFidget)
        {
            if (!inCharacterDialogue)
            {
                if (dialogueOptionsBubble.activeSelf)
                        animNPC.SetTrigger("Fidget01");
                else
                {
                    SetRadnomAnimIndexNPC();

                    if (fidgetIndexNPC == 0)
                        animNPC.SetTrigger("Fidget01");

                    if (fidgetIndexNPC == 1)
                        animNPC.SetTrigger("Fidget02");

                    if (fidgetIndexNPC == 2)
                        animNPC.SetTrigger("Fidget03");

                    if (fidgetIndexNPC == 3)
                        animNPC.SetTrigger("Fidget04");
                }
            }

            if (inCharacterDialogue)
            {
                SetRadnomAnimIndexNPC();

                if (fidgetIndexNPC == 0)
                    animNPC.SetTrigger("Fidget01");

                if (fidgetIndexNPC == 1)
                    animNPC.SetTrigger("Fidget02");

                if (fidgetIndexNPC == 2)
                    animNPC.SetTrigger("Fidget03");

                if (fidgetIndexNPC == 3)
                    animNPC.SetTrigger("Fidget04");
            }

            idleAnimIndexNPC = 0;
            SetTimesToRepeat();
            canFidget = false;
        }
    }

    // Checks when the bool should be false or true
    private void CanNPCFidgetCheck()
    {
        if (animNPC.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            // Plays the greet anim if the bool is false and the npc isn't fidgeting - bool is set to false when dialogue ends
            if (!hasPlayedGreetAnimNPC && inCharacterDialogue && characterDialogueScript.hasStartedDialogueNPC)
            {
                animNPC.SetTrigger("Greet");
                idleAnimIndexNPC = 0;
                SetTimesToRepeat();
                canFidget = false;
                hasPlayedGreetAnimNPC = true;
            }
            else
                canFidget = true;
        }
        else
        {
            hasPlayedGreetAnimNPC = true;
            canFidget = false;
        }
    }

    // Sets a random animIndexNPC - each index has its own animation to be played
    private void SetRadnomAnimIndexNPC()
    {
        int attempts = 3;
        int newFidgetIndexNPC = UnityEngine.Random.Range(0, 4);

        while (newFidgetIndexNPC == fidgetIndexNPC && attempts > 0)
        {
            newFidgetIndexNPC = UnityEngine.Random.Range(0, 4);
            attempts--;
        }

        fidgetIndexNPC = newFidgetIndexNPC;
    }


    // Sets a random size for the timeToRepeatPlayer float - how many times the player's idle will play before triggering a fidget animation
    private void SetTimesToRepeat()
    {
        if (!inCharacterDialogue)
        {
            int attempts = 3;
            int newTimesToRepeatNPC = UnityEngine.Random.Range(3, 6);

            while (newTimesToRepeatNPC == timesToRepeatNPC && attempts > 0)
            {
                newTimesToRepeatNPC = UnityEngine.Random.Range(3, 6);
                attempts--;
            }

            timesToRepeatNPC = newTimesToRepeatNPC;
        }

        else if (inCharacterDialogue)
        {
            int attempts = 3;
            int newTimesToRepeatNPC = UnityEngine.Random.Range(2, 4);

            while (newTimesToRepeatNPC == timesToRepeatNPC && attempts > 0)
            {
                newTimesToRepeatNPC = UnityEngine.Random.Range(2, 4);
                attempts--;
            }

            timesToRepeatNPC = newTimesToRepeatNPC;
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < characterDialogueScript.transform.childCount; i++)
        {
            GameObject child = characterDialogueScript.transform.GetChild(i).gameObject;

            if (child.name == "DialogueOptionsBubble")
                dialogueOptionsBubble = child;
        }
    }

}
