using UnityEngine;
using UnityEngine.Events;

public class SanitySystem : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Maximum sanity value (100 = completely calm).")]
    public float maxSanity = 100f;
    [Tooltip("Current sanity value.")]
    public float currentSanity;
    [Tooltip("If true, sanity decreases automatically over time.")]
    public bool decreaseOverTime = false;
    [Tooltip("How much sanity is lost per second when decreasing over time.")]
    public float decayRate = 1.0f;

    [Header("Events")]
    [Tooltip("Event invoked when sanity changes. Passes the current stress level (0.0 to 1.0), where 1.0 is max stress.")]
    public UnityEvent<float> OnStressLevelChanged;

    void Start()
    {
        currentSanity = maxSanity;
    }

    void Update()
    {
        if (decreaseOverTime)
        {
            ModifySanity(-decayRate * Time.deltaTime);
        }
    }

    /// <summary>
    /// Modify sanity by amount. Use negative values to damage sanity.
    /// </summary>
    public void ModifySanity(float amount)
    {
        currentSanity = Mathf.Clamp(currentSanity + amount, 0f, maxSanity);
        
        // Calculate Stress Level (Inverse of Sanity fraction)
        // Sanity 100 -> Stress 0
        // Sanity 0   -> Stress 1
        float stressLevel = 1.0f - (currentSanity / maxSanity);
        
        OnStressLevelChanged?.Invoke(stressLevel);
    }

    /// <summary>
    /// Set exact sanity value.
    /// </summary>
    public void SetSanity(float value)
    {
        currentSanity = Mathf.Clamp(value, 0f, maxSanity);
        float stressLevel = 1.0f - (currentSanity / maxSanity);
        OnStressLevelChanged?.Invoke(stressLevel);
    }
}

