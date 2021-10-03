using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSFX : MonoBehaviour
{
    [SerializeField]                                                
    public AudioClip[] clips;                                       

    private AudioSource audioSource;                                

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();                  
        AudioClip clips = GetRandomClip();                          
        audioSource.PlayOneShot(clips);                             
    }

    // Update is called once per frame
    void Update()
    {   
   
    }

    // Gets a random audio clip from its respective array
    private AudioClip GetRandomClip()                               
    {
        return clips[UnityEngine.Random.Range(0, clips.Length)];
    }

}
