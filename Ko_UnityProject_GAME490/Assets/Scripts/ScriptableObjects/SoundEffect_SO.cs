using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "NewSFX" ,menuName = "ScriptableObjects/SFX")]
public class SoundEffect_SO : ScriptableObject
{
    public AudioClip[] audioClips;
    public AudioMixerGroup audioOutput;
    private AudioClip previousClip;

    private float originalVol;
    [SerializeField]
    private AudioSource audioSource;
    private AudioManager audioManagerScript;
    private IEnumerator lerpAudioCoroutine;

    public bool loop;
    [Range(0f, 1f)]
    public float volume = 0.5f;
    [Range(0f, 3f)]
    public float pitch = 1f;

    public float OriginalVol
    {
        get { return originalVol; }
    }

    // Awake is called before Start()
    void Awake()
    {
        SetScripts();
    }

    // Stops playing the sfx within unity editor
    public void StopInEditor(AudioSource audioSource) => audioSource.Stop();

    // Plays the sfx within unity editor
    public void PlayInEditor(AudioSource audioSource)
    {
        if (audioClips.Length == 0) return;
        AudioClip audioClip = (audioClips.Length > 1) ? ReturnRandomClip() : ReturnFirstClip();
      
        audioSource.clip = audioClip;
        audioSource.outputAudioMixerGroup = audioOutput;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.Play();
    }

    // Plays the sfx within the game
    public void Play()
    {
        if (audioClips.Length == 0) return;
        AudioClip audioClip = (audioClips.Length > 1) ? ReturnRandomClip() : ReturnFirstClip();

        if (audioSource != null && loop)
        {
            audioSource.Stop();
            audioSource.clip = audioClip;
            audioSource.name = audioClip.name;
        }
        else
        {
            GameObject audioObject = new GameObject(audioClip.name);
            audioSource = audioObject.AddComponent<AudioSource>();
       
            audioSource.clip = audioClip;
            audioSource.outputAudioMixerGroup = audioOutput;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            originalVol = volume;

            if (!loop) Destroy(audioObject, audioClip.length);
            else LoopAudioSourceCheck(audioClip.name);
        }

        audioSource.Play();
    }

    // Checks to loop the audio source
    private void LoopAudioSourceCheck(string audioClipName)
    {
        if (audioClipName == "CharNoise") return;

        audioSource.loop = true;     
    }

    // Returns a randomly selected audio clip - different from the one previously played
    private AudioClip ReturnRandomClip()
    {        
        AudioClip newClip = audioClips[Random.Range(0, audioClips.Length)];
        int attempts = 3;

        while (newClip == previousClip && attempts > 0)
        {
            newClip = audioClips[Random.Range(0, audioClips.Length)];
            attempts--;
        }

        previousClip = newClip;
        return newClip;
    }

    // Returns the first/only clip within the array
    private AudioClip ReturnFirstClip()
    {
        return audioClips[0];
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        audioManagerScript = FindObjectOfType<AudioManager>();
    }

}
