using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Floats")]
    [Range(0.005f, 0.1f)]
    public float typingSpeed = 0.03f;
    [Range(0.5f, 3f)]
    public float sentenceSpeed = 1f;
    [Range(1f, 10f)]
    public float cameraSpeed = 3f;
    [Range(1f, 10f)]
    public float introCameraSpeed = 3f;
    [Range(0.5f, 10f)]
    public float resetPuzzleDelay = 1.5f;
    [Range(0.5f, 10f)]
    public float fadeAudioLength = 2f;

    public float rotationWithKeys = 200f;
    public float rotationWithMouse = 500f;

    private string levelToLoad;

    [Header("Materials")]
    public Material grassMaterial;
    public Material iceMaterial;

    [Header("Arrays")]
    public Transform[] puzzleViews;
    public Transform[] checkpoints;
    public Sprite[] loadingScreenSprites;

    [Header("Loading Screen Elements")]
    private GameObject blackLoadingScreen;
    private GameObject loadingScreen;
    private GameObject loadingScreenIcon;
    private TextMeshProUGUI loadingScreenText;
    private Slider loadingScreenBar;
    private Image loadingScreenImage;

    private TileMovementController playerScript;
    private PauseMenu pauseMenuScript;
    private TipsManager tipsManagerScript;
    private TransitionFade transitionFadeScript;

    [Header("Debugging Elements")]
    public bool isDebugging;
    public int puzzleNumber;

    void Awake()
    {
        SetScripts();
        SetElements();
    }
    
    // Start is called before the first frame update
    void Start()
    {
    
    }

    void Update()
    {
        CreateNewSaveFileCheck();
    }

    // Checks which scene to load next
    public void LoadNextSceneCheck()
    {
        if (!playerScript.hasFinishedZone)
        {
            levelToLoad = "MainMenu";
            blackLoadingScreen.SetActive(true);
            SceneManager.LoadSceneAsync(levelToLoad);
        }
        if (playerScript.hasFinishedZone)
        {
            LevelToLoadCheck();
            StartCoroutine("LoadNextLevelAsync");
        }
    }

    // Disables player bools, plays chimeSFX, and triggers the fade to next level
    public void FinishedZoneCheck()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene != "FifthMap")
            transitionFadeScript.GameFadeOut();

        // Creates new save file - ONLY delete artifact saves here
        if (currentScene == "FifthMap" || currentScene == "TutorialMap")
        {
            PlayerPrefs.DeleteKey("listOfArtifacts");
            PlayerPrefs.DeleteKey("numberOfArtifactsCollected");
            CreateNewSaveFile();
        }
    }

    // Loads the next level asynchronously as the loading screen is active 
    private IEnumerator LoadNextLevelAsync()
    {
        loadingScreen.SetActive(true);
        SetRandomLoadingScreenImage();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelToLoad);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            loadingScreenBar.value = asyncLoad.progress;

            if (asyncLoad.progress >= 0.9f && !asyncLoad.allowSceneActivation)
            {
                loadingScreenText.text = "Press SPACE to Continue";
                loadingScreenIcon.SetActive(false);

                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    CreateNewSaveFile();

                    loadingScreenImage.color = Color.black;
                    tipsManagerScript.gameObject.SetActive(false);
                    loadingScreenText.gameObject.SetActive(false);
                    loadingScreenBar.gameObject.SetActive(false);
                    loadingScreenIcon.gameObject.SetActive(false);

                    asyncLoad.allowSceneActivation = true;
                }
            }
            yield return null;
        }
    }

    // Sets the levelToLoad string - determined by current scene
    private void LevelToLoadCheck()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "FirstMap")
            levelToLoad = "SecondMap";
        if (currentScene == "SecondMap")
            levelToLoad = "ThirdMap";
        if (currentScene == "ThirdMap")
            levelToLoad = "FourthMap";
        if (currentScene == "FourthMap")
            levelToLoad = "FifthMap";
        if (currentScene == "FifthMap")
            levelToLoad = "MainMenu";
    }

    // Sets a random image/sprite for the loading screen
    private void SetRandomSprite(Sprite newLoadingScreenImg)
    {
        if (loadingScreenImage.sprite.name == newLoadingScreenImg.name)
            return;
        else
            loadingScreenImage.sprite = newLoadingScreenImg;
    }

    // Sets the image/sprite for the loading screen by randomly selecting one from an array
    private void SetRandomLoadingScreenImage()
    {
        if (loadingScreenSprites != null)
            SetRandomSprite(loadingScreenSprites[UnityEngine.Random.Range(0, loadingScreenSprites.Length)]);
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        transitionFadeScript = FindObjectOfType<TransitionFade>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < pauseMenuScript.transform.childCount; i++)
        {
            GameObject child = pauseMenuScript.transform.GetChild(i).gameObject;

            if (child.name == "BlackLoadingScreen")
                blackLoadingScreen = child;

            if (child.name == "LoadingScreen")
            {
                loadingScreen = child;

                for (int j = 0; j < loadingScreen.transform.childCount; j++)
                {
                    GameObject child02 = loadingScreen.transform.GetChild(j).gameObject;

                    if (child02.name == "LoadingIcon")
                        loadingScreenIcon = child02;
                    if (child02.name == "LoadingText")
                        loadingScreenText = child02.GetComponent<TextMeshProUGUI>();
                    if (child02.name == "LoadingBar")
                        loadingScreenBar = child02.GetComponent<Slider>();
                    if (child02.name == "Tips")
                        tipsManagerScript = child02.GetComponent<TipsManager>();
                }
            }
        }

        loadingScreenImage = loadingScreen.GetComponent<Image>();
    }

    // Creates a new save file
    private void CreateNewSaveFile()
    {
        Debug.Log("Updated Save File");

        PlayerPrefs.DeleteKey("p_x");
        PlayerPrefs.DeleteKey("p_z");
        PlayerPrefs.DeleteKey("r_y");
        PlayerPrefs.DeleteKey("cameraIndex");

        PlayerPrefs.DeleteKey("TimeToLoad");
        PlayerPrefs.DeleteKey("Save");
        PlayerPrefs.DeleteKey("savedScene");

        PlayerPrefs.SetInt("Saved", 1);
        PlayerPrefs.Save();
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
