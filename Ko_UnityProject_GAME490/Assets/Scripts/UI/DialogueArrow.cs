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
    private Vector3 originalPosition;
    private Vector3 destination;
    private IEnumerator dialogueArrowCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = dialogueArrow.transform.localPosition;
        destination = new Vector3(originalPosition.x - animDistance, originalPosition.y, originalPosition.z);
    }

    // Stops playing the dialogue arrow animation, and sets resets its position
    public void StopDialogueArrowAnim()
    {
        if (dialogueArrowCoroutine != null)
            StopCoroutine(dialogueArrowCoroutine);

        dialogueArrow.transform.localPosition = originalPosition;
    }

    // Starts the dialogue arrow corouitne (plays the dialogue arrow animation)
    public void PlayDialogueArrowAnim()
    {
        if (dialogueArrowCoroutine != null)
            StopCoroutine(dialogueArrowCoroutine);

        dialogueArrowCoroutine = LerpDialogueArrow(animLength);
        StartCoroutine(dialogueArrowCoroutine);
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
        PlayDialogueArrowAnim();
    }

    // Sets private variables, objects, and components
    public void SetDialogueArrow(GameObject dialogueArrowHolder)
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < dialogueArrowHolder.transform.childCount; i++)
        {
            GameObject child = dialogueArrowHolder.transform.GetChild(i).gameObject;

            if (child.name == "DialogueArrow")
                dialogueArrow = child;
        }
    }

    // Updates the destination if the animDistance is changed - For Debugging Purposes Only
    public void DebuggingCheck(GameManager gameManager)
    {
        if (gameManager.isDebugging)
        {
            if (destination != new Vector3(originalPosition.x - animDistance, originalPosition.y, originalPosition.z))
                destination = new Vector3(originalPosition.x - animDistance, originalPosition.y, originalPosition.z);
        }
    }

}
