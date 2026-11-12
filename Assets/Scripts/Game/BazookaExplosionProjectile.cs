using UnityEngine;
using WallBreaker.Audio;
using WallBreaker.Wall;

namespace WallBreaker.Game
{
    public class BazookaExplosionProjectile : MonoBehaviour
    {
        private const float MaxBalancedExplosionRadius = 1.05f;

        [SerializeField] private float explosionRadius = MaxBalancedExplosionRadius;
        [SerializeField] private float explosionForce = 8.5f;
        [SerializeField] private float upwardModifier = 0.35f;

        private bool exploded;


        public void Configure(float radius, float force, float upwardForce)
        {
            explosionRadius = MaxBalancedExplosionRadius;
            explosionForce = force;
            upwardModifier = upwardForce;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (exploded)
            {
                return;
            }

            Vector3 impactPoint = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            Vector3 impactNormal = collision.contactCount > 0 ? collision.GetContact(0).normal : -transform.forward;
            if (collision.collider.GetComponentInParent<LevelObstacle>() != null)
            {
                // Metal impact detonates rather than continuing as a ricocheting live rocket.
                Rigidbody body = GetComponent<Rigidbody>();
                if (body != null)
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                    body.isKinematic = true;
                    body.detectCollisions = false;
                }
            }
            Explode(impactPoint, impactNormal);
        }

        private void Explode(Vector3 center, Vector3 normal)
        {
            exploded = true;
            bool hasMetalObstacles = FindFirstObjectByType<LevelObstacle>() != null;
            Vector3 sightOrigin = center + normal.normalized * 0.02f;

            Collider[] hits = Physics.OverlapSphere(center, explosionRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].GetComponentInParent<LevelObstacle>() != null) continue;
                Rigidbody hitRigidbody = hits[i].attachedRigidbody;
                if (hitRigidbody == null)
                {
                    continue;
                }
                if (hasMetalObstacles && IsBlockedByMetal(sightOrigin, hits[i].ClosestPoint(sightOrigin))) continue;

                BrickPhysicsActivator brick = hitRigidbody.GetComponent<BrickPhysicsActivator>();
                if (brick != null)
                {
                    brick.Activate();
                }
                else if (hitRigidbody.isKinematic)
                {
                    hitRigidbody.isKinematic = false;
                }

                Vector3 closestPoint = hits[i].ClosestPoint(center);
                Vector3 direction = (closestPoint - center).normalized;
                if (direction.sqrMagnitude <= 0.0001f)
                {
                    direction = (hitRigidbody.worldCenterOfMass - center).normalized;
                }

                float distance = Vector3.Distance(center, hitRigidbody.worldCenterOfMass);
                float normalizedDistance = Mathf.Clamp01(distance / explosionRadius);
                float falloff = 1f - Mathf.SmoothStep(0f, 1f, normalizedDistance);
                Vector3 force = (direction + Vector3.up * upwardModifier).normalized * (explosionForce * falloff);

                hitRigidbody.WakeUp();
                hitRigidbody.AddForceAtPosition(force, center, ForceMode.Impulse);
            }

            SpawnExplosionEffect(center, normal);
            GameAudioManager.Instance.PlayImpact();
            Destroy(gameObject, 0.03f);
        }

        private bool IsBlockedByMetal(Vector3 origin, Vector3 target)
        {
            Vector3 delta = target - origin;
            float distance = delta.magnitude;
            if (distance < 0.0001f) return false;
            // Filter only obstacle colliders: bricks, triggers and the rocket never shield other bricks.
            RaycastHit[] blockers = Physics.RaycastAll(origin, delta / distance, distance,
                Physics.AllLayers, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit hit in blockers)
            {
                if (hit.collider.transform.IsChildOf(transform)) continue;
                if (hit.collider.GetComponentInParent<LevelObstacle>() != null) return true;
            }
            return false;
        }

        private void SpawnExplosionEffect(Vector3 center, Vector3 normal)
        {
            WallBreaker.Effects.WeaponEffectsManager.Instance.Explosion(center, normal);
        }


    }
}
