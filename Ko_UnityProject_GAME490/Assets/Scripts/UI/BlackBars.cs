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

    private RectTransform topBarRectTransform;
    private RectTransform bottomBarRectTransform;
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

    // Starts the coroutines that move the dialogue bars "onto" the screen
    public void MoveBlackBarsIn()
    {
        hasMovedBars = true;

        StopAllCoroutines();
        StartCoroutine(LerpBlackBar(topBarRectTransform, FinalHeight, animDuration));
        StartCoroutine(LerpBlackBar(bottomBarRectTransform, FinalHeight, animDuration));
    }

    // Starts the coroutines that move the dialogue bars "out" of the screen
    public void MoveBlackBarsOut()
    {
        hasMovedBars = false;

        StopAllCoroutines();
        StartCoroutine(LerpBlackBar(topBarRectTransform, 0f, animDuration));
        StartCoroutine(LerpBlackBar(bottomBarRectTransform, 0f, animDuration));
    }

    // Sets each black bar's height to the finalHeight
    public void TurnOnBlackBars()
    {
        topBarRectTransform.sizeDelta = new Vector2(0, FinalHeight);
        bottomBarRectTransform.sizeDelta = new Vector2(0, FinalHeight);
    }

    // Sets each black bar's height to zero
    public void TurnOffBlackBars()
    {
        topBarRectTransform.sizeDelta = new Vector2(0, 0);
        bottomBarRectTransform.sizeDelta = new Vector2(0, 0);
    }

    // Moves the black bars in/out - For Debugging Purposes Only
    private void ToggleBlackBarsDebug()
    {
        if (!hasMovedBars)
        {
            MoveBlackBarsIn();
            Debug.Log("Debugging: moved black bars IN");
        }
        else if (hasMovedBars)
        {
            MoveBlackBarsOut();
            Debug.Log("Debugging: moved black bars OUT");
        }
    }

    // Lerps the height of the black bar to another over a specific duration (finalHeight = height to lerp to, duration = seconds)
    private IEnumerator LerpBlackBar(RectTransform bar, float endHeight, float duration)
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

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets them by looking at the names of children
        for (int i = 0; i < gameHUDScript.transform.parent.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.parent.GetChild(i).gameObject;

            if (child.name == "BlackBars")
            {
                GameObject blackBars = child;

                for (int j = 0; j < blackBars.transform.childCount; j++)
                {
                    GameObject child02 = blackBars.transform.GetChild(j).gameObject;
                    RectTransform rectTransform = child02.GetComponent<RectTransform>();

                    if (child02.name == "TopBar")
                        topBarRectTransform = rectTransform;                
                    if (child02.name == "BottomBar")
                        bottomBarRectTransform = rectTransform;
                }    
            }
        }
    }

    // Enables debugging for the black bars - For Debugging Purposes ONLY
    public void DebuggingCheck(GameManager gameManager)
    {
        if (gameManager.isDebugging)
        {
            if (Input.GetKeyDown(KeyCode.P))
                ToggleBlackBarsDebug();

            // If canDebugBars is true...
            if (!canDebugBars)
                return;
            // And if their height is not equal to the finalHeight...
            if (topBarRectTransform.rect.height == FinalHeight || bottomBarRectTransform.rect.height == FinalHeight)
                return;

            // Adjust the the black bars' height constantly
            Vector2 newHieght = new Vector2(0, FinalHeight);
            topBarRectTransform.sizeDelta = newHieght;
            bottomBarRectTransform.sizeDelta = newHieght;
        }
    }

}
