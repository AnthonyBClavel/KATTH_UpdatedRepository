using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenuAnimEvent : MonoBehaviour
{
    public GameObject pauseScreen;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetOptionsMenuInactive()
    {
        gameObject.SetActive(false);

        pauseScreen.SetActive(true);
    }

}
