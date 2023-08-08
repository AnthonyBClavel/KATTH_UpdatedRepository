using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Gameplay Variables")]
    [SerializeField] [Range(1f, 10f)]
    private float cameraSpeed = 3f; // Original Value = 3f
    [SerializeField] [Range(0f, 1f)]
    private float playerLerpDuration = 0.2f; // Original Value = 0.2f
    [SerializeField] [Range(0f, 1f)]
    private float crateLerpDuration = 0.1f; // Original Value = 0.1f
    [SerializeField] [Range(0.5f, 10f)]
    private float resetPuzzleDelay = 1.5f; // Original Value = 1.5f

    private GameObject savedInvisibleBlock;
    private NotificationBubbles notificationBubblesScript;
    private BlackOverlay blackOverlayScript;
    private SaveManager saveManagerScript;
    private CameraController cameraScript;
    private TorchMeter torchMeterScript;
    private BlackBars blackBarsScript;

    [Header("Debugging Elements")]
    public bool isDebugging;
    public int puzzleNumber;

    public float ResetPuzzleDelay
    {
        get { return resetPuzzleDelay; }
    }

    public float PlayerLerpDuration
    {
        get { return playerLerpDuration; }
    }

    public float CrateLerpDuration
    {
        get { return crateLerpDuration; }
    }

    public float CameraSpeed
    {
        get { return cameraSpeed; }
    }

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // LateUpdate is called once per frame - after Update()
    void LateUpdate()
    {
        ScriptDebuggingCheck();
    }

    // Checks for debugging methods across various scripts
    // Note: comment out this method or the update loop before building the game
    private void ScriptDebuggingCheck()
    {
        if (!isDebugging || Time.deltaTime == 0) return;

        notificationBubblesScript.DebuggingCheck();
        blackOverlayScript.DebuggingCheck();    // "Semicolon" to fade out, "Quote" to fade in
        blackBarsScript.DebuggingCheck();       // "P" to toggle the bars in/out

        torchMeterScript.DebuggingCheck();      // "RightBracket" to increase torch meter, "LeftBracket" to decrease it
        cameraScript.DebuggingCheck();          // "Equals" for next puzzle view, "Minus" for previous puzzle view

        SavedBlockDebuggingCheck();             // "Backspace" to toggle the invisible block on previous bridge active/inactive
        SaveFileDebuggingCheck();               // "Backslash" to create a new/empty save file
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        notificationBubblesScript = FindObjectOfType<NotificationBubbles>();
        blackOverlayScript = FindObjectOfType<BlackOverlay>();
        cameraScript = FindObjectOfType<CameraController>();
        saveManagerScript = FindObjectOfType<SaveManager>();
        torchMeterScript = FindObjectOfType<TorchMeter>();
        blackBarsScript = FindObjectOfType<BlackBars>();
    }
 
    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "SavedInvisibleBlock":
                    savedInvisibleBlock = child.gameObject;
                    break;
                default:
                    break;
            }

            SetVariables(child);
        }
    }

    // Sets private variables, game objects, and components
    private void SetElements()
    {
        SetVariables(saveManagerScript.transform);
    }

    // Checks to create a new save file - For Debugging Purposes ONLY
    private void SaveFileDebuggingCheck()
    {
        if (!Input.GetKeyDown(KeyCode.Backslash)) return; // Debug key is "\" (backslash)

        saveManagerScript.DeleteAllPlayerPrefs();
        Debug.Log("Debugging: created new save file!");
    }

    // Checks to set the saved invisible block active/inactive - For Debugging Purposes ONLY
    private void SavedBlockDebuggingCheck()
    {
        if (!Input.GetKeyDown(KeyCode.Backspace)) return; // Debug key is "<--" (backspace)

        bool isActive = savedInvisibleBlock.activeInHierarchy ? true : false;
        string activeStatus = isActive ? "inactive" : "active";

        savedInvisibleBlock.SetActive(!isActive);
        Debug.Log($"Debugging: saved invisible block is now {activeStatus}");
    }

    /*
    // Checks to enable the debug console in development build - For Debugging Purposes ONLY
    private void DebugConsoleDebuggingCheck()
    {
        if (!Debug.isDebugBuild || Debug.developerConsoleVisible || !Input.GetKeyDown(KeyCode.O)) return; // Debug key is "O"

        Debug.LogError($"Debugging: the debug console has been enabled");
    }
    */

}
