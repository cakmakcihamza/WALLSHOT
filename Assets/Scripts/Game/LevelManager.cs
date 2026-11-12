using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WallBreaker.Wall;

namespace WallBreaker.Game
{
    public class LevelManager : MonoBehaviour
    {
        private const string CanvasName = "LevelCompleteCanvas";
        public const int MaxLevel = 10;

        private static LevelManager instance;
        private static int currentLevel = 1;

        private readonly HashSet<int> activatedBrickIds = new HashSet<int>();
        private int totalBricks;
        private bool levelComplete;
        private GameObject completePanel;
        private WeaponStage? completedWeaponStage;
        private Button unlockContinueButton;
        private Sprite unlockRoundedSprite;

        public static LevelManager Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                LevelManager existing = FindFirstObjectByType<LevelManager>();
                if (existing != null)
                {
                    instance = existing;
                    return instance;
                }

                GameObject managerObject = new GameObject("LevelManager");
                instance = managerObject.AddComponent<LevelManager>();
                return instance;
            }
        }

        public static bool IsLevelComplete => instance != null && instance.levelComplete;
        public static int CurrentLevel => currentLevel;

        public static void SetCurrentLevel(int level)
        {
            currentLevel = Mathf.Clamp(level, 1, MaxLevel);
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

        private void OnEnable()
        {
            BrickPhysicsActivator.BrickActivated += HandleBrickActivated;
            StartCoroutine(InitializeAfterWallGeneration());
        }

        private void OnDisable()
        {
            BrickPhysicsActivator.BrickActivated -= HandleBrickActivated;
        }

        private IEnumerator InitializeAfterWallGeneration()
        {
            yield return null;
            yield return null;

            BrickPhysicsActivator[] bricks = FindObjectsByType<BrickPhysicsActivator>(FindObjectsSortMode.None);
            totalBricks = bricks.Length;
            activatedBrickIds.Clear();
            levelComplete = false;

            for (int i = 0; i < bricks.Length; i++)
            {
                if (bricks[i].IsActivated)
                {
                    activatedBrickIds.Add(bricks[i].GetInstanceID());
                }
            }

            CreateOrUpdateCompleteUI();
            completePanel.SetActive(false);
            UpdateHudLevelText();
            WeaponManager.Instance.ApplyCurrentWeapon();
            CheckForCompletion();
        }

        private void HandleBrickActivated(BrickPhysicsActivator brick)
        {
            if (brick == null || levelComplete)
            {
                return;
            }

            if (totalBricks <= 0)
            {
                totalBricks = FindObjectsByType<BrickPhysicsActivator>(FindObjectsSortMode.None).Length;
            }

            activatedBrickIds.Add(brick.GetInstanceID());
            CheckForCompletion(true);
        }

        private void CheckForCompletion(bool fromGameplay = false)
        {
            if (levelComplete || ShotManager.IsLevelFailed || totalBricks <= 0 || activatedBrickIds.Count < totalBricks)
            {
                return;
            }

            levelComplete = true;
            if (fromGameplay) SaveManager.RecordCompletion(WeaponManager.CurrentStage, currentLevel);
            CreateOrUpdateCompleteUI();
            if (fromGameplay && currentLevel == MaxLevel && WeaponManager.CurrentStage != WeaponStage.Bazooka)
            {
                ShowWeaponUnlock();
                return;
            }

            completePanel.SetActive(true);
        }

        private void ShowWeaponUnlock()
        {
            if (!TryGetNextWeaponStage(WeaponManager.CurrentStage, out WeaponStage nextStage))
            {
                return;
            }

            completedWeaponStage = WeaponManager.CurrentStage;
            completePanel.SetActive(false);

            // Reuse the completion canvas and event system; no scene or Inspector wiring is needed.
            Transform canvas = completePanel.transform.parent;
            Image backdrop = CreateUnlockImage(canvas, "WeaponUnlockOverlay", Vector2.zero, Vector2.zero,
                new Color(0.06f, 0.10f, 0.15f, 0.76f), false);
            backdrop.raycastTarget = true;
            RectTransform backdropRect = backdrop.rectTransform;
            backdropRect.anchorMin = Vector2.zero;
            backdropRect.anchorMax = Vector2.one;
            backdropRect.offsetMin = Vector2.zero;
            backdropRect.offsetMax = Vector2.zero;

            CreateUnlockImage(backdrop.transform, "PanelShadow", new Vector2(0f, -14f), new Vector2(880f, 1000f),
                new Color(0.02f, 0.04f, 0.06f, 0.45f));
            Image panel = CreateUnlockImage(backdrop.transform, "UnlockPanel", Vector2.zero, new Vector2(880f, 1000f),
                new Color(1f, 0.96f, 0.82f));
            CreateUnlockImage(panel.transform, "GoldAccent", new Vector2(0f, 432f), new Vector2(200f, 12f),
                new Color(1f, 0.72f, 0.20f));

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            CreateUnlockText(panel.transform, "UnlockTitle", "NEW WEAPON UNLOCKED", font, 48,
                new Vector2(0f, 330f), new Vector2(800f, 100f), new Color(0.34f, 0.23f, 0.13f));
            string weaponName = nextStage == WeaponStage.DesertEagle ? "DESERT EAGLE" : nextStage.ToString().ToUpperInvariant();
            Text name = CreateUnlockText(panel.transform, "WeaponName", weaponName, font, 92,
                new Vector2(0f, 215f), new Vector2(800f, 130f), new Color(0.94f, 0.57f, 0.12f));
            Shadow nameShadow = name.gameObject.AddComponent<Shadow>();
            nameShadow.effectColor = new Color(0.35f, 0.19f, 0.06f, 0.25f);
            nameShadow.effectDistance = new Vector2(0f, -4f);
            CreateUnlockWeaponIcon(panel.transform, nextStage);

            CreateUnlockImage(panel.transform, "ContinueShadow", new Vector2(0f, -340f), new Vector2(680f, 150f),
                new Color(0.15f, 0.39f, 0.13f));
            Image buttonImage = CreateUnlockImage(panel.transform, "ContinueButton", new Vector2(0f, -328f),
                new Vector2(680f, 150f), Color.white);
            buttonImage.raycastTarget = true;
            unlockContinueButton = buttonImage.gameObject.AddComponent<Button>();
            unlockContinueButton.targetGraphic = buttonImage;
            ColorBlock colors = unlockContinueButton.colors;
            colors.normalColor = new Color(0.32f, 0.78f, 0.30f);
            colors.highlightedColor = new Color(0.42f, 0.88f, 0.38f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(0.22f, 0.62f, 0.22f);
            unlockContinueButton.colors = colors;
            CreateUnlockText(buttonImage.transform, "Label", "CONTINUE", font, 56, Vector2.zero,
                new Vector2(640f, 140f), Color.white);
            unlockContinueButton.onClick.AddListener(ContinueWeaponUnlock);
        }

        private void ContinueWeaponUnlock()
        {
            if (!levelComplete || currentLevel != MaxLevel || !completedWeaponStage.HasValue ||
                WeaponManager.CurrentStage != completedWeaponStage.Value)
            {
                return;
            }

            unlockContinueButton.interactable = false;
            completedWeaponStage = null;
            ReloadCurrentScene();
        }

        private Image CreateUnlockImage(Transform parent, string name, Vector2 position, Vector2 size,
            Color color, bool rounded = true)
        {
            GameObject child = new GameObject(name, typeof(RectTransform), typeof(Image));
            child.transform.SetParent(parent, false);
            Image image = child.GetComponent<Image>();
            image.rectTransform.anchorMin = image.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            image.rectTransform.anchoredPosition = position;
            image.rectTransform.sizeDelta = size;
            image.color = color;
            image.raycastTarget = false;
            if (rounded)
            {
                if (unlockRoundedSprite == null)
                {
                    const int sizePixels = 64;
                    const float radius = 16f;
                    Texture2D texture = new Texture2D(sizePixels, sizePixels, TextureFormat.RGBA32, false);
                    texture.wrapMode = TextureWrapMode.Clamp;
                    Color[] pixels = new Color[sizePixels * sizePixels];
                    for (int y = 0; y < sizePixels; y++)
                    {
                        for (int x = 0; x < sizePixels; x++)
                        {
                            Vector2 closest = new Vector2(Mathf.Clamp(x, radius, sizePixels - radius - 1f),
                                Mathf.Clamp(y, radius, sizePixels - radius - 1f));
                            float alpha = Mathf.Clamp01(radius - Vector2.Distance(new Vector2(x, y), closest) + 0.5f);
                            pixels[y * sizePixels + x] = new Color(1f, 1f, 1f, alpha);
                        }
                    }
                    texture.SetPixels(pixels);
                    texture.Apply();
                    unlockRoundedSprite = Sprite.Create(texture, new Rect(0f, 0f, sizePixels, sizePixels),
                        new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(17f, 17f, 17f, 17f));
                }
                image.sprite = unlockRoundedSprite;
                image.type = Image.Type.Sliced;
            }
            return image;
        }

        private Text CreateUnlockText(Transform parent, string name, string value, Font font, int fontSize,
            Vector2 position, Vector2 size, Color color)
        {
            Text text = CreateOrUpdateText(parent, name, value, font);
            text.rectTransform.anchoredPosition = position;
            text.rectTransform.sizeDelta = size;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        private void CreateUnlockWeaponIcon(Transform parent, WeaponStage stage)
        {
            Image badge = CreateUnlockImage(parent, "WeaponBadge", new Vector2(0f, -40f), new Vector2(540f, 300f),
                new Color(1f, 0.86f, 0.51f));
            Color dark = new Color(0.18f, 0.23f, 0.25f);
            Color wood = new Color(0.58f, 0.32f, 0.14f);
            if (stage == WeaponStage.DesertEagle)
            {
                Color steel = new Color(0.32f, 0.36f, 0.40f);
                Color black = new Color(0.07f, 0.08f, 0.10f);
                CreateUnlockImage(badge.transform, "PistolFrame", new Vector2(-10f, 8f), new Vector2(270f, 40f), dark);
                CreateUnlockImage(badge.transform, "PistolSlide", new Vector2(0f, 48f), new Vector2(320f, 65f), steel, false);
                CreateUnlockImage(badge.transform, "PistolMuzzle", new Vector2(163f, 48f), new Vector2(16f, 48f), black, false);
                CreateUnlockImage(badge.transform, "PistolGrip", new Vector2(-94f, -60f), new Vector2(70f, 130f), black)
                    .rectTransform.localRotation = Quaternion.Euler(0f, 0f, -16f);
                CreateUnlockImage(badge.transform, "PistolMagazineBase", new Vector2(-110f, -120f), new Vector2(82f, 16f), steel);
                // Separate bars leave a visible opening inside the trigger guard.
                CreateUnlockImage(badge.transform, "PistolGuardFront", new Vector2(18f, -35f), new Vector2(12f, 64f), dark);
                CreateUnlockImage(badge.transform, "PistolGuardBottom", new Vector2(-13f, -62f), new Vector2(74f, 12f), dark);
                CreateUnlockImage(badge.transform, "PistolTrigger", new Vector2(-18f, -25f), new Vector2(10f, 32f), steel);
                CreateUnlockImage(badge.transform, "PistolFrontSight", new Vector2(132f, 88f), new Vector2(18f, 16f), black, false);
                CreateUnlockImage(badge.transform, "PistolRearSight", new Vector2(-124f, 88f), new Vector2(30f, 16f), black, false);
            }
            else if (stage == WeaponStage.Cannon)
            {
                CreateUnlockImage(badge.transform, "Carriage", new Vector2(0f, -40f), new Vector2(240f, 55f), wood);
                CreateUnlockImage(badge.transform, "Barrel", new Vector2(10f, 20f), new Vector2(320f, 95f), dark)
                    .rectTransform.localRotation = Quaternion.Euler(0f, 0f, 12f);
                CreateUnlockImage(badge.transform, "Muzzle", new Vector2(162f, 53f), new Vector2(34f, 102f), dark);
                CreateUnlockImage(badge.transform, "Wheel", new Vector2(-60f, -55f), new Vector2(100f, 100f), wood);
                CreateUnlockImage(badge.transform, "WheelHub", new Vector2(-60f, -55f), new Vector2(38f, 38f), dark);
            }
            else if (stage == WeaponStage.Crossbow)
            {
                CreateUnlockImage(badge.transform, "Stock", new Vector2(0f, -25f), new Vector2(55f, 200f), wood);
                CreateUnlockImage(badge.transform, "LeftBow", new Vector2(-80f, 40f), new Vector2(185f, 30f), dark)
                    .rectTransform.localRotation = Quaternion.Euler(0f, 0f, 20f);
                CreateUnlockImage(badge.transform, "RightBow", new Vector2(80f, 40f), new Vector2(185f, 30f), dark)
                    .rectTransform.localRotation = Quaternion.Euler(0f, 0f, -20f);
                CreateUnlockImage(badge.transform, "String", new Vector2(0f, 8f), new Vector2(320f, 5f), wood, false);
                CreateUnlockImage(badge.transform, "Bolt", new Vector2(0f, 40f), new Vector2(10f, 170f), dark);
            }
            else
            {
                CreateUnlockImage(badge.transform, "Grip", new Vector2(-45f, -60f), new Vector2(45f, 100f), dark);
                CreateUnlockImage(badge.transform, "Tube", new Vector2(0f, 20f), new Vector2(350f, 90f),
                    new Color(0.18f, 0.47f, 0.26f));
                CreateUnlockImage(badge.transform, "RearRim", new Vector2(-165f, 20f), new Vector2(32f, 115f), dark);
                CreateUnlockImage(badge.transform, "FrontRim", new Vector2(165f, 20f), new Vector2(32f, 110f), dark);
                CreateUnlockImage(badge.transform, "Sight", new Vector2(35f, 85f), new Vector2(60f, 40f), dark);
            }
        }

        private void OnDestroy()
        {
            if (unlockRoundedSprite != null)
            {
                Destroy(unlockRoundedSprite.texture);
                Destroy(unlockRoundedSprite);
            }
        }

        private void CreateOrUpdateCompleteUI()
        {
            EnsureEventSystem();

            GameObject canvasObject = GameObject.Find(CanvasName);
            if (canvasObject == null)
            {
                canvasObject = new GameObject(CanvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            }

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;

            GraphicRaycaster raycaster = canvasObject.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = canvasObject.AddComponent<GraphicRaycaster>();
            }

            raycaster.enabled = true;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.65f;

            completePanel = canvasObject.transform.Find("LevelCompletePanel")?.gameObject;
            if (completePanel == null)
            {
                completePanel = new GameObject("LevelCompletePanel", typeof(RectTransform), typeof(Image));
                completePanel.transform.SetParent(canvasObject.transform, false);
            }

            RectTransform panelRect = completePanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(720f, 460f);

            Image panelImage = completePanel.GetComponent<Image>();
            panelImage.color = new Color(1f, 0.96f, 0.82f, 0.96f);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            Text title = CreateOrUpdateText(completePanel.transform, "Title", "LEVEL COMPLETE!", font);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0f, 100f);
            titleRect.sizeDelta = new Vector2(640f, 120f);
            title.fontSize = 66;
            title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = new Color(0.17f, 0.12f, 0.08f);

            Button nextButton = CreateOrUpdateButton(completePanel.transform, "NextLevelButton", font);
            RectTransform buttonRect = nextButton.GetComponent<RectTransform>();
            buttonRect.anchoredPosition = new Vector2(0f, -100f);
            buttonRect.sizeDelta = new Vector2(430f, 110f);
            nextButton.interactable = true;
            nextButton.onClick.RemoveListener(ReloadCurrentScene);
            nextButton.onClick.AddListener(ReloadCurrentScene);
        }

        private void UpdateHudLevelText()
        {
            GameObject hud = GameObject.Find("PrototypeHUD");
            if (hud == null)
            {
                return;
            }

            Transform levelTextTransform = hud.transform.Find("LevelText");
            if (levelTextTransform == null)
            {
                return;
            }

            Text levelText = levelTextTransform.GetComponent<Text>();
            if (levelText != null)
            {
                levelText.text = WeaponManager.CurrentWeaponDisplayName + " \u2022 LEVEL " + currentLevel + "/" + MaxLevel;
                levelText.fontSize = 42;
                levelText.alignment = TextAnchor.UpperCenter;

                RectTransform rect = levelText.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 1f);
                    rect.anchorMax = new Vector2(0.5f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    rect.anchoredPosition = new Vector2(0f, -28f);
                    rect.sizeDelta = new Vector2(760f, 78f);
                }
            }
        }

        private void EnsureEventSystem()
        {
            EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
                eventSystem = eventSystemObject.GetComponent<EventSystem>();
            }

            InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
            {
                inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }

            BaseInputModule[] modules = eventSystem.GetComponents<BaseInputModule>();
            for (int i = 0; i < modules.Length; i++)
            {
                modules[i].enabled = modules[i] == inputModule;
            }

            eventSystem.enabled = true;
            inputModule.enabled = true;
        }

        private Text CreateOrUpdateText(Transform parent, string objectName, string value, Font font)
        {
            Transform child = parent.Find(objectName);
            if (child == null)
            {
                child = new GameObject(objectName, typeof(RectTransform), typeof(Text)).transform;
                child.SetParent(parent, false);
            }

            RectTransform rect = child.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            Text text = child.GetComponent<Text>();
            text.text = value;
            text.font = font;
            return text;
        }

        private Button CreateOrUpdateButton(Transform parent, string objectName, Font font)
        {
            Transform child = parent.Find(objectName);
            if (child == null)
            {
                child = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button)).transform;
                child.SetParent(parent, false);
            }

            Image image = child.GetComponent<Image>();
            image.color = new Color(0.32f, 0.78f, 0.30f, 1f);
            image.raycastTarget = true;

            Button button = child.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.32f, 0.78f, 0.30f, 1f);
            colors.highlightedColor = new Color(0.42f, 0.88f, 0.38f, 1f);
            colors.pressedColor = new Color(0.22f, 0.62f, 0.22f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            string buttonLabel = currentLevel == MaxLevel && WeaponManager.CurrentStage == WeaponStage.Bazooka
                ? "MAIN MENU" : "NEXT LEVEL";
            Text label = CreateOrUpdateText(child, "Label", buttonLabel, font);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            label.fontSize = 42;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;

            return button;
        }

#if UNITY_EDITOR
        public static void DebugJumpToLevel(int level)
        {
            SetCurrentLevel(level);
            SaveManager.UseRuntimeProgressForNextLoad(WeaponManager.CurrentStage, currentLevel);
            ReloadActiveScene();
        }

        public static void DebugReloadCurrentLevel()
        {
            SaveManager.UseRuntimeProgressForNextLoad(WeaponManager.CurrentStage, currentLevel);
            ReloadActiveScene();
        }
#endif

        private static bool TryGetNextWeaponStage(WeaponStage stage, out WeaponStage nextStage)
        {
            switch (stage)
            {
                case WeaponStage.Slingshot:
                    nextStage = WeaponStage.DesertEagle;
                    return true;
                case WeaponStage.DesertEagle:
                    nextStage = WeaponStage.Cannon;
                    return true;
                case WeaponStage.Cannon:
                    nextStage = WeaponStage.Crossbow;
                    return true;
                case WeaponStage.Crossbow:
                    nextStage = WeaponStage.Bazooka;
                    return true;
                default:
                    nextStage = stage;
                    return false;
            }
        }

        private void ReloadCurrentScene()
        {
            if (!levelComplete || completedWeaponStage.HasValue)
            {
                return;
            }

            if (currentLevel == MaxLevel && WeaponManager.CurrentStage == WeaponStage.Bazooka)
            {
                completePanel.transform.Find("NextLevelButton").GetComponent<Button>().interactable = false;
                SceneManager.LoadScene("MainMenu");
                return;
            }

            if (currentLevel < MaxLevel)
            {
                currentLevel = Mathf.Max(1, currentLevel + 1);
            }
            else if (TryGetNextWeaponStage(WeaponManager.CurrentStage, out WeaponStage nextStage))
            {
                WeaponManager.SetStage(nextStage);
                currentLevel = 1;
            }
            else
            {
                return;
            }

            SaveManager.SaveProgress(WeaponManager.CurrentStage, currentLevel);
            ReloadActiveScene();
        }

        private static void ReloadActiveScene()
        {
            Scene activeScene = SceneManager.GetActiveScene();

#if UNITY_EDITOR
            if (activeScene.buildIndex < 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.LoadScene(activeScene.path);
                return;
            }
#endif

            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}
