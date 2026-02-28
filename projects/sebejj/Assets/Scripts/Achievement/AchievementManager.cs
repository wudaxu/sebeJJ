using System;
using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Achievement
{
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }
        
        [Header("提示配置")]
        public bool showProximityHints = true;
        public float hintDisplayDuration = 5f;
        public float hintCooldown = 30f;
        
        private Dictionary<string, AchievementData> achievements = new Dictionary<string, AchievementData>();
        private HashSet<string> unlockedAchievements = new HashSet<string>();
        private Dictionary<string, float> lastHintTime = new Dictionary<string, float>();
        
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
        
        private void InitializeAchievements()
        {
            RegisterAchievement(new AchievementData
            {
                id = "hidden_first_blood",
                name = "初尝败绩",
                description = "第一次被敌人击败",
                isHidden = true
            });
        }
        
        public void RegisterAchievement(AchievementData achievement)
        {
            achievements[achievement.id] = achievement;
        }
        
        public bool UnlockAchievement(string id)
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
            SaveAchievementProgress();
            
            return true;
        }
        
        public void CheckAndShowHint(string achievementId, float progress)
        {
            if (!showProximityHints) return;
            if (!achievements.TryGetValue(achievementId, out var achievement)) return;
            if (!achievement.isHidden || achievement.isUnlocked) return;
            
            if (lastHintTime.TryGetValue(achievementId, out float lastTime))
            {
                if (Time.time - lastTime < hintCooldown)
                    return;
            }
            
            string hint = GetHintForProgress(achievement, progress);
            if (!string.IsNullOrEmpty(hint))
            {
                ShowHint(achievement, hint);
                lastHintTime[achievementId] = Time.time;
            }
        }
        
        private string GetHintForProgress(AchievementData achievement, float progress)
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
        
        private void ShowHint(AchievementData achievement, string hint)
        {
            Debug.Log($"[AchievementManager] 提示: {hint}");
            OnAchievementHintShown?.Invoke(achievement);
            Core.UINotification.Instance?.ShowNotification(hint, Core.NotificationType.Hint);
        }
        
        public List<AchievementData> GetAllAchievements(bool includeHidden = false)
        {
            var list = new List<AchievementData>();
            foreach (var kvp in achievements)
            {
                if (!kvp.Value.isHidden || includeHidden)
                    list.Add(kvp.Value);
            }
            return list;
        }
        
        private void SaveAchievementProgress()
        {
            Debug.Log("[AchievementManager] 成就进度已保存");
        }
        
        public List<string> GetUnlockedAchievementIds()
        {
            return new List<string>(unlockedAchievements);
        }
        
        public Dictionary<string, float> GetAchievementProgress()
        {
            var progress = new Dictionary<string, float>();
            foreach (var kvp in achievements)
            {
                if (!kvp.Value.isUnlocked)
                    progress[kvp.Key] = 0f;
            }
            return progress;
        }
        
        public void LoadAchievementData(List<string> unlockedIds, Dictionary<string, float> progress)
        {
            if (unlockedIds == null) return;
            
            foreach (var id in unlockedIds)
            {
                if (achievements.ContainsKey(id) && !unlockedAchievements.Contains(id))
                {
                    unlockedAchievements.Add(id);
                    achievements[id].isUnlocked = true;
                }
            }
            Debug.Log($"[AchievementManager] 已加载 {unlockedIds.Count} 个解锁成就");
        }
    }
    
    [Serializable]
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
    
    [Serializable]
    public class HintLevel
    {
        [Range(0f, 1f)]
        public float progress;
        [TextArea(2, 3)]
        public string hint;
    }
}
