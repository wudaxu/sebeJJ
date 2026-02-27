using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SebeJJ.Experience.Tutorial
{
    /// <summary>
    /// 引导UI控制器
    /// </summary>
    public class TutorialUI : MonoBehaviour
    {
        [Header("主引导面板")]
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private Image illustrationImage;
        [SerializeField] private Button skipButton;
        [SerializeField] private Button continueButton;
        
        [Header("高亮系统")]
        [SerializeField] private GameObject highlightPrefab;
        [SerializeField] private Canvas overlayCanvas;
        
        [Header("系统解锁提示")]
        [SerializeField] private GameObject unlockPanel;
        [SerializeField] private TextMeshProUGUI unlockTitleText;
        [SerializeField] private TextMeshProUGUI unlockDescriptionText;
        [SerializeField] private Image unlockIconImage;
        [SerializeField] private float unlockDisplayDuration = 3f;
        
        [Header("完成庆祝")]
        [SerializeField] private GameObject completionPanel;
        [SerializeField] private ParticleSystem completionParticles;
        
        [Header("帮助面板")]
        [SerializeField] private GameObject helpPanel;
        [SerializeField] private TabGroup helpTabGroup;
        
        private TutorialStep currentStep;
        private List<GameObject> activeHighlights = new List<GameObject>();
        private Queue<SystemUnlockData> unlockQueue = new Queue<SystemUnlockData>();
        private bool isShowingUnlock = false;
        
        private void Awake()
        {
            if (skipButton != null)
                skipButton.onClick.AddListener(OnSkipClicked);
            
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);
        }
        
        private void Update()
        {
            // 更新当前步骤
            currentStep?.Update();
            
            // 更新提示文本
            if (currentStep != null && hintText != null)
            {
                hintText.text = currentStep.GetHintText();
            }
            
            // 检测帮助面板快捷键
            if (Input.GetKeyDown(KeyCode.H) && !helpPanel.activeSelf)
            {
                ShowHelpPanel();
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && helpPanel.activeSelf)
            {
                HideHelpPanel();
            }
        }
        
        /// <summary>
        /// 显示引导步骤
        /// </summary>
        public void ShowStep(TutorialStep step)
        {
            currentStep = step;
            
            if (tutorialPanel != null)
                tutorialPanel.SetActive(true);
            
            if (titleText != null)
                titleText.text = step.DisplayName;
            
            if (descriptionText != null)
                descriptionText.text = step.GetDescription();
            
            if (hintText != null)
                hintText.text = step.GetHintText();
            
            if (illustrationImage != null && step.Data != null)
                illustrationImage.sprite = step.Data.illustration;
            
            // 更新跳过按钮可见性
            if (skipButton != null)
                skipButton.gameObject.SetActive(step.CanSkip);
        }
        
        /// <summary>
        /// 隐藏引导面板
        /// </summary>
        public void HideTutorialPanel()
        {
            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);
            
            currentStep = null;
        }
        
        /// <summary>
        /// 隐藏所有UI
        /// </summary>
        public void HideAll()
        {
            HideTutorialPanel();
            HideHelpPanel();
            ClearHighlights();
        }
        
        /// <summary>
        /// 高亮游戏对象
        /// </summary>
        public void HighlightObject(GameObject target, string hint = "")
        {
            if (target == null || highlightPrefab == null) return;
            
            ClearHighlights();
            
            var highlight = Instantiate(highlightPrefab, overlayCanvas.transform);
            
            // 设置高亮位置
            var highlightComponent = highlight.GetComponent<TutorialHighlight>();
            if (highlightComponent != null)
            {
                highlightComponent.SetTarget(target);
                highlightComponent.SetHint(hint);
            }
            
            activeHighlights.Add(highlight);
        }
        
        /// <summary>
        /// 高亮UI元素
        /// </summary>
        public void HighlightUIElement(string elementName)
        {
            var element = GameObject.Find(elementName);
            if (element != null)
            {
                HighlightObject(element);
            }
        }
        
        /// <summary>
        /// 清除所有高亮
        /// </summary>
        public void ClearHighlights()
        {
            foreach (var highlight in activeHighlights)
            {
                if (highlight != null)
                    Destroy(highlight);
            }
            activeHighlights.Clear();
        }
        
        /// <summary>
        /// 显示系统解锁提示
        /// </summary>
        public void ShowSystemUnlock(SystemUnlockData data)
        {
            unlockQueue.Enqueue(data);
            
            if (!isShowingUnlock)
            {
                ProcessUnlockQueue();
            }
        }
        
        private void ProcessUnlockQueue()
        {
            if (unlockQueue.Count == 0)
            {
                isShowingUnlock = false;
                return;
            }
            
            isShowingUnlock = true;
            var data = unlockQueue.Dequeue();
            
            if (unlockPanel != null)
            {
                unlockPanel.SetActive(true);
                
                if (unlockTitleText != null)
                    unlockTitleText.text = $"解锁: {data.systemName}";
                
                if (unlockDescriptionText != null)
                    unlockDescriptionText.text = data.description;
                
                if (unlockIconImage != null && data.icon != null)
                    unlockIconImage.sprite = data.icon;
            }
            
            // 播放解锁音效
            // AudioManager.Instance.PlaySFX("System_Unlock");
            
            // 自动隐藏
            Invoke(nameof(HideSystemUnlock), unlockDisplayDuration);
        }
        
        private void HideSystemUnlock()
        {
            if (unlockPanel != null)
                unlockPanel.SetActive(false);
            
            // 处理队列中的下一个
            ProcessUnlockQueue();
        }
        
        /// <summary>
        /// 显示引导完成庆祝
        /// </summary>
        public void ShowCompletionCelebration()
        {
            if (completionPanel != null)
            {
                completionPanel.SetActive(true);
                
                if (completionParticles != null)
                    completionParticles.Play();
            }
            
            // 播放庆祝音效
            // AudioManager.Instance.PlaySFX("Tutorial_Complete");
            
            // 3秒后隐藏
            Invoke(nameof(HideCompletionCelebration), 5f);
        }
        
        private void HideCompletionCelebration()
        {
            if (completionPanel != null)
                completionPanel.SetActive(false);
        }
        
        /// <summary>
        /// 显示帮助面板
        /// </summary>
        public void ShowHelpPanel()
        {
            if (helpPanel != null)
            {
                helpPanel.SetActive(true);
                Time.timeScale = 0; // 暂停游戏
            }
        }
        
        /// <summary>
        /// 隐藏帮助面板
        /// </summary>
        public void HideHelpPanel()
        {
            if (helpPanel != null)
            {
                helpPanel.SetActive(false);
                Time.timeScale = 1; // 恢复游戏
            }
        }
        
        /// <summary>
        /// 重新播放引导
        /// </summary>
        public void ReplayTutorial()
        {
            HideHelpPanel();
            TutorialManager.Instance.RestartTutorial();
        }
        
        private void OnSkipClicked()
        {
            TutorialManager.Instance.SkipCurrentStep();
        }
        
        private void OnContinueClicked()
        {
            // 某些步骤可能需要手动继续
            // currentStep?.Continue();
        }
        
        private void OnDestroy()
        {
            if (skipButton != null)
                skipButton.onClick.RemoveListener(OnSkipClicked);
            
            if (continueButton != null)
                continueButton.onClick.RemoveListener(OnContinueClicked);
        }
    }
    
    /// <summary>
    /// 引导高亮效果组件
    /// </summary>
    public class TutorialHighlight : MonoBehaviour
    {
        [SerializeField] private RectTransform highlightRect;
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private Animator animator;
        
        private GameObject targetObject;
        private Camera mainCamera;
        
        private void Awake()
        {
            mainCamera = Camera.main;
        }
        
        private void Update()
        {
            if (targetObject == null)
            {
                Destroy(gameObject);
                return;
            }
            
            // 更新高亮位置
            UpdatePosition();
        }
        
        public void SetTarget(GameObject target)
        {
            targetObject = target;
            UpdatePosition();
        }
        
        public void SetHint(string hint)
        {
            if (hintText != null)
                hintText.text = hint;
        }
        
        private void UpdatePosition()
        {
            if (targetObject == null || mainCamera == null) return;
            
            // 获取目标在屏幕上的位置
            Vector3 screenPos = mainCamera.WorldToScreenPoint(targetObject.transform.position);
            
            // 转换为Canvas坐标
            if (highlightRect != null)
            {
                highlightRect.position = screenPos;
            }
        }
    }
    
    /// <summary>
    /// Tab组组件（用于帮助面板）
    /// </summary>
    public class TabGroup : MonoBehaviour
    {
        [SerializeField] private List<TabButton> tabButtons;
        [SerializeField] private List<GameObject> tabContents;
        
        private int currentTab = 0;
        
        private void Start()
        {
            SelectTab(0);
        }
        
        public void SelectTab(int index)
        {
            currentTab = index;
            
            for (int i = 0; i < tabContents.Count; i++)
            {
                if (tabContents[i] != null)
                    tabContents[i].SetActive(i == index);
            }
            
            foreach (var button in tabButtons)
            {
                button.SetSelected(tabButtons.IndexOf(button) == index);
            }
        }
    }
    
    public class TabButton : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Color selectedColor = Color.white;
        [SerializeField] private Color normalColor = Color.gray;
        
        public void SetSelected(bool selected)
        {
            if (background != null)
                background.color = selected ? selectedColor : normalColor;
        }
    }
}
