/**
 * @file EnemyDamageBridge.cs
 * @brief 敌人伤害桥接器 - 连接EnemyBase与CombatStats系统
 * @description 让敌人可以使用新的DamageInfo伤害系统
 * @author 系统集成工程师
 * @date 2026-02-27
 */

using UnityEngine;
using SebeJJ.Combat;
using SebeJJ.Enemies;
using System;

namespace SebeJJ.Integration
{
    /// <summary>
    /// 敌人伤害桥接器
    /// 为EnemyBase添加对CombatStats/DamageInfo系统的支持
    /// </summary>
    [RequireComponent(typeof(EnemyBase))]
    public class EnemyDamageBridge : MonoBehaviour, IDamageable
    {
        private EnemyBase enemyBase;
        private CombatStats combatStats;

        // 事件
        public event EventHandler<DamageEventArgs> OnDamageTaken;
        public event EventHandler OnDeath;

        // IDamageable接口实现
        public float CurrentHealth => combatStats?.CurrentHealth ?? enemyBase?.CurrentHealth ?? 0;
        public float MaxHealth => combatStats?.MaxHealth ?? enemyBase?.MaxHealth ?? 0;
        public bool IsAlive => combatStats?.IsAlive ?? !enemyBase?.IsDead ?? false;

        private void Awake()
        {
            enemyBase = GetComponent<EnemyBase>();

            // 尝试获取或添加CombatStats
            combatStats = GetComponent<CombatStats>();
            if (combatStats == null)
            {
                combatStats = gameObject.AddComponent<CombatStats>();
                InitializeCombatStats();
            }

            // 订阅CombatStats事件
            combatStats.OnDamageTaken += HandleCombatStatsDamage;
            combatStats.OnDeath += HandleCombatStatsDeath;
        }

        private void Start()
        {
            // 注册到集成系统
            CombatIntegrationSystem.Instance?.RegisterEnemy(enemyBase);
        }

        private void OnDestroy()
        {
            if (combatStats != null)
            {
                combatStats.OnDamageTaken -= HandleCombatStatsDamage;
                combatStats.OnDeath -= HandleCombatStatsDeath;
            }
        }

        /// <summary>
        /// 初始化CombatStats属性
        /// </summary>
        private void InitializeCombatStats()
        {
            if (enemyBase == null || combatStats == null) return;

            // 从EnemyStats同步属性
            var enemyStats = GetEnemyStats();
            if (enemyStats != null)
            {
                // 使用反射或直接设置（这里简化处理）
                combatStats.SetMaxHealth(enemyStats.maxHealth);
                combatStats.SetMaxShield(0); // 机械鱼没有护盾
                combatStats.SetMaxArmor(enemyStats.defenseReduction * 100); // 防御转换为护甲
            }
        }

        /// <summary>
        /// 获取敌人属性（通过反射）
        /// </summary>
        private EnemyStats GetEnemyStats()
        {
            if (enemyBase == null) return null;

            // 使用反射获取stats字段
            var statsField = typeof(EnemyBase).GetField("stats", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public);
            
            if (statsField != null)
            {
                return statsField.GetValue(enemyBase) as EnemyStats;
            }

            return null;
        }

        /// <summary>
        /// IDamageable接口 - 接受DamageInfo伤害
        /// </summary>
        public void TakeDamage(DamageInfo damageInfo)
        {
            if (combatStats != null)
            {
                combatStats.TakeDamage(damageInfo);
            }
            else
            {
                // 回退到旧系统
                enemyBase?.TakeDamage(damageInfo.BaseDamage, 
                    damageInfo.Attacker?.transform);
            }
        }

        /// <summary>
        /// 处理CombatStats受伤事件
        /// </summary>
        private void HandleCombatStatsDamage(object sender, DamageEventArgs e)
        {
            // 转发到EnemyBase的旧事件系统
            enemyBase?.SendMessage("OnTakeDamage", e.FinalDamage, SendMessageOptions.DontRequireReceiver);

            // 触发自己的事件
            OnDamageTaken?.Invoke(this, e);

            // 显示伤害数字
            CombatIntegrationSystem.Instance?.ShowDamageNumber(
                e.FinalDamage, transform.position, e.DamageInfo.IsCritical);

            // 触发战斗反馈
            if (CombatFeedback.Instance != null)
            {
                if (e.IsKillingBlow)
                {
                    CombatFeedback.Instance.TriggerKillFeedback(transform.position);
                }
                else
                {
                    CombatFeedback.Instance.TriggerImpactFeedback(0.2f, 0.03f);
                }
            }
        }

        /// <summary>
        /// 处理CombatStats死亡事件
        /// </summary>
        private void HandleCombatStatsDeath(object sender, EventArgs e)
        {
            OnDeath?.Invoke(this, EventArgs.Empty);

            // 触发EnemyBase的死亡
            if (enemyBase != null && !enemyBase.IsDead)
            {
                // 调用Die方法（通过反射或消息）
                enemyBase.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
            }
        }

        /// <summary>
        /// 外部调用 - 造成DamageInfo伤害
        /// </summary>
        public void ApplyDamage(DamageInfo damageInfo)
        {
            TakeDamage(damageInfo);
        }

        /// <summary>
        /// 外部调用 - 造成基础伤害
        /// </summary>
        public void ApplyDamage(float damage, Transform attacker = null)
        {
            var damageInfo = new DamageInfo(damage, DamageType.Kinetic);
            damageInfo.Attacker = attacker?.gameObject;
            TakeDamage(damageInfo);
        }
    }
}
