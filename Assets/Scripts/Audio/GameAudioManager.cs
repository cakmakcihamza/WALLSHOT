using UnityEngine;

namespace WallBreaker.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class GameAudioManager : MonoBehaviour
    {
        [Header("Clips")]
        [SerializeField] private AudioClip launchClip;
        [SerializeField] private AudioClip impactClip;
        [SerializeField] private AudioClip brickBreakClip;

        [Header("Volumes")]
        [SerializeField] private float launchVolume = 0.55f;
        [SerializeField] private float impactVolume = 0.75f;
        [SerializeField] private float brickBreakVolume = 0.45f;

        private static GameAudioManager instance;
        private AudioSource audioSource;

        public static GameAudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameAudioManager existing = FindBestExistingManager();
                    instance = existing != null ? existing : new GameObject("GameAudioManager").AddComponent<GameAudioManager>();
                }

                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                if (HasAnyClipAssigned() && !instance.HasAnyClipAssigned())
                {
                    Destroy(instance.gameObject);
                    instance = this;
                }
                else
                {
                    Destroy(gameObject);
                    return;
                }
            }

            instance = this;
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        private static GameAudioManager FindBestExistingManager()
        {
            GameAudioManager[] managers = FindObjectsByType<GameAudioManager>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            if (managers.Length == 0)
            {
                return null;
            }

            GameAudioManager fallback = managers[0];
            for (int i = 0; i < managers.Length; i++)
            {
                if (managers[i].HasAnyClipAssigned())
                {
                    return managers[i];
                }
            }

            return fallback;
        }

        private bool HasAnyClipAssigned()
        {
            return launchClip != null || impactClip != null || brickBreakClip != null;
        }

        public void PlayLaunch()
        {
            PlayOneShot(launchClip, launchVolume);
        }

        public void PlayImpact()
        {
            PlayOneShot(impactClip, impactVolume);
        }

        public void PlayBrickBreak()
        {
            PlayOneShot(brickBreakClip, brickBreakVolume);
        }

        private void PlayOneShot(AudioClip clip, float volume)
        {
            if (clip == null)
            {
                return;
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }
    }
}
