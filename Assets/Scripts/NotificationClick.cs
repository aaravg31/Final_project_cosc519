using UnityEngine;
using UnityEngine.UIElements;

public class NotificationClick : MonoBehaviour
{
    private string taskToAdd = "Task 5"; // Default task

    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        Button button1 = root.Q<Button>("notification-button");

        if (button1 == null)
        {
            Debug.LogError("Notification button not found!");
            return;
        }

        Debug.Log("Notification button found and registered");

        button1.clicked += () =>
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
        };
    }

    // Call this from MainGameScript to set the task
    public void SetTask(string task)
    {
        taskToAdd = task;
        Debug.Log($"NotificationClick: Task set to '{task}'");
    }
}