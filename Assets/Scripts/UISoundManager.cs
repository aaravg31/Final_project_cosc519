using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource uiAudioSource;
    public AudioSource backgroundAudioSource;
    
    [Header("UI Sound Effects")]
    public AudioClip notificationSound;
    public AudioClip phoneOpenSound;
    
    [Header("Background Music")]
    public AudioClip backgroundMusic;
    
    [Header("Intro Audio")]
    public AudioClip introAudio;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float notificationVolume = 0.7f;
    [Range(0f, 1f)]
    public float phoneOpenVolume = 0.5f;
    [Range(0f, 1f)]
    public float backgroundVolume = 0.3f;
    [Range(0f, 1f)]
    public float backgroundVolumeWhenAnxious = 0.05f; // Very low when anxious
    
    private static UISoundManager _instance;
    public static UISoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<UISoundManager>();
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Setup UI audio source
        if (uiAudioSource == null)
        {
            uiAudioSource = gameObject.AddComponent<AudioSource>();
        }
        uiAudioSource.playOnAwake = false;
        uiAudioSource.spatialBlend = 0f;
        
        // Setup background audio source
        if (backgroundAudioSource == null)
        {
            backgroundAudioSource = gameObject.AddComponent<AudioSource>();
        }
        backgroundAudioSource.playOnAwake = false;
        backgroundAudioSource.loop = true;
        backgroundAudioSource.spatialBlend = 0f;
        backgroundAudioSource.volume = 0f; // Start at 0 for intro
        
        // Start background music but muted
        if (backgroundMusic != null)
        {
            backgroundAudioSource.clip = backgroundMusic;
            backgroundAudioSource.Play();
            Debug.Log("Background music started (muted for intro)");
        }
    }

    public void PlayNotificationSound()
    {
        if (notificationSound != null)
        {
            uiAudioSource.PlayOneShot(notificationSound, notificationVolume);
            Debug.Log("Playing notification sound");
        }
    }

    public void PlayPhoneOpenSound()
    {
        if (phoneOpenSound != null)
        {
            uiAudioSource.PlayOneShot(phoneOpenSound, phoneOpenVolume);
            Debug.Log("Playing phone open sound");
        }
    }
    
    /// <summary>
    /// Play a custom one-shot audio clip
    /// </summary>
    public void PlayCustomClip(AudioClip clip, float volume = 0.7f)
    {
        if (clip != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(clip, volume);
            Debug.Log($"Playing custom clip: {clip.name}");
        }
    }
    
    /// <summary>
    /// Play intro audio and return its duration
    /// </summary>
    public float PlayIntroAudio()
    {
        if (introAudio != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(introAudio, 1f);
            Debug.Log($"Playing intro audio: {introAudio.name}, Duration: {introAudio.length}s");
            return introAudio.length;
        }
        return 0f;
    }
    
    /// <summary>
    /// Fade background music to normal volume
    /// </summary>
    public void FadeBackgroundToNormal(float duration = 2f)
    {
        StopAllCoroutines();
        StartCoroutine(FadeBackgroundVolume(backgroundVolume, duration));
    }
    
    /// <summary>
    /// Fade background music to anxious (very low) volume
    /// </summary>
    public void FadeBackgroundToAnxious(float duration = 2f)
    {
        StopAllCoroutines();
        StartCoroutine(FadeBackgroundVolume(backgroundVolumeWhenAnxious, duration));
    }
    
    /// <summary>
    /// Fade background to specific volume
    /// </summary>
    public void FadeBackgroundTo(float targetVolume, float duration = 2f)
    {
        StopAllCoroutines();
        StartCoroutine(FadeBackgroundVolume(targetVolume, duration));
    }
    
    private System.Collections.IEnumerator FadeBackgroundVolume(float targetVolume, float duration)
    {
        if (backgroundAudioSource == null) yield break;
        
        float startVolume = backgroundAudioSource.volume;
        float elapsed = 0f;
        
        Debug.Log($"Fading background music from {startVolume} to {targetVolume} over {duration}s");
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            backgroundAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }
        
        backgroundAudioSource.volume = targetVolume;
        Debug.Log($"Background music fade complete: {targetVolume}");
    }
}