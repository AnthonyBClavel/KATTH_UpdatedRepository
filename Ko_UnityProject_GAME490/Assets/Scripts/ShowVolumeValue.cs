using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowVolumeValue : MonoBehaviour
{
    public Text sliderValueText;

    void Start()
    {
        sliderValueText = GetComponent<Text>();
    }

    public void textUpdate (float value)
    {
        sliderValueText.text = Mathf.RoundToInt(value * 100) + "";
    }

}
