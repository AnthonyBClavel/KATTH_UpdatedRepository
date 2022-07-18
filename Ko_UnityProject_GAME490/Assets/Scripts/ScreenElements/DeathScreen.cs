using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathScreen : MonoBehaviour
{
    //[SerializeField]
    private bool canDeathScreen = false;

    private GameObject deathScreenHolder;
    private Animator deathScreenAnim;

    private IEnumerator inputCoroutine;
    private PuzzleManager puzzleManagerScript;
    private AudioManager audioManagerScript;
    private BlackOverlay blackOverlayScript;
    private SafetyMenu safetyMenuScript;

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Note: the death screen is currently not in use - scrapped
        DisableDeathScreen();
    }

    // Plays the pop in animation for the death screen
    public void PopInDeathScreen() => ChangeAnimationState("DS_PopIn");

    // Plays the pop out animation for the death screen
    public void PopOutDeathScreen() => ChangeAnimationState("DS_PopOut");

    // Sets the death screen game object and script active - For Debugging Purposes ONLY
    [ContextMenu("Enable Death Screen")]
    private void EnableDeathScreen()
    {
        gameObject.SetActive(true);
        canDeathScreen = true;
        enabled = true;
    }

    // Sets the death screen game object and script inactive - For Debugging Purposes ONLY
    [ContextMenu("Disable Death Screen")]
    private void DisableDeathScreen()
    {
        deathScreenHolder.SetActive(false);
        gameObject.SetActive(false);
        canDeathScreen = false;
        enabled = false;
    }

    // Checks to play the sfx for the death screen
    public void SetDeathScreenActive()
    {
        ReselectLastSelected.instance.enabled = true;
        audioManagerScript.PlayDeathScreenSFX();
        deathScreenHolder.SetActive(true);
        StartInputCoroutine();
    }

    // Plays a new animation state for the death screen
    private void ChangeAnimationState(string newState)
    {
        switch (newState)
        {
            case ("DS_PopOut"):
                deathScreenAnim.SetTrigger(newState);
                break;
            case ("DS_PopIn"):
                deathScreenAnim.SetTrigger(newState);
                break;
            default:
                //Debug.Log("Animation state does not exist");
                break;
        }
    }

    // Checks if the death screen can be activted
    public bool CanDeathScreen()
    {
        if (!canDeathScreen || !isActiveAndEnabled) return false;

        return true;
    }

    // Checks for the input that opens the safety menu
    private void OpenSafetyMenuCheck()
    {
        if (safetyMenuScript.IsSafetyMenu || !Input.GetKeyDown(KeyCode.Q)) return;

        safetyMenuScript.OpenSafetyMenu();
    }

    // Checks for the input that restarts the puzzle
    private void RestartPuzzleCheck()
    {
        if (!Input.GetKeyDown(KeyCode.R)) return;

        ReselectLastSelected.instance.enabled = false;
        puzzleManagerScript.ResetPuzzle();
        deathScreenHolder.SetActive(false);
    }

    // Start the coroutine that checks for the death screen input
    private void StartInputCoroutine()
    {
        if (inputCoroutine != null) StopCoroutine(inputCoroutine);

        inputCoroutine = DeathScreenInputCheck();
        StartCoroutine(inputCoroutine);
    }

    // Checks for the death screen input
    private IEnumerator DeathScreenInputCheck()
    {
        while (deathScreenHolder.activeInHierarchy && !blackOverlayScript.IsChangingScenes)
        {
            if (Time.deltaTime > 0 && !safetyMenuScript.IsChangingMenus)
            {
                OpenSafetyMenuCheck();
                RestartPuzzleCheck();
            }
            yield return null;
        }
        //Debug.Log("Stopped looking for death screen input check");
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        puzzleManagerScript = FindObjectOfType<PuzzleManager>();
        audioManagerScript = FindObjectOfType<AudioManager>();
        blackOverlayScript = FindObjectOfType<BlackOverlay>();
        safetyMenuScript = FindObjectOfType<SafetyMenu>();   
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "DS_Holder":
                    deathScreenAnim = child.GetComponent<Animator>();
                    deathScreenHolder = child.gameObject;
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
    }

}
