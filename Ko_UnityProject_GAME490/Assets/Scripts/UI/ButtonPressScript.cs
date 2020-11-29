using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonPressScript : MonoBehaviour
{
    private Button myButton;
    public Animator anim;

    // Start is called before the first frame update
    void Awake()
    {
        myButton = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return) && EventSystem.current.alreadySelecting)
        {
            anim.SetTrigger("Pressed");
        }
    }

}
