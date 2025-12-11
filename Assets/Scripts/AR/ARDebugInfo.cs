using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARDebugInfo : MonoBehaviour
{
    private ARSession arSession;
    private ARPlacementManager placementManager;
    private SanitySystem sanitySystem;
    private ARAnxietyController anxietyController;
    
    void Start()
    {
        arSession = FindObjectOfType<ARSession>();
        placementManager = FindObjectOfType<ARPlacementManager>();
        sanitySystem = FindObjectOfType<SanitySystem>();
        anxietyController = FindObjectOfType<ARAnxietyController>();
    }

    void OnGUI()
    {
        // scale font size for high dpi screens
        int fontSize = 40;
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;
        style.normal.textColor = Color.yellow;

        GUILayout.BeginArea(new Rect(30, 30, Screen.width - 60, Screen.height - 60));
        
        GUILayout.Label($"Graphics: {SystemInfo.graphicsDeviceType}", style);
        
        if (arSession != null)
        {
            GUILayout.Label($"AR State: {ARSession.state}", style);
            GUILayout.Label($"Tracking: {ARSession.notTrackingReason}", style);
        }
        else 
        {
             GUILayout.Label("AR Session: NULL", style);
        }

        // NPC spawn info
        if (placementManager != null)
        {
            var npcs = placementManager.GetSpawnedNPCs();
            GUILayout.Label($"NPCs Spawned: {npcs.Count}", style);
            if (placementManager.npcPrefab == null)
            {
                GUILayout.Label("ERROR: NPC Prefab is NULL!", style);
            }
        }
        else
        {
            GUILayout.Label("PlacementManager: NULL", style);
        }

        // Sanity/Stress info
        if (sanitySystem != null)
        {
            float stress = 1f - (sanitySystem.currentSanity / sanitySystem.maxSanity);
            GUILayout.Label($"Sanity: {sanitySystem.currentSanity:F0}/{sanitySystem.maxSanity:F0}", style);
            GUILayout.Label($"Stress: {stress:P0}", style);
        }
        
        // Anxiety effect info
        if (anxietyController != null)
        {
            GUILayout.Label($"Effect Stress: {anxietyController.currentStress:P0}", style);
        }

        // Camera info
        if (Camera.main != null)
        {
            var cam = Camera.main.transform;
            GUILayout.Label($"Cam Pos: ({cam.position.x:F2}, {cam.position.y:F2}, {cam.position.z:F2})", style);
            GUILayout.Label($"Cam Rot: ({cam.eulerAngles.x:F0}, {cam.eulerAngles.y:F0}, {cam.eulerAngles.z:F0})", style);
        }

        GUILayout.EndArea();
    }
}

