using UnityEngine;
using WallBreaker.Audio;
using WallBreaker.Effects;
using WallBreaker.Wall;

namespace WallBreaker.Projectile
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float impactImpulse = 4f;
        [SerializeField] private float lifeTime = 5f;

        private Rigidbody projectileRigidbody;
        private bool impactEffectPlayed;

        private void Awake()
        {
            projectileRigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.GetComponentInParent<LevelObstacle>() != null)
            {
                // Static panel material handles restitution using the incoming velocity and contact normal.
                return;
            }

            Rigidbody hitRigidbody = collision.rigidbody;
            if (hitRigidbody == null)
            {
                return;
            }

            BrickPhysicsActivator brick = hitRigidbody.GetComponent<BrickPhysicsActivator>();
            if (brick != null)
            {
                brick.Activate();
                PlayImpactEffectOnce(collision, brick.gameObject);
            }
            else if (hitRigidbody.isKinematic)
            {
                hitRigidbody.isKinematic = false;
            }

            Vector3 direction = projectileRigidbody.linearVelocity.sqrMagnitude > 0.01f
                ? projectileRigidbody.linearVelocity.normalized
                : transform.forward;

            Vector3 hitPoint = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            hitRigidbody.WakeUp();
            hitRigidbody.AddForceAtPosition(direction * impactImpulse, hitPoint, ForceMode.Impulse);
        }

        private void PlayImpactEffectOnce(Collision collision, GameObject brick)
        {
            if (impactEffectPlayed || collision.contactCount == 0)
            {
                return;
            }

            impactEffectPlayed = true;
            ContactPoint contact = collision.GetContact(0);
            bool hasVisualEffects = GetComponent<ProjectileVisualEffects>() != null;
            ImpactEffectManager.Instance.PlayBrickImpact(contact.point, contact.normal, brick, !hasVisualEffects);
            // The explosive component already owns the rocket's impact sound.
            if (GetComponent<WallBreaker.Game.BazookaExplosionProjectile>() == null)
                GameAudioManager.Instance.PlayImpact();
        }
    }
}
