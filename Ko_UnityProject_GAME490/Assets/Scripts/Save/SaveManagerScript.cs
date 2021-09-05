using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManagerScript : MonoBehaviour
{
    public GameObject player;
    public GameObject savedInvisibleBlock;
    public GameObject pixelatedCamera;

    private string sceneName;

    // Floats for saving the player's position
    private float pX;
    private float pZ;
    private float rY;

    [Header("Debugging Elements")]
    [Space(20)]
    public bool isDebugging;
    public int puzzleNumber;
    private Transform[] checkpoints;

    private GameManager gameManagerScript;

    // Start is called before the first frame update
    void Awake()
    {      
        SaveSceneName();
        SetCheckpoints();
        LoadAllPlayerPrefs();       

        if (PlayerPrefs.GetInt("Saved") == 1 && PlayerPrefs.GetInt("TimeToLoad") == 1 && SceneManager.GetActiveScene().name != "TutorialMap" && !isDebugging)
        {
            Debug.Log("Save Loaded Successfully");          

            pX = player.transform.position.x;
            pZ = player.transform.position.z;
            rY = player.transform.eulerAngles.y;

            pX = PlayerPrefs.GetFloat("p_x");
            pZ = PlayerPrefs.GetFloat("p_z");
            rY = PlayerPrefs.GetFloat("r_y");

            player.transform.position = new Vector3(pX, 0, pZ);
            player.transform.eulerAngles = new Vector3(0, rY, 0);

            pixelatedCamera.GetComponent<CameraController>().currentIndex = PlayerPrefs.GetInt("cameraIndex");
            OnFirstPuzzleCheck();

            PlayerPrefs.SetInt("TimeToLoad", 0);
            PlayerPrefs.Save();
        }     

        if (isDebugging)
        {
            SetPuzzleToLoad();
            OnFirstPuzzleCheck();
        }
    }

    // Update is called once per frame
    void Update()
    {
        CreateNewSaveFileCheck();
    }

    // Saves the player's position
    public void SavePlayerPosition(GameObject checkpoint)
    {
        if (SceneManager.GetActiveScene().name != "TutorialMap")
        {
            PlayerPrefs.SetFloat("p_x", checkpoint.transform.position.x);
            PlayerPrefs.SetFloat("p_z", checkpoint.transform.position.z);
            //PlayerPrefs.SetFloat("p_x", player.transform.position.x);
            //PlayerPrefs.SetFloat("p_z", player.transform.position.z);
        }

        PlayerPrefs.SetInt("Saved", 1);
        PlayerPrefs.Save();
    }

    // Saves the player's rotation
    public void SavePlayerRotation(float playerRotation)
    {
        PlayerPrefs.SetFloat("r_y", playerRotation);
        //PlayerPrefs.SetFloat("r_y", player.transform.eulerAngles.y);

        PlayerPrefs.SetInt("Saved", 1);
        PlayerPrefs.Save();
    }

    // Saves the camera's position
    public void SaveCameraPosition()
    {
        if (SceneManager.GetActiveScene().name != "TutorialMap")
        {
            PlayerPrefs.SetInt("cameraIndex", pixelatedCamera.GetComponent<CameraController>().currentIndex);

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

    // Sets the checkpoints array
    private void SetCheckpoints()
    {
        gameManagerScript = FindObjectOfType<GameManager>();
        checkpoints = gameManagerScript.checkpoints;
    }

    // Loads a puzzle - For Debugging Purposes ONLY
    private void SetPuzzleToLoad()
    {
        if (SceneManager.GetActiveScene().name != "TutorialMap")
        {
            if (puzzleNumber >= 1 && puzzleNumber <= checkpoints.Length)
            {
                Debug.Log("Loaded Debug");

                Transform checkpointTransform = checkpoints[puzzleNumber - 1];
                CameraController cameraScript = pixelatedCamera.GetComponent<CameraController>();

                // Sets the player's position to loaded checkpoint's position
                player.transform.position = new Vector3(checkpointTransform.position.x, 0, checkpointTransform.position.z);
                cameraScript.currentIndex = puzzleNumber - 1;
            }
        }     
    }

    // Checks if a new save file can be created - For Debugging Purposes ONLY
    private void CreateNewSaveFileCheck()
    {
        if (Input.GetKeyDown(KeyCode.F) && isDebugging)
        {
            Debug.Log("New Game created");

            PlayerPrefs.DeleteKey("p_x");
            PlayerPrefs.DeleteKey("p_z");
            PlayerPrefs.DeleteKey("r_y");
            PlayerPrefs.DeleteKey("cameraIndex");

            PlayerPrefs.DeleteKey("TimeToLoad");
            PlayerPrefs.DeleteKey("Save");
            PlayerPrefs.DeleteKey("savedScene");

            PlayerPrefs.DeleteKey("listOfArtifacts");
            PlayerPrefs.DeleteKey("numberOfArtifactsCollected");

            PlayerPrefs.SetInt("Saved", 1);
            PlayerPrefs.Save();
        }
    }

}
