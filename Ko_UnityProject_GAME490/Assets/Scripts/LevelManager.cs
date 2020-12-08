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

    private Animator playerAnimator;
    private TileMovementV2 playerScript;
    private LevelFade levelFadeScript;

    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = FindObjectOfType<TileMovementV2>().GetComponentInChildren<Animator>();
        playerScript = FindObjectOfType<TileMovementV2>();
        levelFadeScript = FindObjectOfType<LevelFade>();
    }

    // Update is called once per frame
    void Update()
    {
        /*For Debugging...
        if(Input.GetKeyDown(KeyCode.L))
        {
            DisablePlayer();
        }*/
    }

    public IEnumerator LoadNextLevelAsync()
    {
        loadingScreen.SetActive(true);

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

    public bool checkIfCompletedLevel()
    {
        Ray myRay = new Ray(transform.position + new Vector3(0, 0, 0), Vector3.up);
        RaycastHit hit;
        Debug.DrawRay(myRay.origin, myRay.direction, Color.blue);

        if(Physics.Raycast(myRay, out hit, rayLength))
        {
            // If we hit the player
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

    public void DisablePlayer()
    {
        levelFadeScript.FadeOutToNextLevel();
        //playerAnimator.SetTrigger("Idle");
        playerScript.enabled = false;
    }


}
