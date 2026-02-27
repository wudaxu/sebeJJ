using System.Collections.Generic;
using UnityEngine;
using SebeJJ.Utils;

namespace SebeJJ.Core
{
    /// <summary>
    /// UI面板基类
    /// </summary>
    public abstract class UIPanel : MonoBehaviour
    {
        [Header("Panel Settings")]
        [SerializeField] protected bool startHidden = true;
        [SerializeField] protected float fadeDuration = 0.2f;
        [SerializeField] protected CanvasGroup canvasGroup;

        public bool IsVisible { get; protected set; }

        protected virtual void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (startHidden)
            {
                HideInstant();
            }
            else
            {
                ShowInstant();
            }
        }

        public virtual void Show()
        {
            if (IsVisible) return;
            
            gameObject.SetActive(true);
            StartCoroutine(FadeIn());
            IsVisible = true;
            OnShow();
        }

        public virtual void Hide()
        {
            if (!IsVisible) return;
            
            StartCoroutine(FadeOut(() =>
            {
                gameObject.SetActive(false);
                IsVisible = false;
                OnHide();
            }));
        }

        public virtual void ShowInstant()
        {
            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            IsVisible = true;
            OnShow();
        }

        public virtual void HideInstant()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
            IsVisible = false;
            OnHide();
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }

        private System.Collections.IEnumerator FadeIn()
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        private System.Collections.IEnumerator FadeOut(System.Action onComplete)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// UI管理器 - 管理所有UI面板的显示和隐藏
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        [Header("UI Panels")]
        [SerializeField] private UIPanel hudPanel;
        [SerializeField] private UIPanel pauseMenuPanel;
        [SerializeField] private UIPanel gameOverPanel;
        [SerializeField] private UIPanel victoryPanel;
        [SerializeField] private UIPanel mainMenuPanel;
        [SerializeField] private UIPanel inventoryPanel;
        [SerializeField] private UIPanel shopPanel;
        [SerializeField] private UIPanel missionPanel;

        [Header("Settings")]
        [SerializeField] private bool allowMultiplePanels = false;

        private List<UIPanel> _openPanels = new List<UIPanel>();
        private UIPanel _currentPanel;

        public HUD HUD { get; private set; }
        public bool IsAnyPanelOpen => _openPanels.Count > 0;

        #region Unity Lifecycle

        protected override void OnAwake()
        {
            base.OnAwake();
            Initialize();
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            // 获取HUD组件
            if (hudPanel != null)
            {
                HUD = hudPanel.GetComponent<HUD>();
            }

            // 初始状态：只显示HUD
            CloseAllPanels();
            ShowHUD();
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnGamePause += OnGamePause;
            GameEvents.OnGameResume += OnGameResume;
            GameEvents.OnGameOver += OnGameOver;
            GameEvents.OnGameVictory += OnGameVictory;
            GameEvents.OnShowWarning += ShowWarning;
            GameEvents.OnShowNotification += ShowNotification;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnGamePause -= OnGamePause;
            GameEvents.OnGameResume -= OnGameResume;
            GameEvents.OnGameOver -= OnGameOver;
            GameEvents.OnGameVictory -= OnGameVictory;
            GameEvents.OnShowWarning -= ShowWarning;
            GameEvents.OnShowNotification -= ShowNotification;
        }

        #endregion

        #region Panel Management

        public void OpenPanel(UIPanel panel)
        {
            if (panel == null) return;

            if (!allowMultiplePanels && _currentPanel != null && _currentPanel != panel)
            {
                ClosePanel(_currentPanel);
            }

            if (!_openPanels.Contains(panel))
            {
                panel.Show();
                _openPanels.Add(panel);
                _currentPanel = panel;
            }
        }

        public void ClosePanel(UIPanel panel)
        {
            if (panel == null) return;

            if (_openPanels.Contains(panel))
            {
                panel.Hide();
                _openPanels.Remove(panel);
                
                if (_currentPanel == panel)
                {
                    _currentPanel = _openPanels.Count > 0 ? _openPanels[_openPanels.Count - 1] : null;
                }
            }
        }

        public void CloseAllPanels()
        {
            foreach (var panel in _openPanels)
            {
                if (panel != null)
                {
                    panel.HideInstant();
                }
            }
            _openPanels.Clear();
            _currentPanel = null;
        }

        public void TogglePanel(UIPanel panel)
        {
            if (panel == null) return;

            if (_openPanels.Contains(panel))
            {
                ClosePanel(panel);
            }
            else
            {
                OpenPanel(panel);
            }
        }

        #endregion

        #region Specific Panels

        public void ShowHUD()
        {
            if (hudPanel != null)
            {
                hudPanel.Show();
                if (!_openPanels.Contains(hudPanel))
                {
                    _openPanels.Add(hudPanel);
                }
            }
        }

        public void HideHUD()
        {
            if (hudPanel != null)
            {
                ClosePanel(hudPanel);
            }
        }

        public void ShowPauseMenu()
        {
            OpenPanel(pauseMenuPanel);
        }

        public void HidePauseMenu()
        {
            ClosePanel(pauseMenuPanel);
        }

        public void ShowGameOver()
        {
            CloseAllPanels();
            OpenPanel(gameOverPanel);
        }

        public void ShowVictory()
        {
            CloseAllPanels();
            OpenPanel(victoryPanel);
        }

        public void ShowMainMenu()
        {
            CloseAllPanels();
            OpenPanel(mainMenuPanel);
        }

        public void ShowInventory()
        {
            TogglePanel(inventoryPanel);
        }

        public void ShowShop()
        {
            OpenPanel(shopPanel);
        }

        public void ShowMissionPanel()
        {
            TogglePanel(missionPanel);
        }

        #endregion

        #region Notifications

        public void ShowWarning(string message)
        {
            if (HUD != null)
            {
                HUD.ShowWarning(message);
            }
        }

        public void ShowNotification(string message, float duration = 3f)
        {
            if (HUD != null)
            {
                HUD.ShowNotification(message, duration);
            }
        }

        #endregion

        #region Event Handlers

        private void OnGamePause()
        {
            ShowPauseMenu();
        }

        private void OnGameResume()
        {
            HidePauseMenu();
        }

        private void OnGameOver()
        {
            ShowGameOver();
        }

        private void OnGameVictory()
        {
            ShowVictory();
        }

        #endregion
    }
}
