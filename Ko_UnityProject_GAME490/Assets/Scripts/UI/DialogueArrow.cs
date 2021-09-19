using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueArrow : MonoBehaviour
{
    [Range(0.1f, 2.0f)]
    public float animLength = 1f;
    public float animDistance = 15f;

    private bool canPlayAnim = true;
    private bool hasStoppedCoroutine = true;

    private GameObject dialogueArrowHolder;
    private GameObject dialogueArrow;

    private Vector3 originalPosition;
    private Vector3 destination;

    private CharacterDialogue characterDialogueScript;
    private GameManager gameManagerScript;

    void Awake()
    {
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        gameManagerScript = FindObjectOfType<GameManager>();

        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = dialogueArrow.transform.localPosition;
        destination = new Vector3(originalPosition.x - animDistance, originalPosition.y, originalPosition.z);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        UpdateDestinationCheck();
        PlayDialogueArrowAnim();
    }

    // Plays the animation for the dilaogue arrow
    private void PlayDialogueArrowAnim()
    {
        if (dialogueArrowHolder.activeSelf & canPlayAnim)
        {
            StartCoroutine(LerpDialogueArrow(animLength));
            hasStoppedCoroutine = false;
            canPlayAnim = false;
        }
        if (!dialogueArrowHolder.activeSelf && !hasStoppedCoroutine)
        {
            StopCoroutine("LerpDialogueArrow");
            canPlayAnim = true;
            hasStoppedCoroutine = true;
        }    
    }

    // Lerps the color of the image to another, over a specific duration - for the zone intro ONLY
    private IEnumerator LerpDialogueArrow(float duration)
    {
        float time01 = 0;
        while (time01 < (duration / 2))
        {
            dialogueArrow.transform.localPosition = Vector3.Lerp(originalPosition, destination, time01 / (duration / 2));
            time01 += Time.deltaTime;
            yield return null;
        }

        dialogueArrow.transform.localPosition = destination;

        float time02 = 0;
        while (time02 < (duration / 2))
        {
            dialogueArrow.transform.localPosition = Vector3.Lerp(destination, originalPosition, time02 / (duration / 2));
            time02 += Time.deltaTime;
            yield return null;
        }

        dialogueArrow.transform.localPosition = originalPosition;
        canPlayAnim = true;
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        dialogueArrowHolder = characterDialogueScript.ReturnDialogueArrowHolder();

        // Sets the game objects by looking at names of children
        for (int i = 0; i < dialogueArrowHolder.transform.childCount; i++)
        {
            GameObject child = dialogueArrowHolder.transform.GetChild(i).gameObject;

            if (child.name == "DialogueArrow")
                dialogueArrow = child;
        }
    }

    // Updates the destination if the animDistance is changed - For Debugging Purposes Only
    private void UpdateDestinationCheck()
    {
        if (gameManagerScript.isDebugging)
        {
            if (destination != new Vector3(originalPosition.x - animDistance, originalPosition.y, originalPosition.z))
                destination = new Vector3(originalPosition.x - animDistance, originalPosition.y, originalPosition.z);
        }
    }

    /*** OLD FUNCTIONS START HERE - FOR REFERENCE ***/

    // Lerps the position of the dialogue arrow between two positions (set animSpeed to 2f) - OLD VERSION 02
    /*private void DialogueArrowLerpCheck()
    {
        if (pingPong !=  Mathf.PingPong(animSpeed * Time.time, lerpLength))
            pingPong = Mathf.PingPong(animSpeed * Time.time, lerpLength);

        if (dialogueArrowHolder.activeSelf)
            dialogueArrow.transform.localPosition = Vector3.Lerp(originalPosition, destination, pingPong);

        if (!dialogueArrowHolder.activeSelf && dialogueArrow.transform.localPosition != originalPosition)
            dialogueArrow.transform.localPosition = originalPosition;     
    }

    // The animation for the dialogue arrow (set animSpeed to 30f) - OLD VERSION 01
    private void DialogueArrowAnimCheck()
    {
        if (dialogueArrowHolder.activeSelf)
        {
            if (!hasReachedDestination)
            {
                dialogueArrow.transform.localPosition = Vector3.MoveTowards(dialogueArrow.transform.localPosition, destination, animSpeed * Time.deltaTime);

                if (dialogueArrow.transform.localPosition == destination)
                    hasReachedDestination = true;
            }
            if (hasReachedDestination)
            {
                dialogueArrow.transform.localPosition = Vector3.MoveTowards(dialogueArrow.transform.localPosition, originalPosition, animSpeed * Time.deltaTime);

                if (dialogueArrow.transform.localPosition == originalPosition)
                    hasReachedDestination = false;
            }
        }

        if (!dialogueArrowHolder.activeSelf && dialogueArrow.transform.localPosition != originalPosition)
            dialogueArrow.transform.localPosition = originalPosition;
    }*/
}
