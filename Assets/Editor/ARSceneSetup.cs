using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ARSceneSetup : EditorWindow
{
    [MenuItem("Tools/Setup Mobile AR Level")]
    public static void CreateARScene()
    {
        // 1. Create new scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // 2. Create AR Session
        GameObject arSessionGO = new GameObject("AR Session");
        arSessionGO.AddComponent<ARSession>();
        arSessionGO.AddComponent<ARInputManager>();

        // 3. Create XR Origin (Mobile)
        GameObject xrOriginGO = new GameObject("XR Origin (Mobile)");
        var origin = xrOriginGO.AddComponent<Unity.XR.CoreUtils.XROrigin>();
        
        var camOffset = new GameObject("Camera Offset");
        camOffset.transform.SetParent(xrOriginGO.transform);
        
        var mainCam = new GameObject("Main Camera");
        mainCam.transform.SetParent(camOffset.transform);
        mainCam.tag = "MainCamera";
        var camComp = mainCam.AddComponent<Camera>();
        camComp.clearFlags = CameraClearFlags.SolidColor;
        camComp.backgroundColor = Color.black;
        
        // ARCameraManager drives the camera pose on mobile AR (no TrackedPoseDriver needed!)
        // The ARCameraManager + XROrigin combination handles pose updates automatically.
        mainCam.AddComponent<ARCameraManager>();
        mainCam.AddComponent<ARCameraBackground>();
        
        // Add custom pose updater for camera movement
        mainCam.AddComponent<ARPoseUpdater>();
        
        // Add Universal Additional Camera Data if URP
        mainCam.AddComponent<UniversalAdditionalCameraData>();

        if (origin != null)
        {
            origin.CameraFloorOffsetObject = camOffset;
            origin.Camera = camComp;
        }

        // 4. Create Volume with Proper Profile
        GameObject volumeGO = new GameObject("Global Volume");
        var volume = volumeGO.AddComponent<Volume>();
        volume.isGlobal = true;
        
        // Create a new Volume Profile with effects
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        
        // Add Vignette
        var vignette = profile.Add<Vignette>(true);
        vignette.intensity.overrideState = true;
        vignette.intensity.value = 0f; // Start at 0, AnxietyEffectController will control it
        vignette.color.overrideState = true;
        vignette.color.value = Color.black;
        
        // Add Chromatic Aberration
        var aberration = profile.Add<ChromaticAberration>(true);
        aberration.intensity.overrideState = true;
        aberration.intensity.value = 0f;
        
        // Add Film Grain
        var grain = profile.Add<FilmGrain>(true);
        grain.intensity.overrideState = true;
        grain.intensity.value = 0f;
        grain.type.overrideState = true;
        grain.type.value = FilmGrainLookup.Medium3;
        
        // Save profile as asset
        string profilePath = "Assets/Settings/ARAnxietyProfile.asset";
        AssetDatabase.CreateAsset(profile, profilePath);
        
        volume.profile = profile;

        // 5. Create AR Managers
        GameObject gameManager = new GameObject("AR Game Manager");
        
        // Bootstrap ensures all components exist even if editor setup has issues
        gameManager.AddComponent<ARBootstrap>();
        
        // Add minimal test first to confirm app is running
        gameManager.AddComponent<MinimalStartupTest>();
        
        // Sanity
        var sanitySys = gameManager.AddComponent<SanitySystem>();
        sanitySys.decreaseOverTime = true; // Enable decay in AR
        sanitySys.decayRate = 2f;
        
        var anxiety = gameManager.AddComponent<AnxietyEffectController>();
        anxiety.globalVolume = volume;
        anxiety.tensionAudioSource = gameManager.AddComponent<AudioSource>();
        
        // Wire up the event
        sanitySys.OnStressLevelChanged.AddListener(anxiety.SetStressLevel);
        
        var arSanity = gameManager.AddComponent<ARSanityManager>();
        arSanity.sanitySystem = sanitySys;
        arSanity.playerCamera = mainCam.transform;
        
        // Debug
        gameManager.AddComponent<ARDebugInfo>();

        // Placement
        var placement = gameManager.AddComponent<ARPlacementManager>();
        placement.arCamera = mainCam.transform;
        
        // Load NPC Prefab - try both paths
        string[] npcPaths = {
            "Assets/ZS_Assets/Prefabs/NPC.prefab",
            "Assets/ZS_Assets/Prefabs/NPC _1.prefab"
        };
        GameObject npcPrefab = null;
        foreach (var path in npcPaths)
        {
            npcPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (npcPrefab != null)
            {
                Debug.Log($"Loaded NPC prefab from: {path}");
                break;
            }
        }
        
        if (npcPrefab != null)
        {
            placement.npcPrefab = npcPrefab;
        }
        else
        {
            Debug.LogError("Could not find NPC prefab at any expected path!");
        }

        // 6. Add Directional Light for NPC visibility
        GameObject lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

        // Save Scene
        string scenePath = "Assets/Scenes/MobileAR_Level.unity";
        EditorSceneManager.SaveScene(newScene, scenePath);
        
        // Add to Build Settings (avoid duplicates)
        var original = EditorBuildSettings.scenes;
        bool alreadyExists = false;
        foreach (var s in original)
        {
            if (s.path == scenePath) { alreadyExists = true; break; }
        }
        if (!alreadyExists)
        {
            var newSettings = new EditorBuildSettingsScene[original.Length + 1];
            System.Array.Copy(original, newSettings, original.Length);
            var sceneToAdd = new EditorBuildSettingsScene(scenePath, true);
            newSettings[newSettings.Length - 1] = sceneToAdd;
            EditorBuildSettings.scenes = newSettings;
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"Mobile AR Scene created at {scenePath}. Volume Profile saved to {profilePath}.");
    }
}

