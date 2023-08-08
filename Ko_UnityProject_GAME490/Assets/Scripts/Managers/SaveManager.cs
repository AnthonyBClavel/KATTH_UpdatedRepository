using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Linq;

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

    public Vector3 SavedPlayerPosition
    {
        get { return ReturnSavedPlayerPosition(); }
        set { SetSavedPlayerPosition(value); }
    }

    public Vector3 SavedPlayerRotation
    {
        get { return ReturnSavedPlayerRotation(); }
        set { SetSavedPlayerRotation(value); }
    }

    public int CameraIndex
    {
        get { return PlayerPrefs.GetInt("cameraIndex"); }
        set { SetCameraIndex(value); }
    }

    public string ArtifactsCollected
    {
        get { return PlayerPrefs.GetString("artifactsCollected"); }
        set { SetArtifactsCollected(value); }
    }

    public int ArtifactCount
    {
        get { return PlayerPrefs.GetInt("artifactCount"); }
        set { SetArtifactCount(value); }
    }
    public string SavedScene
    {
        get { return PlayerPrefs.GetString("savedScene"); }
    }

    public int HasOpenedGame
    {
        get { return PlayerPrefs.GetInt("hasOpenedGame"); }
    }

    // Awake is called before Start()
    void Awake()
    {
        sceneName = SceneManager.GetActiveScene().name;
        SetScripts();
        SetElements();

        UpdateCurrentSaveFile();
        LoadPuzzleCheck();
        SetSavedScene();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetHasOpenedGame();
    }

    // OnApplicationQuit is called before the application quits
    void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("hasOpenedGame");
    }

    // Loads the player rotation
    public void LoadPlayerRotation() => player.transform.eulerAngles = SavedPlayerRotation;

    // Loads the player position
    public void LoadPlayerPosition() => player.transform.position = SavedPlayerPosition;

    // Checks to update the current save file
    private void UpdateCurrentSaveFile()
    {
        if (sceneName == tutorialZone)
        {
            PlayerPrefs.DeleteKey("artifactsCollected");
            PlayerPrefs.DeleteKey("artifactCount");
        }

        if (sceneName == mainMenu || sceneName == SavedScene || gameManagerScript.isDebugging) return;

        PlayerPrefs.DeleteKey("cameraIndex");
        PlayerPrefs.DeleteKey("savedScene");
        PlayerPrefs.DeleteKey("p_x");
        PlayerPrefs.DeleteKey("p_y");
        PlayerPrefs.DeleteKey("p_z");
        PlayerPrefs.DeleteKey("r_y");
        PlayerPrefs.Save();
    }

    // Deletes all of the player pref keys
    [ContextMenu("Delete All Player Prefs")]
    public void DeleteAllPlayerPrefs()
    {
        PlayerPrefs.DeleteKey("artifactsCollected");
        PlayerPrefs.DeleteKey("artifactCount");

        PlayerPrefs.DeleteKey("hasOpenedGame");
        PlayerPrefs.DeleteKey("cameraIndex");
        PlayerPrefs.DeleteKey("savedScene");
        PlayerPrefs.DeleteKey("p_x");
        PlayerPrefs.DeleteKey("p_y");
        PlayerPrefs.DeleteKey("p_z");
        PlayerPrefs.DeleteKey("r_y");
        PlayerPrefs.Save();
    }

    // Sets the artifcatsCollected PlayerPref
    private void SetArtifactsCollected(string value)
    {
        PlayerPrefs.SetString("artifactsCollected", value);
        PlayerPrefs.Save();
    }

    // Sets the artifcatCount PlayerPref
    private void SetArtifactCount(int value)
    {
        PlayerPrefs.SetInt("artifactCount", value);
        PlayerPrefs.Save();
    }

    // Checks to set the cameraIndex PlayerPref
    private void SetCameraIndex(int value)
    {
        if (sceneName == tutorialZone) return;

        PlayerPrefs.SetInt("cameraIndex", value);
        PlayerPrefs.Save();
    }

    // Checks to set the savedScene PlayerPref
    // Note: be sure to call this after UpdateCurrentSaveFile() in Awake()!
    private void SetSavedScene()
    {
        if (sceneName == mainMenu) return;

        PlayerPrefs.SetString("savedScene", sceneName);
        PlayerPrefs.Save();
    }

    // Checks to set the hasOpenedGame PlayerPref
    private void SetHasOpenedGame()
    {
        if (sceneName == mainMenu) return;

        PlayerPrefs.SetInt("hasOpenedGame", 1);
        PlayerPrefs.Save();
    }

    // Sets the player's saved rotation
    private void SetSavedPlayerRotation(Vector3 value)
    {
        PlayerPrefs.SetFloat("r_y", value.y);
        PlayerPrefs.Save();
    }

    // Checks to set the player's saved position
    private void SetSavedPlayerPosition(Vector3 value)
    {
        if (sceneName == tutorialZone) return;

        PlayerPrefs.SetFloat("p_x", value.x);
        PlayerPrefs.SetFloat("p_y", value.y);
        PlayerPrefs.SetFloat("p_z", value.z);
        PlayerPrefs.Save();
    }

    // Returns the player's saved position
    private Vector3 ReturnSavedPlayerPosition()
    {
        float playerPosX = PlayerPrefs.GetFloat("p_x");
        float playerPosY = PlayerPrefs.GetFloat("p_y");
        float playerPosZ = PlayerPrefs.GetFloat("p_z");

        return new Vector3(playerPosX, playerPosY, playerPosZ);
    }

    // Returns the player's saved rotation
    private Vector3 ReturnSavedPlayerRotation()
    {
        float playerRotY = PlayerPrefs.GetFloat("r_y");
        return new Vector3(0, playerRotY, 0);
    }

    // Returns the intended camera index - For Debugging Purposes ONLY
    private int CameraIndexDebug()
    {
        int cameraIndex = gameManagerScript.puzzleNumber - 1;

        if (cameraIndex < 0 || cameraIndex > checkpoints.Length - 1) 
            return 0;

        return cameraIndex;
    }

    // Checks to load the appropriate puzzle
    // Note: the player rotation is set by the checkpoint manager
    private void LoadPuzzleCheck()
    {
        if (sceneName == mainMenu || sceneName == tutorialZone) return;

        int puzzleViewIndex = gameManagerScript.isDebugging ? CameraIndexDebug() : CameraIndex;
        Vector3 checkpointPos = checkpoints[puzzleViewIndex].transform.position;

        cameraScript.PuzzleViewIndex = puzzleViewIndex;
        player.transform.position = checkpointPos;

        SavedPlayerPosition = checkpointPos;
        CameraIndex = puzzleViewIndex;

        OnFirstPuzzleCheck();
    }

    // Checks if the player is on the first puzzle - if the player position is zero/null
    // Note: the position of the first checkpoint should alway be (x: 0f, y: 0f, z: 0f)
    private void OnFirstPuzzleCheck()
    {
        if (player.transform.position != Vector3.zero) return;

        savedInvisibleBlock.transform.position = new Vector3(0, 0, -1);
        player.transform.position = new Vector3(0, 0, -5);
    }

    // Checks if the player has completed the tutorial - deletes artifact player prefs if so
    public void HasCompletedTutorialCheck()
    {
        if (sceneName != tutorialZone || !playerScript.HasFinishedZone) return;

        PlayerPrefs.DeleteKey("artifactsCollected");
        PlayerPrefs.DeleteKey("artifactCount");
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
        checkpoints = (sceneName != mainMenu) ? GameObject.FindGameObjectsWithTag("Checkpoint").OrderBy(x => x.transform.parent.parent.name).ToArray() : null;

        //if (sceneName != mainMenu) Debug.Log($"Save Manager: order of the checkpoints/puzzles found: {checkpoints.ConvertGameObjectArrayToString()}");
    }

}
