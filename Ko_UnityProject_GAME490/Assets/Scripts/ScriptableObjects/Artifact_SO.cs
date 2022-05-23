using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Artifact", menuName = "ScriptableObjects/Artifact")]
public class Artifact_SO : ScriptableObject
{
    public string artifactName;
    public TextAsset dialogueOptions;
    public TextAsset[] artifactDialogue;

}
