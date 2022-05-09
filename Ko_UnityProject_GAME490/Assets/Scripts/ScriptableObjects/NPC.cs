using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New NPC", menuName = "ScriptableObjects/NPC")]
public class NPC : ScriptableObject
{
    public string nPCName;
    public TextAsset dialogueOptions;
    public TextAsset[] playerDialogue;
    public TextAsset[] nPCDialogue;

}
