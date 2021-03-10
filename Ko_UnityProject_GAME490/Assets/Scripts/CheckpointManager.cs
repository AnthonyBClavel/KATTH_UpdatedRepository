using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour
{
    public int numMovements;

    private GameObject player;

    private Animator playerAnimator;

    Vector3 p; // Player position for debugging
    Vector3 blockPosition; // Block position
    private bool hit; // True if we hit it before, false otherwise

    private IceMaterialScript iceMaterialScript;
    private TileMovementController tileMovementScript;
    private SaveManagerScript saveManagerScript;
    private GameHUD gameHUDScript;
    private PauseMenu pauseMenuScript;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        iceMaterialScript = player.GetComponent<IceMaterialScript>();
        tileMovementScript = player.GetComponent<TileMovementController>();
        saveManagerScript = FindObjectOfType<SaveManagerScript>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
    }

    // Start is called before the first frame update
    void Start()
    {      
        hit = false;
        blockPosition = transform.position;
        playerAnimator = player.GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        /*** For Debugging purposes ***/
        /*if (Input.GetKeyDown(KeyCode.T))
        {
            p = player.transform.position;
            Debug.Log("Player Position: " + p);
        }
        /*** End Debugging ***/

    }

    // Returns the number of movements for the checkpoint
    public int getNumMovements() { return numMovements; }
    
    // Resets the player's position back to the checkpoint - also resets the player animation, rotation, torch meter, and Cold UI materials
    public void resetPlayerPosition()
    {
        gameHUDScript.SetDeathScreenInactive();
        ResetPlayer();
        ResetIceMatAlphas();   
    }

    // Resets the player's animation, position, rotation, torch meter, and Cold UI materials - ONLY USE THIS after the you press "yes" on the safety menu in the death screen
    /*public void resetPlayerPosition_DS()
    {
        player.SetActive(false);
        playerAnimator.enabled = true;
        player.SetActive(true);
        player.transform.position = blockPosition;
        saveManagerScript.LoadPlayerRotation();
        tileMovementScript.setDestination(blockPosition);
        tileMovementScript.ResetTorchMeter();

        gameHUDScript.deathScreen.SetActive(false);
        ResetIceMatAlphas();
    }*/

    // Resets the player's position and other elements after a certain amount of seconds
    public IEnumerator resetPlayerPositionWithDelay(float seconds)
    {
        DetermineIceMatCoroutines();
        tileMovementScript.SetPlayerBoolsFalse();
        playerAnimator.enabled = false;
        pauseMenuScript.canPause = false;
        gameHUDScript.SetDeathScreenActiveCheck();

        yield return new WaitForSeconds(seconds);

        if(!gameHUDScript.canDeathScreen)
            ResetPlayer();         
    }

    // Resets the player's position and other elements after a certain amount of seconds - ONLY USED IN TUTORIAL
    public IEnumerator resetPlayerPositionInTutorialWithDelay(float seconds)
    {
        DetermineIceMatCoroutines();
        tileMovementScript.SetPlayerBoolsFalse();
        playerAnimator.enabled = false;
        pauseMenuScript.canPause = false;

        yield return new WaitForSeconds(seconds);
        ResetPlayer();
    }

    public void setCheckpoint()
    {
        hit = true;
    }

    public bool hitCheckpoint()
    {
        return hit;
    }

    // Determines if the death screen should be activated
    private void DetermineIceMatCoroutines()
    {
        if (gameHUDScript.canDeathScreen && SceneManager.GetActiveScene().name != "TutorialMap")
            StartIceMatCoroutines02();
        else
            StartIceMatCoroutines();
    }


    // Resets the player's animation, position, rotation, and torch meter
    private void ResetPlayer()
    { 
        player.SetActive(false);
        playerAnimator.enabled = true;
        player.SetActive(true);
        pauseMenuScript.canPause = true;
        player.transform.position = blockPosition;
        saveManagerScript.LoadPlayerRotation();
        tileMovementScript.setDestination(blockPosition);
        tileMovementScript.ResetTorchMeter();
        tileMovementScript.SetPlayerBoolsTrue();
    }

    // Fades in the alphas for the Ice Material and Cold UI and resets them after a delay
    private void StartIceMatCoroutines()
    {
        iceMaterialScript.StartCoroutine("IncreaseAlpha_ColdUI");
        iceMaterialScript.StartCoroutine("ResetUIAlpha_ColdUI");
        iceMaterialScript.StartCoroutine("FadeMaterialToFullAlpha");
        iceMaterialScript.StartCoroutine("ResetPlayerMaterial");
    }

    // Fades in the alphas for the Ice Material and Cold UI
    private void StartIceMatCoroutines02()
    {
        iceMaterialScript.StartCoroutine("IncreaseAlpha_ColdUI");
        iceMaterialScript.StartCoroutine("FadeMaterialToFullAlpha");
    }

    // Resets the alphas for the Ice Material and Cold UI immediately 
    private void ResetIceMatAlphas()
    {
        iceMaterialScript.ResetPlayerMaterial02();
        iceMaterialScript.ResetUIAlpha_ColdUI02();
    }


}
