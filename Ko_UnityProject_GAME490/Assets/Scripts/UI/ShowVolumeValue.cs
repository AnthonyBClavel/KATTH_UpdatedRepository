using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShowVolumeValue : MonoBehaviour
{
    public TextMeshProUGUI sliderValueText;

    void Start()
    {
        sliderValueText = GetComponent<TextMeshProUGUI>();
    }

    // Sets the volume slider's text value to a whole number (ex: 0.1 -> 10.0)
    public void textUpdate (float value)
    {
        sliderValueText.text = Mathf.RoundToInt(value * 100) + "";
    }

}
