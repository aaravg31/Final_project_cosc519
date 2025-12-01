using UnityEngine;
using System.Collections;

public class HelpPaperController : MonoBehaviour
{
    [Header("References")]
    public MainGameScript gameManager;
    
    [Header("Display Settings")]
    public Vector3 displayOffset = new Vector3(0f, 0f, 1.5f);
    public Vector3 displayScale = new Vector3(5f, 1f, 5f);
    public float moveSpeed = 2f;
    public float tiltAngle = 25f; // Tilt the paper back
    
    private bool hasBeenRead = false;
    private bool isDisplayed = false;
    private bool isVisible = false;
    private Transform playerCamera;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private Renderer paperRenderer;
    private Collider paperCollider;

    void Start()
    {
        playerCamera = Camera.main.transform;
        
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
        
        // Get renderer and collider
        paperRenderer = GetComponent<Renderer>();
        paperCollider = GetComponent<Collider>();
        
        // Hide paper initially
        HidePaper();
    }

    void Update()
    {
        // If paper is displayed, keep it in front of camera
        if (isDisplayed && playerCamera != null)
        {
            UpdatePaperPosition();
        }
    }

    private void UpdatePaperPosition()
    {
        // Calculate target position in front of camera
        Vector3 targetPosition = playerCamera.position + 
                                playerCamera.forward * displayOffset.z + 
                                playerCamera.right * displayOffset.x + 
                                playerCamera.up * displayOffset.y;
        
        // Smoothly move to target
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
        
        // Face the camera with tilt
        Vector3 directionToCamera = playerCamera.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
        targetRotation *= Quaternion.Euler(90f - tiltAngle, 0f, 0f); // Apply tilt
        
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * moveSpeed);
    }

    // Called by MainGameScript when paper should appear
    public void ShowPaperInWorld()
    {
        isVisible = true;
        
        if (paperRenderer != null)
        {
            paperRenderer.enabled = true;
        }
        
        if (paperCollider != null)
        {
            paperCollider.enabled = true;
        }
        
        Debug.Log("Help paper is now visible in the world");
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
    }

    // Called by XR Simple Interactable
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
        
        // Trigger sanity restoration and ending audio
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
            
            // Face camera with tilt
            Vector3 directionToCamera = playerCamera.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
            targetRotation *= Quaternion.Euler(90f - tiltAngle, 0f, 0f); // Apply tilt
            
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            
            yield return null;
        }
        
        Debug.Log("Paper animation complete");
    }
}