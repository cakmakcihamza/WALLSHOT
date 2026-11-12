using UnityEngine;
using UnityEngine.SceneManagement;

namespace WallBreaker.Wall
{
    public class VerticalMovingGapObstacle : LevelObstacle
    {
        [SerializeField, Range(3f, 6.4f)] private float openingWidth = 5.8f;
        [SerializeField, Range(0.6f, 3f)] private float openingHeight = 1.7f;
        [SerializeField, Range(0f, 2f)] private float travelDistance = 0.9f;
        [SerializeField, Range(4f, 20f)] private float cycleDuration = 8f;
        private const float HalfWidth = 3.4f;
        private float Bottom = 0f;
        private const float Height = 4f;
        private float frameThickness = 0.25f;
        private float panelDepth = 0.3f;
        private Rigidbody left;
        private Rigidbody right;
        private Material metal;
        private PhysicsMaterial bounce;
        private float phase;
        [SerializeField] private Color metalColor = new Color(0.22f, 0.25f, 0.29f);

        public static void EnsureVerticalForWall(Transform wall)
        {
            foreach (VerticalMovingGapObstacle existing in FindObjectsByType<VerticalMovingGapObstacle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (existing.gameObject.scene == wall.gameObject.scene) return;
            GameObject root = new GameObject("SlingshotLevel9VerticalGap");
            SceneManager.MoveGameObjectToScene(root, wall.gameObject.scene);
            root.transform.SetPositionAndRotation(wall.TransformPoint(new Vector3(0f, 0f, -2f)), wall.rotation);
            root.AddComponent<VerticalMovingGapObstacle>().Build();
        }

        public static void EnsureDesertEagleVerticalForWall(Transform wall)
        {
            const string objectName = "DesertEagleLevel9VerticalGap";
            foreach (VerticalMovingGapObstacle existing in FindObjectsByType<VerticalMovingGapObstacle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (existing.gameObject.scene == wall.gameObject.scene && existing.name == objectName) return;
            GameObject root = new GameObject(objectName);
            SceneManager.MoveGameObjectToScene(root, wall.gameObject.scene);
            root.transform.SetPositionAndRotation(wall.TransformPoint(new Vector3(0f, 0f, -2f)), wall.rotation);
            VerticalMovingGapObstacle obstacle = root.AddComponent<VerticalMovingGapObstacle>();
            obstacle.openingWidth = 4f;
            obstacle.openingHeight = 1.3f;
            obstacle.travelDistance = 1.1f;
            obstacle.cycleDuration = 6f;
            obstacle.metalColor = new Color(0.12f, 0.14f, 0.17f);
            obstacle.AllowsDesertEagleBounce = true;
            obstacle.Build();
        }

        public static void EnsureCannonVerticalForWall(Transform wall)
        {
            const string objectName = "CannonLevel9VerticalGap";
            foreach (VerticalMovingGapObstacle existing in FindObjectsByType<VerticalMovingGapObstacle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (existing.gameObject.scene == wall.gameObject.scene && existing.name == objectName) return;
            GameObject root = new GameObject(objectName);
            SceneManager.MoveGameObjectToScene(root, wall.gameObject.scene);
            root.transform.SetPositionAndRotation(wall.TransformPoint(new Vector3(0f, 0f, -2f)), wall.rotation);
            VerticalMovingGapObstacle obstacle = root.AddComponent<VerticalMovingGapObstacle>();
            obstacle.openingWidth = 6.2f;
            obstacle.openingHeight = 1.8f;
            obstacle.travelDistance = 0.8f;
            obstacle.cycleDuration = 9f;
            obstacle.Bottom = -0.1f;
            obstacle.frameThickness = 0.4f;
            obstacle.panelDepth = 0.4f;
            obstacle.metalColor = new Color(0.11f, 0.13f, 0.15f);
            obstacle.Build();
        }

        public static void EnsureRemainingVertical(Transform wall, bool bazooka)
        {
            string objectName = bazooka ? "BazookaLevel9Gap" : "CrossbowLevel9Gap";
            foreach (VerticalMovingGapObstacle existing in FindObjectsByType<VerticalMovingGapObstacle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (existing.gameObject.scene == wall.gameObject.scene && existing.name == objectName) return;
            GameObject root = new GameObject(objectName);
            SceneManager.MoveGameObjectToScene(root, wall.gameObject.scene);
            root.transform.SetPositionAndRotation(wall.TransformPoint(new Vector3(0f, 0f, -2f)), wall.rotation);
            VerticalMovingGapObstacle obstacle = root.AddComponent<VerticalMovingGapObstacle>();
            obstacle.openingWidth = bazooka ? 6.4f : 5.4f;
            obstacle.openingHeight = bazooka ? 2f : 1.6f;
            obstacle.Bottom = -0.1f;
            obstacle.travelDistance = bazooka ? 0.8f : 0.9f;
            obstacle.cycleDuration = bazooka ? 10f : 7f;
            obstacle.frameThickness = obstacle.panelDepth = bazooka ? 0.45f : 0.3f;
            obstacle.metalColor = new Color(0.12f, 0.15f, 0.18f);
            obstacle.Build();
        }

        private void Build()
        {
            bounce = new PhysicsMaterial("Runtime_VerticalGapBounce")
            {
                bounciness = 0.65f, bounceCombine = PhysicsMaterialCombine.Maximum,
                dynamicFriction = 0.1f, staticFriction = 0.1f, frictionCombine = PhysicsMaterialCombine.Minimum
            };
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null)
            {
                metal = new Material(shader) { name = "Runtime_VerticalGapMetal" };
                metal.SetColor("_BaseColor", metalColor);
                metal.SetFloat("_Metallic", 0.65f);
                metal.SetFloat("_Smoothness", 0.3f);
            }
            Panel("FrameLeft", new Vector3(-3.525f, 2f, 0f), new Vector3(0.25f, 4.5f, 0.3f));
            Panel("FrameRight", new Vector3(3.525f, 2f, 0f), new Vector3(0.25f, 4.5f, 0.3f));
            Panel("FrameBottom", new Vector3(0f, Bottom - frameThickness * 0.5f, 0f), new Vector3(6.8f, frameThickness, panelDepth));
            Panel("FrameTop", new Vector3(0f, Bottom + Height + frameThickness * 0.5f, 0f), new Vector3(6.8f, frameThickness, panelDepth));
            left = MovingPanel("BottomShutter");
            right = MovingPanel("TopShutter");
            UpdatePanels(0f, true);
        }

        private GameObject Panel(string name, Vector3 position, Vector3 size)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = name;
            panel.transform.SetParent(transform, false);
            panel.transform.localPosition = position;
            panel.transform.localScale = size;
            BoxCollider collider = panel.GetComponent<BoxCollider>();
            collider.isTrigger = false;
            collider.sharedMaterial = bounce;
            if (metal != null) panel.GetComponent<Renderer>().sharedMaterial = metal;
            return panel;
        }

        private Rigidbody MovingPanel(string name)
        {
            Rigidbody body = Panel(name, Vector3.zero, Vector3.one).AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            // Do not interpolate different collider widths against a different visual center.
            body.interpolation = RigidbodyInterpolation.None;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            return body;
        }

        private void FixedUpdate()
        {
            if (left == null || right == null) return;
            phase = Mathf.Repeat(phase + (1f / Mathf.Clamp(cycleDuration, 4f, 20f)) * Time.fixedDeltaTime, 1f);
            UpdatePanels(Mathf.Sin(phase * Mathf.PI * 2f), false);
        }

        private void UpdatePanels(float oscillation, bool initial)
        {
            float gapHeight = Mathf.Clamp(openingHeight, 0.6f, 3f);
            float gapWidth = Mathf.Clamp(openingWidth, 3f, 6.4f);
            float travel = Mathf.Clamp(travelDistance, 0f, Height * 0.5f - gapHeight * 0.5f - 0.15f);
            float center = Bottom + Height * 0.5f + oscillation * travel;
            float lowerEdge = center - gapHeight * 0.5f;
            float upperEdge = center + gapHeight * 0.5f;
            // Fixed side infills close the space outside the adjustable aperture width.
            UpdateSide("FrameLeft", -1f, gapWidth);
            UpdateSide("FrameRight", 1f, gapWidth);
            SetPanel(left, (Bottom + lowerEdge) * 0.5f, lowerEdge - Bottom, initial);
            SetPanel(right, (Bottom + Height + upperEdge) * 0.5f, Bottom + Height - upperEdge, initial);
        }

        private void UpdateSide(string name, float sign, float gapWidth)
        {
            Transform side = transform.Find(name);
            float outer = HalfWidth + frameThickness;
            float inner = gapWidth * 0.5f;
            side.localPosition = new Vector3(sign * (outer + inner) * 0.5f, Bottom + Height * 0.5f, 0f);
            side.localScale = new Vector3(outer - inner, Height + frameThickness * 2f, panelDepth);
        }

        private void SetPanel(Rigidbody body, float y, float height, bool initial)
        {
            body.transform.localScale = new Vector3(Mathf.Clamp(openingWidth, 3f, 6.4f), height, panelDepth);
            Vector3 position = transform.TransformPoint(new Vector3(0f, y, 0f));
            if (initial) body.position = position;
            else body.MovePosition(position);
        }
        private void OnDestroy()
        {
            if (metal != null) Destroy(metal);
            if (bounce != null) Destroy(bounce);
        }
    }
}
