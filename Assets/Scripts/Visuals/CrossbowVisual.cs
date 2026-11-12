using System.Collections;
using UnityEngine;

namespace WallBreaker.Visuals
{
    public class CrossbowVisual : MonoBehaviour
    {
        private readonly Vector3 neutralAimDirection = new Vector3(0f, 0.28f, 1f).normalized;
        private readonly Vector3 bowRestLocalPosition = new Vector3(0f, 0.72f, 0.24f);

        private Transform aimPivot;
        private Transform stringCenter;
        private Transform leftString;
        private Transform rightString;
        private Transform launchPoint;
        private Material woodMaterial;
        private Material darkWoodMaterial;
        private Material metalMaterial;
        private Material stringMaterial;
        private Material boltMaterial;
        private Material flashMaterial;
        private Coroutine recoilRoutine;
        private bool finishCreated;
        private Texture2D grainTexture;
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
            SetupBow();
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
            localDirection.z = Mathf.Max(0.3f, localDirection.z);
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.forward, localDirection.normalized);
            aimPivot.localRotation = Quaternion.Slerp(aimPivot.localRotation, targetRotation, 0.5f);
        }

        public void SetDrawVisual(float normalizedDraw)
        {
            EnsureVisuals();
            normalizedDraw = Mathf.Clamp01(normalizedDraw);
            stringCenter.localPosition = new Vector3(0f, 0.02f, -0.22f * normalizedDraw);
            UpdateStringSegment(leftString, new Vector3(-0.78f, 0.06f, 0.62f), stringCenter.localPosition);
            UpdateStringSegment(rightString, new Vector3(0.78f, 0.06f, 0.62f), stringCenter.localPosition);
        }

        public void ResetVisual()
        {
            SetDrawVisual(0f);
        }

        public void PlayFireFeedback()
        {
            EnsureVisuals();

            if (recoilRoutine != null)
            {
                StopCoroutine(recoilRoutine);
            }

            recoilRoutine = StartCoroutine(PlayRecoil());
            SpawnBoltFlash();
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

            woodMaterial = CreateMaterial(shader, "Runtime_CrossbowWood", new Color(0.48f, 0.25f, 0.10f), 0.26f);
            darkWoodMaterial = CreateMaterial(shader, "Runtime_CrossbowDarkWood", new Color(0.30f, 0.14f, 0.06f), 0.22f);
            metalMaterial = CreateMaterial(shader, "Runtime_CrossbowMetal", new Color(0.27f, 0.29f, 0.30f), 0.48f);
            stringMaterial = CreateMaterial(shader, "Runtime_CrossbowString", new Color(0.055f, 0.040f, 0.030f), 0.12f);
            boltMaterial = CreateMaterial(shader, "Runtime_CrossbowBolt", new Color(0.12f, 0.12f, 0.13f), 0.32f);
            flashMaterial = CreateMaterial(shader, "Runtime_CrossbowFlash", new Color(1f, 0.82f, 0.28f), 0.16f);
            grainTexture = WeaponDetailMesh.Surface(true);
            woodMaterial.SetTexture(woodMaterial.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex", grainTexture);
            darkWoodMaterial.SetTexture(darkWoodMaterial.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex", grainTexture);
            if(metalMaterial.HasProperty("_Metallic")) metalMaterial.SetFloat("_Metallic",0.7f);
            if(stringMaterial.HasProperty("_Smoothness")) stringMaterial.SetFloat("_Smoothness",0.08f);
        }

        private void SetupFinishDetails()
        {
            if(finishCreated) return;
            finishCreated=true;
            WeaponDetailMesh hardware=new WeaponDetailMesh();
            for(int side=-1;side<=1;side+=2)
            {
                hardware.Box(new Vector3(side*0.105f,0.195f,0.12f),new Vector3(0.035f,0.045f,1.28f));
                hardware.Box(new Vector3(side*0.18f,0.02f,0.48f),new Vector3(0.13f,0.2f,0.24f));
                hardware.Ring(new Vector3(side*0.74f,0.06f,0.62f),0.065f,0.035f,0.05f,Quaternion.Euler(90,0,0));
            }
            hardware.Box(new Vector3(0f,-0.17f,-0.12f),new Vector3(0.07f,0.14f,0.045f));
            hardware.Box(new Vector3(0f,0.18f,-0.42f),new Vector3(0.24f,0.06f,0.16f));
            detailMeshes.Add(hardware.Build(aimPivot,"RailsTriggerAndLimbBindings",metalMaterial));
            WeaponDetailMesh limbs=new WeaponDetailMesh();
            for(int side=-1;side<=1;side+=2)
            {
                // Three short, changing-angle sections suggest a recurved composite limb.
                limbs.Box(new Vector3(side*0.24f,0.05f,0.52f),new Vector3(0.32f,0.12f,0.16f),Quaternion.Euler(0,side*-12f,side*8f));
                limbs.Box(new Vector3(side*0.50f,0.07f,0.59f),new Vector3(0.30f,0.10f,0.14f),Quaternion.Euler(0,side*-22f,side*4f));
                limbs.Box(new Vector3(side*0.72f,0.065f,0.62f),new Vector3(0.18f,0.08f,0.12f),Quaternion.Euler(0,side*15f,0));
            }
            detailMeshes.Add(limbs.Build(aimPivot,"RecurvedLimbs",darkWoodMaterial));
            aimPivot.Find("LeftBowArm").GetComponent<Renderer>().enabled=false;
            aimPivot.Find("RightBowArm").GetComponent<Renderer>().enabled=false;
            WeaponDetailMesh bolt=new WeaponDetailMesh();
            bolt.Box(new Vector3(0f,0.23f,0.82f),new Vector3(0.10f,0.065f,0.18f),Quaternion.Euler(0,0,45));
            bolt.Box(new Vector3(0f,0.27f,-0.10f),new Vector3(0.025f,0.12f,0.20f));
            bolt.Box(new Vector3(0f,0.23f,-0.10f),new Vector3(0.15f,0.025f,0.20f));
            detailMeshes.Add(bolt.Build(aimPivot,"LoadedBoltHeadAndFletching",boltMaterial));
        }

        private void OnDestroy()
        {
            foreach(Mesh mesh in detailMeshes) if(mesh!=null) Destroy(mesh);
            if(grainTexture!=null) Destroy(grainTexture);
            foreach(Material material in new[] {woodMaterial,darkWoodMaterial,metalMaterial,stringMaterial,boltMaterial,flashMaterial})
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

            Transform stock = FindOrCreatePrimitive(aimPivot, "Stock", PrimitiveType.Cube);
            stock.localPosition = new Vector3(0f, 0f, -0.02f);
            stock.localRotation = Quaternion.identity;
            stock.localScale = new Vector3(0.30f, 0.22f, 1.28f);
            ApplyMaterial(stock.gameObject, woodMaterial);
            RemoveCollider(stock.gameObject);

            Transform grip = FindOrCreatePrimitive(aimPivot, "Grip", PrimitiveType.Cylinder);
            grip.localPosition = new Vector3(0f, -0.22f, -0.34f);
            grip.localRotation = Quaternion.Euler(22f, 0f, 0f);
            grip.localScale = new Vector3(0.16f, 0.32f, 0.16f);
            ApplyMaterial(grip.gameObject, darkWoodMaterial);
            RemoveCollider(grip.gameObject);

            Transform rail = FindOrCreatePrimitive(aimPivot, "MetalRail", PrimitiveType.Cube);
            rail.localPosition = new Vector3(0f, 0.14f, 0.10f);
            rail.localRotation = Quaternion.identity;
            rail.localScale = new Vector3(0.14f, 0.08f, 1.42f);
            ApplyMaterial(rail.gameObject, metalMaterial);
            RemoveCollider(rail.gameObject);

            Transform loadedBolt = FindOrCreatePrimitive(aimPivot, "LoadedBolt", PrimitiveType.Cylinder);
            loadedBolt.localPosition = new Vector3(0f, 0.23f, 0.30f);
            loadedBolt.localRotation = Quaternion.Euler(90f, 0f, 0f);
            loadedBolt.localScale = new Vector3(0.055f, 0.48f, 0.055f);
            ApplyMaterial(loadedBolt.gameObject, boltMaterial);
            RemoveCollider(loadedBolt.gameObject);
        }

        private void SetupBow()
        {
            EnsureAimPivot();

            Transform leftArm = FindOrCreatePrimitive(aimPivot, "LeftBowArm", PrimitiveType.Cube);
            leftArm.localPosition = new Vector3(-0.42f, 0.05f, 0.58f);
            leftArm.localRotation = Quaternion.Euler(0f, 22f, -10f);
            leftArm.localScale = new Vector3(0.72f, 0.10f, 0.12f);
            ApplyMaterial(leftArm.gameObject, darkWoodMaterial);
            RemoveCollider(leftArm.gameObject);

            Transform rightArm = FindOrCreatePrimitive(aimPivot, "RightBowArm", PrimitiveType.Cube);
            rightArm.localPosition = new Vector3(0.42f, 0.05f, 0.58f);
            rightArm.localRotation = Quaternion.Euler(0f, -22f, 10f);
            rightArm.localScale = new Vector3(0.72f, 0.10f, 0.12f);
            ApplyMaterial(rightArm.gameObject, darkWoodMaterial);
            RemoveCollider(rightArm.gameObject);

            Transform center = aimPivot.Find("StringCenter");
            if (center == null)
            {
                center = new GameObject("StringCenter").transform;
                center.SetParent(aimPivot, false);
            }

            center.localPosition = Vector3.zero;
            stringCenter = center;

            leftString = SetupString("LeftString", new Vector3(-0.78f, 0.06f, 0.62f), stringCenter.localPosition);
            rightString = SetupString("RightString", new Vector3(0.78f, 0.06f, 0.62f), stringCenter.localPosition);
        }

        private Transform SetupString(string objectName, Vector3 start, Vector3 end)
        {
            Transform segment = FindOrCreatePrimitive(aimPivot, objectName, PrimitiveType.Cube);
            UpdateStringSegment(segment, start, end);
            ApplyMaterial(segment.gameObject, stringMaterial);
            RemoveCollider(segment.gameObject);
            return segment;
        }

        private void UpdateStringSegment(Transform segment, Vector3 start, Vector3 end)
        {
            if (segment == null)
            {
                return;
            }

            Vector3 middle = (start + end) * 0.5f;
            Vector3 direction = end - start;
            segment.localPosition = middle;
            segment.localRotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            segment.localScale = new Vector3(0.035f, direction.magnitude * 0.5f, 0.035f);
        }

        private void SetupLaunchPoint()
        {
            Transform existing = transform.Find("CrossbowLaunchPoint");
            if (existing == null)
            {
                existing = new GameObject("CrossbowLaunchPoint").transform;
                existing.SetParent(transform, false);
            }

            existing.localPosition = new Vector3(0f, 0.92f, 1.14f);
            existing.localRotation = Quaternion.identity;
            launchPoint = existing;
        }

        private IEnumerator PlayRecoil()
        {
            EnsureAimPivot();

            const float recoilDistance = 0.10f;
            const float outTime = 0.035f;
            const float returnTime = 0.08f;
            Vector3 recoilPosition = bowRestLocalPosition - aimPivot.localRotation * Vector3.forward * recoilDistance;

            float timer = 0f;
            while (timer < outTime)
            {
                timer += Time.deltaTime;
                aimPivot.localPosition = Vector3.Lerp(bowRestLocalPosition, recoilPosition, timer / outTime);
                yield return null;
            }

            timer = 0f;
            while (timer < returnTime)
            {
                timer += Time.deltaTime;
                aimPivot.localPosition = Vector3.Lerp(recoilPosition, bowRestLocalPosition, timer / returnTime);
                yield return null;
            }

            aimPivot.localPosition = bowRestLocalPosition;
            recoilRoutine = null;
        }

        private void SpawnBoltFlash()
        {
            if (launchPoint != null) WallBreaker.Effects.WeaponEffectsManager.Instance.Fire(WallBreaker.Game.WeaponType.Crossbow, launchPoint.position, aimPivot.forward);
        }

        private void EnsureAimPivot()
        {
            bool created = false;
            if (aimPivot == null)
            {
                aimPivot = transform.Find("CrossbowAimPivot");
            }

            if (aimPivot == null)
            {
                aimPivot = new GameObject("CrossbowAimPivot").transform;
                aimPivot.SetParent(transform, false);
                created = true;
            }

            if (created || recoilRoutine == null)
            {
                aimPivot.localPosition = bowRestLocalPosition;
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
