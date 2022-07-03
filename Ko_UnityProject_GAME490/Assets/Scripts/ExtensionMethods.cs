using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public static class ExtensionMethods
{
    // Sets the alpha of the text
    public static void SetTextAlpha(this TextMeshProUGUI text, float alpha)
    {
        Color color = text.color;
        text.color = new Color(color.r, color.g, color.b, alpha);
    }

    // Sets the alpha of the image
    public static void SetImageAlpha(this Image image, float alpha)
    {
        Color color = image.color;
        image.color = new Color(color.r, color.g, color.b, alpha);
    }

    // Returns the color of the image
    // Note: the default alpha to return will always be 1 if the parameter is not set
    public static Color ReturnImageColor(this Image image, float alpha = 1)
    {
        Color color = image.color;
        return new Color(color.r, color.g, color.b, alpha);
    }

    // Returns the color of the text 
    // Note: the default alpha to return will always be 1 if the parameter is not set
    public static Color ReturnTextColor(this TextMeshProUGUI text, float alpha = 1)
    {
        Color color = text.color;
        return new Color(color.r, color.g, color.b, alpha);
    }

    // Returns an array of strings/sentences
    public static string[] ReturnSentences(this TextAsset textFile)
    {
        if (textFile == null) return null;
        return textFile.text.Split("\n"[0]);
    }

    // Returns the hexadecimal string for the color
    public static string ReturnHexColor(this Color32 color)
    {
        return ColorUtility.ToHtmlStringRGB(color);
    }

    // Returns the name of the current unity scene
    public static string SceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    // Return the length of a clip within an animator
    public static float ReturnClipLength(this Animator animator, string nameOfClip)
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;

        foreach (AnimationClip clip in clips)
        {
            if (clip.name == nameOfClip)
                return clip.length;
        }

        return 0;
    }

}
