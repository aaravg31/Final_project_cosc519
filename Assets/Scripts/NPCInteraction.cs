using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class NPCInteraction : MonoBehaviour
{
    [Header("References")]
    public ChoiceUIController dialogueUI;
    public MainGameScript gameManager;
    
    [Header("Conversation Data")]
    public NPCConversationData conversationData;
    
    [Header("NPC Type")]
    [Tooltip("If true, this NPC must be interacted with first before other NPCs become available")]
    public bool isFirstNPC = false;
    
    [Header("Interaction Settings")]
    public float interactionRadius = 3f;
    public bool requireClick = true;
    
    [Header("Visual Feedback - Interaction Ring")]
    [Tooltip("The InteractionRingController component on this NPC")]
    public InteractionRingController interactionRing;
    
    [Header("Optional Text Prompts (Legacy)")]
    public GameObject interactionPrompt;
    public GameObject lockedPrompt;
    
    private Transform playerTransform;
    private bool hasGreeted = false;
    private bool conversationActive = false;
    private bool playerInRange = false;

    void Start()
    {
        // Find the player (XR Origin)
        GameObject xrOrigin = GameObject.Find("XR Origin (VR) Main_A");
        if (xrOrigin != null)
        {
            playerTransform = xrOrigin.transform;
        }
        
        // Auto-find InteractionRingController if not assigned
        if (interactionRing == null)
        {
            interactionRing = GetComponent<InteractionRingController>();
        }
        
        // Hide legacy prompts if they exist
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        if (lockedPrompt != null)
        {
            lockedPrompt.SetActive(false);
        }
        
        // Hide ring initially
        if (interactionRing != null)
        {
            interactionRing.HideRing();
        }
    }

    void Update()
    {
        if (playerTransform == null || conversationActive || hasGreeted)
            return;
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distance <= interactionRadius)
        {
            if (!playerInRange)
            {
                OnPlayerEnterRange();
            }
            
            if (!requireClick)
            {
                StartGreeting();
            }
        }
        else
        {
            if (playerInRange)
            {
                OnPlayerExitRange();
            }
        }
    }

    private void OnPlayerEnterRange()
    {
        playerInRange = true;
        
        // Check if this is an optional NPC and if first NPC hasn't been interacted with yet
        bool isLocked = !isFirstNPC && gameManager != null && !gameManager.CanInteractWithOptionalNPC();
        
        if (isLocked)
        {
            // NPC is LOCKED - don't show ring
            if (interactionRing != null)
            {
                interactionRing.HideRing();
            }
            
            // Show locked prompt if using legacy system
            if (lockedPrompt != null)
            {
                lockedPrompt.SetActive(true);
            }
            
            Debug.Log($"NPC {gameObject.name} is locked - interact with first NPC first");
        }
        else
        {
            // NPC is UNLOCKED - show yellow ring
            if (interactionRing != null)
            {
                interactionRing.ShowRing();
            }
            
            // Show interaction prompt if using legacy system
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
        }
    }

    private void OnPlayerExitRange()
    {
        playerInRange = false;
        
        // Hide ring
        if (interactionRing != null)
        {
            interactionRing.HideRing();
        }
        
        // Hide legacy prompts
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        if (lockedPrompt != null)
        {
            lockedPrompt.SetActive(false);
        }
    }

    public void OnNPCClicked()
    {
        if (!playerInRange || conversationActive || hasGreeted)
            return;
        
        // Check if this NPC can be interacted with
        if (!isFirstNPC && gameManager != null && !gameManager.CanInteractWithOptionalNPC())
        {
            Debug.Log($"Cannot interact with {gameObject.name} yet - must interact with first NPC first");
            
            // Optional: Play a "locked" sound
            if (UISoundManager.Instance != null)
            {
                // You could add a locked sound here
                // UISoundManager.Instance.PlayLockedSound();
            }
            
            return;
        }
        
        StartGreeting();
    }

    private void StartGreeting()
    {
        if (conversationData == null)
        {
            Debug.LogError("No conversation data assigned to NPC!");
            return;
        }
    
        hasGreeted = true;
        conversationActive = true;
    
        // Hide ring when conversation starts
        if (interactionRing != null)
        {
            interactionRing.HideRing();
        }
    
        // Hide legacy prompts
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        if (lockedPrompt != null)
        {
            lockedPrompt.SetActive(false);
        }
    
        // Set dialogue UI to position near this NPC
        dialogueUI.SetTargetNPC(transform);
    
        // Lock player movement
        if (gameManager != null)
        {
            gameManager.LockPlayerMovement(true);
        }
    
        // Play greeting audio and get duration
        float greetingDuration = conversationData.greetingDisplayTime;
        if (conversationData.greetingAudioClip != null)
        {
            greetingDuration = DialogueSoundManager.Instance.PlayDialogueClip(conversationData.greetingAudioClip);
        }
    
        // Show greeting (no choices)
        dialogueUI.ShowDialogue(conversationData.greetingText, "", "", conversationData.greetingAudioClip, null, null);
    
        // Start main conversation after greeting audio
        Invoke("StartMainConversation", greetingDuration);
    }

    private void StartMainConversation()
    {
        if (gameManager != null)
        {
            // Pass isFirstNPC flag to game manager
            gameManager.StartNPCConversation(conversationData, dialogueUI, isFirstNPC);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw interaction radius
        Gizmos.color = isFirstNPC ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}