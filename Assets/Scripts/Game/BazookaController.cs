using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using WallBreaker.Audio;
using WallBreaker.Visuals;
using InputTouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace WallBreaker.Game
{
    public class BazookaController : MonoBehaviour
    {
        private const float MaxBalancedExplosionRadius = 1.05f;

        [Header("References")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform launchPoint;
        [SerializeField] private Transform wallTarget;
        [SerializeField] private BazookaVisual bazookaVisual;

        [Header("Launch Settings")]
        [SerializeField] private float launchPower = 23f;
        [SerializeField] private float maxDragDistance = 320f;
        [SerializeField] private float maxHorizontalTargetOffset = 3.05f;
        [SerializeField] private float minTargetHeight = 0.15f;
        [SerializeField] private float maxTargetHeight = 3.75f;

        [Header("Explosion Settings")]
        [SerializeField] private float explosionRadius = MaxBalancedExplosionRadius;
        [SerializeField] private float explosionForce = 8.5f;
        [SerializeField] private float explosionUpwardModifier = 0.35f;

        private bool isDragging;
        private int activeFingerId = -1;
        private Vector2 dragStartPosition;
        private Vector2 currentDragPosition;

        private void Awake()
        {
            explosionRadius = MaxBalancedExplosionRadius;
        }

        public void Initialize(GameObject newProjectilePrefab, Transform newLaunchPoint, Transform newWallTarget)
        {
            projectilePrefab = newProjectilePrefab;
            launchPoint = newLaunchPoint;
            wallTarget = newWallTarget;
            bazookaVisual = GetComponent<BazookaVisual>();
        }

        private void Update()
        {
            if (LevelManager.IsLevelComplete || ShotManager.IsInputLocked)
            {
                CancelDrag();
                return;
            }

            if (!HandleTouchInput())
            {
                HandleMouseInput();
            }
        }

        private bool HandleTouchInput()
        {
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null)
            {
                return false;
            }

            foreach (TouchControl touch in touchscreen.touches)
            {
                int touchId = touch.touchId.ReadValue();
                InputTouchPhase phase = touch.phase.ReadValue();
                Vector2 position = touch.position.ReadValue();

                if (phase == InputTouchPhase.Began && !isDragging)
                {
                    BeginDrag(position, touchId);
                    return true;
                }

                if (touchId != activeFingerId)
                {
                    continue;
                }

                if (phase == InputTouchPhase.Moved || phase == InputTouchPhase.Stationary)
                {
                    UpdateDrag(position);
                    return true;
                }

                if (phase == InputTouchPhase.Ended || phase == InputTouchPhase.Canceled)
                {
                    ReleaseDrag(position);
                    return true;
                }
            }

            return false;
        }

        private void HandleMouseInput()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            Vector2 mousePosition = mouse.position.ReadValue();

            if (mouse.leftButton.wasPressedThisFrame)
            {
                BeginDrag(mousePosition, -1);
            }
            else if (mouse.leftButton.isPressed && isDragging)
            {
                UpdateDrag(mousePosition);
            }
            else if (mouse.leftButton.wasReleasedThisFrame && isDragging)
            {
                ReleaseDrag(mousePosition);
            }
        }

        private void BeginDrag(Vector2 screenPosition, int fingerId)
        {
            isDragging = true;
            activeFingerId = fingerId;
            dragStartPosition = screenPosition;
            currentDragPosition = screenPosition;
        }

        private void UpdateDrag(Vector2 screenPosition)
        {
            currentDragPosition = screenPosition;
            UpdateAimVisual(dragStartPosition - currentDragPosition);
        }

        private void ReleaseDrag(Vector2 screenPosition)
        {
            currentDragPosition = screenPosition;
            Vector2 dragVector = dragStartPosition - currentDragPosition;
            CancelDrag();

            if (dragVector.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            FireProjectile(dragVector);
        }

        private void CancelDrag()
        {
            isDragging = false;
            activeFingerId = -1;
        }

        private void FireProjectile(Vector2 dragVector)
        {
            if (!ShotManager.CanShoot || projectilePrefab == null || launchPoint == null || wallTarget == null)
            {
                return;
            }

            Vector3 launchDirection = CalculateLaunchDirection(dragVector);
            GameObject projectileObject = Instantiate(projectilePrefab, launchPoint.position, Quaternion.LookRotation(launchDirection));
            Rigidbody projectileRigidbody = projectileObject.GetComponent<Rigidbody>();
            if (projectileRigidbody == null)
            {
                Destroy(projectileObject);
                return;
            }

            BazookaExplosionProjectile explosionProjectile = projectileObject.GetComponent<BazookaExplosionProjectile>();
            if (explosionProjectile == null)
            {
                explosionProjectile = projectileObject.AddComponent<BazookaExplosionProjectile>();
            }

            explosionProjectile.Configure(explosionRadius, explosionForce, explosionUpwardModifier);

            projectileRigidbody.isKinematic = false;
            projectileRigidbody.linearVelocity = Vector3.zero;
            projectileRigidbody.angularVelocity = Vector3.zero;
            projectileRigidbody.AddForce(launchDirection * launchPower, ForceMode.Impulse);

            ShotManager.Instance.ConsumeShot();
            WallBreaker.Effects.ProjectileVisualEffects.Attach(projectileObject, WeaponType.Bazooka);
            GetBazookaVisual()?.PlayFireFeedback();
            GameAudioManager.Instance.PlayLaunch();
        }

        private void UpdateAimVisual(Vector2 dragVector)
        {
            BazookaVisual visual = GetBazookaVisual();
            if (visual == null || launchPoint == null || wallTarget == null)
            {
                return;
            }

            visual.SetAimVisual(CalculateLaunchDirection(dragVector));
        }

        private Vector3 CalculateLaunchDirection(Vector2 dragVector)
        {
            float effectiveMaxDragDistance = Mathf.Max(1f, maxDragDistance);
            Vector2 clampedDragVector = Vector2.ClampMagnitude(dragVector, effectiveMaxDragDistance);
            float normalizedX = Mathf.Clamp(clampedDragVector.x / effectiveMaxDragDistance, -1f, 1f);
            float normalizedY = Mathf.Clamp01(clampedDragVector.y / effectiveMaxDragDistance);

            Vector3 targetPoint =
                wallTarget.position +
                wallTarget.right * (normalizedX * maxHorizontalTargetOffset) +
                Vector3.up * Mathf.Lerp(minTargetHeight, maxTargetHeight, normalizedY);

            return (targetPoint - launchPoint.position).normalized;
        }

        private BazookaVisual GetBazookaVisual()
        {
            if (bazookaVisual == null)
            {
                bazookaVisual = GetComponent<BazookaVisual>();
            }

            return bazookaVisual;
        }
    }
}
