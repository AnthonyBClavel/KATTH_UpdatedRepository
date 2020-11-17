using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButtonController : MonoBehaviour
{
    public int index;
    [SerializeField] bool keyDown;
    [SerializeField] int maxIndex;
    public AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetAxis ("Vertical") != 0)
        {
            if(!keyDown)
            {
                //increase the index if the max index hasn't been met yet
                if(Input.GetAxis("Vertical") < 0)
                {
                    //if you get to the bottom button, go back to the top button
                    if(index < maxIndex)
                    {
                        index++;
                    }
                    else
                    {
                        index = 0;
                    }
                }
                else if (Input.GetAxis("Vertical") > 0)
                {
                    //if index is greater than zero, you can go down the index
                    if(index > 0)
                    {
                        index--;
                    }
                    //if it is zero...
                    else
                    {
                        index = maxIndex;
                    }
                }
                keyDown = true;
            }
        }
        else
        {
            keyDown = false;
        }
    }

}
