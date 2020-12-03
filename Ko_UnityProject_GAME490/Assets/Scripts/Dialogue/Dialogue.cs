using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    public TextAsset textFile; // Text file containing dialogue

    /***
     * Reads the text file and splits the sentences by each new line
     ***/
    public string[] readTextFile()
    {
        return textFile.text.Split("\n"[0]);
    }

    public string[] readTextFile(TextAsset textFile)
    {
        return textFile.text.Split("\n"[0]);
    }
}
