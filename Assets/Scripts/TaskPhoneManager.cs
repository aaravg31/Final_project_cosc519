using UnityEngine;
using UnityEngine.UIElements;

public class TaskPhoneManager : MonoBehaviour
{
    [SerializeField] private UIDocument phoneUI;

    private VisualElement _root;
    private VisualElement _taskGrid;
    private VisualElement _newTaskGrid;
    
    // For swapping
    private VisualElement _firstSelected;
    private Label _firstSelectedLabel;

    private void Start()
    {
        _root = phoneUI.rootVisualElement;
        _taskGrid = _root.Q<VisualElement>("TaskGrid");
        _newTaskGrid = _root.Q<VisualElement>("NewTaskGrid");
        
        // Hide phone initially
        _root.style.display = DisplayStyle.None;
        
        // Register all existing task boxes
        RegisterAllTaskBoxes();
    }

    public void TogglePhone()
    {
        bool isVisible = _root.style.display == DisplayStyle.Flex;
        
        if (isVisible)
        {
            // Hide phone
            _root.style.display = DisplayStyle.None;
            ClearSelection();
        }
        else
        {
            // Show phone and add new task
            _root.style.display = DisplayStyle.Flex;
            AddNewTask();
        }
    }

    private void AddNewTask()
    {
        var newTaskSlot = _root.Q<VisualElement>("NewTaskSlot1");
        
        // Clear any existing content
        newTaskSlot.Clear();
        
        // Add new label
        var label = new Label("Task 5");
        label.AddToClassList("task-label");
        label.pickingMode = PickingMode.Ignore;
        newTaskSlot.Add(label);
        
        Debug.Log("Task 5 added to NewTaskSlot1");
    }

    private void RegisterAllTaskBoxes()
    {
        // Register task boxes in TaskGrid
        RegisterTaskBox("TaskSlot1");
        RegisterTaskBox("TaskSlot2");
        RegisterTaskBox("TaskSlot3");
        RegisterTaskBox("TaskSlot4");
        
        // Register task boxes in NewTaskGrid
        RegisterTaskBox("NewTaskSlot1");
        RegisterTaskBox("NewTaskSlot2");
    }

    private void RegisterTaskBox(string slotName)
    {
        var taskBox = _root.Q<VisualElement>(slotName);
        
        if (taskBox == null)
        {
            Debug.LogWarning($"Task box {slotName} not found!");
            return;
        }
        
        // Make it clickable
        taskBox.pickingMode = PickingMode.Position;
        taskBox.focusable = true;
        
        // Register click event
        taskBox.RegisterCallback<ClickEvent>(evt =>
        {
            OnTaskBoxClicked(taskBox);
            evt.StopPropagation();
        });
        
        // Add hover effects
        taskBox.RegisterCallback<MouseEnterEvent>(evt =>
        {
            if (taskBox != _firstSelected)
            {
                taskBox.AddToClassList("task-hover");
            }
        });
        
        taskBox.RegisterCallback<MouseLeaveEvent>(evt =>
        {
            taskBox.RemoveFromClassList("task-hover");
        });
    }

    private void OnTaskBoxClicked(VisualElement clickedBox)
    {
        var clickedLabel = clickedBox.Q<Label>();
        
        // If no label (empty slot), ignore
        if (clickedLabel == null)
        {
            Debug.Log("Clicked empty slot");
            return;
        }
        
        Debug.Log($"Clicked: {clickedLabel.text}");
        
        // First selection
        if (_firstSelected == null)
        {
            _firstSelected = clickedBox;
            _firstSelectedLabel = clickedLabel;
            
            // Highlight it
            _firstSelected.AddToClassList("task-selected");
            
            Debug.Log($"Selected: {clickedLabel.text}");
        }
        // Second selection - swap
        else if (_firstSelected != clickedBox)
        {
            Debug.Log($"Swapping {_firstSelectedLabel.text} with {clickedLabel.text}");
            
            // Swap the label texts
            string tempText = _firstSelectedLabel.text;
            _firstSelectedLabel.text = clickedLabel.text;
            clickedLabel.text = tempText;
            
            Debug.Log("Swap complete!");
            
            // Clear selection
            ClearSelection();
        }
        // Same box clicked - deselect
        else
        {
            Debug.Log("Deselected");
            ClearSelection();
        }
    }

    private void ClearSelection()
    {
        if (_firstSelected != null)
        {
            _firstSelected.RemoveFromClassList("task-selected");
            _firstSelected = null;
            _firstSelectedLabel = null;
        }
    }
}