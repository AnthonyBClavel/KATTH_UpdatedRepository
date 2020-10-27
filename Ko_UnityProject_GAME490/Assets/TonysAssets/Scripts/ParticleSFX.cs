using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSFX : MonoBehaviour
{

    [SerializeField]                                                //allows you to see and manipulate the variable within the Unity inspector if it's private
    public AudioClip[] clips;                                       //creates an array of audio clips

    private AudioSource audioSource;                                //variable for the AudioSource component

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();                  //sets the variable to the AudioSource component
        AudioClip clips = GetRandomClip();                          //calls the function stated below (at bottom of script)
        audioSource.PlayOneShot(clips);                             //plays an audio clip - the clip cannot be canceled if another is played
    }

    // Update is called once per frame
    void Update()
    {   
   
    }

    private AudioClip GetRandomClip()                               //the function for getting a random audio clip within the array
    {
        return clips[UnityEngine.Random.Range(0, clips.Length)];    //selects a random audio clip based on the size of the array - its length
    }

}
