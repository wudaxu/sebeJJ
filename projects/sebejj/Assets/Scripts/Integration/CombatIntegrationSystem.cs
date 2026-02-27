/**
 * @file CombatIntegrationSystem.cs
 * @brief 战斗集成系统 - 连接所有战斗相关组件
 * @description 整合MechController、CombatStats、EnemyBase、DamageCalculator、WeaponManager、CombatFeedback
 * @author 系统集成工程师
 * @date 2026-02-27
 */

using UnityEngine;
using SebeJJ.Combat;
using SebeJJ.Player;
using SebeJJ.Enemies;
using System;

namespace SebeJJ.Integration
{
    /// <summary>
    /// 战斗集成系统 - 核心集成类
    /// 负责连接机甲、敌人、武器、反馈等所有战斗相关系统
    /// </summary>
    public class CombatIntegrationSystem : MonoBehaviour
    {
        public static CombatIntegrationSystem Instance { get; private set; }

        [Header("玩家引用")]
        [SerializeField] private MechController mechController;
        [SerializeField] private CombatStats mechCombatStats;
        [SerializeField] private WeaponManager weaponManager;

        [Header("战斗反馈")]
        [SerializeField] private CombatFeedback combatFeedback;
        [SerializeField] private bool enableCameraShake = true;
        [SerializeField] private bool enableHitStop = true;

        [Header("伤害数字")]
        [SerializeField] private DamageNumber damageNumberPrefab;
        [SerializeField] private Transform damageNumberCanvas;

        [Header("敌人管理")]
        [SerializeField] private Transform enemyContainer;
        [SerializeField] private LayerMask enemyLayers;

        // 事件
        public event Action<EnemyBase> OnEnemyKilled;
        public event Action<float> OnPlayerDamaged;
        public event Action<float, GameObject> OnEnemyDamaged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // 自动查找引用
            FindReferences();
        }

        private void Start()
        {
            InitializePlayerSystems();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #region 初始化

        /// <summary>
        /// 自动查找必要的引用
        /// </summary>
        private void FindReferences()
        {
            if (mechController == null)
                mechController = MechController.Instance;

            if (mechController == null)
                mechController = FindObjectOfType<MechController>();

            if (mechCombatStats == null && mechController != null)
                mechCombatStats = mechController.GetComponent<CombatStats>();

            if (weaponManager == null && mechController != null)
                weaponManager = mechController.GetComponent<WeaponManager>();

            if (combatFeedback == null)
                combatFeedback = CombatFeedback.Instance;

            if (combatFeedback == null)
                combatFeedback = FindObjectOfType<CombatFeedback>();

            if (enemyContainer == null)
            {
                var container = GameObject.Find("Enemies");
                if (container != null)
                    enemyContainer = container.transform;
            }
        }

        /// <summary>
        /// 初始化玩家系统连接
        /// </summary>
        private void InitializePlayerSystems()
        {
            // 确保MechController有CombatStats组件
            if (mechController != null && mechCombatStats == null)
            {
                mechCombatStats = mechController.GetComponent<CombatStats>();
                if (mechCombatStats == null)
                {
                    mechCombatStats = mechController.gameObject.AddComponent<CombatStats>();
                    Debug.Log("[CombatIntegrationSystem] 为MechController添加了CombatStats组件");
                }
            }

            // 确保MechController有WeaponManager组件
            if (mechController != null && weaponManager == null)
            {
                weaponManager = mechController.GetComponent<WeaponManager>();
                if (weaponManager == null)
                {
                    weaponManager = mechController.gameObject.AddComponent<WeaponManager>();
                    Debug.Log("[CombatIntegrationSystem] 为MechController添加了WeaponManager组件");
                }
            }

            // 确保场景中有CombatFeedback
            if (combatFeedback == null)
            {
                var go = new GameObject("CombatFeedback");
                combatFeedback = go.AddComponent<CombatFeedback>();
                Debug.Log("[CombatIntegrationSystem] 创建了CombatFeedback对象");
            }
        }

        /// <summary>
        /// 订阅所有战斗事件
        /// </summary>
        private void SubscribeToEvents()
        {
            // 订阅玩家受伤事件
            if (mechCombatStats != null)
            {
                mechCombatStats.OnDamageTaken += HandlePlayerDamageTaken;
                mechCombatStats.OnDeath += HandlePlayerDeath;
                mechCombatStats.OnShieldBroken += HandlePlayerShieldBroken;
            }

            // 订阅武器事件
            if (weaponManager != null)
            {
                weaponManager.OnWeaponChanged += HandleWeaponChanged;
            }
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (mechCombatStats != null)
            {
                mechCombatStats.OnDamageTaken -= HandlePlayerDamageTaken;
                mechCombatStats.OnDeath -= HandlePlayerDeath;
                mechCombatStats.OnShieldBroken -= HandlePlayerShieldBroken;
            }

            if (weaponManager != null)
            {
                weaponManager.OnWeaponChanged -= HandleWeaponChanged;
            }
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 处理玩家受到伤害
        /// </summary>
        private void HandlePlayerDamageTaken(object sender, DamageEventArgs e)
        {
            // 触发相机震动
            if (enableCameraShake && combatFeedback != null)
            {
                float shakeIntensity = Mathf.Clamp01(e.FinalDamage / 50f) * 0.5f;
                combatFeedback.TriggerScreenShake(shakeIntensity, 0.2f);
            }

            // 触发命中停顿
            if (enableHitStop && combatFeedback != null)
            {
                combatFeedback.TriggerHitStop(0.05f);
            }

            // 显示伤害数字
            ShowDamageNumber(e.FinalDamage, mechController.transform.position, false, false, true);

            OnPlayerDamaged?.Invoke(e.FinalDamage);

            Debug.Log($"[CombatIntegrationSystem] 玩家受到 {e.FinalDamage:F1} 点伤害");
        }

        /// <summary>
        /// 处理玩家死亡
        /// </summary>
        private void HandlePlayerDeath(object sender, EventArgs e)
        {
            Debug.Log("[CombatIntegrationSystem] 玩家死亡!");
            
            if (combatFeedback != null)
            {
                combatFeedback.TriggerScreenShake(0.8f, 0.5f);
                combatFeedback.TriggerTimeScale(0.2f, 0.3f);
            }
        }

        /// <summary>
        /// 处理玩家护盾破碎
        /// </summary>
        private void HandlePlayerShieldBroken(object sender, EventArgs e)
        {
            Debug.Log("[CombatIntegrationSystem] 玩家护盾破碎!");
            
            if (combatFeedback != null)
            {
                combatFeedback.TriggerShieldBreakFeedback(mechController.transform.position);
            }
        }

        /// <summary>
        /// 处理武器切换
        /// </summary>
        private void HandleWeaponChanged(WeaponBase weapon, int index)
        {
            Debug.Log($"[CombatIntegrationSystem] 武器切换到: {weapon?.WeaponData?.weaponName ?? "None"}");
        }

        #endregion

        #region 敌人集成

        /// <summary>
        /// 注册敌人到集成系统
        /// </summary>
        public void RegisterEnemy(EnemyBase enemy)
        {
            if (enemy == null) return;

            // 订阅敌人事件
            enemy.OnTakeDamage += (damage) => HandleEnemyTakeDamage(enemy, damage);
            enemy.OnDeath += () => HandleEnemyDeath(enemy);

            Debug.Log($"[CombatIntegrationSystem] 注册敌人: {enemy.Type}");
        }

        /// <summary>
        /// 处理敌人受到伤害
        /// </summary>
        private void HandleEnemyTakeDamage(EnemyBase enemy, float damage)
        {
            // 显示伤害数字
            ShowDamageNumber(damage, enemy.transform.position, false, false, false);

            // 触发受击反馈
            if (combatFeedback != null)
            {
                combatFeedback.TriggerImpactFeedback(0.2f, 0.03f);
            }

            OnEnemyDamaged?.Invoke(damage, enemy.gameObject);

            Debug.Log($"[CombatIntegrationSystem] 敌人 {enemy.Type} 受到 {damage:F1} 点伤害");
        }

        /// <summary>
        /// 处理敌人死亡
        /// </summary>
        private void HandleEnemyDeath(EnemyBase enemy)
        {
            // 触发击杀反馈
            if (combatFeedback != null)
            {
                combatFeedback.TriggerKillFeedback(enemy.transform.position);
            }

            // 生成掉落
            LootDropSystem.Instance?.SpawnLoot(enemy.transform.position, enemy.Type);

            OnEnemyKilled?.Invoke(enemy);

            Debug.Log($"[CombatIntegrationSystem] 敌人 {enemy.Type} 被击杀!");
        }

        #endregion

        #region 伤害数字

        /// <summary>
        /// 显示伤害数字
        /// </summary>
        public void ShowDamageNumber(float damage, Vector3 position, bool isCritical = false, 
            bool isHeal = false, bool isPlayer = false)
        {
            if (damageNumberPrefab == null)
            {
                // 尝试使用CombatFeedback的预设
                if (combatFeedback != null)
                {
                    combatFeedback.ShowDamageNumber(damage, position, isCritical, isHeal);
                }
                return;
            }

            // 实例化伤害数字
            Vector3 spawnPos = position + Vector3.up * 0.5f + UnityEngine.Random.insideUnitSphere * 0.3f;
            var damageNumber = Instantiate(damageNumberPrefab, spawnPos, Quaternion.identity, damageNumberCanvas);
            
            // 如果是玩家受到伤害，调整显示
            if (isPlayer)
            {
                damageNumber.Initialize(damage, false, false, false);
            }
            else
            {
                damageNumber.Initialize(damage, isCritical, isHeal, false);
            }
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 对玩家造成伤害
        /// </summary>
        public void DamagePlayer(DamageInfo damageInfo)
        {
            mechCombatStats?.TakeDamage(damageInfo);
        }

        /// <summary>
        /// 治疗玩家
        /// </summary>
        public void HealPlayer(float amount)
        {
            mechCombatStats?.Heal(amount);
            ShowDamageNumber(amount, mechController.transform.position, false, true, true);
        }

        /// <summary>
        /// 获取玩家CombatStats
        /// </summary>
        public CombatStats GetPlayerCombatStats()
        {
            return mechCombatStats;
        }

        /// <summary>
        /// 获取玩家武器管理器
        /// </summary>
        public WeaponManager GetPlayerWeaponManager()
        {
            return weaponManager;
        }

        #endregion
    }
}
