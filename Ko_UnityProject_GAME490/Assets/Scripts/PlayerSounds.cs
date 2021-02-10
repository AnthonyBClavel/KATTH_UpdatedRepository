using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    public static PlayerSounds instance;


    //creates arrays of audio clips for each puzzle area
    public AudioClip[] snowFootstepClips;                                    

    public AudioClip[] grassFootstepClips;                                   

    public AudioClip[] stoneFootstepClips;

    public AudioClip[] metalFootstepClips;

    public AudioClip[] woodFootstepClips;

    public AudioClip[] woodenCrateFootstepClips;


    float rayLength = 1f;

    private AudioSource audioSource;

    void Start()
    {
        //set this script as an instance here so it can be called upon other scripts without creating a variable for it
        instance = this;

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
       
    }

    //the function that checks to see which tile the player is on and determnes which array of audio clips to play
    public void TileCheck()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), -transform.up);
        RaycastHit hit;

        Debug.DrawRay(myRay.origin, myRay.direction, Color.blue);                                                                                

        if (Physics.Raycast(myRay, out hit, rayLength))
        {
            if (hit.collider.tag == "SnowTiles" || hit.collider.gameObject.name == "SnowCheckpoint" || hit.collider.gameObject.name == "Checkpoint_SnowTiles" || hit.collider.name == "SnowTileBlock")
            {
                audioSource.volume = 0.7f;
                audioSource.pitch = 1.0f;
                SnowFootsteps();
            }
            else if (hit.collider.tag == "GrassTiles" || hit.collider.gameObject.name == "GrassCheckpoint" || hit.collider.gameObject.name == "Checkpoint_GrassTiles" || hit.collider.name == "PatchyGrassBlock")
            {
                audioSource.volume = 0.7f;
                audioSource.pitch = 1.0f;
                GrassFootsteps();
            }
            else if (hit.collider.tag == "StoneTiles" || hit.collider.gameObject.name == "CaveCheckpoint" || hit.collider.gameObject.name == "Checkpoint_CaveTiles" || hit.collider.name == "CaveBlock")
            {
                audioSource.volume = 0.55f; //0.5f
                audioSource.pitch = 1.2f;
                StoneFootsteps();
            }
            else if (hit.collider.tag == "MetalTiles" || hit.collider.gameObject.name == "MetalCheckpoint" || hit.collider.gameObject.name == "Checkpoint_MetalTiles" || hit.collider.gameObject.name == "Checkpoint_EmberTiles" || hit.collider.name == "PowerStationBlock" || hit.collider.name == "EmberCityBlock")
            {
                audioSource.volume = 1.0f;
                audioSource.pitch = 1.0f;
                MetalFootsteps();
            }
            else if (hit.collider.tag == "WoodTiles" || hit.collider.tag == "MoveCameraBlock" || hit.collider.name == "BridgeBlock")
            {
                audioSource.volume = 0.75f;
                audioSource.pitch = 0.9f;
                WoodFootsteps();
            }
            else if (hit.collider.tag == "Obstacle")
            {
                audioSource.volume = 0.36f; //0.34f
                audioSource.pitch = 1.0f;
                WoodenCrateFootsteps();
            }
        }
    }

    /* the functions below are for playing/getting the a random audio clip for each array */
    //the function below plays the random audio clip
    private void SnowFootsteps()                                                                                                                 
    {
        AudioClip snowFootstepClips = GetRandomClipSF();                                                                                        
        audioSource.PlayOneShot(snowFootstepClips);                                                                                                       
    }

    //the function below gets a random audio clip from its respective array
    private AudioClip GetRandomClipSF()
    { 
        return snowFootstepClips[UnityEngine.Random.Range(0, snowFootstepClips.Length)];                                                                                
    }


    private void GrassFootsteps()
    {
        AudioClip grassFootstepClips = GetRandomClipGF();
        audioSource.PlayOneShot(grassFootstepClips);
    }
    private AudioClip GetRandomClipGF()
    {
        return grassFootstepClips[UnityEngine.Random.Range(0, grassFootstepClips.Length)];
    }


    private void StoneFootsteps()
    {
        AudioClip stoneFootstepClips = GetRandomClipSSFF();
        audioSource.PlayOneShot(stoneFootstepClips);
    }
    private AudioClip GetRandomClipSSFF()
    {
        return stoneFootstepClips[UnityEngine.Random.Range(0, stoneFootstepClips.Length)];
    }
    

    private void MetalFootsteps()
    {
        AudioClip metalFootstepClips = GetRandomClipMF();
        audioSource.PlayOneShot(metalFootstepClips);
    }
    private AudioClip GetRandomClipMF()
    {
        return metalFootstepClips[UnityEngine.Random.Range(0, metalFootstepClips.Length)];
    }


    private void WoodFootsteps()
    {
        AudioClip woodFootstepClips = GetRandomClipWF();
        audioSource.PlayOneShot(woodFootstepClips);
    }
    private AudioClip GetRandomClipWF()
    {
        return woodFootstepClips[UnityEngine.Random.Range(0, woodFootstepClips.Length)];
    }


    private void WoodenCrateFootsteps()
    {
        AudioClip woodenCrateFootstepClips = GetRandomClipWCF();
        audioSource.PlayOneShot(woodenCrateFootstepClips);
    }
    private AudioClip GetRandomClipWCF()
    {
        return woodenCrateFootstepClips[UnityEngine.Random.Range(0, woodenCrateFootstepClips.Length)];
    }

}
