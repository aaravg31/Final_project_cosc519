using UnityEngine;

/// <summary>
/// Blue Creature Glow Controller - Controls CoreURP Shader glow effects and chest light point
/// </summary>
public class CreatureGlowController : MonoBehaviour
{
    [Header("Chest Light Settings")]
    [SerializeField] private Transform chestLightTransform;
    [SerializeField] private float chestLightIntensity = 2f;
    [SerializeField] private Color chestLightColor = Color.white;
    [SerializeField] private float chestPulseSpeed = 1.5f;
    [SerializeField] private float chestPulseAmount = 0.5f;

    [Header("Body Glow Settings (CoreURP)")]
    [SerializeField] private float emissionIntensity = 0.5f;
    [SerializeField] private float edgeGlow = 1.0f;

    [Header("Color Settings")]
    [SerializeField] private Color bodyColor = new Color(0.3f, 0.7f, 1f, 0.6f);
    [SerializeField] private float transparency = 0.4f;

    private Material bodyMaterial;
    private Light chestPointLight;
    private Renderer chestLightRenderer;
    private Material chestLightMaterial;
    private float baseChestIntensity;

    // CoreURP Shader property IDs (performance optimization)
    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private static readonly int TransparencyID = Shader.PropertyToID("_Transparency");
    private static readonly int EmissionIntensityID = Shader.PropertyToID("_EmissionIntensity");
    private static readonly int EdgeGlowID = Shader.PropertyToID("_EdgeGlow");
    private static readonly int WaveSpeedID = Shader.PropertyToID("_WaveSpeed");
    private static readonly int WaveAmountID = Shader.PropertyToID("_WaveAmount");
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

    void Start()
    {
        SetupBodyGlow();
        SetupChestLight();
        baseChestIntensity = chestLightIntensity;
    }

    void Update()
    {
        AnimateChestGlow();
    }

    void SetupBodyGlow()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning("CreatureGlowController: Renderer component not found!");
            return;
        }

        // Get material instance
        bodyMaterial = renderer.material;
        
        if (bodyMaterial == null)
        {
            Debug.LogWarning("CreatureGlowController: Material not found!");
            return;
        }


        // Set glow parameters
        UpdateBodyGlowParameters();
        Debug.Log("CreatureGlowController: Body glow has been set (CoreURP)");
    }

    void UpdateBodyGlowParameters()
    {
        if (bodyMaterial == null)
            return;

        // Set CoreURP Shader parameters
        if (bodyMaterial.HasProperty(ColorID))
        {
            bodyMaterial.SetColor(ColorID, bodyColor);
        }

        if (bodyMaterial.HasProperty(TransparencyID))
        {
            bodyMaterial.SetFloat(TransparencyID, transparency);
        }

        if (bodyMaterial.HasProperty(EmissionIntensityID))
        {
            bodyMaterial.SetFloat(EmissionIntensityID, emissionIntensity);
        }

        if (bodyMaterial.HasProperty(EdgeGlowID))
        {
            bodyMaterial.SetFloat(EdgeGlowID, edgeGlow);
        }
    }

    void SetupChestLight()
    {
        if (chestLightTransform == null)
        {
            // Create chest light point
            GameObject lightObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lightObject.name = "ChestLight";
            lightObject.transform.SetParent(transform);
            lightObject.transform.localPosition = new Vector3(0f, 0.3f, 0.05f); // Chest position
            lightObject.transform.localScale = Vector3.one * 0.08f;
            chestLightTransform = lightObject.transform;

            // Remove collider
            Collider collider = lightObject.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);
        }

        // Set light point material (glowing sphere)
        chestLightRenderer = chestLightTransform.GetComponent<Renderer>();
        if (chestLightRenderer != null)
        {
            // Use Unlit shader to make light point always visible
            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (unlitShader == null)
            {
                unlitShader = Shader.Find("Unlit/Color");
            }
            
            if (unlitShader != null)
            {
                chestLightMaterial = new Material(unlitShader);
                chestLightMaterial.SetColor(BaseColorID, chestLightColor);
                chestLightRenderer.material = chestLightMaterial;
            }
        }

        // Add point light
        chestPointLight = chestLightTransform.gameObject.GetComponent<Light>();
        if (chestPointLight == null)
        {
            chestPointLight = chestLightTransform.gameObject.AddComponent<Light>();
        }
        
        chestPointLight.type = LightType.Point;
        chestPointLight.color = chestLightColor;
        chestPointLight.intensity = chestLightIntensity;
        chestPointLight.range = 2f;
        chestPointLight.shadows = LightShadows.None; // VR performance optimization

        Debug.Log("CreatureGlowController: Chest light point has been created");
    }

    void AnimateChestGlow()
    {
        if (chestPointLight == null)
            return;

        // Pulse effect
        float pulse = Mathf.Sin(Time.time * chestPulseSpeed) * chestPulseAmount + 1f;
        chestPointLight.intensity = baseChestIntensity * pulse;

        // Update light point material color
        if (chestLightMaterial != null)
        {
            Color glowColor = chestLightColor * pulse;
            chestLightMaterial.SetColor(BaseColorID, glowColor);
        }
    }

    // Public methods: dynamically adjust glow intensity
    public void SetChestGlowIntensity(float intensity)
    {
        chestLightIntensity = intensity;
        baseChestIntensity = intensity;
        if (chestPointLight != null)
            chestPointLight.intensity = intensity;
    }

    public void SetBodyGlowIntensity(float intensity)
    {
        emissionIntensity = intensity;
        if (bodyMaterial != null && bodyMaterial.HasProperty(EmissionIntensityID))
        {
            bodyMaterial.SetFloat(EmissionIntensityID, intensity);
        }
    }

    public void SetEdgeGlow(float intensity)
    {
        edgeGlow = intensity;
        if (bodyMaterial != null && bodyMaterial.HasProperty(EdgeGlowID))
        {
            bodyMaterial.SetFloat(EdgeGlowID, intensity);
        }
    }

    public void SetBodyColor(Color newColor)
    {
        bodyColor = newColor;
        if (bodyMaterial != null && bodyMaterial.HasProperty(ColorID))
        {
            bodyMaterial.SetColor(ColorID, bodyColor);
        }
    }

    public void SetTransparency(float alpha)
    {
        transparency = alpha;
        if (bodyMaterial != null && bodyMaterial.HasProperty(TransparencyID))
        {
            bodyMaterial.SetFloat(TransparencyID, transparency);
        }
    }

    public void SetGlowColor(Color newColor)
    {
        chestLightColor = newColor;
        
        if (chestPointLight != null)
            chestPointLight.color = newColor;
    }

    // Control wave effect (if dynamic adjustment is needed)
    public void SetWaveSpeed(float speed)
    {
        if (bodyMaterial != null && bodyMaterial.HasProperty(WaveSpeedID))
        {
            bodyMaterial.SetFloat(WaveSpeedID, speed);
        }
    }

    public void SetWaveAmount(float amount)
    {
        if (bodyMaterial != null && bodyMaterial.HasProperty(WaveAmountID))
        {
            bodyMaterial.SetFloat(WaveAmountID, amount);
        }
    }

    void OnDestroy()
    {
        // Clean up material instances
        if (bodyMaterial != null)
        {
            if (Application.isPlaying)
                Destroy(bodyMaterial);
            else
                DestroyImmediate(bodyMaterial);
        }

        if (chestLightMaterial != null)
        {
            if (Application.isPlaying)
                Destroy(chestLightMaterial);
            else
                DestroyImmediate(chestLightMaterial);
        }
    }

    // Real-time update in editor
    void OnValidate()
    {
        if (Application.isPlaying && bodyMaterial != null)
        {
            UpdateBodyGlowParameters();
        }
    }
}
