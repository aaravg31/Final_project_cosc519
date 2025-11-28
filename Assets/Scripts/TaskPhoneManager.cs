using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class TaskPhoneManager : MonoBehaviour
{
    [SerializeField] private UIDocument phoneUI;

    private VisualElement _root;
    private VisualElement _taskGrid;     // My Tasks grid (4 slots)
    private VisualElement _newTaskGrid;  // New Tasks grid (2 slots)
    private bool _isVisible;
    
    // For swapping logic
    private VisualElement _firstSelectedTask;
    private VisualElement _firstSelectedSlot;

    private void Start()
    {
        _root = phoneUI.rootVisualElement;
        _taskGrid = _root.Q<VisualElement>("TaskGrid");
        _newTaskGrid = _root.Q<VisualElement>("NewTaskGrid");
        _root.style.display = DisplayStyle.None;

        // Register click handlers for existing tasks inside TaskGrid
        foreach (var slot in _taskGrid.Children())
        {
            var taskBox = slot.Q<VisualElement>(className: "task-box");
            if (taskBox != null)
                RegisterClickHandler(taskBox, slot);
        }
    }

    public void TogglePhone()
    {
        _isVisible = !_isVisible;
        _root.style.display = _isVisible ? DisplayStyle.Flex : DisplayStyle.None;

        if (_isVisible)
            TryAddNewTask();
        else
        {
            ClearNewTasks();
            ClearSelection();
        }
    }

    private void TryAddNewTask()
    {
        // Look for the first empty slot in NewTaskGrid
        foreach (var slot in _newTaskGrid.Children())
        {
            if (SlotIsEmpty(slot))
            {
                var newTask = CreateTaskBox("Task 5");
                slot.Add(newTask);
                RegisterClickHandler(newTask, slot);
                return;
            }
        }

        Debug.Log("No empty slot in NewTaskGrid!");
    }

    private void ClearNewTasks()
    {
        foreach (var slot in _newTaskGrid.Children())
            slot.Clear();
    }

    private VisualElement CreateTaskBox(string label)
    {
        var box = new VisualElement();
        box.AddToClassList("task-box");

        var lbl = new Label(label);
        lbl.style.unityTextAlign = TextAnchor.MiddleCenter;
        lbl.pickingMode = PickingMode.Ignore; // Prevent label from blocking clicks
        box.Add(lbl);

        return box;
    }

    // Check if a slot contains a task box
    private static bool SlotIsEmpty(VisualElement slot)
    {
        return slot.Q<VisualElement>(className: "task-box") == null;
    }

    private void RegisterClickHandler(VisualElement task, VisualElement parentSlot)
    {
        task.RegisterCallback<ClickEvent>(evt =>
        {
            HandleTaskClick(task, parentSlot);
        });
    }

    private void HandleTaskClick(VisualElement clickedTask, VisualElement clickedSlot)
    {
        // First click - select this task
        if (_firstSelectedTask == null)
        {
            _firstSelectedTask = clickedTask;
            _firstSelectedSlot = clickedSlot;
            
            // Visual feedback - highlight selected task
            _firstSelectedTask.AddToClassList("task-selected");
            
            Debug.Log($"First task selected: {GetTaskLabel(_firstSelectedTask)}");
        }
        // Second click - swap if it's a different task
        else if (_firstSelectedTask != clickedTask)
        {
            Debug.Log($"Swapping {GetTaskLabel(_firstSelectedTask)} with {GetTaskLabel(clickedTask)}");
            
            // Check if one is from NewTaskGrid and one is from TaskGrid
            bool firstIsNew = IsInNewTaskGrid(_firstSelectedSlot);
            bool secondIsNew = IsInNewTaskGrid(clickedSlot);
            
            // Only allow swap if they're from different grids
            if (firstIsNew != secondIsNew)
            {
                SwapTasks(_firstSelectedTask, _firstSelectedSlot, clickedTask, clickedSlot);
            }
            else
            {
                Debug.Log("Can only swap between New Tasks and My Tasks!");
            }
            
            // Clear selection
            ClearSelection();
        }
        // Same task clicked again - deselect
        else
        {
            Debug.Log("Same task clicked - deselecting");
            ClearSelection();
        }
    }

    private void SwapTasks(VisualElement task1, VisualElement slot1, VisualElement task2, VisualElement slot2)
    {
        // Remove both tasks from their slots
        slot1.Remove(task1);
        slot2.Remove(task2);
        
        // Add them to each other's slots
        slot1.Add(task2);
        slot2.Add(task1);
        
        // Re-register click handlers with new parent slots
        RegisterClickHandler(task1, slot2);
        RegisterClickHandler(task2, slot1);
        
        Debug.Log("Swap completed!");
    }

    private void ClearSelection()
    {
        if (_firstSelectedTask != null)
        {
            _firstSelectedTask.RemoveFromClassList("task-selected");
            _firstSelectedTask = null;
            _firstSelectedSlot = null;
        }
    }

    private bool IsInNewTaskGrid(VisualElement slot)
    {
        // Check if the slot is a child of NewTaskGrid
        return _newTaskGrid.Contains(slot);
    }

    private string GetTaskLabel(VisualElement task)
    {
        var label = task.Q<Label>();
        return label != null ? label.text : "Unknown";
    }
}