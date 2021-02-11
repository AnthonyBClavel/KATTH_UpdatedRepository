using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public DialogueManager dialogueManager;
    private string[] dialogue;

    /***
     * Interact function
     * Call when you want to interact with the NPC
     ***/

    public void Interact()
    {
        if (transform.childCount <= 0)
        {
            Debug.Log("No dialogue");
            return;
        }

        Transform child = transform.GetChild(0);
        dialogue = child.GetComponent<Dialogue>().readTextFile();
        if (dialogue == null) return;

        dialogueManager.setDialogue(dialogue);
        dialogueManager.startDialogue();

        if (transform.childCount > 1)
            GameObject.Destroy(child.gameObject);
    }


}
