using UnityEngine;
using UnityEngine.UIElements;

public class NotificationClick : MonoBehaviour
{
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        Button button1 = root.Q<Button>("notification-button");

        button1.clicked += () =>
        {
            FindFirstObjectByType<TaskPhoneManager>().TogglePhone();
            gameObject.SetActive(false);
        };
    }
}