using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New NPC", menuName = "ScriptableObjects/NPC")]
public class NonPlayerCharacter_SO : ScriptableObject
{
    public string nPCName;
    public TextAsset dialogueOptions;
    public TextAsset[] nPCDialogue;
    public Color32 nPCTextColor;
    public Color32 nPCBubbleColor;
}

// The original text colors for each npc/character - For Reference
/*
    BabyMammoth = Color32(196, 146, 102, 255)

    FirstVillageExplorer = Color32(115, 106, 142, 255)

    Fisherman = Color32(194, 130, 104, 255)

    FriendlyGhost = Color32(96, 182, 124, 255)

    SecondVillageExplorer = Color32(155, 162, 125, 255)

    VillageElder = Color32(58, 78, 112, 255)

    AnyOtherNPC = Color.black

    Player/Ko = Color32(128, 160, 198, 255)
*/

