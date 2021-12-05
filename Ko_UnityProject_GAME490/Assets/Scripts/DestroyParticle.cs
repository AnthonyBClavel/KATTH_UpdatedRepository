using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyParticle : MonoBehaviour
{
    //private new ParticleSystem particleSystem;
    //private TileMovementController playerScript;

    /*void Awake()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        particleSystem = GetComponent<ParticleSystem>();
    }*/

    // Start is called before the first frame update
    /*void Start()
    {
        StartCoroutine(DestroyParticleCheck());
    }*/

    // Destroys the object after a specific duartion (duration = seconds) - OLD VERSION
    /*private IEnumerator DestroyParticleCheck()
    {
        bool hasRestartedPuzzle = false;

        while (!hasRestartedPuzzle)
        {
            // If the puzzle was restarted, or if the particle has stopped playing...
            if (playerScript.CanRestartPuzzle && Input.GetKeyDown(KeyCode.R) || particleSystem.isStopped)
            {
                hasRestartedPuzzle = true;
                Destroy(gameObject);
            }

            yield return null;
        }
    }*/

    // Destroys the particle when it is no longer visible by any camera
    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

}
