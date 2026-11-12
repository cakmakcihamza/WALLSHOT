using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WallBreaker.Visuals
{
    public class EnvironmentVisualBootstrap : MonoBehaviour
    {
        [Header("Island")]
        [SerializeField] private Color grassColor = new Color(0.32f, 0.78f, 0.30f);
        [SerializeField] private Color grassDarkColor = new Color(0.23f, 0.62f, 0.24f);
        [SerializeField] private Color dirtColor = new Color(0.55f, 0.34f, 0.18f);
        [SerializeField] private Color rockColor = new Color(0.42f, 0.43f, 0.40f);

        [Header("Background")]
        [SerializeField] private Color skyColor = new Color(0.66f, 0.86f, 1f);
        [SerializeField] private Color farMountainColor = new Color(0.67f, 0.82f, 0.76f);
        [SerializeField] private Color hillColor = new Color(0.42f, 0.72f, 0.43f);
        [SerializeField] private Color treeTrunkColor = new Color(0.48f, 0.27f, 0.12f);
        [SerializeField] private Color treeLeafColor = new Color(0.20f, 0.64f, 0.27f);

        [Header("Bricks")]
        [SerializeField] private Color brickBaseColor = new Color(0.86f, 0.34f, 0.13f);
        [SerializeField] private float brickColorVariation = 0.10f;
        [SerializeField] private Color wallBackdropColor = new Color(0.78f, 0.92f, 0.86f);

        private static EnvironmentVisualBootstrap instance;
        private Material grassMaterial;
        private Material dirtMaterial;
        private Material dirtLightMaterial;
        private Material dirtDarkMaterial;
        private Material rockMaterial;
        private Material rockDarkMaterial;
        private Material grassDarkMaterial;
        private Material farMountainMaterial;
        private Material hillMaterial;
        private Material trunkMaterial;
        private Material leafMaterial;
        private Material brickMaterial;
        private Material brickHighlightMaterial;
        private Material brickShadowMaterial;
        private Material flowerMaterial;
        private Material wallBackdropMaterial;
        private Material wallBackdropTrimMaterial;
        private Sprite hudCircleSprite;
        private bool visualsApplied;

        public static EnvironmentVisualBootstrap EnsureExists()
        {
            if (instance != null)
            {
                return instance;
            }

            EnvironmentVisualBootstrap existing = FindFirstObjectByType<EnvironmentVisualBootstrap>();
            if (existing != null)
            {
                instance = existing;
                return instance;
            }

            GameObject bootstrapObject = new GameObject("EnvironmentVisualBootstrap");
            instance = bootstrapObject.AddComponent<EnvironmentVisualBootstrap>();
            return instance;
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

        private void Start()
        {
            ApplyVisuals();
        }

        public void ApplyVisuals()
        {
            if (visualsApplied)
            {
                return;
            }

            visualsApplied = true;
            CreateMaterials();
            SetupSkyAndLighting();
            SetupIsland();
            SetupBackgroundDecor();
            SetupWallReadability();
            SetupHudPolish();
            StartCoroutine(ApplyBrickVisualsAfterWallGeneration());
        }

        private void CreateMaterials()
        {
            if (grassMaterial != null)
            {
                return;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            grassMaterial = CreateMaterial(shader, "Runtime_IslandGrass", grassColor, 0.35f);
            grassDarkMaterial = CreateMaterial(shader, "Runtime_IslandGrassDark", grassDarkColor, 0.28f);
            dirtMaterial = CreateMaterial(shader, "Runtime_IslandDirt", dirtColor, 0.22f);
            dirtLightMaterial = CreateMaterial(shader, "Runtime_IslandDirtLight", Color.Lerp(dirtColor, new Color(0.78f, 0.50f, 0.25f), 0.45f), 0.24f);
            dirtDarkMaterial = CreateMaterial(shader, "Runtime_IslandDirtDark", Color.Lerp(dirtColor, new Color(0.28f, 0.17f, 0.10f), 0.50f), 0.18f);
            rockMaterial = CreateMaterial(shader, "Runtime_DecorRock", rockColor, 0.30f);
            rockDarkMaterial = CreateMaterial(shader, "Runtime_DecorRockDark", Color.Lerp(rockColor, new Color(0.22f, 0.23f, 0.22f), 0.38f), 0.24f);
            farMountainMaterial = CreateMaterial(shader, "Runtime_FarMountain", farMountainColor);
            hillMaterial = CreateMaterial(shader, "Runtime_BackgroundHill", hillColor);
            trunkMaterial = CreateMaterial(shader, "Runtime_TreeTrunk", treeTrunkColor);
            leafMaterial = CreateMaterial(shader, "Runtime_TreeLeaves", treeLeafColor);
            brickMaterial = CreateMaterial(shader, "Runtime_StylizedBrick", brickBaseColor, 0.32f);
            brickHighlightMaterial = CreateMaterial(shader, "Runtime_BrickHighlight", Color.Lerp(brickBaseColor, Color.white, 0.28f), 0.36f);
            brickShadowMaterial = CreateMaterial(shader, "Runtime_BrickShadow", Color.Lerp(brickBaseColor, new Color(0.32f, 0.11f, 0.05f), 0.38f), 0.22f);
            flowerMaterial = CreateMaterial(shader, "Runtime_FlowerAccent", new Color(1f, 0.86f, 0.22f));
            wallBackdropMaterial = CreateMaterial(shader, "Runtime_WallBackdrop", wallBackdropColor, 0.40f);
            wallBackdropTrimMaterial = CreateMaterial(shader, "Runtime_WallBackdropTrim", Color.Lerp(wallBackdropColor, new Color(0.54f, 0.58f, 0.52f), 0.35f), 0.34f);
        }

        private Material CreateMaterial(Shader shader, string materialName, Color color)
        {
            return CreateMaterial(shader, materialName, color, 0.25f);
        }

        private Material CreateMaterial(Shader shader, string materialName, Color color, float smoothness)
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

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            return material;
        }

        private void SetupSkyAndLighting()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = skyColor;
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.72f, 0.86f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.55f, 0.72f, 0.58f);
            RenderSettings.ambientGroundColor = new Color(0.36f, 0.30f, 0.26f);
            RenderSettings.ambientIntensity = 1.05f;

            Light directionalLight = FindFirstObjectByType<Light>();
            if (directionalLight == null || directionalLight.type != LightType.Directional)
            {
                return;
            }

            directionalLight.color = new Color(1f, 0.95f, 0.84f);
            directionalLight.intensity = 1.82f;
            directionalLight.shadows = LightShadows.Soft;
            directionalLight.shadowStrength = 0.44f;
            directionalLight.transform.rotation = Quaternion.Euler(50f, -32f, 0f);
        }

        private void SetupIsland()
        {
            GameObject ground = GameObject.Find("Ground");
            if (ground == null)
            {
                return;
            }

            ApplyMaterial(ground, grassMaterial);

            Transform root = GetOrCreateRoot("Environment_Island");
            HideLegacyChild(root, "DirtLayerVisual");
            Vector3 center = ground.transform.position;
            Vector3 scale = ground.transform.localScale;

            GameObject grassTop = CreateOrUpdateCube(root, "GrassTopVisual", center + new Vector3(0f, 0.025f, 0.2f), new Vector3(scale.x * 0.82f, 0.03f, scale.z * 0.90f), grassMaterial);
            GameObject frontDirtEdge = CreateOrUpdateCube(root, "FrontDirtEdge", center + new Vector3(0f, -0.36f, -scale.z * 0.5f + 0.12f), new Vector3(scale.x * 0.95f, 0.72f, 0.34f), dirtMaterial);
            GameObject backDirtEdge = CreateOrUpdateCube(root, "BackDirtEdge", center + new Vector3(0f, -0.28f, scale.z * 0.5f - 0.2f), new Vector3(scale.x * 0.78f, 0.42f, 0.18f), dirtMaterial);
            GameObject leftDirtEdge = CreateOrUpdateCube(root, "LeftDirtEdge", center + new Vector3(-scale.x * 0.47f, -0.30f, 0.15f), new Vector3(0.22f, 0.58f, scale.z * 0.76f), dirtMaterial);
            GameObject rightDirtEdge = CreateOrUpdateCube(root, "RightDirtEdge", center + new Vector3(scale.x * 0.47f, -0.30f, 0.15f), new Vector3(0.22f, 0.58f, scale.z * 0.76f), dirtMaterial);
            GameObject frontCliffFace = CreateOrUpdateTaperedFace(root, "FrontCliffFace", center + new Vector3(0f, -0.95f, -scale.z * 0.5f + 0.02f), scale.x * 0.95f, scale.x * 0.42f, 1.45f, dirtMaterial);
            GameObject taperedBody = CreateOrUpdateTaperedBox(root, "TaperedIslandBody", center + new Vector3(0f, -1.02f, 0.25f), new Vector3(scale.x * 0.76f, 0.15f, scale.z * 0.72f), new Vector3(scale.x * 0.30f, -1.55f, scale.z * 0.26f), dirtMaterial);
            GameObject lowerRock = CreateOrUpdateSphere(root, "LowerRockMass", center + new Vector3(0f, -1.76f, -0.15f), new Vector3(scale.x * 0.24f, 0.62f, scale.z * 0.22f), rockMaterial);
            GameObject dirtLayerTop = CreateOrUpdateCube(root, "FrontDirtLayer_Top", center + new Vector3(0f, -0.22f, -scale.z * 0.5f - 0.06f), new Vector3(scale.x * 0.86f, 0.12f, 0.055f), dirtLightMaterial);
            GameObject dirtLayerMid = CreateOrUpdateCube(root, "FrontDirtLayer_Mid", center + new Vector3(0f, -0.62f, -scale.z * 0.5f - 0.08f), new Vector3(scale.x * 0.70f, 0.10f, 0.06f), dirtDarkMaterial);
            GameObject dirtLayerLow = CreateOrUpdateCube(root, "FrontDirtLayer_Low", center + new Vector3(0f, -1.04f, -scale.z * 0.5f - 0.09f), new Vector3(scale.x * 0.48f, 0.12f, 0.06f), dirtLightMaterial);
            GameObject rockLeft = CreateOrUpdateSphere(root, "LowerRockMass_Left", center + new Vector3(-0.95f, -1.60f, -0.22f), new Vector3(scale.x * 0.13f, 0.34f, scale.z * 0.12f), rockDarkMaterial);
            GameObject rockRight = CreateOrUpdateSphere(root, "LowerRockMass_Right", center + new Vector3(0.95f, -1.66f, -0.10f), new Vector3(scale.x * 0.12f, 0.38f, scale.z * 0.11f), rockMaterial);
            GameObject rockTip = CreateOrUpdateSphere(root, "LowerRockMass_Tip", center + new Vector3(0f, -2.18f, -0.08f), new Vector3(scale.x * 0.10f, 0.30f, scale.z * 0.09f), rockDarkMaterial);

            CreatePlayAreaDetails(root, center, scale);

            RemoveCollider(grassTop);
            RemoveCollider(frontDirtEdge);
            RemoveCollider(backDirtEdge);
            RemoveCollider(leftDirtEdge);
            RemoveCollider(rightDirtEdge);
            RemoveCollider(frontCliffFace);
            RemoveCollider(taperedBody);
            RemoveCollider(lowerRock);
            RemoveCollider(dirtLayerTop);
            RemoveCollider(dirtLayerMid);
            RemoveCollider(dirtLayerLow);
            RemoveCollider(rockLeft);
            RemoveCollider(rockRight);
            RemoveCollider(rockTip);
        }

        private void CreatePlayAreaDetails(Transform root, Vector3 center, Vector3 scale)
        {
            CreateOrUpdateSphere(root, "GrassClump_Left", center + new Vector3(-2.95f, 0.12f, 0.0f), new Vector3(0.20f, 0.08f, 0.20f), grassDarkMaterial);
            CreateOrUpdateSphere(root, "GrassClump_Right", center + new Vector3(2.95f, 0.12f, 1.1f), new Vector3(0.18f, 0.08f, 0.18f), grassDarkMaterial);
            CreateOrUpdateSphere(root, "GrassClump_FrontLeft", center + new Vector3(-2.45f, 0.12f, -2.35f), new Vector3(0.16f, 0.065f, 0.15f), grassDarkMaterial);
            CreateOrUpdateSphere(root, "GrassClump_FrontRight", center + new Vector3(2.35f, 0.12f, -2.05f), new Vector3(0.15f, 0.065f, 0.14f), grassDarkMaterial);
            CreateOrUpdateSphere(root, "GrassClump_BackLeft", center + new Vector3(-3.05f, 0.12f, 3.25f), new Vector3(0.18f, 0.07f, 0.16f), grassDarkMaterial);
            CreateOrUpdateSphere(root, "SmallStone_LeftPath", center + new Vector3(-2.9f, 0.12f, 2.55f), new Vector3(0.18f, 0.08f, 0.13f), rockMaterial);
            CreateOrUpdateSphere(root, "SmallStone_RightPath", center + new Vector3(2.8f, 0.12f, 2.35f), new Vector3(0.16f, 0.07f, 0.12f), rockMaterial);
            CreateOrUpdateSphere(root, "SmallStone_FrontEdge", center + new Vector3(1.9f, 0.12f, -2.58f), new Vector3(0.12f, 0.055f, 0.10f), rockDarkMaterial);
            CreateFlower(root, "Flower_Left", center + new Vector3(-3.15f, 0.16f, -0.95f), 0.08f);
            CreateFlower(root, "Flower_Right", center + new Vector3(3.1f, 0.16f, -0.55f), 0.075f);
            CreateFlower(root, "Flower_Front", center + new Vector3(-1.85f, 0.16f, -2.30f), 0.06f);
        }

        private void SetupBackgroundDecor()
        {
            Transform root = GetOrCreateRoot("Environment_Background");

            CreateOrUpdateSphere(root, "FarMountain_Left", new Vector3(-5.8f, 1.65f, 13.2f), new Vector3(5.2f, 1.55f, 0.55f), farMountainMaterial);
            CreateOrUpdateSphere(root, "FarMountain_Right", new Vector3(5.8f, 1.72f, 13.6f), new Vector3(5.4f, 1.7f, 0.55f), farMountainMaterial);

            CreateOrUpdateSphere(root, "Hill_Left", new Vector3(-5.0f, 0.9f, 9.4f), new Vector3(3.8f, 1.05f, 0.75f), hillMaterial);
            CreateOrUpdateSphere(root, "Hill_Center", new Vector3(0f, 0.75f, 10.2f), new Vector3(4.4f, 1.05f, 0.8f), hillMaterial);
            CreateOrUpdateSphere(root, "Hill_Right", new Vector3(5.0f, 0.9f, 9.5f), new Vector3(3.9f, 1.0f, 0.75f), hillMaterial);

            CreateTree(root, "Tree_Left_Back", new Vector3(-4.45f, 0.1f, 6.6f), 0.78f, 0.92f);
            CreateTree(root, "Tree_Right_Back", new Vector3(4.25f, 0.1f, 6.8f), 0.74f, 1.08f);
            CreateTree(root, "Tree_Far_Right", new Vector3(5.35f, 0.1f, 5.7f), 0.54f, 0.85f);
            CreateTree(root, "Tree_Far_Left", new Vector3(-5.35f, 0.1f, 5.55f), 0.58f, 1.15f);

            CreateOrUpdateSphere(root, "Rock_Left", new Vector3(-3.75f, 0.12f, 3.25f), new Vector3(0.46f, 0.18f, 0.30f), rockMaterial);
            CreateOrUpdateSphere(root, "Rock_Right", new Vector3(3.85f, 0.12f, 3.15f), new Vector3(0.42f, 0.16f, 0.30f), rockMaterial);

            RemoveCollidersInChildren(root);
        }

        private void SetupWallReadability()
        {
            Transform root = GetOrCreateRoot("Environment_WallFrame");
            GameObject backdrop = CreateOrUpdateCube(root, "WallBackdrop", new Vector3(0f, 1.50f, 5.38f), new Vector3(5.65f, 3.15f, 0.08f), wallBackdropMaterial);
            GameObject topTrim = CreateOrUpdateCube(root, "WallBackdrop_TopTrim", new Vector3(0f, 3.10f, 5.33f), new Vector3(5.95f, 0.12f, 0.12f), wallBackdropTrimMaterial);
            GameObject bottomTrim = CreateOrUpdateCube(root, "WallBackdrop_BottomTrim", new Vector3(0f, -0.10f, 5.33f), new Vector3(5.95f, 0.12f, 0.12f), wallBackdropTrimMaterial);
            GameObject leftTrim = CreateOrUpdateCube(root, "WallBackdrop_LeftTrim", new Vector3(-2.92f, 1.50f, 5.33f), new Vector3(0.12f, 3.12f, 0.12f), wallBackdropTrimMaterial);
            GameObject rightTrim = CreateOrUpdateCube(root, "WallBackdrop_RightTrim", new Vector3(2.92f, 1.50f, 5.33f), new Vector3(0.12f, 3.12f, 0.12f), wallBackdropTrimMaterial);
            RemoveCollider(backdrop);
            RemoveCollider(topTrim);
            RemoveCollider(bottomTrim);
            RemoveCollider(leftTrim);
            RemoveCollider(rightTrim);
        }

        private void CreateTree(Transform root, string treeName, Vector3 position, float size, float widthMultiplier)
        {
            Transform treeRoot = root.Find(treeName);
            if (treeRoot == null)
            {
                treeRoot = new GameObject(treeName).transform;
                treeRoot.SetParent(root, false);
            }

            treeRoot.position = position;
            CreateOrUpdateCylinder(treeRoot, "Trunk", position + new Vector3(0f, 0.38f * size, 0f), 0.09f * size, 0.38f * size, trunkMaterial);
            CreateOrUpdateSphere(treeRoot, "Canopy_Lower", position + new Vector3(0f, 0.78f * size, 0f), new Vector3(0.62f * widthMultiplier, 0.38f, 0.42f) * size, leafMaterial);
            CreateOrUpdateSphere(treeRoot, "Canopy_Left", position + new Vector3(-0.18f * size * widthMultiplier, 0.98f * size, 0f), new Vector3(0.38f, 0.32f, 0.36f) * size, leafMaterial);
            CreateOrUpdateSphere(treeRoot, "Canopy_Right", position + new Vector3(0.18f * size * widthMultiplier, 1.02f * size, 0.02f), new Vector3(0.40f, 0.34f, 0.36f) * size, leafMaterial);
        }

        private void CreateFlower(Transform root, string flowerName, Vector3 position, float size)
        {
            CreateOrUpdateCylinder(root, flowerName + "_Stem", position + new Vector3(0f, size * 0.35f, 0f), size * 0.18f, size * 0.45f, grassDarkMaterial);
            CreateOrUpdateSphere(root, flowerName + "_Head", position + new Vector3(0f, size * 0.95f, 0f), Vector3.one * size, flowerMaterial);
        }

        private void SetupHudPolish()
        {
            GameObject hud = GameObject.Find("PrototypeHUD");
            if (hud == null)
            {
                return;
            }

            CanvasScaler scaler = hud.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
                scaler.matchWidthOrHeight = 0.65f;
            }

            Text levelText = FindText(hud.transform, "LevelText");
            if (levelText != null)
            {
                levelText.fontSize = 44;
                levelText.fontStyle = FontStyle.Bold;
                levelText.color = new Color(0.06f, 0.10f, 0.13f);
                EnsureTextShadow(levelText, new Color(1f, 1f, 1f, 0.55f), new Vector2(0f, -2f));
                RectTransform rect = levelText.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(54f, -54f);
                rect.sizeDelta = new Vector2(280f, 76f);
            }

            Text coinText = FindText(hud.transform, "CoinText");
            if (coinText != null)
            {
                coinText.fontSize = 46;
                coinText.fontStyle = FontStyle.Bold;
                coinText.color = new Color(0.06f, 0.10f, 0.13f);
                EnsureTextShadow(coinText, new Color(1f, 1f, 1f, 0.50f), new Vector2(0f, -2f));
                RectTransform rect = coinText.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(-54f, -54f);
                rect.sizeDelta = new Vector2(150f, 76f);
            }

            Transform progress = hud.transform.Find("ProgressBar");
            if (progress != null)
            {
                RectTransform rect = progress.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(0f, -64f);
                rect.sizeDelta = new Vector2(420f, 40f);

                Image background = progress.GetComponent<Image>();
                if (background != null)
                {
                    background.color = new Color(0.04f, 0.08f, 0.10f, 0.86f);
                }

                Transform fill = progress.Find("Fill");
                if (fill != null)
                {
                    Image fillImage = fill.GetComponent<Image>();
                    if (fillImage != null)
                    {
                        fillImage.color = new Color(0.72f, 0.95f, 0.20f, 1f);
                    }

                    RectTransform fillRect = fill.GetComponent<RectTransform>();
                    fillRect.offsetMin = new Vector2(7f, 7f);
                    fillRect.offsetMax = new Vector2(-7f, -7f);
                }

                SetupProgressCaps(progress, rect.sizeDelta);
            }
        }

        private void SetupProgressCaps(Transform progress, Vector2 progressSize)
        {
            float capSize = progressSize.y;
            CreateHudCircle(progress, "TrackCapLeft", new Vector2(-progressSize.x * 0.5f, 0f), capSize, new Color(0.04f, 0.08f, 0.10f, 0.86f));
            CreateHudCircle(progress, "TrackCapRight", new Vector2(progressSize.x * 0.5f, 0f), capSize, new Color(0.04f, 0.08f, 0.10f, 0.86f));
        }

        private void EnsureTextShadow(Text text, Color color, Vector2 distance)
        {
            Shadow shadow = text.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = text.gameObject.AddComponent<Shadow>();
            }

            shadow.effectColor = color;
            shadow.effectDistance = distance;
        }

        private void CreateHudCircle(Transform parent, string objectName, Vector2 anchoredPosition, float size, Color color)
        {
            Transform child = parent.Find(objectName);
            if (child == null)
            {
                child = new GameObject(objectName, typeof(RectTransform), typeof(Image)).transform;
                child.SetParent(parent, false);
            }

            RectTransform rectTransform = child.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(size, size);

            Image image = child.GetComponent<Image>();
            image.sprite = GetHudCircleSprite();
            image.color = color;
        }

        private IEnumerator ApplyBrickVisualsAfterWallGeneration()
        {
            yield return null;
            yield return null;

            GameObject wallGenerator = GameObject.Find("WallGenerator");
            if (wallGenerator == null)
            {
                yield break;
            }

            Renderer[] renderers = wallGenerator.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                float variation = Random.Range(1f - brickColorVariation, 1f + brickColorVariation);
                Color variedColor = new Color(
                    Mathf.Clamp01(brickBaseColor.r * variation),
                    Mathf.Clamp01(brickBaseColor.g * variation),
                    Mathf.Clamp01(brickBaseColor.b * variation),
                    1f
                );

                Material brickInstance = new Material(brickMaterial)
                {
                    color = variedColor
                };

                if (brickInstance.HasProperty("_BaseColor"))
                {
                    brickInstance.SetColor("_BaseColor", variedColor);
                }

                renderers[i].sharedMaterial = brickInstance;
                SetupBrickFaceDetails(renderers[i].transform, variedColor);
            }
        }

        private void SetupBrickFaceDetails(Transform brick, Color variedColor)
        {
            Material faceMaterial = CreateRuntimeMaterialFromColor("Runtime_BrickInset_" + brick.GetInstanceID(), Color.Lerp(variedColor, Color.white, 0.10f), 0.34f);
            Material highlightMaterial = CreateRuntimeMaterialFromColor("Runtime_BrickEdgeLight_" + brick.GetInstanceID(), Color.Lerp(variedColor, Color.white, 0.36f), 0.38f);
            Material shadowMaterial = CreateRuntimeMaterialFromColor("Runtime_BrickEdgeDark_" + brick.GetInstanceID(), Color.Lerp(variedColor, brickShadowMaterial.color, 0.45f), 0.20f);

            CreateBrickDetailCube(brick, "BrickFrontInset", new Vector3(0f, 0f, -0.512f), new Vector3(0.86f, 0.68f, 0.025f), faceMaterial);
            CreateBrickDetailCube(brick, "BrickTopBevel", new Vector3(0f, 0.405f, -0.522f), new Vector3(0.88f, 0.055f, 0.030f), highlightMaterial);
            CreateBrickDetailCube(brick, "BrickBottomBevel", new Vector3(0f, -0.405f, -0.522f), new Vector3(0.88f, 0.055f, 0.030f), shadowMaterial);
            CreateBrickDetailCube(brick, "BrickLeftBevel", new Vector3(-0.455f, 0f, -0.524f), new Vector3(0.045f, 0.72f, 0.030f), shadowMaterial);
            CreateBrickDetailCube(brick, "BrickRightBevel", new Vector3(0.455f, 0f, -0.524f), new Vector3(0.045f, 0.72f, 0.030f), highlightMaterial);
        }

        private Material CreateRuntimeMaterialFromColor(string materialName, Color color, float smoothness)
        {
            Material material = new Material(brickMaterial)
            {
                name = materialName,
                color = color
            };

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            return material;
        }

        private void CreateBrickDetailCube(Transform brick, string objectName, Vector3 localPosition, Vector3 localScale, Material material)
        {
            Transform child = brick.Find(objectName);
            if (child == null)
            {
                child = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                child.name = objectName;
                child.SetParent(brick, false);
            }

            child.localPosition = localPosition;
            child.localRotation = Quaternion.identity;
            child.localScale = localScale;
            ApplyMaterial(child.gameObject, material);
            RemoveCollider(child.gameObject);
        }

        private Transform GetOrCreateRoot(string rootName)
        {
            GameObject rootObject = GameObject.Find(rootName);
            if (rootObject == null)
            {
                rootObject = new GameObject(rootName);
            }

            return rootObject.transform;
        }

        private GameObject CreateOrUpdateCube(Transform parent, string objectName, Vector3 position, Vector3 scale, Material material)
        {
            Transform child = GetOrCreatePrimitive(parent, objectName, PrimitiveType.Cube);
            child.position = position;
            child.rotation = Quaternion.identity;
            child.localScale = scale;
            ApplyMaterial(child.gameObject, material);
            return child.gameObject;
        }

        private void HideLegacyChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                child.gameObject.SetActive(false);
            }
        }

        private GameObject CreateOrUpdateSphere(Transform parent, string objectName, Vector3 position, Vector3 scale, Material material)
        {
            Transform child = GetOrCreatePrimitive(parent, objectName, PrimitiveType.Sphere);
            child.position = position;
            child.rotation = Quaternion.identity;
            child.localScale = scale;
            ApplyMaterial(child.gameObject, material);
            RemoveCollider(child.gameObject);
            return child.gameObject;
        }

        private GameObject CreateOrUpdateCylinder(Transform parent, string objectName, Vector3 position, float radius, float halfHeight, Material material)
        {
            Transform child = GetOrCreatePrimitive(parent, objectName, PrimitiveType.Cylinder);
            child.position = position;
            child.rotation = Quaternion.identity;
            child.localScale = new Vector3(radius, halfHeight, radius);
            ApplyMaterial(child.gameObject, material);
            RemoveCollider(child.gameObject);
            return child.gameObject;
        }

        private GameObject CreateOrUpdateTaperedBox(Transform parent, string objectName, Vector3 center, Vector3 topSize, Vector3 bottomSize, Material material)
        {
            Transform child = parent.Find(objectName);
            if (child == null)
            {
                child = new GameObject(objectName, typeof(MeshFilter), typeof(MeshRenderer)).transform;
                child.SetParent(parent, false);
            }

            child.position = center;
            child.rotation = Quaternion.identity;
            child.localScale = Vector3.one;

            MeshFilter meshFilter = child.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = CreateTaperedBoxMesh(topSize, bottomSize);
            ApplyMaterial(child.gameObject, material);
            RemoveCollider(child.gameObject);
            return child.gameObject;
        }

        private GameObject CreateOrUpdateTaperedFace(Transform parent, string objectName, Vector3 center, float topWidth, float bottomWidth, float height, Material material)
        {
            Transform child = parent.Find(objectName);
            if (child == null)
            {
                child = new GameObject(objectName, typeof(MeshFilter), typeof(MeshRenderer)).transform;
                child.SetParent(parent, false);
            }

            child.position = center;
            child.rotation = Quaternion.identity;
            child.localScale = Vector3.one;

            float topHalf = topWidth * 0.5f;
            float bottomHalf = bottomWidth * 0.5f;
            float halfHeight = height * 0.5f;

            Mesh mesh = new Mesh
            {
                name = "Runtime_TaperedIslandFrontFace",
                vertices = new[]
                {
                    new Vector3(-topHalf, halfHeight, 0f),
                    new Vector3(topHalf, halfHeight, 0f),
                    new Vector3(-bottomHalf, -halfHeight, 0f),
                    new Vector3(bottomHalf, -halfHeight, 0f)
                },
                triangles = new[] { 0, 1, 2, 1, 3, 2 }
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            MeshFilter meshFilter = child.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            ApplyMaterial(child.gameObject, material);
            RemoveCollider(child.gameObject);
            return child.gameObject;
        }

        private Mesh CreateTaperedBoxMesh(Vector3 topSize, Vector3 bottomSize)
        {
            float topY = topSize.y;
            float bottomY = bottomSize.y;
            float topHalfX = topSize.x * 0.5f;
            float topHalfZ = topSize.z * 0.5f;
            float bottomHalfX = bottomSize.x * 0.5f;
            float bottomHalfZ = bottomSize.z * 0.5f;

            Vector3[] vertices =
            {
                new Vector3(-topHalfX, topY, -topHalfZ),
                new Vector3(topHalfX, topY, -topHalfZ),
                new Vector3(topHalfX, topY, topHalfZ),
                new Vector3(-topHalfX, topY, topHalfZ),
                new Vector3(-bottomHalfX, bottomY, -bottomHalfZ),
                new Vector3(bottomHalfX, bottomY, -bottomHalfZ),
                new Vector3(bottomHalfX, bottomY, bottomHalfZ),
                new Vector3(-bottomHalfX, bottomY, bottomHalfZ)
            };

            int[] triangles =
            {
                0, 1, 2, 0, 2, 3,
                4, 6, 5, 4, 7, 6,
                0, 4, 5, 0, 5, 1,
                1, 5, 6, 1, 6, 2,
                2, 6, 7, 2, 7, 3,
                3, 7, 4, 3, 4, 0
            };

            Mesh mesh = new Mesh
            {
                name = "Runtime_TaperedIslandBody"
            };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private Transform GetOrCreatePrimitive(Transform parent, string objectName, PrimitiveType primitiveType)
        {
            Transform child = parent.Find(objectName);
            if (child != null)
            {
                return child;
            }

            child = GameObject.CreatePrimitive(primitiveType).transform;
            child.name = objectName;
            child.SetParent(parent, false);
            return child;
        }

        private Text FindText(Transform parent, string objectName)
        {
            Transform child = parent.Find(objectName);
            return child != null ? child.GetComponent<Text>() : null;
        }

        private Sprite GetHudCircleSprite()
        {
            if (hudCircleSprite != null)
            {
                return hudCircleSprite;
            }

            const int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Runtime_HudCircle"
            };

            Color clear = new Color(1f, 1f, 1f, 0f);
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.48f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    texture.SetPixel(x, y, distance <= radius ? Color.white : clear);
                }
            }

            texture.Apply();
            hudCircleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            hudCircleSprite.name = "Runtime_HudCircleSprite";
            return hudCircleSprite;
        }

        private void ApplyMaterial(GameObject target, Material material)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private void RemoveCollidersInChildren(Transform root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                Destroy(colliders[i]);
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
