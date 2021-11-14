using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCFidgetController : MonoBehaviour
{
    private bool hasPlayedInitialFidget = false;
    private bool canFidget = true;
    private bool canAddToIdleIdex = false;
    private bool canFidgetDuringIdle = true;

    private float idleClipLength;
    private float idleSpeedMultiplier;

    private float timesToRepeat = 3f;
    private int fidgetIdex = 0;
    private int idleCount = 0;

    private string currentAnimPlaying;
    private GameObject dialogueOptionsBubble;
    private Animator nPCAnimator;
    private AnimatorClipInfo[] nPCAnimClipInfo;

    private CharacterDialogue characterDialogueScript;
    private PauseMenu pauseMenuScript;
    private NonPlayerCharacter nPCScript;

    void Awake()
    {
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        nPCScript = GetComponent<NonPlayerCharacter>();

        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetTimesToRepeat();
        CanFidgetDuringIdleCheck();
    }

    // Update is called once per frame
    void Update()
    {
        CurrentAnimationCheck();
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
            if (!characterDialogueScript.hasStartedNPCDialogue)
            {
                if (dialogueOptionsBubble.activeSelf)
                    nPCAnimator.SetTrigger("Fidget01");
                else
                    PlayRandomFidgetAnimation();
            }

            if (characterDialogueScript.hasStartedNPCDialogue)
                PlayRandomFidgetAnimation();

            SetTimesToRepeat();
            idleCount = 0;
            canFidget = false;
        }
    }

    // Checks if the npc can fidget
    private void CanFidgetCheck()
    {
        if (currentAnimPlaying == "Idle")
        {
            // Plays the greet anim if the bool is false and the npc isn't fidgeting - bool is set to false when dialogue ends
            if (!hasPlayedInitialFidget && characterDialogueScript.hasStartedNPCDialogue)
            {
                nPCAnimator.SetTrigger("Greet");
                SetTimesToRepeat();
                idleCount = 0;
                canFidget = false;
                hasPlayedInitialFidget = true;
            }
            else
                canFidget = true;
        }
        else
        {
            hasPlayedInitialFidget = true;
            canFidget = false;
        }
    }

    // Checks for the name of the current animation being played
    private void CurrentAnimationCheck()
    {
        if (nPCAnimClipInfo != nPCAnimator.GetCurrentAnimatorClipInfo(0))
            nPCAnimClipInfo = nPCAnimator.GetCurrentAnimatorClipInfo(0);

        if (nPCAnimClipInfo.Length > 0 && currentAnimPlaying != nPCAnimClipInfo[0].clip.name)
            currentAnimPlaying = nPCAnimClipInfo[0].clip.name;

        if (canFidgetDuringIdle)
        {
            if (currentAnimPlaying == "Idle")
            {
                if (!canAddToIdleIdex)
                {
                    StartCoroutine(AddToIdleIndex());
                    canAddToIdleIdex = true;
                }
            }
            else
                idleCount = 0;
        }
    }

    // Sets a random value for the fidgetIdex - each number corresponds to an animation
    private void PlayRandomFidgetAnimation()
    {
        int attempts = 3;
        int newFidgetIndex = UnityEngine.Random.Range(0, 4);

        while (newFidgetIndex == fidgetIdex && attempts > 0)
        {
            newFidgetIndex = UnityEngine.Random.Range(0, 4);
            attempts--;
        }

        fidgetIdex = newFidgetIndex;

        if (fidgetIdex == 0)
            nPCAnimator.SetTrigger("Fidget01");

        else if (fidgetIdex == 1)
            nPCAnimator.SetTrigger("Fidget02");

        else if (fidgetIdex == 2)
            nPCAnimator.SetTrigger("Fidget03");

        else if (fidgetIdex == 3)
            nPCAnimator.SetTrigger("Fidget04");
    }

    // Sets a random value for the timesToRepeat float - the amount of times the idle animation has to loop before triggering a fidget animation
    private void SetTimesToRepeat()
    {
        // Fidgets occur less often while not in dialogue...
        if (!characterDialogueScript.hasStartedNPCDialogue)
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
        else if (characterDialogueScript.hasStartedNPCDialogue)
        {
            int attempts = 3;
            int newTimesToRepeatNPC = UnityEngine.Random.Range(2, 4);

            while (newTimesToRepeatNPC == timesToRepeat && attempts > 0)
            {
                newTimesToRepeatNPC = UnityEngine.Random.Range(2, 4);
                attempts--;
            }

            timesToRepeat = newTimesToRepeatNPC;
        }
    }

    // Sets the canLookForCurrentAnimation bool - false if the NPC doesn't have enough fidget animations
    private void CanFidgetDuringIdleCheck()
    {
        string characterName = nPCScript.CharacterName;

        if (characterName == "BabyMammoth" || characterName == "Fisherman")
            canFidgetDuringIdle = false;
    }

    // Adds one to the idleCount everytime the idle animation loops 
    private IEnumerator AddToIdleIndex()
    {
        yield return new WaitForSeconds(idleClipLength / idleSpeedMultiplier);

        if (currentAnimPlaying == "Idle" && !characterDialogueScript.hasStartedNPCDialogue && !characterDialogueScript.hasTransitionedToArtifactView && pauseMenuScript.CanPause)
        {
            if (idleCount < timesToRepeat)
                idleCount++;

            if (idleCount >= timesToRepeat)
                FidgetAnimationCheck();
        }

        canAddToIdleIdex = false;
    }

    // Sets the lengths of each clip/animation (idle and pushing)
    private void SetAnimationClipVariables()
    {
        AnimationClip[] clips = nPCAnimator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            switch (clip.name)
            {
                case "Idle":
                    idleClipLength = clip.length;
                    break;
            }

            idleSpeedMultiplier = nPCAnimator.GetFloat("IdleSpeedMultiplier");
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

        nPCAnimator = GetComponentInChildren<Animator>();
        SetAnimationClipVariables();
    }

}
