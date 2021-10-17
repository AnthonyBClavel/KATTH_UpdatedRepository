using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SkipButtonStat
{
    public SkipSceneButton skipButtonBar;

    [SerializeField]
    private float maxValue;

    [SerializeField]
    private float currentValue;

    public float CurrentVal
    {
        get
        {
            return currentValue;
        }

        set
        {
            this.currentValue = Mathf.Clamp(value, 0, MaxVal);
            //skipButtonBar.Value = currentValue;
        }
    }

    public float MaxVal
    {
        get
        {
            return maxValue;
        }

        set
        {
            this.maxValue = value;
            //skipButtonBar.MaxValue = maxValue;
        }
    }

    public void Initialize()
    {
        this.MaxVal = maxValue;
        this.CurrentVal = currentValue;
    }

    public void setMaxValue(int newMax)
    {
        maxValue = (float)newMax;
        //skipButtonBar.setMaxValue(maxValue);
    }

    public void setCurrentVal(int newCurrentVal)
    {
        CurrentVal = (float)newCurrentVal;
    }

}
