using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalAnimEvent : MonoBehaviour
{
    private Animator crystalAnim;
    private string currentState;
    public float lightIntensity;

    public bool canResetIdle; // Used to determine during which crystal animations can the player reset its light
    public bool canFadeOut;

    // Start is called before the first frame update
    void Start()
    {
        crystalAnim = GetComponent<Animator>();
        canResetIdle = false; 
        canFadeOut = true;     
    }

    // Update is called once per frame
    void Update()
    {
        lightIntensity = GetComponent<Light>().intensity;
    }


    /*** Functions for animation events in crystal's animator ***/
    // Sets the game object inactive
    public void SetCrystalInactive()
    {
        canResetIdle = false;
        gameObject.SetActive(false);
    }

    // Changes the animation state to its "Idle" anim
    public void SetCrystalToIdle()
    {
        canResetIdle = true;
        crystalAnim.SetTrigger("Idle");
    }

    // Changes the animation state to its "FadeOut" anim
    public void FadeOutCrystal()
    {
        if(canFadeOut)
            crystalAnim.SetTrigger("FadeOut");
    }
    /*** Functions for animation events end here ***/

}
