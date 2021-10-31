using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    private GameObject player;
    private GameObject savedInvisibleBlock;
    private GameObject[] checkpoints;
    private string sceneName;

    // Floats for saving the player's position
    private float pX;
    private float pZ;
    private float rY;

    private CameraController cameraScript;
    private GameManager gameManagerScript;

    // Start is called before the first frame update
    void Awake()
    {      
        SaveSceneName();
        SetElements();
        LoadAllPlayerPrefs();

        if (PlayerPrefs.GetInt("Saved") == 1 && PlayerPrefs.GetInt("TimeToLoad") == 1 && SceneManager.GetActiveScene().name != "TutorialMap" && !gameManagerScript.isDebugging)
        {
            Debug.Log("Save Loaded Successfully");          

            LoadPlayerPosition();
            LoadPlayerRotation();
            cameraScript.CameraIndex = PlayerPrefs.GetInt("cameraIndex");
            cameraScript.SetCameraPosition();
            OnFirstPuzzleCheck();

            PlayerPrefs.SetInt("TimeToLoad", 0);
            PlayerPrefs.Save();
        }     
        if (gameManagerScript.isDebugging)
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
            PlayerPrefs.SetFloat("p_x", checkpoint.transform.position.x);
            PlayerPrefs.SetFloat("p_z", checkpoint.transform.position.z);
        }

        PlayerPrefs.SetInt("Saved", 1);
        PlayerPrefs.Save();
    }

    // Saves the player's rotation
    public void SavePlayerRotation(float playerRotation)
    {
        PlayerPrefs.SetFloat("r_y", playerRotation);
        PlayerPrefs.SetInt("Saved", 1);
        PlayerPrefs.Save();
    }

    // Saves the camera's position
    public void SaveCameraPosition()
    {
        if (SceneManager.GetActiveScene().name != "TutorialMap")
        {
            PlayerPrefs.SetInt("cameraIndex", cameraScript.CameraIndex);
            PlayerPrefs.SetInt("Saved", 1);
            PlayerPrefs.Save();
        }
    }

    // Saves the name of the scene
    public void SaveSceneName()
    {
        sceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("savedScene", sceneName);
        PlayerPrefs.SetInt("Saved", 1);
        PlayerPrefs.Save();
    }

    // Saves the name of the collected artifact - name is added to string
    public void SaveCollectedArtifact(string collectedArtifacts)
    {
        PlayerPrefs.SetString("listOfArtifacts", collectedArtifacts);
        PlayerPrefs.SetInt("Saved", 1);
        PlayerPrefs.Save();
    }

    // Saves the amount of artifacts that have been collected
    public void SaveNumberOfArtifactsCollected(int numberOfArtifactsCollected)
    {
        PlayerPrefs.SetInt("numberOfArtifactsCollected", numberOfArtifactsCollected);
        PlayerPrefs.SetInt("Saved", 1);
        PlayerPrefs.Save();
    }

    // Loads the player's initial rotation for when a puzzle is restarted/failed - the rotation the player was in after stepping on the first block of a puzzle
    public void LoadPlayerRotation()
    {
        rY = player.transform.eulerAngles.y;
        rY = PlayerPrefs.GetFloat("r_y");
        player.transform.eulerAngles = new Vector3(0, rY, 0);
    }

    // Loads the player's position
    private void LoadPlayerPosition()
    {
        pX = player.transform.position.x;
        pZ = player.transform.position.z;

        pX = PlayerPrefs.GetFloat("p_x");
        pZ = PlayerPrefs.GetFloat("p_z");

        player.transform.position = new Vector3(pX, 0, pZ);
    }

    // Loads ALL of the saved values within the PlayerPrefs
    private void LoadAllPlayerPrefs()
    {
        PlayerPrefs.SetInt("TimeToLoad", 1);
        PlayerPrefs.Save();
    }

    // Checks if the player is on the first puzzle - if a new save file has been created
    private void OnFirstPuzzleCheck()
    {
        // If a new game is created or if the PlayerPrefs is null, set the savedInvisibleBlock to its default position
        if (savedInvisibleBlock.transform.position == new Vector3(0, 0, 0))
            savedInvisibleBlock.transform.position = new Vector3(0, 1, -1);

        // If a new game is created or if the PlayerPrefs is null, set the player to its default position
        if (player.transform.position == new Vector3(0, 0, 0))
            player.transform.position = new Vector3(0, 0, -5);
    }

    // Loads a puzzle - For Debugging Purposes ONLY
    private void SetPuzzleToLoad()
    {
        if (SceneManager.GetActiveScene().name != "TutorialMap")
        {
            int puzzleNumber = gameManagerScript.puzzleNumber;

            if (puzzleNumber >= 1 && puzzleNumber <= checkpoints.Length)
            {
                Debug.Log("Debugging: Loaded Puzzle " + puzzleNumber);
                Vector3 checkpointPosition = checkpoints[puzzleNumber - 1].transform.position;

                // Sets the player's position to loaded checkpoint's position
                player.transform.position = new Vector3(checkpointPosition.x, 0, checkpointPosition.z);
                cameraScript.CameraIndex = puzzleNumber - 1;
                cameraScript.SetCameraPosition();
                LoadPlayerRotation();
            }
        }     
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "SavedInvisibleBlock")
                savedInvisibleBlock = child;
        }

        gameManagerScript = FindObjectOfType<GameManager>();
        cameraScript = FindObjectOfType<CameraController>();
        player = FindObjectOfType<TileMovementController>().gameObject;

        checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
    }

}
