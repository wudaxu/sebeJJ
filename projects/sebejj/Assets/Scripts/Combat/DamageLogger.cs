using UnityEngine;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 伤害日志系统 - DM-005
    /// 用于调试和统计
    /// </summary>
    public class DamageLogger : MonoBehaviour
    {
        public static DamageLogger Instance { get; private set; }

        [Header("日志设置")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private bool logToConsole = true;
        [SerializeField] private bool logToFile = false;
        [SerializeField] private int maxLogEntries = 1000;

        // 统计
        private float totalDamageDealt = 0;
        private float totalDamageTaken = 0;
        private int totalAttacks = 0;
        private int criticalHits = 0;
        private int kills = 0;

        // 属性
        public float TotalDamageDealt => totalDamageDealt;
        public float TotalDamageTaken => totalDamageTaken;
        public int TotalAttacks => totalAttacks;
        public int CriticalHits => criticalHits;
        public int Kills => kills;
        public float CriticalRate => totalAttacks > 0 ? (float)criticalHits / totalAttacks : 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// 记录伤害事件
        /// </summary>
        public void LogDamage(DamageInfo damageInfo, float finalDamage, bool isPlayerDealt)
        {
            if (!enableLogging) return;

            totalAttacks++;
            if (damageInfo.IsCritical) criticalHits++;

            if (isPlayerDealt)
                totalDamageDealt += finalDamage;
            else
                totalDamageTaken += finalDamage;

            if (logToConsole)
            {
                string attacker = damageInfo.Attacker?.name ?? "Unknown";
                string target = damageInfo.Target?.name ?? "Unknown";
                string critStr = damageInfo.IsCritical ? " [CRIT]" : "";
                
                Debug.Log($"[Damage] {attacker} -> {target}: {finalDamage:F1} ({damageInfo.DamageType}){critStr}");
            }
        }

        /// <summary>
        /// 记录击杀
        /// </summary>
        public void LogKill(GameObject victim, GameObject killer)
        {
            kills++;
            
            if (logToConsole)
            {
                Debug.Log($"[Kill] {killer?.name ?? "Unknown"} killed {victim?.name ?? "Unknown"}");
            }
        }

        /// <summary>
        /// 重置统计
        /// </summary>
        public void ResetStats()
        {
            totalDamageDealt = 0;
            totalDamageTaken = 0;
            totalAttacks = 0;
            criticalHits = 0;
            kills = 0;
        }

        /// <summary>
        /// 打印统计报告
        /// </summary>
        public void PrintReport()
        {
            Debug.Log("=== 战斗统计报告 ===");
            Debug.Log($"总攻击次数: {totalAttacks}");
            Debug.Log($"暴击次数: {criticalHits} ({CriticalRate:P1})");
            Debug.Log($"总伤害输出: {totalDamageDealt:F1}");
            Debug.Log($"总伤害承受: {totalDamageTaken:F1}");
            Debug.Log($"击杀数: {kills}");
            Debug.Log("===================");
        }
    }
}