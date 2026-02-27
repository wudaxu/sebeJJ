using UnityEngine;
using System;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 装甲系统 - SD-002
    /// </summary>
    public class ArmorSystem : MonoBehaviour
    {
        [Header("装甲配置")]
        [SerializeField] private float maxArmor = 50f;
        [SerializeField] private float baseDamageReduction = 0.2f;  // 基础伤害减免
        [SerializeField] private float armorEffectiveness = 1f;     // 装甲效能系数

        [Header("破损效果")]
        [SerializeField] private SpriteRenderer armorVisual;        // 装甲视觉
        [SerializeField] private Sprite[] damageStages;             // 不同破损阶段的贴图
        [SerializeField] private ParticleSystem damageEffect;       // 受损特效
        [SerializeField] private AudioClip armorBreakSound;         // 装甲破损音效

        [Header("破损阈值")]
        [SerializeField] private float stage1Threshold = 0.75f;     // 第一阶段破损阈值
        [SerializeField] private float stage2Threshold = 0.5f;      // 第二阶段破损阈值
        [SerializeField] private float stage3Threshold = 0.25f;     // 第三阶段破损阈值

        // 状态
        private float currentArmor;
        private int currentDamageStage = 0;

        // 事件
        public event Action<float> OnArmorChanged;
        public event Action<int> OnDamageStageChanged;  // 破损阶段变化
        public event Action OnArmorDestroyed;

        // 属性
        public float CurrentArmor => currentArmor;
        public float MaxArmor => maxArmor;
        public float ArmorPercent => maxArmor > 0 ? currentArmor / maxArmor : 0;
        public int CurrentDamageStage => currentDamageStage;
        public bool HasArmor => currentArmor > 0;

        private void Awake()
        {
            currentArmor = maxArmor;
            UpdateVisuals();
        }

        /// <summary>
        /// 对装甲造成伤害
        /// </summary>
        /// <returns>返回计算后的伤害</returns>
        public float CalculateArmorMitigation(float damage)
        {
            if (!HasArmor) return damage;

            // 装甲减伤公式: 伤害 = 原始伤害 * (1 - 基础减免) * (100 / (100 + 装甲值 * 效能))
            float mitigation = baseDamageReduction + 
                (1 - baseDamageReduction) * (currentArmor * armorEffectiveness / 
                (currentArmor * armorEffectiveness + 100f));

            mitigation = Mathf.Clamp01(mitigation);
            float mitigatedDamage = damage * (1 - mitigation);

            // 装甲也会受到损耗
            TakeArmorDamage(damage * 0.1f); // 受到10%伤害作为装甲损耗

            return mitigatedDamage;
        }

        /// <summary>
        /// 装甲受到直接伤害
        /// </summary>
        public void TakeArmorDamage(float damage)
        {
            if (!HasArmor) return;

            float previousArmor = currentArmor;
            currentArmor = Mathf.Max(0, currentArmor - damage);

            OnArmorChanged?.Invoke(currentArmor);

            // 检查破损阶段
            CheckDamageStage();

            // 检查装甲完全损毁
            if (previousArmor > 0 && currentArmor <= 0)
            {
                OnArmorDestroyed?.Invoke();
                PlayArmorBreakEffect();
            }

            UpdateVisuals();
        }

        /// <summary>
        /// 修复装甲
        /// </summary>
        public void RepairArmor(float amount)
        {
            if (currentArmor >= maxArmor) return;

            currentArmor = Mathf.Min(currentArmor + amount, maxArmor);
            OnArmorChanged?.Invoke(currentArmor);

            CheckDamageStage();
            UpdateVisuals();
        }

        /// <summary>
        /// 检查并更新破损阶段 - SD-005
        /// </summary>
        private void CheckDamageStage()
        {
            int newStage = 0;
            float percent = ArmorPercent;

            if (percent <= 0)
                newStage = 4; // 完全损毁
            else if (percent < stage3Threshold)
                newStage = 3;
            else if (percent < stage2Threshold)
                newStage = 2;
            else if (percent < stage1Threshold)
                newStage = 1;

            if (newStage != currentDamageStage)
            {
                currentDamageStage = newStage;
                OnDamageStageChanged?.Invoke(currentDamageStage);
                
                if (damageEffect != null)
                {
                    Instantiate(damageEffect, transform.position, Quaternion.identity);
                }
            }
        }

        /// <summary>
        /// 更新视觉效果
        /// </summary>
        private void UpdateVisuals()
        {
            if (armorVisual != null && damageStages != null && damageStages.Length > 0)
            {
                int spriteIndex = Mathf.Min(currentDamageStage, damageStages.Length - 1);
                if (damageStages[spriteIndex] != null)
                {
                    armorVisual.sprite = damageStages[spriteIndex];
                }
            }
        }

        /// <summary>
        /// 播放装甲破损效果
        /// </summary>
        private void PlayArmorBreakEffect()
        {
            if (armorBreakSound != null)
            {
                AudioSource.PlayClipAtPoint(armorBreakSound, transform.position);
            }
        }

        /// <summary>
        /// 获取当前减伤比例
        /// </summary>
        public float GetCurrentDamageReduction()
        {
            if (!HasArmor) return 0;

            return baseDamageReduction + 
                (1 - baseDamageReduction) * (currentArmor * armorEffectiveness / 
                (currentArmor * armorEffectiveness + 100f));
        }

        /// <summary>
        /// 设置最大装甲
        /// </summary>
        public void SetMaxArmor(float value)
        {
            maxArmor = value;
            currentArmor = Mathf.Min(currentArmor, maxArmor);
            OnArmorChanged?.Invoke(currentArmor);
        }
    }
}