using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAnimScript : MonoBehaviour
{
    private Animator characterAnim;

    // Start is called before the first frame update
    void Start()
    {
        characterAnim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            characterAnim.SetTrigger("Wave");
        }
    }
}
