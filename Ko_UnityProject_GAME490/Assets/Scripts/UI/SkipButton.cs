using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SkipButton : MonoBehaviour
{
    public Image content;

    public float fillAmount = 0f;
    private float fillSpeed = 0.35f;
    private float maxFillAmount = 1f;
    private float lerpSpeed = 10f;

    public AudioClip skipTutorialSFX;
    public AudioClip barPopUpSFX;

    private bool canHoldButton;
    private bool hasSkippedTutorial;
    private bool isHoldingDownKey;

    private Animator skipButtonAnim;
    private AudioSource audioSource;
    private GameManager gameManagerScript;
    private PauseMenu pauseMenuScript;
    private TileMovementController playerScript;

    public float MaxValue { get; set; }

    public float Value
    {
        set
        {
            fillAmount = Map(value, MaxValue);
        }
    }

    public void setMaxValue(float value)
    {
        MaxValue = value;
    }

    private float Map(float value, float max)
    {
        return value / max;
    }

    void Awake()
    {
        gameManagerScript = FindObjectOfType<GameManager>();
        pauseMenuScript = FindObjectOfType<PauseMenu>();
        playerScript = FindObjectOfType<TileMovementController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        skipButtonAnim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        hasSkippedTutorial = false;
        canHoldButton = true;
    }

    // Update is called once per frame
    void Update()
    {
        SkipTutorialBar();

        if (fillAmount >= maxFillAmount && !hasSkippedTutorial)
        {
            Debug.Log("Tutorial Has Been Skipped");

            skipButtonAnim.SetTrigger("NotHolding");
            content.fillAmount = maxFillAmount;
            PlaySkipTutorialSFX();

            //playerScript.SetPlayerBoolsFalse(); // PlayerBools are set to flase in line below
            gameManagerScript.FinishedZoneCheck();

            pauseMenuScript.canPause = false;
            canHoldButton = false;
            hasSkippedTutorial = true;
        }

        if (!hasSkippedTutorial && !pauseMenuScript.isPaused && !pauseMenuScript.isChangingScenes && canHoldButton)
        {
            if (Input.GetKey(KeyCode.X) && fillAmount <= maxFillAmount)
            {
                fillAmount += Time.deltaTime * fillSpeed;

                if (!isHoldingDownKey)
                {
                    skipButtonAnim.SetTrigger("Holding");
                    PlayPopUpSFX();

                    isHoldingDownKey = true;
                    pauseMenuScript.canPause = false;
                    playerScript.SetPlayerBoolsFalse();
                }
            }

            else if (Input.GetKeyUp(KeyCode.X))
            {
                //hasSkippedTutorial = false;

                skipButtonAnim.SetTrigger("NotHolding");
                fillAmount = 0;

                isHoldingDownKey = false;
                canHoldButton = false;
                pauseMenuScript.canPause = true;
                playerScript.SetPlayerBoolsTrue();
            }
        }

    }

    // Triggers an animation if the bool is false - for an animation event
    public void TransitionToIdle()
    {
        if (!hasSkippedTutorial)
            skipButtonAnim.SetTrigger("BackToIdle");         
    }

    // Sets the canHoldButton bool to true - for an animation event
    public void SetBoolTrue()
    {
        canHoldButton = true;
    }

    // Adjusts the fillAmount via linear interpolation
    private void SkipTutorialBar()
    {
        if (fillAmount != content.fillAmount && !hasSkippedTutorial)
        {
            content.fillAmount = Mathf.Lerp(content.fillAmount, fillAmount, Time.deltaTime * lerpSpeed);
        }
    }

    // Plays the skip tutorial sfx
    private void PlaySkipTutorialSFX()
    {
        audioSource.volume = 0.2f;
        audioSource.pitch = 3f;
        audioSource.PlayOneShot(skipTutorialSFX);
    }

    // Plays an skip tutorial pop up sfx
    private void PlayPopUpSFX()
    {
        audioSource.volume = 0.3f;
        audioSource.pitch = 1f;
        audioSource.PlayOneShot(barPopUpSFX);
    }

}
