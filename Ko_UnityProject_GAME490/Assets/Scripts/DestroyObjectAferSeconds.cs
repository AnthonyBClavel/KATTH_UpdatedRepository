﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObjectAferSeconds : MonoBehaviour
{
    public float destroyAfterSeconds;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, destroyAfterSeconds);              // Destroy this object within the specified time
        //Destroy(gameObject, 1.5f);            // Original 
    }

}
