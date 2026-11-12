using System;
using System.Reflection;
using UnityEngine;
using WallBreaker.Player;
using WallBreaker.Visuals;

namespace WallBreaker.Game
{
    public enum WeaponType
    {
        Slingshot,
        Cannon,
        Crossbow,
        Bazooka,
        DesertEagle = 5
    }

    public enum WeaponStage
    {
        Slingshot,
        Cannon,
        Crossbow,
        Bazooka,
        DesertEagle = 5
    }

    public class WeaponManager : MonoBehaviour
    {
        private static WeaponManager instance;
        private const string CannonObjectName = "CannonWeapon";
        private const string CrossbowObjectName = "CrossbowWeapon";
        private const string BazookaObjectName = "BazookaWeapon";
        private const string DesertEagleObjectName = "DesertEagleWeapon";

        private static WeaponStage currentStage = WeaponStage.Slingshot;

        [SerializeField] private WeaponType currentWeapon = WeaponType.Slingshot;

        public static event Action<WeaponType> WeaponChanged;

        public static WeaponManager Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                WeaponManager existing = FindFirstObjectByType<WeaponManager>();
                if (existing != null)
                {
                    instance = existing;
                    return instance;
                }

                GameObject managerObject = new GameObject("WeaponManager");
                instance = managerObject.AddComponent<WeaponManager>();
                return instance;
            }
        }

        public static WeaponStage CurrentStage => currentStage;
        public static WeaponType CurrentWeapon => Instance.GetWeaponForCurrentStage();
        public static string CurrentWeaponDisplayName
        {
            get
            {
                if (CurrentWeapon == WeaponType.DesertEagle)
                {
                    return "DESERT EAGLE";
                }

                if (CurrentWeapon == WeaponType.Crossbow)
                {
                    return "CROSSBOW";
                }

                if (CurrentWeapon == WeaponType.Bazooka)
                {
                    return "BAZOOKA";
                }

                return CurrentWeapon == WeaponType.Cannon ? "CANNON" : "SLINGSHOT";
            }
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

        public void SetWeaponForLevel(int level)
        {
            ApplyCurrentWeapon();
        }

        public void ApplyCurrentWeapon()
        {
            SetCurrentWeapon(GetWeaponForCurrentStage());
            ApplyWeaponActivation();
        }

        public void AdvanceToCannonStage()
        {
            SetStage(WeaponStage.Cannon);
        }

        public void AdvanceToCrossbowStage()
        {
            SetStage(WeaponStage.Crossbow);
        }

        public void AdvanceToBazookaStage()
        {
            SetStage(WeaponStage.Bazooka);
        }

        public static void SetStage(WeaponStage stage)
        {
            currentStage = stage;

            if (instance != null)
            {
                instance.ApplyCurrentWeapon();
            }
        }

#if UNITY_EDITOR
        public static void DebugSetStage(WeaponStage stage)
        {
            SetStage(stage);
        }
#endif

        private WeaponType GetWeaponForCurrentStage()
        {
            if (currentStage == WeaponStage.DesertEagle)
            {
                return WeaponType.DesertEagle;
            }

            if (currentStage == WeaponStage.Crossbow)
            {
                return WeaponType.Crossbow;
            }

            if (currentStage == WeaponStage.Bazooka)
            {
                return WeaponType.Bazooka;
            }

            return currentStage == WeaponStage.Cannon ? WeaponType.Cannon : WeaponType.Slingshot;
        }

        private void SetCurrentWeapon(WeaponType weaponType)
        {
            if (currentWeapon == weaponType)
            {
                return;
            }

            currentWeapon = weaponType;
            WeaponChanged?.Invoke(currentWeapon);
        }

        private void ApplyWeaponActivation()
        {
            SlingshotController slingshotController = FindFirstObjectByType<SlingshotController>(FindObjectsInactive.Include);
            GameObject slingshotObject = slingshotController != null ? slingshotController.gameObject : GameObject.Find("SlingshotPlaceholder");
            bool useCannon = currentWeapon == WeaponType.Cannon;
            bool useCrossbow = currentWeapon == WeaponType.Crossbow;
            bool useBazooka = currentWeapon == WeaponType.Bazooka;
            bool useDesertEagle = currentWeapon == WeaponType.DesertEagle;

            if (!useDesertEagle)
            {
                SetDesertEagleActive(false);
            }

            if (slingshotObject != null)
            {
                slingshotObject.SetActive(!useCannon && !useCrossbow && !useBazooka && !useDesertEagle);
            }

            if (useDesertEagle)
            {
                GameObject desertEagleObject = GetOrCreateDesertEagle(slingshotController, slingshotObject);
                SetCannonActive(false);
                SetCrossbowActive(false);
                SetBazookaActive(false);
                desertEagleObject.SetActive(true);
                return;
            }

            if (useCannon)
            {
                GameObject cannonObject = GetOrCreateCannon(slingshotController, slingshotObject);
                cannonObject.SetActive(true);
                SetCrossbowActive(false);
                SetBazookaActive(false);
                return;
            }

            if (useCrossbow)
            {
                GameObject crossbowObject = GetOrCreateCrossbow(slingshotController, slingshotObject);
                crossbowObject.SetActive(true);
                SetCannonActive(false);
                SetBazookaActive(false);
                return;
            }

            if (useBazooka)
            {
                GameObject bazookaObject = GetOrCreateBazooka(slingshotController, slingshotObject);
                bazookaObject.SetActive(true);
                SetCannonActive(false);
                SetCrossbowActive(false);
                return;
            }

            SetCannonActive(false);
            SetCrossbowActive(false);
            SetBazookaActive(false);
        }

        private GameObject GetOrCreateCannon(SlingshotController slingshotController, GameObject slingshotObject)
        {
            CannonController existingController = FindFirstObjectByType<CannonController>(FindObjectsInactive.Include);
            GameObject cannonObject = existingController != null ? existingController.gameObject : GameObject.Find(CannonObjectName);
            if (cannonObject == null)
            {
                cannonObject = new GameObject(CannonObjectName);
            }

            cannonObject.transform.position = slingshotObject != null ? slingshotObject.transform.position : new Vector3(0f, 0.45f, -3f);
            cannonObject.transform.rotation = Quaternion.identity;
            cannonObject.transform.localScale = Vector3.one;

            CannonVisual visual = cannonObject.GetComponent<CannonVisual>();
            if (visual == null)
            {
                visual = cannonObject.AddComponent<CannonVisual>();
            }

            visual.EnsureVisuals();

            CannonController controller = cannonObject.GetComponent<CannonController>();
            if (controller == null)
            {
                controller = cannonObject.AddComponent<CannonController>();
            }

            controller.Initialize(
                ReadSerializedField<GameObject>(slingshotController, "projectilePrefab"),
                visual.LaunchPoint,
                ReadSerializedField<Transform>(slingshotController, "wallTarget")
            );

            return cannonObject;
        }

        private GameObject GetOrCreateCrossbow(SlingshotController slingshotController, GameObject slingshotObject)
        {
            CrossbowController existingController = FindFirstObjectByType<CrossbowController>(FindObjectsInactive.Include);
            GameObject crossbowObject = existingController != null ? existingController.gameObject : GameObject.Find(CrossbowObjectName);
            if (crossbowObject == null)
            {
                crossbowObject = new GameObject(CrossbowObjectName);
            }

            crossbowObject.transform.position = slingshotObject != null ? slingshotObject.transform.position : new Vector3(0f, 0.45f, -3f);
            crossbowObject.transform.rotation = Quaternion.identity;
            crossbowObject.transform.localScale = Vector3.one;

            CrossbowVisual visual = crossbowObject.GetComponent<CrossbowVisual>();
            if (visual == null)
            {
                visual = crossbowObject.AddComponent<CrossbowVisual>();
            }

            visual.EnsureVisuals();

            CrossbowController controller = crossbowObject.GetComponent<CrossbowController>();
            if (controller == null)
            {
                controller = crossbowObject.AddComponent<CrossbowController>();
            }

            controller.Initialize(
                ReadSerializedField<GameObject>(slingshotController, "projectilePrefab"),
                visual.LaunchPoint,
                ReadSerializedField<Transform>(slingshotController, "wallTarget")
            );

            return crossbowObject;
        }

        private GameObject GetOrCreateBazooka(SlingshotController slingshotController, GameObject slingshotObject)
        {
            BazookaController existingController = FindFirstObjectByType<BazookaController>(FindObjectsInactive.Include);
            GameObject bazookaObject = existingController != null ? existingController.gameObject : GameObject.Find(BazookaObjectName);
            if (bazookaObject == null)
            {
                bazookaObject = new GameObject(BazookaObjectName);
            }

            bazookaObject.transform.position = slingshotObject != null ? slingshotObject.transform.position : new Vector3(0f, 0.45f, -3f);
            bazookaObject.transform.rotation = Quaternion.identity;
            bazookaObject.transform.localScale = Vector3.one;

            BazookaVisual visual = bazookaObject.GetComponent<BazookaVisual>();
            if (visual == null)
            {
                visual = bazookaObject.AddComponent<BazookaVisual>();
            }

            visual.EnsureVisuals();

            BazookaController controller = bazookaObject.GetComponent<BazookaController>();
            if (controller == null)
            {
                controller = bazookaObject.AddComponent<BazookaController>();
            }

            controller.Initialize(
                ReadSerializedField<GameObject>(slingshotController, "projectilePrefab"),
                visual.LaunchPoint,
                ReadSerializedField<Transform>(slingshotController, "wallTarget")
            );

            return bazookaObject;
        }

        private GameObject GetOrCreateDesertEagle(SlingshotController slingshotController, GameObject slingshotObject)
        {
            DesertEagleController existingController = FindFirstObjectByType<DesertEagleController>(FindObjectsInactive.Include);
            GameObject desertEagleObject = existingController != null ? existingController.gameObject : GameObject.Find(DesertEagleObjectName);
            if (desertEagleObject == null)
            {
                desertEagleObject = new GameObject(DesertEagleObjectName);
            }

            desertEagleObject.transform.position = slingshotObject != null ? slingshotObject.transform.position : new Vector3(0f, 0.45f, -3f);
            desertEagleObject.transform.rotation = Quaternion.identity;
            desertEagleObject.transform.localScale = Vector3.one;

            DesertEagleVisual visual = desertEagleObject.GetComponent<DesertEagleVisual>();
            if (visual == null)
            {
                visual = desertEagleObject.AddComponent<DesertEagleVisual>();
            }
            visual.EnsureVisuals();

            DesertEagleController controller = desertEagleObject.GetComponent<DesertEagleController>();
            if (controller == null)
            {
                controller = desertEagleObject.AddComponent<DesertEagleController>();
            }
            controller.Initialize(
                visual,
                ReadSerializedField<Transform>(slingshotController, "wallTarget"),
                desertEagleObject.transform
            );
            return desertEagleObject;
        }

        private void SetDesertEagleActive(bool active)
        {
            DesertEagleController existingController = FindFirstObjectByType<DesertEagleController>(FindObjectsInactive.Include);
            if (existingController != null)
            {
                existingController.gameObject.SetActive(active);
            }
        }

        private void SetCannonActive(bool active)
        {
            CannonController existingCannon = FindFirstObjectByType<CannonController>(FindObjectsInactive.Include);
            if (existingCannon != null)
            {
                existingCannon.gameObject.SetActive(active);
            }
        }

        private void SetCrossbowActive(bool active)
        {
            CrossbowController existingCrossbow = FindFirstObjectByType<CrossbowController>(FindObjectsInactive.Include);
            if (existingCrossbow != null)
            {
                existingCrossbow.gameObject.SetActive(active);
            }
        }

        private void SetBazookaActive(bool active)
        {
            BazookaController existingBazooka = FindFirstObjectByType<BazookaController>(FindObjectsInactive.Include);
            if (existingBazooka != null)
            {
                existingBazooka.gameObject.SetActive(active);
            }
        }

        private T ReadSerializedField<T>(SlingshotController slingshotController, string fieldName) where T : UnityEngine.Object
        {
            if (slingshotController == null)
            {
                return null;
            }

            FieldInfo field = typeof(SlingshotController).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            return field != null ? field.GetValue(slingshotController) as T : null;
        }
    }
}
