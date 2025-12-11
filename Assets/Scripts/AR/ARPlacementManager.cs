using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// AR NPC Placement Manager
/// Spawns NPCs in front of the player in AR world space.
/// NPCs remain fixed in world space - player walks toward/away from them.
/// Works with ARSanityManager for the anxiety/comfort system.
/// </summary>
public class ARPlacementManager : MonoBehaviour
{
    [Header("AR References")]
    [Tooltip("The XR Origin for coordinate system. Will auto-find if null.")]
    public XROrigin xrOrigin;
    [Tooltip("The AR Camera. If null, will try to find Camera.main.")]
    public Transform arCamera;

    [Header("Spawning")]
    public GameObject npcPrefab;
    [Tooltip("How many NPCs to spawn.")]
    public int spawnCount = 1;  // Changed from 3 to 1 for testing
    [Tooltip("Distance in front of player to spawn NPCs (meters).")]
    public float spawnDistance = 1.5f;  // Changed from 3.0 for indoor testing
    [Tooltip("Spread angle for multiple NPCs (degrees).")]
    public float spreadAngle = 120f;
    [Tooltip("Height of NPCs in world space (0 = ground level).")]
    public float spawnHeight = 0f;
    [Tooltip("Scale factor for spawned NPCs (1.0 = original size).")]
    public float npcScale = 1.0f;
    [Tooltip("Delay before spawning NPCs (seconds). Allows AR tracking to stabilize.")]
    public float spawnDelay = 2.0f;
    
    [Header("Glow Effect")]
    [Tooltip("Enable enhanced glow effect on NPCs.")]
    public bool enableGlowEffect = true;
    [Tooltip("Enable safety zone circle around NPCs.")]
    public bool enableSafetyZone = true;
    [Tooltip("Safety zone radius (should match comfort distance).")]
    public float safetyZoneRadius = 0.8f;  // Changed from 2.0 for indoor testing
    
    [Header("NPC Materials (for variety)")]
    [Tooltip("Body materials to randomly assign to NPCs. Leave empty to use prefab default.")]
    public Material[] bodyMaterials;
    [Tooltip("Core materials to randomly assign to NPCs. Leave empty to use prefab default.")]
    public Material[] coreMaterials;

    private List<GameObject> spawnedNPCs = new List<GameObject>();
    private bool hasSpawned = false;
    private Vector3 initialSpawnCenter;
    private Vector3 spawnAnchorWorldPos;

    void Start()
    {
        // Try to find XR Origin for coordinate system
        if (xrOrigin == null)
        {
            xrOrigin = FindObjectOfType<XROrigin>();
        }
        
        // Try to find AR camera if not assigned
        if (arCamera == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                arCamera = mainCam.transform;
            }
        }

        Debug.Log($"ARPlacementManager: XROrigin={(xrOrigin != null ? "Found" : "NULL")}, Camera={(arCamera != null ? "Found" : "NULL")}");

        // AUTO-INITIALIZE UI COMPONENTS (ensure they exist)
        EnsureUIComponents();

        // Spawn with delay to allow AR tracking to stabilize
        StartCoroutine(DelayedSpawn());
    }
    
    void EnsureUIComponents()
    {
        // Find or create SanitySystem reference
        SanitySystem sanitySystem = FindObjectOfType<SanitySystem>();
        
        // CRITICAL: Add camera position fixer to ensure AR tracking works
        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.GetComponent<ARCameraPosFixer>() == null)
        {
            mainCam.gameObject.AddComponent<ARCameraPosFixer>();
            Debug.Log("ARPlacementManager: Added ARCameraPosFixer to camera");
        }
        
        // Add ARSanityUI if missing
        if (FindObjectOfType<ARSanityUI>() == null)
        {
            ARSanityUI sanityUI = gameObject.AddComponent<ARSanityUI>();
            sanityUI.sanitySystem = sanitySystem;
            Debug.Log("ARPlacementManager: Added ARSanityUI");
        }
        
        // Add ARAnxietyController if missing
        if (FindObjectOfType<ARAnxietyController>() == null)
        {
            ARAnxietyController anxietyController = gameObject.AddComponent<ARAnxietyController>();
            anxietyController.sanitySystem = sanitySystem;
            anxietyController.arCamera = Camera.main;
            Debug.Log("ARPlacementManager: Added ARAnxietyController");
        }
        
        // NOTE: Removed ARSimpleTest debug component
    }

    IEnumerator DelayedSpawn()
    {
        // Wait for AR tracking to stabilize
        yield return new WaitForSeconds(spawnDelay);
        
        // Record the spawn center position (this becomes the fixed anchor point)
        if (arCamera != null)
        {
            // Use the camera's forward direction on the horizontal plane
            Vector3 forward = arCamera.forward;
            forward.y = 0;
            forward.Normalize();
            
            // Spawn center is in front of where the player is looking
            initialSpawnCenter = arCamera.position + forward * spawnDistance;
            initialSpawnCenter.y = spawnHeight;
        }
        else
        {
            initialSpawnCenter = new Vector3(0, spawnHeight, spawnDistance);
        }
        
        SpawnNPCsInWorld();
    }

    /// <summary>
    /// Spawn NPCs at fixed world positions in front of initial camera position
    /// </summary>
    public void SpawnNPCsInWorld()
    {
        if (npcPrefab == null)
        {
            Debug.LogWarning("ARPlacementManager: NPC Prefab is not assigned!");
            return;
        }

        if (hasSpawned)
        {
            Debug.Log("ARPlacementManager: NPCs already spawned. Call RespawnNPCs() to respawn.");
            return;
        }

        Vector3 cameraPos = arCamera != null ? arCamera.position : Vector3.zero;
        Vector3 cameraForward = arCamera != null ? arCamera.forward : Vector3.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        // Calculate spread for multiple NPCs
        float angleStep = spawnCount > 1 ? spreadAngle / (spawnCount - 1) : 0;
        float startAngle = -spreadAngle / 2f;

        for (int i = 0; i < spawnCount; i++)
        {
            // Calculate spawn direction (spread around forward direction)
            float angle = startAngle + (angleStep * i);
            Vector3 spawnDirection = Quaternion.Euler(0, angle, 0) * cameraForward;
            
            // Calculate spawn position in world space - at camera height so visible!
            Vector3 spawnPos = cameraPos + spawnDirection * spawnDistance;
            // Use camera height minus a bit (so NPC is at chest/waist level)
            spawnPos.y = cameraPos.y - 0.5f;
            
            // Store the world anchor position (for debugging)
            if (i == 0)
            {
                spawnAnchorWorldPos = spawnPos;
            }
            
            // Instantiate NPC at fixed world position
            GameObject npc = Instantiate(npcPrefab, spawnPos, Quaternion.identity);
            
            // CRITICAL FIX: Do NOT parent NPC to anything!
            // Keep NPC at scene root level so it stays fixed in world space
            // Parenting to XROrigin or its parent can cause NPC to follow camera
            npc.transform.SetParent(null, true);  // null parent = scene root
            
            // Double-check: force world position after unparenting
            npc.transform.position = spawnPos;
            
            // Apply scale
            npc.transform.localScale = Vector3.one * npcScale;
            
            // Make NPC face toward where player started
            Vector3 lookTarget = new Vector3(cameraPos.x, spawnPos.y, cameraPos.z);
            npc.transform.LookAt(lookTarget);

            // Apply random materials for variety
            ApplyRandomMaterials(npc, i);

            // Add ARNPCBehavior for face-player and random color
            ARNPCBehavior npcBehavior = npc.GetComponent<ARNPCBehavior>();
            if (npcBehavior == null)
            {
                npcBehavior = npc.AddComponent<ARNPCBehavior>();
            }
            npcBehavior.SetPlayerCamera(arCamera);

            // Ensure NPC has ComfortZone component for ARSanityManager
            if (npc.GetComponent<ARComfortZone>() == null)
            {
                npc.AddComponent<ARComfortZone>();
            }
            
            // Add enhanced glow effect
            if (enableGlowEffect)
            {
                ARNPCGlowEffect glowEffect = npc.GetComponent<ARNPCGlowEffect>();
                if (glowEffect == null)
                {
                    glowEffect = npc.AddComponent<ARNPCGlowEffect>();
                    glowEffect.SetPlayerCamera(arCamera);
                    
                    // Random glow color
                    Color glowColor = Color.HSVToRGB(Random.value, 0.7f, 1f);
                    glowEffect.SetGlowColor(glowColor);
                }
                
                // Add safety zone visual
                if (enableSafetyZone)
                {
                    glowEffect.AddSafetyZone(safetyZoneRadius);
                }
            }

            spawnedNPCs.Add(npc);
            
            Debug.Log($"ARPlacementManager: Spawned NPC {i} at world position {spawnPos}, Parent={(npc.transform.parent != null ? npc.transform.parent.name : "null")}");
        }

        hasSpawned = true;
        Debug.Log($"ARPlacementManager: Spawned {spawnCount} NPCs. Walk toward them to reduce anxiety!");
    }

    /// <summary>
    /// Clear existing NPCs and respawn based on current camera position
    /// </summary>
    public void RespawnNPCs()
    {
        ClearNPCs();
        hasSpawned = false;
        
        // Update spawn center to current camera position
        if (arCamera != null)
        {
            Vector3 forward = arCamera.forward;
            forward.y = 0;
            forward.Normalize();
            initialSpawnCenter = arCamera.position + forward * spawnDistance;
            initialSpawnCenter.y = spawnHeight;
        }
        
        SpawnNPCsInWorld();
    }

    /// <summary>
    /// Remove all spawned NPCs
    /// </summary>
    public void ClearNPCs()
    {
        foreach (var npc in spawnedNPCs)
        {
            if (npc != null)
            {
                Destroy(npc);
            }
        }
        spawnedNPCs.Clear();
    }

    /// <summary>
    /// Get list of spawned NPCs
    /// </summary>
    public List<GameObject> GetSpawnedNPCs()
    {
        return spawnedNPCs;
    }
    
    /// <summary>
    /// Apply random materials to NPC for visual variety
    /// </summary>
    void ApplyRandomMaterials(GameObject npc, int npcIndex)
    {
        // Get all renderers in the NPC
        Renderer[] renderers = npc.GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            // Check if this is a body renderer (by name or material name)
            string objName = renderer.gameObject.name.ToLower();
            
            if (bodyMaterials != null && bodyMaterials.Length > 0)
            {
                if (objName.Contains("body") || (renderer.sharedMaterial != null && renderer.sharedMaterial.name.ToLower().Contains("body")))
                {
                    // Assign random body material
                    int matIndex = npcIndex % bodyMaterials.Length;
                    if (bodyMaterials[matIndex] != null)
                    {
                        renderer.material = bodyMaterials[matIndex];
                        Debug.Log($"ARPlacementManager: Applied body material {matIndex} to NPC {npcIndex}");
                    }
                }
            }
            
            if (coreMaterials != null && coreMaterials.Length > 0)
            {
                if (objName.Contains("core") || (renderer.sharedMaterial != null && renderer.sharedMaterial.name.ToLower().Contains("core")))
                {
                    // Assign random core material
                    int matIndex = npcIndex % coreMaterials.Length;
                    if (coreMaterials[matIndex] != null)
                    {
                        renderer.material = coreMaterials[matIndex];
                        Debug.Log($"ARPlacementManager: Applied core material {matIndex} to NPC {npcIndex}");
                    }
                }
            }
        }
    }
    
    void OnGUI()
    {
        // Button sizes
        float buttonWidth = 120f;
        float buttonHeight = 50f;
        float margin = 15f;
        
        // Style for button
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 18;
        buttonStyle.fontStyle = FontStyle.Bold;
        
        // Respawn button at bottom-right
        Rect respawnRect = new Rect(
            Screen.width - buttonWidth - margin,
            Screen.height - buttonHeight - margin - 120f,
            buttonWidth,
            buttonHeight
        );
        
        if (GUI.Button(respawnRect, "Respawn", buttonStyle))
        {
            RespawnNPCs();
            Debug.Log("ARPlacementManager: NPC Respawned by user");
        }
        
        // Add NPC button next to respawn button
        Rect addRect = new Rect(
            Screen.width - buttonWidth * 2 - margin * 2,
            Screen.height - buttonHeight - margin - 120f,
            buttonWidth,
            buttonHeight
        );
        
        if (GUI.Button(addRect, "+ Add NPC", buttonStyle))
        {
            SpawnRandomNPC();
            Debug.Log("ARPlacementManager: Added random NPC");
        }
        
        // Show NPC count
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 16;
        labelStyle.normal.textColor = Color.white;
        
        GUI.Label(new Rect(
            Screen.width - buttonWidth - margin,
            Screen.height - buttonHeight - margin - 145f,
            buttonWidth,
            25f
        ), $"NPCs: {spawnedNPCs.Count}", labelStyle);
    }
    
    /// <summary>
    /// Spawn a single NPC at random position around player
    /// </summary>
    public void SpawnRandomNPC()
    {
        if (npcPrefab == null || arCamera == null) return;
        
        // Random angle around player
        float randomAngle = Random.Range(0f, 360f);
        float randomDistance = Random.Range(1f, 2.5f);
        
        Vector3 direction = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
        Vector3 spawnPos = arCamera.position + direction * randomDistance;
        spawnPos.y = arCamera.position.y - 0.5f;
        
        // Spawn
        GameObject npc = Instantiate(npcPrefab, spawnPos, Quaternion.identity);
        npc.transform.SetParent(null);
        npc.transform.position = spawnPos;
        npc.transform.localScale = Vector3.one * npcScale;
        
        // Face player
        Vector3 lookTarget = new Vector3(arCamera.position.x, spawnPos.y, arCamera.position.z);
        npc.transform.LookAt(lookTarget);
        
        // Apply beautiful color scheme to NPC materials
        ApplyBeautifulColors(npc);
        
        // Add components
        ARNPCBehavior npcBehavior = npc.GetComponent<ARNPCBehavior>();
        if (npcBehavior == null)
            npcBehavior = npc.AddComponent<ARNPCBehavior>();
        npcBehavior.SetPlayerCamera(arCamera);
        npcBehavior.useRandomColor = false; // We already applied colors
        
        if (npc.GetComponent<ARComfortZone>() == null)
            npc.AddComponent<ARComfortZone>();
        
        // Add glow effect
        if (enableGlowEffect)
        {
            ARNPCGlowEffect glowEffect = npc.GetComponent<ARNPCGlowEffect>();
            if (glowEffect == null)
                glowEffect = npc.AddComponent<ARNPCGlowEffect>();
            
            glowEffect.SetPlayerCamera(arCamera);
            
            if (enableSafetyZone)
                glowEffect.AddSafetyZone(safetyZoneRadius);
        }
        
        spawnedNPCs.Add(npc);
    }
    
    /// <summary>
    /// Apply beautiful, harmonious colors to NPC materials
    /// </summary>
    void ApplyBeautifulColors(GameObject npc)
    {
        // Predefined beautiful color palettes (curated for visual appeal)
        Color[][] palettes = new Color[][]
        {
            // Palette 1: Ocean Dream (cyan/blue theme)
            new Color[] {
                new Color(0.15f, 0.74f, 0.90f, 1f),   // _Color - soft cyan
                new Color(0.0f, 0.8f, 1f, 1f),        // _EmissionColor - bright cyan
                new Color(0.28f, 0.72f, 0.85f, 1f),   // _EnergyFlowColor
                new Color(0.0f, 0.11f, 1f, 1f)        // _FresnelColor - blue edge
            },
            // Palette 2: Sunset Glow (warm orange/pink)
            new Color[] {
                new Color(1f, 0.5f, 0.3f, 1f),        // _Color - warm orange
                new Color(1f, 0.3f, 0.5f, 1f),        // _EmissionColor - pink
                new Color(0.9f, 0.4f, 0.3f, 1f),      // _EnergyFlowColor
                new Color(1f, 0.2f, 0.4f, 1f)         // _FresnelColor - magenta edge
            },
            // Palette 3: Aurora (green/purple)
            new Color[] {
                new Color(0.3f, 0.9f, 0.5f, 1f),      // _Color - mint green
                new Color(0.5f, 0.2f, 0.9f, 1f),      // _EmissionColor - purple
                new Color(0.4f, 0.8f, 0.6f, 1f),      // _EnergyFlowColor
                new Color(0.7f, 0.3f, 1f, 1f)         // _FresnelColor - violet edge
            },
            // Palette 4: Golden Hour (yellow/gold)
            new Color[] {
                new Color(1f, 0.85f, 0.4f, 1f),       // _Color - golden
                new Color(1f, 0.7f, 0.2f, 1f),        // _EmissionColor - amber
                new Color(1f, 0.8f, 0.3f, 1f),        // _EnergyFlowColor
                new Color(1f, 0.6f, 0.1f, 1f)         // _FresnelColor - orange edge
            },
            // Palette 5: Mystic Rose (pink/purple)
            new Color[] {
                new Color(0.9f, 0.4f, 0.7f, 1f),      // _Color - rose pink
                new Color(0.8f, 0.2f, 0.6f, 1f),      // _EmissionColor - deep pink
                new Color(0.85f, 0.3f, 0.65f, 1f),    // _EnergyFlowColor
                new Color(0.6f, 0.1f, 0.8f, 1f)       // _FresnelColor - purple edge
            }
        };
        
        // Pick a random palette
        Color[] palette = palettes[Random.Range(0, palettes.Length)];
        
        // Apply to all renderers
        Renderer[] renderers = npc.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material mat = renderer.material;
            if (mat != null)
            {
                if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", palette[0]);
                if (mat.HasProperty("_EmissionColor"))
                    mat.SetColor("_EmissionColor", palette[1]);
                if (mat.HasProperty("_EnergyFlowColor"))
                    mat.SetColor("_EnergyFlowColor", palette[2]);
                if (mat.HasProperty("_FresnelColor"))
                    mat.SetColor("_FresnelColor", palette[3]);
            }
        }
        
        Debug.Log($"ARPlacementManager: Applied color palette to NPC");
    }
}


