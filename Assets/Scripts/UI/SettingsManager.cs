using UnityEngine;

namespace WallBreaker.UI
{
    public static class SettingsManager
    {
        public const string SoundKey = "Wallshot.Settings.Sound";
        public const string VibrationKey = "Wallshot.Settings.Vibration";

        public static bool SoundEnabled => PlayerPrefs.GetInt(SoundKey, 1) == 1;
        public static bool VibrationEnabled => PlayerPrefs.GetInt(VibrationKey, 1) == 1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplySavedSettingsBeforeSceneLoad()
        {
            ApplyAudio();
        }

        public static void SetSoundEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(SoundKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
            ApplyAudio();
        }

        public static void SetVibrationEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(VibrationKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void ToggleSound()
        {
            SetSoundEnabled(!SoundEnabled);
        }

        public static void ToggleVibration()
        {
            SetVibrationEnabled(!VibrationEnabled);
        }

        private static void ApplyAudio()
        {
            AudioListener.volume = SoundEnabled ? 1f : 0f;
        }
    }
}
