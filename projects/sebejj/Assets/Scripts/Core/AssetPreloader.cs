using UnityEngine;
using System.Collections.Generic;

namespace SebeJJ.Core
{
    /// <summary>
    /// 资源预加载器 - 优化场景加载性能
    /// 在游戏启动或场景切换前预加载常用资源
    /// </summary>
    public class AssetPreloader : MonoBehaviour
    {
        public static AssetPreloader Instance { get; private set; }
        
        [System.Serializable]
        public class PreloadGroup
        {
            public string groupName;
            public GameObject[] prefabs;
            public int poolSize = 10;
            public bool preloadOnStart = true;
        }
        
        [Header("预加载配置")]
        [SerializeField] private PreloadGroup[] preloadGroups;
        [SerializeField] private bool preloadOnAwake = true;
        [SerializeField] private bool showDebugInfo = true;
        
        [Header("音频预加载")]
        [SerializeField] private AudioClip[] commonAudioClips;
        
        // 预加载状态
        private Dictionary<string, bool> preloadStatus = new Dictionary<string, bool>();
        private Dictionary<string, GameObject> preloadedPrefabs = new Dictionary<string, GameObject>();
        private Dictionary<string, AudioClip> preloadedAudio = new Dictionary<string, AudioClip>();
        
        // 加载进度
        public float TotalProgress { get; private set; }
        public bool IsPreloading { get; private set; }
        public System.Action<float> OnProgressChanged;
        public System.Action OnPreloadComplete;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (preloadOnAwake)
            {
                PreloadAll();
            }
        }
        
        /// <summary>
        /// 预加载所有配置的资源
        /// </summary>
        public async void PreloadAll()
        {
            if (IsPreloading) return;
            
            IsPreloading = true;
            TotalProgress = 0f;
            
            int totalGroups = preloadGroups.Length;
            int completedGroups = 0;
            
            foreach (var group in preloadGroups)
            {
                if (group.preloadOnStart)
                {
                    await PreloadGroupAsync(group);
                }
                
                completedGroups++;
                TotalProgress = (float)completedGroups / totalGroups;
                OnProgressChanged?.Invoke(TotalProgress);
            }
            
            // 预加载音频
            PreloadAudio();
            
            IsPreloading = false;
            TotalProgress = 1f;
            OnProgressChanged?.Invoke(1f);
            OnPreloadComplete?.Invoke();
            
            if (showDebugInfo)
            {
                Debug.Log("[AssetPreloader] All assets preloaded successfully");
            }
        }
        
        /// <summary>
        /// 异步预加载一个资源组
        /// </summary>
        private async System.Threading.Tasks.Task PreloadGroupAsync(PreloadGroup group)
        {
            if (preloadStatus.ContainsKey(group.groupName) && preloadStatus[group.groupName])
            {
                return; // 已预加载
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[AssetPreloader] Preloading group: {group.groupName}");
            }
            
            // 确保对象池管理器已初始化
            if (ObjectPoolManager.Instance == null)
            {
                await System.Threading.Tasks.Task.Delay(100);
            }
            
            // 为每个预制体创建对象池
            foreach (var prefab in group.prefabs)
            {
                if (prefab == null) continue;
                
                preloadedPrefabs[prefab.name] = prefab;
                
                // 通知对象池管理器预加载
                ObjectPoolManager.Instance?.Prewarm(prefab.name, group.poolSize);
                
                // 每处理一个资源让出一帧，避免阻塞
                await System.Threading.Tasks.Task.Yield();
            }
            
            preloadStatus[group.groupName] = true;
        }
        
        /// <summary>
        /// 预加载音频资源
        /// </summary>
        private void PreloadAudio()
        {
            if (commonAudioClips == null) return;
            
            foreach (var clip in commonAudioClips)
            {
                if (clip != null)
                {
                    preloadedAudio[clip.name] = clip;
                    
                    // 预加载到内存
                    clip.LoadAudioData();
                }
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[AssetPreloader] Preloaded {commonAudioClips.Length} audio clips");
            }
        }
        
        /// <summary>
        /// 获取预加载的预制体
        /// </summary>
        public GameObject GetPrefab(string name)
        {
            if (preloadedPrefabs.TryGetValue(name, out var prefab))
            {
                return prefab;
            }
            
            Debug.LogWarning($"[AssetPreloader] Prefab '{name}' not found in preloaded assets");
            return null;
        }
        
        /// <summary>
        /// 获取预加载的音频
        /// </summary>
        public AudioClip GetAudioClip(string name)
        {
            if (preloadedAudio.TryGetValue(name, out var clip))
            {
                return clip;
            }
            return null;
        }
        
        /// <summary>
        /// 检查资源组是否已预加载
        /// </summary>
        public bool IsGroupPreloaded(string groupName)
        {
            return preloadStatus.ContainsKey(groupName) && preloadStatus[groupName];
        }
        
        /// <summary>
        /// 手动预加载指定组
        /// </summary>
        public async void PreloadGroup(string groupName)
        {
            foreach (var group in preloadGroups)
            {
                if (group.groupName == groupName)
                {
                    await PreloadGroupAsync(group);
                    return;
                }
            }
            
            Debug.LogWarning($"[AssetPreloader] Group '{groupName}' not found");
        }
        
        /// <summary>
        /// 释放资源组
        /// </summary>
        public void UnloadGroup(string groupName)
        {
            if (!preloadStatus.ContainsKey(groupName)) return;
            
            // 清空对象池
            ObjectPoolManager.Instance?.ClearPool(groupName);
            
            preloadStatus[groupName] = false;
            
            if (showDebugInfo)
            {
                Debug.Log($"[AssetPreloader] Unloaded group: {groupName}");
            }
        }
        
        /// <summary>
        /// 释放所有预加载资源
        /// </summary>
        public void UnloadAll()
        {
            ObjectPoolManager.Instance?.ClearAllPools();
            
            // 卸载音频数据
            foreach (var clip in preloadedAudio.Values)
            {
                if (clip != null)
                {
                    clip.UnloadAudioData();
                }
            }
            
            preloadedAudio.Clear();
            preloadedPrefabs.Clear();
            preloadStatus.Clear();
            
            // 强制垃圾回收
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            
            if (showDebugInfo)
            {
                Debug.Log("[AssetPreloader] All assets unloaded");
            }
        }
        
        /// <summary>
        /// 获取预加载统计信息
        /// </summary>
        public string GetPreloadStats()
        {
            int totalPrefabs = preloadedPrefabs.Count;
            int totalAudio = preloadedAudio.Count;
            int loadedGroups = 0;
            
            foreach (var status in preloadStatus.Values)
            {
                if (status) loadedGroups++;
            }
            
            return $"Prefabs: {totalPrefabs}, Audio: {totalAudio}, Groups: {loadedGroups}/{preloadGroups.Length}";
        }
    }
}
