using UnityEngine;

namespace SebeJJ.Audio
{
    /// <summary>
    /// 机甲音效控制器 - 附加到机甲对象上
    /// </summary>
    public class MechaAudioController : MonoBehaviour
    {
        [Header("音频源")]
        [SerializeField] private AudioSource movementSource;
        [SerializeField] private AudioSource thrusterSource;
        [SerializeField] private AudioSource damageSource;

        [Header("音效剪辑")]
        [SerializeField] private AudioClip moveLoopClip;
        [SerializeField] private AudioClip thrusterStartClip;
        [SerializeField] private AudioClip thrusterLoopClip;
        [SerializeField] private AudioClip thrusterStopClip;
        [SerializeField] private AudioClip hitLightClip;
        [SerializeField] private AudioClip hitHeavyClip;
        [SerializeField] private AudioClip shieldClip;
        [SerializeField] private AudioClip transformClip;

        [Header("音量设置")]
        [Range(0f, 1f)] [SerializeField] private float moveVolume = 0.5f;
        [Range(0f, 1f)] [SerializeField] private float thrusterVolume = 0.7f;
        [Range(0f, 1f)] [SerializeField] private float damageVolume = 0.8f;

        [Header("3D音效")]
        [SerializeField] private float minDistance = 3f;
        [SerializeField] private float maxDistance = 80f;

        private bool isMoving;
        private bool isThrusting;

        private void Awake()
        {
            SetupAudioSources();
        }

        private void SetupAudioSources()
        {
            if (movementSource == null)
                movementSource = gameObject.AddComponent<AudioSource>();
            if (thrusterSource == null)
                thrusterSource = gameObject.AddComponent<AudioSource>();
            if (damageSource == null)
                damageSource = gameObject.AddComponent<AudioSource>();

            Configure3DAudio(movementSource);
            Configure3DAudio(thrusterSource);
            Configure3DAudio(damageSource);

            // 设置循环音效
            if (moveLoopClip != null)
            {
                movementSource.clip = moveLoopClip;
                movementSource.loop = true;
                movementSource.volume = 0f; // 初始静音
            }

            if (thrusterLoopClip != null)
            {
                thrusterSource.clip = thrusterLoopClip;
                thrusterSource.loop = true;
                thrusterSource.volume = 0f; // 初始静音
            }
        }

        private void Configure3DAudio(AudioSource source)
        {
            source.spatialBlend = 1.0f;
            source.rolloffMode = AudioRolloffMode.Custom;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            source.spread = 90;
        }

        /// <summary>
        /// 设置移动状态
        /// </summary>
        public void SetMoving(bool moving, float intensity = 1f)
        {
            if (isMoving == moving) return;
            isMoving = moving;

            if (moving)
            {
                if (!movementSource.isPlaying)
                    movementSource.Play();
                
                // 根据移动强度调整音量
                movementSource.volume = moveVolume * Mathf.Clamp01(intensity);
            }
            else
            {
                // 淡出停止
                StartCoroutine(FadeOutSource(movementSource, 0.3f));
            }
        }

        /// <summary>
        /// 更新移动强度（用于调整音量）
        /// </summary>
        public void UpdateMoveIntensity(float intensity)
        {
            if (isMoving && movementSource != null)
            {
                movementSource.volume = moveVolume * Mathf.Clamp01(intensity);
            }
        }

        /// <summary>
        /// 启动推进器
        /// </summary>
        public void StartThruster()
        {
            if (isThrusting) return;
            isThrusting = true;

            // 播放启动音效
            if (thrusterStartClip != null)
                thrusterSource.PlayOneShot(thrusterStartClip, thrusterVolume);

            // 播放循环音效
            if (thrusterLoopClip != null)
            {
                thrusterSource.clip = thrusterLoopClip;
                thrusterSource.loop = true;
                thrusterSource.volume = thrusterVolume;
                
                // 延迟启动循环（等启动音效播放一部分）
                Invoke(nameof(PlayThrusterLoop), thrusterStartClip != null ? 0.3f : 0f);
            }
        }

        private void PlayThrusterLoop()
        {
            if (isThrusting && thrusterSource != null)
                thrusterSource.Play();
        }

        /// <summary>
        /// 停止推进器
        /// </summary>
        public void StopThruster()
        {
            if (!isThrusting) return;
            isThrusting = false;

            // 停止循环
            if (thrusterSource.isPlaying)
                thrusterSource.Stop();

            // 播放停止音效
            if (thrusterStopClip != null)
                thrusterSource.PlayOneShot(thrusterStopClip, thrusterVolume);
        }

        /// <summary>
        /// 播放受伤音效
        /// </summary>
        public void PlayHit(bool isHeavy)
        {
            AudioClip clip = isHeavy ? hitHeavyClip : hitLightClip;
            
            if (clip != null)
            {
                damageSource.PlayOneShot(clip, damageVolume);
            }
            else
            {
                SFXType sfxType = isHeavy ? SFXType.MechaHitHeavy : SFXType.MechaHitLight;
                AudioManager.Instance?.PlaySFX(sfxType, transform.position);
            }
        }

        /// <summary>
        /// 播放护盾音效
        /// </summary>
        public void PlayShield()
        {
            if (shieldClip != null)
            {
                damageSource.PlayOneShot(shieldClip, damageVolume);
            }
            else
            {
                AudioManager.Instance?.PlaySFX(SFXType.MechaShield, transform.position);
            }
        }

        /// <summary>
        /// 播放变形音效
        /// </summary>
        public void PlayTransform()
        {
            if (transformClip != null)
            {
                damageSource.PlayOneShot(transformClip, damageVolume);
            }
            else
            {
                AudioManager.Instance?.PlaySFX(SFXType.MechaTransform, transform.position);
            }
        }

        private System.Collections.IEnumerator FadeOutSource(AudioSource source, float duration)
        {
            float startVolume = source.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            source.Stop();
            source.volume = startVolume; // 恢复音量以便下次使用
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}
