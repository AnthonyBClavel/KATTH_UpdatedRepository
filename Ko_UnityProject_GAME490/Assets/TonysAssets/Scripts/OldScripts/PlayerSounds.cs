using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [SerializeField]
    public AudioClip[] clips_LF;

    [SerializeField]
    public AudioClip[] clips_RF;

    private bool isWalking;
    private Animator anim;
    private AudioSource audioSource;

    void Start()
    {
        anim = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }
    void FixedUpdate()
    {
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
    }

    private void LeftFootstep()
    {
        if (isWalking)
        {
            AudioClip clips_LF = GetRandomClip_LF();
            audioSource.PlayOneShot(clips_LF);
        }
    }
    private void RightFootstep()
    {
        if (isWalking)
        {
            AudioClip clips_RF = GetRandomClip_RF();
            audioSource.PlayOneShot(clips_RF);
        }
    }

    private AudioClip GetRandomClip_LF()                                    //Get random clips for left foot and right foot
    {
        return clips_LF[UnityEngine.Random.Range(0, clips_LF.Length)];
    }
    private AudioClip GetRandomClip_RF()
    {
        return clips_RF[UnityEngine.Random.Range(0, clips_RF.Length)];
    }

}
