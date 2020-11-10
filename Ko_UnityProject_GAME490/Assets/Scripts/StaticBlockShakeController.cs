using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectShakeController : MonoBehaviour
{
    [SerializeField]                                                              //allows you to see and manipulate the variable within the Unity inspector if it's private (the variable below this line)
    public AudioClip[] clips;                                                     //creates an array of audio clips 

    public static ObjectShakeController instance;                                 //creates an instance variable for this script

    private float shakeTimeRemaining, shakePower, shakeFadeTime, shakeRotation;   //create priavte float values

    public float rotationMultiplier = 7.5f;                                       //creates a public float value (can set/manipulate in the Unity inspector)

    public GameObject particleEffect;                                             //variable for a gameobject (either within the Unity hierarchy, or a prefab)

    private AudioSource audioSource;                                              //establishes a variable for an audio source component

    // Start is called before the first frame update
    void Start()
    {
        instance = this;                                                          //set the instance variable to this script

        audioSource = GetComponent<AudioSource>();                                //sets the audio source variable to the object's audio source component (sets instance)
    }

    // Update is called once per frame
    void Update()
    {
        //to test the screen shake (just for debugging purposes)
        /*if(Input.GetKeyDown(KeyCode.K))
        {
            StartShake(0.5f, 1f);
        }*/
    }

    private void LateUpdate()
    {
        if (shakeTimeRemaining > 0)                                                                                         //the statement that determines the shake...
        {
            shakeTimeRemaining -= Time.deltaTime;

            float xAmount = Random.Range(-1f, 1f) * shakePower;
            float yAmount = Random.Range(-1f, 1f) * shakePower;

            //transform.position += new Vector3(xAmount, yAmount, 0);                                                       //ignore this line, unless you want the object to move to random/new position after it shakes

            shakePower = Mathf.MoveTowards(shakePower, 0f, shakeFadeTime * Time.deltaTime);

            shakeRotation = Mathf.MoveTowards(shakeRotation, 0f, shakeFadeTime * rotationMultiplier * Time.deltaTime);
        }

        transform.rotation = Quaternion.Euler(0f, 0f, shakeRotation * Random.Range(-1f, 1f));

    }

    public void StartShake(float length, float power)                                                                       //the function for the shake itself (length is for how long the shake will last in seconds, power is the shake's intensity)
    {
        StaticBlockSFX();                                                                                                   //calls the function stated below (bottom of script)

        Instantiate(particleEffect, gameObject.transform.position, gameObject.transform.rotation);                          //spwans the particle effect on the object's position and rotation

        shakeTimeRemaining = length;
        shakePower = power;

        shakeFadeTime = power / length;

        shakeRotation = power * rotationMultiplier;
    }
    private void StaticBlockSFX()                                                                                            //the function that plays the audio clip
    {
        AudioClip clips = GetRandomClip();                                                                                   //calls the random audio clip function below
        audioSource.PlayOneShot(clips);                                                                                      //plays the audio clip (from start to end - without intturuption) through the object's audio source component
    }
    private AudioClip GetRandomClip()                                                                                        //the function for getting a random audio clip
    {
        return clips[UnityEngine.Random.Range(0, clips.Length)];                                                             //selects a random audio clip based on the size of the array (its length)
    }

}
