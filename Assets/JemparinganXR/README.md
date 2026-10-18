# Jemparingan XR — Phase 1: Core Archery Prototype

**Jemparingan XR** is a seated, hands-first traditional Javanese archery experience for Meta Quest, targeting the Meta Start Developer Competition 2026.

This repository contains **Phase 1: Playable Core Archery Prototype** built in **Unity 6000.2.14f1 (URP)** using temporary mouse & keyboard controls to validate archery mechanics, pendulum target physics, and the 5-rambahan match loop prior to OpenXR and Meta XR Hand Tracking integration.

---

## Folder Structure

```
Assets/JemparinganXR/
├── Scenes/
│   └── Prototype.unity          # Generated playable prototype scene
├── Scripts/
│   ├── Archery/
│   │   ├── IArcheryInput.cs             # Decoupled input abstraction
│   │   ├── MouseKeyboardArcheryInput.cs # Mouse & keyboard input provider
│   │   ├── ArcherCameraController.cs    # Seated first-person aiming controller
│   │   ├── BowController.cs             # Gandhewa bow drawing & string deformation
│   │   └── Arrow.cs                     # Rigidbody arrow flight, wind & penetration
│   ├── Targets/
│   │   ├── BandulanTarget.cs            # Hanging cylindrical pendulum target
│   │   └── TargetHitZone.cs             # Configurable scoring zones (Molo, Awak, etc.)
│   ├── GameFlow/
│   │   ├── RoundConfig.cs               # Per-round configuration (distance, wind, objectives)
│   │   ├── GameFlowManager.cs           # 5-rambahan game loop, scoring & progression
│   │   └── ArcheryGameplayManager.cs    # Coordinates input, bow, camera & game flow
│   ├── UI/
│   │   └── ArcheryHUD.cs                # In-game HUD, tension meter & result modals
│   └── Editor/
│       └── PrototypeSceneBuilder.cs     # Menu & batch automated scene builder
├── Materials/                           # 12 URP-compatible lit materials
├── Prefabs/
│   └── ArrowPrefab.prefab               # Reusable arrow projectile prefab
└── README.md
```

---

## Scene Generation Instructions

### Option A: From Unity Editor UI
1. Open the project in Unity Editor (6000.2.14f1).
2. In the top menu bar, click:
   **Tools > Jemparingan XR > Build Prototype Scene**
3. The generator will:
   - Ensure all necessary folders exist.
   - Generate all URP materials in `Assets/JemparinganXR/Materials/`.
   - Generate `ArrowPrefab.prefab` in `Assets/JemparinganXR/Prefabs/`.
   - Build and save the scene to `Assets/JemparinganXR/Scenes/Prototype.unity`.
4. Open `Assets/JemparinganXR/Scenes/Prototype.unity` in the Project window and press **Play**!

### Option B: Via Command Line / Batch Mode
```bash
/home/omz/Unity/Hub/Editor/6000.2.14f1/Editor/Unity \
  -quit -batchmode -projectPath . \
  -executeMethod JemparinganXR.Editor.PrototypeSceneBuilder.BuildSceneBatch \
  -logFile -
```

---

## Testing Controls

| Action | Control | Description |
|---|---|---|
| **Aim** | Move Mouse | Rotates camera and bow (clamped to realistic seated *sila* range). |
| **Draw Bow** | Hold Left Mouse Button | Starts drawing the bowstring. The HUD tension meter fills up from green to red. |
| **Shoot** | Release Left Mouse Button | Fires the arrow. Launch velocity scales with bow draw strength (16m/s – 38m/s). |
| **Cancel Draw** | Right Mouse Button | Resets string and loaded arrow back to rest position without firing. |
| **Next Round** | Space / Enter or Click Button | Advances to the next Rambahan when the round summary is visible. |
| **Restart Match** | R or Click Button | Restarts the 5-rambahan session from Rambahan 1. |
| **Toggle Cursor** | Escape / Click Window | Press Escape to release mouse cursor; click window to lock it back. |

---

## Gameplay Mechanics

### 1. Seated Archery Position (*Sila*)
In traditional Jemparingan, the archer sits cross-legged (*sila*). The camera is placed at seated eye height (1.0m above ground).

### 2. Traditional Gandhewa Bow & Arrow
- As the player draws, the bowstring visibly pulls backward toward the nock point.
- The arrow stays aligned with the nock point until released.
- Upon release, the arrow follows realistic ballistics: gravity, drag, and continuous dynamic collision detection.
- Arrows align aerodynamically with their velocity vector in flight.

### 3. Hanging Bandulan Target & Hit Zones
The target is a traditional hanging cylinder (*Bandulan*) suspended by a rope. When struck, the arrow embeds into the target and induces a physics-based pendulum sway.

Configurable scoring zones (customizable in Inspector via `TargetHitZone`):
- **Klinthing (Genta/Bell)**: 5 Points (top brass bell)
- **Molo (Head)**: 3 Points (traditional red top section)
- **Awak (Body)**: 1 Point (traditional white cylindrical body)
- **Buntut (Tail/Weight)**: 0 Points (bottom counterweight)

When an arrow strikes a zone, the zone flashes with a hit highlight color and displays the result on the HUD. Missed arrows embed into the ground and safely despawn after a 4-second delay.

### 4. Five-Rambahan Match Loop
A full match consists of **5 Rambahan** (rounds) with **4 arrows per round** (20 arrows total):
- **Rambahan 1**: 15m Distance, Stationary, Objective: 4 Pts
- **Rambahan 2**: 18m Distance, Stationary, Objective: 4 Pts
- **Rambahan 3**: 20m Distance, Stationary, Objective: 6 Pts
- **Rambahan 4**: 22m Distance, Stationary, Objective: 6 Pts
- **Rambahan 5**: 25m Distance, Stationary, Objective: 8 Pts

*Note: Achieving the score objective is tracked and displayed, but players are never blocked from advancing.*

---

## Architectural Extensibility (Preparing for Phase 2)

- **`IArcheryInput`**: Input handling is decoupled through an interface. In Phase 2, `MouseKeyboardArcheryInput` will be complemented or replaced with `MetaXRHandTrackingInput` without changing `BowController` or `ArcheryGameplayManager`.
- **`RoundConfig`**: The game loop is purely data-driven. Moving targets and wind simulations are already implemented in `BandulanTarget` and `Arrow` and can be enabled simply by adjusting parameters on the `RoundConfig` objects.
