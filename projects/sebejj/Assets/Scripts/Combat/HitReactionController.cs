using UnityEngine;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 受击动画控制器 - FB-005
    /// </summary>
    public class HitReactionController : MonoBehaviour
    {
        [Header("受击动画")]
        [SerializeField] private Animator animator;
        [SerializeField] private string hitTriggerName = "Hit";
        [SerializeField] private string stunBoolName = "Stunned";
        [SerializeField] private string deathTriggerName = "Die";

        [Header("受击效果")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float flashDuration = 0.1f;
        [SerializeField] private Color flashColor = Color.red;
        [SerializeField] private Material flashMaterial;

        [Header("击退设置")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private bool applyKnockback = true;

        private Material originalMaterial;
        private Color originalColor;
        private CombatStats combatStats;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            combatStats = GetComponent<CombatStats>();

            if (spriteRenderer != null)
            {
                originalMaterial = spriteRenderer.material;
                originalColor = spriteRenderer.color;
            }
        }

        private void OnEnable()
        {
            if (combatStats != null)
            {
                combatStats.OnDamageTaken += OnDamageTaken;
                combatStats.OnDeath += OnDeath;
            }
        }

        private void OnDisable()
        {
            if (combatStats != null)
            {
                combatStats.OnDamageTaken -= OnDamageTaken;
                combatStats.OnDeath -= OnDeath;
            }
        }

        /// <summary>
        /// 受到伤害回调
        /// </summary>
        private void OnDamageTaken(object sender, DamageEventArgs e)
        {
            // 播放受击动画
            PlayHitAnimation();

            // 闪烁效果
            StartCoroutine(FlashCoroutine());

            // 击退效果
            if (applyKnockback && rb != null)
            {
                ApplyKnockback(e.DamageInfo.HitDirection, e.DamageInfo.KnockbackForce);
            }

            // 眩晕效果
            if (e.DamageInfo.StunDuration > 0)
            {
                ApplyStun(e.DamageInfo.StunDuration);
            }
        }

        /// <summary>
        /// 死亡回调
        /// </summary>
        private void OnDeath(object sender, System.EventArgs e)
        {
            PlayDeathAnimation();
        }

        /// <summary>
        /// 播放受击动画
        /// </summary>
        private void PlayHitAnimation()
        {
            if (animator != null && !string.IsNullOrEmpty(hitTriggerName))
            {
                animator.SetTrigger(hitTriggerName);
            }
        }

        /// <summary>
        /// 播放死亡动画
        /// </summary>
        private void PlayDeathAnimation()
        {
            if (animator != null && !string.IsNullOrEmpty(deathTriggerName))
            {
                animator.SetTrigger(deathTriggerName);
            }
        }

        /// <summary>
        /// 应用击退
        /// </summary>
        private void ApplyKnockback(Vector2 direction, float force)
        {
            if (rb == null || force <= 0) return;

            Vector2 knockback = DamageCalculator.CalculateKnockback(direction, force, rb.mass);
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }

        /// <summary>
        /// 应用眩晕
        /// </summary>
        private void ApplyStun(float duration)
        {
            if (animator != null && !string.IsNullOrEmpty(stunBoolName))
            {
                animator.SetBool(stunBoolName, true);
            }

            Invoke(nameof(EndStun), duration);
        }

        private void EndStun()
        {
            if (animator != null && !string.IsNullOrEmpty(stunBoolName))
            {
                animator.SetBool(stunBoolName, false);
            }
        }

        /// <summary>
        /// 闪烁效果协程
        /// </summary>
        private System.Collections.IEnumerator FlashCoroutine()
        {
            if (spriteRenderer == null) yield break;

            // 使用材质或颜色闪烁
            if (flashMaterial != null)
            {
                spriteRenderer.material = flashMaterial;
            }
            else
            {
                spriteRenderer.color = flashColor;
            }

            yield return new WaitForSeconds(flashDuration);

            // 恢复原状
            if (flashMaterial != null)
            {
                spriteRenderer.material = originalMaterial;
            }
            else
            {
                spriteRenderer.color = originalColor;
            }
        }
    }
}