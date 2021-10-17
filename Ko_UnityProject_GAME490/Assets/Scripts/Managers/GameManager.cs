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
    [Range(1f, 10f)]
    public float cameraSpeed = 3f;
    [Range(1f, 10f)]
    public float introCameraSpeed = 3f;
    [Range(0f, 1f)]
    public float playerLerpLength = 0.2f;
    [Range(0f, 1f)]
    public float crateLerpLength = 0.1f;
    [Range(0.5f, 10f)]
    public float resetPuzzleDelay = 1.5f;
    [Range(0.5f, 10f)]
    public float fadeAudioLength = 2f;

    public float rotateWithKeysSpeed = 200f;
    public float rotateWithMouseSpeed = 500f;

    private string levelToLoad;

    [Header("Bools")]
    public bool canDeathScreen = false;

    [Header("Materials")]
    public Material grassMaterial;
    public Material iceMaterial;

    [Header("Prefabs")]
    public GameObject destroyedRockParticle;
    public GameObject treeHitParticle;
    public GameObject snowTreeHitParticle;
    private GameObject savedInvisibleBlock;

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
    private BlackBars blackBarsScript;
    private SaveManager saveManagerScript;
    private BlockMovementController blockMovementScript;
    private CameraController cameraScript;
    private IntroManager introManagerScript;
    private TorchMeter torchMeterScript;
    private AudioManager audioManagerScript;
    private CharacterDialogue characterDialogueScript;
    private DialogueArrow dialogueArrowScript;
    private NotificationBubbles notificationBubblesScript;

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
        canDeathScreen = false;
    }

    // Update is called once per frame
    /*void Update()
    {

    }*/

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
            transitionFadeScript.DebuggingCheck(this);
            blackBarsScript.DebuggingCheck(this);
            dialogueArrowScript.DebuggingCheck(this);
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
            if (playerScript.LerpLength != playerLerpLength)
                playerScript.LerpLength = playerLerpLength;

            if (playerScript.ResetPuzzleDelay != resetPuzzleDelay)
                playerScript.ResetPuzzleDelay = resetPuzzleDelay;
        }
        else
        {
            if (playerScript.LerpLength != 0.2f)
                playerScript.LerpLength = 0.2f;

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
            if (blockMovementScript.LerpLength != crateLerpLength)
                blockMovementScript.LerpLength = crateLerpLength;
        }         
        else
        {
            if (blockMovementScript.LerpLength != 0.1f)
                blockMovementScript.LerpLength = 0.1f;
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

    // Updates the intoCameraSpeed and typingSpeed in introManagerScript - For Debugging Purposes Only!
    public void CheckForIntroManagerDebug()
    {
        if (isDebugging)
        {
            if (introManagerScript.IntroCameraSpeed != introCameraSpeed)
                introManagerScript.IntroCameraSpeed = introCameraSpeed;

            if (introManagerScript.TypingSpeed != typingSpeed)
                introManagerScript.TypingSpeed = typingSpeed;
        }
        else
        {
            if (introManagerScript.IntroCameraSpeed != 3f)
                introManagerScript.IntroCameraSpeed = 3f;

            if (introManagerScript.TypingSpeed != 0.03f)
                introManagerScript.TypingSpeed = 0.03f;
        }
    }

    // Updates the intoCameraSpeed and typingSpeed in introManagerScript - For Debugging Purposes Only!
    public void CheckForAudioManagerDebug()
    {
        if (isDebugging)
        {
            if (audioManagerScript.FadeAudioLength != fadeAudioLength)
                audioManagerScript.FadeAudioLength = fadeAudioLength;
        }
        else
        {
            if (audioManagerScript.FadeAudioLength != 2f)
                audioManagerScript.FadeAudioLength = 2f;
        }
    }

    // Updates the typingSpeed in characterDialogueScript - For Debugging Purposes Only!
    public void CheckForCharacterDialogueDebug()
    {
        if (isDebugging)
        {
            if (characterDialogueScript.TypingSpeed != typingSpeed)
                characterDialogueScript.TypingSpeed = typingSpeed;
        }
        else
        {
            if (characterDialogueScript.TypingSpeed != 0.03f)
                characterDialogueScript.TypingSpeed = 0.03f;
        }
    }
    /*** Debug functions END here ***/

    // Checks which scene to load next
    public void LoadNextSceneCheck()
    {
        if (!playerScript.HasFinishedZone)
        {
            levelToLoad = "MainMenu";
            blackLoadingScreen.SetActive(true);
            SceneManager.LoadSceneAsync(levelToLoad);
        }
        else
        {
            LevelToLoadCheck();
            StartCoroutine(LoadNextLevelAsync());
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
        saveManagerScript = FindObjectOfType<SaveManager>();
        blockMovementScript = FindObjectOfType<BlockMovementController>();
        cameraScript = FindObjectOfType<CameraController>();
        introManagerScript = FindObjectOfType<IntroManager>();
        torchMeterScript = FindObjectOfType<TorchMeter>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        characterDialogueScript = FindObjectOfType<CharacterDialogue>();
        blackBarsScript = FindObjectOfType<BlackBars>();
        dialogueArrowScript = FindObjectOfType<DialogueArrow>();
        notificationBubblesScript = FindObjectOfType<NotificationBubbles>();
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
        savedInvisibleBlock = saveManagerScript.transform.GetChild(0).gameObject;
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
