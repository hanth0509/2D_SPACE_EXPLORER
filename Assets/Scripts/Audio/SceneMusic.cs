using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SceneMusic : MonoBehaviour
{
    public AudioClip musicClip; // gán clip trong Inspector
    private AudioSource audioSource;

    void Awake()
    {
        // Lấy component AudioSource
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null && musicClip != null)
        {
            audioSource.clip = musicClip; // gán clip
            audioSource.Play();            // phát nhạc
            Debug.Log("Playing music: " + musicClip.name);
        }
        else
        {
            Debug.LogWarning("AudioSource hoặc AudioClip chưa được gán!");
        }
    }
}
