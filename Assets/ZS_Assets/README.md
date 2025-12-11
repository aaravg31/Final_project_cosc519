# ZS_Assets Documentation

This directory contains the core custom assets for the Final Project (COSC 519), focusing on the **Sanity System** and **NPC Interaction**.

## Directory Structure

*   **Scripts/**: Core logic scripts.
    *   `SanitySystem.cs`: Manages player's current sanity/stress level.
    *   `AnxietyEffectController.cs`: Handles visual and audio feedback (Vignette, Shake, Heartbeat) based on stress.
*   **Prefabs/**:
    *   `NPC.prefab` / `NPC _1.prefab`: The "comfort" characters. Players must approach them to restore sanity.
*   **Materials/ & Textures/**: Custom visual assets for the NPCs and environment.
*   **Shaders/**: Custom shaders for visual effects.
*   **ZS_TestScene.unity**: A sandbox scene for testing the Sanity mechanics.

## Key Systems

### 1. Sanity System
The `SanitySystem` tracks a float value (0-100).
*   **Decay**: Can be set to decrease over time (default behavior in VR).
*   **Recovery**: Restored by staying inside `ARComfortZone` triggers (around NPCs).

### 2. Anxiety Effects
Controlled by `AnxietyEffectController`. As Sanity drops (Stress increases):
*   **Visual**: Screen darkens (Vignette), chromatic aberration increases, camera shake intensifies.
*   **Audio**: Heartbeat/Tension sound volume increases.

## Mobile AR Integration
These assets are reused in the **Mobile AR Level**:
*   NPCs act as anchors in the real world.
*   The `ARSanityManager` (in `Assets/Scripts/AR`) bridges these assets with AR distance checks.
