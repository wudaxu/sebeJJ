using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 敌人生成控制器 - CB-002
    /// 控制敌人生成节奏和波次
    /// </summary>
    public class EnemySpawnController : MonoBehaviour
    {
        [Header("生成配置")]
        [SerializeField] private float initialSpawnDelay = 0f;       // 开场延迟
        [SerializeField] private float baseSpawnInterval = 15f;      // 基础生成间隔
        [SerializeField] private float spawnIntervalIncrement = 5f;  // 间隔增量
        [SerializeField] private int maxEnemies = 10;                // 最大敌人数量
        
        [Header("波次配置")]
        [SerializeField] private List<SpawnWave> spawnWaves = new List<SpawnWave>();
        
        [Header("强度控制")]
        [SerializeField] private float intensityRampUp = 0.1f;       // 强度递增
        [SerializeField] private float maxIntensity = 3f;            // 最大强度
        
        [Header("生成区域")]
        [SerializeField] private Vector2 spawnAreaSize = new Vector2(20f, 15f);
        [SerializeField] private float minSpawnDistance = 8f;        // 最小生成距离
        [SerializeField] private float maxSpawnDistance = 15f;       // 最大生成距离
        
        // 运行时状态
        private int currentWave = 0;
        private float currentIntensity = 1f;
        private float spawnTimer = 0f;
        private float currentSpawnInterval;
        private int totalEnemiesSpawned = 0;
        private List<GameObject> activeEnemies = new List<GameObject>();
        private Transform playerTransform;
        private bool isSpawning = false;
        
        // 事件
        public event Action<int> OnWaveStarted;
        public event Action<GameObject> OnEnemySpawned;
        public event Action OnAllWavesComplete;
        
        [System.Serializable]
        public class SpawnWave
        {
            public string waveName = "Wave";
            public float delayBeforeWave = 0f;
            public int enemyCount = 3;
            public List<EnemySpawnData> enemies = new List<EnemySpawnData>();
            public float spawnInterval = 2f;
        }
        
        [System.Serializable]
        public class EnemySpawnData
        {
            public GameObject enemyPrefab;
            public int count = 1;
            public float spawnWeight = 1f;
        }
        
        private void Start()
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            currentSpawnInterval = baseSpawnInterval;
            
            if (initialSpawnDelay <= 0)
            {
                StartSpawning();
            }
            else
            {
                Invoke(nameof(StartSpawning), initialSpawnDelay);
            }
        }
        
        /// <summary>
        /// 开始生成
        /// </summary>
        public void StartSpawning()
        {
            isSpawning = true;
            StartCoroutine(SpawnWavesCoroutine());
        }
        
        /// <summary>
        /// 停止生成
        /// </summary>
        public void StopSpawning()
        {
            isSpawning = false;
            StopAllCoroutines();
        }
        
        /// <summary>
        /// 波次生成协程
        /// </summary>
        private IEnumerator SpawnWavesCoroutine()
        {
            foreach (var wave in spawnWaves)
            {
                if (!isSpawning) yield break;
                
                currentWave++;
                OnWaveStarted?.Invoke(currentWave);
                
                // 波次前延迟
                if (wave.delayBeforeWave > 0)
                {
                    yield return new WaitForSeconds(wave.delayBeforeWave);
                }
                
                // 生成敌人
                int spawned = 0;
                while (spawned < wave.enemyCount && isSpawning)
                {
                    // 等待生成间隔
                    yield return new WaitForSeconds(wave.spawnInterval);
                    
                    // 检查敌人数量上限
                    CleanupActiveEnemies();
                    if (activeEnemies.Count >= maxEnemies)
                    {
                        yield return new WaitForSeconds(1f);
                        continue;
                    }
                    
                    // 选择并生成敌人
                    GameObject enemy = SpawnEnemyFromWave(wave);
                    if (enemy != null)
                    {
                        spawned++;
                        totalEnemiesSpawned++;
                        activeEnemies.Add(enemy);
                        OnEnemySpawned?.Invoke(enemy);
                    }
                }
                
                // 增加强度
                currentIntensity = Mathf.Min(currentIntensity + intensityRampUp, maxIntensity);
                
                // 等待当前波次敌人被消灭
                yield return new WaitUntil(() => {
                    CleanupActiveEnemies();
                    return activeEnemies.Count == 0 || !isSpawning;
                });
                
                // 波次间延迟
                yield return new WaitForSeconds(currentSpawnInterval);
                currentSpawnInterval += spawnIntervalIncrement;
            }
            
            OnAllWavesComplete?.Invoke();
        }
        
        /// <summary>
        /// 从波次中生成敌人
        /// </summary>
        private GameObject SpawnEnemyFromWave(SpawnWave wave)
        {
            if (wave.enemies.Count == 0) return null;
            
            // 根据权重选择敌人类型
            float totalWeight = 0f;
            foreach (var enemy in wave.enemies)
            {
                totalWeight += enemy.spawnWeight;
            }
            
            float random = UnityEngine.Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            EnemySpawnData selectedEnemy = wave.enemies[0];
            foreach (var enemy in wave.enemies)
            {
                currentWeight += enemy.spawnWeight;
                if (random <= currentWeight)
                {
                    selectedEnemy = enemy;
                    break;
                }
            }
            
            if (selectedEnemy.enemyPrefab == null) return null;
            
            // 计算生成位置
            Vector3 spawnPosition = CalculateSpawnPosition();
            
            // 生成敌人
            GameObject enemy = Instantiate(selectedEnemy.enemyPrefab, spawnPosition, Quaternion.identity);
            
            // 应用强度倍率
            ApplyIntensityScaling(enemy);
            
            return enemy;
        }
        
        /// <summary>
        /// 计算生成位置
        /// </summary>
        private Vector3 CalculateSpawnPosition()
        {
            if (playerTransform == null)
            {
                return new Vector3(
                    UnityEngine.Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                    UnityEngine.Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
                    0f
                );
            }
            
            // 在玩家周围生成
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = UnityEngine.Random.Range(minSpawnDistance, maxSpawnDistance);
            
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance,
                0f
            );
            
            return playerTransform.position + offset;
        }
        
        /// <summary>
        /// 应用强度倍率
        /// </summary>
        private void ApplyIntensityScaling(GameObject enemy)
        {
            var enemyBase = enemy.GetComponent<SebeJJ.Enemies.EnemyBase>();
            if (enemyBase != null)
            {
                // 根据强度增加生命值和伤害
                // 实际实现应在EnemyBase中添加相关方法
            }
        }
        
        /// <summary>
        /// 清理已销毁的敌人
        /// </summary>
        private void CleanupActiveEnemies()
        {
            activeEnemies.RemoveAll(e => e == null);
        }
        
        /// <summary>
        /// 强制生成一波敌人
        /// </summary>
        public void ForceSpawnWave(int waveIndex)
        {
            if (waveIndex >= 0 && waveIndex < spawnWaves.Count)
            {
                StartCoroutine(SpawnSingleWave(spawnWaves[waveIndex]));
            }
        }
        
        /// <summary>
        /// 生成单波
        /// </summary>
        private IEnumerator SpawnSingleWave(SpawnWave wave)
        {
            for (int i = 0; i < wave.enemyCount; i++)
            {
                GameObject enemy = SpawnEnemyFromWave(wave);
                if (enemy != null)
                {
                    activeEnemies.Add(enemy);
                    OnEnemySpawned?.Invoke(enemy);
                }
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }
        
        /// <summary>
        /// 获取当前波次
        /// </summary>
        public int GetCurrentWave() => currentWave;
        
        /// <summary>
        /// 获取活跃敌人数量
        /// </summary>
        public int GetActiveEnemyCount()
        {
            CleanupActiveEnemies();
            return activeEnemies.Count;
        }
    }
}
