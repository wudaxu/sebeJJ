using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SebeJJ.Leaderboard
{
    /// <summary>
    /// 排行榜管理器 - 解决数据同步延迟问题
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
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
        /// </summary>
    private IEnumerator AutoSyncRoutine()
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
        /// </summary>
    private IEnumerator SyncAllLeaderboards()
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
        /// </summary>
    public List<LeaderboardEntry> GetLeaderboard(LeaderboardType type, bool forceRefresh = false)
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
        /// </summary>
    private IEnumerator FetchLeaderboard(LeaderboardType type, int limit)
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
                    // 使用缓存数据或生成模拟数据
                    if (!cache.ContainsKey(type))
                    {
                        cache[type] = new LeaderboardCache
                        {
                            entries = GenerateMockData(type),
                            fetchTime = DateTime.Now.AddSeconds(-cacheValidity) // 标记为过期
                        };
                        OnLeaderboardUpdated?.Invoke(type, cache[type].entries);
                    }
                }
            }
        }
        
        /// <summary>
        /// 提交分数
        /// </summary>
    public void SubmitScore(LeaderboardType type, string playerName, long score, Dictionary<string, object> metadata = null)
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
        /// </summary>
    private IEnumerator UploadPendingScores()
        {
            if (pendingUploads.Count == 0) yield break;
            
            var batch = new List<ScoreEntry>();
            while (pendingUploads.Count > 0 && batch.Count < 10)
            {
                batch.Add(pendingUploads.Dequeue());
            }
            
            // 发送HTTP请求上传分数
            yield return StartCoroutine(UploadScoreBatch(batch));
        }
        
        /// <summary>
        /// 上传分数批次
        /// </summary>
        private IEnumerator UploadScoreBatch(List<ScoreEntry> batch)
        {
            foreach (var entry in batch)
            {
                string url = $"{apiBaseUrl}/submit";
                string jsonData = JsonUtility.ToJson(entry);
                
                using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
                {
                    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");
                    request.timeout = (int)requestTimeout;
                    
                    yield return request.SendWebRequest();
                    
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log($"[LeaderboardManager] 分数上传成功: {entry.playerName} - {entry.score}");
                    }
                    else
                    {
                        Debug.LogWarning($"[LeaderboardManager] 分数上传失败: {request.error}");
                        // 重新加入队列稍后重试
                        pendingUploads.Enqueue(entry);
                    }
                }
                
                yield return new WaitForSeconds(0.1f); // 避免请求过快
            }
        }
        
        /// <summary>
        /// 解析排行榜响应
        /// </summary>
        private List<LeaderboardEntry> ParseLeaderboardResponse(string json)
        {
            try
            {
                // 尝试解析为包装对象
                var wrapper = JsonUtility.FromJson<LeaderboardResponseWrapper>(json);
                if (wrapper != null && wrapper.entries != null)
                {
                    return new List<LeaderboardEntry>(wrapper.entries);
                }
                
                // 尝试直接解析为数组
                var entries = JsonUtility.FromJson<LeaderboardEntryList>("{\"entries\":" + json + "}");
                if (entries?.entries != null)
                {
                    return new List<LeaderboardEntry>(entries.entries);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LeaderboardManager] JSON解析失败: {e.Message}");
            }
            
            return GenerateMockData(LeaderboardType.GlobalScore);
        }
        
        /// <summary>
        /// 生成模拟数据(离线模式)
        /// </summary>
        private List<LeaderboardEntry> GenerateMockData(LeaderboardType type)
        {
            var entries = new List<LeaderboardEntry>();
            string[] mockNames = { "深海探索者", "机甲猎人", "深渊行者", "海洋之心", "赛博渔夫" };
            
            for (int i = 0; i < 10; i++)
            {
                entries.Add(new LeaderboardEntry
                {
                    rank = i + 1,
                    playerId = $"player_{i}",
                    playerName = mockNames[i % mockNames.Length] + $"_{UnityEngine.Random.Range(100, 999)}",
                    score = GetMockScore(type, i),
                    timestamp = DateTime.Now.AddDays(-UnityEngine.Random.Range(1, 30))
                });
            }
            
            return entries;
        }
        
        private long GetMockScore(LeaderboardType type, int rank)
        {
            long baseScore = type switch
            {
                LeaderboardType.GlobalDepth => 2000,
                LeaderboardType.GlobalScore => 50000,
                LeaderboardType.GlobalPlayTime => 3600 * 10,
                _ => 1000
            };
            return baseScore - (rank * (baseScore / 20));
        }
        
        /// <summary>
        /// 手动触发同步
        /// </summary>
    public void ForceSync()
        {
            StartCoroutine(SyncAllLeaderboards());
        }
    }
    
    /// <summary>
    /// 排行榜响应包装器
    /// </summary>
    [Serializable]
    public class LeaderboardResponseWrapper
    {
        public LeaderboardEntry[] entries;
        public int totalCount;
        public string timestamp;
    }
    
    /// <summary>
    /// 排行榜条目列表(用于JSON解析)
    /// </summary>
    [Serializable]
    public class LeaderboardEntryList
    {
        public LeaderboardEntry[] entries;
    }
    
    /// <summary>
    /// 排行榜类型
    /// </summary>
    public enum LeaderboardType
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
    /// </summary>
[Serializable]
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
    /// </summary>
    public class ScoreEntry
    {
        public LeaderboardType type;
        public string playerName;
        public long score;
        public DateTime timestamp;
        public Dictionary<string, object> metadata;
    }
    
    /// <summary>
    /// 排行榜缓存
    /// </summary>
    public class LeaderboardCache
    {
        public List<LeaderboardEntry> entries;
        public DateTime fetchTime;
    }
}
