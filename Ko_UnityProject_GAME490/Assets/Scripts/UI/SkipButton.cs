using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SkipButton : MonoBehaviour
{
    public Image content;

    public float fillAmount;
    public float fillSpeed;
    public float maxFillAmount;
    public float lerpSpeed;

    public AudioClip skipTutorialSFX;
    public AudioClip barPopUpSFX;

    private bool canHoldButton;
    private bool hasSkippedTutorial;
    private bool hasStartedHolding;

    private Animator skipButtonAnim;
    private AudioSource audioSource;
    private LevelManager levelManagerScript;
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

    void Awake()
    {
        levelManagerScript = FindObjectOfType<LevelManager>();
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

        if (Input.GetKey(KeyCode.X) && fillAmount <= maxFillAmount && !hasSkippedTutorial && !pauseMenuScript.isPaused && canHoldButton)
        {
            fillAmount += Time.deltaTime * fillSpeed;

            if (!hasStartedHolding)
            {
                skipButtonAnim.SetTrigger("Holding");
                PlayPopUpSFX();

                hasStartedHolding = true;     
                pauseMenuScript.canPause = false;
                playerScript.SetPlayerBoolsFalse();
            }
        }
        else if (fillAmount >= maxFillAmount && !hasSkippedTutorial && !pauseMenuScript.isPaused)
        {
            Debug.Log("Tutorial Has Been Skipped");
            content.fillAmount = maxFillAmount;
            skipButtonAnim.SetTrigger("NotHolding");
            PlaySkipTutorialSFX();

            levelManagerScript.DisablePlayer();//      

            hasSkippedTutorial = true;
            canHoldButton = false;//
            pauseMenuScript.canPause = false;
            playerScript.SetPlayerBoolsFalse();
        }
        else if (Input.GetKeyUp(KeyCode.X) && !pauseMenuScript.isPaused && canHoldButton)
        {
            //hasSkippedTutorial = false;//
            hasStartedHolding = false;
            canHoldButton = false;
            pauseMenuScript.canPause = true;
            playerScript.SetPlayerBoolsTrue();

            skipButtonAnim.SetTrigger("NotHolding");
            fillAmount = 0;
        }
    }

    // Adjusts the fillAmount via linear interpolation
    private void SkipTutorialBar()
    {
        if (fillAmount != content.fillAmount && !hasSkippedTutorial)
        {
            content.fillAmount = Mathf.Lerp(content.fillAmount, fillAmount, Time.deltaTime * lerpSpeed);
        }
    }

    private float Map(float value, float max)
    {
        return value / max;
    }

    public void setMaxValue(float value)
    {
        MaxValue = value;
    }

    // Triggers an animation if the bool is false - for an animation event
    public void TransitionToIdle()
    {
        if(!hasSkippedTutorial)
            skipButtonAnim.SetTrigger("BackToIdle");         
    }

    // Sets the canHoldButton bool to true - for an animation event
    public void SetBoolTrue()
    {
        canHoldButton = true;
    }

    // Plays an SFX when the Skip Tutorial Bar is maxed
    private void PlaySkipTutorialSFX()
    {
        audioSource.volume = 0.2f;
        audioSource.pitch = 3f;
        audioSource.PlayOneShot(skipTutorialSFX);
    }

    // Plays an SFX when the Skip Tutorial Bar pops in
    private void PlayPopUpSFX()
    {
        audioSource.volume = 0.3f;
        audioSource.pitch = 1f;
        audioSource.PlayOneShot(barPopUpSFX);
    }


}
