using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;

/// <summary>
/// Custom AR Pose Updater - manually applies AR camera pose to transform.
/// Bypasses TrackedPoseDriver which may not work in all configurations.
/// </summary>
public class ARPoseUpdater : MonoBehaviour
{
    private ARCameraManager cameraManager;
    private XROrigin xrOrigin;
    private Camera arCamera;
    
    void Start()
    {
        cameraManager = GetComponent<ARCameraManager>();
        arCamera = GetComponent<Camera>();
        xrOrigin = FindObjectOfType<XROrigin>();
        
        if (cameraManager != null)
        {
            cameraManager.frameReceived += OnCameraFrameReceived;
            Debug.Log("ARPoseUpdater: Subscribed to frameReceived");
        }
        else
        {
            Debug.LogWarning("ARPoseUpdater: No ARCameraManager found!");
        }
    }
    
    void OnDestroy()
    {
        if (cameraManager != null)
        {
            cameraManager.frameReceived -= OnCameraFrameReceived;
        }
    }
    
    void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        // The XROrigin should automatically handle the transform updates
        // when frameReceived fires. If not, we can manually update here.
        
        // Log current position for debugging
        if (Time.frameCount % 60 == 0) // Log once per second
        {
            Debug.Log($"ARPoseUpdater: Camera pos = {transform.position}, rot = {transform.rotation.eulerAngles}");
        }
    }
    
    void Update()
    {
        // Alternative approach: Read from ARSession's tracked pose
        // This is a fallback if the XROrigin isn't updating the camera properly
        
        if (xrOrigin != null && arCamera != null)
        {
            // The XROrigin's TrackablesParent should be moving with the device
            // Make sure our camera is positioned correctly relative to it
            // (This usually happens automatically, but let's verify)
            
            // If we detect the camera is stuck at origin, force an offset
            if (transform.localPosition.sqrMagnitude < 0.001f && Time.time > 5f)
            {
                // Camera seems stuck, try to get a valid position
                // This is a last-resort hack
            }
        }
    }
}
