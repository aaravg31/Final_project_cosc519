using UnityEngine;

public class InteractionRingController : MonoBehaviour
{
    [Header("Ring Settings")]
    [Tooltip("The ring GameObject (cylinder or other mesh)")]
    public GameObject ringObject;
    
    [Header("Appearance")]
    [Range(0f, 5f)]
    public float ringRadius = 1.5f;
    
    [Range(0f, 1f)]
    public float ringHeight = 0.1f;
    
    [Tooltip("Height offset from NPC's position (usually 0 for ground level)")]
    public float heightOffset = 0.05f;
    
    [Header("Animation")]
    public bool enableRotation = true;
    [Range(0f, 200f)]
    public float rotationSpeed = 30f;
    
    public bool enablePulse = true;
    [Range(0f, 5f)]
    public float pulseSpeed = 2f;
    [Range(0f, 0.5f)]
    public float pulseAmount = 0.1f;
    
    [Header("Material")]
    [Tooltip("Optional: Assign a yellow emissive material for the ring")]
    public Material ringMaterial;
    
    private Vector3 baseScale;
    private Renderer ringRenderer;

    void Start()
    {
        if (ringObject == null)
        {
            Debug.LogError($"InteractionRingController on {gameObject.name}: Ring Object is not assigned!");
            return;
        }
        
        // Position ring at NPC's feet
        ringObject.transform.SetParent(transform);
        ringObject.transform.localPosition = new Vector3(0, heightOffset, 0);
        ringObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
        
        // Set initial scale based on radius and height
        baseScale = new Vector3(ringRadius * 2, ringHeight, ringRadius * 2);
        ringObject.transform.localScale = baseScale;
        
        // Apply material if assigned
        ringRenderer = ringObject.GetComponent<Renderer>();
        if (ringRenderer != null && ringMaterial != null)
        {
            ringRenderer.material = ringMaterial;
        }
        
        // Hide ring initially
        ringObject.SetActive(false);
        
        Debug.Log($"Interaction ring initialized for {gameObject.name}");
    }

    void Update()
    {
        if (ringObject == null || !ringObject.activeSelf)
            return;
        
        // Rotate ring
        if (enableRotation)
        {
            ringObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        }
        
        // Pulse ring
        if (enablePulse)
        {
            float pulse = 1 + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            ringObject.transform.localScale = baseScale * pulse;
        }
    }

    /// <summary>
    /// Show the interaction ring
    /// </summary>
    public void ShowRing()
    {
        if (ringObject != null)
        {
            ringObject.SetActive(true);
            Debug.Log($"Showing interaction ring for {gameObject.name}");
        }
    }

    /// <summary>
    /// Hide the interaction ring
    /// </summary>
    public void HideRing()
    {
        if (ringObject != null)
        {
            ringObject.SetActive(false);
            Debug.Log($"Hiding interaction ring for {gameObject.name}");
        }
    }

    /// <summary>
    /// Check if ring is currently visible
    /// </summary>
    public bool IsRingVisible()
    {
        return ringObject != null && ringObject.activeSelf;
    }

    /// <summary>
    /// Set ring color (if you want to change it dynamically)
    /// </summary>
    public void SetRingColor(Color color)
    {
        if (ringRenderer != null)
        {
            ringRenderer.material.color = color;
            
            // If material has emission, set that too
            if (ringRenderer.material.HasProperty("_EmissionColor"))
            {
                ringRenderer.material.SetColor("_EmissionColor", color * 2f);
            }
        }
    }
}