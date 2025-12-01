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
    
    private bool hasBeenRead = false;
    private bool isDisplayed = false;
    private Transform playerCamera;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    void Start()
    {
        playerCamera = Camera.main.transform;
        
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
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
    
        // Face the camera with 90 degree tilt
        Vector3 directionToCamera = playerCamera.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
        targetRotation *= Quaternion.Euler(90f, 0f, 0f); // Keep it vertical
    
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * moveSpeed);
    }

    // Called by XR Simple Interactable
    public void OnPaperInteracted()
    {
        Debug.Log("=== HELP PAPER CLICKED ===");
        
        if (isDisplayed)
        {
            Debug.Log("Paper already displayed, ignoring click");
            return;
        }
        
        Debug.Log("Showing paper in front of camera");
        ShowPaper();
        
        // Only play audio on first interaction
        /*
        if (!hasBeenRead)
        {
            hasBeenRead = true;
            
            Debug.Log("Playing ending audio from paper");
            if (gameManager != null)
            {
                gameManager.PlayEndingAudioFromPaper();
            }
            else
            {
                Debug.LogError("GameManager not assigned!");
            }
        }
        */
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
        
            // Face camera directly (perpendicular to view)
            Vector3 directionToCamera = playerCamera.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
        
            // Add 90 degrees pitch to make it vertical/upright
            targetRotation *= Quaternion.Euler(90f, 0f, 0f); // Adjust these values
        
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
        
            yield return null;
        }
    
        Debug.Log("Paper animation complete");
    }
}