using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    private string sceneName;

    private float playerPosX;
    private float playerPosZ;
    private float playerRotY;

    private GameObject player;
    private GameObject savedInvisibleBlock;
    private GameObject[] checkpoints;

    private CameraController cameraScript;
    private GameManager gameManagerScript;

    // Awake is called before Start()
    void Awake()
    {      
        SetScripts();
        SetElements();

        if (!gameManagerScript.isDebugging && sceneName != "TutorialMap")
        {
            cameraScript.PuzzleViewIndex = PlayerPrefs.GetInt("cameraIndex");
            LoadPlayerPosition();
            LoadPlayerRotation();
            OnFirstPuzzleCheck();

            Debug.Log("Save loaded successfully");
        }
        else if (gameManagerScript.isDebugging)
        {
            SetPuzzleToLoad();
            OnFirstPuzzleCheck();
        }
    }

    // Saves the player's position
    public void SavePlayerPosition(GameObject checkpoint)
    {
        if (SceneManager.GetActiveScene().name != "TutorialMap")
        {
            Vector3 checkpointPosition = checkpoint.transform.position;

            PlayerPrefs.SetFloat("p_x", checkpointPosition.x);
            PlayerPrefs.SetFloat("p_z", checkpointPosition.z);
        }
        PlayerPrefs.Save();
    }

    // Saves the player's rotation
    public void SavePlayerRotation(float playerRotation)
    {
        PlayerPrefs.SetFloat("r_y", playerRotation);
        PlayerPrefs.Save();
    }

    // Saves the camera's position
    public void SaveCameraPosition()
    {
        if (sceneName != "TutorialMap")
        {
            PlayerPrefs.SetInt("cameraIndex", cameraScript.PuzzleViewIndex);
            PlayerPrefs.Save();
        }
    }

    // Saves the name of the collected artifact - adds the name to a saved string
    public void SaveCollectedArtifact(string collectedArtifacts)
    {
        PlayerPrefs.SetString("listOfArtifacts", collectedArtifacts);
        PlayerPrefs.Save();
    }

    // Saves the amount of collected artifacts 
    public void SaveNumberOfArtifactsCollected(int numberOfArtifactsCollected)
    {
        PlayerPrefs.SetInt("numberOfArtifactsCollected", numberOfArtifactsCollected);
        PlayerPrefs.Save();
    }

    // Saves the name of the current scene
    private void SaveSceneName()
    {
        sceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("savedScene", sceneName);
        PlayerPrefs.Save();
    }

    // Loads the player's initial rotation - for restarting/failing a puzzle
    public void LoadPlayerRotation()
    {
        playerRotY = PlayerPrefs.GetFloat("r_y");
        player.transform.eulerAngles = new Vector3(0, playerRotY, 0);
    }

    // Loads the player's position
    private void LoadPlayerPosition()
    {
        playerPosX = PlayerPrefs.GetFloat("p_x");
        playerPosZ = PlayerPrefs.GetFloat("p_z");
        player.transform.position = new Vector3(playerPosX, 0, playerPosZ);
    }

    // Sets the puzzle to loads - For Debugging Purposes ONLY
    private void SetPuzzleToLoad()
    {
        int puzzleNumber = gameManagerScript.puzzleNumber;

        if (puzzleNumber > 0 && puzzleNumber <= checkpoints.Length)
        {
            // Sets the player's position to the loaded checkpoint's position
            Vector3 checkpointPosition = checkpoints[puzzleNumber - 1].transform.position;
            player.transform.position = new Vector3(checkpointPosition.x, 0, checkpointPosition.z);

            cameraScript.PuzzleViewIndex = puzzleNumber - 1;
            LoadPlayerRotation();

            Debug.Log("Debugging: Loaded Puzzle " + puzzleNumber);
        }
    }

    // Checks if the player is on the first puzzle (if a new save file was created, or if a PlayerPrefs is null)
    private void OnFirstPuzzleCheck()
    {
        // Set the savedInvisibleBlock to its default position
        if (savedInvisibleBlock.transform.position == new Vector3(0, 0, 0))
            savedInvisibleBlock.transform.position = new Vector3(0, 1, -1);

        // Set the player to its default position (at the end of the entry bridge)
        if (player.transform.position == new Vector3(0, 0, 0))
            player.transform.position = new Vector3(0, 0, -5);
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        gameManagerScript = FindObjectOfType<GameManager>();
        cameraScript = FindObjectOfType<CameraController>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets them by looking at the names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "SavedInvisibleBlock")
                savedInvisibleBlock = child;
        }

        player = FindObjectOfType<TileMovementController>().gameObject;
        checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
        SaveSceneName();
    }

}
