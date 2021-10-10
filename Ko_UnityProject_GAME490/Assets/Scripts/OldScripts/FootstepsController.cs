using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FootstepsController : MonoBehaviour
{
    public bool canPlayFootsteps = true;
    public bool canPlaySecondFootstep = false;

    //private float playerLerpLength;
    private float rayLength = 1f;

    private new string tag;
    private new string name;

    private IEnumerator playerFootstepsCoroutine;
    private TileMovementController playerScript;
    private AudioManager audioManagerScript;

    void Awake()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        audioManagerScript = FindObjectOfType<AudioManager>();

        //playerLerpLength = playerScript.lerpLength;
    }

    void Start()
    {

    }

    // Plays the footsteps coroutine for the player
    /*public void PlayerFootstepsSFX()
    {
        if (playerFootstepsCoroutine != null)
            StopCoroutine(playerFootstepsCoroutine);

        playerFootstepsCoroutine = PlayerFootsteps();
        StartCoroutine(playerFootstepsCoroutine);
    }*/

    // Checks which footstep sfx to play for the PLAYER - determined by the tag/name of object the player walks on
    private void PlayerFootstepSFXCheck()
    {
        Ray myRay = new Ray(playerScript.transform.position + new Vector3(0, 0.5f, 0), Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.blue);                                                                                

        if (Physics.Raycast(myRay, out hit, rayLength) && canPlayFootsteps)
        {
            if (tag != hit.collider.tag)
                tag = hit.collider.tag;

            if (name != hit.collider.name)
                name = hit.collider.name;

            // Checks to play a footstep sfx for grass
            if (tag == "GrassTile" || name == "Checkpoint_GrassTile")
                audioManagerScript.PlayGrassFootstepSFX();

            // Checks to play a footstep sfx for snow
            else if (tag == "SnowTile" || name == "Checkpoint_SnowTile" || name == "Checkpoint_SnowTile02")
                audioManagerScript.PlaySnowFootstepSFX();

            // Checks to play a footstep sfx for stone
            else if (tag == "StoneTile" || name == "Checkpoint_StoneTile")
                audioManagerScript.PlayStoneFootstepSFX();

            // Checks to play a footstep sfx for metal
            else if (tag == "MetalTile" || name == "Checkpoint_MetalTile")
                audioManagerScript.PlayMetalFootstepSFX();

            // Checks to play a footstep sfx for wood (bridge tiles)
            else if (tag == "BridgeTile" || name == "BridgeTile")
                audioManagerScript.PlayWoodFootstepSFX();

            // Checks to play a footstep sfx for crates (wooden crates)
            else if (tag == "PushableBlock")
                audioManagerScript.PlayCrateFootstepSFX();
        }
    }

    // Plays the footsteps sfx based on the player's playerLerpLength (time it takes to move from its current position to the destination)
    /*private IEnumerator PlayerFootsteps()
    {
        float duration = playerLerpLength / 4f;

        // Plays a footstep sfx when the player has traveled for 1/4th of the playerLerpLength
        yield return new WaitForSeconds(duration);
        PlayerFootstepSFXCheck();

        // Plays a footstep sfx when the player has traveled for 3/4th of the playerLerpLength 
        yield return new WaitForSeconds(duration * 2f);
        if (canPlaySecondFootstep)
            PlayerFootstepSFXCheck();
    }*/

}
