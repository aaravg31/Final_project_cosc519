using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;

/// <summary>
/// ULTRA SIMPLE AR Test - Uses absolute world coordinates
/// This helps diagnose if AR tracking is working correctly
/// </summary>
public class ARSimpleTest : MonoBehaviour
{
    [Header("Test Settings")]
    public float spawnDelay = 2.0f;
    public float cubeSize = 0.3f;
    
    [Header("Debug Info (Read Only)")]
    public string spawnMethod = "";
    public Vector3 cubeWorldPosition;
    public bool cubeCreated = false;
    public string arSessionState = "Unknown";
    
    private GameObject testCube;
    private Transform arCamera;
    private ARSession arSession;

    void Start()
    {
        arCamera = Camera.main?.transform;
        arSession = FindObjectOfType<ARSession>();
        StartCoroutine(DelayedSpawn());
        
        Debug.Log("ARSimpleTest: Starting...");
    }

    System.Collections.IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(spawnDelay);
        CreateTestCube();
    }

    void CreateTestCube()
    {
        // Create cube at ABSOLUTE world origin with offset
        testCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        testCube.name = "AR_TestCube_WorldFixed";
        testCube.transform.localScale = Vector3.one * cubeSize;
        
        // NO PARENT - stay at world root
        testCube.transform.SetParent(null);
        
        // Position: 1.5m in front of where camera was at spawn time
        if (arCamera != null)
        {
            Vector3 camPos = arCamera.position;
            Vector3 camFwd = arCamera.forward;
            camFwd.y = 0;
            camFwd.Normalize();
            
            // Place at camera position + forward, at same height
            cubeWorldPosition = camPos + camFwd * 1.5f;
            cubeWorldPosition.y = camPos.y - 0.3f;
            
            testCube.transform.position = cubeWorldPosition;
            spawnMethod = "Camera-relative spawn";
        }
        else
        {
            // Fallback: absolute position
            cubeWorldPosition = new Vector3(0, 0.5f, 1.5f);
            testCube.transform.position = cubeWorldPosition;
            spawnMethod = "Absolute fallback (no camera)";
        }

        // Make it BRIGHT for visibility
        Renderer renderer = testCube.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Create unlit material that doesn't depend on lighting
            Material mat = null;
            
            // Try different shaders
            string[] shaders = {
                "Universal Render Pipeline/Unlit",
                "Unlit/Color",
                "Sprites/Default"
            };
            
            foreach (string shaderName in shaders)
            {
                Shader shader = Shader.Find(shaderName);
                if (shader != null)
                {
                    mat = new Material(shader);
                    break;
                }
            }
            
            if (mat != null)
            {
                mat.color = Color.green;
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", Color.green);
                renderer.material = mat;
            }
        }

        cubeCreated = true;
        Debug.Log($"ARSimpleTest: Created cube at {cubeWorldPosition} using {spawnMethod}");
    }

    void Update()
    {
        // Update AR session state for debugging
        if (arSession != null)
        {
            arSessionState = ARSession.state.ToString();
        }
    }

    void OnGUI()
    {
        // FIXED: Smaller font, positioned at bottom of screen
        int fontSize = 24;  // Fixed size, not scaled
        
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;
        style.normal.textColor = Color.yellow;

        float y = Screen.height - 300;  // Start from bottom
        float lineHeight = fontSize + 5;

        GUI.Label(new Rect(10, y, 800, 40), $"=== AR Test ===", style);
        y += lineHeight;
        
        GUI.Label(new Rect(10, y, 800, 40), $"AR: {arSessionState}", style);
        y += lineHeight;
        
        if (cubeCreated && testCube != null)
        {
            Vector3 currentPos = testCube.transform.position;
            float drift = Vector3.Distance(currentPos, cubeWorldPosition);
            
            style.normal.textColor = drift > 0.05f ? Color.red : Color.green;
            GUI.Label(new Rect(10, y, 800, 40), $"Drift: {drift:F3}m {(drift > 0.1f ? "BAD" : "OK")}", style);
            y += lineHeight;
        }
        
        if (arCamera != null)
        {
            style.normal.textColor = Color.cyan;
            GUI.Label(new Rect(10, y, 800, 40), $"Cam: {arCamera.position:F1}", style);
        }
    }
}
