using UnityEngine;

namespace WallBreaker.Game
{
    public static class SaveManager
    {
        private const string WeaponStageKey = "WallBreaker.Progression.WeaponStage";
        private const string LevelKey = "WallBreaker.Progression.Level";
        private const string HighestUnlockedKey = "WallBreaker.Progression.HighestUnlocked";
        public static readonly WeaponStage[] OrderedStages =
        {
            WeaponStage.Slingshot, WeaponStage.DesertEagle, WeaponStage.Cannon,
            WeaponStage.Crossbow, WeaponStage.Bazooka
        };

        public static int GetLevelIndex(WeaponStage stage, int level)
        {
            if (level < 1 || level > LevelManager.MaxLevel) return -1;
            int stageIndex = System.Array.IndexOf(OrderedStages, stage);
            return stageIndex < 0 ? -1 : stageIndex * LevelManager.MaxLevel + level - 1;
        }

        public static int HighestUnlocked
        {
            get
            {
                if (!PlayerPrefs.HasKey(HighestUnlockedKey))
                {
                    // Legacy saves represent the last known location, not proven completion history.
                    ProgressData legacy = LoadProgress();
                    int index = Mathf.Max(0, GetLevelIndex(legacy.Stage, legacy.Level));
                    PlayerPrefs.SetInt(HighestUnlockedKey, index);
                    PlayerPrefs.Save();
                }
                return Mathf.Clamp(PlayerPrefs.GetInt(HighestUnlockedKey), 0, 49);
            }
        }

        public static bool IsLevelUnlocked(WeaponStage stage, int level)
        {
            int index = GetLevelIndex(stage, level);
            return index >= 0 && index <= HighestUnlocked;
        }

        private static void RaiseUnlocked(int index)
        {
            int highest = HighestUnlocked;
            if (index <= highest) return;
            PlayerPrefs.SetInt(HighestUnlockedKey, Mathf.Clamp(index, 0, 49));
            PlayerPrefs.Save();
        }

        public static void RecordCompletion(WeaponStage stage, int level)
        {
            int index = GetLevelIndex(stage, level);
            if (index >= 0) RaiseUnlocked(Mathf.Min(49, index + 1));
        }

        public static bool TrySelectLevel(WeaponStage stage, int level)
        {
            if (!IsLevelUnlocked(stage, level)) return false;
            // Selection updates the last played location only, never the unlock frontier.
            PlayerPrefs.SetInt(WeaponStageKey, (int)stage);
            PlayerPrefs.SetInt(LevelKey, level);
            PlayerPrefs.Save();
            WeaponManager.SetStage(stage);
            LevelManager.SetCurrentLevel(level);
            return true;
        }

#if UNITY_EDITOR
        private static bool useRuntimeOverrideForNextLoad;
        private static ProgressData runtimeOverride = ProgressData.Default;
#endif

        public static bool HasSave => PlayerPrefs.HasKey(WeaponStageKey) && PlayerPrefs.HasKey(LevelKey);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadProgressBeforeSceneStart()
        {
#if UNITY_EDITOR
            if (useRuntimeOverrideForNextLoad)
            {
                useRuntimeOverrideForNextLoad = false;
                WeaponManager.SetStage(runtimeOverride.Stage);
                LevelManager.SetCurrentLevel(runtimeOverride.Level);
                return;
            }
#endif

            ProgressData progress = LoadProgress();
            WeaponManager.SetStage(progress.Stage);
            LevelManager.SetCurrentLevel(progress.Level);
        }

        public static void SaveProgress(WeaponStage stage, int level)
        {
            // Also preserves F11's ability to save a debug-selected location.
            int index = GetLevelIndex(stage, Mathf.Clamp(level, 1, LevelManager.MaxLevel));
            if (index >= 0) RaiseUnlocked(index);
            PlayerPrefs.SetInt(WeaponStageKey, (int)stage);
            PlayerPrefs.SetInt(LevelKey, Mathf.Clamp(level, 1, LevelManager.MaxLevel));
            PlayerPrefs.Save();
        }

        public static ProgressData LoadProgress()
        {
            if (!HasSave)
            {
                return ProgressData.Default;
            }

            int savedStage = PlayerPrefs.GetInt(WeaponStageKey, (int)WeaponStage.Slingshot);
            int savedLevel = PlayerPrefs.GetInt(LevelKey, 1);

            if (!IsSupportedStage(savedStage))
            {
                return ProgressData.Default;
            }

            return new ProgressData((WeaponStage)savedStage, Mathf.Clamp(savedLevel, 1, LevelManager.MaxLevel));
        }

        public static void ResetProgress()
        {
            PlayerPrefs.DeleteKey(WeaponStageKey);
            PlayerPrefs.DeleteKey(LevelKey);
            PlayerPrefs.DeleteKey(HighestUnlockedKey);
            PlayerPrefs.Save();

            WeaponManager.SetStage(WeaponStage.Slingshot);
            LevelManager.SetCurrentLevel(1);
        }

#if UNITY_EDITOR
        public static void UseRuntimeProgressForNextLoad(WeaponStage stage, int level)
        {
            runtimeOverride = new ProgressData(stage, Mathf.Clamp(level, 1, LevelManager.MaxLevel));
            useRuntimeOverrideForNextLoad = true;
        }
#endif

        private static bool IsSupportedStage(int stage)
        {
            return stage == (int)WeaponStage.Slingshot ||
                stage == (int)WeaponStage.Cannon ||
                stage == (int)WeaponStage.Crossbow ||
                stage == (int)WeaponStage.Bazooka ||
                stage == (int)WeaponStage.DesertEagle;
        }

        public readonly struct ProgressData
        {
            public static ProgressData Default => new ProgressData(WeaponStage.Slingshot, 1);

            public ProgressData(WeaponStage stage, int level)
            {
                Stage = stage;
                Level = level;
            }

            public WeaponStage Stage { get; }
            public int Level { get; }
        }
    }
}
