using UnityEngine;

/// <summary>
/// AR Dark Particles Effect - Creates dark floating particles when stress is high
/// Adds to the atmosphere of anxiety/depression
/// </summary>
public class ARDarkParticles : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to SanitySystem. Will auto-find if null.")]
    public SanitySystem sanitySystem;
    [Tooltip("Camera to attach particles to.")]
    public Transform arCamera;

    [Header("Particle Settings")]
    [Tooltip("Custom particle material. If null, will auto-create.")]
    public Material customMaterial;
    [Tooltip("Max number of particles at full stress.")]
    public int maxParticles = 50;
    [Tooltip("Particle size range.")]
    public Vector2 sizeRange = new Vector2(0.02f, 0.08f);
    [Tooltip("How far from camera particles spawn.")]
    public float spawnDistance = 3f;
    [Tooltip("Particle color at high stress.")]
    public Color particleColor = new Color(0.1f, 0.05f, 0.15f, 0.6f);

    [Header("Stress Response")]
    [Tooltip("Minimum stress level to start showing particles (0-1).")]
    public float minStressThreshold = 0.3f;

    private ParticleSystem particles;
    private ParticleSystem.EmissionModule emission;
    private float currentStress = 0f;

    void Start()
    {
        if (sanitySystem == null)
        {
            sanitySystem = FindObjectOfType<SanitySystem>();
        }

        if (arCamera == null && Camera.main != null)
        {
            arCamera = Camera.main.transform;
        }

        if (sanitySystem != null)
        {
            sanitySystem.OnStressLevelChanged.AddListener(OnStressChanged);
        }

        CreateParticleSystem();
    }

    void OnStressChanged(float stressLevel)
    {
        currentStress = stressLevel;
        UpdateParticleEmission();
    }

    void CreateParticleSystem()
    {
        GameObject particleGO = new GameObject("DarkAtmosphereParticles");
        
        // Parent to camera so particles follow player
        if (arCamera != null)
        {
            particleGO.transform.SetParent(arCamera);
            particleGO.transform.localPosition = Vector3.forward * spawnDistance;
        }
        else
        {
            particleGO.transform.SetParent(transform);
        }

        particles = particleGO.AddComponent<ParticleSystem>();

        // Main module
        var main = particles.main;
        main.loop = true;
        main.startLifetime = 5f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startSize = new ParticleSystem.MinMaxCurve(sizeRange.x, sizeRange.y);
        main.startColor = particleColor;
        main.maxParticles = maxParticles;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.02f; // Slight upward float

        // Emission - controlled by stress
        emission = particles.emission;
        emission.rateOverTime = 0; // Start with no particles

        // Shape - sphere around camera
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = spawnDistance;

        // Color over lifetime - fade in and out
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(particleColor, 0f),
                new GradientColorKey(particleColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(particleColor.a, 0.3f),
                new GradientAlphaKey(particleColor.a, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        // Noise for organic movement
        var noise = particles.noise;
        noise.enabled = true;
        noise.strength = 0.3f;
        noise.frequency = 0.5f;
        noise.scrollSpeed = 0.2f;

        // Size over lifetime - grow slightly
        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.5f);
        sizeCurve.AddKey(0.5f, 1f);
        sizeCurve.AddKey(1f, 0.3f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Renderer
        var renderer = particleGO.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial();

        Debug.Log("ARDarkParticles: Dark atmosphere particle system created");
    }

    Material CreateParticleMaterial()
    {
        if (customMaterial != null)
            return customMaterial;

        string[] shaderNames = new string[]
        {
            "Universal Render Pipeline/Particles/Unlit",
            "Particles/Standard Unlit",
            "Sprites/Default"
        };

        Shader shader = null;
        foreach (string name in shaderNames)
        {
            shader = Shader.Find(name);
            if (shader != null) break;
        }

        if (shader == null)
        {
            Debug.LogWarning("ARDarkParticles: No particle shader found");
            return null;
        }

        Material mat = new Material(shader);
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", particleColor);
        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", particleColor);
        
        mat.renderQueue = 3100;
        return mat;
    }

    void UpdateParticleEmission()
    {
        if (particles == null) return;

        // Calculate emission rate based on stress
        float stressAboveThreshold = Mathf.Max(0, currentStress - minStressThreshold) / (1f - minStressThreshold);
        float emissionRate = stressAboveThreshold * maxParticles * 0.5f;

        emission.rateOverTime = emissionRate;

        // Also adjust particle color intensity
        var main = particles.main;
        Color adjustedColor = particleColor;
        adjustedColor.a = particleColor.a * stressAboveThreshold;
        main.startColor = adjustedColor;
    }

    void Update()
    {
        // Keep particles centered on camera
        if (arCamera != null && particles != null)
        {
            particles.transform.position = arCamera.position + arCamera.forward * (spawnDistance * 0.5f);
        }
    }

    void OnDestroy()
    {
        if (sanitySystem != null)
        {
            sanitySystem.OnStressLevelChanged.RemoveListener(OnStressChanged);
        }
    }
}
