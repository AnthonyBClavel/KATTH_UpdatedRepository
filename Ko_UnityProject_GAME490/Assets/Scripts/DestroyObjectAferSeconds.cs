using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObjectAferSeconds : MonoBehaviour
{
    public float destroyAfterSeconds = 1f;

    // Start is called before the first frame update
    void Start()
    {
        // Destroys this object after specified duration - OLD VERSION
        //Destroy(gameObject, destroyAfterSeconds);
    }

    // Destroys the object/particle when it is no longer visible by any camera
    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

}
