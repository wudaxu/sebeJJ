using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 状态效果系统 - BUG-007修复
    /// 管理流血、中毒、燃烧等持续伤害效果
    /// </summary>
    public class StatusEffectSystem : MonoBehaviour
    {
        public static StatusEffectSystem Instance { get; private set; }
        
        [Header("效果设置")]
        [SerializeField] private float tickInterval = 0.5f; // 伤害触发间隔
        [SerializeField] private bool showEffectParticles = true;
        
        [Header("效果预制体")]
        [SerializeField] private GameObject bleedEffectPrefab;
        [SerializeField] private GameObject poisonEffectPrefab;
        [SerializeField] private GameObject burnEffectPrefab;
        [SerializeField] private GameObject stunEffectPrefab;
        [SerializeField] private GameObject slowEffectPrefab;
        
        // 活跃效果列表
        private Dictionary<GameObject, List<StatusEffect>> activeEffects = new Dictionary<GameObject, List<StatusEffect>>();
        private float lastTickTime;
        
        // 事件
        public event Action<GameObject, StatusEffectType, float> OnEffectApplied;
        public event Action<GameObject, StatusEffectType> OnEffectRemoved;
        public event Action<GameObject, StatusEffectType, float> OnEffectTick;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Update()
        {
            // 定期触发效果
            if (Time.time - lastTickTime >= tickInterval)
            {
                ProcessEffectTicks();
                lastTickTime = Time.time;
            }
            
            // 更新效果持续时间
            UpdateEffectDurations();
        }
        
        /// <summary>
        /// 应用状态效果
        /// </summary>
        public void ApplyEffect(GameObject target, StatusEffectType type, float duration, float damagePerTick, float tickRate = 0.5f)
        {
            if (target == null) return;
            
            // 获取或创建效果列表
            if (!activeEffects.ContainsKey(target))
            {
                activeEffects[target] = new List<StatusEffect>();
            }
            
            var effects = activeEffects[target];
            
            // 检查是否已有同类型效果
            var existingEffect = effects.Find(e => e.type == type);
            if (existingEffect != null)
            {
                // 刷新持续时间
                existingEffect.remainingDuration = Mathf.Max(existingEffect.remainingDuration, duration);
                existingEffect.damagePerTick = Mathf.Max(existingEffect.damagePerTick, damagePerTick);
            }
            else
            {
                // 创建新效果
                var newEffect = new StatusEffect
                {
                    type = type,
                    duration = duration,
                    remainingDuration = duration,
                    damagePerTick = damagePerTick,
                    tickRate = tickRate,
                    nextTickTime = Time.time + tickRate,
                    effectObject = SpawnEffectVisuals(target, type)
                };
                
                effects.Add(newEffect);
                
                // 应用即时效果
                ApplyImmediateEffect(target, type);
            }
            
            OnEffectApplied?.Invoke(target, type, duration);
            Debug.Log($"[StatusEffect] 对 {target.name} 应用了 {type} 效果，持续 {duration} 秒");
        }
        
        /// <summary>
        /// 处理效果触发
        /// </summary>
        private void ProcessEffectTicks()
        {
            var targetsToRemove = new List<GameObject>();
            
            foreach (var kvp in activeEffects)
            {
                var target = kvp.Key;
                var effects = kvp.Value;
                
                if (target == null)
                {
                    targetsToRemove.Add(target);
                    continue;
                }
                
                foreach (var effect in effects.ToList())
                {
                    if (Time.time >= effect.nextTickTime)
                    {
                        // 触发伤害
                        ApplyDamage(target, effect.damagePerTick, effect.type);
                        effect.nextTickTime = Time.time + effect.tickRate;
                        
                        OnEffectTick?.Invoke(target, effect.type, effect.damagePerTick);
                    }
                }
            }
            
            // 清理无效目标
            foreach (var target in targetsToRemove)
            {
                activeEffects.Remove(target);
            }
        }
        
        /// <summary>
        /// 更新效果持续时间
        /// </summary>
        private void UpdateEffectDurations()
        {
            var targetsToRemove = new List<GameObject>();
            
            foreach (var kvp in activeEffects)
            {
                var target = kvp.Key;
                var effects = kvp.Value;
                
                if (target == null)
                {
                    targetsToRemove.Add(target);
                    continue;
                }
                
                for (int i = effects.Count - 1; i >= 0; i--)
                {
                    var effect = effects[i];
                    effect.remainingDuration -= Time.deltaTime;
                    
                    if (effect.remainingDuration <= 0)
                    {
                        // 效果结束
                        RemoveEffect(target, effect);
                        effects.RemoveAt(i);
                    }
                }
                
                if (effects.Count == 0)
                {
                    targetsToRemove.Add(target);
                }
            }
            
            foreach (var target in targetsToRemove)
            {
                activeEffects.Remove(target);
            }
        }
        
        /// <summary>
        /// 应用伤害
        /// </summary>
        private void ApplyDamage(GameObject target, float damage, StatusEffectType type)
        {
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                
                // 显示伤害数字
                ShowDamageNumber(target.transform.position, damage, type);
            }
        }
        
        /// <summary>
        /// 应用即时效果
        /// </summary>
        private void ApplyImmediateEffect(GameObject target, StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.Stun:
                    var enemyAI = target.GetComponent<AI.AIStateMachine>();
                    enemyAI?.SetStunned(true);
                    break;
                    
                case StatusEffectType.Slow:
                    var enemy = target.GetComponent<Enemies.EnemyBase>();
                    if (enemy != null)
                    {
                        // 应用减速
                        enemy.ApplySpeedModifier(0.5f);
                    }
                    break;
            }
        }
        
        /// <summary>
        /// 移除效果
        /// </summary>
        private void RemoveEffect(GameObject target, StatusEffect effect)
        {
            // 移除视觉效果
            if (effect.effectObject != null)
            {
                Destroy(effect.effectObject);
            }
            
            // 移除即时效果
            switch (effect.type)
            {
                case StatusEffectType.Stun:
                    var enemyAI = target.GetComponent<AI.AIStateMachine>();
                    enemyAI?.SetStunned(false);
                    break;
                    
                case StatusEffectType.Slow:
                    var enemy = target.GetComponent<Enemies.EnemyBase>();
                    enemy?.RemoveSpeedModifier();
                    break;
            }
            
            OnEffectRemoved?.Invoke(target, effect.type);
        }
        
        /// <summary>
        /// 生成视觉效果
        /// </summary>
        private GameObject SpawnEffectVisuals(GameObject target, StatusEffectType type)
        {
            if (!showEffectParticles) return null;
            
            GameObject prefab = type switch
            {
                StatusEffectType.Bleed => bleedEffectPrefab,
                StatusEffectType.Poison => poisonEffectPrefab,
                StatusEffectType.Burn => burnEffectPrefab,
                StatusEffectType.Stun => stunEffectPrefab,
                StatusEffectType.Slow => slowEffectPrefab,
                _ => null
            };
            
            if (prefab != null)
            {
                var instance = Instantiate(prefab, target.transform);
                instance.transform.localPosition = Vector3.up;
                return instance;
            }
            
            return null;
        }
        
        /// <summary>
        /// 显示伤害数字
        /// </summary>
        private void ShowDamageNumber(Vector3 position, float damage, StatusEffectType type)
        {
            Color color = type switch
            {
                StatusEffectType.Bleed => Color.red,
                StatusEffectType.Poison => Color.green,
                StatusEffectType.Burn => Color.yellow,
                _ => Color.white
            };
            
            // 调用伤害数字显示系统
            Utils.EffectManager.Instance?.ShowDamageNumber(position, Mathf.RoundToInt(damage), color);
        }
        
        /// <summary>
        /// 检查目标是否有特定效果
        /// </summary>
        public bool HasEffect(GameObject target, StatusEffectType type)
        {
            if (!activeEffects.ContainsKey(target)) return false;
            return activeEffects[target].Any(e => e.type == type);
        }
        
        /// <summary>
        /// 清除目标的所有效果
        /// </summary>
        public void ClearAllEffects(GameObject target)
        {
            if (!activeEffects.ContainsKey(target)) return;
            
            foreach (var effect in activeEffects[target])
            {
                if (effect.effectObject != null)
                {
                    Destroy(effect.effectObject);
                }
            }
            
            activeEffects.Remove(target);
        }
        
        /// <summary>
        /// 获取目标的效果列表
        /// </summary>
        public List<StatusEffect> GetActiveEffects(GameObject target)
        {
            if (activeEffects.ContainsKey(target))
            {
                return new List<StatusEffect>(activeEffects[target]);
            }
            return new List<StatusEffect>();
        }
    }
    
    /// <summary>
    /// 状态效果类型
    /// </summary>
    public enum StatusEffectType
    {
        Bleed,      // 流血
        Poison,     // 中毒
        Burn,       // 燃烧
        Stun,       // 眩晕
        Slow,       // 减速
        Freeze,     // 冰冻
        Shock       // 电击
    }
    
    /// <summary>
    /// 状态效果数据
    /// </summary>
    [Serializable]
    public class StatusEffect
    {
        public StatusEffectType type;
        public float duration;
        public float remainingDuration;
        public float damagePerTick;
        public float tickRate;
        public float nextTickTime;
        public GameObject effectObject;
    }
}
