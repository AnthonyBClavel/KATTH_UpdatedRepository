using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    private string sceneName;

    private GameObject savedInvisibleBlock;
    private GameObject player;
    private GameObject[] checkpoints;

    private CameraController cameraScript;
    private GameManager gameManagerScript;

    // Awake is called before Start()
    void Awake()
    {
        sceneName = SceneManager.GetActiveScene().name;

        SetScripts();
        SetElements();
        LoadPuzzleCheck();
        LoadPuzzleDebugCheck();
    }

    // Saves the player position
    public void SavePlayerPosition(GameObject checkpoint)
    {
        if (sceneName == "TutorialMap") return;

        PlayerPrefs.SetFloat("p_x", checkpoint.transform.position.x);
        PlayerPrefs.SetFloat("p_z", checkpoint.transform.position.z);
        PlayerPrefs.Save();
    }

    // Saves the player rotation
    public void SavePlayerRotation(float playerRotation)
    {
        PlayerPrefs.SetFloat("r_y", playerRotation);
        PlayerPrefs.Save();
    }

    // Saves the camera position
    public void SaveCameraPosition()
    {
        if (sceneName == "TutorialMap") return;

        PlayerPrefs.SetInt("cameraIndex", cameraScript.PuzzleViewIndex);
        PlayerPrefs.Save();
    }

    // Saves the name of the collected artifact
    public void SaveCollectedArtifact(string collectedArtifacts)
    {
        PlayerPrefs.SetString("listOfArtifacts", collectedArtifacts);
        PlayerPrefs.Save();
    }

    // Saves the number of collected artifacts 
    public void SaveNumberOfArtifactsCollected(int numberOfArtifactsCollected)
    {
        PlayerPrefs.SetInt("numberOfArtifactsCollected", numberOfArtifactsCollected);
        PlayerPrefs.Save();
    }

    // Saves the name of the current scene
    private void SaveSceneName()
    {     
        if (sceneName == "MainMenu") return;

        PlayerPrefs.SetString("savedScene", sceneName);
        PlayerPrefs.Save();
    }

    // Loads the player rotation
    public void LoadPlayerRotation()
    {
        float yRot = PlayerPrefs.GetFloat("r_y");
        player.transform.eulerAngles = new Vector3(0, yRot, 0);
    }

    // Loads the player position
    private void LoadPlayerPosition()
    {
        float xPos = PlayerPrefs.GetFloat("p_x");
        float zPos = PlayerPrefs.GetFloat("p_z");
        player.transform.position = new Vector3(xPos, 0, zPos);
    }

    // Checks to load the puzzle normally
    // Note: player rotation is set by the checkpoint manager - only load player rotation when restarting a puzzle
    private void LoadPuzzleCheck()
    {
        if (sceneName == "MainMenu" || sceneName == "TutorialMap" || gameManagerScript.isDebugging) return;

        int puzzleViewIndex = PlayerPrefs.GetInt("cameraIndex");
        float playerPosX = PlayerPrefs.GetFloat("p_x");
        float playerPosZ = PlayerPrefs.GetFloat("p_z");

        Vector3 checkpointPos = checkpoints[puzzleViewIndex].transform.position;
        Vector3 intendedPlayerPos = new Vector3(checkpointPos.x, 0, checkpointPos.z);
        Vector3 savedPlayerPos = new Vector3(playerPosX, 0, playerPosZ);

        player.transform.position = (savedPlayerPos == intendedPlayerPos) ? savedPlayerPos : Vector3.zero;
        cameraScript.PuzzleViewIndex = (savedPlayerPos == intendedPlayerPos) ? puzzleViewIndex : 0;
        OnFirstPuzzleCheck();

        //Debug.Log((savedPlayerPos == intendedPlayerPos) ? "Puzzle loaded successfully" : "Could NOT find puzzle");
    }

    // Checks to load the puzzle via debug - For Debugging Purposes ONLY
    // Note: player rotation is set by the checkpoint manager - only load player rotation when restarting a puzzle
    private void LoadPuzzleDebugCheck()
    {
        if (sceneName == "MainMenu" || !gameManagerScript.isDebugging) return;

        int puzzleNumber = gameManagerScript.puzzleNumber;
        int checkpointIndex = (puzzleNumber < 1 || puzzleNumber > checkpoints.Length) ? 0 : puzzleNumber - 1;
        Vector3 checkpointPos = checkpoints[checkpointIndex].transform.position;

        player.transform.position = new Vector3(checkpointPos.x, 0, checkpointPos.z);
        cameraScript.PuzzleViewIndex = checkpointIndex;
        OnFirstPuzzleCheck();

        Debug.Log($"Debugging: loaded puzzle {puzzleNumber}");
    }

    // Checks if the player is on the first puzzle - if the player position is zero/null
    // Note: the position of the first checkpoint should alway be (x: 0f, y: 0f, z: 0f)
    private void OnFirstPuzzleCheck()
    {
        if (player.transform.position != Vector3.zero) return;

        savedInvisibleBlock.transform.position = new Vector3(0, 1, -1);
        player.transform.position = new Vector3(0, 0, -5);
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        gameManagerScript = FindObjectOfType<GameManager>();
        cameraScript = (sceneName != "MainMenu") ? FindObjectOfType<CameraController>() : null;
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "SavedInvisibleBlock":
                    savedInvisibleBlock = child.gameObject;
                    break;
                default:
                    break;
            }

            SetVariables(child);
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        SetVariables(transform);    
        SaveSceneName();

        if (sceneName == "MainMenu") savedInvisibleBlock.SetActive(false);
        player = (sceneName != "MainMenu") ? FindObjectOfType<TileMovementController>().gameObject : null;
        checkpoints = (sceneName != "MainMenu") ? GameObject.FindGameObjectsWithTag("Checkpoint") : null;
    }

}
