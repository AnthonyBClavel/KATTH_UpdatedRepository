﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TorchMeterStat // Dont change this script - use the TileMovementController's script component (in the Unity inspector) to manipulate the torch meter                
{
    public TorchMeterScript bar;

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
            bar.Value = currentValue;
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
            bar.MaxValue = maxValue;
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
        bar.setMaxValue(maxValue);
    }

    public void setCurrentVal(int newCurrentVal)
    {
        CurrentVal = (float)newCurrentVal;
    }

}
