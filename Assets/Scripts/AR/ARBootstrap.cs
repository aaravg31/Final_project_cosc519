using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Bootstrap script that ensures all AR components exist at runtime.
/// Runs before other scripts via Script Execution Order or simply finding/adding.
/// </summary>
public class ARBootstrap : MonoBehaviour
{
    [Header("Feature Toggles")]
    [Tooltip("Enable NPC spawning")]
    public bool enableNPCSpawning = true;
    [Tooltip("Enable anxiety effects")]
    public bool enableAnxietyEffects = true;
    [Tooltip("Enable sanity UI")]
    public bool enableSanityUI = true;
    [Tooltip("Enable simple test cube (for debugging)")]
    public bool enableSimpleTest = true;

    void Awake()
    {
        Debug.Log("ARBootstrap: Initializing...");
        
        // Ensure we have the debug info
        if (FindObjectOfType<ARDebugInfo>() == null)
        {
            gameObject.AddComponent<ARDebugInfo>();
            Debug.Log("ARBootstrap: Added ARDebugInfo");
        }
        
        // Add simple test for debugging
        if (enableSimpleTest)
        {
            if (FindObjectOfType<ARSimpleTest>() == null)
            {
                gameObject.AddComponent<ARSimpleTest>();
                Debug.Log("ARBootstrap: Added ARSimpleTest for debugging");
            }
        }
        
        // Ensure sanity system (required by others)
        SanitySystem ss = FindObjectOfType<SanitySystem>();
        if (ss == null)
        {
            ss = gameObject.AddComponent<SanitySystem>();
            ss.decreaseOverTime = true;
            ss.decayRate = 2f;
            Debug.Log("ARBootstrap: Added SanitySystem");
        }
        
        // Optional: Placement manager
        if (enableNPCSpawning)
        {
            ARPlacementManager pm = FindObjectOfType<ARPlacementManager>();
            if (pm == null)
            {
                pm = gameObject.AddComponent<ARPlacementManager>();
                Debug.Log("ARBootstrap: Added ARPlacementManager");
            }
            
            // Try to assign NPC prefab if missing
            if (pm != null && pm.npcPrefab == null)
            {
                pm.npcPrefab = Resources.Load<GameObject>("NPC");
                if (pm.npcPrefab != null)
                {
                    Debug.Log("ARBootstrap: Loaded NPC from Resources");
                }
            }
            
            // Ensure AR Sanity Manager
            ARSanityManager asm = FindObjectOfType<ARSanityManager>();
            if (asm == null)
            {
                asm = gameObject.AddComponent<ARSanityManager>();
                asm.sanitySystem = ss;
                asm.playerCamera = Camera.main?.transform;
                Debug.Log("ARBootstrap: Added ARSanityManager");
            }
        }
        
        // Optional: Anxiety effects
        if (enableAnxietyEffects)
        {
            ARAnxietyController aac = FindObjectOfType<ARAnxietyController>();
            if (aac == null)
            {
                aac = gameObject.AddComponent<ARAnxietyController>();
                aac.sanitySystem = ss;
                aac.arCamera = Camera.main;
                Debug.Log("ARBootstrap: Added ARAnxietyController");
            }
            
            // Dark particles
            ARDarkParticles darkParticles = FindObjectOfType<ARDarkParticles>();
            if (darkParticles == null)
            {
                darkParticles = gameObject.AddComponent<ARDarkParticles>();
                darkParticles.sanitySystem = ss;
                darkParticles.arCamera = Camera.main?.transform;
                Debug.Log("ARBootstrap: Added ARDarkParticles");
            }
        }
        
        // Optional: Sanity UI
        if (enableSanityUI)
        {
            ARSanityUI sui = FindObjectOfType<ARSanityUI>();
            if (sui == null)
            {
                sui = gameObject.AddComponent<ARSanityUI>();
                sui.sanitySystem = ss;
                Debug.Log("ARBootstrap: Added ARSanityUI");
            }
        }
        
        Debug.Log("ARBootstrap: Initialization complete!");
    }
}
