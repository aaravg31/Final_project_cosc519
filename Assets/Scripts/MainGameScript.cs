using UnityEngine;
using System.Collections;

public class MainGameScript : MonoBehaviour
{
    [SerializeField] private TaskPhoneManager phoneManager;
    [SerializeField] private GameObject notificationUI;
    [SerializeField] private NotificationClick notificationScript;

    private void Start()
    {
        // Subscribe to phone close event
        if (phoneManager != null)
        {
            phoneManager.OnPhoneClosed += HandlePhoneClosed;
        }
        else
        {
            Debug.LogError("TaskPhoneManager not assigned!");
        }

        // Hide UIs at start
        if (notificationUI != null)
        {
            notificationUI.SetActive(false);
        }
        else
        {
            Debug.LogError("Notification UI not assigned!");
        }

        // Make sure phone is hidden (it should be from TaskPhoneManager.Start())
        Debug.Log("Game started - UIs hidden");

        // Start the timer for first notification
        StartCoroutine(ShowFirstNotification());
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (phoneManager != null)
        {
            phoneManager.OnPhoneClosed -= HandlePhoneClosed;
        }
    }

    private IEnumerator ShowFirstNotification()
    {
        // Wait 10 seconds
        yield return new WaitForSeconds(10f);

        // Show notification with "Homework" task
        ShowNewTaskNotification("Homework");
    }

    // Call this when you want to show a new task notification
    private void ShowNewTaskNotification(string taskName)
    {
        Debug.Log($"Showing notification for task: {taskName}");
        
        if (notificationScript != null)
        {
            notificationScript.SetTask(taskName);
            notificationUI.SetActive(true);
        }
        else
        {
            Debug.LogError("NotificationClick script not assigned!");
        }
    }

    // This gets called when the phone closes
    private void HandlePhoneClosed(string newTask, string swappedOutTask)
    {
        Debug.Log($"=== Phone Closed ===");
        Debug.Log($"New task was: {newTask}");
        
        if (swappedOutTask != null)
        {
            Debug.Log($"User accepted '{newTask}' and dropped '{swappedOutTask}'");
            PerformActionBasedOnSwap(newTask, swappedOutTask);
        }
        else
        {
            Debug.Log($"User rejected '{newTask}' (didn't swap it in)");
            PerformActionForRejection(newTask);
        }
    }

    private void PerformActionBasedOnSwap(string acceptedTask, string droppedTask)
    {
        // Your custom logic here based on what was swapped
        Debug.Log($"Performing action because {droppedTask} was replaced with {acceptedTask}");
        
        // Example:
        if (droppedTask == "Task 1")
        {
            Debug.Log("Task 1 was dropped - trigger consequence A");
        }
        else if (droppedTask == "Task 2")
        {
            Debug.Log("Task 2 was dropped - trigger consequence B");
        }
        else if (droppedTask == "Task 3")
        {
            Debug.Log("Task 3 was dropped - trigger consequence C");
        }
        else if (droppedTask == "Task 4")
        {
            Debug.Log("Task 4 was dropped - trigger consequence D");
        }
    }

    private void PerformActionForRejection(string rejectedTask)
    {
        // Logic for when user closes without swapping
        Debug.Log($"{rejectedTask} was rejected - maybe offer it again later?");
    }
}