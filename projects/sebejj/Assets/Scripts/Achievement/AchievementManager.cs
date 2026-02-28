using System;
using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Achievement
{
    /// <summary>
    /// 成就管理器 - 解决隐藏成就触发条件过于隐蔽的问题
    /// </summary>    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }
        
        [Header("提示配置")]
        [Tooltip("隐藏成就首次接近时显示提示")]
        public bool showProximityHints = true;
        
        [Tooltip("提示显示时长(秒)")]
        public float hintDisplayDuration = 5f;
        
        [Tooltip("提示冷却时间(秒)")]
        public float hintCooldown = 30f;
        
        // 成就数据
        private Dictionary<string, AchievementData> achievements = new Dictionary<string, AchievementData>();
        private HashSet<string> unlockedAchievements = new HashSet<string>();
        
        // 提示状态
        private Dictionary<string, float> lastHintTime = new Dictionary<string, float>();
        
        // 事件
        public event Action<AchievementData> OnAchievementUnlocked;
        public event Action<AchievementData> OnAchievementHintShown;
        
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeAchievements();
        }
        
        /// <summary>
        /// 初始化所有成就
        /// </summary>        private void InitializeAchievements()
        {
            // 隐藏成就 - 添加渐进式提示
            RegisterAchievement(new AchievementData
            {
                id = "hidden_first_blood",
                name = "初尝败绩",
                description = "第一次被敌人击败",
                isHidden = true,
                hintProgression = new List<HintLevel>
                {
                    new HintLevel { progress = 0.0f, hint = "危险潜伏在深海..." },
                    new HintLevel { progress = 0.5f, hint = "你已经接近死亡边缘，小心行事" },
                    new HintLevel { progress = 0.8f, hint = "再受到一次致命伤害将解锁隐藏成就" }
                }
            });
            
            RegisterAchievement(new AchievementData
            {
                id = "hidden_depth_2000",
                name = "深渊探索者",
                description = "到达2000米深度",
                isHidden = true,
                hintProgression = new List<HintLevel>
                {
                    new HintLevel { progress = 0.0f, hint = "深海之下还有更深的秘密..." },
                    new HintLevel { progress = 0.5f, hint = "你已经下潜了1000米，继续向下探索" },
                    new HintLevel { progress = 0.8f, hint = "1800米了，传说中的遗迹就在前方" }
                }
            });
            
            RegisterAchievement(new AchievementData
            {
                id = "hidden_no_damage_boss",
                name = "完美猎手",
                description = "无伤击败Boss",
                isHidden = true,
                hintProgression = new List<HintLevel>
                {
                    new HintLevel { progress = 0.0f, hint = "真正的猎手从不受伤..." },
                    new HintLevel { progress = 0.5f, hint = "Boss战即将开始，保持警惕" },
                    new HintLevel { progress = 0.9f, hint = "你还没有受伤！保持这个状态击败Boss" }
                }
            });
            
            RegisterAchievement(new AchievementData
            {
                id = "hidden_collect_all",
                name = "收藏家",
                description = "收集所有类型的资源",
                isHidden = true,
                hintProgression = new List<HintLevel>
                {
                    new HintLevel { progress = 0.0f, hint = "深海中隐藏着珍稀的资源..." },
                    new HintLevel { progress = 0.5f, hint = "你已经收集了半数资源类型" },
                    new HintLevel { progress = 0.9f, hint = "只差最后几种资源了，仔细搜索每个角落" }
                }
            });
            
            RegisterAchievement(new AchievementData
            {
                id = "hidden_speedrun",
                name = "极速下潜",
                description = "30分钟内到达1500米",
                isHidden = true,
                hintProgression = new List<HintLevel>
                {
                    new HintLevel { progress = 0.0f, hint = "速度也是一种力量..." },
                    new HintLevel { progress = 0.5f, hint = "你下潜得很快，保持节奏" },
                    new HintLevel { progress = 0.8f, hint = "时间紧迫，不要浪费一分一秒" }
                }
            });
            
            // 公开成就...
        }
        
        /// <summary>
        /// 注册成就
        /// </summary>        public void RegisterAchievement(AchievementData achievement)
        {
            achievements[achievement.id] = achievement;
        }
        
        /// <summary>
        /// 解锁成就
        /// </summary>        public bool UnlockAchievement(string id)
        {
            if (unlockedAchievements.Contains(id))
                return false;
            
            if (!achievements.TryGetValue(id, out var achievement))
                return false;
            
            unlockedAchievements.Add(id);
            achievement.isUnlocked = true;
            achievement.unlockTime = DateTime.Now;
            
            OnAchievementUnlocked?.Invoke(achievement);
            
            Debug.Log($"[AchievementManager] 成就解锁: {achievement.name}");
            
            // 保存进度
            SaveAchievementProgress();
            
            return true;
        }
        
        /// <summary>
        /// 检查并显示提示
        /// </summary>        public void CheckAndShowHint(string achievementId, float progress)
        {
            if (!showProximityHints) return;
            
            if (!achievements.TryGetValue(achievementId, out var achievement))
                return;
            
            if (!achievement.isHidden || achievement.isUnlocked)
                return;
            
            // 检查冷却
            if (lastHintTime.TryGetValue(achievementId, out float lastTime))
            {
                if (Time.time - lastTime < hintCooldown)
                    return;
            }
            
            // 找到合适的提示等级
            string hint = GetHintForProgress(achievement, progress);
            if (!string.IsNullOrEmpty(hint))
            {
                ShowHint(achievement, hint);
                lastHintTime[achievementId] = Time.time;
            }
        }
        
        /// <summary>
        /// 获取对应进度的提示
        /// </summary>        private string GetHintForProgress(AchievementData achievement, float progress)
        {
            if (achievement.hintProgression == null || achievement.hintProgression.Count == 0)
                return null;
            
            string bestHint = null;
            float bestProgress = -1;
            
            foreach (var hint in achievement.hintProgression)
            {
                if (progress >= hint.progress && hint.progress > bestProgress)
                {
                    bestHint = hint.hint;
                    bestProgress = hint.progress;
                }
            }
            
            return bestHint;
        }
        
        /// <summary>
        /// 显示提示
        /// </summary>        private void ShowHint(AchievementData achievement, string hint)
        {
            Debug.Log($"[AchievementManager] 提示: {hint}");
            OnAchievementHintShown?.Invoke(achievement);
            
            // TODO: 显示UI提示
        }
        
        /// <summary>
        /// 获取成就列表
        /// </summary>        public List<AchievementData> GetAllAchievements(bool includeHidden = false)
        {
            var list = new List<AchievementData>();
            
            foreach (var kvp in achievements)
            {
                if (!kvp.Value.isHidden || includeHidden)
                {
                    list.Add(kvp.Value);
                }
            }
            
            return list;
        }
        
        /// <summary>
        /// 保存成就进度
        /// </summary>        private void SaveAchievementProgress()
        {
            // TODO: 调用SaveManager保存
        }
        
        /// <summary>
        /// 加载成就进度
        /// </summary>        private void LoadAchievementProgress()
        {
            // TODO: 调用SaveManager加载
        }
    }
    
    /// <summary>
    /// 成就数据
    /// </summary>    [Serializable]
    public class AchievementData
    {
        public string id;
        public string name;
        public string description;
        public bool isHidden;
        public bool isUnlocked;
        public DateTime unlockTime;
        public Sprite icon;
        public int rewardPoints;
        public List<HintLevel> hintProgression;
    }
    
    /// <summary>
    /// 提示等级
    /// </summary>    [Serializable]
    public class HintLevel
    {
        [Range(0f, 1f)]
        public float progress;
        [TextArea(2, 3)]
        public string hint;
    }
}
