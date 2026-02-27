using UnityEngine;
using System;
using System.Collections;

namespace SebeJJ.Enemies
{
    /// <summary>
    /// 敌人受击控制器 - E-FB-001
    /// 管理敌人的受击硬直、击退和反馈
    /// </summary>
    public class EnemyHitReaction : MonoBehaviour
    {
        [Header("硬直设置")]
        [SerializeField] private float baseStunDuration = 0.15f;     // 基础硬直时间
        [SerializeField] private float maxStunDuration = 0.5f;       // 最大硬直时间
        [SerializeField] private float stunDecayRate = 0.2f;         // 硬直递减率
        
        [Header("击退设置")]
        [SerializeField] private bool enableKnockback = true;        // 是否启用击退
        [SerializeField] private float knockbackDamping = 0.9f;      // 击退阻尼
        [SerializeField] private float knockbackDecay = 0.1f;        // 击退递减
        
        [Header("无敌帧")]
        [SerializeField] private float invincibilityDuration = 0.1f; // 受伤后无敌时间
        
        [Header("视觉效果")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float flashDuration = 0.1f;
        [SerializeField] private Color flashColor = Color.red;
        [SerializeField] private AnimationCurve flashCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        [Header("受击动画")]
        [SerializeField] private Animator animator;
        [SerializeField] private string hitTriggerName = "Hit";
        [SerializeField] private string stunBoolName = "Stunned";
        
        // 组件引用
        private Rigidbody2D rb;
        private EnemyBase enemyBase;
        
        // 运行时状态
        private bool isStunned = false;
        private float stunTimer = 0f;
        private float currentStunDuration = 0f;
        private float invincibilityEndTime = 0f;
        private int consecutiveHits = 0;
        private float lastHitTime = -999f;
        private Color originalColor;
        private Material originalMaterial;
        
        // 事件
        public event Action OnStunStart;
        public event Action OnStunEnd;
        public event Action OnHitReacted;
        
        // 属性
        public bool IsStunned => isStunned;
        public bool IsInvincible => Time.time < invincibilityEndTime;
        public float StunProgress => currentStunDuration > 0 ? stunTimer / currentStunDuration : 0f;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            enemyBase = GetComponent<EnemyBase>();
            
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (animator == null) animator = GetComponent<Animator>();
            
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
                originalMaterial = spriteRenderer.material;
            }
        }
        
        /// <summary>
        /// 处理受击反应
        /// </summary>
        public void ProcessHitReaction(float damage, Vector2 hitDirection, float knockbackForce, 
            float stunDuration = -1, bool triggerInvincibility = true)
        {
            // 检查无敌
            if (IsInvincible) return;
            
            // 计算连击递减
            float currentTime = Time.time;
            if (currentTime - lastHitTime > 1f)
            {
                consecutiveHits = 0;
            }
            consecutiveHits++;
            lastHitTime = currentTime;
            
            // 计算实际硬直时间
            float actualStunDuration = stunDuration >= 0 ? stunDuration : baseStunDuration;
            float decayMultiplier = Mathf.Pow(1f - stunDecayRate, consecutiveHits - 1);
            actualStunDuration = Mathf.Min(actualStunDuration * decayMultiplier, maxStunDuration);
            
            // 应用硬直
            if (actualStunDuration > 0)
            {
                ApplyStun(actualStunDuration);
            }
            
            // 应用击退
            if (enableKnockback && knockbackForce > 0 && rb != null)
            {
                ApplyKnockback(hitDirection, knockbackForce);
            }
            
            // 视觉反馈
            StartCoroutine(HitFlashCoroutine());
            
            // 动画
            if (animator != null && !string.IsNullOrEmpty(hitTriggerName))
            {
                animator.SetTrigger(hitTriggerName);
            }
            
            // 无敌帧
            if (triggerInvincibility)
            {
                invincibilityEndTime = Time.time + invincibilityDuration;
            }
            
            OnHitReacted?.Invoke();
        }
        
        /// <summary>
        /// 应用硬直
        /// </summary>
        private void ApplyStun(float duration)
        {
            if (duration <= 0) return;
            
            isStunned = true;
            currentStunDuration = duration;
            stunTimer = 0f;
            
            // 设置动画
            if (animator != null && !string.IsNullOrEmpty(stunBoolName))
            {
                animator.SetBool(stunBoolName, true);
            }
            
            OnStunStart?.Invoke();
            
            // 停止移动
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
        
        /// <summary>
        /// 应用击退
        /// </summary>
        private void ApplyKnockback(Vector2 direction, float force)
        {
            if (rb == null) return;
            
            // 计算击退力（考虑递减）
            float decayMultiplier = Mathf.Pow(1f - knockbackDecay, consecutiveHits - 1);
            float actualForce = force * decayMultiplier;
            
            Vector2 knockback = direction.normalized * actualForce;
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }
        
        /// <summary>
        /// 结束硬直
        /// </summary>
        private void EndStun()
        {
            isStunned = false;
            stunTimer = 0f;
            currentStunDuration = 0f;
            
            // 恢复动画
            if (animator != null && !string.IsNullOrEmpty(stunBoolName))
            {
                animator.SetBool(stunBoolName, false);
            }
            
            OnStunEnd?.Invoke();
        }
        
        /// <summary>
        /// 受击闪烁效果
        /// </summary>
        private IEnumerator HitFlashCoroutine()
        {
            if (spriteRenderer == null) yield break;
            
            float elapsed = 0f;
            
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flashDuration;
                float curveValue = flashCurve.Evaluate(t);
                
                spriteRenderer.color = Color.Lerp(originalColor, flashColor, curveValue);
                
                yield return null;
            }
            
            spriteRenderer.color = originalColor;
        }
        
        /// <summary>
        /// 强制结束硬直
        /// </summary>
        public void BreakStun()
        {
            if (isStunned)
            {
                EndStun();
            }
        }
        
        /// <summary>
        /// 设置无敌
        /// </summary>
        public void SetInvincible(float duration)
        {
            invincibilityEndTime = Time.time + duration;
        }
        
        /// <summary>
        /// 重置状态
        /// </summary>
        public void Reset()
        {
            isStunned = false;
            stunTimer = 0f;
            currentStunDuration = 0f;
            consecutiveHits = 0;
            invincibilityEndTime = 0f;
            
            if (animator != null && !string.IsNullOrEmpty(stunBoolName))
            {
                animator.SetBool(stunBoolName, false);
            }
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
        
        private void Update()
        {
            // 更新硬直计时
            if (isStunned)
            {
                stunTimer += Time.deltaTime;
                
                if (stunTimer >= currentStunDuration)
                {
                    EndStun();
                }
            }
            
            // 击退阻尼
            if (enableKnockback && rb != null && !isStunned)
            {
                rb.linearVelocity *= knockbackDamping;
            }
        }
    }
}
