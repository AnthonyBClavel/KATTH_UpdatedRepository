using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyParticle : MonoBehaviour
{
    // Destroys the object/particle when it is no longer visible by any camera
    private void OnBecameInvisible() => Destroy(gameObject);
}
