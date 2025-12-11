using UnityEngine;

/// <summary>
/// AR Safety Zone Visual - Creates a glowing ground circle around NPC
/// indicating the "safe zone" where player's anxiety is reduced
/// </summary>
public class ARSafetyZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [Tooltip("Radius of the safety zone circle.")]
    public float zoneRadius = 2f;
    [Tooltip("Color of the zone when player is OUTSIDE (danger).")]
    public Color outsideColor = new Color(1f, 0.2f, 0.2f, 0.5f);  // RED
    [Tooltip("Color of the zone when player is INSIDE (safe).")]
    public Color insideColor = new Color(0.2f, 1f, 0.3f, 0.7f);   // GREEN
    [Tooltip("Number of segments for the circle.")]
    public int segments = 32;
    [Tooltip("Line width.")]
    public float lineWidth = 0.05f;
    [Tooltip("Pulse speed.")]
    public float pulseSpeed = 1.5f;
    [Tooltip("Pulse amount.")]
    public float pulseAmount = 0.2f;

    [Header("References")]
    [Tooltip("Custom material for the ring. If null, will auto-create.")]
    public Material customMaterial;

    private LineRenderer lineRenderer;
    private Transform playerCamera;
    private bool playerInside = false;
    private float baseAlpha;

    void Start()
    {
        if (Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        CreateZoneVisual();
        baseAlpha = outsideColor.a;
    }

    void CreateZoneVisual()
    {
        GameObject ringGO = new GameObject("SafetyZoneRing");
        ringGO.transform.SetParent(transform);
        ringGO.transform.localPosition = new Vector3(0, 0.05f, 0); // Slightly above ground
        ringGO.transform.localRotation = Quaternion.Euler(90, 0, 0); // Flat on ground

        lineRenderer = ringGO.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = segments;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        // Create material
        lineRenderer.material = CreateRingMaterial();

        // Set positions for circle
        UpdateCirclePositions();

        lineRenderer.startColor = outsideColor;
        lineRenderer.endColor = outsideColor;
    }

    Material CreateRingMaterial()
    {
        if (customMaterial != null)
            return customMaterial;

        string[] shaderNames = new string[]
        {
            "Universal Render Pipeline/Unlit",
            "Unlit/Color",
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
            Debug.LogWarning("ARSafetyZone: No suitable shader found");
            return null;
        }

        Material mat = new Material(shader);
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", outsideColor);
        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", outsideColor);

        // Enable transparency
        mat.renderQueue = 3000;

        return mat;
    }

    void UpdateCirclePositions()
    {
        Vector3[] positions = new Vector3[segments];
        
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * 2f * Mathf.PI;
            float x = Mathf.Cos(angle) * zoneRadius;
            float y = Mathf.Sin(angle) * zoneRadius;
            positions[i] = new Vector3(x, y, 0);
        }

        lineRenderer.SetPositions(positions);
    }


    void Update()
    {
        if (playerCamera == null || lineRenderer == null) return;

        // Check distance to player (horizontal only)
        float distance = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(playerCamera.position.x, 0, playerCamera.position.z)
        );

        bool wasInside = playerInside;
        playerInside = distance <= zoneRadius;
        
        // Calculate blend factor based on distance (smooth transition near boundary)
        // Creates a smooth gradient instead of hard switch
        float transitionZone = zoneRadius * 0.3f; // 30% of radius for transition
        float innerBoundary = zoneRadius - transitionZone;
        
        float blendFactor;
        if (distance < innerBoundary)
        {
            blendFactor = 1f; // Fully inside - green
        }
        else if (distance > zoneRadius)
        {
            blendFactor = 0f; // Fully outside - red
        }
        else
        {
            // In transition zone - smooth blend
            blendFactor = 1f - ((distance - innerBoundary) / transitionZone);
            blendFactor = Mathf.SmoothStep(0f, 1f, blendFactor); // Even smoother
        }
        
        // Smooth color interpolation
        Color targetColor = Color.Lerp(outsideColor, insideColor, blendFactor);

        // Pulse effect (stronger when inside)
        float pulseStrength = Mathf.Lerp(pulseAmount * 0.5f, pulseAmount, blendFactor);
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseStrength + 1f;
        
        // Smooth transition of current displayed color
        Color displayColor = Color.Lerp(lineRenderer.startColor, targetColor, Time.deltaTime * 5f);
        displayColor.a = Mathf.Lerp(outsideColor.a, insideColor.a, blendFactor) * pulse;
        
        // Line width also transitions smoothly
        float targetWidth = Mathf.Lerp(lineWidth, lineWidth * 1.8f, blendFactor);
        lineRenderer.startWidth = Mathf.Lerp(lineRenderer.startWidth, targetWidth, Time.deltaTime * 5f);
        lineRenderer.endWidth = lineRenderer.startWidth;

        lineRenderer.startColor = displayColor;
        lineRenderer.endColor = displayColor;

        // Also update material color
        if (lineRenderer.material != null)
        {
            if (lineRenderer.material.HasProperty("_BaseColor"))
                lineRenderer.material.SetColor("_BaseColor", displayColor);
            if (lineRenderer.material.HasProperty("_Color"))
                lineRenderer.material.SetColor("_Color", displayColor);
        }
    }


    /// <summary>
    /// Set the zone radius
    /// </summary>
    public void SetRadius(float radius)
    {
        zoneRadius = radius;
        if (lineRenderer != null)
        {
            UpdateCirclePositions();
        }
    }

    /// <summary>
    /// Set the zone colors
    /// </summary>
    public void SetColors(Color outside, Color inside)
    {
        outsideColor = outside;
        insideColor = inside;
        baseAlpha = outside.a;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = outsideColor;
        Gizmos.DrawWireSphere(transform.position, zoneRadius);
    }
}
