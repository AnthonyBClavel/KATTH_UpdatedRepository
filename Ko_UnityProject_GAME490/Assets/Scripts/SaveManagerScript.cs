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

    private float pX;
    private float pZ;
    private float rY;

    private float bX;
    private float bY;
    private float bZ;

    // Start is called before the first frame update
    void Awake()
    {
        SaveSceneName();

        if (PlayerPrefs.GetInt("Saved") == 1 && PlayerPrefs.GetInt("TimeToLoad") == 1 && SceneManager.GetActiveScene().name != "TutorialMap")
        {
            Debug.Log("Save Loaded Successfully");          

            pX = player.transform.position.x;
            pZ = player.transform.position.z;
            rY = player.transform.eulerAngles.y;

            bX = savedInvisibleBlock.transform.position.x;
            bY = savedInvisibleBlock.transform.position.y;
            bZ = savedInvisibleBlock.transform.position.z;

            pX = PlayerPrefs.GetFloat("p_x");
            pZ = PlayerPrefs.GetFloat("p_z");
            rY = PlayerPrefs.GetFloat("r_y");

            bX = PlayerPrefs.GetFloat("b_x");
            bY = PlayerPrefs.GetFloat("b_y");
            bZ = PlayerPrefs.GetFloat("b_z");

            player.transform.position = new Vector3(pX, 0, pZ);
            player.transform.eulerAngles = new Vector3(0, rY, 0);

            savedInvisibleBlock.transform.position = new Vector3(bX, bY, bZ);

            // If a new game is created or if the PlayerPrefs is null, set the savedInvisibleBlock to its default position
            if (savedInvisibleBlock.transform.position == new Vector3(0,0,0))
                savedInvisibleBlock.transform.position = new Vector3(0, 1, -1);

            // If a new game is created or if the PlayerPrefs is null, set the player to its default position
            if (player.transform.position == new Vector3(0, 0, 0))
                player.transform.position = new Vector3(0, 0, -5);

            pixelatedCamera.GetComponent<CameraController>().currentIndex = PlayerPrefs.GetInt("cameraIndex");

            PlayerPrefs.SetInt("TimeToLoad", 0);
            PlayerPrefs.Save();
        }     

    }

    // Update is called once per frame
    void Update()
    {
        /*** For Debugging purposes ***/
        /*if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("New Game created");

            PlayerPrefs.DeleteKey("p_x");
            PlayerPrefs.DeleteKey("p_z");
            PlayerPrefs.DeleteKey("r_y");

            PlayerPrefs.DeleteKey("b_x");
            PlayerPrefs.DeleteKey("b_y");
            PlayerPrefs.DeleteKey("b_z");

            PlayerPrefs.DeleteKey("pc_x");
            PlayerPrefs.DeleteKey("pc_y");
            PlayerPrefs.DeleteKey("pc_z");
            PlayerPrefs.DeleteKey("cameraIndex");

            PlayerPrefs.DeleteKey("TimeToLoad");
            PlayerPrefs.DeleteKey("Save");
            PlayerPrefs.DeleteKey("savedScene");

            PlayerPrefs.DeleteKey("listOfArtifacts");
            PlayerPrefs.DeleteKey("numberOfArtifactsCollected");

            PlayerPrefs.SetInt("Saved", 1);
            PlayerPrefs.Save();

            //string tutorialScene = "TutorialMap";
            //PlayerPrefs.SetString("savedScene", tutorialScene);

        }
        /*** End Debugging ***/

    }

    // Saves the player's position
    public void SavePlayerPosition()
    {
        if (SceneManager.GetActiveScene().name != "TutorialMap")
        {
            PlayerPrefs.SetFloat("p_x", player.transform.position.x);
            PlayerPrefs.SetFloat("p_z", player.transform.position.z);
        }
        PlayerPrefs.SetFloat("r_y", player.transform.eulerAngles.y);
        PlayerPrefs.SetInt("Saved", 1);
        PlayerPrefs.Save();
    }

    // Loads the player's position (sets the timeToLoad to 1 so that the condition in "void Awake" can be met)
    public void LoadPlayerPosition()
    {
        PlayerPrefs.SetInt("TimeToLoad", 1);
        PlayerPrefs.Save();
    }

    // This is only for loading the player's rotation after the freezing UI
    public void LoadPlayerRotation()
    {
        rY = player.transform.eulerAngles.y;
        rY = PlayerPrefs.GetFloat("r_y");
        player.transform.eulerAngles = new Vector3(0, rY, 0);
    }

    // Loads the camera's position
    public void LoadCameraPosition()
    {
        PlayerPrefs.SetInt("TimeToLoad", 1);
        PlayerPrefs.Save();
    }

    // Loads the savedBlock's position
    public void LoadBlockPosition()
    {
        PlayerPrefs.SetInt("TimeToLoad", 1);
        PlayerPrefs.Save();
    }

    // Loads the camera's position
    public void LoadCollectedArtifacts()
    {
        PlayerPrefs.SetInt("TimeToLoad", 1);
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

    // Saves the savedBlock's position - the block that's moved to the last tile on the bridge after every puzzle
    public void SaveBlockPosition()
    {
        if (SceneManager.GetActiveScene().name != "TutorialMap")
        {
            PlayerPrefs.SetFloat("b_x", savedInvisibleBlock.transform.position.x);
            PlayerPrefs.SetFloat("b_y", savedInvisibleBlock.transform.position.y);
            PlayerPrefs.SetFloat("b_z", savedInvisibleBlock.transform.position.z);
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

    // Creates a new save file
    public void CreateNewSaveFile()
    {
        Debug.Log("Updated Save File");

        PlayerPrefs.DeleteKey("p_x");
        PlayerPrefs.DeleteKey("p_z");
        PlayerPrefs.DeleteKey("r_y");

        PlayerPrefs.DeleteKey("b_x");
        PlayerPrefs.DeleteKey("b_y");
        PlayerPrefs.DeleteKey("b_z");

        PlayerPrefs.DeleteKey("pc_x");
        PlayerPrefs.DeleteKey("pc_y");
        PlayerPrefs.DeleteKey("pc_z");
        PlayerPrefs.DeleteKey("cameraIndex");

        PlayerPrefs.DeleteKey("TimeToLoad");
        PlayerPrefs.DeleteKey("Save");
        PlayerPrefs.DeleteKey("savedScene");

        //PlayerPrefs.DeleteKey("listOfArtifacts");
        //PlayerPrefs.DeleteKey("numberOfArtifactsCollected"); // Check level manager...

        PlayerPrefs.SetInt("Saved", 1);
        PlayerPrefs.Save();

        //string tutorialScene = "TutorialMap";
        //PlayerPrefs.SetString("savedScene", tutorialScene);
    }

}
