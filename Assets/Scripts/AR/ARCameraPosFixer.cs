using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

/// <summary>
/// AR Camera Position Fixer - Manually updates camera position from XR input
/// This fixes the issue where TrackedPoseDriver doesn't update the camera position
/// </summary>
public class ARCameraPosFixer : MonoBehaviour
{
    [Header("Debug")]
    public bool debugMode = true;
    public Vector3 lastPosition;
    public Quaternion lastRotation;
    public bool trackingWorking = false;
    public string trackingStatus = "Not started";

    private List<XRNodeState> nodeStates = new List<XRNodeState>();
    private float lastLogTime = 0f;

    void Update()
    {
        UpdateFromXRInput();
    }

    void UpdateFromXRInput()
    {
        // Get tracking data from XR Input Subsystem
        InputTracking.GetNodeStates(nodeStates);

        bool foundTracking = false;

        foreach (var nodeState in nodeStates)
        {
            // Look for CenterEye (AR camera) or Head node
            if (nodeState.nodeType == XRNode.CenterEye || nodeState.nodeType == XRNode.Head)
            {
                Vector3 position;
                Quaternion rotation;

                bool gotPosition = nodeState.TryGetPosition(out position);
                bool gotRotation = nodeState.TryGetRotation(out rotation);

                if (gotPosition && gotRotation)
                {
                    // Apply to this transform
                    transform.localPosition = position;
                    transform.localRotation = rotation;

                    lastPosition = position;
                    lastRotation = rotation;
                    foundTracking = true;
                    trackingStatus = $"OK ({nodeState.nodeType})";

                    if (debugMode && Time.time - lastLogTime > 2f)
                    {
                        lastLogTime = Time.time;
                        Debug.Log($"ARCameraPosFixer: Pos={position}, Rot={rotation.eulerAngles}");
                    }
                    break;
                }
                else
                {
                    trackingStatus = $"No data ({nodeState.nodeType})";
                }
            }
        }

        if (!foundTracking && nodeStates.Count > 0)
        {
            trackingStatus = $"No CenterEye/Head in {nodeStates.Count} nodes";
        }
        else if (nodeStates.Count == 0)
        {
            trackingStatus = "No XR nodes";
        }

        trackingWorking = foundTracking;
    }
    
    // NOTE: OnGUI debug display removed for production
}

