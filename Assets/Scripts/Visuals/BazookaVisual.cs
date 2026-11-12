using System.Collections;
using UnityEngine;

namespace WallBreaker.Visuals
{
    public class BazookaVisual : MonoBehaviour
    {
        private readonly Vector3 neutralAimDirection = new Vector3(0f, 0.22f, 1f).normalized;
        private readonly Vector3 tubeRestLocalPosition = new Vector3(0f, 0.78f, 0.15f);

        private Transform aimPivot;
        private Transform muzzlePoint;
        private Transform launchPoint;
        private Material tubeMaterial;
        private Material darkMaterial;
        private Material metalMaterial;
        private Material accentMaterial;
        private Material flashMaterial;
        private Coroutine recoilRoutine;
        private bool finishCreated;
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
            SetupBody();
            SetupLaunchPoint();
            SetupFinishDetails();
        }

        public void SetAimVisual(Vector3 worldDirection)
        {
            EnsureAimPivot();

            if (worldDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector3 localDirection = transform.InverseTransformDirection(worldDirection.normalized);
            localDirection.z = Mathf.Max(0.28f, localDirection.z);
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.forward, localDirection.normalized);
            aimPivot.localRotation = Quaternion.Slerp(aimPivot.localRotation, targetRotation, 0.28f);
        }

        public void PlayFireFeedback()
        {
            EnsureVisuals();

            if (recoilRoutine != null)
            {
                StopCoroutine(recoilRoutine);
            }

            recoilRoutine = StartCoroutine(PlayRecoil());
            SpawnMuzzleSmoke();
        }

        private void CreateMaterials()
        {
            if (tubeMaterial != null)
            {
                return;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            tubeMaterial = CreateMaterial(shader, "Runtime_BazookaTube", new Color(0.18f, 0.47f, 0.26f), 0.32f);
            darkMaterial = CreateMaterial(shader, "Runtime_BazookaDark", new Color(0.08f, 0.13f, 0.08f), 0.20f);
            metalMaterial = CreateMaterial(shader, "Runtime_BazookaMetal", new Color(0.24f, 0.26f, 0.24f), 0.45f);
            accentMaterial = CreateMaterial(shader, "Runtime_BazookaAccent", new Color(0.95f, 0.48f, 0.14f), 0.22f);
            flashMaterial = CreateMaterial(shader, "Runtime_BazookaMuzzle", new Color(1f, 0.56f, 0.16f), 0.12f);
            if(tubeMaterial.HasProperty("_Metallic")) tubeMaterial.SetFloat("_Metallic",0.25f);
            if(metalMaterial.HasProperty("_Metallic")) metalMaterial.SetFloat("_Metallic",0.75f);
            if(darkMaterial.HasProperty("_Smoothness")) darkMaterial.SetFloat("_Smoothness",0.14f);
        }

        private void SetupFinishDetails()
        {
            if(finishCreated) return;
            finishCreated=true;
            WeaponDetailMesh steel=new WeaponDetailMesh();
            steel.Ring(new Vector3(0,0,1.22f),0.19f,0.14f,0.09f);
            steel.Ring(new Vector3(0,0,-0.69f),0.18f,0.13f,0.08f);
            for(int i=0;i<3;i++) steel.Ring(new Vector3(0,0,-0.3f+i*0.45f),0.15f,0.137f,0.045f);
            steel.Box(new Vector3(0,0.21f,0.32f),new Vector3(0.25f,0.08f,0.66f));
            for(int side=-1;side<=1;side+=2)
                steel.Box(new Vector3(side*0.15f,0,0.1f),new Vector3(0.045f,0.20f,0.55f));
            detailMeshes.Add(steel.Build(aimPivot,"TubeBandsAndArmorPlates",metalMaterial));
            WeaponDetailMesh black=new WeaponDetailMesh();
            black.Ring(new Vector3(0,0,1.19f),0.137f,0.005f,0.012f);
            black.Ring(new Vector3(0,0,-0.66f),0.127f,0.005f,0.012f);
            black.Box(new Vector3(0,-0.31f,0.63f),new Vector3(0.13f,0.34f,0.19f),Quaternion.Euler(-12,0,0));
            black.Box(new Vector3(0,0.34f,0.37f),new Vector3(0.13f,0.19f,0.32f));
            for(int i=0;i<4;i++) black.Box(new Vector3(0,-0.27f-i*0.065f,0.10f),new Vector3(0.27f,0.025f,0.16f));
            detailMeshes.Add(black.Build(aimPivot,"RecessedMouthsSightAndGrips",darkMaterial));
            WeaponDetailMesh markings=new WeaponDetailMesh();
            for(int i=0;i<3;i++) markings.Box(new Vector3(-0.166f,0.015f,-0.12f+i*0.08f),new Vector3(0.01f,0.13f,0.035f),Quaternion.Euler(20,0,0));
            detailMeshes.Add(markings.Build(aimPivot,"SafetyMarkings",accentMaterial));
        }

        private void OnDestroy()
        {
            foreach(Mesh mesh in detailMeshes) if(mesh!=null) Destroy(mesh);
            foreach(Material material in new[] {tubeMaterial,darkMaterial,metalMaterial,accentMaterial,flashMaterial})
                if(material!=null) Destroy(material);
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

        private void SetupBody()
        {
            EnsureAimPivot();

            Transform tube = FindOrCreatePrimitive(aimPivot, "LauncherTube", PrimitiveType.Cylinder);
            tube.localPosition = new Vector3(0f, 0f, 0.25f);
            tube.localRotation = Quaternion.Euler(90f, 0f, 0f);
            tube.localScale = new Vector3(0.27f, 0.90f, 0.27f);
            ApplyMaterial(tube.gameObject, tubeMaterial);
            RemoveCollider(tube.gameObject);

            Transform frontRing = FindOrCreatePrimitive(aimPivot, "FrontRing", PrimitiveType.Cylinder);
            frontRing.localPosition = new Vector3(0f, 0f, 1.18f);
            frontRing.localRotation = Quaternion.Euler(90f, 0f, 0f);
            frontRing.localScale = new Vector3(0.33f, 0.06f, 0.33f);
            ApplyMaterial(frontRing.gameObject, metalMaterial);
            RemoveCollider(frontRing.gameObject);

            Transform rearRing = FindOrCreatePrimitive(aimPivot, "RearRing", PrimitiveType.Cylinder);
            rearRing.localPosition = new Vector3(0f, 0f, -0.65f);
            rearRing.localRotation = Quaternion.Euler(90f, 0f, 0f);
            rearRing.localScale = new Vector3(0.31f, 0.06f, 0.31f);
            ApplyMaterial(rearRing.gameObject, metalMaterial);
            RemoveCollider(rearRing.gameObject);

            Transform shoulderPad = FindOrCreatePrimitive(aimPivot, "ShoulderPad", PrimitiveType.Cube);
            shoulderPad.localPosition = new Vector3(0f, -0.28f, -0.42f);
            shoulderPad.localRotation = Quaternion.identity;
            shoulderPad.localScale = new Vector3(0.52f, 0.16f, 0.28f);
            ApplyMaterial(shoulderPad.gameObject, darkMaterial);
            RemoveCollider(shoulderPad.gameObject);

            Transform handle = FindOrCreatePrimitive(aimPivot, "Handle", PrimitiveType.Cylinder);
            handle.localPosition = new Vector3(0f, -0.36f, 0.16f);
            handle.localRotation = Quaternion.Euler(18f, 0f, 0f);
            handle.localScale = new Vector3(0.12f, 0.30f, 0.12f);
            ApplyMaterial(handle.gameObject, darkMaterial);
            RemoveCollider(handle.gameObject);

            Transform stripe = FindOrCreatePrimitive(aimPivot, "AccentStripe", PrimitiveType.Cube);
            stripe.localPosition = new Vector3(0f, 0.29f, 0.46f);
            stripe.localRotation = Quaternion.identity;
            stripe.localScale = new Vector3(0.56f, 0.05f, 0.22f);
            ApplyMaterial(stripe.gameObject, accentMaterial);
            RemoveCollider(stripe.gameObject);

            muzzlePoint = aimPivot.Find("MuzzlePoint");
            if (muzzlePoint == null)
            {
                muzzlePoint = new GameObject("MuzzlePoint").transform;
                muzzlePoint.SetParent(aimPivot, false);
            }

            muzzlePoint.localPosition = new Vector3(0f, 0f, 1.26f);
            muzzlePoint.localRotation = Quaternion.identity;
        }

        private void SetupLaunchPoint()
        {
            Transform existing = transform.Find("BazookaLaunchPoint");
            if (existing == null)
            {
                existing = new GameObject("BazookaLaunchPoint").transform;
                existing.SetParent(transform, false);
            }

            existing.localPosition = new Vector3(0f, 0.90f, 1.26f);
            existing.localRotation = Quaternion.identity;
            launchPoint = existing;
        }

        private IEnumerator PlayRecoil()
        {
            EnsureAimPivot();

            const float recoilDistance = 0.16f;
            const float outTime = 0.045f;
            const float returnTime = 0.12f;
            Vector3 recoilPosition = tubeRestLocalPosition - aimPivot.localRotation * Vector3.forward * recoilDistance;

            float timer = 0f;
            while (timer < outTime)
            {
                timer += Time.deltaTime;
                aimPivot.localPosition = Vector3.Lerp(tubeRestLocalPosition, recoilPosition, timer / outTime);
                yield return null;
            }

            timer = 0f;
            while (timer < returnTime)
            {
                timer += Time.deltaTime;
                aimPivot.localPosition = Vector3.Lerp(recoilPosition, tubeRestLocalPosition, timer / returnTime);
                yield return null;
            }

            aimPivot.localPosition = tubeRestLocalPosition;
            recoilRoutine = null;
        }

        private void SpawnMuzzleSmoke()
        {
            if (muzzlePoint != null) WallBreaker.Effects.WeaponEffectsManager.Instance.Fire(WallBreaker.Game.WeaponType.Bazooka, muzzlePoint.position, aimPivot.forward);
        }

        private void EnsureAimPivot()
        {
            bool created = false;
            if (aimPivot == null)
            {
                aimPivot = transform.Find("BazookaAimPivot");
            }

            if (aimPivot == null)
            {
                aimPivot = new GameObject("BazookaAimPivot").transform;
                aimPivot.SetParent(transform, false);
                created = true;
            }

            if (created || recoilRoutine == null)
            {
                aimPivot.localPosition = tubeRestLocalPosition;
            }

            if (created)
            {
                aimPivot.localRotation = Quaternion.FromToRotation(Vector3.forward, neutralAimDirection);
            }
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
