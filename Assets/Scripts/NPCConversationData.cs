using UnityEngine;

[CreateAssetMenu(fileName = "NPCConversation", menuName = "Game/NPC Conversation")]
public class NPCConversationData : ScriptableObject
{
    [System.Serializable]
    public class DialogueNode
    {
        [TextArea(2, 4)]
        public string npcText;
        public string choiceA;
        public string choiceB;
        
        [Header("Next Nodes (Optional - for branching)")]
        public DialogueNode nextIfChoiceA;
        public DialogueNode nextIfChoiceB;
    }
    
    [Header("Initial Greeting")]
    public string greetingText = "Hey!!";
    public float greetingDisplayTime = 2f;
    
    [Header("Conversation Flow")]
    public DialogueNode[] dialogueSequence;
    
    [Header("Task After Conversation")]
    public string taskToAssign = "Complete homework";
    public float delayBeforeTask = 2.5f;
}