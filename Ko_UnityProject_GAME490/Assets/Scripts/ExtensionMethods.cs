using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    // Returns an array of strings/sentences
    public static string[] ReturnSentences(this TextAsset textFile)
    {
        return textFile.text.Split("\n"[0]);
    }

}
