using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectShakeController : MonoBehaviour
{
    [SerializeField]                                                            
    public AudioClip[] clips;                                                     

    public static ObjectShakeController instance;                                 

    private float shakeTimeRemaining, shakePower, shakeFadeTime, shakeRotation;   

    public float rotationMultiplier = 7.5f;                                       

    public GameObject particleEffect;                                             

    private AudioSource audioSource;                                              

    // Start is called before the first frame update
    void Start()
    {
        instance = this;                                                          

        audioSource = GetComponent<AudioSource>();                                
    }

    // Update is called once per frame
    void Update()
    {
        /*** For Debugging purposes ***/
        /*if(Input.GetKeyDown(KeyCode.K))
            StartShake(0.5f, 1f);*/
        /*** End Debugging ***/
    }

    private void LateUpdate()
    {
        // Determines the shake...
        if (shakeTimeRemaining > 0)                                                                                         
        {
            shakeTimeRemaining -= Time.deltaTime;

            float xAmount = Random.Range(-1f, 1f) * shakePower;
            float yAmount = Random.Range(-1f, 1f) * shakePower;

            /* Ignore this line below, unless you want the object to move to a random/new position after it shakes */
            //transform.position += new Vector3(xAmount, yAmount, 0);                                                       

            shakePower = Mathf.MoveTowards(shakePower, 0f, shakeFadeTime * Time.deltaTime);

            shakeRotation = Mathf.MoveTowards(shakeRotation, 0f, shakeFadeTime * rotationMultiplier * Time.deltaTime);
        }

        transform.rotation = Quaternion.Euler(0f, 0f, shakeRotation * Random.Range(-1f, 1f));

    }

    // Function for the shake itself (length is for how long the shake will last in seconds, power is the shake's intensity)
    public void StartShake(float length, float power)                                                                       
    {
        StaticBlockSFX();                                                                                                   

        Instantiate(particleEffect, gameObject.transform.position, gameObject.transform.rotation);                          

        shakeTimeRemaining = length;
        shakePower = power;

        shakeFadeTime = power / length;

        shakeRotation = power * rotationMultiplier;
    }

    // Plays the random audio clip it aquired
    private void StaticBlockSFX()                                                                                           
    {
        AudioClip clips = GetRandomClip();                                                                                   
        audioSource.PlayOneShot(clips);                                                                                      
    }

    // Gets a random audio clip from its respective array
    private AudioClip GetRandomClip()                                                                                        
    {
        return clips[UnityEngine.Random.Range(0, clips.Length)];                                                             
    }

}
