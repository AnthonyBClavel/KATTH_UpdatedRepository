using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObjectAferSeconds : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 1.5f);              //destroy this object within the specified time (one and a half seconds)
    }

}
