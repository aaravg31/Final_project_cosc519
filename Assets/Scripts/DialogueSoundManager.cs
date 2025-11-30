using UnityEngine;
using System.Collections;

public class DialogueSoundManager : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource dialogueAudioSource;
    
    [Header("Default Settings")]
    public float defaultWaitTime = 5f; // Wait time if no audio clip
    
    private static DialogueSoundManager _instance;
    public static DialogueSoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DialogueSoundManager>();
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
        if (dialogueAudioSource == null)
        {
            dialogueAudioSource = GetComponent<AudioSource>();
            
            if (dialogueAudioSource == null)
            {
                Debug.LogError("DialogueSoundManager: No AudioSource found! Adding one...");
                dialogueAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Configure audio source
        dialogueAudioSource.playOnAwake = false;
        dialogueAudioSource.spatialBlend = 0f; // 2D sound (not 3D spatial)
    }

    /// <summary>
    /// Play an audio clip. Returns the duration (clip length or default wait time).
    /// </summary>
    public float PlayDialogueClip(AudioClip clip)
    {
        if (clip != null)
        {
            dialogueAudioSource.Stop();
            dialogueAudioSource.clip = clip;
            dialogueAudioSource.Play();
            
            Debug.Log($"Playing audio clip: {clip.name}, Duration: {clip.length}s");
            return clip.length;
        }
        else
        {
            Debug.Log($"No audio clip assigned, using default wait time: {defaultWaitTime}s");
            return defaultWaitTime;
        }
    }

    /// <summary>
    /// Stop currently playing audio
    /// </summary>
    public void StopAudio()
    {
        if (dialogueAudioSource != null)
        {
            dialogueAudioSource.Stop();
        }
    }

    /// <summary>
    /// Check if audio is currently playing
    /// </summary>
    public bool IsPlaying()
    {
        return dialogueAudioSource != null && dialogueAudioSource.isPlaying;
    }
}