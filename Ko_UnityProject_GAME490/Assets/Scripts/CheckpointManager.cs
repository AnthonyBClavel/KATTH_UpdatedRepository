using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public int numMovements;

    private GameObject player;
    private IceMaterialScript iceMaterialScript;
    private TileMovementController tileMovementScript;
    private SaveManagerScript saveManagerScript;
    private Animator playerAnimator;

    Vector3 p; // Player position for debugging
    Vector3 blockPosition; // Block position
    private bool hit; // True if we hit it before, false otherwise

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        iceMaterialScript = player.GetComponent<IceMaterialScript>();
        tileMovementScript = player.GetComponent<TileMovementController>();
        saveManagerScript = FindObjectOfType<SaveManagerScript>();
    }

    // Start is called before the first frame update
    void Start()
    {      
        blockPosition = transform.position;
        hit = false;
        
        playerAnimator = player.GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            p = player.transform.position;
            Debug.Log("Player Position: " + p);
        }
    }

    // Returns the number of movements for the checkpoint
    public int getNumMovements() { return numMovements; }
    
    // Resets the player's position back to the checkpoint
    public void resetPlayerPosition()
    {
        player.transform.position = blockPosition;
        saveManagerScript.LoadPlayerRotation();
        tileMovementScript.setDestination(blockPosition);
    }

    // Resets the player's position after a certain amount of seconds
    public IEnumerator resetPlayerPositionWithDelay(float seconds)
    {
        //StartCoroutine("SetAnimInactiveDelay");     
        StartIceMatCoroutines();
        tileMovementScript.SetPlayerBoolsFalse();
        playerAnimator.enabled = false;

        yield return new WaitForSeconds(seconds);
        player.SetActive(false);
        playerAnimator.enabled = true;
        player.SetActive(true);
        player.transform.position = blockPosition;
        saveManagerScript.LoadPlayerRotation();
        tileMovementScript.setDestination(blockPosition);
        tileMovementScript.ResetTorchMeter();
        tileMovementScript.SetPlayerBoolsTrue();
    }

    public void setCheckpoint()
    {
        hit = true;
    }

    public bool hitCheckpoint()
    {
        return hit;
    }

    private void StartIceMatCoroutines()
    {
        iceMaterialScript.StartCoroutine("IncreaseAlpha_ColdUI");
        iceMaterialScript.StartCoroutine("ResetUIAlpha_ColdUI");
        iceMaterialScript.StartCoroutine("FadeMaterialToFullAlpha");
        iceMaterialScript.StartCoroutine("ResetPlayerMaterial");
    }

    private IEnumerator SetAnimInactiveDelay()
    {
        yield return new WaitForSecondsRealtime(0.1f);      
        playerAnimator.enabled = false;
    }


}
