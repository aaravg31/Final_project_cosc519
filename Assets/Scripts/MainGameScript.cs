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
    [SerializeField] private CharacterController characterController;
    [SerializeField] private ContinuousMoveProvider moveProvider;
    
    [Header("Game Flow Audio")]
    [SerializeField] private AudioClip afterFirstInteractionAudio;
    [SerializeField] private AudioClip afterSecondInteractionAudio;
    [SerializeField] private AudioClip secondNotificationAudio;
    
    [Header("Sanity Settings")]
    [SerializeField] private float sanityDecreaseSpeed = 2f;
    [SerializeField] private float sanityRecoverySpeed = 2f;
    
    private bool movementLocked = false;
    private int interactionCount = 0; // Track which NPC interaction

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
        
        // Handle different interactions based on count
        if (interactionCount == 1)
        {
            // After FIRST interaction (first phone close)
            StartCoroutine(HandleFirstInteractionComplete());
        }
        else if (interactionCount == 2)
        {
            // After SECOND interaction (second phone close)
            StartCoroutine(HandleSecondInteractionComplete());
        }
    }

    private IEnumerator HandleFirstInteractionComplete()
    {
        Debug.Log("=== FIRST INTERACTION COMPLETE ===");
        
        // Play audio for first interaction
        if (afterFirstInteractionAudio != null && UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayCustomClip(afterFirstInteractionAudio, 0.7f);
        }
        
        // Decrease sanity by 50 (from 100 to 50)
        yield return StartCoroutine(DecreaseSanityTo(50f));
        
        // Wait a bit at low sanity
        yield return new WaitForSeconds(2f);
        
        // Restore sanity back to 100
        yield return StartCoroutine(RestoreSanityToMax());
        
        Debug.Log("First interaction sanity cycle complete");
    }

    private IEnumerator HandleSecondInteractionComplete()
    {
        Debug.Log("=== SECOND INTERACTION COMPLETE ===");
        
        // Play audio right after second interaction ends
        if (afterSecondInteractionAudio != null && UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayCustomClip(afterSecondInteractionAudio, 0.7f);
        }
        
        // Decrease sanity by 30 (from 100 to 70)
        yield return StartCoroutine(DecreaseSanityTo(70f));
        
        Debug.Log("Second interaction sanity decrease complete");
    }

    private IEnumerator DecreaseSanityTo(float targetSanity)
    {
        if (sanitySystem == null)
        {
            Debug.LogError("Cannot modify sanity - SanitySystem not assigned!");
            yield break;
        }

        Debug.Log($"Decreasing sanity from {sanitySystem.currentSanity} to {targetSanity}");
        
        // Gradually decrease sanity
        while (sanitySystem.currentSanity > targetSanity)
        {
            sanitySystem.ModifySanity(-sanityDecreaseSpeed * Time.deltaTime);
            yield return null;
        }
        
        // Ensure exact value
        sanitySystem.SetSanity(targetSanity);
        Debug.Log($"Sanity decreased to {sanitySystem.currentSanity}");
    }

    private IEnumerator RestoreSanityToMax()
    {
        if (sanitySystem == null)
        {
            Debug.LogError("Cannot modify sanity - SanitySystem not assigned!");
            yield break;
        }

        Debug.Log($"Restoring sanity from {sanitySystem.currentSanity} to {sanitySystem.maxSanity}");
        
        // Gradually restore sanity
        while (sanitySystem.currentSanity < sanitySystem.maxSanity)
        {
            sanitySystem.ModifySanity(sanityRecoverySpeed * Time.deltaTime);
            yield return null;
        }
        
        // Ensure exact max value
        sanitySystem.SetSanity(sanitySystem.maxSanity);
        Debug.Log($"Sanity restored to {sanitySystem.currentSanity}");
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
        
        // Increment interaction count
        interactionCount++;
        Debug.Log($"This is interaction #{interactionCount}");
        
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
    
            // Show dialogue with audio FIRST
            dialogueUI.ShowDialogue(
                dialogue.npcText,
                dialogue.choiceA,
                dialogue.choiceB,
                dialogue.npcAudioClip,
                () => { npcAudioComplete = true; },
                (selectedChoice) =>
                {
                    choice = selectedChoice;
                    choiceMade = true;
                }
            );
        
            // THEN set choice audio callbacks AFTER ShowDialogue
            dialogueUI.SetChoiceAudioAndCallbacks(dialogue.choiceAAudioClip, dialogue.choiceBAudioClip);
    
            // Wait for user to make a choice
            yield return new WaitUntil(() => choiceMade);
    
            // Small buffer before next dialogue
            yield return new WaitForSeconds(0.5f);
        }

        // Conversation complete
        LockPlayerMovement(false);
        Debug.Log("Conversation complete - player can move again");

        // Play audio for second notification ONLY on second interaction
        if (interactionCount == 2 && secondNotificationAudio != null)
        {
            yield return new WaitForSeconds(0.5f);
            if (UISoundManager.Instance != null)
            {
                UISoundManager.Instance.PlayCustomClip(secondNotificationAudio, 0.7f);
            }
        }

        // Wait then show notification
        yield return new WaitForSeconds(data.delayBeforeTask);
        ShowNewTaskNotification(data.taskToAssign);
    }
}