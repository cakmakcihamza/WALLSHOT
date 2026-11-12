using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using WallBreaker.Game;

namespace WallBreaker.Effects
{
    public sealed class WeaponEffectsManager : MonoBehaviour
    {
        private sealed class BurstSlot { public ParticleSystem Particles; public float Until; }
        private sealed class TrailSlot { public TrailRenderer Trail; public bool Borrowed; public float Until; }
        private sealed class WaveSlot { public LineRenderer Line; public float Start; public float Until; public Vector3 Center; public Quaternion Rotation; public readonly Vector3[] Points = new Vector3[33]; }
        private static WeaponEffectsManager instance;
        private readonly List<BurstSlot> bursts = new List<BurstSlot>();
        private readonly List<TrailSlot> trails = new List<TrailSlot>();
        private readonly List<WaveSlot> waves = new List<WaveSlot>();
        private Material material;
        private Texture2D texture;
        public Material SharedMaterial { get { EnsureMaterial(); return material; } }

        public static WeaponEffectsManager Instance
        {
            get
            {
                if (instance == null) instance = new GameObject("WeaponEffectsManager").AddComponent<WeaponEffectsManager>();
                return instance;
            }
        }

        private void EnsureMaterial()
        {
            if (material != null) return;
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) return;
            texture = new Texture2D(32,32,TextureFormat.RGBA32,false);
            texture.name = "Runtime_SoftParticle";
            texture.wrapMode = TextureWrapMode.Clamp;
            Color[] pixels = new Color[1024];
            for(int y=0;y<32;y++) for(int x=0;x<32;x++)
            {
                float radius = new Vector2((x-15.5f)/15.5f,(y-15.5f)/15.5f).magnitude;
                pixels[y*32+x] = new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1f-radius),1.5f));
            }
            texture.SetPixels(pixels); texture.Apply(false,true);
            material = new Material(shader) { name = "Runtime_WeaponEffects", renderQueue = 3000 };
            if(material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap",texture);
            if(material.HasProperty("_MainTex")) material.SetTexture("_MainTex",texture);
            if(material.HasProperty("_BaseColor")) material.SetColor("_BaseColor",Color.white);
            if(material.HasProperty("_Color")) material.SetColor("_Color",Color.white);
            SetFloat("_Surface",1); SetFloat("_Blend",0); SetFloat("_ZWrite",0);
            SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha); SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
        private void SetFloat(string property,float value) { if(material.HasProperty(property)) material.SetFloat(property,value); }

        public void Burst(Vector3 position, Vector3 direction, Color color, int count, float lifetime, float speed, float size, bool smoke = false)
        {
            if(SharedMaterial == null) return;
            BurstSlot slot = null;
            foreach(BurstSlot candidate in bursts) if(candidate.Until <= Time.time) { slot=candidate; break; }
            if(slot==null && bursts.Count < 32)
            {
                GameObject obj = new GameObject("PooledWeaponBurst"); obj.transform.SetParent(transform,false);
                ParticleSystem ps = obj.AddComponent<ParticleSystem>();
                ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var main = ps.main; main.playOnAwake=false; main.loop=false; main.duration=0.1f;
                main.maxParticles=40; main.simulationSpace=ParticleSystemSimulationSpace.World;
                var emission=ps.emission; emission.enabled=false;
                var collision=ps.collision; collision.enabled=false;
                var lights=ps.lights; lights.enabled=false;
                var fade=ps.colorOverLifetime; fade.enabled=true;
                Gradient gradient=new Gradient();
                gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},
                    new[]{new GradientAlphaKey(0.95f,0),new GradientAlphaKey(0.65f,0.25f),new GradientAlphaKey(0,1)});
                fade.color=gradient;
                ParticleSystemRenderer renderer=obj.GetComponent<ParticleSystemRenderer>();
                renderer.sharedMaterial=material; renderer.shadowCastingMode=ShadowCastingMode.Off; renderer.receiveShadows=false;
                slot=new BurstSlot { Particles=ps }; bursts.Add(slot);
            }
            if(slot==null) { slot=bursts[0]; foreach(BurstSlot candidate in bursts) if(candidate.Until<slot.Until)slot=candidate; }
            ParticleSystem particles=slot.Particles;
            particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            particles.transform.SetPositionAndRotation(position,Quaternion.LookRotation(direction.sqrMagnitude>0.0001f?direction:Vector3.up));
            var settings=particles.main; settings.startLifetime=lifetime; settings.startSpeed=new ParticleSystem.MinMaxCurve(speed*0.5f,speed);
            settings.startSize=new ParticleSystem.MinMaxCurve(size*0.5f,size); settings.startColor=color; settings.gravityModifier=smoke?-0.08f:0.12f;
            var shape=particles.shape; shape.enabled=true; shape.shapeType=ParticleSystemShapeType.Cone; shape.angle=smoke?32f:24f; shape.radius=smoke?0.09f:0.025f;
            var scale=particles.sizeOverLifetime; scale.enabled=true;
            scale.size=new ParticleSystem.MinMaxCurve(1f,smoke?AnimationCurve.Linear(0,0.6f,1,2.2f):AnimationCurve.Linear(0,1,1,0.15f));
            particles.Play(); particles.Emit(Mathf.Clamp(count,1,40)); slot.Until=Time.time+lifetime+0.15f;
        }

        public void Fire(WeaponType weapon, Vector3 position, Vector3 direction)
        {
            if(weapon==WeaponType.Slingshot || weapon==WeaponType.Crossbow)
            {
                Burst(position,direction,new Color(0.75f,0.69f,0.55f,0.5f),weapon==WeaponType.Slingshot?4:3,0.16f,1.1f,0.08f);
                return;
            }
            float size=weapon==WeaponType.DesertEagle?0.22f:0.38f;
            Burst(position,direction,new Color(1f,0.75f,0.25f),weapon==WeaponType.DesertEagle?7:12,0.08f,2.5f,size);
            Burst(position,direction,new Color(0.46f,0.48f,0.50f,0.45f),weapon==WeaponType.Cannon?10:5,
                weapon==WeaponType.Cannon?0.85f:0.45f,0.7f,weapon==WeaponType.Cannon?0.28f:0.15f,true);
        }

        public void Impact(WeaponType weapon, Vector3 point, Vector3 normal, bool metal, bool brick, bool ground)
        {
            bool heavy=weapon==WeaponType.Cannon;
            if(metal)
            {
                Burst(point,normal,new Color(1f,0.82f,0.38f),heavy?12:weapon==WeaponType.DesertEagle?9:4,0.20f,heavy?3.5f:2.3f,0.045f);
                Burst(point,normal,new Color(0.55f,0.57f,0.59f,0.45f),3,0.3f,0.5f,0.13f,true);
            }
            else
            {
                Color dust=brick?new Color(0.73f,0.40f,0.23f,0.7f):ground?new Color(0.62f,0.57f,0.46f,0.6f):new Color(0.55f,0.56f,0.58f,0.5f);
                Burst(point,normal,dust,heavy?16:weapon==WeaponType.Crossbow?4:8,heavy?0.55f:0.32f,heavy?2.1f:1.2f,heavy?0.22f:0.12f,true);
                if(brick) Burst(point,normal,new Color(0.65f,0.34f,0.19f),heavy?7:3,0.25f,2f,0.045f);
            }
        }

        public void Explosion(Vector3 center,Vector3 normal)
        {
            Burst(center,normal,new Color(1,0.92f,0.62f),12,0.10f,3f,0.5f);
            Burst(center,normal,new Color(1,0.37f,0.07f),24,0.35f,3.2f,0.48f);
            Burst(center,Vector3.up,new Color(0.22f,0.23f,0.25f,0.65f),16,1.1f,1.5f,0.55f,true);
            if(SharedMaterial==null)return;
            WaveSlot slot=null;
            foreach(WaveSlot wave in waves) if(wave.Until<=Time.time){slot=wave;break;}
            if(slot==null && waves.Count<4)
            {
                GameObject obj=new GameObject("VisualShockwave");obj.transform.SetParent(transform,false);
                LineRenderer line=obj.AddComponent<LineRenderer>();line.positionCount=33;line.useWorldSpace=true;
                line.sharedMaterial=material;line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;
                slot=new WaveSlot{Line=line};waves.Add(slot);
            }
            if(slot==null)return;
            slot.Center=center;slot.Start=Time.time;slot.Until=Time.time+0.35f;
            Camera camera=Camera.main;slot.Rotation=camera!=null?camera.transform.rotation:Quaternion.identity;
            slot.Line.enabled=true;
        }

        public TrailRenderer BorrowTrail(Color color,float duration,float width)
        {
            if(SharedMaterial==null)return null;
            TrailSlot slot=null;
            foreach(TrailSlot candidate in trails)if(!candidate.Borrowed && candidate.Until<=Time.time){slot=candidate;break;}
            if(slot==null && trails.Count<24)
            {
                GameObject obj=new GameObject("PooledProjectileTrail");obj.transform.SetParent(transform,false);
                TrailRenderer trail=obj.AddComponent<TrailRenderer>();trail.sharedMaterial=material;
                trail.shadowCastingMode=ShadowCastingMode.Off;trail.receiveShadows=false;trail.minVertexDistance=0.05f;
                slot=new TrailSlot{Trail=trail};trails.Add(slot);
            }
            if(slot==null)return null;
            slot.Borrowed=true;slot.Trail.emitting=false;slot.Trail.Clear();
            slot.Trail.time=duration;slot.Trail.startWidth=width;slot.Trail.endWidth=0;
            slot.Trail.startColor=color;slot.Trail.endColor=new Color(color.r,color.g,color.b,0);
            return slot.Trail;
        }
        public void ReturnTrail(TrailRenderer trail)
        {
            if(trail==null)return;
            foreach(TrailSlot slot in trails) if(slot.Trail==trail)
            {trail.emitting=false;slot.Borrowed=false;slot.Until=Time.time+trail.time;return;}
        }
        private void Update()
        {
            foreach(WaveSlot wave in waves)
            {
                if(wave.Until<=Time.time){wave.Line.enabled=false;continue;}
                float t=Mathf.Clamp01((Time.time-wave.Start)/0.35f);
                for(int i=0;i<33;i++)
                {float angle=i*Mathf.PI*2f/32f;wave.Points[i]=wave.Center+wave.Rotation*new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0)*Mathf.Lerp(0.1f,1.4f,t);}
                wave.Line.SetPositions(wave.Points);wave.Line.startWidth=wave.Line.endWidth=0.08f*(1f-t);
                wave.Line.startColor=wave.Line.endColor=new Color(1,0.7f,0.3f,1f-t);
            }
        }
        private void OnDestroy()
        {
            if(instance==this)instance=null;
            if(material!=null)Destroy(material);if(texture!=null)Destroy(texture);
        }
    }
}
