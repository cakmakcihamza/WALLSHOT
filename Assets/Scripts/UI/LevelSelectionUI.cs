using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WallBreaker.Game;

namespace WallBreaker.UI
{
    public class LevelSelectionUI : MonoBehaviour
    {
        private GameObject root;
        private GameObject mainMenu;
        private bool loading;
        private Font font;
        private ScrollRect scroll;
        private readonly List<Button> buttons = new List<Button>();
        private readonly List<GameObject> locks = new List<GameObject>();
        private static readonly string[] Names = { "SLINGSHOT", "DESERT EAGLE", "CANNON", "CROSSBOW", "BAZOOKA" };

        public void Open(GameObject menu)
        {
            if (loading) return;
            mainMenu = menu;
            if (root == null) Build();
            Refresh();
            if (mainMenu != null) mainMenu.SetActive(false);
            root.SetActive(true);
            scroll.verticalNormalizedPosition = 1f;
        }

        public void Back()
        {
            if (loading) return;
            if (root != null) root.SetActive(false);
            if (mainMenu != null) mainMenu.SetActive(true);
        }

        public void StartLevel(WeaponStage stage, int level)
        {
            if (loading || !SaveManager.TrySelectLevel(stage, level)) return;
            loading = true;
            SceneManager.LoadScene("SampleScene");
        }

        private void Refresh()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                bool unlocked = SaveManager.IsLevelUnlocked(SaveManager.OrderedStages[i / 10], i % 10 + 1);
                buttons[i].interactable = unlocked;
                locks[i].SetActive(!unlocked);
            }
        }

        private void Build()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            root = new GameObject("LevelSelectionCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.65f;
            Image background = Box(root.transform, "Background", new Color(0.055f, 0.075f, 0.105f));
            Stretch(background.rectTransform);
            Text title = Label(root.transform, "LEVEL SELECTION", 62);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(1000f, 110f));
            Button back = MakeButton(root.transform, "BACK", 34);
            Place(back.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(145f, -55f), new Vector2(230f, 80f));
            back.onClick.AddListener(Back);

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
            viewport.transform.SetParent(root.transform, false);
            RectTransform viewRect = viewport.GetComponent<RectTransform>();
            Stretch(viewRect);
            viewRect.offsetMin = new Vector2(45f, 70f);
            viewRect.offsetMax = new Vector2(-45f, -240f);
            viewport.GetComponent<Image>().color = new Color(0.07f, 0.09f, 0.13f);
            scroll = viewport.GetComponent<ScrollRect>();
            scroll.viewport = viewRect;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 45f;

            RectTransform content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter)).GetComponent<RectTransform>();
            content.SetParent(viewport.transform, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;
            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 20, 30);
            layout.spacing = 30f;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = content;
            for (int s = 0; s < SaveManager.OrderedStages.Length; s++)
            {
                Text heading = Label(content, Names[s], 42);
                heading.color = new Color(1f, 0.77f, 0.25f);
                heading.gameObject.AddComponent<LayoutElement>().preferredHeight = 75f;
                for (int row = 0; row < 2; row++)
                {
                    GameObject rowObject = new GameObject("LevelRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
                    rowObject.transform.SetParent(content, false);
                    rowObject.GetComponent<LayoutElement>().preferredHeight = 145f;
                    HorizontalLayoutGroup rowLayout = rowObject.GetComponent<HorizontalLayoutGroup>();
                    rowLayout.spacing = 18f;
                    rowLayout.childControlWidth = rowLayout.childControlHeight = true;
                    rowLayout.childForceExpandWidth = rowLayout.childForceExpandHeight = true;
                    for (int col = 0; col < 5; col++)
                    {
                        int level = row * 5 + col + 1;
                        WeaponStage stage = SaveManager.OrderedStages[s];
                        Button button = MakeButton(rowObject.transform, level.ToString("00"), 48);
                        button.onClick.AddListener(() => StartLevel(stage, level));
                        buttons.Add(button);
                        locks.Add(CreateLock(button.transform));
                    }
                }
            }
        }

        private Button MakeButton(Transform parent, string text, int size)
        {
            Image image = Box(parent, "Button_" + text, Color.white);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.18f, 0.42f, 0.57f);
            colors.highlightedColor = new Color(0.25f, 0.57f, 0.72f);
            colors.selectedColor = colors.normalColor;
            colors.pressedColor = new Color(0.12f, 0.29f, 0.39f);
            colors.disabledColor = new Color(0.13f, 0.16f, 0.21f);
            button.colors = colors;
            Text label = Label(image.transform, text, size);
            Stretch(label.rectTransform);
            return button;
        }

        private GameObject CreateLock(Transform parent)
        {
            RectTransform icon = new GameObject("LockIcon", typeof(RectTransform)).GetComponent<RectTransform>();
            icon.SetParent(parent, false);
            Place(icon, new Vector2(1f, 0f), new Vector2(-25f, 25f), new Vector2(30f, 34f));
            Color color = new Color(0.67f, 0.71f, 0.77f);
            Image body = Box(icon, "Body", color); body.raycastTarget = false;
            Place(body.rectTransform, Vector2.one * 0.5f, new Vector2(0f, -6f), new Vector2(26f, 20f));
            foreach (float x in new[] { -8f, 8f })
            {
                Image side = Box(icon, "ShackleSide", color); side.raycastTarget = false;
                Place(side.rectTransform, Vector2.one * 0.5f, new Vector2(x, 8f), new Vector2(5f, 14f));
            }
            Image top = Box(icon, "ShackleTop", color); top.raycastTarget = false;
            Place(top.rectTransform, Vector2.one * 0.5f, new Vector2(0f, 14f), new Vector2(21f, 5f));
            return icon.gameObject;
        }

        private Image Box(Transform parent, string name, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            Image image = obj.GetComponent<Image>(); image.color = color; return image;
        }
        private Text Label(Transform parent, string value, int size)
        {
            GameObject obj = new GameObject("Label", typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);
            Text text = obj.GetComponent<Text>();
            text.text = value; text.font = font; text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter; text.color = Color.white; text.raycastTarget = false;
            return text;
        }
        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        private static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = anchor; rect.pivot = Vector2.one * 0.5f;
            rect.anchoredPosition = position; rect.sizeDelta = size;
        }
    }
}
