using UnityEngine;
using System;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 护盾系统 - SD-001
    /// </summary>
    public class ShieldSystem : MonoBehaviour
    {
        [Header("护盾配置")]
        [SerializeField] private float maxShield = 100f;
        [SerializeField] private float shieldRegenRate = 10f;      // 每秒恢复量
        [SerializeField] private float shieldRegenDelay = 3f;      // 受伤后延迟恢复
        [SerializeField] private float shieldDamageAbsorption = 1f; // 护盾吸收比例(1=100%)

        [Header("护盾效果")]
        [SerializeField] private GameObject shieldVisual;          // 护盾视觉效果
        [SerializeField] private ParticleSystem shieldBreakEffect; // 护盾破碎特效
        [SerializeField] private AudioClip shieldBreakSound;       // 护盾破碎音效
        [SerializeField] private AudioClip shieldHitSound;         // 护盾受击音效

        // 状态
        private float currentShield;
        private float lastDamageTime;
        private bool isShieldActive;
        private bool wasShieldBroken;

        // 事件
        public event Action<float> OnShieldChanged;     // 护盾值变化
        public event Action OnShieldDepleted;            // 护盾耗尽
        public event Action OnShieldRestored;            // 护盾恢复满
        public event Action<float> OnShieldHit;          // 护盾受击

        // 属性
        public float CurrentShield => currentShield;
        public float MaxShield => maxShield;
        public float ShieldPercent => maxShield > 0 ? currentShield / maxShield : 0;
        public bool HasShield => currentShield > 0;
        public bool IsShieldActive => isShieldActive;

        private void Awake()
        {
            currentShield = maxShield;
            UpdateShieldVisual();
        }

        private void Update()
        {
            RegenerateShield();
        }

        /// <summary>
        /// 对护盾造成伤害
        /// </summary>
        /// <returns>返回穿透护盾的剩余伤害</returns>
        public float TakeShieldDamage(float damage)
        {
            if (!HasShield) return damage;

            float absorbedDamage = damage * shieldDamageAbsorption;
            float penetratingDamage = damage - absorbedDamage;

            currentShield -= absorbedDamage;
            lastDamageTime = Time.time;

            // 触发事件
            OnShieldChanged?.Invoke(currentShield);
            OnShieldHit?.Invoke(absorbedDamage);

            // 播放受击效果
            PlayShieldHitEffect();

            // 检查护盾破碎
            if (currentShield <= 0)
            {
                currentShield = 0;
                OnShieldDepleted?.Invoke();
                PlayShieldBreakEffect();
            }

            UpdateShieldVisual();
            return penetratingDamage;
        }

        /// <summary>
        /// 恢复护盾
        /// </summary>
        public void RestoreShield(float amount)
        {
            if (currentShield >= maxShield) return;

            bool wasDepleted = !HasShield;
            currentShield = Mathf.Min(currentShield + amount, maxShield);
            
            OnShieldChanged?.Invoke(currentShield);

            if (wasDepleted && HasShield)
            {
                OnShieldRestored?.Invoke();
            }

            UpdateShieldVisual();
        }

        /// <summary>
        /// 护盾自动恢复
        /// </summary>
        private void RegenerateShield()
        {
            if (!HasShield || currentShield >= maxShield) return;
            if (Time.time < lastDamageTime + shieldRegenDelay) return;

            float regenAmount = shieldRegenRate * Time.deltaTime;
            RestoreShield(regenAmount);
        }

        /// <summary>
        /// 设置最大护盾
        /// </summary>
        public void SetMaxShield(float value)
        {
            maxShield = value;
            currentShield = Mathf.Min(currentShield, maxShield);
            OnShieldChanged?.Invoke(currentShield);
        }

        /// <summary>
        /// 更新护盾视觉效果
        /// </summary>
        private void UpdateShieldVisual()
        {
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(HasShield);
                
                // 可以在这里添加护盾强度视觉效果
                var renderer = shieldVisual.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    Color color = renderer.color;
                    color.a = 0.3f + ShieldPercent * 0.5f;
                    renderer.color = color;
                }
            }
        }

        /// <summary>
        /// 播放护盾受击效果
        /// </summary>
        private void PlayShieldHitEffect()
        {
            if (shieldHitSound != null)
            {
                AudioSource.PlayClipAtPoint(shieldHitSound, transform.position, 0.5f);
            }

            // 护盾闪烁效果
            if (shieldVisual != null)
            {
                // 可以通过动画或材质实现闪烁
            }
        }

        /// <summary>
        /// 播放护盾破碎效果 - SD-004
        /// </summary>
        private void PlayShieldBreakEffect()
        {
            if (shieldBreakEffect != null)
            {
                Instantiate(shieldBreakEffect, transform.position, Quaternion.identity);
            }

            if (shieldBreakSound != null)
            {
                AudioSource.PlayClipAtPoint(shieldBreakSound, transform.position);
            }
        }
    }
}