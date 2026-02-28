/**
 * @file CombatSceneManager.cs
 * @brief 战斗场景管理器 - 50米深度测试区
 * @description 管理战斗测试场景的生成、波次、胜利条件等
 * @author 系统集成工程师
 * @date 2026-02-27
 */

using UnityEngine;
using SebeJJ.Enemies;
using SebeJJ.Player;
using System.Collections;
using System.Collections.Generic;

namespace SebeJJ.Integration
{
    /// <summary>
    /// 敌人生成点
    /// </summary>
    [System.Serializable]
    public class SpawnPoint
    {
        public string id;
        public Transform transform;
        public float spawnRadius = 2f;
        public bool isOccupied = false;
    }

    /// <summary>
    /// 敌人波次配置
    /// </summary>
    [System.Serializable]
    public class EnemyWave
    {
        public string waveName;
        public float delayBeforeSpawn = 2f;
        public List<EnemySpawnEntry> enemies = new List<EnemySpawnEntry>();
        public bool waitForClear = true; // 是否等待清理完才进入下一波
    }

    /// <summary>
    /// 敌人生成条目
    /// </summary>
    [System.Serializable]
    public class EnemySpawnEntry
    {
        public EnemyType enemyType;
        public GameObject enemyPrefab;
        public int count = 1;
        public float spawnInterval = 0.5f;
        public bool useRandomSpawnPoint = true;
        public int specificSpawnPointIndex = -1;
    }

    /// <summary>
    /// 战斗场景管理器
    /// </summary>
    public class CombatSceneManager : MonoBehaviour
    {
        public static CombatSceneManager Instance { get; private set; }

        [Header("场景设置")]
        [SerializeField] private string sceneName = "50米深度测试区";
        [SerializeField] private float depth = 50f;
        [SerializeField] private float oxygenConsumptionMultiplier = 1.2f;

        [Header("生成设置")]
        [SerializeField] private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
        [SerializeField] private Transform enemyContainer;

        [Header("波次配置")]
        [SerializeField] private List<EnemyWave> waves = new List<EnemyWave>();
        [SerializeField] private bool autoStartWaves = true;

        [Header("敌人预制体")]
        [SerializeField] private GameObject mechFishPrefab;
        [SerializeField] private GameObject mechCrabPrefab;
        [SerializeField] private GameObject mechJellyfishPrefab;

        [Header("玩家设置")]
        [SerializeField] private Transform playerSpawnPoint;

        // 状态
        private int currentWaveIndex = -1;
        private bool isWaveActive = false;
        private List<EnemyBase> activeEnemies = new List<EnemyBase>();
        private bool isSceneComplete = false;

        // 事件
        public System.Action OnSceneStarted;
        public System.Action OnSceneCompleted;
        public System.Action<int> OnWaveStarted;
        public System.Action<int> OnWaveCompleted;
        public System.Action<EnemyBase> OnEnemySpawned;
        public System.Action<EnemyBase> OnEnemyKilled;

        // 属性
        public int CurrentWave => currentWaveIndex + 1;
        public int TotalWaves => waves.Count;
        public int ActiveEnemyCount => activeEnemies.Count;
        public bool IsSceneComplete => isSceneComplete;
        public bool IsWaveActive => isWaveActive;

        private void Awake()
        {
            if (Instance != null && Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // 查找或创建敌人容器
            if (enemyContainer == null)
            {
                var container = GameObject.Find("Enemies");
                if (container == null)
                {
                    container = new GameObject("Enemies");
                }
                enemyContainer = container.transform;
            }
        }

        private void Start()
        {
            InitializeScene();

            if (autoStartWaves)
            {
                StartCoroutine(StartWavesCoroutine());
            }
        }

        private void OnDestroy()
        {
            // 清理事件订阅
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    enemy.OnDeath -= HandleEnemyDeath;
                }
            }
        }

        #region 场景初始化

        /// <summary>
        /// 初始化场景
        /// </summary>
        private void InitializeScene()
        {
            Debug.Log($"[CombatSceneManager] 初始化场景: {sceneName} (深度: {depth}米)");

            // 设置深度影响
            ApplyDepthEffects();

            // 生成玩家
            SpawnPlayer();

            // 查找生成点
            FindSpawnPoints();

            OnSceneStarted?.Invoke();
        }

        /// <summary>
        /// 应用深度效果
        /// </summary>
        private void ApplyDepthEffects()
        {
            // 设置氧气消耗倍率
            var diveManager = Core.GameManager.Instance?.diveManager;
            if (diveManager != null)
            {
                // 通过反射或公共方法设置
                Debug.Log($"[CombatSceneManager] 设置深度效果: 氧气消耗倍率 {oxygenConsumptionMultiplier}");
            }
        }

        /// <summary>
        /// 生成玩家
        /// </summary>
        private void SpawnPlayer()
        {
            if (MechController.Instance != null)
            {
                if (playerSpawnPoint != null)
                {
                    MechController.Instance.transform.position = playerSpawnPoint.position;
                }
            }
            else
            {
                Debug.LogWarning("[CombatSceneManager] 未找到MechController实例");
            }
        }

        /// <summary>
        /// 查找场景中的生成点
        /// </summary>
        private void FindSpawnPoints()
        {
            if (spawnPoints.Count > 0) return;

            // 自动查找标记为SpawnPoint的对象
            var points = GameObject.FindGameObjectsWithTag("SpawnPoint");
            foreach (var point in points)
            {
                spawnPoints.Add(new SpawnPoint
                {
                    id = point.name,
                    transform = point.transform,
                    spawnRadius = 1f
                });
            }

            // 如果没有找到，创建默认生成点
            if (spawnPoints.Count == 0)
            {
                CreateDefaultSpawnPoints();
            }

            Debug.Log($"[CombatSceneManager] 找到 {spawnPoints.Count} 个生成点");
        }

        /// <summary>
        /// 创建默认生成点
        /// </summary>
        private void CreateDefaultSpawnPoints()
        {
            // 在玩家周围创建3个生成点
            Vector3[] offsets = new Vector3[]
            {
                new Vector3(10, 5, 0),
                new Vector3(-10, 5, 0),
                new Vector3(0, -8, 0)
            };

            for (int i = 0; i < offsets.Length; i++)
            {
                var go = new GameObject($"SpawnPoint_{i}");
                go.tag = "SpawnPoint";
                
                Vector3 pos = playerSpawnPoint != null ? 
                    playerSpawnPoint.position + offsets[i] : offsets[i];
                go.transform.position = pos;

                spawnPoints.Add(new SpawnPoint
                {
                    id = go.name,
                    transform = go.transform,
                    spawnRadius = 2f
                });
            }
        }

        #endregion

        #region 波次管理

        /// <summary>
        /// 启动波次协程
        /// </summary>
        private IEnumerator StartWavesCoroutine()
        {
            yield return new WaitForSeconds(1f); // 等待场景加载完成

            for (int i = 0; i < waves.Count; i++)
            {
                yield return StartWave(i);

                // 等待波次完成
                if (waves[i].waitForClear)
                {
                    yield return new WaitUntil(() => activeEnemies.Count == 0);
                }
                else
                {
                    yield return new WaitForSeconds(5f); // 固定间隔
                }

                OnWaveCompleted?.Invoke(i);
            }

            // 所有波次完成
            CompleteScene();
        }

        /// <summary>
        /// 开始指定波次
        /// </summary>
        public IEnumerator StartWave(int waveIndex)
        {
            if (waveIndex < 0 || waveIndex >= waves.Count) yield break;

            currentWaveIndex = waveIndex;
            var wave = waves[waveIndex];

            Debug.Log($"[CombatSceneManager] 开始波次 {waveIndex + 1}: {wave.waveName}");

            yield return new WaitForSeconds(wave.delayBeforeSpawn);

            isWaveActive = true;
            OnWaveStarted?.Invoke(waveIndex);

            // 生成敌人
            foreach (var entry in wave.enemies)
            {
                for (int i = 0; i < entry.count; i++)
                {
                    SpawnEnemy(entry);
                    yield return new WaitForSeconds(entry.spawnInterval);
                }
            }
        }

        /// <summary>
        /// 生成单个敌人
        /// </summary>
        private void SpawnEnemy(EnemySpawnEntry entry)
        {
            GameObject prefab = GetEnemyPrefab(entry.enemyType);
            if (prefab == null)
            {
                Debug.LogWarning($"[CombatSceneManager] 未找到 {entry.enemyType} 的预制体");
                return;
            }

            // 确定生成位置
            Vector3 spawnPos = GetSpawnPosition(entry);

            // 创建敌人
            var enemyObj = Instantiate(prefab, spawnPos, Quaternion.identity, enemyContainer);
            var enemy = enemyObj.GetComponent<EnemyBase>();

            if (enemy != null)
            {
                activeEnemies.Add(enemy);
                enemy.OnDeath += () => HandleEnemyDeath(enemy);

                // 注册到集成系统
                CombatIntegrationSystem.Instance?.RegisterEnemy(enemy);

                OnEnemySpawned?.Invoke(enemy);

                Debug.Log($"[CombatSceneManager] 生成敌人: {enemy.Type} at {spawnPos}");
            }
        }

        /// <summary>
        /// 获取敌人生成位置
        /// </summary>
        private Vector3 GetSpawnPosition(EnemySpawnEntry entry)
        {
            if (entry.useRandomSpawnPoint || entry.specificSpawnPointIndex < 0)
            {
                // 随机选择生成点
                if (spawnPoints.Count > 0)
                {
                    var point = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
                    Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * point.spawnRadius;
                    return point.transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
                }
            }
            else if (entry.specificSpawnPointIndex < spawnPoints.Count)
            {
                var point = spawnPoints[entry.specificSpawnPointIndex];
                Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * point.spawnRadius;
                return point.transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
            }

            // 默认在玩家附近生成
            if (MechController.Instance != null)
            {
                Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * 10f;
                return MechController.Instance.transform.position + 
                    new Vector3(randomOffset.x, randomOffset.y, 0);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// 获取敌人预制体
        /// </summary>
        private GameObject GetEnemyPrefab(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.MechFish:
                    return mechFishPrefab;
                case EnemyType.MechCrab:
                    return mechCrabPrefab;
                case EnemyType.MechJellyfish:
                    return mechJellyfishPrefab;
                default:
                    return null;
            }
        }

        /// <summary>
        /// 处理敌人死亡
        /// </summary>
        private void HandleEnemyDeath(EnemyBase enemy)
        {
            if (activeEnemies.Contains(enemy))
            {
                activeEnemies.Remove(enemy);
            }

            OnEnemyKilled?.Invoke(enemy);

            Debug.Log($"[CombatSceneManager] 敌人死亡，剩余 {activeEnemies.Count} 个敌人");
        }

        #endregion

        #region 场景完成

        /// <summary>
        /// 完成场景
        /// </summary>
        private void CompleteScene()
        {
            isSceneComplete = true;
            isWaveActive = false;

            Debug.Log("[CombatSceneManager] 场景完成!");

            OnSceneCompleted?.Invoke();

            // 显示完成UI
            ShowCompletionUI();
        }

        /// <summary>
        /// 显示完成UI
        /// </summary>
        private void ShowCompletionUI()
        {
            Debug.Log("[CombatSceneManager] 显示完成UI");
            
            // 使用通知系统显示胜利
            UINotification.Instance?.ShowNotification("🎉 战斗胜利！所有波次已完成！", NotificationType.Success);
            
            // 显示统计信息
            string stats = $"总击杀: {totalEnemiesKilled} | 最高连击: {maxCombo} | 剩余生命: {playerLives}";
            UINotification.Instance?.ShowNotification(stats, NotificationType.Info);
            
            // 触发游戏事件
            GameEvents.TriggerNotification("战斗场景完成！");
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 手动开始波次
        /// </summary>
        public void StartNextWave()
        {
            if (currentWaveIndex + 1 < waves.Count)
            {
                StartCoroutine(StartWave(currentWaveIndex + 1));
            }
        }

        /// <summary>
        /// 生成指定类型的敌人（测试用）
        /// </summary>
        public void SpawnEnemyForTest(EnemyType type, Vector3? position = null)
        {
            var entry = new EnemySpawnEntry
            {
                enemyType = type,
                count = 1,
                useRandomSpawnPoint = !position.HasValue
            };

            if (position.HasValue)
            {
                // 创建临时生成点
                var tempPoint = new GameObject("TempSpawnPoint");
                tempPoint.transform.position = position.Value;
                spawnPoints.Add(new SpawnPoint
                {
                    id = "temp",
                    transform = tempPoint.transform,
                    spawnRadius = 0.5f
                });
                entry.specificSpawnPointIndex = spawnPoints.Count - 1;
            }

            SpawnEnemy(entry);
        }

        /// <summary>
        /// 清理所有敌人
        /// </summary>
        public void ClearAllEnemies()
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }
            activeEnemies.Clear();
        }

        #endregion
    }
}
