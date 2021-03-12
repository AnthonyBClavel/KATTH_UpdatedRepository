using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ButtonPressScript : MonoBehaviour
{
    //private MenuSounds menuSoundsScript;
    //private EventSystem eventSystem;
    
    void Awake()
    {
        //eventSystem = FindObjectOfType<EventSystem>();
        //menuSoundsScript = FindObjectOfType<MenuSounds>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //CheckIfButtonPressed();
    }

    // Plays the ButtonClickSFX if this gameObject is selected and you hit return
    /*private void CheckIfButtonPressed()
    {
        if (Input.GetKeyDown(KeyCode.Return) && eventSystem.currentSelectedGameObject == this.gameObject)
        {
            
        }
    }*/

}
