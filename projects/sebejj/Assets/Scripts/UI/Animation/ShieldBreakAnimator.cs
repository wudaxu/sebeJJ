using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

namespace SebeJJ.UI.Animation
{
    /// <summary>
    /// 护盾破碎动画 - 破裂效果
    /// </summary>
    public class ShieldBreakAnimator : MonoBehaviour
    {
        [Header("基础配置")]
        [SerializeField] private RectTransform shieldTransform;
        [SerializeField] private Image shieldImage;
        [SerializeField] private CanvasGroup shieldGroup;
        
        [Header("破碎效果")]
        [SerializeField] private float crackDuration = 0.3f;
        [SerializeField] private float shatterDuration = 0.5f;
        [SerializeField] private int shardCount = 8;
        [SerializeField] private float shardScatterDistance = 150f;
        [SerializeField] private float shardRotationSpeed = 720f;
        
        [Header("裂纹效果")]
        [SerializeField] private float crackShakeAmount = 3f;
        [SerializeField] private int crackCount = 3;
        [SerializeField] private Color crackColor = new Color(0.8f, 0.9f, 1f, 0.8f);
        
        [Header("光效")]
        [SerializeField] private Image glowImage;
        [SerializeField] private float glowIntensity = 2f;
        [SerializeField] private float glowDuration = 0.4f;
        
        [Header("粒子效果")]
        [SerializeField] private ParticleSystem breakParticles;
        [SerializeField] private float particleEmissionMultiplier = 1.5f;
        
        [Header("声音反馈")]
        [SerializeField] private AudioClip crackSound;
        [SerializeField] private AudioClip shatterSound;
        
        private Sequence breakSequence;
        private List<RectTransform> shards = new List<RectTransform>();
        private bool isBroken;
        
        private void Awake()
        {
            if (shieldTransform == null)
                shieldTransform = GetComponent<RectTransform>();
            if (shieldImage == null)
                shieldImage = GetComponent<Image>();
            if (shieldGroup == null)
                shieldGroup = GetComponent<CanvasGroup>();
        }
        
        /// <summary>
        /// 播放护盾破碎动画
        /// </summary>
        public void PlayBreakAnimation()
        {
            if (isBroken) return;
            isBroken = true;
            
            breakSequence?.Kill();
            breakSequence = DOTween.Sequence();
            
            // 阶段1：裂纹出现
            breakSequence.AppendCallback(() => PlayCrackPhase());
            breakSequence.AppendInterval(crackDuration);
            
            // 阶段2：破碎
            breakSequence.AppendCallback(() => PlayShatterPhase());
            
            breakSequence.OnComplete(() =>
            {
                // 清理碎片
                ClearShards();
                gameObject.SetActive(false);
            });
        }
        
        /// <summary>
        /// 裂纹阶段
        /// </summary>
        private void PlayCrackPhase()
        {
            // 震动
            shieldTransform.DOShakeAnchorPos(
                crackDuration,
                new Vector2(crackShakeAmount, crackShakeAmount),
                crackCount * 10,
                90f
            );
            
            // 颜色变化（显示裂纹）
            if (shieldImage != null)
            {
                shieldImage.DOColor(crackColor, crackDuration * 0.5f)
                    .SetLoops(2, LoopType.Yoyo);
            }
            
            // 发光增强
            if (glowImage != null)
            {
                glowImage.DOFade(0.8f, crackDuration * 0.3f)
                    .SetLoops(2, LoopType.Yoyo);
            }
            
            // 播放裂纹音效
            if (crackSound != null)
                AudioSource.PlayClipAtPoint(crackSound, Camera.main.transform.position);
        }
        
        /// <summary>
        /// 破碎阶段
        /// </summary>
        private void PlayShatterPhase()
        {
            // 创建碎片
            CreateShards();
            
            // 隐藏原护盾
            if (shieldImage != null)
                shieldImage.DOFade(0f, 0.05f);
            
            // 播放粒子效果
            if (breakParticles != null)
            {
                var emission = breakParticles.emission;
                emission.rateOverTime = emission.rateOverTime.constant * particleEmissionMultiplier;
                breakParticles.Play();
            }
            
            // 播放破碎音效
            if (shatterSound != null)
                AudioSource.PlayClipAtPoint(shatterSound, Camera.main.transform.position);
            
            // 碎片飞散动画
            AnimateShards();
        }
        
        /// <summary>
        /// 创建碎片
        /// </summary>
        private void CreateShards()
        {
            shards.Clear();
            
            Vector2 shieldSize = shieldTransform.rect.size;
            Vector3 shieldPos = shieldTransform.position;
            
            for (int i = 0; i < shardCount; i++)
            {
                // 创建碎片对象
                GameObject shard = new GameObject($"Shard_{i}");
                shard.transform.SetParent(transform.parent);
                
                RectTransform shardRect = shard.AddComponent<RectTransform>();
                Image shardImage = shard.AddComponent<Image>();
                
                // 复制护盾外观
                if (shieldImage != null)
                {
                    shardImage.sprite = shieldImage.sprite;
                    shardImage.color = shieldImage.color;
                    shardImage.material = shieldImage.material;
                }
                
                // 设置大小和位置
                shardRect.sizeDelta = shieldSize * Random.Range(0.2f, 0.4f);
                shardRect.position = shieldPos;
                shardRect.rotation = shieldTransform.rotation;
                shardRect.localScale = shieldTransform.localScale;
                
                shards.Add(shardRect);
            }
        }
        
        /// <summary>
        /// 动画化碎片
        /// </summary>
        private void AnimateShards()
        {
            Vector3 center = shieldTransform.position;
            
            for (int i = 0; i < shards.Count; i++)
            {
                var shard = shards[i];
                
                // 计算飞散方向
                float angle = (360f / shardCount) * i + Random.Range(-20f, 20f);
                Vector3 direction = Quaternion.Euler(0f, 0f, angle) * Vector3.right;
                Vector3 targetPos = center + direction * shardScatterDistance * Random.Range(0.8f, 1.2f);
                
                // 飞散动画
                shard.DOAnchorPos3D(targetPos, shatterDuration)
                    .SetEase(Ease.OutQuad);
                
                // 旋转
                shard.DORotate(
                    new Vector3(0f, 0f, Random.Range(-shardRotationSpeed, shardRotationSpeed)),
                    shatterDuration,
                    RotateMode.FastBeyond360
                ).SetEase(Ease.OutQuad);
                
                // 缩放消失
                shard.DOScale(Vector3.zero, shatterDuration * 0.7f)
                    .SetDelay(shatterDuration * 0.3f)
                    .SetEase(Ease.InBack);
                
                // 淡出
                var shardImage = shard.GetComponent<Image>();
                if (shardImage != null)
                {
                    shardImage.DOFade(0f, shatterDuration * 0.5f)
                        .SetDelay(shatterDuration * 0.5f);
                }
            }
        }
        
        /// <summary>
        /// 清理碎片
        /// </summary>
        private void ClearShards()
        {
            foreach (var shard in shards)
            {
                if (shard != null)
                    Destroy(shard.gameObject);
            }
            shards.Clear();
        }
        
        /// <summary>
        /// 护盾恢复动画
        /// </summary>
        public void PlayRestoreAnimation()
        {
            isBroken = false;
            gameObject.SetActive(true);
            
            breakSequence?.Kill();
            breakSequence = DOTween.Sequence();
            
            // 重置状态
            if (shieldImage != null)
            {
                shieldImage.color = new Color(1f, 1f, 1f, 0f);
                shieldImage.DOFade(1f, 0.3f);
            }
            
            shieldTransform.localScale = Vector3.zero;
            
            // 缩放进入
            breakSequence.Append(
                shieldTransform.DOScale(Vector3.one, 0.4f)
                    .SetEase(Ease.OutBack)
            );
            
            // 发光效果
            if (glowImage != null)
            {
                glowImage.color = new Color(1f, 1f, 1f, 0f);
                breakSequence.Join(
                    glowImage.DOFade(0.5f, 0.3f)
                        .SetLoops(2, LoopType.Yoyo)
                );
            }
        }
        
        /// <summary>
        /// 护盾受击动画（未破碎）
        /// </summary>
        public void PlayHitAnimation()
        {
            if (isBroken) return;
            
            // 震动
            shieldTransform.DOShakeAnchorPos(
                0.2f,
                new Vector2(5f, 5f),
                10,
                90f
            );
            
            // 闪烁
            if (shieldImage != null)
            {
                shieldImage.DOColor(Color.white, 0.1f)
                    .SetLoops(2, LoopType.Yoyo);
            }
            
            // 缩放反弹
            shieldTransform.DOScale(Vector3.one * 1.1f, 0.1f)
                .SetEase(Ease.OutBack)
                .SetLoops(2, LoopType.Yoyo);
        }
        
        private void OnDestroy()
        {
            breakSequence?.Kill();
            ClearShards();
        }
    }
}
