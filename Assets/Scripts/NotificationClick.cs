using UnityEngine;
using UnityEngine.UIElements;

public class NotificationClick : MonoBehaviour
{
    private string taskToAdd = "Task 5";
    private VRUIPositioner uiPositioner;
    private UIDocument uiDocument;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        uiPositioner = GetComponent<VRUIPositioner>();
    }

    void OnEnable()
    {
        // Register button every time notification is shown
        RegisterButton();
        
        // Reposition when notification appears
        if (uiPositioner != null)
        {
            uiPositioner.ForceReposition();
        }
        
        // Play notification sound
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayNotificationSound();
        }
    }

    private void RegisterButton()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument not found!");
                return;
            }
        }

        VisualElement root = uiDocument.rootVisualElement;
        Button button1 = root.Q<Button>("notification-button");

        if (button1 == null)
        {
            Debug.LogError("Notification button not found!");
            return;
        }

        button1.clicked -= OnButtonClicked;
        button1.clicked += OnButtonClicked;
        
        Debug.Log("Notification button registered");
    }

    private void OnButtonClicked()
    {
        Debug.Log($"Notification button clicked! Task: {taskToAdd}");
        var phoneManager = FindFirstObjectByType<TaskPhoneManager>();
        if (phoneManager != null)
        {
            phoneManager.ShowPhoneWithTask(taskToAdd);
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("TaskPhoneManager not found!");
        }
    }

    public void SetTask(string task)
    {
        taskToAdd = task;
        Debug.Log($"NotificationClick: Task set to '{task}'");
    }
}