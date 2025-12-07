using UnityEngine;
using System.Collections;
using TMPro; // Add this for TextMeshPro

public class HelpPaperController : MonoBehaviour
{
    [Header("References")]
    public MainGameScript gameManager;
    
    [Header("Display Settings")]
    public Vector3 displayOffset = new Vector3(0f, 0f, 1.5f);
    public Vector3 displayScale = new Vector3(5f, 1f, 5f);
    public float moveSpeed = 2f;
    public float tiltAngle = 25f;
    
    [Header("Glow & Pulse Effect")]
    public Material glowMaterial; // Assign your white/glowing material here
    [Range(0f, 5f)]
    public float pulseSpeed = 2f;
    [Range(0f, 0.3f)]
    public float pulseAmount = 0.15f;
    [Range(0f, 10f)]
    public float emissionIntensity = 2f; // How bright the glow is
    
    [Header("Hover Effect (Optional)")]
    public Material highlightMaterial;
    public float hoverScale = 1.1f;
    
    private bool hasBeenRead = false;
    private bool isDisplayed = false;
    private bool isVisible = false;
    private bool isHovering = false;
    private Transform playerCamera;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private Renderer paperRenderer;
    private Collider paperCollider;
    private TextMeshPro paperText;
    private Material originalMaterial;
    private Material instanceMaterial; // Instance of glow material for runtime changes

    void Start()
    {
        playerCamera = Camera.main.transform;
        
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
        
        // Get components
        paperRenderer = GetComponent<Renderer>();
        paperCollider = GetComponent<Collider>();
        paperText = GetComponentInChildren<TextMeshPro>(); // Find text in children
        
        if (paperRenderer != null)
        {
            originalMaterial = paperRenderer.material;
        }
        
        // Hide paper initially
        HidePaper();
    }

    void Update()
    {
        if (isDisplayed && playerCamera != null)
        {
            UpdatePaperPosition();
        }
        
        // Pulse the glow while visible and not yet displayed
        if (isVisible && !isDisplayed)
        {
            PulseGlow();
        }
    }

    private void UpdatePaperPosition()
    {
        Vector3 targetPosition = playerCamera.position + 
                                playerCamera.forward * displayOffset.z + 
                                playerCamera.right * displayOffset.x + 
                                playerCamera.up * displayOffset.y;
        
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
        
        Vector3 directionToCamera = playerCamera.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
        targetRotation *= Quaternion.Euler(90f - tiltAngle, 0f, 0f);
        
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * moveSpeed);
    }

    private void PulseGlow()
    {
        // Pulse the scale
        float pulse = 1 + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * pulse;
        
        // Pulse the emission if material supports it
        if (instanceMaterial != null && instanceMaterial.HasProperty("_EmissionColor"))
        {
            // Pulse emission intensity
            float emissionPulse = 1 + Mathf.Sin(Time.time * pulseSpeed) * 0.5f;
            Color emissionColor = Color.white * emissionIntensity * emissionPulse;
            instanceMaterial.SetColor("_EmissionColor", emissionColor);
        }
    }

    public void ShowPaperInWorld()
    {
        isVisible = true;
        
        if (paperRenderer != null)
        {
            paperRenderer.enabled = true;
            
            // Apply glow material
            if (glowMaterial != null)
            {
                // Create an instance so we can modify it at runtime
                instanceMaterial = new Material(glowMaterial);
                paperRenderer.material = instanceMaterial;
                
                // Set initial emission
                if (instanceMaterial.HasProperty("_EmissionColor"))
                {
                    instanceMaterial.EnableKeyword("_EMISSION");
                    instanceMaterial.SetColor("_EmissionColor", Color.white * emissionIntensity);
                }
            }
        }
        
        if (paperCollider != null)
        {
            paperCollider.enabled = true;
        }
        
        // Show text
        if (paperText != null)
        {
            paperText.enabled = true;
        }
        
        Debug.Log("Help paper is now visible in the world with glow effect");
    }

    private void HidePaper()
    {
        isVisible = false;
        
        if (paperRenderer != null)
        {
            paperRenderer.enabled = false;
        }
        
        if (paperCollider != null)
        {
            paperCollider.enabled = false;
        }
        
        // Hide text
        if (paperText != null)
        {
            paperText.enabled = false;
        }
    }
    
    // Called by XR Simple Interactable - Hover Enter
    public void OnHoverEnter()
    {
        if (!isVisible || isDisplayed)
            return;
        
        isHovering = true;
        
        // Optional: Change to even brighter highlight material on hover
        if (paperRenderer != null && highlightMaterial != null)
        {
            paperRenderer.material = highlightMaterial;
        }
        
        Debug.Log("Hovering over paper");
    }

    // Called by XR Simple Interactable - Hover Exit
    public void OnHoverExit()
    {
        if (!isVisible || isDisplayed)
            return;
        
        isHovering = false;
        
        // Restore glow material (not original)
        if (paperRenderer != null && instanceMaterial != null)
        {
            paperRenderer.material = instanceMaterial;
        }
        
        Debug.Log("Stopped hovering over paper");
    }

    public void OnPaperInteracted()
    {
        Debug.Log("=== HELP PAPER CLICKED ===");
        
        if (!isVisible)
        {
            Debug.Log("Paper not visible yet");
            return;
        }
        
        if (isDisplayed)
        {
            Debug.Log("Paper already displayed, ignoring click");
            return;
        }
        
        Debug.Log("Showing paper in front of camera");
        ShowPaper();
        
        if (!hasBeenRead)
        {
            hasBeenRead = true;
            
            Debug.Log("Restoring sanity and playing ending audio");
            if (gameManager != null)
            {
                gameManager.OnHelpPaperRead();
            }
            else
            {
                Debug.LogError("GameManager not assigned!");
            }
        }
    }

    private void ShowPaper()
    {
        isDisplayed = true;
        
        // Stop glowing when displayed (restore to original material)
        if (paperRenderer != null && originalMaterial != null)
        {
            paperRenderer.material = originalMaterial;
        }
        
        StartCoroutine(AnimatePaperToCamera());
    }

    private IEnumerator AnimatePaperToCamera()
    {
        float elapsed = 0f;
        float duration = 0.5f;
        
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Vector3 startScale = transform.localScale;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            Vector3 targetPosition = playerCamera.position + 
                                    playerCamera.forward * displayOffset.z + 
                                    playerCamera.right * displayOffset.x + 
                                    playerCamera.up * displayOffset.y;
            
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            transform.localScale = Vector3.Lerp(startScale, displayScale, t);
            
            Vector3 directionToCamera = playerCamera.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
            targetRotation *= Quaternion.Euler(90f - tiltAngle, 0f, 0f);
            
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            
            yield return null;
        }
        
        Debug.Log("Paper animation complete");
    }
    
    private void OnDestroy()
    {
        // Clean up instanced material
        if (instanceMaterial != null)
        {
            Destroy(instanceMaterial);
        }
    }
}