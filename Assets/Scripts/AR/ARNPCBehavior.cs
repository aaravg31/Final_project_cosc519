using UnityEngine;

/// <summary>
/// Controls AR NPC behavior:
/// - Always face the player camera
/// - Random color on spawn
/// - Prevents disappearing when player gets close
/// </summary>
public class ARNPCBehavior : MonoBehaviour
{
    [Header("Look At Player")]
    [Tooltip("If true, NPC will always rotate to face the player")]
    public bool facePlayer = true;
    [Tooltip("How fast to rotate toward player (degrees per second)")]
    public float rotationSpeed = 5f;
    [Tooltip("Only rotate on Y axis (keep upright)")]
    public bool lockVerticalRotation = true;

    [Header("Random Color")]
    [Tooltip("If true, apply random color on start")]
    public bool useRandomColor = true;
    [Tooltip("Minimum color saturation")]
    public float minSaturation = 0.5f;
    [Tooltip("Minimum color brightness")]
    public float minBrightness = 0.6f;

    private Transform playerCamera;
    private CreatureGlowController glowController;

    void Start()
    {
        // Find player camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            playerCamera = mainCam.transform;
        }

        // Get glow controller if exists
        glowController = GetComponentInChildren<CreatureGlowController>();

        // Apply random color
        if (useRandomColor)
        {
            ApplyRandomColor();
        }
    }

    void Update()
    {
        if (facePlayer && playerCamera != null)
        {
            FacePlayer();
        }
    }

    void FacePlayer()
    {
        Vector3 directionToPlayer = playerCamera.position - transform.position;
        
        if (lockVerticalRotation)
        {
            // Only rotate on Y axis
            directionToPlayer.y = 0;
        }

        if (directionToPlayer.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void ApplyRandomColor()
    {
        // Generate random hue with controlled saturation and brightness
        float hue = Random.value;
        float saturation = Random.Range(minSaturation, 1f);
        float brightness = Random.Range(minBrightness, 1f);
        
        Color randomColor = Color.HSVToRGB(hue, saturation, brightness);
        randomColor.a = 0.6f; // Semi-transparent

        // Apply to glow controller if available
        if (glowController != null)
        {
            glowController.SetBodyColor(randomColor);
            
            // Also set a complementary glow color
            float complementaryHue = (hue + 0.5f) % 1f;
            Color glowColor = Color.HSVToRGB(complementaryHue, 0.8f, 1f);
            glowController.SetGlowColor(glowColor);
        }
        else
        {
            // Fallback: try to set material color directly
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer.material != null)
                {
                    // Try various color property names
                    if (renderer.material.HasProperty("_Color"))
                        renderer.material.SetColor("_Color", randomColor);
                    if (renderer.material.HasProperty("_BaseColor"))
                        renderer.material.SetColor("_BaseColor", randomColor);
                }
            }
        }

        Debug.Log($"ARNPCBehavior: Applied random color {randomColor} to NPC");
    }

    /// <summary>
    /// Set custom color for this NPC
    /// </summary>
    public void SetColor(Color color)
    {
        if (glowController != null)
        {
            glowController.SetBodyColor(color);
        }
    }

    /// <summary>
    /// Set the camera to face
    /// </summary>
    public void SetPlayerCamera(Transform camera)
    {
        playerCamera = camera;
    }
}

