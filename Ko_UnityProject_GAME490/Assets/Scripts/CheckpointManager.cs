using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour
{
    public int numMovements;

    private GameObject bridgeTileCheck;
    private GameObject player;
    private GameObject savedInvisibleBlock;
    private GameObject deathScreen;
    private Animator playerAnimator;

    private float rayLengthLBT = 1f;
    private bool hit; // True if we hit it before, false otherwise
    private Vector3 checkpointPosition;

    private IceMaterialScript iceMaterialScript;
    private TileMovementController playerScript;
    private SaveManagerScript saveManagerScript;
    private GameHUD gameHUDScript;
    private PauseMenu pauseMenuScript;

    void Awake()
    {
        SetScript();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {      
        hit = false;
        checkpointPosition = transform.position;
    }

    // Returns the number of movements for the checkpoint - movements determined within the script's component in unity inspector
    public int getNumMovements() { return numMovements; }

    public void setCheckpoint()
    {
        hit = true;
    }

    public bool hitCheckpoint()
    {
        return hit;
    }

    // Checks for the closet bridge tile and sets the savedInvisibleBlock's position to that bridge tile
    public bool LastBridgeTileCheck()
    {
        int rayDirection = 0;

        for (int i = 0; i <= 270; i += 90)
        {
            Ray myRayLBT = new Ray(bridgeTileCheck.transform.position + new Vector3(0, -0.1f, 0), bridgeTileCheck.transform.TransformDirection(Vector3.forward));
            RaycastHit hit;
            Debug.DrawRay(myRayLBT.origin, myRayLBT.direction, Color.red);

            if (Physics.Raycast(myRayLBT, out hit, rayLengthLBT))
            {
                GameObject lastBridgeTile = hit.collider.gameObject;
                float lastBridgeTileX = lastBridgeTile.transform.position.x;
                float lastBridgeTileZ = lastBridgeTile.transform.position.z;
                string name = lastBridgeTile.name;
                string tag = lastBridgeTile.tag;

                if (name == "BridgeTile" || tag == "BridgeTile")
                {
                    //Debug.Log("BridgeBlock Found");
                    playerScript.bridge = lastBridgeTile.transform.parent.gameObject;
                    savedInvisibleBlock.transform.position = new Vector3(lastBridgeTileX, 1, lastBridgeTileZ);

                    // Saves the player's rotataion as the opposite angle from which the BridgeBlock was found
                    float playerRotation = bridgeTileCheck.transform.localEulerAngles.y - 180;
                    player.transform.localEulerAngles = new Vector3(0, playerRotation, 0);
                    saveManagerScript.SavePlayerRotation(playerRotation);

                    return true;
                }
            }

            //Debug.Log("BridgeBlock Not Found");
            i = rayDirection;
            rayDirection += 90;
            bridgeTileCheck.transform.localEulerAngles = new Vector3(0, rayDirection, 0);
        }

        return false;
    }

    // Sets the player's position to the checkpoint - also resets the player's animation, rotation, torch meter, and Cold UI elements
    public void resetPlayerPosition()
    {
        gameHUDScript.SetDeathScreenInactive();
        ResetPlayer();
        ResetIceMatAlphas();   
    }

    // Resets the player's position, animation, rotation, torch meter, and Cold UI elements - ONLY USE THIS after pressing "yes" in the death screen's safety menu 
    public void resetPlayerPosition_DS()
    {
        player.SetActive(false);
        playerAnimator.enabled = true;
        player.SetActive(true);
        player.transform.position = checkpointPosition;
        saveManagerScript.LoadPlayerRotation();
        playerScript.setDestination(checkpointPosition);
        playerScript.ResetTorchMeter();

        deathScreen.SetActive(false);
        ResetIceMatAlphas();
    }

    // Resets the player's position and other elements after a certain amount of seconds
    public IEnumerator resetPlayerPositionWithDelay(float seconds)
    {
        StartIceMatCoroutines();
        playerScript.SetPlayerBoolsFalse();
        playerAnimator.enabled = false;
        pauseMenuScript.canPause = false;
        gameHUDScript.SetDeathScreenActiveCheck();

        yield return new WaitForSeconds(seconds);

        if (!gameHUDScript.canDeathScreen)
            ResetPlayer();         
    }

    // Resets the player's position and other elements after a certain amount of seconds - ONLY USED IN TUTORIAL
    public IEnumerator resetPlayerPositionInTutorialWithDelay(float seconds)
    {
        StartIceMatCoroutines();
        playerScript.SetPlayerBoolsFalse();
        playerAnimator.enabled = false;
        pauseMenuScript.canPause = false;

        yield return new WaitForSeconds(seconds);
        ResetPlayer();
    }

    // Resets player elements (position, animation, rotation, torch meter)
    private void ResetPlayer()
    {
        ResetIceMatAlphas();

        player.SetActive(false);
        playerAnimator.enabled = true;
        player.SetActive(true);
        pauseMenuScript.canPause = true;

        player.transform.position = checkpointPosition;
        saveManagerScript.LoadPlayerRotation();
        playerScript.setDestination(checkpointPosition);
        playerScript.ResetTorchMeter();
        playerScript.SetPlayerBoolsTrue();
    }

    // Fades in the alphas for all Cold UI elements
    private void StartIceMatCoroutines()
    {
        iceMaterialScript.StartCoroutine("IncreaseFrostedBorderAlpha");
        iceMaterialScript.StartCoroutine("IncreaseIceMaterialAlpha");
    }

    // Resets the alphas for all Cold UI elements
    private void ResetIceMatAlphas()
    {
        iceMaterialScript.ResetIceMaterial();
        iceMaterialScript.ResetFrostedBorderAlpha();
    }

    // Sets the scripts to use
    private void SetScript()
    {
        iceMaterialScript = FindObjectOfType<IceMaterialScript>();
        playerScript = FindObjectOfType<TileMovementController>();
        saveManagerScript = FindObjectOfType<SaveManagerScript>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "BridgeTileCheck")
                bridgeTileCheck = child;
        }

        for (int i = 0; i < saveManagerScript.transform.childCount; i++)
        {
            GameObject child = saveManagerScript.transform.GetChild(i).gameObject;

            if (child.name == "SavedInvisibleBlock")
                savedInvisibleBlock = child;
        }

        for (int i = 0; i < gameHUDScript.transform.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.GetChild(i).gameObject;

            if (child.name == "OptionalDeathScreen")
                deathScreen = child;
        }

        player = playerScript.gameObject;
        playerAnimator = playerScript.GetComponentInChildren<Animator>();
    }

}
