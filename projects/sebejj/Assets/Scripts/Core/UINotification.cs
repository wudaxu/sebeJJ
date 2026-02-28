using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace SebeJJ.Core
{
    /// <summary>
    /// UI通知系统 - 显示游戏内通知、成就提示、任务完成等
    /// </summary>    public class UINotification : MonoBehaviour
    {
        public static UINotification Instance { get; private set; }
        
        [Header("通知预制体")]
        public GameObject notificationPrefab;
        public GameObject achievementNotificationPrefab;
        public GameObject missionCompletePrefab;
        
        [Header("通知容器")]
        public Transform notificationContainer;
        public Transform achievementContainer;
        
        [Header("通知设置")]
        public float notificationDuration = 3f;
        public float fadeInDuration = 0.3f;
        public float fadeOutDuration = 0.5f;
        public int maxNotifications = 5;
        
        // 通知队列
        private Queue<NotificationData> notificationQueue = new Queue<NotificationData>();
        private List<GameObject> activeNotifications = new List<GameObject>();
        private bool isProcessingQueue = false;
        
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        void Start()
        {
            // 如果没有指定容器，使用当前Transform
            if (notificationContainer == null)
                notificationContainer = transform;
            if (achievementContainer == null)
                achievementContainer = transform;
                
            // 订阅成就事件
            if (Achievement.AchievementManager.Instance != null)
            {
                Achievement.AchievementManager.Instance.OnAchievementUnlocked += ShowAchievementNotification;
                Achievement.AchievementManager.Instance.OnAchievementHintShown += ShowAchievementHint;
            }
        }
        
        #region 普通通知
        
        /// <summary>
        /// 显示普通通知
        /// </summary>        public void ShowNotification(string message, NotificationType type = NotificationType.Info)
        {
            ShowNotification(message, type, null);
        }
        
        /// <summary>
        /// 显示带图标的通知
        /// </summary>        public void ShowNotification(string message, NotificationType type, Sprite icon)
        {
            var data = new NotificationData
            {
                message = message,
                type = type,
                icon = icon,
                duration = notificationDuration
            };
            
            notificationQueue.Enqueue(data);
            
            if (!isProcessingQueue)
            {
                StartCoroutine(ProcessNotificationQueue());
            }
        }
        
        /// <summary>
        /// 处理通知队列
        /// </summary>        private IEnumerator ProcessNotificationQueue()
        {
            isProcessingQueue = true;
            
            while (notificationQueue.Count > 0)
            {
                // 限制同时显示的通知数量
                while (activeNotifications.Count >= maxNotifications)
                {
                    yield return new WaitForSeconds(0.5f);
                }
                
                var data = notificationQueue.Dequeue();
                yield return StartCoroutine(DisplayNotification(data));
            }
            
            isProcessingQueue = false;
        }
        
        /// <summary>
        /// 显示单个通知
        /// </summary>        private IEnumerator DisplayNotification(NotificationData data)
        {
            if (notificationPrefab == null) yield break;
            
            // 创建通知对象
            GameObject notification = Instantiate(notificationPrefab, notificationContainer);
            activeNotifications.Add(notification);
            
            // 设置内容
            SetupNotificationContent(notification, data);
            
            // 淡入
            CanvasGroup canvasGroup = notification.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = notification.AddComponent<CanvasGroup>();
            
            canvasGroup.alpha = 0f;
            float timer = 0f;
            while (timer < fadeInDuration)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = timer / fadeInDuration;
                yield return null;
            }
            canvasGroup.alpha = 1f;
            
            // 等待显示时间
            yield return new WaitForSeconds(data.duration);
            
            // 淡出
            timer = 0f;
            while (timer < fadeOutDuration)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = 1f - (timer / fadeOutDuration);
                yield return null;
            }
            
            // 清理
            activeNotifications.Remove(notification);
            Destroy(notification);
        }
        
        /// <summary>
        /// 设置通知内容
        /// </summary>        private void SetupNotificationContent(GameObject notification, NotificationData data)
        {
            // 设置文本
            TextMeshProUGUI text = notification.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = data.message;
                text.color = GetNotificationColor(data.type);
            }
            
            // 设置图标
            Image iconImage = notification.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage != null)
            {
                if (data.icon != null)
                {
                    iconImage.sprite = data.icon;
                    iconImage.gameObject.SetActive(true);
                }
                else
                {
                    iconImage.sprite = GetDefaultIcon(data.type);
                }
            }
            
            // 设置背景颜色
            Image background = notification.GetComponent<Image>();
            if (background != null)
            {
                background.color = GetBackgroundColor(data.type);
            }
        }
        
        #endregion
        
        #region 成就通知
        
        /// <summary>
        /// 显示成就解锁通知
        /// </summary>        private void ShowAchievementNotification(Achievement.AchievementData achievement)
        {
            if (achievementNotificationPrefab == null) return;
            
            StartCoroutine(DisplayAchievementNotification(achievement));
        }
        
        /// <summary>
        /// 显示成就提示
        /// </summary>        private void ShowAchievementHint(Achievement.AchievementData achievement)
        {
            // 显示一个更 subtle 的提示
            string hintMessage = $"💡 隐藏成就线索: {achievement.name}";
            ShowNotification(hintMessage, NotificationType.Hint);
        }
        
        /// <summary>
        /// 显示成就解锁通知
        /// </summary>        private IEnumerator DisplayAchievementNotification(Achievement.AchievementData achievement)
        {
            GameObject notification = Instantiate(achievementNotificationPrefab, achievementContainer);
            
            // 设置成就信息
            Transform nameText = notification.transform.Find("Name");
            Transform descText = notification.transform.Find("Description");
            Transform iconImage = notification.transform.Find("Icon");
            
            if (nameText != null)
                nameText.GetComponent<TextMeshProUGUI>().text = $"🏆 成就解锁: {achievement.name}";
            
            if (descText != null)
                descText.GetComponent<TextMeshProUGUI>().text = achievement.description;
            
            if (iconImage != null && achievement.icon != null)
                iconImage.GetComponent<Image>().sprite = achievement.icon;
            
            // 动画效果
            Animator animator = notification.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Show");
            }
            
            // 播放音效
            AudioManager.Instance?.PlaySFX("achievement_unlock");
            
            // 显示时间更长
            yield return new WaitForSeconds(5f);
            
            // 淡出
            CanvasGroup canvasGroup = notification.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                float timer = 0f;
                while (timer < fadeOutDuration)
                {
                    timer += Time.deltaTime;
                    canvasGroup.alpha = 1f - (timer / fadeOutDuration);
                    yield return null;
                }
            }
            
            Destroy(notification);
        }
        
        #endregion
        
        #region 任务完成
        
        /// <summary>
        /// 显示任务完成
        /// </summary>        public void ShowMissionComplete(string missionName, string rewardText)
        {
            if (missionCompletePrefab == null)
            {
                ShowNotification($"✅ 任务完成: {missionName}", NotificationType.Success);
                return;
            }
            
            StartCoroutine(DisplayMissionComplete(missionName, rewardText));
        }
        
        /// <summary>
        /// 显示任务完成
        /// </summary>        private IEnumerator DisplayMissionComplete(string missionName, string rewardText)
        {
            GameObject notification = Instantiate(missionCompletePrefab, notificationContainer);
            
            // 设置任务信息
            Transform nameText = notification.transform.Find("MissionName");
            Transform rewardTextObj = notification.transform.Find("Reward");
            
            if (nameText != null)
                nameText.GetComponent<TextMeshProUGUI>().text = missionName;
            
            if (rewardTextObj != null)
                rewardTextObj.GetComponent<TextMeshProUGUI>().text = rewardText;
            
            // 播放音效
            AudioManager.Instance?.PlaySFX("mission_complete");
            
            yield return new WaitForSeconds(4f);
            
            Destroy(notification);
        }
        
        #endregion
        
        #region 辅助方法
        
        private Color GetNotificationColor(NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Success: return Color.green;
                case NotificationType.Warning: return Color.yellow;
                case NotificationType.Error: return Color.red;
                case NotificationType.Hint: return Color.cyan;
                default: return Color.white;
            }
        }
        
        private Color GetBackgroundColor(NotificationType type)
        {
            Color baseColor = GetNotificationColor(type);
            baseColor.a = 0.2f;
            return baseColor;
        }
        
        private Sprite GetDefaultIcon(NotificationType type)
        {
            // 返回默认图标，实际项目中从资源加载
            return null;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 通知数据
    /// </summary>    public class NotificationData
    {
        public string message;
        public NotificationType type;
        public Sprite icon;
        public float duration;
    }
    
    /// <summary>
    /// 通知类型
    /// </summary>    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error,
        Hint
    }
}
