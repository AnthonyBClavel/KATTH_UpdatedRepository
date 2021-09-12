using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueArrow : MonoBehaviour
{
    public float animSpeed = 30f;
    public float animDistance = 15f;
    private bool hasReachedDestination = false;

    private GameObject dialogueArrow;
    private GameObject dialogueArrowHolder;

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
        originalPosition = dialogueArrowHolder.transform.localPosition;
        destination = new Vector3(originalPosition.x - animDistance, originalPosition.y, originalPosition.z);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        UpdateDestinationCheck();
        DialogueArrowAnimCheck();
    }

    // The animation for the dialogue arrow
    private void DialogueArrowAnimCheck()
    {
        if (dialogueArrow.activeSelf)
        {
            if (!hasReachedDestination)
            {
                dialogueArrowHolder.transform.localPosition = Vector3.MoveTowards(dialogueArrowHolder.transform.localPosition, destination, animSpeed * Time.deltaTime);

                if (dialogueArrowHolder.transform.localPosition == destination)
                    hasReachedDestination = true;
            }
            if (hasReachedDestination)
            {
                dialogueArrowHolder.transform.localPosition = Vector3.MoveTowards(dialogueArrowHolder.transform.localPosition, originalPosition, animSpeed * Time.deltaTime);

                if (dialogueArrowHolder.transform.localPosition == originalPosition)
                    hasReachedDestination = false;
            }
        }

        if (!dialogueArrow.activeSelf && dialogueArrowHolder.transform.localPosition != originalPosition)
            dialogueArrowHolder.transform.localPosition = originalPosition;
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        dialogueArrow = characterDialogueScript.dialogueArrow;

        // Sets the game objects by looking at names of children
        for (int i = 0; i < dialogueArrow.transform.childCount; i++)
        {
            GameObject child = dialogueArrow.transform.GetChild(i).gameObject;

            if (child.name == "DialogueArrowHolder")
                dialogueArrowHolder = child;
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
}
