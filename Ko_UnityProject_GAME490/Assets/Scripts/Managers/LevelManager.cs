using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [SerializeField] [Range(0.01f, 1f)]
    private float loadingIconSpeed = 0.08f; // Original Value = 0.08f
    public Sprite[] loadingIconSprites;
    public Sprite[] loadingScreenImages;

    private string messageOnLoaded = "Press SPACE to continue";
    private string mainMenu = "MainMenu";
    private string levelToLoad;
    private string sceneName;

    private GameObject loadingScreenTips;
    private GameObject loadingScreen;

    private TextMeshProUGUI loadingScreenText;
    private Image loadingScreenImage;
    private Image loadingScreenIcon;
    private Slider loadingScreenBar;

    private IEnumerator loadingIconCoroutine;
    private TileMovementController playerScript;
    private BlackOverlay blackOverlayScript;
    private SaveManager saveManagerScript;

    // Awake is called before Start()
    void Awake()
    {
        sceneName = SceneManager.GetActiveScene().name;

        SetScripts();
        SetElements();
    }

    // Loads the next appropriate scene
    // Note: the string will always be null if the parameter is not set
    public void LoadNextScene(string nextScene = null)
    {
        levelToLoad = nextScene ?? Nextlevel();
        StartCoroutine(LoadNextLevelAsync());
    }

    // Checks to set a random image/sprite for the loading screen
    private void SetLoadingScreenImage()
    {
        if (loadingScreenImages == null || loadingScreenImages.Length <= 1) return;

        loadingScreenImage.sprite = loadingScreenImages[Random.Range(0, loadingScreenImages.Length)];
    }

    // Checks if the main menu is next level to load - returns true if so, false otherwise
    private bool IsLoadingMainMenu()
    {
        if (levelToLoad != mainMenu) return false;

        loadingScreenBar.gameObject.SetActive(false);
        loadingScreenImage.color = Color.black;
        loadingScreenTips.SetActive(false);
        return true;
    }

    // Returns the name of the next level to load
    // Note: Returns the main menu if the player hasn't finished the zone
    private string Nextlevel()
    {
        if (playerScript != null && !playerScript.HasFinishedZone)
            return "MainMenu";

        switch (sceneName)
        {
            case "TutorialMap":
                return "FirstMap";
            case "FirstMap":
                return "SecondMap";
            case "SecondMap":
                return "ThirdMap";
            case "ThirdMap":
                return "FourthMap";
            case "FourthMap":
                return "FifthMap";
            case "FifthMap":
                return "MainMenu";
            case "MainMenu":
                return "TutorialMap";
            default:
                break;
        }
        return "FirstMap";
    }

    // Starts the coroutine that plays the loading screen icon animation
    [ContextMenu("Play Loading Icon Animation")]
    private void StartLoadingIconCoroutine()
    {
        if (loadingIconCoroutine != null) StopCoroutine(loadingIconCoroutine);

        loadingIconCoroutine = PlayLoadingIconAnimation();
        StartCoroutine(loadingIconCoroutine);
    }

    // Plays the loading screen icon animation - updates the image with a new sprite after every time interval
    private IEnumerator PlayLoadingIconAnimation()
    {
        while (loadingScreenIcon.enabled && loadingScreenIcon.gameObject.activeInHierarchy)
        {
            foreach (Sprite sprite in loadingIconSprites)
            {
                yield return new WaitForSeconds(loadingIconSpeed);
                loadingScreenIcon.sprite = sprite;
            }
            yield return null;
        }
    }

    // Loads the next level asynchronously 
    private IEnumerator LoadNextLevelAsync()
    {
        saveManagerScript.HasCompletedTutorialCheck();
        saveManagerScript.HasCompletedGameCheck();

        loadingScreen.SetActive(true);
        StartLoadingIconCoroutine();
        SetLoadingScreenImage();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelToLoad);
        if (!IsLoadingMainMenu()) asyncLoad.allowSceneActivation = false;
        else yield break;

        while (!asyncLoad.isDone)
        {
            loadingScreenBar.value = asyncLoad.progress;

            if (asyncLoad.progress >= 0.9f && !asyncLoad.allowSceneActivation)
            {
                loadingScreenIcon.gameObject.SetActive(false);
                loadingScreenText.text = messageOnLoaded;
                ActivateNextSceneInputCheck(asyncLoad);
            }
            yield return null;
        }
    }

    // Checks to activate the next loaded scene 
    private void ActivateNextSceneInputCheck(AsyncOperation asyncLoad)
    {
        if (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.KeypadEnter)) return;

        foreach (Transform child in loadingScreen.transform)
            child.gameObject.SetActive(false);

        loadingScreenImage.color = Color.black;
        asyncLoad.allowSceneActivation = true;
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        playerScript = (sceneName != mainMenu) ? FindObjectOfType<TileMovementController>() : null;
        blackOverlayScript = FindObjectOfType<BlackOverlay>();
        saveManagerScript = FindObjectOfType<SaveManager>();
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "LoadingScreen":
                    loadingScreen = child.gameObject;
                    loadingScreenImage = loadingScreen.GetComponent<Image>();
                    break;
                case "LS_Text":
                    loadingScreenText = child.GetComponent<TextMeshProUGUI>();
                    break;
                case "LS_Icon":
                    loadingScreenIcon = child.GetComponent<Image>();
                    break;
                case "LS_Bar":
                    loadingScreenBar = child.GetComponent<Slider>();
                    break;
                case "Tips":
                    loadingScreenTips = child.gameObject;
                    break;
                default:
                    break;
            }

            if (child.name == "ZoneIntroHolder" || child.name == "EndCreditsHolder") continue;
            SetVariables(child);
        }
    }

    // Sets private variables, game objects, and components
    private void SetElements()
    {
        SetVariables(blackOverlayScript.transform.parent);
    }

}
