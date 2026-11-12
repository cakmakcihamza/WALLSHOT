#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using WallBreaker.Game;

namespace WallBreaker.DebugTools
{
    [DefaultExecutionOrder(1000)]
    public class EditorDebugShortcuts : MonoBehaviour
    {
        private const string ObjectName = "EditorDebugShortcuts";
        private static EditorDebugShortcuts instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureExists()
        {
            if (instance != null || FindFirstObjectByType<EditorDebugShortcuts>() != null)
            {
                return;
            }

            GameObject shortcutObject = new GameObject(ObjectName);
            shortcutObject.hideFlags = HideFlags.DontSave;
            DontDestroyOnLoad(shortcutObject);
            shortcutObject.AddComponent<EditorDebugShortcuts>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (WasPressed(keyboard.f1Key))
            {
                WeaponManager.DebugSetStage(WeaponStage.Slingshot);
                LevelManager.DebugReloadCurrentLevel();
                return;
            }

            if (WasPressed(keyboard.f2Key))
            {
                WeaponManager.DebugSetStage(WeaponStage.DesertEagle);
                LevelManager.DebugReloadCurrentLevel();
                return;
            }

            if (WasPressed(keyboard.f3Key))
            {
                WeaponManager.DebugSetStage(WeaponStage.Cannon);
                LevelManager.DebugReloadCurrentLevel();
                return;
            }

            if (WasPressed(keyboard.f4Key))
            {
                WeaponManager.DebugSetStage(WeaponStage.Crossbow);
                LevelManager.DebugReloadCurrentLevel();
                return;
            }

            if (WasPressed(keyboard.f5Key))
            {
                WeaponManager.DebugSetStage(WeaponStage.Bazooka);
                LevelManager.DebugReloadCurrentLevel();
                return;
            }

            if (WasPressed(keyboard.f11Key))
            {
                SaveManager.SaveProgress(WeaponManager.CurrentStage, LevelManager.CurrentLevel);
                return;
            }

            if (WasPressed(keyboard.f12Key))
            {
                SaveManager.ResetProgress();
                LevelManager.DebugReloadCurrentLevel();
                return;
            }

            if (WasPressed(keyboard.rKey))
            {
                LevelManager.DebugReloadCurrentLevel();
                return;
            }

            int requestedLevel = GetRequestedLevel(keyboard);
            if (requestedLevel > 0)
            {
                LevelManager.DebugJumpToLevel(requestedLevel);
            }
        }

        private int GetRequestedLevel(Keyboard keyboard)
        {
            if (WasPressed(keyboard.digit1Key) || WasPressed(keyboard.numpad1Key))
            {
                return 1;
            }

            if (WasPressed(keyboard.digit2Key) || WasPressed(keyboard.numpad2Key))
            {
                return 2;
            }

            if (WasPressed(keyboard.digit3Key) || WasPressed(keyboard.numpad3Key))
            {
                return 3;
            }

            if (WasPressed(keyboard.digit4Key) || WasPressed(keyboard.numpad4Key))
            {
                return 4;
            }

            if (WasPressed(keyboard.digit5Key) || WasPressed(keyboard.numpad5Key))
            {
                return 5;
            }

            if (WasPressed(keyboard.digit6Key) || WasPressed(keyboard.numpad6Key))
            {
                return 6;
            }

            if (WasPressed(keyboard.digit7Key) || WasPressed(keyboard.numpad7Key))
            {
                return 7;
            }

            if (WasPressed(keyboard.digit8Key) || WasPressed(keyboard.numpad8Key))
            {
                return 8;
            }

            if (WasPressed(keyboard.digit9Key) || WasPressed(keyboard.numpad9Key))
            {
                return 9;
            }

            if (WasPressed(keyboard.digit0Key) || WasPressed(keyboard.numpad0Key))
            {
                return 10;
            }

            return 0;
        }

        private bool WasPressed(KeyControl key)
        {
            return key != null && key.wasPressedThisFrame;
        }
    }
}
#endif
