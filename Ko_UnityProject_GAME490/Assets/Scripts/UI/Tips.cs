using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Tips : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    private bool onPointerDown = false;
    private bool canOnPointerClick = false;

    //[TextArea] // <-- Uncomment this for larger text boxes in script's component (in unity inspector)
    private string[] tips;
    private string objectName;
    private int currentTipIndex;
    private int tipsArraySize;
    private float animLength = 0.05f;

    private TextMeshProUGUI tipText;
    private GameObject leftArrow;
    private GameObject rightArrow;

    private Vector3 rightArrowOriginalSize;
    private Vector3 leftArrowOriginalSize;
    private Vector3 rightArrowScaledSize;
    private Vector3 leftArrowScaledSize;

    private KeyCode leftArrowKeyCode = KeyCode.Q;
    private KeyCode rightArrowKeyCode = KeyCode.E;

    private AudioManager audioManagerScript;

    void Awake()
    {
        audioManagerScript = FindObjectOfType<AudioManager>();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetTips();

        rightArrowOriginalSize = rightArrow.transform.localScale;
        leftArrowOriginalSize = leftArrow.transform.localScale;
        rightArrowScaledSize = rightArrow.transform.localScale * 0.9f;
        leftArrowScaledSize = leftArrow.transform.localScale * 0.9f;
    }

    // Update is called once per frame
    void Update()
    {
        TipsInputCheck();
    }

    // Returns or sets the value of currentTipIndex
    public int CurrentTipIndex
    {
        get
        {
            return currentTipIndex;
        }
        set
        {
            currentTipIndex = value;

            if (currentTipIndex > tipsArraySize)
                currentTipIndex = 0;
            else if (currentTipIndex < 0)
                currentTipIndex = tipsArraySize;

            tipText.text = "TIP: " + tips[currentTipIndex];
            audioManagerScript.PlayTipButtonClickSFX();
        }
    }

    // Checks when the player can load the next/previous tip
    private void TipsInputCheck()
    {
        if (!onPointerDown)
        {
            if (Input.GetKeyDown(rightArrowKeyCode))
            {
                if (rightArrow.transform.localScale == rightArrowOriginalSize)
                {
                    CurrentTipIndex++;
                    PlayTipButtonAnim(rightArrow, rightArrowScaledSize);
                }
            }

            if (Input.GetKeyDown(leftArrowKeyCode))
            {
                if (leftArrow.transform.localScale == leftArrowOriginalSize)
                {
                    CurrentTipIndex--;
                    PlayTipButtonAnim(leftArrow, leftArrowScaledSize);
                }
            }
        }
    }

    // Sets the right arrow to its scaled size
    private void SelectRightArrow()
    {
        if (rightArrow.transform.localScale == rightArrowOriginalSize)
        {
            rightArrow.transform.localScale = rightArrowScaledSize;
            onPointerDown = true;
            canOnPointerClick = true;
        }
    }

    // Sets the left arrow to its scaled size
    private void SelectLeftArrow()
    {
        if (leftArrow.transform.localScale == leftArrowOriginalSize)
        {
            leftArrow.transform.localScale = leftArrowScaledSize;
            onPointerDown = true;
            canOnPointerClick = true;
        }
    }

    // Sets the right arrow to its original size
    private void DeselectRightArrow()
    {
        if (rightArrow.transform.localScale == rightArrowScaledSize)
        {
            rightArrow.transform.localScale = rightArrowOriginalSize;
            onPointerDown = false;
        }
    }

    // Sets the left arrow to its original size
    private void DeselectLeftArrow()
    {
        if (leftArrow.transform.localScale == leftArrowScaledSize)
        {
            leftArrow.transform.localScale = leftArrowOriginalSize;
            onPointerDown = false;
        }
    }

    // Determines when the right/left arrow is selected - by looking at the name of the object the pointer (mouse0) hit
    public void OnPointerDown(PointerEventData eventData)
    {
        objectName = eventData.pointerCurrentRaycast.gameObject.name;
        canOnPointerClick = false;

        if (objectName == "RightArrow")
            SelectRightArrow();
        else if (objectName == "LeftArrow")
            SelectLeftArrow();
    }

    // Determines when the right/left arrow is unselected - by looking at the name of the object the pointer (mouse0) hit previously
    public void OnPointerUp(PointerEventData eventData)
    {
        if (objectName == "RightArrow")
            DeselectRightArrow();
        else if (objectName == "LeftArrow")
            DeselectLeftArrow();
    }

    // Determines when the right/left arrow has been clicked (OnPointerDown and OnPointerUp must occur on the object for click to register)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (canOnPointerClick)
        {
            if (objectName == "RightArrow")
            {
                CurrentTipIndex++;
                canOnPointerClick = false;
            }
            else if (objectName == "LeftArrow")
            {
                CurrentTipIndex--;
                canOnPointerClick = false;
            }
        }
    }

    // Starts the coroutine that plays the animation for the tip button
    private void PlayTipButtonAnim(GameObject arrow, Vector3 endScale)
    {
        IEnumerator tipButtonCoroutine = TipButtonAnimation(arrow, endScale);
        StartCoroutine(tipButtonCoroutine);
    }

    // Plays the animation for pressing a tip button
    private IEnumerator TipButtonAnimation(GameObject arrow, Vector3 endScale)
    {
        //Vector3 startScale = arrow.transform.localScale;
        arrow.transform.localScale = endScale;
        yield return new WaitForSeconds(animLength);
        //arrow.transform.localScale = startScale;

        if (arrow == rightArrow)
            arrow.transform.localScale = rightArrowOriginalSize;
        else if (arrow == leftArrow)
            arrow.transform.localScale = leftArrowOriginalSize;
    }

    // Randomly selects a string within the tips array and sets it to the tip text
    private void SetTips()
    {
        // Sets the size of the array
        tips = new string[10];
        tipsArraySize = tips.Length - 1;

        // Sets a tip for each index in the array (10 total tips)
        tips[0] = "you should always have one move left before interacting with a firestone.";
        tips[1] = "sometimes you don't need to push a wooden crate, some are just for decor!";
        tips[2] = "remember this, every object in a puzzle is interactable in some way!";
        tips[3] = "your torch meter will reset after walking on a bridge, so don't worry!";
        tips[4] = "some rocks are just not meant to be broken, but then again, some are!";
        tips[5] = "remember, you can only interact with a firestone once in every puzzle!";
        tips[6] = "you can push wooden crates into holes and walk over them, very useful!";
        tips[7] = "sometimes there can be more than one way to both solve and fail a puzzle.";
        tips[8] = "remember this, interacting with a firestone will reset your torch meter!";
        tips[9] = "wooden crates cannot be pushed onto bridges, they're afraid of heights.";

        // Randomly selects a tip from the array and sets it to the tipText
        currentTipIndex = UnityEngine.Random.Range(0, tips.Length);
        tipText.text = "TIP: " + tips[currentTipIndex];
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "LeftArrow")
                leftArrow = child;
            if (child.name == "RightArrow")
                rightArrow = child;
            if (child.name == "TipText")
                tipText = child.GetComponent<TextMeshProUGUI>();
        }
    }
}
