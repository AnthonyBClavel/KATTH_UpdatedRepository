using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FidgetController : MonoBehaviour
{
    private bool hasPlayedInitialFidget = false;

    private float idleRepetitions = 3f;
    private int fidgetIndex = 0;
    private int idleCount = 0;
    private string characterName;

    private GameObject dialogueOptionsBubble;
    private Animator animator;

    private CharacterDialogue characterDialogueScript;
    private PauseMenu pauseMenuScript;

    public bool HasPlayedInitialFidget
    {
        get { return hasPlayedInitialFidget; }
        set { hasPlayedInitialFidget = value; }
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
        SetNewIdleRepetitions();
    }

    // Sets the idle count to zero
    public void SetIdleCountToZero() => idleCount = 0;

    // Checks if an animation is currently playing
    public bool IsPlayingAnimation(string animName)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(animName))
            return true;

        return false;
    }

    // Checks to play a fidget animation - called at the end of the character's idle animation via animation event
    // Note: not all characters have a fidget check due to their lack of animations
    public void FidgetCheck()
    {
        if (!characterDialogueScript.hasStartedPlayerDialogue && !characterDialogueScript.hasStartedNPCDialogue && !characterDialogueScript.hasTransitionedToArtifactView && pauseMenuScript.CanPause)
        {
            if (idleCount < idleRepetitions - 1)
                idleCount++;
            else
                Fidget();
        }
    }

    // Plays a fidget animation
    public void Fidget()
    {
        // The player will only fidget while in the idle state
        if (IsPlayingAnimation("Idle") && HasInitialFidget())
        {
            // While the dialogue options is active...
            if (dialogueOptionsBubble.activeSelf)
            {
                // The player will always play the scratching-head animation
                if (characterName == "Player")
                    ChangeAnimationState("Fidget03");

                // The npc will always play the Fidget01 animation
                else
                    ChangeAnimationState("Fidget01");
            }

            // Play a random fidget animation otherwise 
            else
                PlayRandomFidgetAnimation();
        }
    }

    // Checks if the character has played its initial fidget animation - returns true if so, false otherwise
    private bool HasInitialFidget()
    {      
        if (!hasPlayedInitialFidget && characterDialogueScript.InDialogue)
        {
            // Plays the scratching-head animation when initially interacting with an artifact - For The Player ONLY
            if (characterName == "Player" && characterDialogueScript.isInteractingWithArtifact)
                ChangeAnimationState("Fidget03");

            // Plays the greeting animation otherwise
            else
                ChangeAnimationState("Greet");

            hasPlayedInitialFidget = true;
            return false;
        }

        hasPlayedInitialFidget = true;
        return true;
    }

    // Plays a random fidget animation
    private void PlayRandomFidgetAnimation()
    {
        int newFidgetIndex = Random.Range(0, 4);
        int attempts = 3;

        // Checks to play a fidget that's different from the one previously played
        while (newFidgetIndex == fidgetIndex && attempts > 0)
        {
            newFidgetIndex = Random.Range(0, 4);
            attempts--;
        }
        fidgetIndex = newFidgetIndex;

        switch (fidgetIndex)
        {
            case 0:
                ChangeAnimationState("Fidget01"); // Strecth animation (Player)
                break;
            case 1:
                ChangeAnimationState("Fidget02"); // Look-at-torch animation (Player)
                break;
            case 2:
                ChangeAnimationState("Fidget03"); // Scratch-head animation (Player)
                break;
            case 3:             
                // Note: the player's jumping-jacks animation does not play during dialogue
                if (characterName == "Player" && characterDialogueScript.InDialogue)
                    ChangeAnimationState("Fidget03");
                else               
                    ChangeAnimationState("Fidget04"); // Jumping-jacks animation (Player) 
                break;
        }          
    }

    // Sets a random value for idleRepetitions - the amount of times the idle animation will loop before fidgeting
    private void SetNewIdleRepetitions()
    {
        int newIdleRepetitions = Random.Range(3, 6);
        int attempts = 3;

        // Attempts to set a value that's different from idleRepetitions current value
        while (newIdleRepetitions == idleRepetitions && attempts > 0)
        {
            newIdleRepetitions = Random.Range(3, 6);
            attempts--;
        }

        idleRepetitions = newIdleRepetitions;
    }

    // Plays a new animation state and resets fidget variables
    private void ChangeAnimationState(string newState)
    {
        animator.Play(newState);
        SetNewIdleRepetitions();
        idleCount = 0;
    }

    // Returns the animation state's speed multiplier - For Reference
    /*private float FindSpeedMultiplier(string animState)
    {
        float speedMultiplier;

        switch (animState)
        {
            case ("Pushing"):
                speedMultiplier = animator.GetFloat("PushingSpeedMultiplier");
                break;
            case ("Interacting"):
                speedMultiplier = animator.GetFloat("WalkingSpeedMultiplier");
                break;
            case ("Walking"):
                speedMultiplier = animator.GetFloat("InteractingSpeedMultiplier");
                break;
            case ("Idle"):
                speedMultiplier = animator.GetFloat("IdleSpeedMultiplier");
                break;
            default:
                speedMultiplier = 1f;
                break;
        }

        //Debug.Log($"Speed multiplier found: {speedMultiplier}");
        return speedMultiplier;
    }*/

    // Sets the scripts to use
    private void SetScripts()
    {
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets them by looking at the names of children
        for (int i = 0; i < characterDialogueScript.transform.childCount; i++)
        {
            GameObject child = characterDialogueScript.transform.GetChild(i).gameObject;

            if (child.name == "DialogueOptionsBubble")
                dialogueOptionsBubble = child;
        }

        animator = GetComponent<Animator>();
        characterName = animator.runtimeAnimatorController.name;
    }

}
