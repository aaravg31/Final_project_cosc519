using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System;

public class ChoiceUIController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    
    [Header("Positioning")]
    [SerializeField] private Transform targetNPC;
    [SerializeField] private Vector3 offsetFromNPC = new Vector3(2f, 5f, 0f);
    [SerializeField] private bool followNPC = false;
    
    private VisualElement root;
    private VisualElement npcBubble;
    private Label npcText;
    private VisualElement choicesContainer;
    private Button choiceA;
    private Button choiceB;
    private Label choiceAText;
    private Label choiceBText;
    
    private bool choicesMade = false;
    private Action<string> onChoiceSelected;
    private Coroutine currentDialogueCoroutine;
    private Transform mainCamera;
    private System.Action onAudioComplete;
    
    // Store current choice actions so we can unregister them
    private System.Action currentChoiceAAction;
    private System.Action currentChoiceBAction;

    private void Start()
    {
        root = uiDocument.rootVisualElement;
    
        // Get references to UI elements
        npcBubble = root.Q<VisualElement>("npc-bubble");
        npcText = root.Q<Label>("npc-text");
        choicesContainer = root.Q<VisualElement>("choices-container");
        choiceA = root.Q<Button>("choice-a");
        choiceB = root.Q<Button>("choice-b");
        choiceAText = root.Q<Label>("choice-a-text");
        choiceBText = root.Q<Label>("choice-b-text");
    
        // Find main camera
        mainCamera = Camera.main.transform;
    
        // Hide everything initially
        HideAll();
    }

    private void LateUpdate()
    {
        // Position dialogue UI near NPC if target is set
        if (targetNPC != null && (choicesContainer.style.display == DisplayStyle.Flex || npcBubble.style.display == DisplayStyle.Flex))
        {
            PositionNearNPC();
        }
    }

    private void PositionNearNPC()
    {
        if (targetNPC == null || mainCamera == null) return;
        
        // Position to the right of NPC
        Vector3 targetPosition = targetNPC.position + targetNPC.right * offsetFromNPC.x 
                                                      + targetNPC.up * offsetFromNPC.y 
                                                      + targetNPC.forward * offsetFromNPC.z;
        
        transform.position = targetPosition;
        
        // Face the camera
        transform.LookAt(mainCamera);
        transform.Rotate(0, 180, 0);
    }

    public void SetTargetNPC(Transform npc)
    {
        targetNPC = npc;
        if (npc != null)
        {
            PositionNearNPC();
        }
    }

    public void ShowDialogue(string npcDialogue, string optionA, string optionB, AudioClip npcAudio, System.Action onNPCAudioComplete, Action<string> callback = null)
    {
        // Stop any running dialogue coroutines
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
        }
        
        // Unregister old callbacks
        UnregisterChoiceCallbacks();

        // Reset state
        choicesMade = false;
        onChoiceSelected = callback;
        onAudioComplete = onNPCAudioComplete;
    
        Debug.Log($"ShowDialogue called - choicesMade reset to false");
    
        // Set text
        npcText.text = npcDialogue;
        choiceAText.text = optionA;
        choiceBText.text = optionB;
    
        // Reset button states
        choiceA.style.display = DisplayStyle.Flex;
        choiceB.style.display = DisplayStyle.Flex;
        choiceA.style.opacity = 1;
        choiceB.style.opacity = 1;
    
        // Show NPC bubble immediately
        npcBubble.style.display = DisplayStyle.Flex;
        npcBubble.style.opacity = 0;
    
        // Hide choices initially
        choicesContainer.style.display = DisplayStyle.None;
    
        // Position near NPC
        if (targetNPC != null)
        {
            PositionNearNPC();
        }
    
        // Fade in NPC bubble
        StartCoroutine(FadeIn(npcBubble, 0.3f));
    
        // Play audio and wait for it to finish before showing choices
        if (npcAudio != null || (!string.IsNullOrEmpty(optionA) && !string.IsNullOrEmpty(optionB)))
        {
            float audioLength = 0f;
        
            if (npcAudio != null)
            {
                audioLength = DialogueSoundManager.Instance.PlayDialogueClip(npcAudio);
            }
            else
            {
                audioLength = DialogueSoundManager.Instance.defaultWaitTime;
            }
        
            currentDialogueCoroutine = StartCoroutine(ShowChoicesAfterAudio(audioLength, optionA, optionB));
        }
    }
    
    public void SetChoiceAudioAndCallbacks(AudioClip choiceAAudio, AudioClip choiceBAudio)
    {
        // Unregister old callbacks first
        UnregisterChoiceCallbacks();
        
        Debug.Log($"Setting choice callbacks - A: {(choiceAAudio != null ? choiceAAudio.name : "NULL")}, B: {(choiceBAudio != null ? choiceBAudio.name : "NULL")}");
        
        // Create new actions with the correct audio
        currentChoiceAAction = () => OnChoiceClicked(choiceA, choiceB, choiceAText.text, choiceAAudio);
        currentChoiceBAction = () => OnChoiceClicked(choiceB, choiceA, choiceBText.text, choiceBAudio);
        
        // Register new callbacks
        choiceA.clicked += currentChoiceAAction;
        choiceB.clicked += currentChoiceBAction;
    }
    
    private void UnregisterChoiceCallbacks()
    {
        // Remove old callbacks if they exist
        if (currentChoiceAAction != null)
        {
            choiceA.clicked -= currentChoiceAAction;
            currentChoiceAAction = null;
        }
        
        if (currentChoiceBAction != null)
        {
            choiceB.clicked -= currentChoiceBAction;
            currentChoiceBAction = null;
        }
    }
    
    private IEnumerator ShowChoicesAfterAudio(float audioLength, string optionA, string optionB)
    {
        Debug.Log($"Waiting {audioLength}s for audio to complete");
        yield return new WaitForSeconds(audioLength);
    
        // Notify that NPC audio is complete
        onAudioComplete?.Invoke();
    
        // Only show choices if there are actual choices to make
        if (!string.IsNullOrEmpty(optionA) && !string.IsNullOrEmpty(optionB))
        {
            Debug.Log("Audio complete, showing choices");
            if (!choicesMade)
            {
                choicesContainer.style.display = DisplayStyle.Flex;
                choicesContainer.style.opacity = 0;
                StartCoroutine(FadeIn(choicesContainer, 0.3f));
            }
        }
        else
        {
            // No choices, just wait and clear
            yield return new WaitForSeconds(0.5f);
            HideAll();
        }
    }

    private void OnChoiceClicked(Button selected, Button other, string selectedText, AudioClip choiceAudio)
    {
        Debug.Log($"=== OnChoiceClicked - Text: {selectedText}, Audio: {(choiceAudio != null ? choiceAudio.name : "NULL")} ===");
        
        if (choicesMade)
        {
            Debug.Log("Choice already made, ignoring click");
            return;
        }
    
        choicesMade = true;
    
        Debug.Log($"Choice selected: {selectedText}");
    
        // Fade out the other choice immediately
        StartCoroutine(FadeOutAndHide(other, 0.3f));
    
        // Play choice audio and wait for it
        float audioLength = DialogueSoundManager.Instance.PlayDialogueClip(choiceAudio);
    
        // Keep selected choice visible during audio, then hide
        StartCoroutine(HideSelectedAfterAudio(selected, selectedText, audioLength));
    }
    
    private IEnumerator HideSelectedAfterAudio(Button selected, string selectedText, float audioLength)
    {
        yield return new WaitForSeconds(audioLength);
    
        // Fade out selected choice
        StartCoroutine(FadeOutAndHide(selected, 0.3f));
    
        // Fade out NPC bubble
        StartCoroutine(FadeOutAndHide(npcBubble, 0.3f));
    
        // After fading, hide the choices container
        yield return new WaitForSeconds(0.3f);
        choicesContainer.style.display = DisplayStyle.None;
    
        // Call the callback with the selected choice
        Debug.Log($"Calling callback with choice: {selectedText}");
        onChoiceSelected?.Invoke(selectedText);
    }

    private IEnumerator FadeIn(VisualElement element, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float opacity = Mathf.Lerp(0f, 1f, elapsed / duration);
            element.style.opacity = opacity;
            yield return null;
        }
        
        element.style.opacity = 1f;
    }

    private IEnumerator FadeOutAndHide(VisualElement element, float duration)
    {
        float elapsed = 0f;
        float startOpacity = element.style.opacity.value;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float opacity = Mathf.Lerp(startOpacity, 0f, elapsed / duration);
            element.style.opacity = opacity;
            yield return null;
        }
        
        element.style.opacity = 0f;
        element.style.display = DisplayStyle.None;
    }

    public void HideAll()
    {
        Debug.Log("HideAll called");
        if (npcBubble != null) npcBubble.style.display = DisplayStyle.None;
        if (choicesContainer != null) choicesContainer.style.display = DisplayStyle.None;
        choicesMade = false;
        targetNPC = null;
        
        // Unregister callbacks when hiding
        UnregisterChoiceCallbacks();
    }
}