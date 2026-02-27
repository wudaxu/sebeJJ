using UnityEngine;

namespace SebeJJ.Shop
{
    /// <summary>
    /// 商店音效管理器
    /// </summary>
    public class ShopAudioManager : MonoBehaviour
    {
        public static ShopAudioManager Instance { get; private set; }
        
        [Header("音效")]
        [SerializeField] private AudioClip openShopSound;
        [SerializeField] private AudioClip closeShopSound;
        [SerializeField] private AudioClip itemHoverSound;
        [SerializeField] private AudioClip itemClickSound;
        [SerializeField] private AudioClip addToCartSound;
        [SerializeField] private AudioClip removeFromCartSound;
        [SerializeField] private AudioClip purchaseSuccessSound;
        [SerializeField] private AudioClip purchaseFailSound;
        [SerializeField] private AudioClip unlockSound;
        
        [Header("设置")]
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private bool spatialAudio = false;
        
        private AudioSource _audioSource;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.spatialBlend = spatialAudio ? 1f : 0f;
            }
        }
        
        private void Start()
        {
            // 订阅商店事件
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnItemUnlocked += _ => PlayUnlockSound();
                ShopManager.Instance.OnPurchaseCompleted += result =>
                {
                    if (result.Success)
                        PlayPurchaseSuccessSound();
                    else
                        PlayPurchaseFailSound();
                };
            }
        }
        
        private void PlaySound(AudioClip clip)
        {
            if (clip != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(clip, sfxVolume);
            }
        }
        
        public void PlayOpenShopSound() => PlaySound(openShopSound);
        public void PlayCloseShopSound() => PlaySound(closeShopSound);
        public void PlayItemHoverSound() => PlaySound(itemHoverSound);
        public void PlayItemClickSound() => PlaySound(itemClickSound);
        public void PlayAddToCartSound() => PlaySound(addToCartSound);
        public void PlayRemoveFromCartSound() => PlaySound(removeFromCartSound);
        public void PlayPurchaseSuccessSound() => PlaySound(purchaseSuccessSound);
        public void PlayPurchaseFailSound() => PlaySound(purchaseFailSound);
        public void PlayUnlockSound() => PlaySound(unlockSound);
    }
}