using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Artifact", menuName = "ScriptableObjects/Artifact")]
public class ARTIFACT : ScriptableObject
{
    public string artifactName;
    public TextAsset dialogueOptions;
    public TextAsset[] artifactDialogue;

}
