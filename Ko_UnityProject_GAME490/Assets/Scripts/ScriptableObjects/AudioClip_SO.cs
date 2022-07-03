using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "NewSFX", menuName = "ScriptableObjects/SFX")]
public class AudioClip_SO : ScriptableObject
{
    public AudioClip[] audioClips;
    public AudioMixerGroup audioOutput;
    private AudioClip previousClip;

    private GameObject audioObject;
    private AudioSource audioSource;

    public bool playOneShot;
    public bool loop;

    [Range(0f, 1f)]
    public float volume = 0.5f;
    [Range(0f, 3f)]
    public float pitch = 1f;

    // Plays the audio clip within unity editor
    public void PlayInEditor(AudioSource audioSource)
    {
        if (audioClips.Length == 0) return;
        AudioClip audioClip = (audioClips.Length > 1) ? ReturnRandomClip() : audioClips[0];
      
        audioSource.clip = audioClip;
        audioSource.outputAudioMixerGroup = audioOutput;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.Play();
    }

    // Stops playing the audio clip within unity editor
    public void StopInEditor(AudioSource audioSource) => audioSource.Stop();

    // Plays the audio clip
    public void Play()
    {
        if (audioClips.Length == 0) return;
        AudioClip audioClip = (audioClips.Length > 1) ? ReturnRandomClip() : audioClips[0];

        if (audioObject == null)
        {
            audioObject = new GameObject(name);
            audioObject.transform.parent = AudioManager.instance.transform;
            audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            LoopAudioCheck(audioClip);
            AudioManager.instance.AddAudioSourceCheck(audioSource);
        }
        else if (!audioObject.activeInHierarchy) audioObject.SetActive(true);

        PlayAudioCheck(audioClip);
        SetInactiveCheck(audioClip);
    }

    // Checks to play the audio clip via Play() or PlayOneShot()
    // Note: PlayOneShot() should be used for sfx that play frequently, Play() otherwise
    private void PlayAudioCheck(AudioClip audioClip)
    {
        if (!playOneShot && audioSource.isPlaying) audioSource.Stop();

        audioSource.outputAudioMixerGroup = audioOutput;
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;

        if (playOneShot) audioSource.PlayOneShot(audioClip);
        else audioSource.Play();
    }

    // Checks to loop the audio clip
    private void LoopAudioCheck(AudioClip audioClip)
    {
        if (!loop || audioClip.name == "CharNoise") return;

        audioSource.loop = true;
    }

    // Checks to set the audio object inactive after a duration
    private void SetInactiveCheck(AudioClip audioClip)
    {
        if (loop || audioClip.name == "CharNoise") return;

        AudioManager.instance.StartSetInactiveCoroutine(audioSource, audioClip.length);
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

    // Fades the audio IN
    // Note: the float parameters will use their default values if they are NOT set
    public void FadeInAudio(float? sv = null, float? ev = null, float? d = null)
    {
        if (audioSource == null || !audioObject.activeInHierarchy) Play();

        float startVolume = sv ?? 0f;
        float endVolume = ev ?? volume;
        float duration = d ?? TransitionFade.instance.gameFadeIn;

        AudioManager.instance.StartLerpAudioCoroutine(audioSource, startVolume, endVolume, duration);
    }

    // Fades the audio OUT
    // Note: the float parameters will use their default values if they are NOT set
    public void FadeOutAudio(float? sv = null, float? ev = null, float? d = null)
    {
        if (audioSource == null || !audioObject.activeInHierarchy) Play();

        float startVolume = sv ?? volume;
        float endVolume = ev ?? 0f;
        float duration = d ?? TransitionFade.instance.gameFadeOut;

        AudioManager.instance.StartLerpAudioCoroutine(audioSource, startVolume, endVolume, duration);
    }

}
