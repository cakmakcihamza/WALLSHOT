using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WallBreaker.Game
{
    public class ShotManager : MonoBehaviour
    {
        private const int SlingshotShotLimit = 12;
        private const int DesertEagleShotLimit = 13;
        private const int CannonShotLimit = 10;
        private const int CrossbowShotLimit = 8;
        private const int BazookaShotLimit = 7;
        private const string FailedCanvasName = "LevelFailedCanvas";

        private static ShotManager instance;

        [SerializeField] private float failDelay = 2f;

        private int shotsRemaining;
        private bool levelFailed;
        private Text shotsText;
        private GameObject failedPanel;
        private Coroutine failRoutine;

        public static ShotManager Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                ShotManager existing = FindFirstObjectByType<ShotManager>();
                if (existing != null)
                {
                    instance = existing;
                    return instance;
                }

                GameObject managerObject = new GameObject("ShotManager");
                instance = managerObject.AddComponent<ShotManager>();
                return instance;
            }
        }

        public static int CurrentShotsRemaining => instance != null ? instance.shotsRemaining : SlingshotShotLimit;
        public static bool IsLevelFailed => instance != null && instance.levelFailed;
        public static bool IsInputLocked => IsLevelFailed || CurrentShotsRemaining <= 0;
        public static bool CanShoot => !LevelManager.IsLevelComplete && !IsInputLocked;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
        }

        public void ResetForLevel()
        {
            if (failRoutine != null)
            {
                StopCoroutine(failRoutine);
                failRoutine = null;
            }

            shotsRemaining = GetShotLimitForCurrentWeapon();
            levelFailed = false;

            CreateOrUpdateShotsHud();
            CreateOrUpdateFailedUI();
            failedPanel.SetActive(false);
            UpdateShotsText();
        }

        private int GetShotLimitForCurrentWeapon()
        {
            if (WeaponManager.CurrentWeapon == WeaponType.DesertEagle)
            {
                return DesertEagleShotLimit;
            }

            if (WeaponManager.CurrentWeapon == WeaponType.Bazooka)
            {
                return BazookaShotLimit;
            }

            if (WeaponManager.CurrentWeapon == WeaponType.Crossbow)
            {
                return CrossbowShotLimit;
            }

            return WeaponManager.CurrentWeapon == WeaponType.Cannon ? CannonShotLimit : SlingshotShotLimit;
        }

        public bool ConsumeShot()
        {
            if (!CanShoot)
            {
                return false;
            }

            shotsRemaining = Mathf.Max(0, shotsRemaining - 1);
            UpdateShotsText();

            if (shotsRemaining == 0 && failRoutine == null)
            {
                failRoutine = StartCoroutine(WaitThenFailIfNeeded());
            }

            return true;
        }

        private IEnumerator WaitThenFailIfNeeded()
        {
            yield return new WaitForSeconds(failDelay);
            failRoutine = null;

            if (LevelManager.IsLevelComplete || levelFailed)
            {
                yield break;
            }

            ShowLevelFailed();
        }

        private void ShowLevelFailed()
        {
            if (LevelManager.IsLevelComplete || levelFailed)
            {
                return;
            }

            levelFailed = true;
            CreateOrUpdateFailedUI();
            failedPanel.SetActive(true);
        }

        private void CreateOrUpdateShotsHud()
        {
            GameObject hud = GameObject.Find("PrototypeHUD");
            if (hud == null)
            {
                return;
            }

            Transform existing = hud.transform.Find("ShotsText");
            if (existing == null)
            {
                existing = new GameObject("ShotsText", typeof(RectTransform), typeof(Text)).transform;
                existing.SetParent(hud.transform, false);
            }

            RectTransform rect = existing.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 86f);
            rect.sizeDelta = new Vector2(360f, 76f);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            shotsText = existing.GetComponent<Text>();
            shotsText.font = font;
            shotsText.fontSize = 40;
            shotsText.fontStyle = FontStyle.Bold;
            shotsText.alignment = TextAnchor.MiddleCenter;
            shotsText.color = new Color(0.06f, 0.10f, 0.13f);
            shotsText.raycastTarget = false;
        }

        private void UpdateShotsText()
        {
            if (shotsText == null)
            {
                CreateOrUpdateShotsHud();
            }

            if (shotsText != null)
            {
                shotsText.text = "SHOTS " + shotsRemaining;
            }
        }

        private void CreateOrUpdateFailedUI()
        {
            EnsureEventSystem();

            GameObject canvasObject = GameObject.Find(FailedCanvasName);
            if (canvasObject == null)
            {
                canvasObject = new GameObject(FailedCanvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            }

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 45;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.65f;

            GraphicRaycaster raycaster = canvasObject.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = canvasObject.AddComponent<GraphicRaycaster>();
            }

            raycaster.enabled = true;

            failedPanel = canvasObject.transform.Find("LevelFailedPanel")?.gameObject;
            if (failedPanel == null)
            {
                failedPanel = new GameObject("LevelFailedPanel", typeof(RectTransform), typeof(Image));
                failedPanel.transform.SetParent(canvasObject.transform, false);
            }

            RectTransform panelRect = failedPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(720f, 460f);

            Image panelImage = failedPanel.GetComponent<Image>();
            panelImage.color = new Color(1f, 0.90f, 0.82f, 0.96f);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            Text title = CreateOrUpdateText(failedPanel.transform, "Title", "LEVEL FAILED!", font);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0f, 100f);
            titleRect.sizeDelta = new Vector2(640f, 120f);
            title.fontSize = 66;
            title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = new Color(0.28f, 0.10f, 0.06f);
            title.raycastTarget = false;

            Button retryButton = CreateOrUpdateButton(failedPanel.transform, "RetryButton", "RETRY", font);
            RectTransform buttonRect = retryButton.GetComponent<RectTransform>();
            buttonRect.anchoredPosition = new Vector2(0f, -100f);
            buttonRect.sizeDelta = new Vector2(430f, 110f);
            retryButton.interactable = true;
            retryButton.onClick.RemoveListener(RetryCurrentLevel);
            retryButton.onClick.AddListener(RetryCurrentLevel);
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

        private Button CreateOrUpdateButton(Transform parent, string objectName, string labelText, Font font)
        {
            Transform child = parent.Find(objectName);
            if (child == null)
            {
                child = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button)).transform;
                child.SetParent(parent, false);
            }

            Image image = child.GetComponent<Image>();
            image.color = new Color(0.92f, 0.34f, 0.20f, 1f);
            image.raycastTarget = true;

            Button button = child.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.92f, 0.34f, 0.20f, 1f);
            colors.highlightedColor = new Color(1f, 0.44f, 0.28f, 1f);
            colors.pressedColor = new Color(0.74f, 0.22f, 0.14f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            Text label = CreateOrUpdateText(child, "Label", labelText, font);
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

        private void RetryCurrentLevel()
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
