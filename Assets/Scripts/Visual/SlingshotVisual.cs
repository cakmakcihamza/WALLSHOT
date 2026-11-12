using UnityEngine;

namespace WallBreaker.Visual
{
    public class SlingshotVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform launchPoint;

        [Header("Visual Pull")]
        [SerializeField] private float maxVisualPullDistance = 0.68f;
        [SerializeField] private float returnSpeed = 18f;

        [Header("Shape")]
        [SerializeField] private float stoneRadius = 0.18f;
        [SerializeField] private float bandWidth = 0.055f;

        private readonly Vector3 leftAnchorLocalPosition = new Vector3(-0.62f, 1.2f, 0.02f);
        private readonly Vector3 rightAnchorLocalPosition = new Vector3(0.62f, 1.2f, 0.02f);
        private readonly Vector3 restStoneLocalPosition = new Vector3(0f, 0.98f, 0.2f);

        private Transform loadedStone;
        private LineRenderer leftBand;
        private LineRenderer rightBand;
        private Material woodMaterial;
        private Material woodTipMaterial;
        private Material stoneMaterial;
        private Material bandMaterial;
        private Material leatherMaterial;
        private Material metalMaterial;
        private Material stitchMaterial;
        private Texture2D woodGrain;
        private bool detailCreated;
        private bool isDragging;

        private void Awake()
        {
            EnsureVisuals();
            ResetVisual();
        }

        private void Update()
        {
            if (isDragging || loadedStone == null)
            {
                return;
            }

            loadedStone.localPosition = Vector3.Lerp(
                loadedStone.localPosition,
                restStoneLocalPosition,
                Time.deltaTime * returnSpeed
            );

            UpdateBands();
        }

        public void Initialize(Transform newLaunchPoint)
        {
            launchPoint = newLaunchPoint;
            EnsureVisuals();
            ResetVisual();
        }

        public void SetDrag(Vector2 dragVector, float maxDragDistance)
        {
            EnsureVisuals();
            isDragging = true;

            float effectiveMaxDragDistance = Mathf.Max(1f, maxDragDistance);
            Vector2 normalizedDrag = Vector2.ClampMagnitude(dragVector / effectiveMaxDragDistance, 1f);

            Vector3 visualOffset = new Vector3(
                -normalizedDrag.x * maxVisualPullDistance,
                -Mathf.Max(0f, normalizedDrag.y) * maxVisualPullDistance * 0.75f,
                -normalizedDrag.magnitude * maxVisualPullDistance * 0.65f
            );

            loadedStone.localPosition = restStoneLocalPosition + visualOffset;
            UpdateBands();
        }

        public void ResetVisual()
        {
            EnsureVisuals();
            isDragging = false;
            loadedStone.localPosition = restStoneLocalPosition;
            loadedStone.gameObject.SetActive(true);
            UpdateBands();
        }

        public void PlayRelease()
        {
            EnsureVisuals();
            isDragging = false;
            loadedStone.localPosition = restStoneLocalPosition;
            loadedStone.gameObject.SetActive(true);
            UpdateBands();
        }

        private void EnsureVisuals()
        {
            CreateMaterials();
            HideLegacyVisuals();
            SetupLaunchPoint();
            SetupBody();
            SetupArms();
            SetupLoadedStone();
            SetupBands();
            SetupDetails();
        }

        private void HideLegacyVisuals()
        {
            HideChildRenderer("SlingshotLeftArm");
            HideChildRenderer("SlingshotRightArm");
        }

        private void HideChildRenderer(string childName)
        {
            Transform child = transform.Find(childName);
            if (child == null)
            {
                return;
            }

            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            RemoveCollider(child.gameObject);
        }

        private void CreateMaterials()
        {
            if (woodMaterial != null)
            {
                return;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            woodMaterial = CreateMaterial(shader, "Runtime_SlingshotWood", new Color(0.62f, 0.36f, 0.17f), 0.30f);
            woodTipMaterial = CreateMaterial(shader, "Runtime_SlingshotWoodTips", new Color(0.72f, 0.46f, 0.23f), 0.34f);
            stoneMaterial = CreateMaterial(shader, "Runtime_LoadedStone", new Color(0.10f, 0.10f, 0.12f), 0.18f);
            bandMaterial = CreateMaterial(shader, "Runtime_SlingshotBand", new Color(0.028f, 0.032f, 0.035f), 0.12f);
            leatherMaterial = CreateMaterial(shader, "Runtime_SlingshotLeather", new Color(0.22f, 0.095f, 0.045f), 0.15f);
            metalMaterial = CreateMaterial(shader, "Runtime_SlingshotBindings", new Color(0.36f, 0.39f, 0.40f), 0.45f);
            if (metalMaterial.HasProperty("_Metallic")) metalMaterial.SetFloat("_Metallic", 0.75f);
            stitchMaterial = CreateMaterial(shader, "Runtime_SlingshotStitch", new Color(0.73f, 0.56f, 0.34f), 0.10f);
            CreateWoodGrain();
        }

        private void CreateWoodGrain()
        {
            // Small, deterministic texture shared by both wood materials; no external asset or per-frame work.
            const int width = 64;
            const int height = 128;
            woodGrain = new Texture2D(width, height, TextureFormat.RGB24, true);
            woodGrain.name = "Runtime_SlingshotWoodGrain";
            woodGrain.wrapMode = TextureWrapMode.Repeat;
            woodGrain.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = x / (float)width;
                    float v = y / (float)height;
                    float wave = u * 60f + Mathf.Sin(v * 12f + u * 4f) * 1.4f;
                    float grain = Mathf.Pow(Mathf.Abs(Mathf.Sin(wave)), 12f);
                    float tone = 0.91f - grain * 0.16f + Mathf.Sin(v * 18f + u * 8f) * 0.035f;
                    pixels[y * width + x] = new Color(tone, tone * 0.97f, tone * 0.91f);
                }
            }
            woodGrain.SetPixels(pixels);
            woodGrain.Apply(true, true);
            string property = woodMaterial.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex";
            woodMaterial.SetTexture(property, woodGrain);
            woodTipMaterial.SetTexture(property, woodGrain);
        }

        private void SetupDetails()
        {
            if (detailCreated || loadedStone == null) return;
            detailCreated = true;
            SetupBinding("LeftBinding", leftAnchorLocalPosition, leftAnchorLocalPosition - new Vector3(-0.1f, 0.72f, 0f));
            SetupBinding("RightBinding", rightAnchorLocalPosition, rightAnchorLocalPosition - new Vector3(0.1f, 0.72f, 0f));

            // Child of the animated stone: the pouch follows existing drag/release without moving the muzzle.
            Transform pouch = DetailCube("LeatherPouch", loadedStone, new Vector3(0f, -0.03f, -0.40f),
                new Vector3(1.12f, 0.72f, 0.20f), leatherMaterial);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 3; i++)
                    DetailCube("Stitch" + side + "_" + i, pouch, new Vector3(side * 0.4f, -0.27f + i * 0.26f, -0.54f),
                        new Vector3(0.065f, 0.08f, 0.06f), stitchMaterial);
            }
            // Thin grip inlays add variation without changing the wooden handle silhouette.
            for (int i = 0; i < 4; i++)
                DetailCube("GripInlay" + i, transform, new Vector3(0f, 0.20f + i * 0.09f, -0.14f),
                    new Vector3(0.13f, 0.018f, 0.012f), woodTipMaterial);
        }

        private void SetupBinding(string name, Vector3 anchor, Vector3 direction)
        {
            Transform binding = FindOrCreatePrimitive(name, PrimitiveType.Cylinder);
            binding.localPosition = anchor - direction.normalized * 0.055f;
            binding.localRotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            binding.localScale = new Vector3(0.29f, 0.045f, 0.29f);
            ApplyMaterial(binding.gameObject, metalMaterial);
            Collider collider = binding.GetComponent<Collider>();
            if (collider != null) { collider.enabled = false; Destroy(collider); }
        }

        private Transform DetailCube(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject detail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            detail.name = name;
            detail.transform.SetParent(parent, false);
            detail.transform.localPosition = position;
            detail.transform.localScale = scale;
            detail.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = detail.GetComponent<Collider>();
            collider.enabled = false;
            Destroy(collider);
            return detail.transform;
        }

        private void OnDestroy()
        {
            if (woodMaterial != null) Destroy(woodMaterial);
            if (woodTipMaterial != null) Destroy(woodTipMaterial);
            if (stoneMaterial != null) Destroy(stoneMaterial);
            if (bandMaterial != null) Destroy(bandMaterial);
            if (leatherMaterial != null) Destroy(leatherMaterial);
            if (metalMaterial != null) Destroy(metalMaterial);
            if (stitchMaterial != null) Destroy(stitchMaterial);
            if (woodGrain != null) Destroy(woodGrain);
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

        private void SetupLaunchPoint()
        {
            if (launchPoint == null)
            {
                launchPoint = transform.Find("LaunchPoint");
            }

            if (launchPoint != null)
            {
                launchPoint.localPosition = restStoneLocalPosition;
            }
        }

        private void SetupBody()
        {
            Transform body = FindOrCreatePrimitive("SlingshotBase", PrimitiveType.Capsule);
            body.localPosition = new Vector3(0f, 0.34f, 0f);
            body.localRotation = Quaternion.identity;
            body.localScale = new Vector3(0.28f, 0.48f, 0.28f);
            ApplyMaterial(body.gameObject, woodMaterial);
            RemoveCollider(body.gameObject);

            SetupSphere("CenterJoint", new Vector3(0f, 0.76f, 0f), 0.2f, woodTipMaterial);
        }

        private void SetupArms()
        {
            SetupCapsule("LeftArm", new Vector3(-0.1f, 0.72f, 0f), leftAnchorLocalPosition, 0.13f);
            SetupCapsule("RightArm", new Vector3(0.1f, 0.72f, 0f), rightAnchorLocalPosition, 0.13f);
            SetupSphere("LeftArmTip", leftAnchorLocalPosition, 0.17f, woodTipMaterial);
            SetupSphere("RightArmTip", rightAnchorLocalPosition, 0.17f, woodTipMaterial);
        }

        private void SetupCapsule(string objectName, Vector3 localStart, Vector3 localEnd, float radius)
        {
            Transform capsule = FindOrCreatePrimitive(objectName, PrimitiveType.Capsule);
            Vector3 direction = localEnd - localStart;
            capsule.localPosition = (localStart + localEnd) * 0.5f;
            capsule.localRotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            capsule.localScale = new Vector3(radius * 2f, direction.magnitude * 0.5f, radius * 2f);
            ApplyMaterial(capsule.gameObject, woodMaterial);
            RemoveCollider(capsule.gameObject);
        }

        private void SetupSphere(string objectName, Vector3 localPosition, float radius, Material material)
        {
            Transform sphere = FindOrCreatePrimitive(objectName, PrimitiveType.Sphere);
            sphere.localPosition = localPosition;
            sphere.localRotation = Quaternion.identity;
            sphere.localScale = Vector3.one * radius * 2f;
            ApplyMaterial(sphere.gameObject, material);
            RemoveCollider(sphere.gameObject);
        }

        private void SetupLoadedStone()
        {
            loadedStone = FindOrCreatePrimitive("LoadedStone", PrimitiveType.Sphere);
            loadedStone.localPosition = restStoneLocalPosition;
            loadedStone.localRotation = Quaternion.identity;
            loadedStone.localScale = Vector3.one * stoneRadius * 2f;
            ApplyMaterial(loadedStone.gameObject, stoneMaterial);
            RemoveCollider(loadedStone.gameObject);
        }

        private void SetupBands()
        {
            leftBand = FindOrCreateBand("LeftBand");
            rightBand = FindOrCreateBand("RightBand");
            UpdateBands();
        }

        private LineRenderer FindOrCreateBand(string objectName)
        {
            Transform bandTransform = transform.Find(objectName);
            if (bandTransform == null)
            {
                bandTransform = new GameObject(objectName).transform;
                bandTransform.SetParent(transform, false);
            }

            LineRenderer lineRenderer = bandTransform.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = bandTransform.gameObject.AddComponent<LineRenderer>();
            }

            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = bandWidth;
            lineRenderer.endWidth = bandWidth;
            lineRenderer.numCapVertices = 6;
            lineRenderer.sharedMaterial = bandMaterial;
            return lineRenderer;
        }

        private void UpdateBands()
        {
            if (loadedStone == null || leftBand == null || rightBand == null)
            {
                return;
            }

            Vector3 stonePosition = loadedStone.position;
            leftBand.SetPosition(0, transform.TransformPoint(leftAnchorLocalPosition));
            leftBand.SetPosition(1, stonePosition);
            rightBand.SetPosition(0, transform.TransformPoint(rightAnchorLocalPosition));
            rightBand.SetPosition(1, stonePosition);
        }

        private Transform FindOrCreatePrimitive(string objectName, PrimitiveType primitiveType)
        {
            Transform child = transform.Find(objectName);
            if (child != null)
            {
                if (HasExpectedPrimitiveMesh(child, primitiveType))
                {
                    return child;
                }

                child.name = objectName + "_HiddenOld";
                child.gameObject.SetActive(false);
            }

            child = GameObject.CreatePrimitive(primitiveType).transform;
            child.name = objectName;
            child.SetParent(transform, false);
            return child;
        }

        private bool HasExpectedPrimitiveMesh(Transform child, PrimitiveType primitiveType)
        {
            MeshFilter meshFilter = child.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                return false;
            }

            return meshFilter.sharedMesh.name.Contains(primitiveType.ToString());
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
