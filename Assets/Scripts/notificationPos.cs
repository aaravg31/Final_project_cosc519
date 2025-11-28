using UnityEngine;

public class notificationPos : MonoBehaviour
{
    public Transform cameraTransform;
    public Vector3 offset = new Vector3(0.45f, 2f, 4f); // top-right of view

    void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        // Match camera rotation
        transform.rotation = cameraTransform.rotation;

        // Stay offset in front of camera
        transform.position = cameraTransform.position + cameraTransform.rotation * offset;
    }
}
