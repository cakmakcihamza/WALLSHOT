using UnityEngine;
using UnityEngine.SceneManagement;

namespace WallBreaker.Wall
{
    // Inherits the obstacle marker used by Projectile to leave collision response to physics.
    public class RotatingObstacle : LevelObstacle
    {
        [SerializeField, Range(-60f, 60f)] private float degreesPerSecond = 15f;
        [SerializeField] private Color metalColor = new Color(0.20f, 0.23f, 0.27f);
        [SerializeField, Range(0.5f, 3f)] private float armLength = 1.6f;
        [SerializeField, Range(0.1f, 0.5f)] private float armWidth = 0.18f;
        private float hubDiameter = 0.44f;
        private float armInnerOffset = 0.16f;
        private float armDepth = 0.3f;

        public static void EnsureRemainingRotors(Transform wall, bool bazooka, bool final)
        {
            int count = final ? 2 : 1;
            for (int i = 0; i < count; i++)
            {
                string objectName = (bazooka ? "Bazooka" : "Crossbow") + (final ? "Level10Rotor" : "Level7Rotor") + i;
                bool exists = false;
                foreach (RotatingObstacle existing in FindObjectsByType<RotatingObstacle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    if (existing.gameObject.scene == wall.gameObject.scene && existing.name == objectName) exists = true;
                if (exists) continue;
                GameObject root = new GameObject(objectName);
                SceneManager.MoveGameObjectToScene(root, wall.gameObject.scene);
                float z = final ? (i == 0 ? -2.5f : -1.7f) : -2f;
                float start = i == 0 ? 0f : (bazooka ? 30f : 25f);
                root.transform.SetPositionAndRotation(wall.TransformPoint(new Vector3(0f, 1.4f, z)), wall.rotation * Quaternion.Euler(0f, 0f, start));
                RotatingObstacle rotor = root.AddComponent<RotatingObstacle>();
                rotor.degreesPerSecond = final ? (i == 0 ? (bazooka ? -14f : -25f) : (bazooka ? 10f : 18f)) : (bazooka ? 10f : 22f);
                rotor.armLength = bazooka ? (final ? 1.7f : 1.8f) : (final ? 1.4f : 1.5f);
                rotor.armWidth = bazooka ? (final ? 0.22f : 0.25f) : (final ? 0.15f : 0.16f);
                rotor.hubDiameter = bazooka ? (final ? 0.32f : 0.4f) : (final ? 0.22f : 0.24f);
                rotor.armInnerOffset = rotor.hubDiameter * 0.35f;
                rotor.armDepth = bazooka ? 0.35f : 0.3f;
                rotor.metalColor = i == 0 ? new Color(0.12f, 0.15f, 0.18f) : new Color(0.27f, 0.30f, 0.33f);
                rotor.BuildRotor();
            }
        }
        private Rigidbody rotorBody;
        private Material metalMaterial;
        private PhysicsMaterial metalPhysics;
        private Quaternion initialRotation;
        private float angle;
        private bool built;

        public static void EnsureRotatingForWall(Transform wall)
        {
            foreach (RotatingObstacle existing in FindObjectsByType<RotatingObstacle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (existing.gameObject.scene == wall.gameObject.scene) return;

            GameObject root = new GameObject("SlingshotLevel7Rotor");
            SceneManager.MoveGameObjectToScene(root, wall.gameObject.scene);
            root.transform.SetPositionAndRotation(wall.TransformPoint(new Vector3(0f, 1.8f, -2f)), wall.rotation);
            root.AddComponent<RotatingObstacle>().BuildRotor();
        }

        public static void EnsureDesertEagleRotorForWall(Transform wall)
        {
            const string objectName = "DesertEagleLevel7Rotor";
            foreach (RotatingObstacle existing in FindObjectsByType<RotatingObstacle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (existing.gameObject.scene == wall.gameObject.scene && existing.name == objectName) return;

            GameObject root = new GameObject(objectName);
            SceneManager.MoveGameObjectToScene(root, wall.gameObject.scene);
            // The central passage has no low central bricks: keep the permanent hub in that passage.
            root.transform.SetPositionAndRotation(wall.TransformPoint(new Vector3(0f, 1.2f, -2f)), wall.rotation);
            RotatingObstacle rotor = root.AddComponent<RotatingObstacle>();
            rotor.degreesPerSecond = 30f;
            rotor.armLength = 1.4f;
            rotor.armWidth = 0.16f;
            rotor.metalColor = new Color(0.12f, 0.14f, 0.17f);
            rotor.AllowsDesertEagleBounce = true;
            rotor.BuildRotor();
        }

        public static void EnsureCannonRotorForWall(Transform wall)
        {
            const string objectName = "CannonLevel7Rotor";
            foreach (RotatingObstacle existing in FindObjectsByType<RotatingObstacle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (existing.gameObject.scene == wall.gameObject.scene && existing.name == objectName) return;

            GameObject root = new GameObject(objectName);
            SceneManager.MoveGameObjectToScene(root, wall.gameObject.scene);
            // Low central gate leaves room for the permanent hub below the upper keep.
            root.transform.SetPositionAndRotation(wall.TransformPoint(new Vector3(0f, 1.25f, -2f)), wall.rotation);
            RotatingObstacle rotor = root.AddComponent<RotatingObstacle>();
            rotor.degreesPerSecond = 12f;
            rotor.armLength = 1.7f;
            rotor.armWidth = 0.24f;
            rotor.hubDiameter = 0.36f;
            rotor.metalColor = new Color(0.11f, 0.13f, 0.15f);
            rotor.BuildRotor();
        }

        public static void EnsureFinalRotorsForWall(Transform wall)
        {
            EnsureFinalRotor(wall, "SlingshotLevel10FrontRotor", -2.5f, -18f, 0f,
                new Color(0.20f, 0.23f, 0.27f));
            EnsureFinalRotor(wall, "SlingshotLevel10RearRotor", -1.7f, 12f, 25f,
                new Color(0.32f, 0.35f, 0.39f));
        }

        public static void EnsureDesertEagleFinalRotorsForWall(Transform wall)
        {
            EnsureFinalRotor(wall, "DesertEagleLevel10FrontRotor", -2.5f, -36f, 0f,
                new Color(0.12f, 0.14f, 0.17f), true);
            EnsureFinalRotor(wall, "DesertEagleLevel10RearRotor", -1.7f, 25f, 30f,
                new Color(0.28f, 0.31f, 0.35f), true);
        }

        public static void EnsureCannonFinalRotorsForWall(Transform wall)
        {
            EnsureFinalRotor(wall, "CannonLevel10FrontRotor", -2.5f, -15f, 0f,
                new Color(0.11f, 0.13f, 0.15f), cannon: true);
            EnsureFinalRotor(wall, "CannonLevel10RearRotor", -1.7f, 10f, 25f,
                new Color(0.25f, 0.28f, 0.31f), cannon: true);
        }

        private static void EnsureFinalRotor(Transform wall, string objectName, float localZ, float speed,
            float startAngle, Color color, bool desertEagle = false, bool cannon = false)
        {
            foreach (RotatingObstacle existing in FindObjectsByType<RotatingObstacle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (existing.gameObject.scene == wall.gameObject.scene && existing.name == objectName) return;

            GameObject root = new GameObject(objectName);
            SceneManager.MoveGameObjectToScene(root, wall.gameObject.scene);
            root.transform.SetPositionAndRotation(wall.TransformPoint(new Vector3(0f, cannon ? 1.5f : 1.8f, localZ)),
                wall.rotation * Quaternion.Euler(0f, 0f, startAngle));
            RotatingObstacle rotor = root.AddComponent<RotatingObstacle>();
            rotor.degreesPerSecond = speed;
            rotor.armLength = 1.4f;
            rotor.armWidth = 0.16f;
            rotor.metalColor = color;
            if (desertEagle)
            {
                rotor.armLength = 1.25f;
                rotor.armWidth = 0.12f;
                rotor.hubDiameter = 0.16f;
                rotor.AllowsDesertEagleBounce = true;
            }
            if (cannon)
            {
                rotor.armLength = 1.6f;
                rotor.armWidth = 0.22f;
                rotor.hubDiameter = 0.3f;
                rotor.armInnerOffset = 0.12f;
            }
            rotor.BuildRotor();
        }

        private void BuildRotor()
        {
            if (built) return;
            built = true;
            initialRotation = transform.rotation;
            metalPhysics = new PhysicsMaterial("Runtime_RotorBounce")
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
                metalMaterial = new Material(shader) { name = "Runtime_RotorMetal" };
                metalMaterial.SetColor("_BaseColor", metalColor);
                metalMaterial.SetFloat("_Metallic", 0.65f);
                metalMaterial.SetFloat("_Smoothness", 0.3f);
            }

            // Cylinder's native axis is Y; rotate it so the hub faces along Z.
            Part("Hub", PrimitiveType.Cylinder, Vector3.zero, new Vector3(hubDiameter, 0.18f, hubDiameter),
                Quaternion.Euler(90f, 0f, 0f));
            float length = Mathf.Clamp(armLength, 0.5f, 3f);
            float width = Mathf.Clamp(armWidth, 0.1f, 0.5f);
            for (int i = 0; i < 4; i++)
            {
                Quaternion rotation = Quaternion.Euler(0f, 0f, i * 90f);
                // Slightly overlap the hub; the quadrants between arms remain empty.
                float innerOffset = AllowsDesertEagleBounce && hubDiameter < 0.44f ? hubDiameter * 0.35f : armInnerOffset;
                Part("Arm_" + i, PrimitiveType.Cube, rotation * new Vector3(innerOffset + length * 0.5f, 0f, 0f),
                    new Vector3(length, width, armDepth), rotation);
            }
            rotorBody = gameObject.AddComponent<Rigidbody>();
            rotorBody.isKinematic = true;
            rotorBody.useGravity = false;
            rotorBody.interpolation = RigidbodyInterpolation.Interpolate;
            // Speculative CCD accounts for angular motion as well as translation.
            rotorBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        private void Part(string partName, PrimitiveType type, Vector3 position, Vector3 scale, Quaternion rotation)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = partName;
            part.transform.SetParent(transform, false);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation;
            part.transform.localScale = scale;
            Collider collider = part.GetComponent<Collider>();
            collider.isTrigger = false;
            collider.sharedMaterial = metalPhysics;
            if (metalMaterial != null) part.GetComponent<Renderer>().sharedMaterial = metalMaterial;
        }

        private void FixedUpdate()
        {
            if (rotorBody == null) return;
            angle = Mathf.Repeat(angle + degreesPerSecond * Time.fixedDeltaTime, 360f);
            rotorBody.MoveRotation(initialRotation * Quaternion.AngleAxis(angle, Vector3.forward));
        }

        private void OnDestroy()
        {
            if (metalMaterial != null) Destroy(metalMaterial);
            if (metalPhysics != null) Destroy(metalPhysics);
        }
    }
}
