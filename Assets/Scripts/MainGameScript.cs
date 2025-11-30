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
    private int interactionCount = 0;

    private void Start()
    {
        // Subscribe to phone close event
        if (phoneManager != null)
        {
            phoneManager.OnPhoneClosed += HandlePhoneClosed;
        }

        // Hide UIs at start
        if (notificationUI != null)
        {
            notificationUI.SetActive(false);
        }

        if (dialogueUI != null)
        {
            dialogueUI.HideAll();
        }

        // Lock player at start for intro
        LockPlayerMovement(true);
        
        Debug.Log("Game started - Playing intro");
        
        // Play intro sequence
        StartCoroutine(PlayIntroSequence());
    }

    private void OnDestroy()
    {
        if (phoneManager != null)
        {
            phoneManager.OnPhoneClosed -= HandlePhoneClosed;
        }
    }

    private IEnumerator PlayIntroSequence()
    {
        if (UISoundManager.Instance != null)
        {
            // Play intro audio and get duration
            float introDuration = UISoundManager.Instance.PlayIntroAudio();
            
            if (introDuration > 0)
            {
                Debug.Log($"Playing intro for {introDuration} seconds");
                
                // Wait for intro to finish
                yield return new WaitForSeconds(introDuration);
            }
            
            // Fade in background music
            UISoundManager.Instance.FadeBackgroundToNormal(2f);
        }
        
        // Wait for fade to complete
        yield return new WaitForSeconds(2f);
        
        // Unlock player movement
        LockPlayerMovement(false);
        Debug.Log("Intro complete - player can move");
    }

    private void ShowNewTaskNotification(string taskName)
    {
        Debug.Log($"Showing notification for task: {taskName}");
        
        if (notificationScript != null)
        {
            notificationScript.SetTask(taskName);
            notificationUI.SetActive(true);
        }
    }

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
            Debug.Log($"User rejected '{newTask}'");
            PerformActionForRejection(newTask);
        }
        
        if (interactionCount == 1)
        {
            StartCoroutine(HandleFirstInteractionComplete());
        }
        else if (interactionCount == 2)
        {
            StartCoroutine(HandleSecondInteractionComplete());
        }
    }

    private IEnumerator HandleFirstInteractionComplete()
    {
        Debug.Log("=== FIRST INTERACTION COMPLETE ===");
        
        // Play audio
        if (afterFirstInteractionAudio != null && UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayCustomClip(afterFirstInteractionAudio, 0.7f);
        }
        
        // Fade background music to anxious level
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.FadeBackgroundToAnxious(1f);
        }
        
        // Decrease sanity by 50
        yield return StartCoroutine(DecreaseSanityTo(50f));
        
        // Wait at low sanity
        yield return new WaitForSeconds(2f);
        
        // Restore sanity
        yield return StartCoroutine(RestoreSanityToMax());
        
        // Fade background music back to normal
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.FadeBackgroundToNormal(2f);
        }
        
        Debug.Log("First interaction complete");
    }

    private IEnumerator HandleSecondInteractionComplete()
    {
        Debug.Log("=== SECOND INTERACTION COMPLETE ===");
        
        // Play audio
        if (afterSecondInteractionAudio != null && UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayCustomClip(afterSecondInteractionAudio, 0.7f);
        }
        
        // Fade background music to anxious level
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.FadeBackgroundToAnxious(1f);
        }
        
        // Decrease sanity to 70
        yield return StartCoroutine(DecreaseSanityTo(70f));
        
        Debug.Log("Second interaction complete");
    }

    private IEnumerator DecreaseSanityTo(float targetSanity)
    {
        if (sanitySystem == null) yield break;

        Debug.Log($"Decreasing sanity to {targetSanity}");
        
        while (sanitySystem.currentSanity > targetSanity)
        {
            sanitySystem.ModifySanity(-sanityDecreaseSpeed * Time.deltaTime);
            yield return null;
        }
        
        sanitySystem.SetSanity(targetSanity);
        Debug.Log($"Sanity decreased to {sanitySystem.currentSanity}");
    }

    private IEnumerator RestoreSanityToMax()
    {
        if (sanitySystem == null) yield break;

        Debug.Log($"Restoring sanity to max");
        
        while (sanitySystem.currentSanity < sanitySystem.maxSanity)
        {
            sanitySystem.ModifySanity(sanityRecoverySpeed * Time.deltaTime);
            yield return null;
        }
        
        sanitySystem.SetSanity(sanitySystem.maxSanity);
        Debug.Log($"Sanity restored to {sanitySystem.currentSanity}");
    }

    private void PerformActionBasedOnSwap(string acceptedTask, string droppedTask)
    {
        Debug.Log($"Performing action: {droppedTask} replaced with {acceptedTask}");
    }

    private void PerformActionForRejection(string rejectedTask)
    {
        Debug.Log($"{rejectedTask} was rejected");
    }
    
    public void LockPlayerMovement(bool lockMovement)
    {
        movementLocked = lockMovement;
        
        if (moveProvider != null)
        {
            moveProvider.enabled = !lockMovement;
        }
        
        Debug.Log($"Player movement {(lockMovement ? "LOCKED" : "UNLOCKED")}");
    }

    public void StartNPCConversation(NPCConversationData conversationData, ChoiceUIController dialogueUI)
    {
        Debug.Log("Starting NPC conversation sequence");
        
        interactionCount++;
        Debug.Log($"Interaction #{interactionCount}");
        
        StartCoroutine(NPCConversationSequence(conversationData, dialogueUI));
    }

    private IEnumerator NPCConversationSequence(NPCConversationData data, ChoiceUIController dialogueUI)
    {
        foreach (var dialogue in data.dialogueSequence)
        {
            string choice = null;
            bool choiceMade = false;
            bool npcAudioComplete = false;
        
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
            
            dialogueUI.SetChoiceAudioAndCallbacks(dialogue.choiceAAudioClip, dialogue.choiceBAudioClip);
        
            yield return new WaitUntil(() => choiceMade);
            yield return new WaitForSeconds(0.5f);
        }
    
        LockPlayerMovement(false);
        Debug.Log("Conversation complete - player can move");
    
        if (interactionCount == 2 && secondNotificationAudio != null)
        {
            yield return new WaitForSeconds(0.5f);
            if (UISoundManager.Instance != null)
            {
                UISoundManager.Instance.PlayCustomClip(secondNotificationAudio, 0.7f);
            }
        }
    
        yield return new WaitForSeconds(data.delayBeforeTask);
        ShowNewTaskNotification(data.taskToAssign);
    }
}