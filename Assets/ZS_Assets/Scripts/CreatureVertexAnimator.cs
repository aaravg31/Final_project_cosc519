using UnityEngine;

/// <summary>
/// Blue Creature Animation Controller - Controls animation effects through Shader parameters
/// </summary>
public class CreatureVertexAnimator : MonoBehaviour
{
    [Header("Wave Parameters")]
    [SerializeField] private float waveSpeed = 2f;
    [SerializeField] private float waveAmplitude = 0.1f;
    [SerializeField] private float waveFrequency = 3f;
    [SerializeField] private bool animateOnStart = true;

    [Header("Energy Flow")]
    [SerializeField] private float energyFlowSpeed = 1f;

    private Material creatureMaterial;
    private bool isAnimating = false;
    private bool isInitialized = false;

    // Shader property IDs (performance optimization)
    private static readonly int WaveSpeedID = Shader.PropertyToID("_WaveSpeed");
    private static readonly int WaveAmplitudeID = Shader.PropertyToID("_WaveAmplitude");
    private static readonly int WaveFrequencyID = Shader.PropertyToID("_WaveFrequency");
    private static readonly int EnergyFlowSpeedID = Shader.PropertyToID("_EnergyFlowSpeed");

    void Start()
    {
        InitializeMaterial();
        if (animateOnStart)
            StartAnimation();
    }

    void InitializeMaterial()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning("CreatureVertexAnimator: Renderer component not found!");
            return;
        }

        // Get material instance
        creatureMaterial = renderer.material;
        
        if (creatureMaterial == null)
        {
            Debug.LogWarning("CreatureVertexAnimator: Material not found!");
            return;
        }

        // Check if material has required properties
        if (!creatureMaterial.HasProperty(WaveSpeedID))
        {
            Debug.LogWarning("CreatureVertexAnimator: Material does not support animation properties! Please use BlueCreatureURP Shader.");
            return;
        }

        isInitialized = true;
        UpdateShaderParameters();
        Debug.Log("CreatureVertexAnimator: Initialized");
    }

    void UpdateShaderParameters()
    {
        if (!isInitialized || creatureMaterial == null)
            return;

        creatureMaterial.SetFloat(WaveSpeedID, waveSpeed);
        creatureMaterial.SetFloat(WaveAmplitudeID, waveAmplitude);
        creatureMaterial.SetFloat(WaveFrequencyID, waveFrequency);
        creatureMaterial.SetFloat(EnergyFlowSpeedID, energyFlowSpeed);
    }

    public void StartAnimation()
    {
        if (!isInitialized)
        {
            InitializeMaterial();
        }
        
        if (isInitialized)
        {
            isAnimating = true;
            UpdateShaderParameters();
            Debug.Log("CreatureVertexAnimator: Animation started");
        }
        else
        {
            Debug.LogWarning("CreatureVertexAnimator: Cannot start animation, initialization failed");
        }
    }

    public void StopAnimation()
    {
        isAnimating = false;
        
        if (creatureMaterial != null)
        {
            // Set parameters to 0 when stopping animation
            creatureMaterial.SetFloat(WaveSpeedID, 0f);
            creatureMaterial.SetFloat(WaveAmplitudeID, 0f);
            creatureMaterial.SetFloat(EnergyFlowSpeedID, 0f);
        }
    }

    public void SetAnimationIntensity(float intensity)
    {
        waveAmplitude = intensity * 0.2f;
        energyFlowSpeed = intensity;
        
        if (isAnimating)
        {
            UpdateShaderParameters();
        }
    }

    public void SetWaveSpeed(float speed)
    {
        waveSpeed = speed;
        if (isAnimating && creatureMaterial != null)
        {
            creatureMaterial.SetFloat(WaveSpeedID, waveSpeed);
        }
    }

    public void SetWaveAmplitude(float amplitude)
    {
        waveAmplitude = amplitude;
        if (isAnimating && creatureMaterial != null)
        {
            creatureMaterial.SetFloat(WaveAmplitudeID, waveAmplitude);
        }
    }

    public bool IsAnimating()
    {
        return isAnimating;
    }

    void OnDestroy()
    {
        // Clean up material instance
        if (creatureMaterial != null)
        {
            if (Application.isPlaying)
                Destroy(creatureMaterial);
            else
                DestroyImmediate(creatureMaterial);
        }
    }

    // Real-time update in editor
    void OnValidate()
    {
        if (Application.isPlaying && isInitialized)
        {
            UpdateShaderParameters();
        }
    }
}
