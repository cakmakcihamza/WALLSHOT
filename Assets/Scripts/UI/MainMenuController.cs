using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace WallBreaker.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private LevelSelectionUI levelSelection;
        private Sprite roundedSprite;
        private Sprite circleSprite;
        private Sprite hillSprite;
        private Sprite islandBodySprite;
        private GameObject settingsPanel;
        private Text soundValueText;
        private Text vibrationValueText;

        private void Start()
        {
            SetupCamera();
            EnsureEventSystem();
            CreateMenu();
        }

        public void Play()
        {
            if (levelSelection == null) levelSelection = GetComponent<LevelSelectionUI>();
            if (levelSelection == null) levelSelection = gameObject.AddComponent<LevelSelectionUI>();
            levelSelection.Open(GameObject.Find("MainMenuCanvas"));
        }

        private void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera", typeof(Camera));
                mainCamera = cameraObject.GetComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.58f, 0.78f, 0.96f);
            mainCamera.transform.position = new Vector3(0f, 0f, -10f);
            mainCamera.transform.rotation = Quaternion.identity;
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
        }

        private void CreateMenu()
        {
            if (GameObject.Find("MainMenuCanvas") != null)
            {
                return;
            }

            GameObject canvasObject = new GameObject("MainMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.65f;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            CreateRuntimeSprites();
            CreateBackground(canvasObject.transform);
            CreateDecorations(canvasObject.transform);
            CreateTitle(canvasObject.transform, font);
            CreatePlayButton(canvasObject.transform, font);
            CreateSettingsButton(canvasObject.transform, font);
            CreateSettingsPanel(canvasObject.transform, font);
        }

        private void CreateRuntimeSprites()
        {
            roundedSprite = CreateSprite(96, 96, (x, y, size) =>
            {
                float radius = size * 0.24f;
                float left = radius;
                float right = size - radius - 1f;
                float bottom = radius;
                float top = size - radius - 1f;
                float closestX = Mathf.Clamp(x, left, right);
                float closestY = Mathf.Clamp(y, bottom, top);
                return Vector2.Distance(new Vector2(x, y), new Vector2(closestX, closestY)) <= radius;
            });

            circleSprite = CreateSprite(96, 96, (x, y, size) =>
            {
                Vector2 center = new Vector2((size - 1f) * 0.5f, (size - 1f) * 0.5f);
                return Vector2.Distance(new Vector2(x, y), center) <= size * 0.48f;
            });

            hillSprite = CreateSprite(128, 128, (x, y, size) =>
            {
                float normalizedX = x / (size - 1f);
                float peak = 0.86f - Mathf.Abs(normalizedX - 0.5f) * 1.18f;
                return y / (size - 1f) <= Mathf.Clamp01(peak);
            });

            islandBodySprite = CreateSprite(128, 128, (x, y, size) =>
            {
                float normalizedY = y / (size - 1f);
                float halfWidth = Mathf.Lerp(0.18f, 0.48f, normalizedY);
                float normalizedX = Mathf.Abs((x / (size - 1f)) - 0.5f);
                return normalizedX <= halfWidth;
            });
        }

        private void CreateBackground(Transform parent)
        {
            GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(parent, false);

            RectTransform rect = background.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = background.GetComponent<Image>();
            image.color = new Color(0.60f, 0.82f, 0.98f, 1f);
            image.raycastTarget = false;
        }

        private void CreateDecorations(Transform parent)
        {
            CreateHill(parent, "FarMountainLeft", new Vector2(-330f, -620f), new Vector2(760f, 370f), new Color(0.70f, 0.84f, 0.89f, 1f));
            CreateHill(parent, "FarMountainRight", new Vector2(330f, -650f), new Vector2(840f, 400f), new Color(0.66f, 0.80f, 0.87f, 1f));
            CreateHill(parent, "NearHillLeft", new Vector2(-360f, -760f), new Vector2(760f, 320f), new Color(0.45f, 0.74f, 0.48f, 1f));
            CreateHill(parent, "NearHillRight", new Vector2(360f, -790f), new Vector2(820f, 340f), new Color(0.39f, 0.68f, 0.44f, 1f));

            CreateCloud(parent, new Vector2(-310f, -250f), 1.05f);
            CreateCloud(parent, new Vector2(330f, -380f), 0.78f);
            CreateCloud(parent, new Vector2(70f, -170f), 0.62f);

            CreateIsland(parent);
            CreateTree(parent, new Vector2(-350f, 660f), 0.9f);
            CreateTree(parent, new Vector2(365f, 690f), 0.72f);
            CreateRock(parent, new Vector2(-210f, 760f), 0.75f);
            CreateRock(parent, new Vector2(255f, 770f), 0.58f);
        }

        private void CreateTitle(Transform parent, Font font)
        {
            GameObject titleObject = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleObject.transform.SetParent(parent, false);

            RectTransform rect = titleObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -330f);
            rect.sizeDelta = new Vector2(860f, 190f);

            Text title = titleObject.GetComponent<Text>();
            title.text = "WALLSHOT";
            title.font = font;
            title.fontSize = 92;
            title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = new Color(1f, 0.77f, 0.25f);
            title.raycastTarget = false;

            Shadow shadow = titleObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.25f, 0.12f, 0.05f, 0.65f);
            shadow.effectDistance = new Vector2(0f, -8f);

            Outline outline = titleObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.25f, 0.12f, 0.05f, 0.9f);
            outline.effectDistance = new Vector2(4f, -4f);
        }

        private void CreatePlayButton(Transform parent, Font font)
        {
            GameObject shadowObject = new GameObject("PlayButtonShadow", typeof(RectTransform), typeof(Image));
            shadowObject.transform.SetParent(parent, false);

            RectTransform shadowRect = shadowObject.GetComponent<RectTransform>();
            shadowRect.anchorMin = new Vector2(0.5f, 0.5f);
            shadowRect.anchorMax = new Vector2(0.5f, 0.5f);
            shadowRect.pivot = new Vector2(0.5f, 0.5f);
            shadowRect.anchoredPosition = new Vector2(0f, -382f);
            shadowRect.sizeDelta = new Vector2(540f, 150f);

            Image shadowImage = shadowObject.GetComponent<Image>();
            shadowImage.sprite = roundedSprite;
            shadowImage.type = Image.Type.Sliced;
            shadowImage.color = new Color(0.30f, 0.16f, 0.08f, 0.30f);
            shadowImage.raycastTarget = false;

            GameObject buttonObject = new GameObject("PlayButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -360f);
            rect.sizeDelta = new Vector2(540f, 150f);

            Image image = buttonObject.GetComponent<Image>();
            image.sprite = roundedSprite;
            image.type = Image.Type.Sliced;
            image.color = new Color(0.95f, 0.48f, 0.18f, 1f);
            image.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(Play);
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.95f, 0.48f, 0.18f, 1f);
            colors.highlightedColor = new Color(1f, 0.58f, 0.24f, 1f);
            colors.pressedColor = new Color(0.78f, 0.34f, 0.12f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            Text label = labelObject.GetComponent<Text>();
            label.text = "PLAY";
            label.font = font;
            label.fontSize = 64;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;

            Shadow labelShadow = labelObject.AddComponent<Shadow>();
            labelShadow.effectColor = new Color(0.44f, 0.18f, 0.06f, 0.7f);
            labelShadow.effectDistance = new Vector2(0f, -5f);
        }

        private void CreateSettingsButton(Transform parent, Font font)
        {
            GameObject buttonObject = new GameObject("SettingsButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-42f, -42f);
            rect.sizeDelta = new Vector2(108f, 108f);

            Image image = buttonObject.GetComponent<Image>();
            image.sprite = circleSprite;
            image.color = new Color(1f, 0.86f, 0.42f, 1f);
            image.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(OpenSettings);

            GameObject labelObject = new GameObject("Icon", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            Text label = labelObject.GetComponent<Text>();
            label.text = "⚙";
            label.font = font;
            label.fontSize = 58;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.24f, 0.14f, 0.08f);
            label.raycastTarget = false;
        }

        private void CreateSettingsPanel(Transform parent, Font font)
        {
            settingsPanel = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Image));
            settingsPanel.transform.SetParent(parent, false);

            RectTransform overlayRect = settingsPanel.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            Image overlayImage = settingsPanel.GetComponent<Image>();
            overlayImage.color = new Color(0.10f, 0.12f, 0.16f, 0.42f);
            overlayImage.raycastTarget = true;

            GameObject card = new GameObject("PanelCard", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(settingsPanel.transform, false);

            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = Vector2.zero;
            cardRect.sizeDelta = new Vector2(760f, 720f);

            Image cardImage = card.GetComponent<Image>();
            cardImage.sprite = roundedSprite;
            cardImage.type = Image.Type.Sliced;
            cardImage.color = new Color(1f, 0.93f, 0.76f, 0.98f);
            cardImage.raycastTarget = true;

            CreatePanelText(card.transform, "Title", "SETTINGS", font, new Vector2(0f, 220f), new Vector2(620f, 95f), 56, new Color(0.20f, 0.12f, 0.06f));

            soundValueText = CreateOptionButton(card.transform, "SoundButton", "SOUND", font, new Vector2(0f, 70f), ToggleSound);
            vibrationValueText = CreateOptionButton(card.transform, "VibrationButton", "VIBRATION", font, new Vector2(0f, -80f), ToggleVibration);
            CreateCloseButton(card.transform, font);

            RefreshSettingsLabels();
            settingsPanel.SetActive(false);
        }

        private Text CreateOptionButton(Transform parent, string objectName, string label, Font font, Vector2 anchoredPosition, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(560f, 105f);

            Image image = buttonObject.GetComponent<Image>();
            image.sprite = roundedSprite;
            image.type = Image.Type.Sliced;
            image.color = new Color(0.36f, 0.72f, 0.36f, 1f);
            image.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(action);

            CreatePanelText(buttonObject.transform, "Label", label, font, new Vector2(-110f, 0f), new Vector2(250f, 90f), 36, Color.white, TextAnchor.MiddleLeft);
            return CreatePanelText(buttonObject.transform, "Value", "ON", font, new Vector2(170f, 0f), new Vector2(180f, 90f), 38, Color.white, TextAnchor.MiddleCenter);
        }

        private void CreateCloseButton(Transform parent, Font font)
        {
            GameObject buttonObject = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -245f);
            rect.sizeDelta = new Vector2(330f, 95f);

            Image image = buttonObject.GetComponent<Image>();
            image.sprite = roundedSprite;
            image.type = Image.Type.Sliced;
            image.color = new Color(0.92f, 0.34f, 0.20f, 1f);
            image.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(CloseSettings);

            CreatePanelText(buttonObject.transform, "Label", "CLOSE", font, Vector2.zero, new Vector2(300f, 80f), 38, Color.white);
        }

        private Text CreatePanelText(Transform parent, string objectName, string value, Font font, Vector2 anchoredPosition, Vector2 size, int fontSize, Color color, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Text text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        private void OpenSettings()
        {
            RefreshSettingsLabels();
            settingsPanel.SetActive(true);
        }

        private void CloseSettings()
        {
            settingsPanel.SetActive(false);
        }

        private void ToggleSound()
        {
            SettingsManager.ToggleSound();
            RefreshSettingsLabels();
        }

        private void ToggleVibration()
        {
            SettingsManager.ToggleVibration();
            RefreshSettingsLabels();
        }

        private void RefreshSettingsLabels()
        {
            if (soundValueText != null)
            {
                soundValueText.text = SettingsManager.SoundEnabled ? "ON" : "OFF";
            }

            if (vibrationValueText != null)
            {
                vibrationValueText.text = SettingsManager.VibrationEnabled ? "ON" : "OFF";
            }
        }

        private void CreateHill(Transform parent, string objectName, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject hill = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            hill.transform.SetParent(parent, false);

            RectTransform rect = hill.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = hill.GetComponent<Image>();
            image.sprite = hillSprite;
            image.color = color;
            image.raycastTarget = false;
        }

        private void CreateCloud(Transform parent, Vector2 anchoredPosition, float scale)
        {
            GameObject cloudRoot = new GameObject("Cloud", typeof(RectTransform));
            cloudRoot.transform.SetParent(parent, false);

            RectTransform rootRect = cloudRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 1f);
            rootRect.anchorMax = new Vector2(0.5f, 1f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = new Vector2(250f * scale, 95f * scale);

            AddCircle(cloudRoot.transform, "PuffLeft", new Vector2(-62f, -5f) * scale, new Vector2(98f, 72f) * scale, new Color(1f, 1f, 1f, 0.88f));
            AddCircle(cloudRoot.transform, "PuffMiddle", new Vector2(0f, 8f) * scale, new Vector2(128f, 90f) * scale, new Color(1f, 1f, 1f, 0.92f));
            AddCircle(cloudRoot.transform, "PuffRight", new Vector2(70f, -8f) * scale, new Vector2(104f, 68f) * scale, new Color(1f, 1f, 1f, 0.86f));
        }

        private void CreateIsland(Transform parent)
        {
            GameObject body = CreateImage(parent, "IslandBody", islandBodySprite, new Color(0.54f, 0.32f, 0.18f, 1f), false);
            RectTransform bodyRect = body.GetComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0.5f, 0f);
            bodyRect.anchorMax = new Vector2(0.5f, 0f);
            bodyRect.pivot = new Vector2(0.5f, 0f);
            bodyRect.anchoredPosition = new Vector2(0f, -18f);
            bodyRect.sizeDelta = new Vector2(860f, 360f);

            GameObject grass = CreateImage(parent, "IslandGrass", roundedSprite, new Color(0.34f, 0.76f, 0.31f, 1f), false);
            RectTransform grassRect = grass.GetComponent<RectTransform>();
            grassRect.anchorMin = new Vector2(0.5f, 0f);
            grassRect.anchorMax = new Vector2(0.5f, 0f);
            grassRect.pivot = new Vector2(0.5f, 0f);
            grassRect.anchoredPosition = new Vector2(0f, 245f);
            grassRect.sizeDelta = new Vector2(900f, 130f);

            Image grassImage = grass.GetComponent<Image>();
            grassImage.type = Image.Type.Sliced;
        }

        private void CreateTree(Transform parent, Vector2 anchoredPosition, float scale)
        {
            GameObject trunk = CreateImage(parent, "TreeTrunk", roundedSprite, new Color(0.45f, 0.25f, 0.12f, 1f), false);
            RectTransform trunkRect = trunk.GetComponent<RectTransform>();
            trunkRect.anchorMin = new Vector2(0.5f, 0f);
            trunkRect.anchorMax = new Vector2(0.5f, 0f);
            trunkRect.pivot = new Vector2(0.5f, 0f);
            trunkRect.anchoredPosition = anchoredPosition;
            trunkRect.sizeDelta = new Vector2(34f, 100f) * scale;

            AddCircle(parent, "TreeCanopyBack", anchoredPosition + new Vector2(0f, 112f) * scale, new Vector2(118f, 100f) * scale, new Color(0.18f, 0.58f, 0.28f, 1f), new Vector2(0.5f, 0f));
            AddCircle(parent, "TreeCanopyFront", anchoredPosition + new Vector2(30f, 82f) * scale, new Vector2(94f, 82f) * scale, new Color(0.25f, 0.70f, 0.34f, 1f), new Vector2(0.5f, 0f));
        }

        private void CreateRock(Transform parent, Vector2 anchoredPosition, float scale)
        {
            AddCircle(parent, "Rock", anchoredPosition, new Vector2(82f, 48f) * scale, new Color(0.47f, 0.52f, 0.54f, 1f), new Vector2(0.5f, 0f));
        }

        private void AddCircle(Transform parent, string objectName, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            AddCircle(parent, objectName, anchoredPosition, size, color, new Vector2(0.5f, 0.5f));
        }

        private void AddCircle(Transform parent, string objectName, Vector2 anchoredPosition, Vector2 size, Color color, Vector2 anchor)
        {
            GameObject circle = CreateImage(parent, objectName, circleSprite, color, false);
            RectTransform rect = circle.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        private GameObject CreateImage(Transform parent, string objectName, Sprite sprite, Color color, bool raycastTarget)
        {
            GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);

            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = raycastTarget;
            return imageObject;
        }

        private delegate bool SpriteMask(int x, int y, int size);

        private Sprite CreateSprite(int width, int height, SpriteMask mask)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Color32 clear = new Color32(255, 255, 255, 0);
            Color32 solid = new Color32(255, 255, 255, 255);
            int size = Mathf.Min(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, mask(x, y, size) ? solid : clear);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(24f, 24f, 24f, 24f));
        }
    }
}
