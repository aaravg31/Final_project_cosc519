using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class MainGameScript : MonoBehaviour
{
    [SerializeField] private TaskPhoneManager phoneManager;
    [SerializeField] private GameObject notificationUI;
    [SerializeField] private NotificationClick notificationScript;
    [SerializeField] private ChoiceUIController dialogueUI;
    [SerializeField] private SanitySystem sanitySystem;
    
    [Header("Player Movement")]
    [SerializeField] private CharacterController characterController; // Assign your XR character controller
    [SerializeField] private ContinuousMoveProvider moveProvider; // Assign your locomotion provider
    
    [Header("Sanity Settings")]
    [SerializeField] private float sanityDecreaseAmount = 40f; // Decrease by 40 (100 -> 60)
    [SerializeField] private float sanityDecreaseSpeed = 2f; // How fast it decreases
    [SerializeField] private float sanityRecoveryDelay = 10f; // Wait 10s before recovering
    [SerializeField] private float sanityRecoverySpeed = 2f; // How fast it recovers
    
    private bool movementLocked = false;

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
        
        // Check sanity system
        if (sanitySystem == null)
        {
            Debug.LogError("SanitySystem not assigned!");
        }

        // Make sure phone is hidden (it should be from TaskPhoneManager.Start())
        Debug.Log("Game started - UIs hidden");
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (phoneManager != null)
        {
            phoneManager.OnPhoneClosed -= HandlePhoneClosed;
        }
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
        
        // Start sanity decrease and recovery cycle
        StartCoroutine(SanityDecreaseAndRecover());
    }

    private IEnumerator SanityDecreaseAndRecover()
    {
        if (sanitySystem == null)
        {
            Debug.LogError("Cannot modify sanity - SanitySystem not assigned!");
            yield break;
        }

        Debug.Log("Starting sanity decrease...");
        
        // Calculate target sanity
        float targetSanity = sanitySystem.currentSanity - sanityDecreaseAmount;
        targetSanity = Mathf.Max(targetSanity, 0f); // Don't go below 0
        
        // Gradually decrease sanity
        while (sanitySystem.currentSanity > targetSanity)
        {
            sanitySystem.ModifySanity(-sanityDecreaseSpeed * Time.deltaTime);
            yield return null;
        }
        
        Debug.Log($"Sanity decreased to {sanitySystem.currentSanity}");
        
        // Wait before recovery
        Debug.Log($"Waiting {sanityRecoveryDelay} seconds before recovery...");
        yield return new WaitForSeconds(sanityRecoveryDelay);
        
        // Gradually recover sanity to max
        Debug.Log("Starting sanity recovery...");
        while (sanitySystem.currentSanity < sanitySystem.maxSanity)
        {
            sanitySystem.ModifySanity(sanityRecoverySpeed * Time.deltaTime);
            yield return null;
        }
        
        // Ensure we reach exactly max sanity
        sanitySystem.SetSanity(sanitySystem.maxSanity);
        Debug.Log($"Sanity recovered to {sanitySystem.currentSanity}");
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
    
    public void LockPlayerMovement(bool lockMovement)
    {
        movementLocked = lockMovement;
        
        // Disable/Enable continuous movement
        if (moveProvider != null)
        {
            moveProvider.enabled = !lockMovement;
        }
        
        Debug.Log($"Player movement {(lockMovement ? "LOCKED" : "UNLOCKED")}");
    }

    public void StartNPCConversation(NPCConversationData conversationData, ChoiceUIController dialogueUI)
    {
        Debug.Log("Starting NPC conversation sequence");
        StartCoroutine(NPCConversationSequence(conversationData, dialogueUI));
    }

    private IEnumerator NPCConversationSequence(NPCConversationData data, ChoiceUIController dialogueUI)
    {
        // Go through each dialogue in sequence
        foreach (var dialogue in data.dialogueSequence)
        {
            string choice = null;
            bool choiceMade = false;
            bool npcAudioComplete = false;
        
            // Set choice audio callbacks
            dialogueUI.SetChoiceAudioAndCallbacks(dialogue.choiceAAudioClip, dialogue.choiceBAudioClip);
        
            // Show dialogue with audio
            dialogueUI.ShowDialogue(
                dialogue.npcText,
                dialogue.choiceA,
                dialogue.choiceB,
                dialogue.npcAudioClip,
                () => { npcAudioComplete = true; }, // Called when NPC audio finishes
                (selectedChoice) =>
                {
                    choice = selectedChoice;
                    choiceMade = true;
                }
            );
        
            // Wait for user to make a choice
            yield return new WaitUntil(() => choiceMade);
        
            // Small buffer before next dialogue
            yield return new WaitForSeconds(0.5f);
        }
    
        // Conversation complete
        LockPlayerMovement(false);
        Debug.Log("Conversation complete - player can move again");
    
        // Wait then show notification
        yield return new WaitForSeconds(data.delayBeforeTask);
        ShowNewTaskNotification(data.taskToAssign);
    }
}