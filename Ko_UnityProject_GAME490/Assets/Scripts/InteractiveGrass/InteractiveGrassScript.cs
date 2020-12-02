﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveGrassScript : MonoBehaviour
{
    public Material[] materials;
    public Transform thePlayer;
    Vector3 thePosition;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("writeToMaterial");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator writeToMaterial()
    {
        while(true)
        {   
            thePosition = thePlayer.transform.position;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i].SetVector("_position", thePosition);
            }

            yield return null;
        }      
    }

}