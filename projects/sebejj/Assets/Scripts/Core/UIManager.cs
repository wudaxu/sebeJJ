using UnityEngine;
using System.Collections.Generic;

namespace SebeJJ.Core
{
    /// <summary>
    /// UI管理器 - 负责所有UI面板的显示/隐藏和管理 (BUG-005 修复: 添加层级管理)
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        
        [Header("UI面板")]
        public GameObject mainMenuPanel;
        public GameObject hudPanel;
        public GameObject pausePanel;
        public GameObject missionPanel;
        public GameObject shopPanel;
        public GameObject gameOverPanel;
        public GameObject inventoryPanel;
        
        [Header("HUD元素")]
        public Transform oxygenBar;
        public Transform energyBar;
        public Transform healthBar;
        public Transform depthText;
        public Transform pressureText;
        public Transform creditsText;
        
        [Header("采集UI")]
        public GameObject collectPanel;
        public UnityEngine.UI.Slider collectProgressBar;
        public TMPro.TextMeshProUGUI collectProgressText;
        
        [Header("UI层级设置")]
        public int baseSortingOrder = 0;
        public int panelSortingOrder = 10;
        public int popupSortingOrder = 20;
        public int notificationSortingOrder = 30;
        
        // BUG-005: UI栈管理
        private Stack<GameObject> uiStack = new Stack<GameObject>();
        private Dictionary<GameState, GameObject> statePanels;
        private GameObject currentOpenPanel;
        private Canvas uiCanvas;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // 获取或创建Canvas
            uiCanvas = GetComponentInParent<Canvas>();
            if (uiCanvas == null)
            {
                uiCanvas = FindObjectOfType<Canvas>();
            }
        }
        
        private void Start()
        {
            InitializePanels();
            
            // 订阅游戏状态变更
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }
        
        private void InitializePanels()
        {
            statePanels = new Dictionary<GameState, GameObject>
            {
                { GameState.MainMenu, mainMenuPanel },
                { GameState.Playing, hudPanel },
                { GameState.Paused, pausePanel },
                { GameState.MissionSelect, missionPanel },
                { GameState.Shop, shopPanel },
                { GameState.GameOver, gameOverPanel }
            };
            
            // 初始状态：显示主菜单
            HideAllPanels();
            ShowPanel(mainMenuPanel);
        }
        
        private void OnGameStateChanged(GameState newState)
        {
            HideAllPanels();
            
            if (statePanels.TryGetValue(newState, out GameObject panel))
            {
                ShowPanel(panel);
            }
        }
        
        #region 面板管理
        
        /// <summary>
        /// 显示指定面板 (BUG-005 修复: 添加层级管理)
        /// </summary>
        public void ShowPanel(GameObject panel)
        {
            if (panel != null)
            {
                panel.SetActive(true);
                currentOpenPanel = panel;
                
                // 设置层级
                SetPanelSortingOrder(panel, panelSortingOrder);
                
                // 添加到UI栈
                if (!uiStack.Contains(panel))
                {
                    uiStack.Push(panel);
                }
            }
        }
        
        /// <summary>
        /// 隐藏指定面板
        /// </summary>
        public void HidePanel(GameObject panel)
        {
            if (panel != null)
            {
                panel.SetActive(false);
                
                // 从UI栈中移除
                if (uiStack.Count > 0 && uiStack.Peek() == panel)
                {
                    uiStack.Pop();
                }
            }
        }
        
        /// <summary>
        /// 隐藏所有面板
        /// </summary>
        public void HideAllPanels()
        {
            foreach (var panel in statePanels.Values)
            {
                if (panel != null)
                    panel.SetActive(false);
            }
            
            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);
            
            if (collectPanel != null)
                collectPanel.SetActive(false);
            
            uiStack.Clear();
        }
        
        /// <summary>
        /// 返回上一个面板
        /// </summary>
        public void GoBack()
        {
            if (uiStack.Count > 1)
            {
                var current = uiStack.Pop();
                current.SetActive(false);
                
                var previous = uiStack.Peek();
                previous.SetActive(true);
            }
        }
        
        /// <summary>
        /// 设置面板层级
        /// </summary>
        private void SetPanelSortingOrder(GameObject panel, int order)
        {
            var canvas = panel.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = panel.AddComponent<Canvas>();
            }
            canvas.overrideSorting = true;
            canvas.sortingOrder = order;
            
            // 确保有GraphicRaycaster
            if (panel.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                panel.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
        }
        
        #endregion
        
        #region UI更新
        
        /// <summary>
        /// 切换背包面板
        /// </summary>
        public void ToggleInventory()
        {
            if (inventoryPanel != null)
            {
                bool willShow = !inventoryPanel.activeSelf;
                inventoryPanel.SetActive(willShow);
                
                if (willShow)
                {
                    SetPanelSortingOrder(inventoryPanel, popupSortingOrder);
                    uiStack.Push(inventoryPanel);
                }
                else
                {
                    // 从栈中移除
                    var tempStack = new Stack<GameObject>();
                    while (uiStack.Count > 0 && uiStack.Peek() != inventoryPanel)
                    {
                        tempStack.Push(uiStack.Pop());
                    }
                    if (uiStack.Count > 0) uiStack.Pop(); // 移除inventoryPanel
                    while (tempStack.Count > 0) uiStack.Push(tempStack.Pop());
                }
            }
        }
        
        /// <summary>
        /// 更新氧气条
        /// </summary>
        public void UpdateOxygenBar(float current, float max)
        {
            if (oxygenBar != null)
            {
                float ratio = Mathf.Clamp01(current / max);
                oxygenBar.localScale = new Vector3(ratio, 1f, 1f);
            }
        }
        
        /// <summary>
        /// 更新能源条
        /// </summary>
        public void UpdateEnergyBar(float current, float max)
        {
            if (energyBar != null)
            {
                float ratio = Mathf.Clamp01(current / max);
                energyBar.localScale = new Vector3(ratio, 1f, 1f);
            }
        }
        
        /// <summary>
        /// 更新深度显示
        /// </summary>
        public void UpdateDepthDisplay(float depth)
        {
            if (depthText != null)
            {
                var text = depthText.GetComponent<TMPro.TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"{depth:F1}m";
                }
            }
        }
        
        /// <summary>
        /// 更新采集进度
        /// </summary>
        public void UpdateCollectProgress(float progress)
        {
            if (collectPanel != null)
                collectPanel.SetActive(progress > 0);
            
            if (collectProgressBar != null)
                collectProgressBar.value = progress;
            
            if (collectProgressText != null)
                collectProgressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
        }
        
        /// <summary>
        /// 显示提示信息
        /// </summary>
        public void ShowNotification(string message, float duration = 2f)
        {
            Debug.Log($"[UI通知] {message}");
            
            // 使用新的通知系统
            UINotification.Instance?.ShowNotification(message, NotificationType.Info);
        }
        
        /// <summary>
        /// 显示任务完成弹窗
        /// </summary>
        public void ShowMissionComplete(string missionName, int reward)
        {
            Debug.Log($"[任务完成] {missionName} - 奖励: {reward}");
            
            // 使用新的通知系统
            UINotification.Instance?.ShowMissionComplete(missionName, $"奖励: {reward} 信用点");
        }
        
        #endregion
    }
}
