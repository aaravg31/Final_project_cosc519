using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AnxietyEffectController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Global Volume containing URP Post-Processing overrides.")]
    public Volume globalVolume;
    [Tooltip("Camera to apply shake effect (preferably a container, not the tracked HMD itself).")]
    public Transform cameraShakeTransform;
    [Tooltip("Audio Source to play tension sound (looping).")]
    public AudioSource tensionAudioSource;

    [Header("Effect Intensities (Max Stress)")]
    [Tooltip("Max Vignette intensity (0-1).")]
    public float maxVignetteIntensity = 0.55f;
    [Tooltip("Max Chromatic Aberration intensity (0-1).")]
    public float maxAberration = 1.0f;
    [Tooltip("Max Film Grain intensity (0-1).")]
    public float maxGrain = 1.0f;
    [Tooltip("Max Camera Shake rotation angle (degrees).")]
    public float maxShakeAngle = 1.0f;
    [Tooltip("Max volume for tension audio (0-1).")]
    public float maxAudioVolume = 1.0f;
    
    [Header("Pulse Settings")]
    [Tooltip("Base pulse speed for heartbeat effect.")]
    public float basePulseSpeed = 1.0f;
    [Tooltip("Multiplier for pulse speed at high stress.")]
    public float maxPulseSpeedMultiplier = 4.0f;

    // Cached Overrides
    private Vignette vignette;
    private ChromaticAberration aberration;
    private FilmGrain grain;
    private LensDistortion distortion;
    
    // State
    public float currentStress = 0f; // 0 to 1 (Public for debug/inspector)
    private Quaternion initialRotation;

    void Start()
    {
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out vignette);
            globalVolume.profile.TryGet(out aberration);
            globalVolume.profile.TryGet(out grain);
            globalVolume.profile.TryGet(out distortion);
        }

        if (cameraShakeTransform != null)
        {
            initialRotation = cameraShakeTransform.localRotation;
        }
        
        // Initialize Audio
        if (tensionAudioSource != null)
        {
            tensionAudioSource.loop = true;
            tensionAudioSource.volume = 0f;
            if (!tensionAudioSource.isPlaying)
                tensionAudioSource.Play();
        }
    }

    void Update()
    {
        ApplyEffects();
    }

    /// <summary>
    /// Called by SanitySystem event.
    /// </summary>
    /// <param name="stressLevel">0.0 (Calm) to 1.0 (Max Stress)</param>
    public void SetStressLevel(float stressLevel)
    {
        currentStress = Mathf.Clamp01(stressLevel);
    }

    // Debug method to verify event connection
    public void TestSetStressFull()
    {
        SetStressLevel(1.0f);
    }

    void ApplyEffects()
    {
        // 1. Pulse Logic (Heartbeat)
        // Speed increases with stress
        float pulseSpeed = basePulseSpeed + (currentStress * maxPulseSpeedMultiplier);
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0 to 1 wave

        // 2. Vignette
        if (vignette != null)
        {
            // Base intensity + Pulse effect
            // Pulse effect only becomes strong at high stress
            float pulseMagnitude = currentStress * 0.2f; // up to 0.2 variance
            float targetIntensity = Mathf.Lerp(0.2f, maxVignetteIntensity, currentStress);
            
            vignette.intensity.value = targetIntensity + (pulse * pulseMagnitude);
            vignette.active = vignette.intensity.value > 0.01f;
        }

        // 3. Chromatic Aberration (Disorientation)
        if (aberration != null)
        {
            aberration.intensity.value = Mathf.Lerp(0f, maxAberration, currentStress);
            aberration.active = aberration.intensity.value > 0.01f;
        }

        // 4. Film Grain (Visual Noise)
        if (grain != null)
        {
            grain.intensity.value = Mathf.Lerp(0f, maxGrain, currentStress);
            grain.active = grain.intensity.value > 0.01f;
        }

        // 5. Camera Shake (Subtle Rotation)
        if (cameraShakeTransform != null && currentStress > 0.1f)
        {
            // Perlin noise for smooth randomness
            float noiseSpeed = 10f;
            float shakeAmount = currentStress * maxShakeAngle;
            
            float x = (Mathf.PerlinNoise(Time.time * noiseSpeed, 0) - 0.5f) * shakeAmount;
            float y = (Mathf.PerlinNoise(0, Time.time * noiseSpeed) - 0.5f) * shakeAmount;
            float z = (Mathf.PerlinNoise(Time.time * noiseSpeed, Time.time * noiseSpeed) - 0.5f) * shakeAmount * 0.5f; // Less roll

            cameraShakeTransform.localRotation = initialRotation * Quaternion.Euler(x, y, z);
        }
        else if (cameraShakeTransform != null)
        {
            // Return to normal smoothly
            cameraShakeTransform.localRotation = Quaternion.Lerp(cameraShakeTransform.localRotation, initialRotation, Time.deltaTime * 2f);
        }
        
        // 6. Audio Tension
        if (tensionAudioSource != null)
        {
            // Lerp volume based on stress
            tensionAudioSource.volume = Mathf.Lerp(0f, maxAudioVolume, currentStress);
            
            // Optional: Adjust pitch based on pulse speed for more frantic feeling
            // tensionAudioSource.pitch = Mathf.Lerp(0.8f, 1.2f, currentStress);
        }
    }
}
