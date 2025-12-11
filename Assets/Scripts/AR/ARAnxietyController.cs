using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

/// <summary>
/// AR-specific Anxiety Effect Controller
/// Creates and manages post-processing effects at runtime for AR mode.
/// Uses MULTIPLE UI-based overlays for MAXIMUM VISIBILITY.
/// </summary>
public class ARAnxietyController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the SanitySystem. Will auto-find if null.")]
    public SanitySystem sanitySystem;
    [Tooltip("Reference to the AR Camera. Will auto-find if null.")]
    public Camera arCamera;

    [Header("Effect Intensities")]
    [Tooltip("Max Vignette intensity at max stress.")]
    public float maxVignetteIntensity = 0.7f;
    [Tooltip("Max Chromatic Aberration intensity at max stress.")]
    public float maxAberration = 1.0f;
    [Tooltip("Max Film Grain intensity at max stress.")]
    public float maxGrain = 0.8f;
    [Tooltip("Max screen darkness overlay (0-1).")]
    public float maxDarknessOverlay = 0.7f;  // Increased for visibility

    [Header("Pulse Settings")]
    [Tooltip("Base speed for the heartbeat pulse effect.")]
    public float basePulseSpeed = 1.0f;
    [Tooltip("Max pulse speed multiplier at high stress.")]
    public float maxPulseSpeedMultiplier = 4.0f;

    [Header("UI Overlay Settings (Fallback)")]
    [Tooltip("Enable UI-based overlay as fallback/supplement.")]
    public bool useUIOverlay = true;
    [Tooltip("Color of the stress overlay.")]
    public Color overlayColor = new Color(0.15f, 0f, 0f, 0.7f);  // More visible red
    [Tooltip("Enable vignette via UI (works if post-processing fails).")]
    public bool useUIVignette = true;

    [Header("Audio")]
    [Tooltip("Audio source for tension sounds.")]
    public AudioSource tensionAudioSource;
    [Tooltip("Max volume for tension audio.")]
    public float maxAudioVolume = 0.8f;

    [Header("Debug")]
    public bool debugMode = true;
    public float currentStress = 0f;

    // Runtime created components
    private Volume runtimeVolume;
    private VolumeProfile runtimeProfile;
    private Vignette vignette;
    private ChromaticAberration aberration;
    private FilmGrain grain;

    // UI Overlay - multiple layers for stronger effect
    private Canvas overlayCanvas;
    private Image overlayImage;        // Full screen darkness
    private Image vignetteImage;       // UI-based vignette
    private Texture2D vignetteTexture; // For UI vignette

    private float debugTimer = 0f;

    void Start()
    {
        // Find SanitySystem
        if (sanitySystem == null)
        {
            sanitySystem = FindObjectOfType<SanitySystem>();
        }

        // Find AR Camera
        if (arCamera == null)
        {
            arCamera = Camera.main;
        }

        if (sanitySystem == null)
        {
            Debug.LogError("ARAnxietyController: SanitySystem not found!");
            return;
        }

        // Subscribe to sanity changes
        sanitySystem.OnStressLevelChanged.AddListener(OnStressChanged);

        // Setup effects
        SetupPostProcessing();

        if (useUIOverlay)
        {
            SetupUIOverlay();
        }

        // Setup audio
        SetupAudio();

        Debug.Log("ARAnxietyController: Initialized with ENHANCED effects!");
    }

    void Update()
    {
        ApplyEffects();

        // Debug output
        if (debugMode)
        {
            debugTimer += Time.deltaTime;
            if (debugTimer >= 2f)
            {
                debugTimer = 0f;
                Debug.Log($"ARAnxietyController: Stress={currentStress:P0}, Overlay Alpha={(overlayImage != null ? overlayImage.color.a : -1):F2}");
            }
        }
    }

    void OnStressChanged(float stressLevel)
    {
        currentStress = Mathf.Clamp01(stressLevel);
        
        if (debugMode && currentStress > 0.1f)
        {
            Debug.Log($"ARAnxietyController: Stress level changed to {currentStress:P0}");
        }
    }

    void SetupPostProcessing()
    {
        // Create a new Volume for AR at runtime
        GameObject volumeGO = new GameObject("AR_AnxietyVolume");
        volumeGO.transform.SetParent(transform);
        
        runtimeVolume = volumeGO.AddComponent<Volume>();
        runtimeVolume.isGlobal = true;
        runtimeVolume.priority = 100; // High priority to override other volumes

        // Create a runtime profile
        runtimeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
        runtimeVolume.profile = runtimeProfile;

        // Add Vignette
        vignette = runtimeProfile.Add<Vignette>(true);
        vignette.intensity.Override(0f);
        vignette.color.Override(new Color(0.3f, 0f, 0f, 1f)); // Dark red tint
        vignette.smoothness.Override(0.3f);  // Sharper edge

        // Add Chromatic Aberration
        aberration = runtimeProfile.Add<ChromaticAberration>(true);
        aberration.intensity.Override(0f);

        // Add Film Grain
        grain = runtimeProfile.Add<FilmGrain>(true);
        grain.type.Override(FilmGrainLookup.Medium3);
        grain.intensity.Override(0f);

        Debug.Log("ARAnxietyController: Runtime Post-Processing Volume created");
    }

    void SetupUIOverlay()
    {
        // Create overlay canvas
        GameObject canvasGO = new GameObject("AnxietyOverlay");
        canvasGO.transform.SetParent(transform);

        overlayCanvas = canvasGO.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 999; // On top of everything

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Create UI-based vignette first (behind overlay)
        if (useUIVignette)
        {
            CreateUIVignette(canvasGO);
        }

        // Create overlay image (darkness)
        GameObject imageGO = new GameObject("OverlayImage");
        imageGO.transform.SetParent(canvasGO.transform, false);

        overlayImage = imageGO.AddComponent<Image>();
        overlayImage.color = new Color(0, 0, 0, 0); // Start invisible
        overlayImage.raycastTarget = false;

        // Stretch to fill screen
        RectTransform rt = overlayImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        Debug.Log("ARAnxietyController: UI Overlay created with vignette");
    }

    void CreateUIVignette(GameObject parent)
    {
        // Create a radial gradient texture for vignette effect
        int size = 512;
        vignetteTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float maxDist = size / 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                // Create smooth falloff from center to edge
                float alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((dist - 0.3f) / 0.7f));
                vignetteTexture.SetPixel(x, y, new Color(0.2f, 0, 0, alpha));
            }
        }
        vignetteTexture.Apply();

        // Create image using texture
        GameObject vigGO = new GameObject("UIVignette");
        vigGO.transform.SetParent(parent.transform, false);
        vigGO.transform.SetAsFirstSibling(); // Behind other overlay

        vignetteImage = vigGO.AddComponent<Image>();
        vignetteImage.sprite = Sprite.Create(vignetteTexture, new Rect(0, 0, size, size), Vector2.one * 0.5f);
        vignetteImage.color = new Color(1, 1, 1, 0); // Start invisible
        vignetteImage.raycastTarget = false;

        // Stretch to fill screen
        RectTransform rt = vignetteImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    void SetupAudio()
    {
        if (tensionAudioSource != null)
        {
            tensionAudioSource.loop = true;
            tensionAudioSource.volume = 0f;
            if (!tensionAudioSource.isPlaying && tensionAudioSource.clip != null)
            {
                tensionAudioSource.Play();
            }
        }
    }

    void ApplyEffects()
    {
        // Calculate pulse for heartbeat effect
        float pulseSpeed = basePulseSpeed + (currentStress * maxPulseSpeedMultiplier);
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        // Apply Post-Processing
        if (vignette != null)
        {
            float pulseMagnitude = currentStress * 0.2f;
            float targetIntensity = Mathf.Lerp(0f, maxVignetteIntensity, currentStress);
            vignette.intensity.Override(targetIntensity + (pulse * pulseMagnitude));
        }

        if (aberration != null)
        {
            aberration.intensity.Override(Mathf.Lerp(0f, maxAberration, currentStress));
        }

        if (grain != null)
        {
            grain.intensity.Override(Mathf.Lerp(0f, maxGrain, currentStress));
        }

        // Apply UI Overlay (darkness effect) - MORE AGGRESSIVE
        if (useUIOverlay && overlayImage != null)
        {
            float darkness = Mathf.Lerp(0f, maxDarknessOverlay, currentStress);
            // Add pulse to make it feel like a heartbeat
            float overlayPulse = pulse * currentStress * 0.15f;
            
            Color c = overlayColor;
            c.a = darkness + overlayPulse;
            overlayImage.color = c;
        }

        // Apply UI Vignette
        if (useUIVignette && vignetteImage != null)
        {
            float vignetteAlpha = Mathf.Lerp(0f, 0.8f, currentStress);
            float vignettePulse = pulse * currentStress * 0.2f;
            
            vignetteImage.color = new Color(1, 1, 1, vignetteAlpha + vignettePulse);
        }

        // Apply Audio
        if (tensionAudioSource != null)
        {
            tensionAudioSource.volume = Mathf.Lerp(0f, maxAudioVolume, currentStress);
        }
    }

    void OnDestroy()
    {
        // Cleanup
        if (sanitySystem != null)
        {
            sanitySystem.OnStressLevelChanged.RemoveListener(OnStressChanged);
        }

        if (runtimeProfile != null)
        {
            Destroy(runtimeProfile);
        }

        if (vignetteTexture != null)
        {
            Destroy(vignetteTexture);
        }
    }

    // Public method to manually test effect
    [ContextMenu("Test Max Stress")]
    public void TestMaxStress()
    {
        OnStressChanged(1.0f);
    }

    [ContextMenu("Test No Stress")]
    public void TestNoStress()
    {
        OnStressChanged(0f);
    }
}

