using System.Collections;
using UnityEngine;

namespace WallBreaker.Effects
{
    public class ImpactEffectManager : MonoBehaviour
    {
        [Header("Brick Flash")]
        [SerializeField] private Color flashColor = new Color(1f, 0.86f, 0.28f, 1f);
        [SerializeField] private float flashDuration = 0.1f;

        [Header("Particles")]
        [SerializeField] private Color particleColor = new Color(0.82f, 0.38f, 0.16f, 1f);
        [SerializeField] private int particleCount = 14;
        [SerializeField] private float particleLifeTime = 0.55f;
        [SerializeField] private float particleSpeed = 2.2f;
        [SerializeField] private float particleSize = 0.08f;

        [Header("Camera Shake")]
        [SerializeField] private float shakeDuration = 0.12f;
        [SerializeField] private float shakeStrength = 0.045f;

        private static ImpactEffectManager instance;
        private Material particleMaterial;
        private MaterialPropertyBlock propertyBlock;

        public static ImpactEffectManager Instance
        {
            get
            {
                if (instance == null)
                {
                    ImpactEffectManager existing = FindFirstObjectByType<ImpactEffectManager>();
                    instance = existing != null ? existing : new GameObject("ImpactEffectManager").AddComponent<ImpactEffectManager>();
                }

                return instance;
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
            propertyBlock = new MaterialPropertyBlock();
        }

        public void PlayBrickImpact(Vector3 position, Vector3 normal, GameObject brick, bool spawnParticles = true)
        {
            if (brick != null)
            {
                StartCoroutine(FlashBrick(brick));
            }

            if (spawnParticles) SpawnParticles(position, normal);
            ShakeCamera();
        }

        private IEnumerator FlashBrick(GameObject brick)
        {
            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }

            Renderer renderer = brick.GetComponent<Renderer>();
            if (renderer == null)
            {
                yield break;
            }

            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", flashColor);
            propertyBlock.SetColor("_Color", flashColor);
            renderer.SetPropertyBlock(propertyBlock);

            yield return new WaitForSeconds(flashDuration);

            if (renderer != null) renderer.SetPropertyBlock(null);
        }

        private void SpawnParticles(Vector3 position, Vector3 normal)
        {
            GameObject particleObject = new GameObject("BrickImpactParticles");
            particleObject.transform.position = position + normal * 0.05f;
            particleObject.transform.rotation = Quaternion.LookRotation(normal);

            ParticleSystem particleSystem = particleObject.AddComponent<ParticleSystem>();
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = particleSystem.main;
            main.playOnAwake = false;
            main.duration = 0.08f;
            main.loop = false;
            main.startLifetime = particleLifeTime;
            main.startSpeed = particleSpeed;
            main.startSize = particleSize;
            main.startColor = particleColor;
            main.maxParticles = particleCount;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)particleCount) });

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.15f;
            shape.angle = 35f;

            ParticleSystemRenderer renderer = particleObject.GetComponent<ParticleSystemRenderer>();
            renderer.material = GetParticleMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            particleSystem.Play();
            Destroy(particleObject, particleLifeTime + 0.2f);
        }

        private Material GetParticleMaterial()
        {
            if (particleMaterial != null)
            {
                return particleMaterial;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Particles/Standard Unlit");
            }

            particleMaterial = new Material(shader)
            {
                name = "Runtime_BrickImpactParticle",
                color = particleColor
            };

            if (particleMaterial.HasProperty("_BaseColor"))
            {
                particleMaterial.SetColor("_BaseColor", particleColor);
            }

            return particleMaterial;
        }

        private void OnDestroy()
        {
            if (particleMaterial != null) Destroy(particleMaterial);
            if (instance == this) instance = null;
        }

        private void ShakeCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            CameraShake cameraShake = mainCamera.GetComponent<CameraShake>();
            if (cameraShake == null)
            {
                cameraShake = mainCamera.gameObject.AddComponent<CameraShake>();
            }

            cameraShake.Shake(shakeDuration, shakeStrength);
        }
    }
}
