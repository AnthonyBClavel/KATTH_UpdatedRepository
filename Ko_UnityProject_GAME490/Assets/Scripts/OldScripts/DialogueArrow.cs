using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueArrow : MonoBehaviour
{
    [Range(0.1f, 2.0f)]
    public float animLength = 1f;
    [Range(1f, 90f)]
    public float animDistance = 15f;

    private GameObject dialogueArrow;
    private RectTransform dialogueArrowRectTransfom;
    private Vector2 originalPosition;
    private Vector2 destination;
    private IEnumerator dialogueArrowCoroutine;

    // Awake is called before Start()
    void Awake()
    {
        
    }

    // Stops the dialogue arrow corouitne and sets resets its position
    public void StopDialogueArrowAnim()
    {
        if (dialogueArrowCoroutine != null)
            StopCoroutine(dialogueArrowCoroutine);

        dialogueArrowRectTransfom.anchoredPosition = originalPosition;
    }

    // Starts the dialogue arrow corouitne
    public void PlayDialogueArrowAnim()
    {
        if (dialogueArrowCoroutine != null)
            StopCoroutine(dialogueArrowCoroutine);

        dialogueArrowCoroutine = LerpDialogueArrow(animLength, originalPosition, destination);
        StartCoroutine(dialogueArrowCoroutine);
    }

    // Starts the dialogue arrow corouitne (for this script only)
    private void PlayDialogueArrowAnim02(float duration, Vector2 startPosition, Vector2 endPosition)
    {
        if (dialogueArrowCoroutine != null)
            StopCoroutine(dialogueArrowCoroutine);

        dialogueArrowCoroutine = LerpDialogueArrow(duration, startPosition, endPosition);
        StartCoroutine(dialogueArrowCoroutine);
    }

    // Lerps the color of the image to another, over a specific duration - for the zone intro ONLY
    private IEnumerator LerpDialogueArrow(float duration, Vector2 startPosition, Vector2 endPosition)
    {
        float time = 0;
        while (time < (duration / 2))
        {
            dialogueArrowRectTransfom.anchoredPosition = Vector2.Lerp(startPosition, endPosition, time / (duration / 2));
            time += Time.deltaTime;
            yield return null;
        }

        dialogueArrowRectTransfom.anchoredPosition = endPosition;

        if (endPosition == destination)
            PlayDialogueArrowAnim02(animLength, destination, originalPosition);
        else
            PlayDialogueArrowAnim02(animLength, originalPosition, destination);
    }

    // Sets private variables, objects, and components
    public void SetDialogueArrow(GameObject dialogueArrowHolder)
    {
        dialogueArrow = dialogueArrowHolder;
        dialogueArrowRectTransfom = dialogueArrow.GetComponent<RectTransform>();
        originalPosition = dialogueArrowRectTransfom.anchoredPosition;
        destination = new Vector2(originalPosition.x - animDistance, originalPosition.y);

        // Sets the game objects by looking at names of children
        /*for (int i = 0; i < dialogueArrowHolder.transform.childCount; i++)
        {
            GameObject child = dialogueArrowHolder.transform.GetChild(i).gameObject;

            if (child.name == "DialogueArrow")
                dialogueArrow = child;
        }*/
    }

    // Updates the destination if the animDistance is changed - For Debugging Purposes Only
    public void DebuggingCheck(GameManager gameManager)
    {
        if (gameManager.isDebugging)
        {
            if (destination != new Vector2(originalPosition.x - animDistance, originalPosition.y))
                destination = new Vector2(originalPosition.x - animDistance, originalPosition.y);
        }
    }

}
