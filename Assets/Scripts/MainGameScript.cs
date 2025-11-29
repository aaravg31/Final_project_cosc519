using UnityEngine;
using System.Collections;

public class MainGameScript : MonoBehaviour
{
    [SerializeField] private TaskPhoneManager phoneManager;
    [SerializeField] private GameObject notificationUI;
    [SerializeField] private NotificationClick notificationScript;
    [SerializeField] private ChoiceUIController dialogueUI;

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

        // Make sure dialogue UI is hidden
        if (dialogueUI != null)
        {
            dialogueUI.HideAll();
        }
        else
        {
            Debug.LogError("ChoiceUIController not assigned!");
        }

        // Make sure phone is hidden (it should be from TaskPhoneManager.Start())
        Debug.Log("Game started - UIs hidden");

        // Start the dialogue sequence after 10 seconds
        StartCoroutine(StartDialogueSequence());
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (phoneManager != null)
        {
            phoneManager.OnPhoneClosed -= HandlePhoneClosed;
        }
    }

    private IEnumerator StartDialogueSequence()
    {
        // Wait 10 seconds
        yield return new WaitForSeconds(10f);

        // Show first dialogue
        Debug.Log("Starting dialogue sequence");
        yield return StartCoroutine(ShowFirstDialogue());
    }

    private IEnumerator ShowFirstDialogue()
    {
        string selectedChoice = null;
        bool choiceMade = false;

        // Show first dialogue
        dialogueUI.ShowDialogue(
            "NPC: Hey! What do you want to do today?",
            "Go to the library",
            "Play video games",
            (choice) =>
            {
                selectedChoice = choice;
                choiceMade = true;
                Debug.Log($"First choice made: {choice}");
            }
        );

        // Wait for user to make a choice
        yield return new WaitUntil(() => choiceMade);

        // Wait longer for first dialogue to fully complete (2s selected stays + 0.3s fade + buffer)
        yield return new WaitForSeconds(3f);

        // Show second dialogue based on first choice
        yield return StartCoroutine(ShowSecondDialogue(selectedChoice));
    }

    private IEnumerator ShowSecondDialogue(string firstChoice)
    {
        string selectedChoice = null;
        bool choiceMade = false;

        if (firstChoice == "Go to the library")
        {
            dialogueUI.ShowDialogue(
                "NPC: Great! What will you study?",
                "Math homework",
                "Read a novel",
                (choice) =>
                {
                    selectedChoice = choice;
                    choiceMade = true;
                    Debug.Log($"Second choice made: {choice}");
                }
            );
        }
        else if (firstChoice == "Play video games")
        {
            dialogueUI.ShowDialogue(
                "NPC: Nice! What game?",
                "Action game",
                "Puzzle game",
                (choice) =>
                {
                    selectedChoice = choice;
                    choiceMade = true;
                    Debug.Log($"Second choice made: {choice}");
                }
            );
        }

        // Wait for user to make a choice
        yield return new WaitUntil(() => choiceMade);

        // Wait for second dialogue to fully complete
        yield return new WaitForSeconds(3f);

        // Show notification with the final selected choice
        ShowNewTaskNotification(selectedChoice);
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