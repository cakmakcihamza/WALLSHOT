using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using WallBreaker.Audio;
using WallBreaker.Game;
using WallBreaker.Visual;
using InputTouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace WallBreaker.Player
{
    public class SlingshotController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform launchPoint;
        [SerializeField] private Transform wallTarget;
        [SerializeField] private SlingshotVisual slingshotVisual;

        [Header("Launch Settings")]
        [SerializeField] private float dragPower = 18f;
        [SerializeField] private float maxDragDistance = 300f;
        [SerializeField] private float horizontalAimStrength = 1.5f;
        [SerializeField] private float verticalAimStrength = 2f;
        [SerializeField] private float maxHorizontalTargetOffset = 2.2f;
        [SerializeField] private float minTargetHeight = 0.25f;
        [SerializeField] private float maxTargetHeight = 3f;

        private bool isDragging;
        private int activeFingerId = -1;
        private Vector2 dragStartPosition;
        private Vector2 currentDragPosition;

        private void Awake()
        {
            if (slingshotVisual == null)
            {
                slingshotVisual = GetComponent<SlingshotVisual>();
            }
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
            GetSlingshotVisual()?.ResetVisual();
        }

        private void UpdateDrag(Vector2 screenPosition)
        {
            currentDragPosition = screenPosition;
            GetSlingshotVisual()?.SetDrag(dragStartPosition - currentDragPosition, maxDragDistance);
        }

        private void ReleaseDrag(Vector2 screenPosition)
        {
            currentDragPosition = screenPosition;

            Vector2 dragVector = dragStartPosition - currentDragPosition;
            isDragging = false;
            activeFingerId = -1;

            if (dragVector.sqrMagnitude <= 0.0001f)
            {
                GetSlingshotVisual()?.ResetVisual();
                return;
            }

            GetSlingshotVisual()?.PlayRelease();
            LaunchProjectile(dragVector);
        }

        private void CancelDrag()
        {
            if (!isDragging)
            {
                return;
            }

            isDragging = false;
            activeFingerId = -1;
            GetSlingshotVisual()?.ResetVisual();
        }

        private SlingshotVisual GetSlingshotVisual()
        {
            if (slingshotVisual == null)
            {
                slingshotVisual = GetComponent<SlingshotVisual>();
            }

            return slingshotVisual;
        }

        private void LaunchProjectile(Vector2 dragVector)
        {
            if (!ShotManager.CanShoot)
            {
                return;
            }

            if (projectilePrefab == null || launchPoint == null || wallTarget == null)
            {
                Debug.LogWarning("SlingshotController needs Projectile Prefab, Launch Point, and Wall Target.");
                return;
            }

            float effectiveMaxDragDistance = Mathf.Max(1f, maxDragDistance);
            Vector2 clampedDragVector = Vector2.ClampMagnitude(dragVector, effectiveMaxDragDistance);
            float normalizedX = Mathf.Clamp(clampedDragVector.x / effectiveMaxDragDistance, -1f, 1f);
            float normalizedY = Mathf.Clamp01(clampedDragVector.y / effectiveMaxDragDistance);
            float horizontalAim = Mathf.Clamp(normalizedX * horizontalAimStrength, -1f, 1f);
            float verticalAim = Mathf.Clamp01(normalizedY * verticalAimStrength);
            bool obstacleLevel = WeaponManager.CurrentStage == WeaponStage.Slingshot && LevelManager.CurrentLevel == 6;
            float horizontalLimit = obstacleLevel ? Mathf.Max(maxHorizontalTargetOffset, 3.2f) : maxHorizontalTargetOffset;
            float heightLimit = obstacleLevel ? Mathf.Max(maxTargetHeight, 4.2f) : maxTargetHeight;
            float horizontalOffset = horizontalAim * horizontalLimit;
            float targetHeight = Mathf.Lerp(minTargetHeight, heightLimit, verticalAim);
            Vector3 targetPoint =
                wallTarget.position +
                wallTarget.right * horizontalOffset +
                Vector3.up * targetHeight;
            Vector3 launchDirection = (targetPoint - launchPoint.position).normalized;

            GameObject projectileObject = Instantiate(
                projectilePrefab,
                launchPoint.position,
                Quaternion.LookRotation(launchDirection)
            );

            Rigidbody projectileRigidbody = projectileObject.GetComponent<Rigidbody>();
            if (projectileRigidbody == null)
            {
                Debug.LogWarning("Projectile Prefab needs a Rigidbody component.");
                Destroy(projectileObject);
                return;
            }

            projectileRigidbody.isKinematic = false;
            projectileRigidbody.linearVelocity = Vector3.zero;
            projectileRigidbody.angularVelocity = Vector3.zero;

            float launchImpulse = clampedDragVector.magnitude / effectiveMaxDragDistance * dragPower;
            projectileRigidbody.AddForce(launchDirection * launchImpulse, ForceMode.Impulse);
            ShotManager.Instance.ConsumeShot();
            WallBreaker.Effects.ProjectileVisualEffects.Attach(projectileObject, WeaponType.Slingshot);
            WallBreaker.Effects.WeaponEffectsManager.Instance.Fire(WeaponType.Slingshot, launchPoint.position, launchDirection);
            GameAudioManager.Instance.PlayLaunch();
        }
    }
}
