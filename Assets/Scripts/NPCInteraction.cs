using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class NPCInteraction : MonoBehaviour
{
    [Header("References")]
    public ChoiceUIController dialogueUI;
    public MainGameScript gameManager;
    
    [Header("Conversation Data")]
    public NPCConversationData conversationData;
    
    [Header("Interaction Settings")]
    public float interactionRadius = 3f;
    public bool requireClick = true;
    
    [Header("Visual Feedback")]
    public GameObject interactionPrompt;
    
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
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
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
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }
    }

    private void OnPlayerExitRange()
    {
        playerInRange = false;
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    public void OnNPCClicked()
    {
        if (playerInRange && !conversationActive && !hasGreeted)
        {
            StartGreeting();
        }
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
    
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
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
            gameManager.StartNPCConversation(conversationData, dialogueUI);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}