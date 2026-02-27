using UnityEngine;

namespace SebeJJ.Audio
{
    /// <summary>
    /// 武器音效控制器 - 附加到武器对象上
    /// </summary>
    public class WeaponAudioController : MonoBehaviour
    {
        [Header("音频源")]
        [SerializeField] private AudioSource fireSource;
        [SerializeField] private AudioSource reloadSource;
        [SerializeField] private AudioSource chargeSource;

        [Header("音效设置")]
        [SerializeField] private AudioClip fireClip;
        [SerializeField] private AudioClip reloadStartClip;
        [SerializeField] private AudioClip reloadCompleteClip;
        [SerializeField] private AudioClip chargeClip;
        [SerializeField] private AudioClip overheatClip;

        [Header("音量设置")]
        [Range(0f, 1f)] [SerializeField] private float fireVolume = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float reloadVolume = 0.6f;
        [Range(0f, 1f)] [SerializeField] private float chargeVolume = 0.7f;

        [Header("3D音效")]
        [SerializeField] private bool use3DAudio = true;
        [SerializeField] private float minDistance = 2f;
        [SerializeField] private float maxDistance = 150f;

        private void Awake()
        {
            SetupAudioSources();
        }

        private void SetupAudioSources()
        {
            if (fireSource == null)
                fireSource = gameObject.AddComponent<AudioSource>();
            if (reloadSource == null)
                reloadSource = gameObject.AddComponent<AudioSource>();
            if (chargeSource == null)
                chargeSource = gameObject.AddComponent<AudioSource>();

            // 配置3D音效
            if (use3DAudio)
            {
                Configure3DAudio(fireSource);
                Configure3DAudio(reloadSource);
                Configure3DAudio(chargeSource);
            }
        }

        private void Configure3DAudio(AudioSource source)
        {
            source.spatialBlend = 1.0f;
            source.rolloffMode = AudioRolloffMode.Custom;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            source.spread = 60;
        }

        /// <summary>
        /// 播放开火音效
        /// </summary>
        public void PlayFire()
        {
            if (fireClip != null && fireSource != null)
            {
                fireSource.PlayOneShot(fireClip, fireVolume);
            }
            else
            {
                AudioManager.Instance?.PlaySFX(SFXType.WeaponPrimaryFire, transform.position);
            }
        }

        /// <summary>
        /// 播放换弹开始音效
        /// </summary>
        public void PlayReloadStart()
        {
            if (reloadStartClip != null && reloadSource != null)
            {
                reloadSource.PlayOneShot(reloadStartClip, reloadVolume);
            }
            else
            {
                AudioManager.Instance?.PlaySFX(SFXType.WeaponReloadStart, transform.position);
            }
        }

        /// <summary>
        /// 播放换弹完成音效
        /// </summary>
        public void PlayReloadComplete()
        {
            if (reloadCompleteClip != null && reloadSource != null)
            {
                reloadSource.PlayOneShot(reloadCompleteClip, reloadVolume);
            }
            else
            {
                AudioManager.Instance?.PlaySFX(SFXType.WeaponReloadComplete, transform.position);
            }
        }

        /// <summary>
        /// 开始蓄力音效
        /// </summary>
        public void StartCharge()
        {
            if (chargeClip != null && chargeSource != null)
            {
                chargeSource.clip = chargeClip;
                chargeSource.volume = chargeVolume;
                chargeSource.loop = true;
                chargeSource.Play();
            }
        }

        /// <summary>
        /// 停止蓄力音效
        /// </summary>
        public void StopCharge()
        {
            if (chargeSource != null && chargeSource.isPlaying)
            {
                chargeSource.Stop();
            }
        }

        /// <summary>
        /// 播放过热音效
        /// </summary>
        public void PlayOverheat()
        {
            if (overheatClip != null && fireSource != null)
            {
                fireSource.PlayOneShot(overheatClip, fireVolume);
            }
            else
            {
                AudioManager.Instance?.PlaySFX(SFXType.WeaponOverheat, transform.position);
            }
        }
    }
}
