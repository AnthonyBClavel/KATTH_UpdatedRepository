﻿using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(AudioClip_SO), true)]
public class AudioEventEditor : Editor
{
    private AudioSource preview;

    public void OnEnable()
    {
        preview = EditorUtility.CreateGameObjectWithHideFlags("Audio preview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();
    }

    public void OnDisable()
    {
        DestroyImmediate(preview.gameObject);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);

        if (GUILayout.Button("Preview"))
        {
            ((AudioClip_SO)target).PlayInEditor(preview);
        }
        if (GUILayout.Button("Stop Preview"))
        {
            ((AudioClip_SO)target).StopInEditor(preview);
        }

        EditorGUI.EndDisabledGroup();
    }
}