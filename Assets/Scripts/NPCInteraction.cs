using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class NPCInteraction : MonoBehaviour
{
    [Header("References")]
    public ChoiceUIController dialogueUI;
    public MainGameScript gameManager;
    
    [Header("Initial Greeting")]
    public string greetingText = "Hey!!";
    public float greetingDisplayTime = 2f;
    
    [Header("Interaction Settings")]
    public float interactionRadius = 3f; // How close player needs to be
    public bool requireClick = true; // If false, auto-triggers when in range
    
    [Header("Visual Feedback")]
    public GameObject interactionPrompt; // Optional: UI prompt saying "Press A to talk"
    
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
        else
        {
            Debug.LogError("XR Origin not found!");
        }
        
        // Hide interaction prompt initially
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    void Update()
    {
        if (playerTransform == null || conversationActive || hasGreeted)
            return;
        
        // Check distance to player
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distance <= interactionRadius)
        {
            if (!playerInRange)
            {
                OnPlayerEnterRange();
            }
            
            // If not requiring click, auto-start
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
        
        // Show interaction prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }
        
        Debug.Log("Player in range of NPC");
    }

    private void OnPlayerExitRange()
    {
        playerInRange = false;
        
        // Hide interaction prompt
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
        hasGreeted = true;
        conversationActive = true;
        
        // Hide interaction prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        Debug.Log("NPC: Hey!!");
        
        // Lock player movement
        if (gameManager != null)
        {
            gameManager.LockPlayerMovement(true);
        }
        
        // Show greeting (no choices)
        dialogueUI.ShowDialogue(greetingText, "", "", null);
        
        // After greeting time, start main conversation
        Invoke("StartMainConversation", greetingDisplayTime);
    }

    private void StartMainConversation()
    {
        if (gameManager != null)
        {
            gameManager.StartNPCConversation();
        }
    }

    // Visualize interaction radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}