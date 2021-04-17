using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class AmbientLoopingSFXManager : MonoBehaviour
{
    public AudioClip[] loopingClipsSFX;
    private AudioClip lastClip;
    private AudioSource audioSource;                                                                               

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();                   
    }

    // Plays a new audio clip different from the one currenlty playing
    public void ChangeAmbientLoopingSFX()
    {
        int attempts = 3;
        AudioClip newClip = loopingClipsSFX[Random.Range(0, loopingClipsSFX.Length)];

        while (newClip == lastClip && attempts > 0)
        {
            newClip = loopingClipsSFX[Random.Range(0, loopingClipsSFX.Length)];
            attempts--;
        }

        lastClip = newClip;

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();
    }

}
