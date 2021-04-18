using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FidgetAnimControllerNPC : MonoBehaviour
{
    public int idleAnimIndex;
    public int fidgetIndex;
    public float timesToRepeat = 3;

    private Animator characterAnim;

    public bool inCharacterDialogue = false;
    public bool hasPlayedGreetAnimNPC = false;
    private bool canFidget = true;

    private CharacterDialogue characterDialogueScript;

    void Awake()
    {
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
    }

    // Start is called before the first frame update
    void Start()
    {
        characterAnim = GetComponent<Animator>();
        idleAnimIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            //PlayGreetAnim();
            //PlayFidgetAnimCheck();
        }
    }

    // For an animation event - plays the fidget animation after the idle is repeated a certain amount of times
    public void AddToIdleIndex()
    {
        if (idleAnimIndex < timesToRepeat)
            idleAnimIndex++;

        if (idleAnimIndex >= timesToRepeat)
            PlayFidgetAnimCheck();
    }

    // Plays a fidget animation when called - cannot play a fidget animation while another is playing
    public void PlayFidgetAnimCheck()
    {
        CanFidgetCheck();

        if (canFidget)
        {
            if (!inCharacterDialogue)
                characterAnim.SetTrigger("Fidget");

            if (inCharacterDialogue)
            {
                SetRadnomAnimIndex();

                if (fidgetIndex == 1)
                    characterAnim.SetTrigger("Fidget01");

                else if (fidgetIndex == 2)
                    characterAnim.SetTrigger("Fidget02");

                else if (fidgetIndex == 3)
                    characterAnim.SetTrigger("Fidget03");              
            }

            idleAnimIndex = 0;
            SetTimesToRepeat();
            canFidget = false;
        }
    }

    // Checks when the bool should be false or true - for playing the fidget animations during the idle anim ONLY
    private void CanFidgetCheck()
    {
        if (characterAnim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            // Plays the greet anim if the bool is false and the player isn't fidgeting - bool is set to false when dialogue ends
            if (!hasPlayedGreetAnimNPC && inCharacterDialogue && characterDialogueScript.hasStartedDialogueNPC)
            {
                characterAnim.SetTrigger("Greet");
                idleAnimIndex = 0;
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

    // Sets a random animIndex - each index has its own animation to be played
    private void SetRadnomAnimIndex()
    {
        int attempts = 3;
        int newAnimIndex = UnityEngine.Random.Range(1, 4);

        while (newAnimIndex == fidgetIndex && attempts > 0)
        {
            newAnimIndex = UnityEngine.Random.Range(1, 4);
            attempts--;
        }

        fidgetIndex = newAnimIndex;
    }

    // Sets a random size for the timeToRepeat float - this determines how many times the idle will play before triggering a fidget animation
    private void SetTimesToRepeat()
    {
        if (!inCharacterDialogue)
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

        else if (inCharacterDialogue)
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


}
