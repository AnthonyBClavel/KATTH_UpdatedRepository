using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TipsManager : MonoBehaviour
{
    public TextMeshProUGUI tipTextBox;
    public GameObject leftArrow;
    public GameObject rightArrow;

    public AudioClip nextTipSFX;
    private AudioSource audioSource;

    private Vector3 originalRightArrow;
    private Vector3 originalLeftArrow;
    private Vector3 scaledRightArrow;
    private Vector3 scaledLeftArrow;

    private bool canPressRightArrow = true;
    private bool canPressLeftArrow = true;
    private bool canGetNewTip = true;
    private bool canLeftClick = true;
    private bool canPushButton = true;

    [TextArea]
    public string[] tips;
    private int currentIndex = 0;
    private int tipsArraySize;

    // Start is called before the first frame update
    void Start()
    {
        SetAndSelectRandomTip();
        tipsArraySize = tips.Length - 1;

        audioSource = GetComponent<AudioSource>();
        originalRightArrow = rightArrow.transform.localScale;
        originalLeftArrow = leftArrow.transform.localScale;
        scaledRightArrow = rightArrow.transform.localScale * 0.9f;
        scaledLeftArrow = leftArrow.transform.localScale * 0.9f;
    }

    // Update is called once per frame
    void Update()
    {
        LoadNewTipCheck();
    }

    // Checks when the player can load the next/previous tip
    private void LoadNewTipCheck()
    {
        if (Input.GetKey(KeyCode.E) && canPressRightArrow && canGetNewTip && canPushButton)
        {
            if (currentIndex < tipsArraySize)
                currentIndex++;
            else if (currentIndex == tipsArraySize)
                currentIndex = 0;

            PlayNextTipSFX();
            canPressLeftArrow = false;
            canGetNewTip = false;
            canLeftClick = false;
            rightArrow.transform.localScale = scaledRightArrow;
            tipTextBox.text = "TIP: " + tips[currentIndex];
        }
        else if (Input.GetKey(KeyCode.Q) && canPressLeftArrow && canGetNewTip && canPushButton)
        {
            if (currentIndex > 0)
                currentIndex--;
            else if (currentIndex == 0)
                currentIndex = tipsArraySize;

            PlayNextTipSFX();
            canPressRightArrow = false;
            canGetNewTip = false;
            canLeftClick = false;
            leftArrow.transform.localScale = scaledLeftArrow;
            tipTextBox.text = "TIP: " + tips[currentIndex];
        }
        else if (Input.GetKeyUp(KeyCode.E) && canPressRightArrow && canPushButton)
        {
            canPressLeftArrow = true;
            canGetNewTip = true;
            canLeftClick = true;
            rightArrow.transform.localScale = originalRightArrow;
        }
        else if (Input.GetKeyUp(KeyCode.Q) && canPressLeftArrow && canPushButton)
        {
            canPressRightArrow = true;
            canGetNewTip = true;
            canLeftClick = true;
            leftArrow.transform.localScale = originalLeftArrow;
        }
    }


    /*** Functions for event trigger START HERE ***/
    // Function for OnPointerDown (on right arrow)...
    public void SelectRightArrow()
    {
        if (canPressRightArrow && canGetNewTip && canLeftClick)
        {
            if (currentIndex < tipsArraySize)
                currentIndex++;
            else if (currentIndex == tipsArraySize)
                currentIndex = 0;

            PlayNextTipSFX();
            canPressLeftArrow = false;
            canGetNewTip = false;
            canPushButton = false;
            rightArrow.transform.localScale = scaledRightArrow;
            tipTextBox.text = "TIP: " + tips[currentIndex];
        }
    }

    // Function for OnPointerDown (on left arrow)...
    public void SelectLeftArrow()
    {
        if (canPressLeftArrow && canGetNewTip && canLeftClick)
        {
            if (currentIndex > 0)
                currentIndex--;
            else if (currentIndex == 0)
                currentIndex = tipsArraySize;

            PlayNextTipSFX();
            canPressRightArrow = false;
            canGetNewTip = false;
            canPushButton = false;
            leftArrow.transform.localScale = scaledLeftArrow;
            tipTextBox.text = "TIP: " + tips[currentIndex];
        }
    }

    // Function for OnPointerUp (on right arrow)...
    public void DeselectRightArrow()
    {
        if (canPressRightArrow && canLeftClick)
        {
            canPressLeftArrow = true;
            canGetNewTip = true;
            canPushButton = true;
            rightArrow.transform.localScale = originalRightArrow;
        }
    }

    // Function for OnPointerUp (on left arrow)...
    public void DeselectLeftArrow()
    {
        if(canPressLeftArrow && canLeftClick)
        {
            canPressRightArrow = true;
            canGetNewTip = true;
            canPushButton = true;
            leftArrow.transform.localScale = originalLeftArrow;
        }
    }
    /*** Functions for event trigger END HERE ***/


    // Randomly selects a string within the tips array and sets it to the tip text
    private void SetAndSelectRandomTip()
    {
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

        currentIndex = UnityEngine.Random.Range(0, tips.Length);
        tipTextBox.text = "TIP: " + tips[currentIndex];
    }

    // Plays the NextTipSFX
    private void PlayNextTipSFX()
    {
        audioSource.volume = 1f;
        audioSource.pitch = 3f;
        audioSource.PlayOneShot(nextTipSFX);
    }


}
