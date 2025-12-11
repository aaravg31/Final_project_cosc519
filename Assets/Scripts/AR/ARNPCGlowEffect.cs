using UnityEngine;

/// <summary>
/// AR NPC Enhanced Glow Effect
/// Creates visible light rings, particles, and pulsing effects around NPC.
/// Designed to be visually appealing in AR camera view.
/// </summary>
public class ARNPCGlowEffect : MonoBehaviour
{
    [Header("Light Ring Settings")]
    [Tooltip("Enable light ring around NPC.")]
    public bool enableLightRing = true;
    [Tooltip("Light ring color.")]
    public Color lightRingColor = new Color(0.5f, 0.8f, 1f, 1f);
    [Tooltip("Light ring radius.")]
    public float lightRingRadius = 1.5f;
    [Tooltip("Light ring intensity.")]
    public float lightRingIntensity = 2f;
    [Tooltip("Pulse speed for light ring.")]
    public float pulseSpeed = 1.5f;
    [Tooltip("Pulse amount (0-1).")]
    public float pulseAmount = 0.3f;

    [Header("Point Light Settings")]
    [Tooltip("Enable point light at NPC center.")]
    public bool enablePointLight = true;
    [Tooltip("Point light intensity.")]
    public float pointLightIntensity = 3f;
    [Tooltip("Point light range.")]
    public float pointLightRange = 5f;

    [Header("Particle Settings")]
    [Tooltip("Enable floating particles.")]
    public bool enableParticles = true;
    [Tooltip("Particle color.")]
    public Color particleColor = new Color(0.7f, 0.9f, 1f, 0.8f);
    [Tooltip("Number of particles.")]
    public int particleCount = 20;
    [Tooltip("Custom particle material. If null, will auto-create. Drag a material here to use your own.")]
    public Material customParticleMaterial;

    [Header("Distance Response")]
    [Tooltip("React to player distance.")]
    public bool reactToDistance = true;
    [Tooltip("Min distance for max glow.")]
    public float minGlowDistance = 1f;
    [Tooltip("Max distance for min glow.")]
    public float maxGlowDistance = 5f;

    [Header("Debug")]
    public bool debugMode = false;

    private Light pointLight;
    private Light[] ringLights;
    private ParticleSystem particles;
    private Transform playerCamera;
    private float baseIntensity;
    private float currentGlowMultiplier = 1f;

    void Start()
    {
        // Find player camera
        if (Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        SetupPointLight();
        
        if (enableLightRing)
        {
            SetupLightRing();
        }
        
        if (enableParticles)
        {
            SetupParticles();
        }

        baseIntensity = pointLightIntensity;
    }

    void Update()
    {
        UpdatePulse();
        
        if (reactToDistance && playerCamera != null)
        {
            UpdateDistanceResponse();
        }
    }

    void SetupPointLight()
    {
        if (!enablePointLight) return;

        // Check for existing light on NPC
        pointLight = GetComponentInChildren<Light>();
        
        if (pointLight == null)
        {
            GameObject lightGO = new GameObject("NPCGlowLight");
            lightGO.transform.SetParent(transform);
            lightGO.transform.localPosition = new Vector3(0, 0.5f, 0);
            
            pointLight = lightGO.AddComponent<Light>();
        }

        pointLight.type = LightType.Point;
        pointLight.color = lightRingColor;
        pointLight.intensity = pointLightIntensity;
        pointLight.range = pointLightRange;
        pointLight.shadows = LightShadows.None; // Better performance

        if (debugMode)
            Debug.Log($"ARNPCGlowEffect: Point light created with intensity {pointLightIntensity}");
    }

    void SetupLightRing()
    {
        // Create multiple point lights in a ring for a halo effect
        int ringLightCount = 6;
        ringLights = new Light[ringLightCount];

        GameObject ringParent = new GameObject("LightRing");
        ringParent.transform.SetParent(transform);
        ringParent.transform.localPosition = Vector3.zero;

        for (int i = 0; i < ringLightCount; i++)
        {
            float angle = (360f / ringLightCount) * i * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * lightRingRadius,
                0.2f, // Slightly above ground
                Mathf.Sin(angle) * lightRingRadius
            );

            GameObject lightGO = new GameObject($"RingLight_{i}");
            lightGO.transform.SetParent(ringParent.transform);
            lightGO.transform.localPosition = pos;

            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = lightRingColor;
            light.intensity = lightRingIntensity / ringLightCount;
            light.range = 2f;
            light.shadows = LightShadows.None;

            ringLights[i] = light;
        }

        if (debugMode)
            Debug.Log($"ARNPCGlowEffect: Light ring created with {ringLightCount} lights");
    }

    void SetupParticles()
    {
        GameObject particleGO = new GameObject("GlowParticles");
        particleGO.transform.SetParent(transform);
        particleGO.transform.localPosition = Vector3.zero;

        particles = particleGO.AddComponent<ParticleSystem>();
        
        // Main module
        var main = particles.main;
        main.loop = true;
        main.startLifetime = 3f;
        main.startSpeed = 0.3f;
        main.startSize = 0.1f;
        main.startColor = particleColor;
        main.maxParticles = particleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        // Emission
        var emission = particles.emission;
        emission.rateOverTime = particleCount / 3f;

        // Shape - sphere around NPC
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = lightRingRadius * 0.8f;

        // Size over lifetime - fade out
        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.5f);
        sizeCurve.AddKey(0.5f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Color over lifetime - fade alpha
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
                new GradientAlphaKey(1f, 0.2f), 
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = gradient;

        // Velocity - gentle upward float
        var velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = 0.5f;

        // Renderer - use custom material if provided, otherwise auto-create
        var renderer = particleGO.GetComponent<ParticleSystemRenderer>();
        
        if (customParticleMaterial != null)
        {
            // Use the user-provided material
            renderer.material = customParticleMaterial;
            Debug.Log("ARNPCGlowEffect: Using custom particle material");
        }
        else
        {
            // Auto-create material with shader fallback chain
            Material particleMaterial = CreateParticleMaterial();
            if (particleMaterial != null)
            {
                renderer.material = particleMaterial;
            }
            else
            {
                Debug.LogWarning("ARNPCGlowEffect: Could not create particle material, using default");
            }
        }

        if (debugMode)
            Debug.Log("ARNPCGlowEffect: Particle system created");
    }

    /// <summary>
    /// Creates a particle material with proper shader fallback
    /// </summary>
    Material CreateParticleMaterial()
    {
        // Try multiple shader names in order of preference
        string[] shaderNames = new string[]
        {
            "Universal Render Pipeline/Particles/Unlit",  // URP shader
            "Universal Render Pipeline/Particles/Simple Lit",
            "Particles/Standard Unlit",  // Built-in fallback
            "Sprites/Default",  // Ultimate fallback
            "Unlit/Color"
        };

        Shader particleShader = null;
        foreach (string shaderName in shaderNames)
        {
            particleShader = Shader.Find(shaderName);
            if (particleShader != null)
            {
                if (debugMode)
                    Debug.Log($"ARNPCGlowEffect: Using shader '{shaderName}'");
                break;
            }
        }

        if (particleShader == null)
        {
            Debug.LogError("ARNPCGlowEffect: No valid particle shader found!");
            return null;
        }

        Material mat = new Material(particleShader);
        
        // Set color properties based on what the shader supports
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", particleColor);
        }
        if (mat.HasProperty("_Color"))
        {
            mat.SetColor("_Color", particleColor);
        }
        if (mat.HasProperty("_TintColor"))
        {
            mat.SetColor("_TintColor", particleColor);
        }
        
        // Enable alpha blending
        if (mat.HasProperty("_Surface"))
        {
            mat.SetFloat("_Surface", 1); // Transparent
        }
        if (mat.HasProperty("_Blend"))
        {
            mat.SetFloat("_Blend", 1); // Additive or Alpha
        }
        
        // Set render queue for transparency
        mat.renderQueue = 3000;

        return mat;
    }

    void UpdatePulse()
    {
        // Create pulsing effect
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float intensityMultiplier = 1f - (pulse * pulseAmount);
        intensityMultiplier *= currentGlowMultiplier;

        // Apply to point light
        if (pointLight != null)
        {
            pointLight.intensity = baseIntensity * intensityMultiplier;
        }

        // Apply to ring lights
        if (ringLights != null)
        {
            float ringBase = lightRingIntensity / ringLights.Length;
            foreach (var light in ringLights)
            {
                if (light != null)
                {
                    light.intensity = ringBase * intensityMultiplier;
                }
            }
        }
    }

    void UpdateDistanceResponse()
    {
        if (playerCamera == null) return;

        float distance = Vector3.Distance(transform.position, playerCamera.position);
        
        // When player is close, increase glow. When far, decrease.
        // Inverse relationship: closer = brighter
        if (distance <= minGlowDistance)
        {
            currentGlowMultiplier = 1.5f; // Extra bright when very close
        }
        else if (distance >= maxGlowDistance)
        {
            currentGlowMultiplier = 0.5f; // Dimmer when far
        }
        else
        {
            // Lerp between close and far
            float t = (distance - minGlowDistance) / (maxGlowDistance - minGlowDistance);
            currentGlowMultiplier = Mathf.Lerp(2.0f, 0.3f, t); // More dramatic difference
        }
        
        // Update light range based on distance (expand when player is close)
        if (pointLight != null)
        {
            float targetRange = distance <= minGlowDistance ? pointLightRange * 1.5f : pointLightRange;
            pointLight.range = Mathf.Lerp(pointLight.range, targetRange, Time.deltaTime * 3f);
        }
        
        // Update particle emission rate based on proximity
        if (particles != null)
        {
            var emission = particles.emission;
            float emissionMultiplier = distance <= minGlowDistance ? 2f : 1f;
            emission.rateOverTime = (particleCount / 3f) * emissionMultiplier * currentGlowMultiplier;
        }
    }

    /// <summary>
    /// Set the glow color at runtime
    /// </summary>
    public void SetGlowColor(Color color)
    {
        lightRingColor = color;
        particleColor = new Color(color.r, color.g, color.b, 0.8f);

        if (pointLight != null)
        {
            pointLight.color = color;
        }

        if (ringLights != null)
        {
            foreach (var light in ringLights)
            {
                if (light != null)
                    light.color = color;
            }
        }

        if (particles != null)
        {
            var main = particles.main;
            main.startColor = particleColor;
        }
        
        // DON'T update safety zone colors - keep RED outside, GREEN inside
    }

    /// <summary>
    /// Set the player camera reference
    /// </summary>
    public void SetPlayerCamera(Transform camera)
    {
        playerCamera = camera;
    }
    
    /// <summary>
    /// Add safety zone visual to this NPC
    /// </summary>
    public void AddSafetyZone(float radius)
    {
        ARSafetyZone safetyZone = GetComponent<ARSafetyZone>();
        if (safetyZone == null)
        {
            safetyZone = gameObject.AddComponent<ARSafetyZone>();
        }
        safetyZone.SetRadius(radius);
        
        // DON'T override colors - keep default RED outside, GREEN inside
        // The ARSafetyZone component has correct default colors
    }

    void OnDrawGizmosSelected()
    {
        // Visualize light ring in editor
        Gizmos.color = lightRingColor;
        Gizmos.DrawWireSphere(transform.position, lightRingRadius);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minGlowDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxGlowDistance);
    }
}

