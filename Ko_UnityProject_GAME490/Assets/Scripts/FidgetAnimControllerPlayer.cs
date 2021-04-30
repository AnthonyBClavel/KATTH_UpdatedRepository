using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FidgetAnimControllerPlayer : MonoBehaviour
{
    [Header("Player Variables")]
    private int idleAnimIndexPlayer;
    private int fidgetIndexPlayer;
    private float timesToRepeatPlayer = 3;

    [Header("Character Animators")]
    private Animator animPlayer;

    [Header("Bools")]
    public bool inCharacterDialogue = false;
    public bool hasPlayedGreetAnimPlayer = false;
    private bool canFidget = true;

    private CharacterDialogue characterDialogueScript;
    private PauseMenu pauseMenuScript;

    void Awake()
    {
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
    }

    // Start is called before the first frame update
    void Start()
    {
        animPlayer = GetComponent<Animator>();
        idleAnimIndexPlayer = 0;
        SetTimesToRepeat();
    }

    // Sets the idle index to zero
    public void SetIdleIndexToZero()
    {
        idleAnimIndexPlayer = 0;
    }

    // Plays the fidget animation after the player's idle has repeated a certain amount of times
    public void AddToIdleIndex()
    {
        if (animPlayer.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !inCharacterDialogue && !characterDialogueScript.hasTransitionedToArtifactView && pauseMenuScript.canPause)
        { 
            if (idleAnimIndexPlayer < timesToRepeatPlayer)
                idleAnimIndexPlayer++;

            if (idleAnimIndexPlayer >= timesToRepeatPlayer)
                FidgetAnimCheck();
        }
    }

    // Plays a fidget animation when called - cannot play a fidget animation while another is playing
    public void FidgetAnimCheck()
    {
        CanPlayerFidgetCheck();

        if (canFidget)
        {
            if (!inCharacterDialogue)
            {
                if (characterDialogueScript.dialogueOptionsBubble.activeSelf)
                    animPlayer.SetTrigger("Fidget03");                   
                else
                {
                    SetRadnomAnimIndexPlayer();

                    if (fidgetIndexPlayer == 0)
                        animPlayer.SetTrigger("Fidget01"); // Strecth

                    if (fidgetIndexPlayer == 1)
                        animPlayer.SetTrigger("Fidget02"); // Look At Torch

                    if (fidgetIndexPlayer == 2)
                        animPlayer.SetTrigger("Fidget03"); // Scratch Head

                    if (fidgetIndexPlayer == 3)
                        animPlayer.SetTrigger("Fidget04"); // Jumping Jacks
                }
            }
     
            if (inCharacterDialogue)
            {
                SetRadnomAnimIndexPlayer();

                if (fidgetIndexPlayer == 0)
                    animPlayer.SetTrigger("Fidget01");

                if (fidgetIndexPlayer == 1)
                    animPlayer.SetTrigger("Fidget02");

                if (fidgetIndexPlayer == 2)
                    animPlayer.SetTrigger("Fidget03");

                if (fidgetIndexPlayer == 3)
                    animPlayer.SetTrigger("Fidget03");
            }

            idleAnimIndexPlayer = 0;
            SetTimesToRepeat();
            canFidget = false;
        }
    }

    // Checks when the canFidget should be false or true
    private void CanPlayerFidgetCheck()
    {
        if (animPlayer.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            // Plays the greet anim if the canFidget bool is false and the npc isn't fidgeting - bool is set to false when dialogue ends
            if (!hasPlayedGreetAnimPlayer && inCharacterDialogue && characterDialogueScript.hasStartedDialoguePlayer)
            {
                if (characterDialogueScript.isInteractingWithArtifact)
                    animPlayer.SetTrigger("Fidget03");
                else
                    animPlayer.SetTrigger("Greet");

                idleAnimIndexPlayer = 0;
                SetTimesToRepeat();
                canFidget = false;
                hasPlayedGreetAnimPlayer = true;
            }
            else
                canFidget = true;
        }
        else
        {
            hasPlayedGreetAnimPlayer = true;
            canFidget = false;
        }
    }

    // Sets a random animIndexNPC - each index has its own animation to be played
    private void SetRadnomAnimIndexPlayer()
    {
        int attempts = 3;
        int newFidgetIndexPlayer = UnityEngine.Random.Range(0, 4);

        while (newFidgetIndexPlayer == fidgetIndexPlayer && attempts > 0)
        {
            newFidgetIndexPlayer = UnityEngine.Random.Range(0, 4);
            attempts--;
        }

        fidgetIndexPlayer = newFidgetIndexPlayer;
    }


    // Sets a random size for the timeToRepeatPlayer float - how many times the player's idle will play before triggering a fidget animation
    private void SetTimesToRepeat()
    {
        if (!inCharacterDialogue)
        {
            int attempts = 3;
            int newTimesToRepeatPlayer = UnityEngine.Random.Range(3, 6);

            while (newTimesToRepeatPlayer == timesToRepeatPlayer && attempts > 0)
            {
                newTimesToRepeatPlayer = UnityEngine.Random.Range(3, 6);
                attempts--;
            }

            timesToRepeatPlayer = newTimesToRepeatPlayer;
        }

        else if (inCharacterDialogue)
        {
            int attempts = 3;
            int newTimesToRepeatPlayer = UnityEngine.Random.Range(2, 4);

            while (newTimesToRepeatPlayer == timesToRepeatPlayer && attempts > 0)
            {
                newTimesToRepeatPlayer = UnityEngine.Random.Range(2, 4);
                attempts--;
            }

            timesToRepeatPlayer = newTimesToRepeatPlayer;
        }
    }

}
