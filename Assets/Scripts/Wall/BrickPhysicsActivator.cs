using UnityEngine;
using System;
using WallBreaker.Audio;

namespace WallBreaker.Wall
{
    [RequireComponent(typeof(Rigidbody))]
    public class BrickPhysicsActivator : MonoBehaviour
    {
        [SerializeField] private float activationImpulse = 0.2f;

        public static event Action<BrickPhysicsActivator> BrickActivated;
        public bool IsActivated => activated;

        private Rigidbody brickRigidbody;
        private bool activated;

        public void Initialize(float impulseToActivate)
        {
            activationImpulse = impulseToActivate;
        }

        private void Awake()
        {
            brickRigidbody = GetComponent<Rigidbody>();
            brickRigidbody.isKinematic = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (activated)
            {
                return;
            }

            if (collision.impulse.magnitude < activationImpulse)
            {
                return;
            }

            Activate();
        }

        public void Activate()
        {
            if (activated)
            {
                return;
            }

            activated = true;
            brickRigidbody.isKinematic = false;
            brickRigidbody.WakeUp();
            BrickActivated?.Invoke(this);
            GameAudioManager.Instance.PlayBrickBreak();
        }
    }
}
