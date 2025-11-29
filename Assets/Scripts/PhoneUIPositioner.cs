using UnityEngine;
using UnityEngine.UIElements;

public class PhoneUIPositioner : MonoBehaviour
{
    public Transform cameraTransform;
    public Vector3 activeOffset = new Vector3(0f, 0f, 1.5f); // In front when active
    public Vector3 hiddenOffset = new Vector3(0f, -10f, 0f); // Way below when hidden
    
    private TaskPhoneManager phoneManager;
    private bool wasActive = false;

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        phoneManager = GetComponent<TaskPhoneManager>();
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;
    
        // Check if phone UI is visible by checking root display style
        bool isPhoneVisible = IsPhoneVisible();
    
        Vector3 targetPosition;
    
        if (isPhoneVisible)
        {
            // Position in front of camera
            targetPosition = cameraTransform.position + 
                             cameraTransform.forward * activeOffset.z + 
                             cameraTransform.right * activeOffset.x + 
                             cameraTransform.up * activeOffset.y;
        
            transform.position = targetPosition;
            transform.LookAt(cameraTransform);
            transform.Rotate(0, 180, 0); // Flip to face player
            transform.Rotate(30, 0, 0);
        }
        else
        {
            // Move way out of the way
            transform.position = cameraTransform.position + hiddenOffset;
        }
    
        wasActive = isPhoneVisible;
    }
    
    private bool IsPhoneVisible()
    {
        if (phoneManager == null) return false;
        
        var root = GetComponent<UIDocument>()?.rootVisualElement;
        if (root == null) return false;
        
        return root.style.display == UnityEngine.UIElements.DisplayStyle.Flex;
    }
}