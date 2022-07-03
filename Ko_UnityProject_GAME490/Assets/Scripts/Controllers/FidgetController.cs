using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FidgetController : MonoBehaviour
{
    private int idleCount = 0;
    private int fidgetIndex = 0;
    private float idleRepetitions = 3f;

    private string characterName;
    private bool hasPlayedInitialFidget = false;

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
    }

    // Sets the idle count to zero
    public void SetIdleCountToZero() => idleCount = 0;

    // Checks if an animation is currently playing - returns true if so, false otherwise
    public bool IsPlayingAnimation(string animName)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(animName)) return true;

        return false;
    }

    // Checks to play a fidget animation - called at the end of the character's idle animation via animation event
    // Note: not all characters have this animation event due to their lack of animations
    public void FidgetCheck()
    {
        // The character will not figdet on its own during dialogue, unless the dialogue options are active
        if (characterDialogueScript.InDialogue && !characterDialogueScript.InDialogueOptions) return;

        if (idleCount < idleRepetitions - 1) idleCount++;
        else Fidget();
    }

    // Plays a fidget animation
    // Note: the character will only fidget while in the idle state
    public void Fidget()
    {
        if (!IsPlayingAnimation("Idle")) return;

        // Checks to play the intial fidget animation
        if (!hasPlayedInitialFidget && characterDialogueScript.InDialogue)
            PlayInitialFidgetAnimation();

        // Plays a specific fidget animation if the dialogue options are active
        else if (characterDialogueScript.InDialogueOptions)
            PlayDialogueOptionsFidgetAnimation();

        // Plays a random fidget animation otherwise
        else PlayRandomFidgetAnimation();

        hasPlayedInitialFidget = true;
    }

    // Determines and plays the intial fidget animation
    private void PlayInitialFidgetAnimation()
    {
        string newState = (characterName == "Player" && characterDialogueScript.IsInteractingWithArtifact) ? "Fidget03" : "Greet";
        ChangeAnimationState(newState);
    }

    // Plays a specific fidget animation for when the dialogue options are active
    private void PlayDialogueOptionsFidgetAnimation()
    {
        string newState = (characterName == "Player") ? "Fidget03" : "Fidget01";
        ChangeAnimationState(newState);
    }

    // Plays a random fidget animation
    private void PlayRandomFidgetAnimation()
    {
        // Note: the player does not play its "Fidget04" animation during dialogue (jumping-jacks anim)
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
            default:
                break;
        }          
    }

    // Sets a random value for idleRepetitions
    // Note: idleRepetitions = the amount of times the idle animation will loop before fidgeting
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

    // Plays a new animation state and resets the fidget variables
    private void ChangeAnimationState(string newState)
    {      
        animator.Play(newState);
        SetNewIdleRepetitions();
        SetIdleCountToZero();
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        SetNewIdleRepetitions();
        animator = GetComponent<Animator>();
        characterName = animator.runtimeAnimatorController.name;
    }

}
