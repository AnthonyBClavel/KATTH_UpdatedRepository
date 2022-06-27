using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    private string levelToLoad;
    private int loadingIconIndex;

    [Header("Gameplay Variables")]
    [Range(1f, 10f)]
    public float cameraSpeed = 3f; // Original Value = 3f
    [Range(0f, 1f)]
    public float playerLerpDuration = 0.2f; // Original Value = 0.2f
    [Range(0f, 1f)]
    public float crateLerpDuration = 0.1f; // Original Value = 0.1f
    [Range(0.5f, 10f)]
    public float resetPuzzleDelay = 1.5f; // Original Value = 1.5f

    [Header("Loading Screen Variables")]
    [Range(0.01f, 1f)]
    public float loadingIconSpeed = 0.08f; // Original Value = 0.8f
    public Sprite[] loadingScreenImages;
    public Sprite[] loadingIconSprites;

    [Header("Materials")]
    public Material grassMaterial;
    public Material iceMaterial;

    private GameObject savedInvisibleBlock;
    private GameObject blackLoadingScreen;
    private GameObject loadingScreen;
    private GameObject loadingScreenIcon;
    private GameObject loadingScreenTips;
    private TextMeshProUGUI loadingScreenText;
    private Slider loadingScreenBar;
    private Image loadingScreenImage;
    private IEnumerator loadingIconCoroutine;

    private TileMovementController playerScript;
    private OptionsMenu optionsMenuScript;
    private TransitionFade transitionFadeScript;
    private BlackBars blackBarsScript;
    private SaveManager saveManagerScript;
    private BlockMovementController blockMovementScript;
    private CameraController cameraScript;
    private IntroManager introManagerScript;
    private TorchMeter torchMeterScript;
    private AudioManager audioManagerScript;
    private CharacterDialogue characterDialogueScript;
    private NotificationBubbles notificationBubblesScript;

    [Header("Debugging Elements")]
    public bool canDeathScreen = false;
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
        canDeathScreen = false;
    }

    void LateUpdate()
    {
        ScriptDebuggingCheck();
    }

    // Checks for debugging across various scripts - to prevent commenting out all debugging checks from EVERY script before building game
    private void ScriptDebuggingCheck()
    {
        if (isDebugging) // Dont need this
        {
            // Debugging checks for UI animations
            blackBarsScript.DebuggingCheck(this);
            transitionFadeScript.DebuggingCheck(this);
            notificationBubblesScript.DebuggingCheck(this);
            // Other debugging checks
            cameraScript.DebuggingCheck(this);
            torchMeterScript.DebuggingCheck(this);
            SavedInvisibleBlockDebuggingCheck();
            CreateNewSaveFileCheck();
        }
    }

    /*** Debug functions START here ***/
    // Updates the lerpLength in playerScript - For Debugging Purposes Only!
    public void CheckForPlayerScriptDebug()
    {
        if (isDebugging)
        {
            if (playerScript.LerpDuration != playerLerpDuration)
                playerScript.LerpDuration = playerLerpDuration;

            if (playerScript.ResetPuzzleDelay != resetPuzzleDelay)
                playerScript.ResetPuzzleDelay = resetPuzzleDelay;
        }
        else
        {
            if (playerScript.LerpDuration != 0.2f)
                playerScript.LerpDuration = 0.2f;

            if (playerScript.ResetPuzzleDelay != 1.5f)
                playerScript.ResetPuzzleDelay = 1.5f;
        }
    }

    // Updates the lerpLength in the blockMovementScript - For Debugging Purposes Only!
    public void CheckForBlockMovementDebug(BlockMovementController script)
    {
        blockMovementScript = script;
        if (isDebugging)
        {
            if (blockMovementScript.LerpDuration != crateLerpDuration)
                blockMovementScript.LerpDuration = crateLerpDuration;
        }         
        else
        {
            if (blockMovementScript.LerpDuration != 0.1f)
                blockMovementScript.LerpDuration = 0.1f;
        }
    }

    // Updates the cameraSpeed in cameraScript - For Debugging Purposes Only!
    public void CheckForCameraScriptDebug()
    {
        if (isDebugging)
        {
            if (cameraScript.CameraSpeed != cameraSpeed)
                cameraScript.CameraSpeed = cameraSpeed;
        }
        else
        {
            if (cameraScript.CameraSpeed != 3f)
                cameraScript.CameraSpeed = 3f;
        }
    }
    /*** Debug functions END here ***/

    // Checks which scene to load next
    public void LoadNextSceneCheck()
    {
        // Returns to the main menu if the player hasn't finished the zone
        if (playerScript != null && !playerScript.HasFinishedZone)
            LoadMainMenu();
        // Loads the next sceen if the player has finsihed the zone
        else
            LoadNextScene();
    }

    // Sets and loads the main menu
    public void LoadMainMenu()
    {
        levelToLoad = "MainMenu";
        blackLoadingScreen.SetActive(true);
        PlayLoadingIconAnim(blackLoadingScreen);
        SceneManager.LoadSceneAsync(levelToLoad);
    }

    // Sets and loads the next scene
    public void LoadNextScene()
    {
        LevelToLoadCheck();
        StartCoroutine(LoadNextLevelAsync());
    }

    // Checks if the saves for the artifacts should be deleted
    public void ResetCollectedArtifactsCheck()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Creates new save file - ONLY delete artifact saves here
        if (currentScene == "FifthMap" || currentScene == "TutorialMap")
        {
            PlayerPrefs.DeleteKey("listOfArtifacts");
            PlayerPrefs.DeleteKey("numberOfArtifactsCollected");
            CreateNewSaveFile();
        }
    }

    // Sets the levelToLoad string - determined by current scene
    private void LevelToLoadCheck()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "TutorialMap")
            levelToLoad = "FirstMap";
        else if (currentScene == "FirstMap")
            levelToLoad = "SecondMap";
        else if (currentScene == "SecondMap")
            levelToLoad = "ThirdMap";
        else if (currentScene == "ThirdMap")
            levelToLoad = "FourthMap";
        else if (currentScene == "FourthMap")
            levelToLoad = "FifthMap";
        else if (currentScene == "FifthMap")
            levelToLoad = "MainMenu";
        else if (currentScene == "MainMenu")
            levelToLoad = "FirstMap";
        else
            levelToLoad = "FirstMap";

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
        if (loadingScreenImages != null)
            SetRandomSprite(loadingScreenImages[UnityEngine.Random.Range(0, loadingScreenImages.Length)]);
    }

    // Starts the corouitne to play the loading icon animation
    private void PlayLoadingIconAnim(GameObject loadingScreenObject)
    {
        if (loadingIconCoroutine != null)
            StopCoroutine(loadingIconCoroutine);

        loadingIconCoroutine = LoadingIconAnimation(loadingScreenObject);
        StartCoroutine(loadingIconCoroutine);
    }

    // Sets the next/new sprite for the loading icon at the end of each time interval
    private IEnumerator LoadingIconAnimation(GameObject loadingScreen)
    {
        for (int i = 0; i < loadingScreen.transform.childCount; i++)
        {
            GameObject child = loadingScreen.transform.GetChild(i).gameObject;
            if (child.name == "LoadingIcon")
            {
                Image loadingIconImage = child.GetComponent<Image>();
                loadingIconIndex = 0;
                loadingIconImage.sprite = loadingIconSprites[loadingIconIndex];

                while (loadingIconImage.isActiveAndEnabled)
                {
                    yield return new WaitForSeconds(loadingIconSpeed);
                    loadingIconIndex++;
                    if (loadingIconIndex > loadingIconSprites.Length - 1 || loadingIconIndex < 0)
                        loadingIconIndex = 0;
                    loadingIconImage.sprite = loadingIconSprites[loadingIconIndex];
                }
            }              
        }
    }

    // Loads the next level asynchronously as the loading screen is active 
    private IEnumerator LoadNextLevelAsync()
    {
        loadingScreen.SetActive(true);
        SetRandomLoadingScreenImage();
        PlayLoadingIconAnim(loadingScreen);

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
                    loadingScreenText.gameObject.SetActive(false);
                    loadingScreenBar.gameObject.SetActive(false);
                    loadingScreenIcon.SetActive(false);
                    loadingScreenTips.SetActive(false);

                    asyncLoad.allowSceneActivation = true;
                }
            }
            yield return null;
        }
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = FindObjectOfType<TileMovementController>();
        optionsMenuScript = FindObjectOfType<OptionsMenu>();
        transitionFadeScript = FindObjectOfType<TransitionFade>();
        saveManagerScript = FindObjectOfType<SaveManager>();
        blockMovementScript = FindObjectOfType<BlockMovementController>();
        cameraScript = FindObjectOfType<CameraController>();
        introManagerScript = FindObjectOfType<IntroManager>();
        torchMeterScript = FindObjectOfType<TorchMeter>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        blackBarsScript = FindObjectOfType<BlackBars>();
        notificationBubblesScript = FindObjectOfType<NotificationBubbles>();
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < optionsMenuScript.transform.parent.childCount; i++)
        {
            GameObject child = optionsMenuScript.transform.parent.GetChild(i).gameObject;

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
                        loadingScreenTips = child02;
                }
            }
        }

        loadingScreenImage = loadingScreen.GetComponent<Image>();
        savedInvisibleBlock = saveManagerScript.transform.GetChild(0).gameObject;
    }

    // Creates a new save file
    //[ContextMenu("Create New Save File")]
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
        if (Input.GetKeyDown(KeyCode.Backslash) && isDebugging) // \
        {
            Debug.Log("Debugging: New Game Created");

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

    // Checks when to set the saved invisible block active/inactive
    private void SavedInvisibleBlockDebuggingCheck()
    {
        if (Input.GetKeyDown(KeyCode.Backspace) && isDebugging) // <--
        {
            if (savedInvisibleBlock.activeSelf)
            {
                Debug.Log("Debugging: SavedInvisisbleBlock Is Now Inactive");
                savedInvisibleBlock.SetActive(false);
            }
            else if (!savedInvisibleBlock.activeSelf)
            {
                Debug.Log("Debugging: SavedInvisisbleBlock Is Now Active");
                savedInvisibleBlock.SetActive(true);
            }
        }
    }

}
