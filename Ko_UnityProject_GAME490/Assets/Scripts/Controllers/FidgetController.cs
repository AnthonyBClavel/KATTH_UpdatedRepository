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

    private Animator animator;
    private CharacterDialogue characterDialogueScript;

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
    // Note: not all characters have this animation event due to their lack of animations
    public void FidgetCheck()
    {
        if (!characterDialogueScript.InDialogue || characterDialogueScript.InDialogueOptions) // && pauseMenuScript.CanPause
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
        // The character will only fidget while in the idle state
        if (IsPlayingAnimation("Idle"))
        {
            // Checks to play the intial fidget animation
            if (!hasPlayedInitialFidget && characterDialogueScript.InDialogue)
                PlayInitialFidgetAnimation();

            // Checks to play a specific fidget animation when the dialogue options are active
            else if (characterDialogueScript.InDialogueOptions)
                PlayDialogueOptionsFidgetAnimation();

            // Plays a random fidget animation otherwise
            else
                PlayRandomFidgetAnimation();

            hasPlayedInitialFidget = true;
        }
    }

    // Determines and plays the intial fidget animation
    private void PlayInitialFidgetAnimation()
    {
        if (characterName == "Player" && characterDialogueScript.IsInteractingWithArtifact)
            ChangeAnimationState("Fidget03");
        else
            ChangeAnimationState("Greet");
    }

    // Plays a specific fidget animation for when the dialogue options are active
    private void PlayDialogueOptionsFidgetAnimation()
    {
        if (characterName == "Player")
            ChangeAnimationState("Fidget03");
        else
            ChangeAnimationState("Fidget01");
    }

    // Plays a random fidget animation
    private void PlayRandomFidgetAnimation()
    {
        // Note: the player's jumping-jacks animation does not play during dialogue ("Fidget04")
        int fidgetRange = (characterName == "Player" && characterDialogueScript.InDialogue) ? 3 : 4;
        int newFidgetIndex = Random.Range(0, fidgetRange);
        int attempts = 3;

        // Checks to play a fidget that's different from the one previously played
        while (newFidgetIndex == fidgetIndex && attempts > 0)
        {
            newFidgetIndex = Random.Range(0, fidgetRange);
            attempts--;
        }
        fidgetIndex = newFidgetIndex;

        switch (fidgetIndex)
        {
            case 0:
                ChangeAnimationState("Fidget01");
                break;
            case 1:
                ChangeAnimationState("Fidget02");
                break;
            case 2:
                ChangeAnimationState("Fidget03");
                break;
            case 3:                 
                ChangeAnimationState("Fidget04");
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

    // Sets the scripts to use
    private void SetScripts()
    {
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        animator = GetComponent<Animator>();
        characterName = animator.runtimeAnimatorController.name;
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

}
