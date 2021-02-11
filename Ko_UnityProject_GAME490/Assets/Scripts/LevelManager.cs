using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public string nextLevelToLoad;
    float rayLength = 1f;

    [Header("Loading Screen Elements")]
    public TextMeshProUGUI loadingText;
    public Slider loadingBar;
    public GameObject loadingScreen, loadingIcon;
    public Sprite[] loadingScreenSprites;

    private Animator playerAnimator;
    private TileMovementController playerScript;
    private LevelFade levelFadeScript;
    

    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = FindObjectOfType<TileMovementController>().GetComponentInChildren<Animator>();
        playerScript = FindObjectOfType<TileMovementController>();
        levelFadeScript = FindObjectOfType<LevelFade>();
    }

    // Update is called once per frame
    void Update()
    {
        /*** For Debugging purposes ***/
        /*if (Input.GetKeyDown(KeyCode.L))
            DisablePlayer();*/
        /*** End Debugging ***/
    }

    // Loads the next level asynchronously as the loading screen is active 
    public IEnumerator LoadNextLevelAsync()
    {
        loadingScreen.SetActive(true);
        ChangeLoadingScreenImg();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextLevelToLoad);

        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            loadingBar.value = asyncLoad.progress;

            if (asyncLoad.progress >= 0.9f && !asyncLoad.allowSceneActivation)
            {
                loadingText.text = "Press Any Key To Continue";
                loadingIcon.SetActive(false);

                if (Input.anyKeyDown)
                {
                    loadingText.gameObject.SetActive(false);
                    loadingBar.gameObject.SetActive(false);
                    loadingIcon.gameObject.SetActive(false);

                    asyncLoad.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }

    // Determines wether the player has touched this object 
    public bool checkIfCompletedLevel()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0, 0), Vector3.up);
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.blue);

        if(Physics.Raycast(myRay, out hit, rayLength))
        {
            if (hit.collider.tag == "Player")
            {
                playerScript.ResetTorchMeter();
                DisablePlayer();
                //SaveManager.DeleteGame();
                return true;
            }
            else return false;
        }
        return false;
    }

    // Prevents the player from receiving input
    public void DisablePlayer()
    {
        levelFadeScript.FadeOutToNextLevel();
        //playerAnimator.SetTrigger("Idle");
        playerScript.enabled = false;
    }

    // Sets a random image/sprite for the loading screen
    private void SetRandomSprite(Sprite newLoadingScreenImg)
    {
        if (loadingScreen.GetComponent<Image>().sprite.name == newLoadingScreenImg.name)
            return;
        else
            loadingScreen.GetComponent<Image>().sprite = newLoadingScreenImg;
    }

    // Gets a random sprite from its respective array
    private void ChangeLoadingScreenImg()
    {
        if (loadingScreenSprites != null)
            SetRandomSprite(loadingScreenSprites[UnityEngine.Random.Range(0, loadingScreenSprites.Length)]);
    }

}
