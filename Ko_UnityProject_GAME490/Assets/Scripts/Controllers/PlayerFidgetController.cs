using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFidgetController : MonoBehaviour
{
    private bool hasPlayedInitialFidget = false;
    private bool canFidget = true;
    private bool canAddToIdleIndex = false;

    private float idleClipLength;
    private float pushingClipLength;
    private float idleSpeedMultiplier;
    private float pushingSpeedMultiplier;

    private float timesToRepeat = 3f;
    private int fidgetIndex = 0;
    private int idleCount = 0;

    private string currentAnimPlaying;
    private GameObject dialogueOptionsBubble;
    private Animator playerAnimator;
    private AnimatorClipInfo[] playerAnimClipInfo;

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
        SetTimesToRepeat();   
    }

    // Update is called once per frame
    void Update()
    {
        CurrentAnimationCheck();
    }

    // Returns the current animation playing
    public string CurrentAnimPlaying
    {
        get
        {
            return currentAnimPlaying;
        }
    }

    // Sets the idle index to zero
    public void SetIdleIndexToZero()
    {
        idleCount = 0;
    }

    // Sets the hasPlayedInitialFidget to false
    public void ResetInitialFidgetBool()
    {
        hasPlayedInitialFidget = false;
    }

    // Plays a fidget animation when called - cannot play a fidget animation while another is playing
    public void FidgetAnimationCheck()
    {
        CanFidgetCheck();

        if (canFidget)
        {
            if (!characterDialogueScript.hasStartedPlayerDialogue)
            {
                if (dialogueOptionsBubble.activeSelf)
                    playerAnimator.SetTrigger("Fidget03");                   
                else
                    PlayRandomFidgetAnimation(0, 4);              
            }
     
            if (characterDialogueScript.hasStartedPlayerDialogue)
                PlayRandomFidgetAnimation(0, 3); // No jumping jacks during dialogue

            SetTimesToRepeat();
            idleCount = 0;
            canFidget = false;
        }
    }

    // Checks if the player can fidget
    private void CanFidgetCheck()
    {
        if (currentAnimPlaying == "Idle")
        {
            // Check to play specific animation if the dialogue has just started
            if (!hasPlayedInitialFidget && characterDialogueScript.hasStartedPlayerDialogue)
            {
                if (characterDialogueScript.isInteractingWithArtifact) // Checks to play scratching head animation
                    playerAnimator.SetTrigger("Fidget03");
                else
                    playerAnimator.SetTrigger("Greet"); // Checks to play greeting animation

                SetTimesToRepeat();
                idleCount = 0;
                canFidget = false;
                hasPlayedInitialFidget = true;
            }
            // Set canFidget to true if the idle animation is currenlty playing
            else
                canFidget = true;
        }
        // If any other animation is being played, canFidget is false
        else
        {
            hasPlayedInitialFidget = true;
            canFidget = false;
        }
    }

    // Checks for the name of the current animation being played
    private void CurrentAnimationCheck()
    {
        if (playerAnimClipInfo != playerAnimator.GetCurrentAnimatorClipInfo(0))
            playerAnimClipInfo = playerAnimator.GetCurrentAnimatorClipInfo(0);

        if (playerAnimClipInfo.Length > 0 && currentAnimPlaying != playerAnimClipInfo[0].clip.name)
            currentAnimPlaying = playerAnimClipInfo[0].clip.name;

        if (currentAnimPlaying == "Idle")
        {
            if (!canAddToIdleIndex)
            {
                StartCoroutine(AddToIdleIndex());
                canAddToIdleIndex = true;
            }
        }
        else
            idleCount = 0;
    }

    // Sets a random value for the fidgetIdex - each number corresponds to an animation
    private void PlayRandomFidgetAnimation(int startRange, int endRange)
    {
        int attempts = 3;
        int newFidgetIndex = UnityEngine.Random.Range(startRange, endRange);

        while (newFidgetIndex == fidgetIndex && attempts > startRange)
        {
            newFidgetIndex = UnityEngine.Random.Range(startRange, endRange);
            attempts--;
        }

        fidgetIndex = newFidgetIndex;

        if (fidgetIndex == 0)
            playerAnimator.SetTrigger("Fidget01"); // Strecth

        else if (fidgetIndex == 1)
            playerAnimator.SetTrigger("Fidget02"); // Look At Torch

        else if (fidgetIndex == 2)
            playerAnimator.SetTrigger("Fidget03"); // Scratch Head

        else if (fidgetIndex == 3)
            playerAnimator.SetTrigger("Fidget04"); // Jumping Jacks
    }


    // Sets a random value for the timesToRepeat float - the amount of times the idle animation has to loop before triggering a fidget animation
    private void SetTimesToRepeat()
    {
        // Fidgets occur less often while not in dialogue...
        if (!characterDialogueScript.hasStartedPlayerDialogue)
        {
            int attempts = 3;
            int newTimesToRepeat = UnityEngine.Random.Range(3, 6);

            while (newTimesToRepeat == timesToRepeat && attempts > 0)
            {
                newTimesToRepeat = UnityEngine.Random.Range(3, 6);
                attempts--;
            }

            timesToRepeat = newTimesToRepeat;
        }

        // Fidgets occur more often while in dialogue...
        else if (characterDialogueScript.hasStartedPlayerDialogue)
        {
            int attempts = 3;
            int newTimesToRepeat = UnityEngine.Random.Range(2, 4);

            while (newTimesToRepeat == timesToRepeat && attempts > 0)
            {
                newTimesToRepeat = UnityEngine.Random.Range(2, 4);
                attempts--;
            }

            timesToRepeat = newTimesToRepeat;
        }
    }

    // Adds one to the idleCount everytime the idle animation loops 
    private IEnumerator AddToIdleIndex()
    {
        yield return new WaitForSeconds(idleClipLength / idleSpeedMultiplier);

        if (currentAnimPlaying == "Idle" && !characterDialogueScript.hasStartedPlayerDialogue && !characterDialogueScript.hasTransitionedToArtifactView && pauseMenuScript.CanPause)
        {
            if (idleCount < timesToRepeat)
                idleCount++;

            if (idleCount >= timesToRepeat)
                FidgetAnimationCheck();
        }

        canAddToIdleIndex = false;
    }

    // Sets the lengths of each clip/animation (idle and pushing)
    private void SetAnimationClipVariables()
    {
        AnimationClip[] clips = playerAnimator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            switch (clip.name)
            {
                case "Idle":
                    idleClipLength = clip.length;
                    break;
                case "Pushing":
                    pushingClipLength = clip.length;
                    break;
            }

            idleSpeedMultiplier = playerAnimator.GetFloat("IdleSpeedMultiplier");
            pushingSpeedMultiplier = playerAnimator.GetFloat("PushingSpeedMultiplier");
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

        playerAnimator = GetComponentInChildren<Animator>();
        SetAnimationClipVariables();
    }

}
