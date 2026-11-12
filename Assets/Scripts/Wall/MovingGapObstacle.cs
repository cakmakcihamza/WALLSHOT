using UnityEngine;
using UnityEngine.SceneManagement;

namespace WallBreaker.Wall
{
    public class MovingGapObstacle : LevelObstacle
    {
        [SerializeField, Range(1f, 4f)] private float openingWidth = 2.4f;
        [SerializeField, Range(0f, 2f)] private float travelDistance = 1.6f;
        [SerializeField, Range(0.05f, 0.5f)] private float cyclesPerSecond = 0.10f;
        private const float HalfWidth = 3.4f;
        [SerializeField] private float Bottom = 0.2f;
        [SerializeField] private float Height = 3.8f;
        [SerializeField] private Color metalColor = new Color(0.22f, 0.25f, 0.29f);
        private float frameThickness = 0.25f;
        private float panelDepth = 0.3f;
        private Rigidbody left;
        private Rigidbody right;
        private Material metal;
        private PhysicsMaterial bounce;
        private float phase;

        public static void EnsureMovingForWall(Transform wall)
        {
            foreach (MovingGapObstacle existing in FindObjectsByType<MovingGapObstacle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (existing.gameObject.scene == wall.gameObject.scene) return;
            GameObject root = new GameObject("SlingshotLevel8MovingGap");
            SceneManager.MoveGameObjectToScene(root, wall.gameObject.scene);
            root.transform.SetPositionAndRotation(wall.TransformPoint(new Vector3(0f, 0f, -2f)), wall.rotation);
            root.AddComponent<MovingGapObstacle>().Build();
        }

        public static void EnsureDesertEagleMovingForWall(Transform wall)
        {
            const string objectName = "DesertEagleLevel8MovingGap";
            foreach (MovingGapObstacle existing in FindObjectsByType<MovingGapObstacle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (existing.gameObject.scene == wall.gameObject.scene && existing.name == objectName) return;
            GameObject root = new GameObject(objectName);
            SceneManager.MoveGameObjectToScene(root, wall.gameObject.scene);
            root.transform.SetPositionAndRotation(wall.TransformPoint(new Vector3(0f, 0f, -2f)), wall.rotation);
            MovingGapObstacle obstacle = root.AddComponent<MovingGapObstacle>();
            obstacle.openingWidth = 1.8f;
            obstacle.travelDistance = 1.2f;
            obstacle.cyclesPerSecond = 1f / 7f;
            obstacle.Bottom = 0.25f;
            obstacle.Height = 2.9f;
            obstacle.metalColor = new Color(0.12f, 0.14f, 0.17f);
            obstacle.AllowsDesertEagleBounce = true;
            obstacle.Build();
        }

        public static void EnsureCannonMovingForWall(Transform wall)
        {
            const string objectName = "CannonLevel8MovingGap";
            foreach (MovingGapObstacle existing in FindObjectsByType<MovingGapObstacle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (existing.gameObject.scene == wall.gameObject.scene && existing.name == objectName) return;
            GameObject root = new GameObject(objectName);
            SceneManager.MoveGameObjectToScene(root, wall.gameObject.scene);
            root.transform.SetPositionAndRotation(wall.TransformPoint(new Vector3(0f, 0f, -2f)), wall.rotation);
            MovingGapObstacle obstacle = root.AddComponent<MovingGapObstacle>();
            obstacle.openingWidth = 3.2f;
            obstacle.travelDistance = 1.5f;
            obstacle.cyclesPerSecond = 1f / 9f;
            obstacle.Bottom = 0.1f;
            obstacle.Height = 3.6f;
            obstacle.frameThickness = 0.4f;
            obstacle.panelDepth = 0.4f;
            obstacle.metalColor = new Color(0.11f, 0.13f, 0.15f);
            obstacle.Build();
        }

        public static void EnsureRemainingMoving(Transform wall, bool bazooka)
        {
            string objectName = bazooka ? "BazookaLevel8Gap" : "CrossbowLevel8Gap";
            foreach (MovingGapObstacle existing in FindObjectsByType<MovingGapObstacle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (existing.gameObject.scene == wall.gameObject.scene && existing.name == objectName) return;
            GameObject root = new GameObject(objectName);
            SceneManager.MoveGameObjectToScene(root, wall.gameObject.scene);
            root.transform.SetPositionAndRotation(wall.TransformPoint(new Vector3(0f, 0f, -2f)), wall.rotation);
            MovingGapObstacle obstacle = root.AddComponent<MovingGapObstacle>();
            obstacle.openingWidth = bazooka ? 3.6f : 2.6f;
            obstacle.Height = bazooka ? 3.8f : 3.4f;
            obstacle.Bottom = 0.1f;
            obstacle.travelDistance = bazooka ? 1.4f : 1.5f;
            obstacle.cyclesPerSecond = 1f / (bazooka ? 10f : 8f);
            obstacle.frameThickness = obstacle.panelDepth = bazooka ? 0.45f : 0.3f;
            obstacle.metalColor = new Color(0.12f, 0.15f, 0.18f);
            obstacle.Build();
        }

        private void Build()
        {
            bounce = new PhysicsMaterial("Runtime_MovingGapBounce")
            {
                bounciness = 0.65f, bounceCombine = PhysicsMaterialCombine.Maximum,
                dynamicFriction = 0.1f, staticFriction = 0.1f, frictionCombine = PhysicsMaterialCombine.Minimum
            };
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null)
            {
                metal = new Material(shader) { name = "Runtime_MovingGapMetal" };
                metal.SetColor("_BaseColor", metalColor);
                metal.SetFloat("_Metallic", 0.65f);
                metal.SetFloat("_Smoothness", 0.3f);
            }
            Panel("FrameLeft", new Vector3(-HalfWidth - frameThickness * 0.5f, Bottom + Height * 0.5f, 0f), new Vector3(frameThickness, Height + frameThickness * 2f, panelDepth));
            Panel("FrameRight", new Vector3(HalfWidth + frameThickness * 0.5f, Bottom + Height * 0.5f, 0f), new Vector3(frameThickness, Height + frameThickness * 2f, panelDepth));
            Panel("FrameBottom", new Vector3(0f, Bottom - frameThickness * 0.5f, 0f), new Vector3(HalfWidth * 2f, frameThickness, panelDepth));
            Panel("FrameTop", new Vector3(0f, Bottom + Height + frameThickness * 0.5f, 0f), new Vector3(HalfWidth * 2f, frameThickness, panelDepth));
            left = MovingPanel("LeftShutter");
            right = MovingPanel("RightShutter");
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
            phase = Mathf.Repeat(phase + Mathf.Clamp(cyclesPerSecond, 0.05f, 0.5f) * Time.fixedDeltaTime, 1f);
            UpdatePanels(Mathf.Sin(phase * Mathf.PI * 2f), false);
        }

        private void UpdatePanels(float oscillation, bool initial)
        {
            float gap = Mathf.Clamp(openingWidth, 1f, 4f);
            float travel = Mathf.Clamp(travelDistance, 0f, HalfWidth - gap * 0.5f - 0.2f);
            float center = oscillation * travel;
            float leftEdge = center - gap * 0.5f;
            float rightEdge = center + gap * 0.5f;
            // Telescoping panels exactly fill [-HalfWidth,leftEdge] and [rightEdge,HalfWidth].
            // Visual geometry and BoxCollider share the same scale; no hidden central collider.
            SetPanel(left, (leftEdge - HalfWidth) * 0.5f, leftEdge + HalfWidth, initial);
            SetPanel(right, (rightEdge + HalfWidth) * 0.5f, HalfWidth - rightEdge, initial);
        }

        private void SetPanel(Rigidbody body, float x, float width, bool initial)
        {
            body.transform.localScale = new Vector3(width, Height, panelDepth);
            Vector3 position = transform.TransformPoint(new Vector3(x, Bottom + Height * 0.5f, 0f));
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
