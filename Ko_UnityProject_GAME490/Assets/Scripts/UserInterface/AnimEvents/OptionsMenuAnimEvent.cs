using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenuAnimEvent : MonoBehaviour
{
    public GameObject mainCanvas;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // For an animation event in the options menu
    public void SetOptionsMenuInactive()
    {
        gameObject.SetActive(false);

        mainCanvas.SetActive(true);
    }

}
