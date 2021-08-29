using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour
{
    public int numMovements;

    [Header("0 = up, 90 = right, 180 = down, 270 = left")]
    public int playerDirection;

    private GameObject player;
    private Animator playerAnimator;

    float rayLengthLBT = 1f;

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
        //DetermineIceMatCoroutines();
        StartIceMatCoroutines02();
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
        //DetermineIceMatCoroutines();
        StartIceMatCoroutines02();
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

    // Determines what coroutines should play if the death screen should is activated
    /*private void DetermineIceMatCoroutines()
    {
        if (gameHUDScript.canDeathScreen && SceneManager.GetActiveScene().name != "TutorialMap")
            StartIceMatCoroutines02();
        else
            StartIceMatCoroutines();
    }*/


    // Resets the player's animation, position, rotation, and torch meter
    private void ResetPlayer()
    {
        ResetIceMatAlphas();
        //if (!player.GetComponent<TileMovementController>().checkIfOnCheckpoint())
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
    /*private void StartIceMatCoroutines()
    {
        iceMaterialScript.StartCoroutine("IncreaseAlpha_ColdUI");
        iceMaterialScript.StartCoroutine("ResetUIAlpha_ColdUI");
        iceMaterialScript.StartCoroutine("FadeMaterialToFullAlpha");
        iceMaterialScript.StartCoroutine("ResetPlayerMaterial");
    }*/

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

    public bool LastBridgeTileCheck()
    {
        //Debug.Log("Checks for last bridge tile");

        Ray myRayLBT01 = new Ray(transform.position + new Vector3(1, 0.5f, 0), Vector3.down);
        Ray myRayLBT02 = new Ray(transform.position + new Vector3(-1, 0.5f, 0), Vector3.down);
        Ray myRayLBT03 = new Ray(transform.position + new Vector3(0, 0.5f, 1), Vector3.down);
        Ray myRayLBT04 = new Ray(transform.position + new Vector3(0, 0.5f, -1), Vector3.down);

        RaycastHit hit;

        Debug.DrawRay(myRayLBT01.origin, myRayLBT01.direction, Color.green);
        Debug.DrawRay(myRayLBT02.origin, myRayLBT02.direction, Color.red);
        Debug.DrawRay(myRayLBT03.origin, myRayLBT03.direction, Color.blue);
        Debug.DrawRay(myRayLBT04.origin, myRayLBT04.direction, Color.yellow);

        GameObject savedInvisibleBlock = FindObjectOfType<SaveManagerScript>().savedInvisibleBlock;

        if (Physics.Raycast(myRayLBT01, out hit, rayLengthLBT))
        {
            GameObject lastBridgeTile = hit.collider.gameObject;
            float lastBridgeTileX = lastBridgeTile.transform.position.x;
            float lastBridgeTileZ = lastBridgeTile.transform.position.z;
            string tag = lastBridgeTile.tag;

            if (tag == "LastBridgeTile")
            {
                Debug.Log("Detected ONE");

                savedInvisibleBlock.transform.position = new Vector3(lastBridgeTileX, 1, lastBridgeTileZ);
                return true;
            }
        }

        if (Physics.Raycast(myRayLBT02, out hit, rayLengthLBT))
        {
            GameObject lastBridgeTile = hit.collider.gameObject;
            float lastBridgeTileX = lastBridgeTile.transform.position.x;
            float lastBridgeTileZ = lastBridgeTile.transform.position.z;
            string tag = lastBridgeTile.tag;

            if (tag == "LastBridgeTile")
            {
                Debug.Log("Detected TWO");

                savedInvisibleBlock.transform.position = new Vector3(lastBridgeTileX, 1, lastBridgeTileZ);
                return true;
            }
        }

        if (Physics.Raycast(myRayLBT03, out hit, rayLengthLBT))
        {
            GameObject lastBridgeTile = hit.collider.gameObject;
            float lastBridgeTileX = lastBridgeTile.transform.position.x;
            float lastBridgeTileZ = lastBridgeTile.transform.position.z;
            string tag = lastBridgeTile.tag;

            if (tag == "LastBridgeTile")
            {
                Debug.Log("Detected THREE");

                savedInvisibleBlock.transform.position = new Vector3(lastBridgeTileX, 1, lastBridgeTileZ);
                return true;
            }
        }

        if (Physics.Raycast(myRayLBT04, out hit, rayLengthLBT))
        {
            GameObject lastBridgeTile = hit.collider.gameObject;
            float lastBridgeTileX = lastBridgeTile.transform.position.x;
            float lastBridgeTileZ = lastBridgeTile.transform.position.z;
            string tag = lastBridgeTile.tag;

            if (tag == "LastBridgeTile")
            {
                Debug.Log("Detected FOUR");

                savedInvisibleBlock.transform.position = new Vector3(lastBridgeTileX, 1, lastBridgeTileZ);
                return true;
            }
        }

        return false;
    }



}
