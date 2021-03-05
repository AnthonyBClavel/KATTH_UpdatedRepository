using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyFinishedParticle : MonoBehaviour
{
    private ParticleSystem thisParticleSystem;                  

    // Start is called before the first frame update
    void Start()
    {
        thisParticleSystem = GetComponent<ParticleSystem>();    
    }

    // Update is called once per frame
    void Update()
    {
        if (thisParticleSystem.isPlaying)
            return;                                             

        Destroy(gameObject, 0.5f);                              
    }

    // Destroys this object after a specified time once the particle system is done playing
    void OnBecameInvisible()
    {
        Destroy(gameObject, 0.5f);                              
    }

}
