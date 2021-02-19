using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManagerScript : MonoBehaviour
{
    public GameObject player;
    public GameObject pixelatedCamera;

    private string sceneName;

    private float pX;
    private float pZ;
    private float rY;

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

            pX = PlayerPrefs.GetFloat("p_x");
            pZ = PlayerPrefs.GetFloat("p_z");
            rY = PlayerPrefs.GetFloat("r_y");

            player.transform.position = new Vector3(pX, 0, pZ);
            player.transform.eulerAngles = new Vector3(0, rY, 0);

            pixelatedCamera.GetComponent<CameraController>().currentIndex = PlayerPrefs.GetInt("cameraIndex");

            PlayerPrefs.SetInt("TimeToLoad", 0);
            PlayerPrefs.Save();
        }     

    }

    // Update is called once per frame
    void Update()
    {
        /*** For Debugging purposes ***/
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("New Game created");

            PlayerPrefs.DeleteKey("p_x");
            PlayerPrefs.DeleteKey("p_z");
            PlayerPrefs.DeleteKey("r_y");

            PlayerPrefs.DeleteKey("pc_x");
            PlayerPrefs.DeleteKey("pc_y");
            PlayerPrefs.DeleteKey("pc_z");
            PlayerPrefs.DeleteKey("cameraIndex");

            PlayerPrefs.DeleteKey("TimeToLoad");
            PlayerPrefs.DeleteKey("Save");
            //PlayerPrefs.DeleteKey("savedScene");

            string tutorialScene = "TutorialMap";
            PlayerPrefs.SetString("savedScene", tutorialScene);

        }
        /*** End Debugging ***/

    }

    public void SavePlayerPosition()
    {
        if(SceneManager.GetActiveScene().name != "TutorialMap")
        {
            PlayerPrefs.SetFloat("p_x", player.transform.position.x);
            PlayerPrefs.SetFloat("p_z", player.transform.position.z);
            PlayerPrefs.SetFloat("r_y", player.transform.eulerAngles.y);
            PlayerPrefs.SetInt("Saved", 1);
            PlayerPrefs.Save();
        }
        
    }

    public void LoadPlayerPosition()
    {
        PlayerPrefs.SetInt("TimeToLoad", 1);
        PlayerPrefs.Save();
    }

    public void SaveCameraPosition()
    {
        if(SceneManager.GetActiveScene().name != "TutorialMap")
        {
            PlayerPrefs.SetInt("cameraIndex", pixelatedCamera.GetComponent<CameraController>().currentIndex);

            PlayerPrefs.SetInt("Saved", 1);
            PlayerPrefs.Save();
        }
    }

    public void LoadCameraPosition()
    {
        PlayerPrefs.SetInt("TimeToLoad", 1);
        PlayerPrefs.Save();
    }

    public void SaveSceneName()
    {
        sceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("savedScene", sceneName);
        PlayerPrefs.SetInt("Saved", 1);
        PlayerPrefs.Save();
    }

    public void CreateNewSaveFile()
    {
        Debug.Log("Updated Save File");

        PlayerPrefs.DeleteKey("p_x");
        PlayerPrefs.DeleteKey("p_z");
        PlayerPrefs.DeleteKey("r_y");

        PlayerPrefs.DeleteKey("pc_x");
        PlayerPrefs.DeleteKey("pc_y");
        PlayerPrefs.DeleteKey("pc_z");
        PlayerPrefs.DeleteKey("cameraIndex");

        PlayerPrefs.DeleteKey("TimeToLoad");
        PlayerPrefs.DeleteKey("Save");
        //PlayerPrefs.DeleteKey("savedScene");

        string tutorialScene = "TutorialMap";
        PlayerPrefs.SetString("savedScene", tutorialScene);
    }

}
