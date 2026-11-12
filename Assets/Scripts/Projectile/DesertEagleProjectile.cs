using UnityEngine;
using WallBreaker.Wall;

namespace WallBreaker.Projectile
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class DesertEagleProjectile : MonoBehaviour
    {
        [SerializeField, Range(1f, 100f)] private float speed = 35f;
        [SerializeField, Range(0f, 10f)] private float impactImpulse = 4f;
        [SerializeField, Range(0.1f, 10f)] private float lifeTime = 3f;

        private Rigidbody projectileBody;
        private Collider[] projectileColliders;
        private Transform weaponRoot;
        private Vector3 launchDirection;
        private bool launched;
        private bool impactHandled;
        private bool bouncedFromObstacle;

        private void Awake()
        {
            projectileBody = GetComponent<Rigidbody>();
            projectileBody.mass = 0.02f;
            projectileBody.useGravity = false;
            projectileBody.isKinematic = true;
            projectileBody.interpolation = RigidbodyInterpolation.Interpolate;
            SphereCollider sphere = GetComponent<SphereCollider>();
            sphere.radius = 0.065f;
            sphere.isTrigger = false;

            // One small primitive and its default shared material; no per-shot material allocation.
            Transform visual = transform.Find("BulletVisual");
            if (visual == null)
            {
                GameObject visualObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visual = visualObject.transform;
                visual.name = "BulletVisual";
                visual.SetParent(transform, false);
                Collider visualCollider = visualObject.GetComponent<Collider>();
                visualCollider.enabled = false;
                Destroy(visualCollider);
            }
            visual.localPosition = Vector3.zero;
            visual.localRotation = Quaternion.identity;
            visual.localScale = new Vector3(0.10f, 0.10f, 0.22f);
            projectileColliders = GetComponentsInChildren<Collider>(true);
            Destroy(gameObject, Mathf.Clamp(lifeTime, 0.1f, 10f));
        }

        /// <summary>
        /// Launch once from the current world position. Pass the complete weapon root so
        /// all of its colliders are ignored. A negative speedOverride uses the Inspector speed.
        /// Use a standalone projectile object without the shared Projectile component.
        /// </summary>
        public bool Launch(Vector3 direction, Transform ownerRoot, float speedOverride = -1f)
        {
            if (launched || impactHandled || ownerRoot == null || direction.sqrMagnitude <= 0.0001f
                || !IsFinite(direction.x) || !IsFinite(direction.y) || !IsFinite(direction.z)
                || !IsFinite(speedOverride) || GetComponent<Projectile>() != null)
                return false;

            weaponRoot = ownerRoot;
            Collider[] ownerColliders = ownerRoot.GetComponentsInChildren<Collider>(true);
            foreach (Collider ownCollider in projectileColliders)
            {
                if (ownCollider == null) continue;
                foreach (Collider ownerCollider in ownerColliders)
                {
                    if (ownerCollider != null && ownerCollider != ownCollider)
                        Physics.IgnoreCollision(ownCollider, ownerCollider, true);
                }
            }

            launchDirection = direction.normalized;
            transform.rotation = Quaternion.LookRotation(launchDirection);
            projectileBody.isKinematic = false;
            projectileBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            projectileBody.angularVelocity = Vector3.zero;
            projectileBody.linearVelocity = launchDirection * Mathf.Clamp(speedOverride < 0f ? speed : speedOverride, 1f, 100f);
            launched = true;
            return true;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!launched || impactHandled) return;
            if (weaponRoot != null && collision.collider.transform.IsChildOf(weaponRoot)) return;

            LevelObstacle obstacle = collision.collider.GetComponentInParent<LevelObstacle>();
            if (obstacle != null && obstacle.AllowsDesertEagleBounce)
            {
                bouncedFromObstacle = true;
                // Restitution is supplied by the panel material; keep the projectile alive and dynamic.
                // Preserve the reflected direction for any later brick impulse.
                if (collision.contactCount > 0)
                    launchDirection = Vector3.Reflect(collision.relativeVelocity, collision.GetContact(0).normal).normalized;
                return;
            }

            // Guard before activation: multiple contacts and completion callbacks must not repeat the hit.
            impactHandled = true;
            foreach (Collider ownCollider in projectileColliders)
                if (ownCollider != null) ownCollider.enabled = false;

            Rigidbody hitBody = collision.rigidbody;
            BrickPhysicsActivator brick = hitBody != null
                ? hitBody.GetComponent<BrickPhysicsActivator>()
                : collision.collider.GetComponentInParent<BrickPhysicsActivator>();
            if (brick != null)
            {
                brick.Activate();
                Rigidbody brickBody = brick.GetComponent<Rigidbody>();
                if (brickBody != null && !brickBody.isKinematic)
                {
                    Vector3 point = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
                    Vector3 impactDirection = bouncedFromObstacle && collision.relativeVelocity.sqrMagnitude > 0.0001f
                        ? collision.relativeVelocity.normalized : launchDirection;
                    brickBody.AddForceAtPosition(impactDirection * Mathf.Clamp(impactImpulse, 0f, 10f), point, ForceMode.Impulse);
                }
            }
            // Static scenery also stops the projectile; unrelated rigidbodies are not activated.
            projectileBody.linearVelocity = Vector3.zero;
            Destroy(gameObject);
        }
    }
}
