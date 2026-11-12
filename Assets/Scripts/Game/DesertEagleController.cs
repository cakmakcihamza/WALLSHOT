using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using WallBreaker.Projectile;
using WallBreaker.Visuals;
using InputTouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace WallBreaker.Game
{
    public class DesertEagleController : MonoBehaviour
    {
        [SerializeField] private DesertEagleVisual visual;
        [SerializeField] private Transform wallTarget;
        [SerializeField] private Transform weaponRoot;
        [SerializeField, Range(1f, 100f)] private float bulletSpeed = 35f;
        [SerializeField, Min(1f)] private float maxDragDistance = 300f;
        [SerializeField, Range(0.1f, 3f)] private float aimSensitivity = 1f;
        [SerializeField] private float maxHorizontalTargetOffset = 3.2f;
        [SerializeField] private float minTargetHeight = 0.2f;
        [SerializeField] private float maxTargetHeight = 3.85f;

        private bool dragging;
        private int fingerId = -1;
        private Vector2 dragStart;
        private Transform muzzle;
        private Transform recoilPivot;
        private Vector3 recoilRest;
        private Quaternion recoilRotation;
        private Coroutine recoilRoutine;
        private LineRenderer[] arrows;
        private Material arrowMaterial;
        private Camera aimCamera;
        private const int ArrowCount = 6;

        public void Initialize(DesertEagleVisual newVisual, Transform newWallTarget, Transform newWeaponRoot = null)
        {
            CancelAim();
            ResetRecoil();
            visual = newVisual;
            wallTarget = newWallTarget;
            weaponRoot = newWeaponRoot != null ? newWeaponRoot : transform;
            muzzle = null;
            recoilPivot = null;
            PrepareVisual();
        }

        private bool PrepareVisual()
        {
            if (visual == null) visual = GetComponent<DesertEagleVisual>();
            if (visual == null || wallTarget == null) return false;
            if (muzzle == null) muzzle = visual.LaunchPoint;
            if (muzzle == null) return false;
            if (recoilPivot == null)
            {
                recoilPivot = visual.RecoilPivot;
                if (recoilPivot == null) return false;
                recoilRest = recoilPivot.localPosition;
                recoilRotation = recoilPivot.localRotation;
            }
            return true;
        }

        private void Update()
        {
            if (!ShotManager.CanShoot || !PrepareVisual()) { CancelAim(); return; }
            Touchscreen screen = Touchscreen.current;
            if (screen != null)
            {
                foreach (var touch in screen.touches)
                {
                    int id = touch.touchId.ReadValue();
                    var phase = touch.phase.ReadValue();
                    Vector2 position = touch.position.ReadValue();
                    if (!dragging && phase == InputTouchPhase.Began) { BeginAim(position, id); return; }
                    if (!dragging || fingerId != id) continue;
                    if (phase == InputTouchPhase.Canceled) { CancelAim(); return; }
                    if (phase == InputTouchPhase.Ended) { ReleaseAim(position); return; }
                    if (phase == InputTouchPhase.Moved || phase == InputTouchPhase.Stationary)
                    { UpdateAim(position); return; }
                }
            }
            if (dragging && fingerId != -1) return;
            Mouse mouse = Mouse.current;
            if (mouse == null) return;
            Vector2 mousePosition = mouse.position.ReadValue();
            if (!dragging && mouse.leftButton.wasPressedThisFrame) BeginAim(mousePosition, -1);
            else if (dragging && mouse.leftButton.wasReleasedThisFrame) ReleaseAim(mousePosition);
            else if (dragging && mouse.leftButton.isPressed) UpdateAim(mousePosition);
        }

        private void BeginAim(Vector2 position, int id)
        {
            ResetRecoil();
            dragging = true;
            fingerId = id;
            dragStart = position;
            UpdateAim(position);
        }

        private Vector3 Aim(Vector2 position)
        {
            float limit = Mathf.Max(1f, maxDragDistance);
            Vector2 drag = Vector2.ClampMagnitude((dragStart - position) * aimSensitivity, limit);
            Vector3 target = wallTarget.position
                + wallTarget.right * (Mathf.Clamp(drag.x / limit, -1f, 1f) * maxHorizontalTargetOffset)
                + Vector3.up * Mathf.Lerp(minTargetHeight, maxTargetHeight, Mathf.Clamp01(drag.y / limit));
            // Aim about a stable pivot, then use the actual barrel axis for both arrows and bullet.
            Vector3 direction = target - recoilPivot.parent.position;
            if (direction.sqrMagnitude > 0.0001f) visual.SetAimVisual(direction.normalized);
            return muzzle.forward.normalized;
        }

        private void UpdateAim(Vector2 position)
        {
            Vector3 direction = Aim(position);
            if (arrows == null && !CreateArrows()) return;
            if (aimCamera == null) aimCamera = Camera.main;
            Vector3 view = aimCamera != null ? aimCamera.transform.position - muzzle.position : Vector3.up;
            Vector3 side = Vector3.Cross(direction, view).normalized;
            if (side.sqrMagnitude < 0.0001f) side = Vector3.Cross(direction, Vector3.up).normalized;
            if (side.sqrMagnitude < 0.0001f) side = Vector3.Cross(direction, Vector3.right).normalized;
            float length = Mathf.Min(3f, Vector3.Distance(muzzle.position, wallTarget.position));
            for (int i = 0; i < ArrowCount; i++)
            {
                float size = Mathf.Min(0.1f, length / 7f * 0.35f);
                Vector3 tip = muzzle.position + direction * ((i + 1f) * length / 7f);
                arrows[i].SetPosition(0, tip - direction * size + side * size * 0.65f);
                arrows[i].SetPosition(1, tip);
                arrows[i].SetPosition(2, tip - direction * size - side * size * 0.65f);
                arrows[i].enabled = length > 0.01f;
            }
        }

        private void ReleaseAim(Vector2 position)
        {
            if (!dragging) return;
            bool hasDrag = (dragStart - position).sqrMagnitude > 0.0001f;
            CancelAim();
            if (!hasDrag || !ShotManager.CanShoot || !PrepareVisual()) return;
            Vector3 direction = Aim(position);
            GameObject bulletObject = new GameObject("DesertEagleBullet");
            bulletObject.transform.SetPositionAndRotation(muzzle.position, Quaternion.LookRotation(direction));
            DesertEagleProjectile bullet = bulletObject.AddComponent<DesertEagleProjectile>();
            if (!bullet.Launch(direction, weaponRoot != null ? weaponRoot : transform, bulletSpeed))
            { Destroy(bulletObject); return; }
            // Launch and consumption are synchronous: physics cannot run between these calls.
            if (!ShotManager.Instance.ConsumeShot())
            { bulletObject.SetActive(false); Destroy(bulletObject); return; }
            WallBreaker.Effects.ProjectileVisualEffects.Attach(bulletObject, WeaponType.DesertEagle);
            WallBreaker.Effects.WeaponEffectsManager.Instance.Fire(WeaponType.DesertEagle, muzzle.position, direction);
            recoilRoutine = StartCoroutine(PlayRecoil());
        }

        private bool CreateArrows()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) return false;
            arrowMaterial = new Material(shader) { name = "Runtime_DesertEagleAim", color = Color.red };
            if (arrowMaterial.HasProperty("_BaseColor")) arrowMaterial.SetColor("_BaseColor", Color.red);
            arrows = new LineRenderer[ArrowCount];
            for (int i = 0; i < ArrowCount; i++)
            {
                GameObject arrowObject = new GameObject("DesertEagleAimArrow_" + i);
                arrowObject.transform.SetParent(transform, false);
                LineRenderer line = arrowObject.AddComponent<LineRenderer>();
                line.sharedMaterial = arrowMaterial;
                line.positionCount = 3;
                line.useWorldSpace = true;
                line.startWidth = line.endWidth = 0.025f;
                line.startColor = line.endColor = Color.red;
                line.shadowCastingMode = ShadowCastingMode.Off;
                line.receiveShadows = false;
                line.enabled = false;
                arrows[i] = line;
            }
            return true;
        }

        private IEnumerator PlayRecoil()
        {
            float elapsed = 0f;
            const float duration = 0.16f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float amount = Mathf.Sin(Mathf.Clamp01(elapsed / duration) * Mathf.PI);
                recoilPivot.localPosition = recoilRest + Vector3.back * (0.07f * amount);
                recoilPivot.localRotation = recoilRotation * Quaternion.Euler(-4f * amount, 0f, 0f);
                yield return null;
            }
            recoilPivot.localPosition = recoilRest;
            recoilPivot.localRotation = recoilRotation;
            recoilRoutine = null;
        }

        private void CancelAim()
        {
            dragging = false;
            fingerId = -1;
            if (arrows != null) foreach (LineRenderer arrow in arrows) arrow.enabled = false;
        }

        private void ResetRecoil()
        {
            if (recoilRoutine != null) { StopCoroutine(recoilRoutine); recoilRoutine = null; }
            if (recoilPivot == null) return;
            recoilPivot.localPosition = recoilRest;
            recoilPivot.localRotation = recoilRotation;
        }

        private void OnDisable() { CancelAim(); ResetRecoil(); }
        private void OnApplicationFocus(bool focus) { if (!focus) { CancelAim(); ResetRecoil(); } }
        private void OnDestroy() { if (arrowMaterial != null) Destroy(arrowMaterial); }
    }
}
