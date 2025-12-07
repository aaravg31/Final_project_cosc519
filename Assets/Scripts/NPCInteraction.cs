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
    private bool isCurrentlyLocked; // Track if NPC is currently locked (initialized in Start)

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
            if (interactionRing != null)
            {
                Debug.Log($"[{gameObject.name}] Auto-found InteractionRingController");
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] InteractionRingController NOT FOUND!");
            }
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
        
        // Set initial lock state: first NPC is never locked, optional NPCs start locked
        isCurrentlyLocked = !isFirstNPC;
        
        Debug.Log($"[{gameObject.name}] START - isFirstNPC: {isFirstNPC}, isCurrentlyLocked: {isCurrentlyLocked}");
        
        // Show ring immediately if this is the first NPC
        if (isFirstNPC)
        {
            Debug.Log($"[{gameObject.name}] This is the FIRST NPC - attempting to show ring...");
            
            if (interactionRing != null)
            {
                // Call ShowRing immediately
                interactionRing.ShowRing();
                Debug.Log($"[{gameObject.name}] ShowRing() called immediately");
                
                // FAILSAFE: Also call it again after a tiny delay to ensure it works
                Invoke("ForceShowRingForFirstNPC", 0.1f);
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] Cannot show ring - InteractionRingController is NULL!");
            }
        }
        else
        {
            Debug.Log($"[{gameObject.name}] This is an OPTIONAL NPC - ring will stay hidden until unlocked");
        }
    }
    
    /// <summary>
    /// Failsafe method to ensure first NPC ring is visible
    /// </summary>
    private void ForceShowRingForFirstNPC()
    {
        if (isFirstNPC && interactionRing != null && !hasGreeted)
        {
            interactionRing.ShowRing();
            Debug.Log($"[{gameObject.name}] FAILSAFE: Ring forced visible for first NPC");
        }
    }

    void Update()
    {
        if (playerTransform == null || conversationActive || hasGreeted)
            return;
        
        // SIMPLIFIED: Just update ring visibility every frame for optional NPCs
        // First NPC ring is shown in Start() and stays visible
        if (!isFirstNPC)
        {
            UpdateRingVisibility();
        }
        
        // Check distance for interaction
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

    /// <summary>
    /// Update ring visibility based on whether NPC is locked or unlocked
    /// ONLY CALLED FOR OPTIONAL NPCs
    /// </summary>
    private void UpdateRingVisibility()
    {
        // Check if first NPC has been completed
        bool canInteract = gameManager != null && gameManager.CanInteractWithOptionalNPC();
        
        // Only update if lock state changed
        if (canInteract != !isCurrentlyLocked)
        {
            isCurrentlyLocked = !canInteract;
            
            if (canInteract)
            {
                // NPC is UNLOCKED - show ring
                if (interactionRing != null)
                {
                    interactionRing.ShowRing();
                }
                Debug.Log($"NPC {gameObject.name} is now unlocked - ring visible");
            }
            else
            {
                // NPC is LOCKED - hide ring
                if (interactionRing != null)
                {
                    interactionRing.HideRing();
                }
                Debug.Log($"NPC {gameObject.name} is now locked - ring hidden");
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
            // Show locked prompt if using legacy system
            if (lockedPrompt != null)
            {
                lockedPrompt.SetActive(true);
            }
            
            Debug.Log($"NPC {gameObject.name} is locked - interact with first NPC first");
        }
        else
        {
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
        
        // Hide legacy prompts (but keep ring visible!)
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