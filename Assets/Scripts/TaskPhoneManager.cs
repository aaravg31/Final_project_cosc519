using UnityEngine;
using UnityEngine.UIElements;
using System;

public class TaskPhoneManager : MonoBehaviour
{
    [SerializeField] private UIDocument phoneUI;

    private VisualElement _root;
    private VisualElement _taskGrid;
    private VisualElement _newTaskGrid;
    
    // For swapping
    private VisualElement _firstSelected;
    private Label _firstSelectedLabel;
    
    // Track the new task and what was swapped
    private string _currentNewTask;
    private string _swappedOutTask;
    
    // Callback for when phone closes
    public event Action<string, string> OnPhoneClosed; // (newTask, swappedOutTask)

    private void Start()
    {
        _root = phoneUI.rootVisualElement;
        _taskGrid = _root.Q<VisualElement>("TaskGrid");
        _newTaskGrid = _root.Q<VisualElement>("NewTaskGrid");
    
        // Hide phone initially (CSS only)
        _root.style.display = DisplayStyle.None;
    
        // DON'T disable GameObject - remove this line if you have it:
        // gameObject.SetActive(false); 
    
        // Register all existing task boxes
        RegisterAllTaskBoxes();
    
        // Register exit button
        var exitButton = _root.Q<Button>("exit-button");
        if (exitButton != null)
        {
            exitButton.clicked += ClosePhone;
            Debug.Log("Exit button registered");
        }
    }

    // Call this to show phone with a specific new task
    public void ShowPhoneWithTask(string taskText)
    {
        // DON'T enable GameObject - remove this line if you have it:
        // gameObject.SetActive(true);
    
        _currentNewTask = taskText;
        _swappedOutTask = null; // Reset
    
        _root.style.display = DisplayStyle.Flex;
        AddNewTask(taskText);
        
        // Play phone open sound
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayPhoneOpenSound();
        }
    
        Debug.Log($"Phone opened with task: {taskText}");
    }

    public void TogglePhone()
    {
        bool isVisible = _root.style.display == DisplayStyle.Flex;
        
        if (isVisible)
        {
            ClosePhone();
        }
        else
        {
            // Show phone with default task
            ShowPhoneWithTask("Task 5");
        }
    }

    private void ClosePhone()
    {
        // Hide phone (CSS only)
        _root.style.display = DisplayStyle.None;

        // Clear selection
        ClearSelection();

        // Check what's in NewTaskSlot1 before clearing
        var newTaskSlot = _root.Q<VisualElement>("NewTaskSlot1");
        if (newTaskSlot != null)
        {
            var label = newTaskSlot.Q<Label>();
            string remainingTask = label != null ? label.text : null;

            newTaskSlot.Clear();
            Debug.Log($"Cleared NewTaskSlot1. Remaining task was: {remainingTask}");

            // Invoke callback with results - THIS WAS MISSING!
            OnPhoneClosed?.Invoke(_currentNewTask, _swappedOutTask);

            Debug.Log($"Phone closed - New task: {_currentNewTask}, Swapped out: {(_swappedOutTask ?? "none")}");
        }
    }

    private void AddNewTask(string taskText)
    {
        var newTaskSlot = _root.Q<VisualElement>("NewTaskSlot1");
        
        // Clear any existing content
        newTaskSlot.Clear();
        
        // Add new label
        var label = new Label(taskText);
        label.AddToClassList("task-label");
        label.pickingMode = PickingMode.Ignore;
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        newTaskSlot.Add(label);
        
        Debug.Log($"Added new task: {taskText}");
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
            
            // Track if new task was swapped with an existing task
            bool firstIsNewTask = (_firstSelectedLabel.text == _currentNewTask);
            bool secondIsNewTask = (clickedLabel.text == _currentNewTask);
            
            // Determine what was swapped out
            if (firstIsNewTask)
            {
                _swappedOutTask = clickedLabel.text;
                Debug.Log($"New task '{_currentNewTask}' replaced '{_swappedOutTask}'");
            }
            else if (secondIsNewTask)
            {
                _swappedOutTask = _firstSelectedLabel.text;
                Debug.Log($"New task '{_currentNewTask}' replaced '{_swappedOutTask}'");
            }
            
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