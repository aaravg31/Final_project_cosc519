using UnityEngine;

public class VRUIPositioner : MonoBehaviour
{
    [Header("Target")]
    public Transform cameraTransform;
    
    [Header("Position Settings")]
    public Vector3 offset = new Vector3(0f, 0f, 2f); // Forward, Right, Up relative to camera
    public bool useLocalOffset = true; // Offset relative to camera rotation
    
    [Header("Rotation Settings")]
    public bool faceCamera = true;
    public bool updateContinuously = true; // If false, only positions once when enabled
    
    [Header("Smoothing (Optional)")]
    public bool smoothFollow = false;
    public float followSpeed = 5f;
    
    private bool hasPositioned = false;

    void Start()
    {
        if (cameraTransform == null)
        {
            // Auto-find main camera
            cameraTransform = Camera.main.transform;
            
            if (cameraTransform == null)
            {
                Debug.LogError("VRUIPositioner: No camera assigned and couldn't find Main Camera!");
            }
        }
    }

    void OnEnable()
    {
        hasPositioned = false;
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;
        
        // If not continuous, only position once
        if (!updateContinuously && hasPositioned) return;
        
        UpdatePosition();
        hasPositioned = true;
    }

    private void UpdatePosition()
    {
        Vector3 targetPosition;
        
        if (useLocalOffset)
        {
            // Offset relative to camera's rotation
            targetPosition = cameraTransform.position + 
                           cameraTransform.forward * offset.z + 
                           cameraTransform.right * offset.x + 
                           cameraTransform.up * offset.y;
        }
        else
        {
            // Offset in world space
            targetPosition = cameraTransform.position + offset;
        }
        
        // Apply position (with or without smoothing)
        if (smoothFollow)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
        }
        else
        {
            transform.position = targetPosition;
        }
        
        // Face camera if enabled
        if (faceCamera)
        {
            transform.LookAt(cameraTransform);
            transform.Rotate(0, 180, 0); // Flip to face player
        }
    }

    // Call this to force immediate repositioning
    public void ForceReposition()
    {
        hasPositioned = false;
        if (cameraTransform != null)
        {
            UpdatePosition();
            hasPositioned = true;
        }
    }
}