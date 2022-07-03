using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackBars : MonoBehaviour
{
    [SerializeField] [Range(0.1f, 1.0f)]
    private float animDuration = 0.25f; // Original Value = 0.25f
    [SerializeField] [Range(1f, 1080f)]
    private float animDistance = 130f; // Original Value = 130f

    private bool hasMovedBars = false;
    private bool canDebugBars = false;

    private RectTransform topBarRT;
    private RectTransform bottomBarRT;
    private GameHUD gameHUDScript;

    public float FinalHeight
    {
        get { return animDistance; }
        set { animDistance = value; }
    }    

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Starts the coroutines that move the black bars "onto" the screen
    public void MoveBarsIn()
    {
        hasMovedBars = true;

        StopAllCoroutines();
        StartCoroutine(LerpBar(topBarRT, FinalHeight, animDuration));
        StartCoroutine(LerpBar(bottomBarRT, FinalHeight, animDuration));
    }

    // Starts the coroutines that move the black bars "out" of the screen
    public void MoveBarsOut()
    {
        hasMovedBars = false;

        StopAllCoroutines();
        StartCoroutine(LerpBar(topBarRT, 0f, animDuration));
        StartCoroutine(LerpBar(bottomBarRT, 0f, animDuration));
    }

    // Sets each black bar's height to the finalHeight
    public void TurnOnBars()
    {
        topBarRT.sizeDelta = new Vector2(0, FinalHeight);
        bottomBarRT.sizeDelta = new Vector2(0, FinalHeight);
    }

    // Sets each black bar's height to zero
    public void TurnOffBars()
    {
        topBarRT.sizeDelta = new Vector2(0, 0);
        bottomBarRT.sizeDelta = new Vector2(0, 0);
    }

    // Moves the black bars in/out - For Debugging Purposes Only
    private void ToggleBarsDebug()
    {
        if (!hasMovedBars)
        {
            MoveBarsIn();
            Debug.Log("Debugging: moved black bars IN");
        }
        else if (hasMovedBars)
        {
            MoveBarsOut();
            Debug.Log("Debugging: moved black bars OUT");
        }
    }

    // Lerps the height of the black bar to another over a specific duration (finalHeight = height to lerp to, duration = seconds)
    private IEnumerator LerpBar(RectTransform bar, float endHeight, float duration)
    {
        canDebugBars = false;
        float startHeight = bar.rect.height;
        float time = 0;

        while (time < duration)
        {
            bar.sizeDelta = new Vector2(0, Mathf.Lerp(startHeight, endHeight, time / duration));
            time += Time.deltaTime;
            yield return null;
        }

        bar.sizeDelta = new Vector2(0, endHeight);
        if (bar.rect.height == FinalHeight) canDebugBars = true;
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        gameHUDScript = FindObjectOfType<GameHUD>();
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "TopBar":
                    topBarRT = child.GetComponent<RectTransform>();
                    break;
                case "BottomBar":
                    bottomBarRT = child.GetComponent<RectTransform>();
                    break;
                default:
                    break;
            }

            if (child.name == "HUD" || child.name == "CharacterDialogue" || child.name == "KeybindButtons") continue;
            SetVariables(child);
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        SetVariables(gameHUDScript.transform.parent);
    }

    // Check to toggle the black bars and to update their height - For Debugging Purposes ONLY
    public void DebuggingCheck(GameManager gameManager)
    {
        if (!gameManager.isDebugging) return;

        if (Input.GetKeyDown(KeyCode.P)) // Debug key is "P"
            ToggleBarsDebug();

        if (!canDebugBars || topBarRT.rect.height == FinalHeight || bottomBarRT.rect.height == FinalHeight) return;

        // Updates the height of the black bars
        Vector2 newHeight = new Vector2(0, FinalHeight);
        topBarRT.sizeDelta = newHeight;
        bottomBarRT.sizeDelta = newHeight;
    }

}
