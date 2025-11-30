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
    
    [Header("Random Anxiety Audio (1 second clips)")]
    [SerializeField] private AudioClip anxietyClip1;
    [SerializeField] private AudioClip anxietyClip2;
    [SerializeField] private AudioClip anxietyClip3;
    [SerializeField] private AudioClip anxietyClip4;
    
    [Header("Sanity Settings")]
    [SerializeField] private float sanityDecreaseSpeed = 2f;
    [SerializeField] private float sanityRecoverySpeed = 2f;
    
    private bool movementLocked = false;
    private int interactionCount = 0;
    private AudioClip[] anxietyClips;

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

        // Store anxiety clips in array for random selection
        anxietyClips = new AudioClip[] { anxietyClip1, anxietyClip2, anxietyClip3, anxietyClip4 };

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
            float introDuration = UISoundManager.Instance.PlayIntroAudio();
            
            if (introDuration > 0)
            {
                Debug.Log($"Playing intro for {introDuration} seconds");
                yield return new WaitForSeconds(introDuration);
            }
            
            UISoundManager.Instance.FadeBackgroundToNormal(2f);
        }
        
        yield return new WaitForSeconds(2f);
        
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
        
        // Decrease sanity to 0 (100% stress)
        yield return StartCoroutine(DecreaseSanityTo(0f));
        
        Debug.Log("Starting random anxiety audio sequence (20 seconds)");
        
        // Play random anxiety clips for 20 seconds (10 clips, 2 seconds apart)
        yield return StartCoroutine(PlayRandomAnxietyClips(20f, 2f));
        
        Debug.Log("Anxiety sequence complete - restoring sanity");
        
        // Restore sanity back to 100
        yield return StartCoroutine(RestoreSanityToMax());
        
        // Fade background back to normal
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.FadeBackgroundToNormal(2f);
        }
        
        yield return new WaitForSeconds(2f);
        
        Debug.Log("Playing ending audio sequence");
        
        // Lock player for ending
        LockPlayerMovement(true);
        
        // Play ending audio clips back-to-back
        yield return StartCoroutine(PlayEndingSequence());
        
        // Unlock player after ending
        LockPlayerMovement(false);
        
        Debug.Log("Second interaction complete - Game sequence finished!");
    }

    private IEnumerator PlayRandomAnxietyClips(float totalDuration, float interval)
    {
        float elapsed = 0f;
        int clipCount = 0;
        
        while (elapsed < totalDuration)
        {
            // Pick random clip
            AudioClip randomClip = anxietyClips[Random.Range(0, anxietyClips.Length)];
            
            if (randomClip != null && UISoundManager.Instance != null)
            {
                UISoundManager.Instance.PlayCustomClip(randomClip, 0.8f);
                clipCount++;
                Debug.Log($"Playing random anxiety clip #{clipCount}: {randomClip.name}");
            }
            
            // Wait for interval
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
        
        Debug.Log($"Played {clipCount} random anxiety clips over {totalDuration} seconds");
    }

    private IEnumerator PlayEndingSequence()
    {
        if (UISoundManager.Instance != null)
        {
            // Play first ending audio
            float audio1Duration = 0f;
            if (UISoundManager.Instance.endingAudio1 != null)
            {
                audio1Duration = UISoundManager.Instance.endingAudio1.length;
                UISoundManager.Instance.PlayEndingAudioSequence();
                
                Debug.Log($"Playing ending audio 1, duration: {audio1Duration}s");
                yield return new WaitForSeconds(audio1Duration);
            }
            
            // Play second ending audio
            if (UISoundManager.Instance.endingAudio2 != null)
            {
                float audio2Duration = UISoundManager.Instance.endingAudio2.length;
                UISoundManager.Instance.PlayEndingAudio2();
                
                Debug.Log($"Playing ending audio 2, duration: {audio2Duration}s");
                yield return new WaitForSeconds(audio2Duration);
            }
        }
        
        Debug.Log("Ending sequence complete");
    }

    private IEnumerator DecreaseSanityTo(float targetSanity)
    {
        if (sanitySystem == null) yield break;

        Debug.Log($"Decreasing sanity from {sanitySystem.currentSanity} to {targetSanity}");
        
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

        Debug.Log($"Restoring sanity from {sanitySystem.currentSanity} to max");
        
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