using System.Collections;
using UnityEngine;

namespace SebeJJ.Experience.Pacing
{
    /// <summary>
    /// 战斗节奏控制器
    /// </summary>
    public class CombatPacingController : MonoBehaviour
    {
        public static CombatPacingController Instance { get; private set; }
        
        [Header("战斗节奏")]
        [SerializeField] private float minCombatDuration = 10f;
        [SerializeField] private float maxCombatDuration = 60f;
        [SerializeField] private float combatCooldown = 15f;
        [SerializeField] private float tensionBuildRate = 0.1f;
        [SerializeField] private float tensionDecayRate = 0.05f;
        
        [Header("遭遇生成")]
        [SerializeField] private float baseSpawnChance = 0.1f;
        [SerializeField] private float minSpawnInterval = 5f;
        [SerializeField] private float maxSpawnInterval = 30f;
        
        [Header("紧张度曲线")]
        [SerializeField] private AnimationCurve tensionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private float currentTension = 0f;
        private float lastCombatTime = 0f;
        private bool inCombat = false;
        private float currentEncounterRate = 1f;
        
        public bool IsInCombat => inCombat;
        public float CurrentTension => currentTension;
        public float TensionPercent => currentTension;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Update()
        {
            UpdateTension();
            ManageEnemySpawning();
        }
        
        /// <summary>
        /// 更新紧张度
        /// </summary>
        private void UpdateTension()
        {
            if (inCombat)
            {
                // 战斗中紧张度快速上升
                currentTension = Mathf.MoveTowards(currentTension, 1f, Time.deltaTime * 0.5f);
            }
            else
            {
                // 非战斗时紧张度缓慢下降
                currentTension = Mathf.MoveTowards(currentTension, 0f, Time.deltaTime * tensionDecayRate);
            }
        }
        
        /// <summary>
        /// 管理敌人生成
        /// </summary>
        private void ManageEnemySpawning()
        {
            if (inCombat) return;
            
            float timeSinceLastCombat = Time.time - lastCombatTime;
            
            // 冷却期内不生成
            if (timeSinceLastCombat < combatCooldown) return;
            
            // 计算生成概率
            float spawnChance = CalculateSpawnChance(timeSinceLastCombat);
            
            // 随机生成
            if (Random.value < spawnChance * Time.deltaTime)
            {
                SpawnEnemyEncounter();
            }
        }
        
        /// <summary>
        /// 计算生成概率
        /// </summary>
        private float CalculateSpawnChance(float timeSinceLastCombat)
        {
            // 基础生成率
            float baseRate = baseSpawnChance * currentEncounterRate;
            
            // 时间因子（越久越可能遭遇）
            float timeFactor = Mathf.Clamp01((timeSinceLastCombat - combatCooldown) / maxSpawnInterval);
            
            // 紧张度因子（紧张度高时降低生成，给玩家喘息）
            float tensionFactor = 1f - currentTension * 0.5f;
            
            // 深度因子
            float depthFactor = GetDepthSpawnMultiplier();
            
            return baseRate * timeFactor * tensionFactor * depthFactor;
        }
        
        /// <summary>
        /// 获取深度生成倍率
        /// </summary>
        private float GetDepthSpawnMultiplier()
        {
            // float depth = DiveManager.Instance.CurrentDepth;
            // return 1f + depth / 100f;
            return 1f;
        }
        
        /// <summary>
        /// 生成敌人遭遇
        /// </summary>
        private void SpawnEnemyEncounter()
        {
            // 确定遭遇规模基于紧张度
            EncounterSize size = DetermineEncounterSize();
            
            // 生成敌人
            StartCoroutine(SpawnEncounterCoroutine(size));
            
            // 开始战斗
            OnCombatStart();
        }
        
        /// <summary>
        /// 确定遭遇规模
        /// </summary>
        private EncounterSize DetermineEncounterSize()
        {
            float roll = Random.value;
            
            if (roll < 0.6f)
                return EncounterSize.Small;     // 60% 小规模
            else if (roll < 0.9f)
                return EncounterSize.Medium;    // 30% 中规模
            else
                return EncounterSize.Large;     // 10% 大规模
        }
        
        /// <summary>
        /// 生成遭遇协程
        /// </summary>
        private IEnumerator SpawnEncounterCoroutine(EncounterSize size)
        {
            int enemyCount = size switch
            {
                EncounterSize.Small => Random.Range(1, 3),
                EncounterSize.Medium => Random.Range(3, 6),
                EncounterSize.Large => Random.Range(6, 10),
                _ => 1
            };
            
            // 波浪式生成
            for (int i = 0; i < enemyCount; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(Random.Range(0.5f, 2f));
            }
        }
        
        /// <summary>
        /// 生成单个敌人
        /// </summary>
        private void SpawnEnemy()
        {
            // EnemySpawnManager.Instance.SpawnEnemyNearPlayer();
        }
        
        /// <summary>
        /// 战斗开始
        /// </summary>
        public void OnCombatStart()
        {
            inCombat = true;
            currentTension = 0.7f;
            
            // 记录会话
            PacingManager.Instance.RecordCombatStart();
            
            // 播放战斗音乐
            // AudioManager.Instance.PlayMusic("Combat", fadeTime: 2f);
            
            Debug.Log("[CombatPacing] 战斗开始");
        }
        
        /// <summary>
        /// 战斗结束
        /// </summary>
        public void OnCombatEnd()
        {
            inCombat = false;
            lastCombatTime = Time.time;
            
            // 播放探索音乐
            // AudioManager.Instance.PlayMusic("Exploration", fadeTime: 3f);
            
            Debug.Log("[CombatPacing] 战斗结束");
        }
        
        /// <summary>
        /// 增加遭遇频率
        /// </summary>
        public void IncreaseEncounterRate(float amount)
        {
            currentEncounterRate = Mathf.Min(2f, currentEncounterRate + amount);
        }
        
        /// <summary>
        /// 减少遭遇频率
        /// </summary>
        public void ReduceEncounterRate(float amount)
        {
            currentEncounterRate = Mathf.Max(0.5f, currentEncounterRate - amount);
        }
        
        /// <summary>
        /// 立即触发遭遇（用于测试或特殊事件）
        /// </summary>
        public void ForceEncounter(EncounterSize size)
        {
            StartCoroutine(SpawnEncounterCoroutine(size));
            OnCombatStart();
        }
    }
    
    /// <summary>
    /// 遭遇规模
    /// </summary>
    public enum EncounterSize
    {
        Small,      // 1-2敌人
        Medium,     // 3-5敌人
        Large       // 6+敌人
    }
}
