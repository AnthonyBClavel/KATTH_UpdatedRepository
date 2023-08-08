using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System;

public static class ExtensionMethods
{
    // Returns the color of the text 
    // Note: the default alpha to return will always be 1 if the parameter is not set
    public static Color ReturnTextColor(this TextMeshProUGUI text, float alpha = 1)
    {
        Color color = text.color;
        return new Color(color.r, color.g, color.b, alpha);
    }

    // Sets the alpha of the text
    public static void SetTextAlpha(this TextMeshProUGUI text, float alpha)
    {
        Color color = text.color;
        text.color = new Color(color.r, color.g, color.b, alpha);
    }

    // Returns the color of the image
    // Note: the default alpha to return will always be 1 if the parameter is not set
    public static Color ReturnImageColor(this Image image, float alpha = 1)
    {
        Color color = image.color;
        return new Color(color.r, color.g, color.b, alpha);
    }

    // Sets the alpha of the image
    public static void SetImageAlpha(this Image image, float alpha)
    {
        Color color = image.color;
        image.color = new Color(color.r, color.g, color.b, alpha);
    }

    // Returns the hexadecimal string for the color
    public static string ReturnHexColor(this Color32 color)
    {
        return ColorUtility.ToHtmlStringRGB(color);
    }

    // Returns as an array of strings/sentences for the text file
    public static string[] ReturnSentences(this TextAsset textFile)
    {
        if (textFile == null) return null;
        return textFile.text.Split("\n"[0]);
    }

    // Return the length of a clip within an animator
    public static float ReturnClipLength(this Animator animator, string nameOfClip)
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;

        foreach (AnimationClip clip in clips)
        {
            if (clip.name != nameOfClip) continue;
            return clip.length;
        }
        return 0;
    }

    // Converts the string to a number and returns it as an integer
    public static int ConvertStringToNumber(this string theString)
    {
        StringBuilder finalString = new StringBuilder(string.Empty);

        foreach (char c in theString)
        {
            if (!Char.IsDigit(c)) continue;
            finalString.Append(c);
        }

        return Convert.ToInt32(finalString.ToString().TrimStart('0'));
    }

    /** Debugging extensions START Here **/
    // Returns a string that lists the names of each game object in ascending order - For Debugging Purposes ONLY
    public static string ConvertGameObjectListToString(this List<GameObject> list)
    {
        StringBuilder listOfGameObjects = new StringBuilder(string.Empty);

        foreach (GameObject checkpoint in list)
            listOfGameObjects.Append($"{checkpoint.transform.parent.parent.name}, ");

        string theString = listOfGameObjects.ToString();

        return theString.Substring(0, theString.Length - 2);
    }

    // Returns a string that lists the names of each game object in ascending order - For Debugging Purposes ONLY
    public static string ConvertGameObjectArrayToString(this GameObject[] array)
    {
        StringBuilder arrayOfGameObjects = new StringBuilder(string.Empty);

        foreach (GameObject checkpoint in array)
            arrayOfGameObjects.Append($"{checkpoint.transform.parent.parent.name}, ");

        string theString = arrayOfGameObjects.ToString();

        return theString.Substring(0, theString.Length - 2);
    }
    /** Debugging extensions END Here **/

}
