using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HUD : MonoBehaviour
{
    static readonly string tutorialZone = "TutorialMap";
    private string sceneName;

    private GameObject notificationBubblesHolder;
    private GameObject torchMeter;

    private NotificationBubbles notificationBubblesScript;
    private SkipSceneButton skipSceneButtonScript;
    private TileMovementController playerScript;
    private BlackOverlay blackOverlayScript;
    private TorchMeter torchMeterScript;
    private PauseMenu pauseMenuScript;


    // Awake is called before Start()
    void Awake()
    {
        sceneName = SceneManager.GetActiveScene().name;

        SetScripts();
        SetElements();
    }

    // Update is called once per frame
    void Update()
    {
        HUDInputCheck();
    }

    // Turns on the HUD
    public void TurnOnHUD()
    {
        enabled = true;
        torchMeter.SetActive(true);
        notificationBubblesHolder.SetActive(true);

        if (sceneName != tutorialZone) return;
        skipSceneButtonScript.SetSkipSceneButtonActive();
    }

    // Turns off the HUD
    public void TurnOffHUD()
    {
        enabled = false;
        torchMeter.SetActive(false);
        notificationBubblesHolder.SetActive(false);

        if (sceneName != tutorialZone) return;
        skipSceneButtonScript.SetSkipSceneButtonInactive();
    }

    // Checks if the HUD can be toggled
    private bool CanToggleHUD()
    {
        if (Time.deltaTime == 0f || !playerScript.CanRestartPuzzle || playerScript.IsCrossingBridge) return false;
        if (!pauseMenuScript.CanPause || blackOverlayScript.IsChangingScenes) return false;

        return true;
    }

    // Checks to toggle the HUD on/off
    private void HUDInputCheck()
    {
        if (!CanToggleHUD()) return;

        notificationBubblesScript.MoveBubblesInputCheck();
        ToggleHUDCheck();
    }

    // Checks to toggle the HUD on/off
    private void ToggleHUDCheck()
    {
        if (!Input.GetKeyDown(KeyCode.C)) return;

        notificationBubblesScript.ToggleKeybindBubbles();
        torchMeterScript.ToggleTorchMeter();
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        notificationBubblesScript = FindObjectOfType<NotificationBubbles>();
        skipSceneButtonScript = FindObjectOfType<SkipSceneButton>();
        playerScript = FindObjectOfType<TileMovementController>();
        blackOverlayScript = FindObjectOfType<BlackOverlay>();
        torchMeterScript = FindObjectOfType<TorchMeter>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "NB_Holder":
                    notificationBubblesHolder = child.gameObject;
                    break;
                case "TorchMeter":
                    torchMeter = child.gameObject;
                    break;
                default:
                    break;
            }

            if (child.parent.name == "TorchMeter" || child.parent.name == "NB_Holder") continue;
            if (child.name == "CharacterDialogue" || child.name == "KeybindButtons") continue;

            SetVariables(child);
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        SetVariables(transform);
    }

}
