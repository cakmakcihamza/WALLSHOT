# WALLSHOT

WALLSHOT is an Android wall-demolition game built with Unity. Aim carefully, launch projectiles, and bring down brick walls with physics-driven impacts while managing a limited number of shots.

## Features

- Physics-based brick destruction and projectile impacts.
- Touch drag-to-aim controls, with mouse input supported for the slingshot in the Unity Editor.
- Five weapon stages: Slingshot, Desert Eagle, Cannon, Crossbow, and Bazooka.
- Ten levels per weapon stage, with progression and weapon unlocking.
- Moving and rotating obstacles that challenge shot timing and aim.
- Shot limits, wall progress feedback, level completion, and retry flows.
- Local progress persistence using Unity PlayerPrefs and a level selection interface.
- Weapon and impact effects, camera shake, sound effects, and sound settings.

These features are based on the current source implementation. Device performance and release readiness should be verified on Android hardware.

## Requirements

- Unity **6000.3.23f1** (Unity 6.3 LTS), as recorded in `ProjectSettings/ProjectVersion.txt`.
- Android Build Support, including the Android SDK, NDK, and OpenJDK supplied through Unity Hub.
- Universal Render Pipeline and Input System dependencies are declared in `Packages/manifest.json`.

## Getting started

1. Clone this repository.
2. In Unity Hub, select **Add project from disk** and choose the repository root.
3. Open it with the recorded Unity version and allow package resolution and asset import to finish.
4. Open `Assets/Scenes/MainMenu.unity` and enter Play mode.

The configured build scene order is `MainMenu`, followed by `SampleScene` (gameplay).

## Android build

1. Open **File > Build Profiles** and select Android.
2. Verify the scene list and review Player Settings for your device and distribution requirements.
3. Configure your own local signing credentials before producing a signed release. The checked-in settings reference a developer-local keystore that is not included in this repository.
4. Build an APK for device testing or an Android App Bundle for distribution.

Keep signing keys, passwords, and service credentials outside version control. Generated builds are ignored.

## Project structure

| Path | Contents |
| --- | --- |
| `Assets/Scripts/` | Gameplay, weapons, wall physics, UI, audio, and effects |
| `Assets/Scenes/` | Main menu and gameplay scenes |
| `Assets/Settings/` | Rendering and related assets |
| `Assets/Audio/` | Audio assets |
| `Packages/` | Unity package manifest and dependency lock file |
| `ProjectSettings/` | Shared Unity project configuration |

Unity `.meta` files are versioned alongside assets to preserve references. Generated caches, IDE files, local preferences, signing keys, and build outputs are excluded by `.gitignore`.

## Asset rights

Third-party assets remain subject to their respective licenses. No additional license grant is provided by this README; review asset permissions before redistribution or reuse.
