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
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float notificationVolume = 0.7f;
    [Range(0f, 1f)]
    public float phoneOpenVolume = 0.5f;
    [Range(0f, 1f)]
    public float backgroundVolume = 0.3f;
    
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
        uiAudioSource.spatialBlend = 0f; // 2D sound
        
        // Setup background audio source
        if (backgroundAudioSource == null)
        {
            backgroundAudioSource = gameObject.AddComponent<AudioSource>();
        }
        backgroundAudioSource.playOnAwake = false;
        backgroundAudioSource.loop = true;
        backgroundAudioSource.spatialBlend = 0f; // 2D sound
        backgroundAudioSource.volume = backgroundVolume;
        
        // Start background music
        if (backgroundMusic != null)
        {
            backgroundAudioSource.clip = backgroundMusic;
            backgroundAudioSource.Play();
            Debug.Log("Background music started");
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
}