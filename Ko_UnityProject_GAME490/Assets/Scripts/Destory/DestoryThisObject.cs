using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryThisObject : MonoBehaviour
{
    private TileMovementController playerScript;
    public static DestoryThisObject instance;
    private bool canDestory;

    void Awake()
    {
        playerScript = FindObjectOfType<TileMovementController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CheckToDestoryObject());
    }

    // Update is called once per frame
    void Update()
    {
        //CheckIfCanDestroy();
    }
    
    // For destroying the instantiated invisible blocks when the player is not on the bridge
    private IEnumerator CheckToDestoryObject()
    {
        while(true)
        {
            if (playerScript.canRestartPuzzle)
                Destroy(gameObject);
            yield return new WaitForSeconds(3f);
        }
    }

    // This function is a bit performance heavy since it checks every frame
    /*public void CheckIfCanDestroy()
    {
        if (playerScript.canRestartPuzzle)
            Destroy(gameObject);
    }*/

}
