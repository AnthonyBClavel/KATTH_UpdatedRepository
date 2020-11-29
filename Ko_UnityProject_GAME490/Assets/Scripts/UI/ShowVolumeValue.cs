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

    public void textUpdate (float value)
    {
        sliderValueText.text = Mathf.RoundToInt(value * 100) + "";
    }

}
