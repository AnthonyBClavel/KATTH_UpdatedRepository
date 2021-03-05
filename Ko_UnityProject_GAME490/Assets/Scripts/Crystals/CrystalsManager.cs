using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalsManager : MonoBehaviour
{
    public GameObject crystalGlow;
    public Animator crystalAnim;   
    public CrystalAnimEvent crystalAnimScript;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetGlowActive()
    {      
        crystalGlow.SetActive(true);
        EnableCrystalFade();
    }

    public void SetGlowInactive()
    {
        crystalGlow.SetActive(false);
    }

    // Resets the glowing animation for the crystal, but only after it fades in
    public void ResetCrystalIdleAnim()
    {
        if(crystalAnimScript.canResetIdle)
            crystalAnim.SetTrigger("Idle");
    }

    // Sets crystal canResetIdle bool to false
    public void ResetCrystalBool()
    {
        crystalAnimScript.canResetIdle = false;
    }

    // Prevents the crystal from transitioning to its "fade out" animation
    public void DisableCrystalFade()
    {
        crystalAnimScript.canFadeOut = false;
    }

    // Allows the crystal to transition to its "fade out" animation
    public void EnableCrystalFade()
    {
        crystalAnimScript.canFadeOut = true;
    }

}
