using UnityEngine;

/// <summary>
/// Minimal test script - just draws text on screen to confirm app is running.
/// No dependencies on AR or any other systems.
/// </summary>
public class MinimalStartupTest : MonoBehaviour
{
    private float startTime;
    
    void Awake()
    {
        startTime = Time.time;
        Debug.Log("MinimalStartupTest: Awake called!");
    }
    
    void Start()
    {
        Debug.Log("MinimalStartupTest: Start called!");
    }

    void OnGUI()
    {
        // Large red text in center of screen
        GUIStyle style = new GUIStyle();
        style.fontSize = 80;
        style.normal.textColor = Color.red;
        style.alignment = TextAnchor.MiddleCenter;
        
        float elapsed = Time.time - startTime;
        
        GUI.Label(new Rect(0, 100, Screen.width, 200), "APP IS RUNNING", style);
        GUI.Label(new Rect(0, 300, Screen.width, 100), $"Time: {elapsed:F1}s", style);
        
        // Also show screen size to prove we're rendering
        style.fontSize = 40;
        GUI.Label(new Rect(0, 450, Screen.width, 100), $"Screen: {Screen.width}x{Screen.height}", style);
    }
}
