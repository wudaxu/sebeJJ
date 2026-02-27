using UnityEngine;
using System;
using System.Collections.Generic;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 武器连招系统 - WP-008
    /// 管理武器连击、连招窗口和连招奖励
    /// </summary>
    public class WeaponComboSystem : MonoBehaviour
    {
        [Header("连招配置")]
        [SerializeField] private float comboWindow = 0.8f;           // 连招窗口时间
        [SerializeField] private float comboResetTime = 2f;          // 连招重置时间
        [SerializeSerializeField] private int maxComboCount = 3;      // 最大连击数
        
        [Header("连招奖励")]
        [SerializeField] private float damageBonusPerCombo = 0.1f;   // 每层连招伤害加成
        [SerializeField] private float speedBonusPerCombo = 0.1f;    // 每层连招速度加成
        [SerializeField] private float rangeBonusPerCombo = 0f;      // 每层连招范围加成
        
        [Header("特殊连击")]
        [SerializeField] private float finalHitMultiplier = 1.5f;    // 终结击伤害倍率
        [SerializeField] private float finalHitRangeBonus = 0.3f;    // 终结击范围加成
        
        // 运行时状态
        private int currentCombo = 0;
        private float lastAttackTime = -999f;
        private float comboTimer = 0f;
        private bool isInComboWindow = false;
        
        // 事件
        public event Action<int> OnComboStarted;      // 连招开始
        public event Action<int> OnComboProgress;     // 连招进展
        public event Action<int> OnComboFinished;     // 连招完成
        public event Action OnComboReset;             // 连招重置
        
        // 属性
        public int CurrentCombo => currentCombo;
        public int MaxCombo => maxComboCount;
        public float ComboProgress => Mathf.Clamp01(comboTimer / comboWindow);
        public bool IsInComboWindow => isInComboWindow;
        public bool CanContinueCombo => isInComboWindow && currentCombo < maxComboCount;
        public bool IsFinalHit => currentCombo >= maxComboCount - 1;
        
        /// <summary>
        /// 获取当前伤害倍率
        /// </summary>
        public float GetDamageMultiplier()
        {
            float multiplier = 1f + (currentCombo * damageBonusPerCombo);
            
            if (IsFinalHit)
            {
                multiplier *= finalHitMultiplier;
            }
            
            return multiplier;
        }
        
        /// <summary>
        /// 获取当前速度倍率
        /// </summary>
        public float GetSpeedMultiplier()
        {
            return 1f - (currentCombo * speedBonusPerCombo);
        }
        
        /// <summary>
        /// 获取当前范围倍率
        /// </summary>
        public float GetRangeMultiplier()
        {
            float multiplier = 1f + (currentCombo * rangeBonusPerCombo);
            
            if (IsFinalHit)
            {
                multiplier += finalHitRangeBonus;
            }
            
            return multiplier;
        }
        
        /// <summary>
        /// 记录一次攻击
        /// </summary>
        public void RecordAttack()
        {
            float currentTime = Time.time;
            float timeSinceLastAttack = currentTime - lastAttackTime;
            
            // 检查是否在连招窗口内
            if (timeSinceLastAttack <= comboWindow && currentCombo < maxComboCount)
            {
                // 继续连招
                currentCombo++;
                OnComboProgress?.Invoke(currentCombo);
            }
            else
            {
                // 开始新连招
                if (currentCombo > 0)
                {
                    OnComboReset?.Invoke();
                }
                currentCombo = 1;
                OnComboStarted?.Invoke(currentCombo);
            }
            
            // 检查是否完成连招
            if (currentCombo >= maxComboCount)
            {
                OnComboFinished?.Invoke(currentCombo);
            }
            
            lastAttackTime = currentTime;
            comboTimer = 0f;
            isInComboWindow = true;
        }
        
        /// <summary>
        /// 重置连招
        /// </summary>
        public void ResetCombo()
        {
            if (currentCombo > 0)
            {
                OnComboReset?.Invoke();
            }
            currentCombo = 0;
            comboTimer = 0f;
            isInComboWindow = false;
        }
        
        /// <summary>
        /// 强制设置连招数
        /// </summary>
        public void SetCombo(int combo)
        {
            currentCombo = Mathf.Clamp(combo, 0, maxComboCount);
        }
        
        private void Update()
        {
            if (!isInComboWindow) return;
            
            comboTimer += Time.deltaTime;
            
            // 检查连招窗口是否结束
            if (comboTimer >= comboWindow)
            {
                isInComboWindow = false;
            }
            
            // 检查连招重置
            if (Time.time - lastAttackTime >= comboResetTime && currentCombo > 0)
            {
                ResetCombo();
            }
        }
        
        /// <summary>
        /// 获取连招描述
        /// </summary>
        public string GetComboDescription()
        {
            if (currentCombo == 0) return "准备连击";
            if (currentCombo >= maxComboCount) return "终结击!";
            return $"连击 x{currentCombo}";
        }
    }
}
