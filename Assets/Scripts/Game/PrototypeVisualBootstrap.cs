using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WallBreaker.Game
{
    [DefaultExecutionOrder(100)]
    public class PrototypeVisualBootstrap : MonoBehaviour
    {
        private Material brickMaterial;
        private Material groundMaterial;
        private Material projectileMaterial;
        private Material slingshotMaterial;

        private void Start()
        {
            CreateMaterials();
            SetupCamera();
            SetupLight();
            SetupGround();
            SetupSlingshot();
            SetupImpactEffects();
            SetupAudio();
            SetupHud();
            SetupLevelManager();
            SetupWeaponManager();
            SetupShotManager();
            SetupEnvironmentVisuals();
            StartCoroutine(ApplyBrickMaterialAfterWallGeneration());
        }

        private void Update()
        {
            ApplyProjectileMaterialToActiveProjectiles();
        }

        private void CreateMaterials()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            brickMaterial = CreateMaterial(shader, "Runtime_BrickWarm", new Color(0.72f, 0.32f, 0.16f));
            groundMaterial = CreateMaterial(shader, "Runtime_GroundClean", new Color(0.86f, 0.92f, 0.88f));
            projectileMaterial = CreateMaterial(shader, "Runtime_ProjectileStone", new Color(0.16f, 0.16f, 0.18f));
            slingshotMaterial = CreateMaterial(shader, "Runtime_SlingshotWood", new Color(0.42f, 0.22f, 0.10f));
        }

        private Material CreateMaterial(Shader shader, string materialName, Color color)
        {
            Material material = new Material(shader)
            {
                name = materialName,
                color = color
            };

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            return material;
        }

        private void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.58f, 0.78f, 0.96f);
            mainCamera.fieldOfView = 56f;
            mainCamera.transform.position = new Vector3(0f, 5.8f, -8.8f);
            mainCamera.transform.rotation = Quaternion.Euler(16f, 0f, 0f);
        }

        private void SetupLight()
        {
            Light directionalLight = FindFirstObjectByType<Light>();
            if (directionalLight == null || directionalLight.type != LightType.Directional)
            {
                return;
            }

            directionalLight.color = new Color(1f, 0.95f, 0.86f);
            directionalLight.intensity = 2.2f;
            directionalLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private void SetupGround()
        {
            GameObject ground = GameObject.Find("Ground");
            if (ground == null)
            {
                return;
            }

            ground.transform.position = new Vector3(0f, -0.05f, 2.5f);
            ground.transform.localScale = new Vector3(8f, 0.1f, 12f);
            ApplyMaterial(ground, groundMaterial);
        }

        private void SetupSlingshot()
        {
            GameObject slingshot = GameObject.Find("SlingshotPlaceholder");
            if (slingshot == null)
            {
                return;
            }

            slingshot.transform.position = new Vector3(0f, 0.45f, -3f);

            Transform launchPoint = slingshot.transform.Find("LaunchPoint");
            if (launchPoint != null)
            {
                launchPoint.localPosition = new Vector3(0f, 0.75f, 0.18f);
            }

            global::WallBreaker.Visual.SlingshotVisual slingshotVisual =
                slingshot.GetComponent<global::WallBreaker.Visual.SlingshotVisual>();

            if (slingshotVisual == null)
            {
                slingshotVisual = slingshot.AddComponent<global::WallBreaker.Visual.SlingshotVisual>();
            }

            slingshotVisual.Initialize(launchPoint);
        }

        private void SetupImpactEffects()
        {
            _ = global::WallBreaker.Effects.ImpactEffectManager.Instance;
        }

        private void SetupAudio()
        {
            _ = global::WallBreaker.Audio.GameAudioManager.Instance;
        }

        private void SetupEnvironmentVisuals()
        {
            global::WallBreaker.Visuals.EnvironmentVisualBootstrap.EnsureExists().ApplyVisuals();
        }

        private void SetupLevelManager()
        {
            _ = LevelManager.Instance;
        }

        private void SetupShotManager()
        {
            ShotManager.Instance.ResetForLevel();
        }

        private void SetupWeaponManager()
        {
            WeaponManager.Instance.ApplyCurrentWeapon();
        }

        private void SetupHud()
        {
            if (GameObject.Find("PrototypeHUD") != null)
            {
                return;
            }

            GameObject canvasObject = new GameObject("PrototypeHUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            CreateText(canvasObject.transform, "LevelText", WeaponManager.CurrentWeaponDisplayName + " \u2022 LEVEL " + LevelManager.CurrentLevel + "/10", new Vector2(0f, -28f), TextAnchor.UpperCenter);
            CreateText(canvasObject.transform, "CoinText", "0", new Vector2(-32f, -32f), TextAnchor.UpperRight);
            Image progressFillImage = CreateProgressBar(canvasObject.transform);
            global::WallBreaker.UI.WallProgressUI progressUI =
                canvasObject.AddComponent<global::WallBreaker.UI.WallProgressUI>();
            progressUI.Initialize(progressFillImage);

            Text[] texts = canvasObject.GetComponentsInChildren<Text>();
            foreach (Text text in texts)
            {
                if (font != null)
                {
                    text.font = font;
                }
            }
        }

        private void CreateText(Transform parent, string objectName, string value, Vector2 anchoredPosition, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);

            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = GetTextAnchorPoint(alignment);
            rectTransform.anchorMax = rectTransform.anchorMin;
            rectTransform.pivot = GetTextAnchorPoint(alignment);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(260f, 70f);
            if (objectName == "LevelText")
            {
                rectTransform.sizeDelta = new Vector2(760f, 78f);
            }

            Text text = textObject.GetComponent<Text>();
            text.text = value;
            text.alignment = alignment;
            text.fontSize = objectName == "LevelText" ? 42 : 42;
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(0.12f, 0.15f, 0.20f);
        }

        private Vector2 GetTextAnchorPoint(TextAnchor alignment)
        {
            if (alignment == TextAnchor.UpperRight)
            {
                return new Vector2(1f, 1f);
            }

            if (alignment == TextAnchor.UpperCenter)
            {
                return new Vector2(0.5f, 1f);
            }

            return new Vector2(0f, 1f);
        }

        private Image CreateProgressBar(Transform parent)
        {
            GameObject background = new GameObject("ProgressBar", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(parent, false);

            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0.5f, 1f);
            backgroundRect.anchorMax = new Vector2(0.5f, 1f);
            backgroundRect.pivot = new Vector2(0.5f, 1f);
            backgroundRect.anchoredPosition = new Vector2(0f, -118f);
            backgroundRect.sizeDelta = new Vector2(420f, 34f);

            Image backgroundImage = background.GetComponent<Image>();
            backgroundImage.color = new Color(1f, 1f, 1f, 0.65f);

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(background.transform, false);

            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = new Vector2(4f, 4f);
            fillRect.offsetMax = new Vector2(-4f, -4f);

            Image fillImage = fill.GetComponent<Image>();
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0;
            fillImage.fillAmount = 0f;
            fillImage.color = new Color(0.27f, 0.72f, 0.42f, 1f);

            return fillImage;
        }

        private void ApplyProjectileMaterialToActiveProjectiles()
        {
            global::WallBreaker.Projectile.Projectile[] projectiles =
                FindObjectsByType<global::WallBreaker.Projectile.Projectile>(FindObjectsSortMode.None);

            foreach (global::WallBreaker.Projectile.Projectile projectile in projectiles)
            {
                ApplyMaterial(projectile.gameObject, projectileMaterial);
            }
        }

        private IEnumerator ApplyBrickMaterialAfterWallGeneration()
        {
            yield return null;

            GameObject wallGenerator = GameObject.Find("WallGenerator");
            if (wallGenerator == null)
            {
                yield break;
            }

            foreach (Renderer renderer in wallGenerator.GetComponentsInChildren<Renderer>())
            {
                renderer.sharedMaterial = brickMaterial;
            }
        }

        private void ApplyMaterial(GameObject target, Material material)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private void RemoveCollider(GameObject target)
        {
            Collider collider = target.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
        }
    }
}
