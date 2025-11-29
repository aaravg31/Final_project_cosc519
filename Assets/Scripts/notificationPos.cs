using UnityEngine;

public class notificationPos : MonoBehaviour
{
    public Transform cameraTransform;
    public Vector3 offset = new Vector3(0.45f, 2f, 4f); // top-right of view
    
    [Tooltip("If true, only follows base camera rotation, ignoring shake/jitter")]
    public bool ignoreShake = true;
    
    private Quaternion baseRotation;

    void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        if (ignoreShake)
        {
            // Get the parent rotation (Camera Offset or XR Origin) which doesn't shake
            Transform parentTransform = cameraTransform.parent;
            if (parentTransform != null)
            {
                baseRotation = parentTransform.rotation;
            }
            else
            {
                // If no parent, use camera's Y rotation only (yaw) to ignore shake
                Vector3 eulerAngles = cameraTransform.rotation.eulerAngles;
                baseRotation = Quaternion.Euler(0, eulerAngles.y, 0);
            }
            
            // Match base rotation (no shake)
            transform.rotation = baseRotation;
            
            // Stay offset in front of camera using base rotation
            transform.position = cameraTransform.position + baseRotation * offset;
        }
        else
        {
            // Original behavior - follows all camera movement including shake
            transform.rotation = cameraTransform.rotation;
            transform.position = cameraTransform.position + cameraTransform.rotation * offset;
        }
    }
}