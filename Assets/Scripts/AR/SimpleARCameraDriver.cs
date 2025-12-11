using UnityEngine;
using UnityEngine.XR;

public class SimpleARCameraDriver : MonoBehaviour
{
    // Simple fallback driver that pulls data directly from XRNode
    // Works with both New/Old Input Systems usually as it hits the underlying Subsystem.
    
    void Update()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);
        
        if (device.isValid)
        {
            if (device.TryGetFeatureValue(CommonUsages.centerEyePosition, out Vector3 pos))
            {
                transform.localPosition = pos;
            }
            
            if (device.TryGetFeatureValue(CommonUsages.centerEyeRotation, out Quaternion rot))
            {
                transform.localRotation = rot;
            }
        }
        else
        {
            // Fallback for some ARCore versions which map to Head
            device = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            if (device.isValid)
            {
                if (device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos))
                    transform.localPosition = pos;
                
                if (device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rot))
                    transform.localRotation = rot;
            }
        }
    }
}
