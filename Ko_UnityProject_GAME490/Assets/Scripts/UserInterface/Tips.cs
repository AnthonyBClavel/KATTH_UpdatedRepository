using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class Tips : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public TextAsset gameTips;

    private float animLength = 0.05f;
    private float scaledSize = 0.9f;
    private string objectName;
    private string[] tips;
    private int tipIndex;

    private bool canOnPointerClick = false;
    private bool onPointerDown = false;

    private TextMeshProUGUI tipText;
    private GameObject leftButton;
    private GameObject rightButton;

    private Vector3 buttonOriginalSize;
    private Vector3 buttonScaledSize;

    private IEnumerator tipsInputCoroutine;
    private KeyCode leftButtonKeyCode = KeyCode.Q;
    private KeyCode rightButtonKeyCode = KeyCode.E;

    public int CurrentTipIndex
    {
        get { return tipIndex; }
        set
        {
            tipIndex = value;
            SetCurrentTip(tipIndex);
            AudioManager.instance.PlayTipButtonClickSFX();
        }
    }

    // Awake is called before Start()
    void Awake()
    {
        SetElements();
    }

    // Called every time the object is enabled
    void OnEnable()
    {
        StartTipsInputCoroutine();
    }

    // Sets the current tip
    private void SetCurrentTip(int index)
    {
        int maxTipIndex = tips.Length - 1;

        if (index < 0) tipIndex = maxTipIndex;
        else if (index > maxTipIndex) tipIndex = 0;

        tipText.text = $"TIP: {tips[tipIndex]}";
    }

    // Checks to set the button to its scaled size
    private void SelectButton(GameObject button)
    {
        if (button.transform.localScale != buttonOriginalSize) return;

        button.transform.localScale = buttonScaledSize;
        canOnPointerClick = true;
        onPointerDown = true;
    }

    // Checks to set the button to its original size
    private void DeselectButton(GameObject button)
    {
        if (button.transform.localScale != buttonScaledSize) return;

        button.transform.localScale = buttonOriginalSize;
        onPointerDown = false;
    }

    // Determines when the right/left button is selected
    // Note: looks at the name of the object the pointer is currenlty hitting (mouse0)
    public void OnPointerDown(PointerEventData eventData)
    {
        objectName = eventData.pointerCurrentRaycast.gameObject.name;
        canOnPointerClick = false;

        GameObject button = (objectName == rightButton.name) ? rightButton : leftButton;
        SelectButton(button);
    }

    // Determines when the right/left button is unselected
    // Note: looks at the name of the object the pointer previously hit (mouse0)
    public void OnPointerUp(PointerEventData eventData)
    {
        GameObject button = (objectName == rightButton.name) ? rightButton : leftButton;
        DeselectButton(button);
    }

    // Determines when the right/left button is clicked
    // Note: OnPointerDown and OnPointerUp must occur on the same object for a "click" to register
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!canOnPointerClick) return;

        int value = (objectName == rightButton.name) ? 1 : -1;
        CurrentTipIndex += value;
        canOnPointerClick = false;
    }

    // Checks to set/play the next/previous tip
    private void NextTipCheck()
    {
        if (onPointerDown) return;

        if (rightButton.transform.localScale == buttonOriginalSize && Input.GetKeyDown(rightButtonKeyCode))
        {
            CurrentTipIndex++;
            StartTipButtonCoroutine(rightButton);
        }
        if (leftButton.transform.localScale == buttonOriginalSize && Input.GetKeyDown(leftButtonKeyCode))
        {
            CurrentTipIndex--;
            StartTipButtonCoroutine(leftButton);
        }      
    }

    // Starts the coroutine that checks for the tips input
    private void StartTipsInputCoroutine()
    {
        if (tipsInputCoroutine != null) StopCoroutine(tipsInputCoroutine);

        tipsInputCoroutine = TipsInputCheck();
        StartCoroutine(tipsInputCoroutine);
    }

    // Starts the coroutine that plays the animation for the button
    private void StartTipButtonCoroutine(GameObject button)
    {
        IEnumerator tipButtonCoroutine = TipButtonAnimation(button);
        StartCoroutine(tipButtonCoroutine);
    }

    // Checks for the tips input
    private IEnumerator TipsInputCheck()
    {
        //Debug.Log("Tips input check has STARTED");
        while (rightButton.activeInHierarchy && leftButton.activeInHierarchy)
        {
            if (Time.deltaTime > 0) NextTipCheck();
            yield return null;
        }
        //Debug.Log("Tips input check has ENDED");
    }

    // Sets the scale of a button to another for a specific duration
    private IEnumerator TipButtonAnimation(GameObject button)
    {
        button.transform.localScale = buttonScaledSize;

        yield return new WaitForSeconds(animLength);
        button.transform.localScale = buttonOriginalSize;
    }

    // Sets the tips to use - also sets a random initial tip
    private void SetTips()
    {
        tips = gameTips.ReturnSentences();
        SetCurrentTip(Random.Range(0, tips.Length));
    }

    // Sets the desired variables - loops through all of the children within a parent object
    private void SetVariables(Transform parent)
    {
        if (parent.childCount == 0) return;

        foreach (Transform child in parent)
        {
            switch (child.name)
            {
                case "TIP_Text":
                    tipText = child.GetComponent<TextMeshProUGUI>();
                    break;
                case "TIP_LeftButton":
                    leftButton = child.gameObject;
                    break;
                case "TIP_RightButton":
                    rightButton = child.gameObject;
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
        SetTips();

        buttonOriginalSize = rightButton.transform.localScale;
        buttonScaledSize = buttonOriginalSize * scaledSize;
    }
}
