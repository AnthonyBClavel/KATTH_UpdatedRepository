using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New NPC", menuName = "ScriptableObjects/NPC")]
public class NonPlayerCharacter_SO : ScriptableObject
{
    public string nPCName;
    public Color32 nPCTextColor;
    public TextAsset dialogueOptions;
    public TextAsset[] nPCDialogue;
    public TextAsset[] playerDialogue;

}
