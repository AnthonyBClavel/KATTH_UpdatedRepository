using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableBlock : MonoBehaviour
{
    bool canDestroy;

    void OnTriggerEnter(Collider other)
    {
        canDestroy = true;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return) && canDestroy)
        {
            Destroy(this.gameObject);
            canDestroy = false;
        }
    }

    
}
