using UnityEngine;
using System.Collections.Generic;

public class ARSanityManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the existing SanitySystem.")]
    public SanitySystem sanitySystem;
    [Tooltip("The AR Camera (Player).")]
    public Transform playerCamera;

    [Header("Sanity Settings")]
    [Tooltip("Sanity restored per second when near an NPC.")]
    public float healRate = 10.0f;
    [Tooltip("Sanity lost per second when alone.")]
    public float decayRate = 3.0f;
    [Tooltip("Starting sanity (0-100). Lower = more stressed at start.")]
    public float startingSanity = 50f;
    
    [Header("Comfort Settings")]
    [Tooltip("Distance in meters to trigger healing.")]
    public float comfortDistance = 0.8f;  // Changed from 2.0 for indoor testing

    [Header("Debug")]
    [Tooltip("Show debug info in console")]
    public bool debugMode = true;
    [Tooltip("Current distance to nearest NPC")]
    public float currentMinDistance;
    [Tooltip("Number of registered NPCs")]
    public int registeredNPCCount;

    private List<ARComfortZone> allComfortZones = new List<ARComfortZone>();
    private float debugTimer = 0f;

    void Start()
    {
        // Find SanitySystem
        if (sanitySystem == null)
            sanitySystem = GetComponent<SanitySystem>();

        if (sanitySystem == null)
        {
            Debug.LogError("ARSanityManager: SanitySystem not found! Stress effects won't work.");
            return;
        }

        // Set starting sanity (not full, so player starts with some stress)
        sanitySystem.SetSanity(startingSanity);
        
        // Disable default decay in SanitySystem - we control it here
        sanitySystem.decreaseOverTime = false;

        // Find camera
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        if (debugMode)
        {
            Debug.Log($"ARSanityManager: Initialized. Starting sanity: {startingSanity}, Decay rate: {decayRate}/s, Heal rate: {healRate}/s");
        }
    }

    void Update()
    {
        if (sanitySystem == null || playerCamera == null) return;

        // Clean up null references
        allComfortZones.RemoveAll(zone => zone == null);
        registeredNPCCount = allComfortZones.Count;

        bool isSafe = false;
        float minDistance = float.MaxValue;

        foreach (var zone in allComfortZones)
        {
            if (zone == null) continue;

            float dist = Vector3.Distance(playerCamera.position, zone.transform.position);
            if (dist < minDistance) minDistance = dist;
            
            if (dist <= comfortDistance)
            {
                isSafe = true;
            }
        }

        currentMinDistance = minDistance;

        if (isSafe)
        {
            // Near NPC - Heal / reduce stress
            sanitySystem.ModifySanity(healRate * Time.deltaTime);
        }
        else
        {
            // Alone - Decay / increase stress
            sanitySystem.ModifySanity(-decayRate * Time.deltaTime);
        }

        // Debug output every 2 seconds
        if (debugMode)
        {
            debugTimer += Time.deltaTime;
            if (debugTimer >= 2f)
            {
                debugTimer = 0f;
                float stress = 1f - (sanitySystem.currentSanity / sanitySystem.maxSanity);
                Debug.Log($"ARSanityManager: NPCs={registeredNPCCount}, Nearest={minDistance:F1}m, Safe={isSafe}, Sanity={sanitySystem.currentSanity:F0}, Stress={stress:P0}");
            }
        }
    }

    public void RegisterZone(ARComfortZone zone)
    {
        if (zone != null && !allComfortZones.Contains(zone))
        {
            allComfortZones.Add(zone);
            if (debugMode)
                Debug.Log($"ARSanityManager: Registered NPC. Total: {allComfortZones.Count}");
        }
    }

    public void UnregisterZone(ARComfortZone zone)
    {
        if (allComfortZones.Contains(zone))
        {
            allComfortZones.Remove(zone);
            if (debugMode)
                Debug.Log($"ARSanityManager: Unregistered NPC. Total: {allComfortZones.Count}");
        }
    }
}
