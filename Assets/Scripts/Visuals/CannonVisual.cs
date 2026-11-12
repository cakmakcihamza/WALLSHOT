using System.Collections;
using UnityEngine;

namespace WallBreaker.Visuals
{
    public class CannonVisual : MonoBehaviour
    {
        private readonly Vector3 neutralBarrelDirection = new Vector3(0f, 0.55f, 1f).normalized;
        private readonly Vector3 barrelRestLocalPosition = new Vector3(0f, 0.48f, 0.18f);

        private Transform barrelPivot;
        private Transform muzzlePoint;
        private Transform launchPoint;
        private Material bodyMaterial;
        private Material wheelMaterial;
        private Material barrelMaterial;
        private Material openingMaterial;
        private Material metalRingMaterial;
        private Material muzzleMaterial;
        private Coroutine recoilRoutine;
        private bool finishCreated;
        private Material brassMaterial;
        private readonly System.Collections.Generic.List<Mesh> detailMeshes = new System.Collections.Generic.List<Mesh>();

        public Transform LaunchPoint
        {
            get
            {
                EnsureVisuals();
                return launchPoint;
            }
        }

        private void Awake()
        {
            EnsureVisuals();
        }

        public void EnsureVisuals()
        {
            CreateMaterials();
            SetupBase();
            SetupWheels();
            SetupBarrel();
            SetupLaunchPoint();
            SetupFinishDetails();
        }

        public void SetAimVisual(Vector3 worldDirection)
        {
            EnsureBarrelPivot();

            if (worldDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector3 localDirection = transform.InverseTransformDirection(worldDirection.normalized);
            localDirection.z = Mathf.Max(0.25f, localDirection.z);
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, localDirection.normalized);
            barrelPivot.localRotation = Quaternion.Slerp(barrelPivot.localRotation, targetRotation, 0.45f);
        }

        public void PlayFireFeedback()
        {
            EnsureVisuals();

            if (recoilRoutine != null)
            {
                StopCoroutine(recoilRoutine);
            }

            recoilRoutine = StartCoroutine(PlayRecoil());
            SpawnMuzzleEffect();
        }

        private void CreateMaterials()
        {
            if (bodyMaterial != null)
            {
                return;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            bodyMaterial = CreateMaterial(shader, "Runtime_CannonBase", new Color(0.16f, 0.19f, 0.22f), 0.28f);
            wheelMaterial = CreateMaterial(shader, "Runtime_CannonWheels", new Color(0.055f, 0.065f, 0.075f), 0.20f);
            barrelMaterial = CreateMaterial(shader, "Runtime_CannonBarrel", new Color(0.13f, 0.15f, 0.17f), 0.42f);
            openingMaterial = CreateMaterial(shader, "Runtime_CannonOpening", new Color(0.035f, 0.035f, 0.04f), 0.20f);
            metalRingMaterial = CreateMaterial(shader, "Runtime_CannonMetalRing", new Color(0.28f, 0.30f, 0.32f), 0.50f);
            muzzleMaterial = CreateMaterial(shader, "Runtime_CannonMuzzle", new Color(1f, 0.62f, 0.16f, 1f), 0.15f);
            brassMaterial = CreateMaterial(shader, "Runtime_CannonBrass", new Color(0.47f, 0.34f, 0.16f), 0.35f);
            if (barrelMaterial.HasProperty("_Metallic")) barrelMaterial.SetFloat("_Metallic", 0.75f);
            if (metalRingMaterial.HasProperty("_Metallic")) metalRingMaterial.SetFloat("_Metallic", 0.8f);
            if (brassMaterial.HasProperty("_Metallic")) brassMaterial.SetFloat("_Metallic", 0.7f);
        }

        private void SetupFinishDetails()
        {
            if (finishCreated) return;
            finishCreated = true;
            WeaponDetailMesh rings = new WeaponDetailMesh();
            Quaternion axis = Quaternion.Euler(-90f,0f,0f);
            for (int i=0;i<3;i++) rings.Ring(new Vector3(0f,0.12f+i*0.25f,0f),0.175f,0.153f,0.045f,axis);
            rings.Ring(new Vector3(0f,0.91f,0f),0.205f,0.135f,0.12f,axis);
            rings.Box(new Vector3(0f,-0.18f,0f),new Vector3(0.38f,0.2f,0.36f));
            detailMeshes.Add(rings.Build(barrelPivot,"ReinforcedBarrelAndBreech",metalRingMaterial));
            Transform oldOpening = barrelPivot.Find("BarrelOpening");
            if (oldOpening != null) oldOpening.GetComponent<Renderer>().enabled = false;
            WeaponDetailMesh bore = new WeaponDetailMesh();
            bore.Ring(new Vector3(0f,0.885f,0f),0.133f,0.005f,0.012f,axis);
            detailMeshes.Add(bore.Build(barrelPivot,"RecessedBore",openingMaterial));
            WeaponDetailMesh carriage = new WeaponDetailMesh();
            for(int side=-1;side<=1;side+=2)
            {
                carriage.Box(new Vector3(side*0.44f,0.39f,0.07f),new Vector3(0.12f,0.32f,0.5f));
                carriage.Ring(new Vector3(side*0.77f,0.16f,-0.02f),0.145f,0.11f,0.03f,Quaternion.Euler(0,90,0));
                carriage.Ring(new Vector3(side*0.79f,0.16f,-0.02f),0.06f,0.025f,0.04f,Quaternion.Euler(0,90,0));
            }
            detailMeshes.Add(carriage.Build(transform,"CarriagePlatesAndWheelHubs",brassMaterial));
        }

        private void OnDestroy()
        {
            foreach(Mesh mesh in detailMeshes) if(mesh != null) Destroy(mesh);
            foreach(Material material in new[] { bodyMaterial,wheelMaterial,barrelMaterial,openingMaterial,metalRingMaterial,muzzleMaterial,brassMaterial })
                if(material != null) Destroy(material);
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

        private void SetupBase()
        {
            Transform baseObject = FindOrCreatePrimitive("CannonBase", PrimitiveType.Cube);
            baseObject.localPosition = new Vector3(0f, 0.18f, 0f);
            baseObject.localRotation = Quaternion.identity;
            baseObject.localScale = new Vector3(1.18f, 0.28f, 0.78f);
            ApplyMaterial(baseObject.gameObject, bodyMaterial);
            RemoveCollider(baseObject.gameObject);

            Transform baseCap = FindOrCreatePrimitive("CannonBaseTop", PrimitiveType.Cube);
            baseCap.localPosition = new Vector3(0f, 0.34f, 0.05f);
            baseCap.localRotation = Quaternion.identity;
            baseCap.localScale = new Vector3(0.86f, 0.12f, 0.54f);
            ApplyMaterial(baseCap.gameObject, bodyMaterial);
            RemoveCollider(baseCap.gameObject);
        }

        private void SetupWheels()
        {
            SetupWheel("LeftWheel", new Vector3(-0.68f, 0.16f, -0.02f));
            SetupWheel("RightWheel", new Vector3(0.68f, 0.16f, -0.02f));
        }

        private void SetupWheel(string objectName, Vector3 localPosition)
        {
            Transform wheel = FindOrCreatePrimitive(objectName, PrimitiveType.Cylinder);
            wheel.localPosition = localPosition;
            wheel.localRotation = Quaternion.Euler(0f, 0f, 90f);
            wheel.localScale = new Vector3(0.34f, 0.14f, 0.34f);
            ApplyMaterial(wheel.gameObject, wheelMaterial);
            RemoveCollider(wheel.gameObject);
        }

        private void SetupBarrel()
        {
            EnsureBarrelPivot();

            Transform barrel = FindOrCreatePrimitive(barrelPivot, "Barrel", PrimitiveType.Cylinder);
            barrel.localPosition = new Vector3(0f, 0.34f, 0f);
            barrel.localRotation = Quaternion.identity;
            barrel.localScale = new Vector3(0.31f, 0.52f, 0.31f);
            ApplyMaterial(barrel.gameObject, barrelMaterial);
            RemoveCollider(barrel.gameObject);

            Transform rearRing = FindOrCreatePrimitive(barrelPivot, "RearMetalRing", PrimitiveType.Cylinder);
            rearRing.localPosition = new Vector3(0f, -0.06f, 0f);
            rearRing.localRotation = Quaternion.identity;
            rearRing.localScale = new Vector3(0.35f, 0.045f, 0.35f);
            ApplyMaterial(rearRing.gameObject, metalRingMaterial);
            RemoveCollider(rearRing.gameObject);

            Transform frontRing = FindOrCreatePrimitive(barrelPivot, "FrontMetalRing", PrimitiveType.Cylinder);
            frontRing.localPosition = new Vector3(0f, 0.83f, 0f);
            frontRing.localRotation = Quaternion.identity;
            frontRing.localScale = new Vector3(0.36f, 0.055f, 0.36f);
            ApplyMaterial(frontRing.gameObject, metalRingMaterial);
            RemoveCollider(frontRing.gameObject);

            Transform opening = FindOrCreatePrimitive(barrelPivot, "BarrelOpening", PrimitiveType.Sphere);
            opening.localPosition = new Vector3(0f, 0.92f, 0f);
            opening.localRotation = Quaternion.identity;
            opening.localScale = new Vector3(0.36f, 0.15f, 0.36f);
            ApplyMaterial(opening.gameObject, openingMaterial);
            RemoveCollider(opening.gameObject);

            muzzlePoint = barrelPivot.Find("MuzzlePoint");
            if (muzzlePoint == null)
            {
                muzzlePoint = new GameObject("MuzzlePoint").transform;
                muzzlePoint.SetParent(barrelPivot, false);
            }

            muzzlePoint.localPosition = new Vector3(0f, 0.98f, 0f);
            muzzlePoint.localRotation = Quaternion.identity;
        }

        private void SetupLaunchPoint()
        {
            Transform existing = transform.Find("CannonLaunchPoint");
            if (existing == null)
            {
                existing = new GameObject("CannonLaunchPoint").transform;
                existing.SetParent(transform, false);
            }

            existing.localPosition = new Vector3(0f, 0.86f, 1.08f);
            existing.localRotation = Quaternion.identity;
            launchPoint = existing;
        }

        private IEnumerator PlayRecoil()
        {
            EnsureBarrelPivot();

            const float recoilDistance = 0.12f;
            const float outTime = 0.035f;
            const float returnTime = 0.09f;
            Vector3 recoilPosition = barrelRestLocalPosition - barrelPivot.localRotation * Vector3.up * recoilDistance;

            float timer = 0f;
            while (timer < outTime)
            {
                timer += Time.deltaTime;
                barrelPivot.localPosition = Vector3.Lerp(barrelRestLocalPosition, recoilPosition, timer / outTime);
                yield return null;
            }

            timer = 0f;
            while (timer < returnTime)
            {
                timer += Time.deltaTime;
                barrelPivot.localPosition = Vector3.Lerp(recoilPosition, barrelRestLocalPosition, timer / returnTime);
                yield return null;
            }

            barrelPivot.localPosition = barrelRestLocalPosition;
            recoilRoutine = null;
        }

        private void SpawnMuzzleEffect()
        {
            if (muzzlePoint != null) WallBreaker.Effects.WeaponEffectsManager.Instance.Fire(WallBreaker.Game.WeaponType.Cannon, muzzlePoint.position, barrelPivot.up);
        }

        private void EnsureBarrelPivot()
        {
            bool created = false;
            if (barrelPivot == null)
            {
                barrelPivot = transform.Find("BarrelPivot");
            }

            if (barrelPivot == null)
            {
                barrelPivot = new GameObject("BarrelPivot").transform;
                barrelPivot.SetParent(transform, false);
                created = true;
            }

            if (created || recoilRoutine == null)
            {
                barrelPivot.localPosition = barrelRestLocalPosition;
            }

            if (created)
            {
                barrelPivot.localRotation = Quaternion.FromToRotation(Vector3.up, neutralBarrelDirection);
            }
        }

        private Transform FindOrCreatePrimitive(string objectName, PrimitiveType primitiveType)
        {
            return FindOrCreatePrimitive(transform, objectName, primitiveType);
        }

        private Transform FindOrCreatePrimitive(Transform parent, string objectName, PrimitiveType primitiveType)
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
