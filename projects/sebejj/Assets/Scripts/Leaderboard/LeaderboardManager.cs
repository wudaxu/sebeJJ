using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SebeJJ.Leaderboard
{
    /// <summary>
    /// 排行榜管理器 - 解决数据同步延迟问题
    /// </summary>    public class LeaderboardManager : MonoBehaviour
    {
        public static LeaderboardManager Instance { get; private set; }
        
        [Header("同步配置")]
        [Tooltip("自动同步间隔(秒)")]
        public float autoSyncInterval = 60f; // 1分钟
        
        [Tooltip("本地缓存有效期(秒)")]
        public float cacheValidity = 300f; // 5分钟
        
        [Tooltip("批量上传阈值")]
        public int batchUploadThreshold = 5;
        
        [Header("网络配置")]
        [Tooltip("API基础URL")]
        public string apiBaseUrl = "https://api.sebejj.game/leaderboard";
        
        [Tooltip("请求超时(秒)")]
        public float requestTimeout = 10f;
        
        // 排行榜数据缓存
        private Dictionary<LeaderboardType, LeaderboardCache> cache = new Dictionary<LeaderboardType, LeaderboardCache>();
        
        // 待上传分数队列
        private Queue<ScoreEntry> pendingUploads = new Queue<ScoreEntry>();
        
        // 事件
        public event Action<LeaderboardType, List<LeaderboardEntry>> OnLeaderboardUpdated;
        public event Action<bool, string> OnSyncComplete;
        
        // 同步状态
        public bool IsSyncing { get; private set; } = false;
        public DateTime LastSyncTime { get; private set; } = DateTime.MinValue;
        
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        void Start()
        {
            // 启动自动同步
            StartCoroutine(AutoSyncRoutine());
        }
        
        /// <summary>
        /// 自动同步协程
        /// </summary>        private IEnumerator AutoSyncRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(autoSyncInterval);
                
                // 检查网络状态
                if (NetworkManager.Instance?.IsNetworkStable() ?? false)
                {
                    yield return StartCoroutine(SyncAllLeaderboards());
                }
            }
        }
        
        /// <summary>
        /// 同步所有排行榜
        /// </summary>        private IEnumerator SyncAllLeaderboards()
        {
            if (IsSyncing) yield break;
            
            IsSyncing = true;
            bool success = true;
            string message = "同步完成";
            
            // 先上传待处理的分数
            if (pendingUploads.Count > 0)
            {
                yield return StartCoroutine(UploadPendingScores());
            }
            
            // 刷新各排行榜
            foreach (LeaderboardType type in Enum.GetValues(typeof(LeaderboardType)))
            {
                yield return StartCoroutine(FetchLeaderboard(type, 100));
            }
            
            LastSyncTime = DateTime.Now;
            IsSyncing = false;
            
            OnSyncComplete?.Invoke(success, message);
        }
        
        /// <summary>
        /// 获取排行榜
        /// </summary>        public List<LeaderboardEntry> GetLeaderboard(LeaderboardType type, bool forceRefresh = false)
        {
            // 检查缓存
            if (!forceRefresh && cache.ContainsKey(type))
            {
                var cached = cache[type];
                if (DateTime.Now - cached.fetchTime < TimeSpan.FromSeconds(cacheValidity))
                {
                    return cached.entries;
                }
            }
            
            // 异步刷新
            StartCoroutine(FetchLeaderboard(type, 100));
            
            // 返回缓存或空列表
            return cache.ContainsKey(type) ? cache[type].entries : new List<LeaderboardEntry>();
        }
        
        /// <summary>
        /// 从服务器获取排行榜
        /// </summary>        private IEnumerator FetchLeaderboard(LeaderboardType type, int limit)
        {
            string url = $"{apiBaseUrl}/{type.ToString().ToLower()}?limit={limit}";
            
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)requestTimeout;
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    // 解析响应
                    var entries = ParseLeaderboardResponse(request.downloadHandler.text);
                    
                    // 更新缓存
                    cache[type] = new LeaderboardCache
                    {
                        entries = entries,
                        fetchTime = DateTime.Now
                    };
                    
                    OnLeaderboardUpdated?.Invoke(type, entries);
                }
                else
                {
                    Debug.LogWarning($"[LeaderboardManager] 获取排行榜失败: {request.error}");
                }
            }
        }
        
        /// <summary>
        /// 提交分数
        /// </summary>        public void SubmitScore(LeaderboardType type, string playerName, long score, Dictionary<string, object> metadata = null)
        {
            var entry = new ScoreEntry
            {
                type = type,
                playerName = playerName,
                score = score,
                timestamp = DateTime.Now,
                metadata = metadata
            };
            
            pendingUploads.Enqueue(entry);
            
            // 达到阈值立即上传
            if (pendingUploads.Count >= batchUploadThreshold)
            {
                StartCoroutine(UploadPendingScores());
            }
        }
        
        /// <summary>
        /// 上传待处理分数
        /// </summary>        private IEnumerator UploadPendingScores()
        {
            if (pendingUploads.Count == 0) yield break;
            
            var batch = new List<ScoreEntry>();
            while (pendingUploads.Count > 0 && batch.Count < 10)
            {
                batch.Add(pendingUploads.Dequeue());
            }
            
            // TODO: 实际项目中发送HTTP请求
            // 这里模拟上传成功
            Debug.Log($"[LeaderboardManager] 上传 {batch.Count} 条分数记录");
            
            yield return new WaitForSeconds(0.5f);
        }
        
        /// <summary>
        /// 解析排行榜响应
        /// </summary>        private List<LeaderboardEntry> ParseLeaderboardResponse(string json)
        {
            // TODO: 使用JsonUtility或Newtonsoft.Json解析
            // 这里返回模拟数据
            return new List<LeaderboardEntry>();
        }
        
        /// <summary>
        /// 手动触发同步
        /// </summary>        public void ForceSync()
        {
            StartCoroutine(SyncAllLeaderboards());
        }
    }
    
    /// <summary>
    /// 排行榜类型
    /// </summary>    public enum LeaderboardType
    {
        GlobalDepth,        // 全球最深下潜
        GlobalScore,        // 全球最高分数
        GlobalPlayTime,     // 全球游戏时长
        WeeklyDepth,        // 本周最深下潜
        WeeklyScore,        // 本周最高分数
        FriendsDepth,       // 好友最深下潜
        FriendsScore,       // 好友最高分数
        Speedrun            // 速通记录
    }
    
    /// <summary>
    /// 排行榜条目
    /// </summary>    [Serializable]
    public class LeaderboardEntry
    {
        public int rank;
        public string playerId;
        public string playerName;
        public long score;
        public DateTime timestamp;
        public Dictionary<string, object> metadata;
    }
    
    /// <summary>
    /// 分数条目
    /// </summary>    public class ScoreEntry
    {
        public LeaderboardType type;
        public string playerName;
        public long score;
        public DateTime timestamp;
        public Dictionary<string, object> metadata;
    }
    
    /// <summary>
    /// 排行榜缓存
    /// </summary>    public class LeaderboardCache
    {
        public List<LeaderboardEntry> entries;
        public DateTime fetchTime;
    }
}
