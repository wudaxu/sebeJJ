using UnityEngine;
using System.Collections.Generic;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 持续伤害效果 - DM-007
    /// </summary>
    public class DamageOverTime : MonoBehaviour
    {
        [System.Serializable]
        public class DotEffect
        {
            public string effectName;
            public DamageType damageType;
            public float damagePerTick;
            public float tickInterval;
            public float duration;
            public GameObject visualEffect;
            
            [HideInInspector] public float elapsedTime;
            [HideInInspector] public float nextTickTime;
            [HideInInspector] public GameObject activeEffect;
        }

        private CombatStats combatStats;
        private List<DotEffect> activeEffects = new List<DotEffect>();

        private void Awake()
        {
            combatStats = GetComponent<CombatStats>();
        }

        private void Update()
        {
            UpdateDotEffects();
        }

        /// <summary>
        /// 添加持续伤害效果
        /// </summary>
        public void ApplyDot(DotEffect effect, GameObject attacker = null)
        {
            // 检查是否已有相同效果，刷新持续时间
            var existing = activeEffects.Find(e => e.effectName == effect.effectName);
            if (existing != null)
            {
                existing.elapsedTime = 0;
                existing.duration = Mathf.Max(existing.duration, effect.duration);
                return;
            }

            // 添加新效果
            var newEffect = new DotEffect
            {
                effectName = effect.effectName,
                damageType = effect.damageType,
                damagePerTick = effect.damagePerTick,
                tickInterval = effect.tickInterval,
                duration = effect.duration,
                visualEffect = effect.visualEffect,
                elapsedTime = 0,
                nextTickTime = Time.time + effect.tickInterval
            };

            // 创建视觉效果
            if (newEffect.visualEffect != null)
            {
                newEffect.activeEffect = Instantiate(newEffect.visualEffect, transform);
            }

            activeEffects.Add(newEffect);
        }

        /// <summary>
        /// 移除持续伤害效果
        /// </summary>
        public void RemoveDot(string effectName)
        {
            var effect = activeEffects.Find(e => e.effectName == effectName);
            if (effect != null)
            {
                if (effect.activeEffect != null)
                    Destroy(effect.activeEffect);
                
                activeEffects.Remove(effect);
            }
        }

        /// <summary>
        /// 更新所有持续伤害效果
        /// </summary>
        private void UpdateDotEffects()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = activeEffects[i];
                effect.elapsedTime += Time.deltaTime;

                // 检查是否到期
                if (effect.elapsedTime >= effect.duration)
                {
                    if (effect.activeEffect != null)
                        Destroy(effect.activeEffect);
                    
                    activeEffects.RemoveAt(i);
                    continue;
                }

                // 触发伤害
                if (Time.time >= effect.nextTickTime)
                {
                    effect.nextTickTime = Time.time + effect.tickInterval;
                    ApplyDotDamage(effect);
                }
            }
        }

        /// <summary>
        /// 应用单次持续伤害
        /// </summary>
        private void ApplyDotDamage(DotEffect effect)
        {
            if (combatStats == null) return;

            var damageInfo = new DamageInfo(effect.damagePerTick, effect.damageType);
            combatStats.TakeDamage(damageInfo);
        }

        /// <summary>
        /// 清除所有持续伤害
        /// </summary>
        public void ClearAllDots()
        {
            foreach (var effect in activeEffects)
            {
                if (effect.activeEffect != null)
                    Destroy(effect.activeEffect);
            }
            activeEffects.Clear();
        }

        /// <summary>
        /// 检查是否有指定持续伤害
        /// </summary>
        public bool HasDot(string effectName)
        {
            return activeEffects.Exists(e => e.effectName == effectName);
        }
    }
}