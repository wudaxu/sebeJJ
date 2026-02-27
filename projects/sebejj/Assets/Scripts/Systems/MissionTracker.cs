using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SebeJJ.Systems
{
    /// <summary>
    /// 委托追踪器UI - 显示当前活跃委托的进度
    /// </summary>
    public class MissionTracker : MonoBehaviour
    {
        [Header("UI组件")]
        public Transform missionListContainer;
        public GameObject missionEntryPrefab;
        public TextMeshProUGUI noMissionText;
        
        [Header("详情面板")]
        public GameObject detailPanel;
        public TextMeshProUGUI detailTitle;
        public TextMeshProUGUI detailDescription;
        public TextMeshProUGUI detailReward;
        public TextMeshProUGUI detailTimeLimit;
        public Transform objectivesContainer;
        public GameObject objectiveEntryPrefab;
        
        [Header("快捷追踪")]
        public GameObject trackedMissionPanel;
        public TextMeshProUGUI trackedTitle;
        public Slider trackedProgress;
        public TextMeshProUGUI trackedProgressText;
        
        private Mission currentTrackedMission;
        private MissionManager missionManager;
        
        private void Start()
        {
            missionManager = MissionManager.Instance;
            if (missionManager != null)
            {
                missionManager.OnMissionAccepted += OnMissionAccepted;
                missionManager.OnMissionUpdated += OnMissionUpdated;
                missionManager.OnMissionCompleted += OnMissionCompleted;
            }
            
            UpdateMissionList();
        }
        
        private void Update()
        {
            UpdateTrackedMission();
        }
        
        private void OnDestroy()
        {
            if (missionManager != null)
            {
                missionManager.OnMissionAccepted -= OnMissionAccepted;
                missionManager.OnMissionUpdated -= OnMissionUpdated;
                missionManager.OnMissionCompleted -= OnMissionCompleted;
            }
        }
        
        /// <summary>
        /// 更新委托列表
        /// </summary>
        private void UpdateMissionList()
        {
            if (missionManager == null) return;
            
            // 清空列表
            foreach (Transform child in missionListContainer)
            {
                Destroy(child.gameObject);
            }
            
            var activeMissions = missionManager.ActiveMissions;
            
            if (activeMissions.Count == 0)
            {
                if (noMissionText != null)
                    noMissionText.gameObject.SetActive(true);
            }
            else
            {
                if (noMissionText != null)
                    noMissionText.gameObject.SetActive(false);
                
                foreach (var mission in activeMissions)
                {
                    CreateMissionEntry(mission);
                }
            }
        }
        
        /// <summary>
        /// 创建委托条目
        /// </summary>
        private void CreateMissionEntry(Mission mission)
        {
            if (missionEntryPrefab == null) return;
            
            var entry = Instantiate(missionEntryPrefab, missionListContainer);
            
            // 设置委托信息
            var titleText = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (titleText != null)
                titleText.text = mission.Title;
            
            // 设置进度条
            var progressSlider = entry.GetComponentInChildren<Slider>();
            if (progressSlider != null)
                progressSlider.value = mission.GetOverallProgress();
            
            // 添加点击事件
            var button = entry.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => ShowMissionDetail(mission));
            }
            
            // 添加快捷追踪按钮
            var trackButton = entry.transform.Find("TrackButton")?.GetComponent<Button>();
            if (trackButton != null)
            {
                trackButton.onClick.AddListener(() => TrackMission(mission));
            }
        }
        
        /// <summary>
        /// 显示委托详情
        /// </summary>
        private void ShowMissionDetail(Mission mission)
        {
            if (detailPanel == null) return;
            
            detailPanel.SetActive(true);
            
            if (detailTitle != null)
                detailTitle.text = mission.Title;
            
            if (detailDescription != null)
                detailDescription.text = mission.Description;
            
            if (detailReward != null)
                detailReward.text = $"奖励: {mission.RewardCredits} 信用点";
            
            if (detailTimeLimit != null)
            {
                if (mission.TimeLimit > 0)
                {
                    float remaining = mission.GetRemainingTime();
                    detailTimeLimit.text = $"剩余时间: {FormatTime(remaining)}";
                    detailTimeLimit.color = remaining < 60f ? Color.red : Color.white;
                }
                else
                {
                    detailTimeLimit.text = "无时间限制";
                    detailTimeLimit.color = Color.white;
                }
            }
            
            // 更新目标列表
            UpdateObjectivesList(mission);
        }
        
        /// <summary>
        /// 更新目标列表
        /// </summary>
        private void UpdateObjectivesList(Mission mission)
        {
            if (objectivesContainer == null) return;
            
            // 清空
            foreach (Transform child in objectivesContainer)
            {
                Destroy(child.gameObject);
            }
            
            // 创建目标条目
            foreach (var objective in mission.Objectives)
            {
                if (objectiveEntryPrefab == null) continue;
                
                var entry = Instantiate(objectiveEntryPrefab, objectivesContainer);
                
                var descText = entry.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
                if (descText != null)
                    descText.text = objective.description;
                
                var progressText = entry.transform.Find("Progress")?.GetComponent<TextMeshProUGUI>();
                if (progressText != null)
                    progressText.text = $"{objective.currentAmount}/{objective.requiredAmount}";
                
                var checkmark = entry.transform.Find("Checkmark")?.gameObject;
                if (checkmark != null)
                    checkmark.SetActive(objective.IsComplete);
                
                // 完成的目标变灰
                if (objective.IsComplete && descText != null)
                {
                    descText.color = Color.gray;
                    descText.fontStyle = FontStyles.Strikethrough;
                }
            }
        }
        
        /// <summary>
        /// 追踪指定委托
        /// </summary>
        public void TrackMission(Mission mission)
        {
            currentTrackedMission = mission;
            
            if (trackedMissionPanel != null)
                trackedMissionPanel.SetActive(true);
            
            if (trackedTitle != null)
                trackedTitle.text = mission.Title;
            
            UpdateTrackedMission();
            
            Utils.GameEvents.TriggerNotification($"开始追踪: {mission.Title}");
        }
        
        /// <summary>
        /// 更新追踪面板
        /// </summary>
        private void UpdateTrackedMission()
        {
            if (currentTrackedMission == null) return;
            if (trackedMissionPanel == null) return;
            
            // 检查委托是否还存在
            if (!MissionManager.Instance.ActiveMissions.Contains(currentTrackedMission))
            {
                ClearTrackedMission();
                return;
            }
            
            float progress = currentTrackedMission.GetOverallProgress();
            
            if (trackedProgress != null)
                trackedProgress.value = progress;
            
            if (trackedProgressText != null)
                trackedProgressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
        }
        
        /// <summary>
        /// 清除追踪
        /// </summary>
        private void ClearTrackedMission()
        {
            currentTrackedMission = null;
            
            if (trackedMissionPanel != null)
                trackedMissionPanel.SetActive(false);
        }
        
        /// <summary>
        /// 格式化时间
        /// </summary>
        private string FormatTime(float seconds)
        {
            int mins = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            return $"{mins:D2}:{secs:D2}";
        }
        
        // 事件处理
        private void OnMissionAccepted(Mission mission)
        {
            UpdateMissionList();
            
            // 自动追踪第一个委托
            if (MissionManager.Instance.ActiveMissions.Count == 1)
            {
                TrackMission(mission);
            }
        }
        
        private void OnMissionUpdated(Mission mission)
        {
            UpdateMissionList();
            
            // 如果正在显示详情，更新详情
            if (detailPanel != null && detailPanel.activeSelf)
            {
                ShowMissionDetail(mission);
            }
        }
        
        private void OnMissionCompleted(Mission mission)
        {
            UpdateMissionList();
            
            if (currentTrackedMission == mission)
            {
                ClearTrackedMission();
            }
        }
        
        /// <summary>
        /// 打开/关闭追踪面板
        /// </summary>
        public void ToggleTracker()
        {
            if (missionListContainer != null)
            {
                bool isActive = missionListContainer.gameObject.activeSelf;
                missionListContainer.gameObject.SetActive(!isActive);
                
                if (!isActive)
                {
                    UpdateMissionList();
                }
            }
        }
    }
}
