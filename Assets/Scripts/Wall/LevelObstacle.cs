using UnityEngine;
using UnityEngine.SceneManagement;

namespace WallBreaker.Wall
{
    public class LevelObstacle : MonoBehaviour
    {
        private Material panelMaterial;
        private PhysicsMaterial bounceMaterial;
        private bool cannonFrame;
        private Vector2 configuredOpening;
        private float configuredRim;

        public static void EnsureRemainingFrame(Transform wall, bool bazooka)
        {
            string objectName = bazooka ? "BazookaLevel6Frame" : "CrossbowLevel6Frame";
            foreach (LevelObstacle existing in FindObjectsByType<LevelObstacle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (existing.gameObject.scene == wall.gameObject.scene && existing.name == objectName) return;
            GameObject root = new GameObject(objectName);
            SceneManager.MoveGameObjectToScene(root, wall.gameObject.scene);
            root.transform.SetPositionAndRotation(wall.TransformPoint(new Vector3(0f, 0f, -2f)), wall.rotation);
            LevelObstacle obstacle = root.AddComponent<LevelObstacle>();
            obstacle.configuredOpening = bazooka ? new Vector2(6.4f, 3.6f) : new Vector2(5.4f, 3.2f);
            obstacle.configuredRim = bazooka ? 0.45f : 0.3f;
            obstacle.Build();
        }
        public bool AllowsDesertEagleBounce { get; protected set; }

        public static void EnsureDesertEagleForWall(Transform wall)
        {
            foreach (LevelObstacle existing in FindObjectsByType<LevelObstacle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (existing.gameObject.scene == wall.gameObject.scene && existing.AllowsDesertEagleBounce) return;
            GameObject root = new GameObject("DesertEagleLevel6Obstacle");
            SceneManager.MoveGameObjectToScene(root, wall.gameObject.scene);
            root.transform.SetPositionAndRotation(wall.TransformPoint(new Vector3(0f, 0f, -2f)), wall.rotation);
            LevelObstacle obstacle = root.AddComponent<LevelObstacle>();
            obstacle.AllowsDesertEagleBounce = true;
            obstacle.Build();
        }

        public static void EnsureCannonForWall(Transform wall)
        {
            const string objectName = "CannonLevel6Obstacle";
            foreach (LevelObstacle existing in FindObjectsByType<LevelObstacle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (existing.gameObject.scene == wall.gameObject.scene && existing.name == objectName) return;
            GameObject root = new GameObject(objectName);
            SceneManager.MoveGameObjectToScene(root, wall.gameObject.scene);
            root.transform.SetPositionAndRotation(wall.TransformPoint(new Vector3(0f, 0f, -2f)), wall.rotation);
            LevelObstacle obstacle = root.AddComponent<LevelObstacle>();
            obstacle.cannonFrame = true;
            obstacle.Build();
        }

        public static void EnsureForWall(Transform wall)
        {
            // Search inactive objects too; repeated creation calls must not duplicate the obstacle.
            foreach (LevelObstacle existing in FindObjectsByType<LevelObstacle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (existing.gameObject.scene == wall.gameObject.scene) return;

            GameObject root = new GameObject("SlingshotLevel6Obstacle");
            SceneManager.MoveGameObjectToScene(root, wall.gameObject.scene);
            root.transform.SetPositionAndRotation(wall.TransformPoint(new Vector3(0f, 0f, -2f)), wall.rotation);
            root.AddComponent<LevelObstacle>().Build();
        }

        private void Build()
        {
            bounceMaterial = new PhysicsMaterial("Runtime_ObstacleBounce")
            {
                bounciness = 0.65f,
                bounceCombine = PhysicsMaterialCombine.Maximum,
                dynamicFriction = 0.1f,
                staticFriction = 0.1f,
                frictionCombine = PhysicsMaterialCombine.Minimum
            };
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null)
            {
                panelMaterial = new Material(shader) { name = "Runtime_LevelObstacle" };
                panelMaterial.SetColor("_BaseColor", new Color(0.15f, 0.18f, 0.21f));
                panelMaterial.SetFloat("_Metallic", 0.65f);
                panelMaterial.SetFloat("_Smoothness", 0.3f);
            }

            // Opening 6.0 x 3.8: x=[-3,3], y=[0.2,4.0].
            // The raised aperture accounts for the elevated camera looking down through z=3.
            // A 0.25-unit metal frame still blocks grazing shots; its center has no collider.
            if (configuredRim > 0f)
            {
                float w = configuredOpening.x;
                float h = configuredOpening.y;
                float t = configuredRim;
                const float bottom = 0.1f;
                Panel("LeftPanel", new Vector3(-(w + t) * 0.5f, bottom + h * 0.5f, 0f), new Vector3(t, h + 2f * t, t));
                Panel("RightPanel", new Vector3((w + t) * 0.5f, bottom + h * 0.5f, 0f), new Vector3(t, h + 2f * t, t));
                Panel("BottomPanel", new Vector3(0f, bottom - t * 0.5f, 0f), new Vector3(w, t, t));
                Panel("TopPanel", new Vector3(0f, bottom + h + t * 0.5f, 0f), new Vector3(w, t, t));
                return;
            }
            if (cannonFrame)
            {
                // Wide aperture for the gravity-driven cannonball, radius 0.125.
                // Opening x=[-3.1,3.1], y=[0.1,3.5]; heavy 0.4-unit rim.
                Panel("LeftPanel", new Vector3(-3.3f, 1.8f, 0f), new Vector3(0.4f, 4.2f, 0.4f));
                Panel("RightPanel", new Vector3(3.3f, 1.8f, 0f), new Vector3(0.4f, 4.2f, 0.4f));
                Panel("BottomPanel", new Vector3(0f, -0.1f, 0f), new Vector3(6.2f, 0.4f, 0.4f));
                Panel("TopPanel", new Vector3(0f, 3.7f, 0f), new Vector3(6.2f, 0.4f, 0.4f));
                return;
            }
            if (AllowsDesertEagleBounce)
            {
                // Gravity-free muzzle rays to the three columns pass through this aperture.
                // Opening x=[-2,2], y=[0.25,2.75], with a 0.18-unit rim.
                Panel("LeftPanel", new Vector3(-2.09f, 1.5f, 0f), new Vector3(0.18f, 2.86f, 0.2f));
                Panel("RightPanel", new Vector3(2.09f, 1.5f, 0f), new Vector3(0.18f, 2.86f, 0.2f));
                Panel("BottomPanel", new Vector3(0f, 0.16f, 0f), new Vector3(4f, 0.18f, 0.2f));
                Panel("TopPanel", new Vector3(0f, 2.84f, 0f), new Vector3(4f, 0.18f, 0.2f));
                return;
            }
            Panel("LeftPanel", new Vector3(-3.125f, 2.1f, 0f), new Vector3(0.25f, 4.3f, 0.2f));
            Panel("RightPanel", new Vector3(3.125f, 2.1f, 0f), new Vector3(0.25f, 4.3f, 0.2f));
            Panel("BottomPanel", new Vector3(0f, 0.075f, 0f), new Vector3(6f, 0.25f, 0.2f));
            Panel("TopPanel", new Vector3(0f, 4.125f, 0f), new Vector3(6f, 0.25f, 0.2f));
        }

        private void Panel(string panelName, Vector3 position, Vector3 size)
        {
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = panelName;
            panel.transform.SetParent(transform, false);
            panel.transform.localPosition = position;
            panel.transform.localScale = size;
            panel.GetComponent<BoxCollider>().isTrigger = false;
            panel.GetComponent<BoxCollider>().sharedMaterial = bounceMaterial;
            Renderer renderer = panel.GetComponent<Renderer>();
            if (panelMaterial != null) renderer.sharedMaterial = panelMaterial;
            // Slightly lighter end panels make the rectangular aperture easy to distinguish.
            MaterialPropertyBlock properties = new MaterialPropertyBlock();
            properties.SetColor("_BaseColor", panelName == "TopPanel" || panelName == "BottomPanel"
                ? new Color(0.23f, 0.26f, 0.29f) : new Color(0.15f, 0.18f, 0.21f));
            if (AllowsDesertEagleBounce)
                properties.SetColor("_BaseColor", new Color(0.10f, 0.12f, 0.15f));
            if (cannonFrame)
                properties.SetColor("_BaseColor", new Color(0.11f, 0.13f, 0.15f));
            renderer.SetPropertyBlock(properties);
        }

        private void OnDestroy()
        {
            if (panelMaterial != null) Destroy(panelMaterial);
            if (bounceMaterial != null) Destroy(bounceMaterial);
        }
    }
}
