using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafetyMenu : MonoBehaviour
{
    public Animator pauseScreenAnim;
    private PauseMenu pauseMenuScript;

    // Start is called before the first frame update
    void Start()
    {
        pauseMenuScript = FindObjectOfType<PauseMenu>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CloseSafetyMenu()
    {
        gameObject.SetActive(false);

        if(pauseMenuScript.isChangingScenes == false)
            pauseScreenAnim.SetTrigger("PS_PopIn02");
    }
    
}
