using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    //public static PlayerSounds instance;

    public bool canPlayFootsteps;
    public bool canCheckBridgeTiles;

    public AudioClip[] snowFootstepClips;                                    

    public AudioClip[] grassFootstepClips;                                   

    public AudioClip[] stoneFootstepClips;

    public AudioClip[] metalFootstepClips;

    public AudioClip[] woodFootstepClips;

    public AudioClip[] woodenCrateFootstepClips;

    float rayLength = 1f;

    private AudioSource audioSource;
    private new string tag;
    private new string name;

    void Start()
    {
        // Sets this script as an instance - other scripts can call it without creating a variable for it
        //instance = this;

        canPlayFootsteps = true;
        canCheckBridgeTiles = false;
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
       
    }

    // Checks to see which tile the player is on and determines which array of audio clips to play
    public void TileCheck()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), -transform.up);
        RaycastHit hit;

        Debug.DrawRay(myRay.origin, myRay.direction, Color.blue);                                                                                

        if (Physics.Raycast(myRay, out hit, rayLength) && canPlayFootsteps)
        {
            tag = hit.collider.tag;
            name = hit.collider.name;

            CheckForSnowTiles();
            CheckForGrassTiles();
            CheckForStoneTiles();
            CheckForMetalTiles();
            CheckForBridgeTiles();
            CheckForCrateTiles();
        }
    }

    public void BridgeTileCheck()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0.5f, 0), -transform.up);
        RaycastHit hit;

        Debug.DrawRay(myRay.origin, myRay.direction, Color.blue);

        if (Physics.Raycast(myRay, out hit, rayLength) && canPlayFootsteps && canCheckBridgeTiles)
        {
            tag = hit.collider.tag;
            name = hit.collider.name;

            CheckForBridgeTiles();
        }
    }

    private void CheckForSnowTiles()
    {
        if (tag == "SnowTiles" || name == "SnowCheckpoint" || name == "Checkpoint_SnowTiles" || name == "SnowTileBlock" || name == "BarrenLandsBlock" || name == "Checkpoint_BarrenLandsTiles")
        {
            audioSource.volume = 0.7f;
            audioSource.pitch = 1.0f;
            SnowFootsteps();
        }
    }

    private void CheckForGrassTiles()
    {
        if (tag == "GrassTiles" || name == "GrassCheckpoint" || name == "Checkpoint_GrassTiles" || name == "PatchyGrassBlock")
        {
            audioSource.volume = 0.7f;
            audioSource.pitch = 1.0f;
            GrassFootsteps();
        }
    }

    private void CheckForStoneTiles()
    {
        if (tag == "StoneTiles" || name == "CaveCheckpoint" || name == "Checkpoint_CaveTiles" || name == "CaveBlock")
        {
            audioSource.volume = 0.55f; //0.5f
            audioSource.pitch = 1.2f;
            StoneFootsteps();
        }
    }

    private void CheckForMetalTiles()
    {
        if (tag == "MetalTiles" || name == "MetalCheckpoint" || name == "Checkpoint_MetalTiles" || name == "PowerStationBlock")
        {
            audioSource.volume = 1.0f;
            audioSource.pitch = 1.0f;
            MetalFootsteps();
        }
    }

    private void CheckForBridgeTiles()
    {
        if (name == "BridgeBlock" || tag == "BridgeController" || tag == "LastBridgeTile" || tag == "WoodTiles")
        {
            audioSource.volume = 0.8f; //0.75f
            audioSource.pitch = 0.9f;
            WoodFootsteps();
        }
    }

    private void CheckForCrateTiles()
    {
        if (tag == "Obstacle")
        {
            audioSource.volume = 0.4f; //0.34f
            audioSource.pitch = 1.0f;
            WoodenCrateFootsteps();
        }
    }

    /*** The functions below are for playing/getting the a random audio clip from each array ***/
    // Plays the random audio clip it aquired
    private void SnowFootsteps()                                                                                                                 
    {
        AudioClip snowFootstepClips = GetRandomClipSF();                                                                                        
        audioSource.PlayOneShot(snowFootstepClips);                                                                                                       
    }

    // Gets a random audio clip from its respective array
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
