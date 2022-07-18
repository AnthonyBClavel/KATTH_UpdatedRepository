using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    static readonly string tutorialZone = "TutorialMap";
    static readonly string finalZone = "FifthMap";
    static readonly string mainMenu = "MainMenu";
    private string sceneName;

    private GameObject savedInvisibleBlock;
    private GameObject[] checkpoints;
    private GameObject player;

    private TileMovementController playerScript;
    private CameraController cameraScript;
    private GameManager gameManagerScript;

    // Awake is called before Start()
    void Awake()
    {
        sceneName = SceneManager.GetActiveScene().name;
        SetScripts();
        SetElements();

        UpdateCurrentSaveFile();
        LoadPuzzleDebugCheck();
        LoadPuzzleCheck();
        SaveSceneName();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (sceneName != mainMenu) SetHasOpenedGame();
    }

    // OnApplicationQuit is called before the application quits
    void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("hasOpenedGame");
    }

    // Checks to update the current save file
    private void UpdateCurrentSaveFile()
    {
        string savedScene = PlayerPrefs.GetString("savedScene");
        if (sceneName == tutorialZone)
        {
            PlayerPrefs.DeleteKey("numberOfArtifactsCollected");
            PlayerPrefs.DeleteKey("listOfArtifacts");
        }

        if (sceneName == mainMenu || sceneName == savedScene || gameManagerScript.isDebugging) return;
        PlayerPrefs.DeleteKey("cameraIndex");
        PlayerPrefs.DeleteKey("savedScene");
        PlayerPrefs.DeleteKey("p_x");
        PlayerPrefs.DeleteKey("p_z");
        PlayerPrefs.DeleteKey("r_y");
        PlayerPrefs.Save();
    }

    // Deletes all of the player pref keys
    [ContextMenu("Delete All Player Prefs")]
    public void DeleteAllPlayerPrefs()
    {
        PlayerPrefs.DeleteKey("numberOfArtifactsCollected");
        PlayerPrefs.DeleteKey("listOfArtifacts");

        PlayerPrefs.DeleteKey("hasOpenedGame");
        PlayerPrefs.DeleteKey("cameraIndex");
        PlayerPrefs.DeleteKey("savedScene");
        PlayerPrefs.DeleteKey("p_x");
        PlayerPrefs.DeleteKey("p_z");
        PlayerPrefs.DeleteKey("r_y");
        PlayerPrefs.Save();
    }

    // Saves the player position
    public void SavePlayerPosition(GameObject checkpoint)
    {
        if (sceneName == tutorialZone) return;

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
        if (sceneName == tutorialZone) return;

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
    // Note: be sure to call this after UpdateCurrentSaveFile() in Awake()!
    private void SaveSceneName()
    {     
        if (sceneName == mainMenu) return;

        PlayerPrefs.SetString("savedScene", sceneName);
        PlayerPrefs.Save();
    }

    // Checks if the game has been opened
    public void SetHasOpenedGame()
    {
        PlayerPrefs.SetInt("hasOpenedGame", 1);
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

    // Checks for the puzzle to load
    // Note: player rotation is set by the checkpoint manager - only load player rotation when restarting a puzzle
    private void LoadPuzzleCheck()
    {
        if (sceneName == mainMenu || sceneName == tutorialZone || gameManagerScript.isDebugging) return;

        int puzzleViewIndex = PlayerPrefs.GetInt("cameraIndex");
        float playerPosX = PlayerPrefs.GetFloat("p_x");
        float playerPosZ = PlayerPrefs.GetFloat("p_z");

        Vector3 checkpointPos = checkpoints[puzzleViewIndex].transform.position;
        Vector3 savedCheckpointPos = new Vector3(checkpointPos.x, 0, checkpointPos.z);
        Vector3 savedPlayerPos = new Vector3(playerPosX, 0, playerPosZ);

        player.transform.position = savedPlayerPos == savedCheckpointPos ? savedPlayerPos : savedCheckpointPos;
        cameraScript.PuzzleViewIndex = puzzleViewIndex;
        OnFirstPuzzleCheck();
    }

    // Checks for the puzzle to load via debug - For Debugging Purposes ONLY
    // Note: player rotation is set by the checkpoint manager - only load player rotation when restarting a puzzle
    private void LoadPuzzleDebugCheck()
    {
        if (sceneName == mainMenu || !gameManagerScript.isDebugging) return;

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

    // Checks if the player has completed the tutorial - deletes artifact player prefs if so
    public void HasCompletedTutorialCheck()
    {
        if (sceneName != tutorialZone || !playerScript.HasFinishedZone) return;

        PlayerPrefs.DeleteKey("numberOfArtifactsCollected");
        PlayerPrefs.DeleteKey("listOfArtifacts");
        //Debug.Log("You have completed the tutorial!");
    }

    // Checks if the player has completed the game - deletes all player prefs if so
    public void HasCompletedGameCheck()
    {
        if (sceneName != finalZone || !playerScript.HasFinishedZone) return;

        DeleteAllPlayerPrefs();
        //Debug.Log("You have completed the game!");
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = (sceneName != mainMenu) ? FindObjectOfType<TileMovementController>() : null;
        cameraScript = (sceneName != mainMenu) ? FindObjectOfType<CameraController>() : null;
        gameManagerScript = FindObjectOfType<GameManager>();
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

        if (sceneName == mainMenu) savedInvisibleBlock.SetActive(false);
        player = (sceneName != mainMenu) ? FindObjectOfType<TileMovementController>().gameObject : null;
        checkpoints = (sceneName != mainMenu) ? GameObject.FindGameObjectsWithTag("Checkpoint") : null;
    }

}
