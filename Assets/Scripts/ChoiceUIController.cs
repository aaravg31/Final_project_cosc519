using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System;

public class ChoiceUIController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    
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
        
        // Hide everything initially
        HideAll();
        
        // Register button callbacks
        choiceA.clicked += () => OnChoiceClicked(choiceA, choiceB, choiceAText.text);
        choiceB.clicked += () => OnChoiceClicked(choiceB, choiceA, choiceBText.text);
    }

    // Call this to show dialogue with choices
    public void ShowDialogue(string npcDialogue, string optionA, string optionB, Action<string> callback = null)
    {
        // Stop any running dialogue coroutines
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
        }

        // IMPORTANT: Reset the flag for new dialogue
        choicesMade = false;
        onChoiceSelected = callback;
        
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
        
        // Fade in NPC bubble
        StartCoroutine(FadeIn(npcBubble, 0.3f));
        
        // Show choices after 2 seconds
        currentDialogueCoroutine = StartCoroutine(ShowChoicesAfterDelay(2f));
    }

    private IEnumerator ShowChoicesAfterDelay(float delay)
    {
        Debug.Log($"ShowChoicesAfterDelay started, waiting {delay}s, choicesMade = {choicesMade}");
        yield return new WaitForSeconds(delay);
        
        Debug.Log($"After delay, choicesMade = {choicesMade}");
        
        if (!choicesMade)
        {
            Debug.Log("Showing choices container");
            choicesContainer.style.display = DisplayStyle.Flex;
            choicesContainer.style.opacity = 0;
            StartCoroutine(FadeIn(choicesContainer, 0.3f));
        }
        else
        {
            Debug.Log("NOT showing choices - choicesMade is true");
        }
    }

    private void OnChoiceClicked(Button selected, Button other, string selectedText)
    {
        if (choicesMade)
        {
            Debug.Log("Choice already made, ignoring click");
            return;
        }
        
        choicesMade = true;
        
        Debug.Log($"Choice selected: {selectedText}, setting choicesMade = true");
        
        // Fade out and hide the other choice
        StartCoroutine(FadeOutAndHide(other, 0.3f));
        
        // Keep selected choice visible for 2 seconds, then hide everything
        StartCoroutine(HideSelectedAfterDelay(selected, selectedText, 2f));
    }

    private IEnumerator HideSelectedAfterDelay(Button selected, string selectedText, float delay)
    {
        yield return new WaitForSeconds(delay);
        
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
        choicesMade = false; // Reset flag when hiding
    }
}