using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveSlot
{
    private string sceneName;
    private float[] position;
    private string puzzleName;
    private int cameraIndex;

    public SaveSlot(string sceneName, float[] position, string puzzleName, int cameraIndex)
    {
        this.sceneName = sceneName;
        this.position = position;
        this.puzzleName = puzzleName;
        this.cameraIndex = cameraIndex;
    }

    public string getSceneName()
    {
        return sceneName;
    }

    public float[] getPosition()
    {
        return position;
    }

    public string getPuzzleName()
    {
        return puzzleName;
    }

    public int getCameraIndex()
    {
        return cameraIndex;
    }


}
