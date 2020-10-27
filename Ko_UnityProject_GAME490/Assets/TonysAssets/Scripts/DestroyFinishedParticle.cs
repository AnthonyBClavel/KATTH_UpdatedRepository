using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyFinishedParticle : MonoBehaviour
{

    private ParticleSystem thisParticleSystem;                  //variable for the particle system

    // Start is called before the first frame update
    void Start()
    {
        thisParticleSystem = GetComponent<ParticleSystem>();    //the variable to the particle system component
    }

    // Update is called once per frame
    void Update()
    {
        if (thisParticleSystem.isPlaying)                       //if the particle sytem is playing/active
        {
            return;                                             //return the statement (till the particle is done playing/active)
        }

        Destroy(gameObject, 0.5f);                              //destroy this object within the specified time (half a second)
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject, 0.5f);                              //destroy this object within the specified time (half a second)
    }

}
