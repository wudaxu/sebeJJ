using UnityEngine;
using System;
using System.Collections.Generic;
using SebeJJ.Enemies;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 战斗管理器 - CB-004
    /// 整合所有战斗相关系统，提供统一的战斗体验控制
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }
        
        [Header("系统引用")]
        [SerializeField] private CombatFeedback combatFeedback;
        [SerializeField] private CombatWarningSystem warningSystem;
        [SerializeField] private CombatMusicController musicController;
        [SerializeField] private EnemySpawnController spawnController;
        
        [Header("玩家引用")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private CombatStats playerStats;
        
        [Header("战斗设置")]
        [SerializeField] private float combatIntensityUpdateInterval = 1f;
        [SerializeField] private float combatCheckRadius = 30f;
        
        // 运行时状态
        private List<EnemyBase> activeEnemies = new List<EnemyBase>();
        private float intensityUpdateTimer = 0f;
        private bool isInCombat = false;
        
        // 事件
        public event Action OnCombatStart;
        public event Action OnCombatEnd;
        public event Action<float> OnCombatIntensityChanged;
        
        // 属性
        public bool IsInCombat => isInCombat;
        public int ActiveEnemyCount => activeEnemies.Count;
        public float CombatIntensity { get; private set; } = 0f;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // 自动查找引用
            if (combatFeedback == null) combatFeedback = CombatFeedback.Instance;
            if (warningSystem == null) warningSystem = CombatWarningSystem.Instance;
            if (musicController == null) musicController = CombatMusicController.Instance;
        }
        
        private void Start()
        {
            // CR-002修复: 添加空值检查
            if (playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                    playerStats = player.GetComponent<CombatStats>();
                }
                else
                {
                    Debug.LogError("[CombatManager] 未找到玩家对象! 请确保玩家对象有'Player'标签。");
                }
            }
            
            if (spawnController != null)
            {
                spawnController.OnEnemySpawned += OnEnemySpawnedHandler;
            }
        }
        
        private void Update()
        {
            // 更新活跃敌人列表
            UpdateActiveEnemies();
            
            // 更新战斗状态
            UpdateCombatState();
            
            // 更新战斗强度
            intensityUpdateTimer += Time.deltaTime;
            if (intensityUpdateTimer >= combatIntensityUpdateInterval)
            {
                intensityUpdateTimer = 0f;
                UpdateCombatIntensity();
            }
        }
        
        /// <summary>
        /// 更新活跃敌人列表
        /// </summary>
        private void UpdateActiveEnemies()
        {
            activeEnemies.RemoveAll(e => e == null || e.IsDead);
        }
        
        /// <summary>
        /// 更新战斗状态
        /// </summary>
        private void UpdateCombatState()
        {
            bool wasInCombat = isInCombat;
            isInCombat = activeEnemies.Count > 0;
            
            if (isInCombat && !wasInCombat)
            {
                OnCombatStart?.Invoke();
            }
            else if (!isInCombat && wasInCombat)
            {
                OnCombatEnd?.Invoke();
            }
        }
        
        /// <summary>
        /// 更新战斗强度
        /// </summary>
        private void UpdateCombatIntensity()
        {
            if (!isInCombat)
            {
                CombatIntensity = 0f;
                return;
            }
            
            // 计算战斗强度
            float enemyCountFactor = activeEnemies.Count;
            float healthFactor = playerStats != null ? 1f - playerStats.HealthPercent : 1f;
            
            CombatIntensity = enemyCountFactor * (1f + healthFactor);
            
            OnCombatIntensityChanged?.Invoke(CombatIntensity);
            
            // 更新音乐
            if (musicController != null)
            {
                float playerHealth = playerStats != null ? playerStats.HealthPercent : 1f;
                musicController.UpdateCombatIntensity(enemyCountFactor, playerHealth);
            }
        }
        
        /// <summary>
        /// 敌人生成处理
        /// </summary>
        private void OnEnemySpawnedHandler(GameObject enemy)
        {
            var enemyBase = enemy.GetComponent<EnemyBase>();
            if (enemyBase != null)
            {
                activeEnemies.Add(enemyBase);
                enemyBase.OnDeath += () => OnEnemyDeathHandler(enemyBase);
            }
        }
        
        /// <summary>
        /// 敌人死亡处理
        /// </summary>
        private void OnEnemyDeathHandler(EnemyBase enemy)
        {
            activeEnemies.Remove(enemy);
            
            // 触发击杀反馈
            if (combatFeedback != null)
            {
                combatFeedback.TriggerKillFeedback(enemy.transform.position);
            }
        }
        
        /// <summary>
        /// 注册敌人
        /// </summary>
        public void RegisterEnemy(EnemyBase enemy)
        {
            if (enemy != null && !activeEnemies.Contains(enemy))
            {
                activeEnemies.Add(enemy);
                enemy.OnDeath += () => OnEnemyDeathHandler(enemy);
            }
        }
        
        /// <summary>
        /// 获取最近的敌人
        /// </summary>
        public EnemyBase GetNearestEnemy()
        {
            if (activeEnemies.Count == 0 || playerTransform == null) return null;
            
            EnemyBase nearest = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var enemy in activeEnemies)
            {
                if (enemy == null || enemy.IsDead) continue;
                
                float distance = Vector3.Distance(playerTransform.position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = enemy;
                }
            }
            
            return nearest;
        }
        
        /// <summary>
        /// 获取范围内的敌人
        /// </summary>
        public List<EnemyBase> GetEnemiesInRange(float range)
        {
            List<EnemyBase> result = new List<EnemyBase>();
            
            if (playerTransform == null) return result;
            
            foreach (var enemy in activeEnemies)
            {
                if (enemy == null || enemy.IsDead) continue;
                
                float distance = Vector3.Distance(playerTransform.position, enemy.transform.position);
                if (distance <= range)
                {
                    result.Add(enemy);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 清除所有敌人
        /// </summary>
        public void ClearAllEnemies()
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null && !enemy.IsDead)
                {
                    enemy.TakeDamage(enemy.MaxHealth);
                }
            }
            activeEnemies.Clear();
        }
        
        /// <summary>
        /// 开始战斗
        /// </summary>
        public void StartCombat()
        {
            if (spawnController != null)
            {
                spawnController.StartSpawning();
            }
        }
        
        /// <summary>
        /// 结束战斗
        /// </summary>
        public void EndCombat()
        {
            if (spawnController != null)
            {
                spawnController.StopSpawning();
            }
            
            ClearAllEnemies();
        }
        
        private void OnDestroy()
        {
            if (spawnController != null)
            {
                spawnController.OnEnemySpawned -= OnEnemySpawnedHandler;
            }
        }
    }
}
