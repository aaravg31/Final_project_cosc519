using UnityEngine;
using UnityEngine.UI;

public class OffScreenIndicator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The target object to track.")]
    public Transform target;
    [Tooltip("The UI object (e.g., an Arrow Image) to show/hide.")]
    public RectTransform arrowUI;
    [Tooltip("The Canvas RectTransform containing the arrow.")]
    public RectTransform canvasRect;
    [Tooltip("The camera used for tracking. If null, uses Camera.main.")]
    public Camera targetCamera;

    [Header("Settings")]
    [Tooltip("Distance from the screen edge (0 to 0.5).")]
    [Range(0f, 0.5f)]
    public float edgeBuffer = 0.05f;
    [Tooltip("If true, the arrow rotates to point towards the target.")]
    public bool rotateArrow = true;
    [Tooltip("Offset angle for rotation (e.g. if arrow sprite points up, use 0. If right, use 90).")]
    public float rotationOffset = 0f;
    [Tooltip("Hide the indicator if the target is within this distance.")]
    public float hideDistance = 1.0f;

    private Vector3 screenCenter = new Vector3(0.5f, 0.5f, 0f);

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (canvasRect == null)
        {
            // Try to find parent canvas
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
                canvasRect = canvas.GetComponent<RectTransform>();
        }
        
        // Ensure arrow is initialized correctly
        if (arrowUI != null)
            arrowUI.gameObject.SetActive(false);
    }

    void Update()
    {
        if (target == null || arrowUI == null || targetCamera == null || canvasRect == null)
            return;

        // Check distance - hide if too close
        if (Vector3.Distance(targetCamera.transform.position, target.position) < hideDistance)
        {
             if (arrowUI.gameObject.activeSelf) arrowUI.gameObject.SetActive(false);
             return;
        }

        // Get position in Viewport coordinates (0,0 bottom-left, 1,1 top-right)
        Vector3 targetPosLocal = targetCamera.transform.InverseTransformPoint(target.position);
        Vector3 targetViewportPos = targetCamera.WorldToViewportPoint(target.position);

        // Is target visible on screen?
        // Z > 0 means in front of camera (WorldToViewportPoint z is distance from camera)
        // X and Y between 0 and 1 means within screen bounds (roughly, not accounting for edge buffer)
        bool inFront = targetPosLocal.z > 0;
        bool inBounds = targetViewportPos.x > 0 && targetViewportPos.x < 1 &&
                        targetViewportPos.y > 0 && targetViewportPos.y < 1;
        
        bool onScreen = inFront && inBounds;

        if (onScreen)
        {
            // Target is visible, hide arrow
            if (arrowUI.gameObject.activeSelf) arrowUI.gameObject.SetActive(false);
        }
        else
        {
            if (!arrowUI.gameObject.activeSelf) arrowUI.gameObject.SetActive(true);
            UpdateArrowPosition(targetViewportPos, targetPosLocal.z);
        }
    }

    void UpdateArrowPosition(Vector3 viewportPos, float zPos)
    {
        // Direction from center (0.5, 0.5) to target viewport position
        Vector3 direction = viewportPos - screenCenter;

        // If target is behind the camera (z < 0), invert the direction
        // Because viewport projection of point behind camera flips it across center
        if (zPos < 0)
        {
            direction = -direction; 
        }
        
        // Avoid zero direction
        if (direction.magnitude < 0.001f)
            direction = Vector3.up;
            
        // Flatten Z
        direction.z = 0;

        // Calculate scale factor to clamp to screen edge box
        // Box is defined by [edgeBuffer, 1-edgeBuffer]
        // Relative to center, box extent is 0.5 - edgeBuffer
        float bound = 0.5f - edgeBuffer;
        
        // Calculate intersection with box edge
        // p = s * direction. 
        // We want max s such that |p.x| <= bound AND |p.y| <= bound
        // s = min(bound / |dir.x|, bound / |dir.y|)
        
        float xFactor = Mathf.Abs(direction.x) > 0 ? bound / Mathf.Abs(direction.x) : float.MaxValue;
        float yFactor = Mathf.Abs(direction.y) > 0 ? bound / Mathf.Abs(direction.y) : float.MaxValue;
        
        float scale = Mathf.Min(xFactor, yFactor);
        
        Vector3 clampedOffset = direction * scale;
        Vector3 finalViewportPos = screenCenter + clampedOffset;

        // Convert Viewport to Canvas Anchored Position
        // (ViewportPos - 0.5) * CanvasSize
        Vector2 anchoredPos = new Vector2(
            (finalViewportPos.x - 0.5f) * canvasRect.rect.width,
            (finalViewportPos.y - 0.5f) * canvasRect.rect.height
        );

        arrowUI.anchoredPosition = anchoredPos;

        // Rotation
        if (rotateArrow)
        {
            // Calculate angle
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // Apply offset
            arrowUI.localEulerAngles = new Vector3(0, 0, angle + rotationOffset);
        }
    }
}

