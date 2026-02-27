using UnityEngine;
using System.Collections.Generic;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 延迟伤害系统 - DM-007
    /// </summary>
    public class DelayedDamage : MonoBehaviour
    {
        [System.Serializable]
        public class DelayedDamageInfo
        {
            public float damage;
            public DamageType damageType;
            public float delay;
            public GameObject warningEffect;
            public GameObject impactEffect;
            public Vector2 position;
            public float radius;
            public GameObject attacker;
            
            [HideInInspector] public float triggerTime;
            [HideInInspector] public GameObject warningInstance;
        }

        private List<DelayedDamageInfo> pendingDamages = new List<DelayedDamageInfo>();

        /// <summary>
        /// 添加延迟伤害
        /// </summary>
        public void AddDelayedDamage(DelayedDamageInfo info)
        {
            info.triggerTime = Time.time + info.delay;

            // 创建预警效果
            if (info.warningEffect != null)
            {
                info.warningInstance = Instantiate(info.warningEffect, info.position, Quaternion.identity);
                
                // 设置预警大小
                var scaler = info.warningInstance.GetComponent<DelayedDamageIndicator>();
                if (scaler != null)
                {
                    scaler.Initialize(info.delay, info.radius);
                }
            }

            pendingDamages.Add(info);
        }

        private void Update()
        {
            ProcessDelayedDamages();
        }

        /// <summary>
        /// 处理延迟伤害
        /// </summary>
        private void ProcessDelayedDamages()
        {
            for (int i = pendingDamages.Count - 1; i >= 0; i--)
            {
                var info = pendingDamages[i];

                if (Time.time >= info.triggerTime)
                {
                    TriggerDelayedDamage(info);
                    pendingDamages.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 触发延迟伤害
        /// </summary>
        private void TriggerDelayedDamage(DelayedDamageInfo info)
        {
            // 清理预警效果
            if (info.warningInstance != null)
            {
                Destroy(info.warningInstance);
            }

            // 创建爆炸效果
            if (info.impactEffect != null)
            {
                var effect = Instantiate(info.impactEffect, info.position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            // 范围伤害
            if (info.radius > 0)
            {
                ApplyAreaDamage(info);
            }

            // 触发反馈
            CombatFeedback.Instance?.TriggerScreenShake(0.4f, 0.3f);
        }

        /// <summary>
        /// 应用范围伤害
        /// </summary>
        private void ApplyAreaDamage(DelayedDamageInfo info)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(info.position, info.radius);

            foreach (var hit in hits)
            {
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    var damageInfo = new DamageInfo(info.damage, info.damageType);
                    damageInfo.Attacker = info.attacker;
                    damageInfo.HitPosition = hit.transform.position;
                    damageInfo.HitDirection = (hit.transform.position - (Vector3)info.position).normalized;

                    damageable.TakeDamage(damageInfo);
                }

                // 击退效果
                var rb = hit.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 knockbackDir = (hit.transform.position - (Vector3)info.position).normalized;
                    rb.AddForce(knockbackDir * 10f, ForceMode2D.Impulse);
                }
            }
        }

        /// <summary>
        /// 清除所有待处理的延迟伤害
        /// </summary>
        public void ClearAllDelayedDamages()
        {
            foreach (var info in pendingDamages)
            {
                if (info.warningInstance != null)
                    Destroy(info.warningInstance);
            }
            pendingDamages.Clear();
        }
    }

    /// <summary>
    /// 延迟伤害预警指示器
    /// </summary>
    public class DelayedDamageIndicator : MonoBehaviour
    {
        private float totalDuration;
        private float currentTime;
        private SpriteRenderer spriteRenderer;

        public void Initialize(float duration, float radius)
        {
            totalDuration = duration;
            currentTime = 0;
            
            transform.localScale = Vector3.one * radius * 2;
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            currentTime += Time.deltaTime;
            float progress = currentTime / totalDuration;

            if (spriteRenderer != null)
            {
                // 颜色从绿变红
                spriteRenderer.color = Color.Lerp(Color.green, Color.red, progress);
                
                // 透明度闪烁
                float alpha = 0.3f + Mathf.PingPong(progress * 5, 0.3f);
                Color color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
        }
    }
}