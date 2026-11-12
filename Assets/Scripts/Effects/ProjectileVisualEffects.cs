using UnityEngine;
using WallBreaker.Game;
using WallBreaker.Wall;

namespace WallBreaker.Effects
{
    [DisallowMultipleComponent]
    public sealed class ProjectileVisualEffects : MonoBehaviour
    {
        private WeaponType weapon;
        private WeaponEffectsManager manager;
        private TrailRenderer trail;
        private Rigidbody body;
        private Transform motor;
        private float nextImpact;
        private bool initialized;

        public static void Attach(GameObject projectile,WeaponType weapon)
        {
            ProjectileVisualEffects visual=projectile.GetComponent<ProjectileVisualEffects>();
            if(visual==null)visual=projectile.AddComponent<ProjectileVisualEffects>();
            visual.Initialize(weapon);
        }
        private void Initialize(WeaponType type)
        {
            if(initialized)return;initialized=true;weapon=type;body=GetComponent<Rigidbody>();manager=WeaponEffectsManager.Instance;
            bool rocket=type==WeaponType.Bazooka;
            Color color=rocket?new Color(0.46f,0.47f,0.49f,0.45f):type==WeaponType.DesertEagle?new Color(1,0.9f,0.58f,0.6f):new Color(0.72f,0.70f,0.65f,0.35f);
            trail=manager.BorrowTrail(color,rocket?0.40f:type==WeaponType.Crossbow?0.09f:0.12f,rocket?0.12f:0.025f);
            if(trail!=null){trail.transform.position=transform.position;trail.Clear();trail.emitting=true;}
            if(rocket && manager.SharedMaterial!=null)
            {
                // Keep ownership independent of projectile activation/destruction from the outset.
                // Follow() updates the world pose; teardown never needs to reparent a child.
                GameObject obj=new GameObject("RocketMotorVisual");obj.transform.SetParent(manager.transform,false);motor=obj.transform;
                motor.localScale=transform.lossyScale;
                ParticleSystem ps=obj.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var main=ps.main;main.playOnAwake=false;main.loop=true;main.startLifetime=0.08f;main.startSpeed=1.8f;main.startSize=0.12f;
                main.startColor=new Color(1,0.65f,0.16f);main.maxParticles=10;main.simulationSpace=ParticleSystemSimulationSpace.World;
                var emission=ps.emission;emission.rateOverTime=40f;
                var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=10f;shape.radius=0.02f;
                var fade=ps.colorOverLifetime;fade.enabled=true;
                Gradient gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(new Color(1,0.25f,0.02f),1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(0,1)});fade.color=gradient;
                var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,1,1,0));
                ParticleSystemRenderer renderer=obj.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=manager.SharedMaterial;
                renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
                GameObject smokeObject=new GameObject("RocketExhaustSmoke");smokeObject.transform.SetParent(motor,false);
                ParticleSystem smoke=smokeObject.AddComponent<ParticleSystem>();smoke.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var smokeMain=smoke.main;smokeMain.playOnAwake=false;smokeMain.loop=true;smokeMain.startLifetime=0.55f;
                smokeMain.startSpeed=0.55f;smokeMain.startSize=0.13f;smokeMain.startColor=new Color(0.40f,0.42f,0.44f,0.45f);
                smokeMain.maxParticles=16;smokeMain.simulationSpace=ParticleSystemSimulationSpace.World;smokeMain.gravityModifier=-0.04f;
                var smokeEmission=smoke.emission;smokeEmission.rateOverTime=16f;
                var smokeShape=smoke.shape;smokeShape.shapeType=ParticleSystemShapeType.Cone;smokeShape.angle=12f;smokeShape.radius=0.02f;
                var smokeFade=smoke.colorOverLifetime;smokeFade.enabled=true;smokeFade.color=gradient;
                var smokeSize=smoke.sizeOverLifetime;smokeSize.enabled=true;smokeSize.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,0.6f,1,2f));
                ParticleSystemRenderer smokeRenderer=smokeObject.GetComponent<ParticleSystemRenderer>();smokeRenderer.sharedMaterial=manager.SharedMaterial;
                smokeRenderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;smokeRenderer.receiveShadows=false;
                smoke.Play();
                Follow();ps.Play();
            }
        }
        private void LateUpdate(){Follow();}
        private void Follow()
        {
            if(trail!=null)trail.transform.position=transform.position;
            if(motor!=null)
            {
                Vector3 direction=body!=null && body.linearVelocity.sqrMagnitude>0.01f?body.linearVelocity.normalized:transform.forward;
                motor.SetPositionAndRotation(transform.position-direction*0.15f,Quaternion.LookRotation(-direction));
            }
        }
        private void OnCollisionEnter(Collision collision)
        {
            // Bazooka's existing exploded guard owns its one and only main impact visual.
            if(!initialized || weapon==WeaponType.Bazooka || manager==null || Time.fixedTime<nextImpact)return;
            nextImpact=Time.fixedTime+0.045f;
            bool metal=collision.collider.GetComponentInParent<LevelObstacle>()!=null;
            bool brick=collision.collider.GetComponentInParent<BrickPhysicsActivator>()!=null;
            bool ground=!metal && !brick && collision.collider.gameObject.name=="Ground";
            Vector3 point=collision.contactCount>0?collision.GetContact(0).point:transform.position;
            Vector3 normal=collision.contactCount>0?collision.GetContact(0).normal:Vector3.up;
            manager.Impact(weapon,point,normal,metal,brick,ground);
        }
        private void OnDisable()
        {
            ReleaseFlightEffects();
        }
        private void OnDestroy()
        {
            // Idempotent fallback if destruction happens before another lifecycle callback.
            ReleaseFlightEffects();
        }
        private void ReleaseFlightEffects()
        {
            if(manager!=null && trail!=null)manager.ReturnTrail(trail);trail=null;
            if(motor!=null)
            {
                // Already manager-owned: stop emission and let existing world-space smoke fade.
                motor.GetComponent<ParticleSystem>().Stop(true,ParticleSystemStopBehavior.StopEmitting);
                Destroy(motor.gameObject,0.75f);motor=null;
            }
        }
    }
}
