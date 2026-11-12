using UnityEngine;

namespace WallBreaker.Visuals
{
    public class DesertEagleVisual : MonoBehaviour
    {
        private Transform aimPivot;
        private Transform recoilPivot;
        private Transform launchPoint;
        private Material bodyMaterial;
        private Material steelMaterial;
        private Material gripMaterial;
        private Material openingMaterial;
        private bool visualsCreated;
        private readonly System.Collections.Generic.List<Mesh> detailMeshes = new System.Collections.Generic.List<Mesh>();
        private Texture2D gripTexture;

        public Transform LaunchPoint
        {
            get { EnsureVisuals(); return launchPoint; }
        }

        // Reserved for future recoil animation; all model parts and the muzzle follow it.
        public Transform RecoilPivot
        {
            get { EnsureVisuals(); return recoilPivot; }
        }

        private void Awake()
        {
            EnsureVisuals();
        }

        public void EnsureVisuals()
        {
            if (visualsCreated) return;
            EnsurePivots();
            if (!CreateMaterials()) return;

            Part("Frame", new Vector3(0f, -0.06f, 0.12f), new Vector3(0.32f, 0.20f, 0.96f), bodyMaterial);
            Part("Slide", new Vector3(0f, 0.13f, 0.15f), new Vector3(0.36f, 0.28f, 1.12f), bodyMaterial);
            Part("SlideTop", new Vector3(0f, 0.285f, 0.18f), new Vector3(0.24f, 0.045f, 0.95f), steelMaterial);
            Part("BarrelShroud", new Vector3(0f, 0.10f, 0.78f), new Vector3(0.30f, 0.25f, 0.30f), steelMaterial);
            Part("MuzzleOpening", new Vector3(0f, 0.12f, 0.936f), new Vector3(0.15f, 0.13f, 0.015f), openingMaterial);
            Part("GripCore", new Vector3(0f, -0.39f, -0.22f), new Vector3(0.28f, 0.58f, 0.34f), bodyMaterial,
                Quaternion.Euler(-16f, 0f, 0f));
            Part("MagazineBase", new Vector3(0f, -0.68f, -0.30f), new Vector3(0.34f, 0.07f, 0.40f), steelMaterial);

            // Three bars form an open trigger guard rather than a solid block.
            Part("GuardFront", new Vector3(0f, -0.24f, 0.36f), new Vector3(0.14f, 0.29f, 0.07f), bodyMaterial);
            Part("GuardBottom", new Vector3(0f, -0.38f, 0.15f), new Vector3(0.14f, 0.065f, 0.47f), bodyMaterial);
            Part("GuardRear", new Vector3(0f, -0.25f, -0.065f), new Vector3(0.14f, 0.22f, 0.06f), bodyMaterial);
            Part("Trigger", new Vector3(0f, -0.22f, 0.12f), new Vector3(0.065f, 0.19f, 0.055f), steelMaterial,
                Quaternion.Euler(-20f, 0f, 0f));
            Part("FrontSight", new Vector3(0f, 0.33f, 0.78f), new Vector3(0.065f, 0.08f, 0.10f), gripMaterial);
            Part("RearSight", new Vector3(0f, 0.32f, -0.32f), new Vector3(0.27f, 0.07f, 0.08f), gripMaterial);
            Part("Hammer", new Vector3(0f, 0.12f, -0.46f), new Vector3(0.12f, 0.17f, 0.10f), steelMaterial);
            Part("EjectionPort", new Vector3(0.183f, 0.15f, 0.12f), new Vector3(0.012f, 0.10f, 0.22f), openingMaterial);
            for (int side = -1; side <= 1; side += 2)
            {
                string suffix = side < 0 ? "Left" : "Right";
                Part("GripPanel" + suffix, new Vector3(side * 0.15f, -0.40f, -0.22f),
                    new Vector3(0.035f, 0.43f, 0.27f), gripMaterial, Quaternion.Euler(-16f, 0f, 0f));
                Part("Safety" + suffix, new Vector3(side * 0.20f, 0.06f, -0.26f),
                    new Vector3(0.04f, 0.045f, 0.16f), steelMaterial);
                for (int i = 0; i < 4; i++)
                    Part("SlideSerration" + suffix + i, new Vector3(side * 0.184f, 0.14f, -0.29f + i * 0.055f),
                        new Vector3(0.012f, 0.19f, 0.018f), gripMaterial);
            }

            SetupFinishDetails();
            launchPoint = FindOrCreateTransform(recoilPivot, "DesertEagleLaunchPoint");
            launchPoint.localPosition = new Vector3(0f, 0.12f, 0.97f);
            launchPoint.localRotation = Quaternion.identity; // Local +Z points out of the barrel.
            visualsCreated = true;
        }

        public void SetAimVisual(Vector3 worldDirection)
        {
            EnsureVisuals();
            if (worldDirection.sqrMagnitude <= 0.0001f) return;
            Vector3 localDirection = transform.InverseTransformDirection(worldDirection.normalized);
            aimPivot.localRotation = Quaternion.FromToRotation(Vector3.forward, localDirection.normalized);
        }

        private void EnsurePivots()
        {
            if (aimPivot == null)
            {
                aimPivot = FindOrCreateTransform(transform, "DesertEagleAimPivot");
                aimPivot.localPosition = new Vector3(0f, 0.78f, 0.15f);
                aimPivot.localRotation = Quaternion.FromToRotation(Vector3.forward, new Vector3(0f, 0.22f, 1f).normalized);
            }
            if (recoilPivot == null) recoilPivot = FindOrCreateTransform(aimPivot, "DesertEagleRecoilPivot");
        }

        private Transform FindOrCreateTransform(Transform parent, string objectName)
        {
            Transform child = parent.Find(objectName);
            if (child != null) return child;
            child = new GameObject(objectName).transform;
            child.SetParent(parent, false);
            return child;
        }

        private bool CreateMaterials()
        {
            if (bodyMaterial != null) return true;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogWarning("Desert Eagle visuals require the URP Lit shader.", this);
                return false;
            }
            bodyMaterial = CreateMaterial(shader, "Body", new Color(0.10f, 0.12f, 0.15f), 0.8f, 0.4f);
            steelMaterial = CreateMaterial(shader, "Steel", new Color(0.30f, 0.33f, 0.37f), 0.85f, 0.5f);
            gripMaterial = CreateMaterial(shader, "Grip", new Color(0.025f, 0.028f, 0.032f), 0f, 0.18f);
            openingMaterial = CreateMaterial(shader, "Opening", new Color(0.007f, 0.008f, 0.01f), 0f, 0f);
            return true;
        }

        private Material CreateMaterial(Shader shader, string suffix, Color color, float metallic, float smoothness)
        {
            Material material = new Material(shader) { name = "Runtime_DesertEagle" + suffix };
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            return material;
        }

        private void Part(string objectName, Vector3 position, Vector3 scale, Material material, Quaternion? rotation = null)
        {
            Transform part = recoilPivot.Find(objectName);
            if (part == null)
            {
                part = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                part.name = objectName;
                part.SetParent(recoilPivot, false);
            }
            part.localPosition = position;
            part.localRotation = rotation ?? Quaternion.identity;
            part.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                Destroy(collider);
            }
        }

        private void SetupFinishDetails()
        {
            gripTexture = WeaponDetailMesh.Surface(false);
            gripMaterial.SetTexture("_BaseMap", gripTexture);
            WeaponDetailMesh steel = new WeaponDetailMesh();
            steel.Box(new Vector3(0f, 0.25f, 0.15f), new Vector3(0.34f, 0.07f, 1.1f));
            steel.Ring(new Vector3(0f, 0.12f, 0.925f), 0.105f, 0.071f, 0.045f);
            for (int side = -1; side <= 1; side += 2)
            {
                steel.Box(new Vector3(side*0.17f,-0.06f,0.15f), new Vector3(0.035f,0.045f,0.82f));
                steel.Box(new Vector3(side*0.19f,0.04f,-0.14f), new Vector3(0.04f,0.08f,0.12f));
                for (int i = 0; i < 2; i++)
                    steel.Ring(new Vector3(side*0.177f,-0.28f-i*0.22f,-0.24f),0.027f,0.012f,0.014f,Quaternion.Euler(0f,90f,0f));
            }
            detailMeshes.Add(steel.Build(recoilPivot,"SlideBevelsAndHardware",steelMaterial));
            WeaponDetailMesh seams = new WeaponDetailMesh();
            seams.Ring(new Vector3(0f,0.12f,0.913f),0.070f,0.005f,0.012f);
            for (int side = -1; side <= 1; side += 2)
                seams.Box(new Vector3(side*0.185f,-0.015f,0.14f),new Vector3(0.008f,0.02f,0.98f));
            detailMeshes.Add(seams.Build(recoilPivot,"SlideSeamsAndBore",openingMaterial));
        }

        private void OnDestroy()
        {
            foreach (Mesh mesh in detailMeshes) if (mesh != null) Destroy(mesh);
            if (gripTexture != null) Destroy(gripTexture);
            if (bodyMaterial != null) Destroy(bodyMaterial);
            if (steelMaterial != null) Destroy(steelMaterial);
            if (gripMaterial != null) Destroy(gripMaterial);
            if (openingMaterial != null) Destroy(openingMaterial);
        }
    }
}
