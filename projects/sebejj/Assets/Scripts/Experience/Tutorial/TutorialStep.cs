using System;
using UnityEngine;

namespace SebeJJ.Experience.Tutorial
{
    /// <summary>
    /// 引导步骤基类
    /// </summary>
    public abstract class TutorialStep
    {
        public abstract string StepId { get; }
        public abstract string DisplayName { get; }
        public abstract bool CanSkip { get; }
        public virtual bool ShouldPauseGame => false;
        
        public event Action<TutorialStep> OnStepCompleted;
        public event Action<TutorialStep> OnStepUpdated;
        
        protected TutorialStepData Data { get; private set; }
        protected bool IsActive { get; private set; }
        protected float StepTimer { get; private set; }
        
        public void Initialize(TutorialStepData data)
        {
            Data = data;
        }
        
        /// <summary>
        /// 进入步骤时调用
        /// </summary>
        public virtual void OnEnter()
        {
            IsActive = true;
            StepTimer = 0;
            Debug.Log($"[TutorialStep] 进入步骤: {DisplayName}");
        }
        
        /// <summary>
        /// 每帧更新
        /// </summary>
        public virtual void Update()
        {
            if (!IsActive) return;
            StepTimer += Time.unscaledDeltaTime;
        }
        
        /// <summary>
        /// 退出步骤时调用
        /// </summary>
        public virtual void OnExit()
        {
            IsActive = false;
        }
        
        /// <summary>
        /// 跳过步骤
        /// </summary>
        public virtual void Skip()
        {
            OnExit();
            Complete();
        }
        
        /// <summary>
        /// 完成步骤
        /// </summary>
        protected void Complete()
        {
            IsActive = false;
            OnStepCompleted?.Invoke(this);
        }
        
        /// <summary>
        /// 更新进度（0-1）
        /// </summary>
        protected void UpdateProgress(float progress)
        {
            OnStepUpdated?.Invoke(this);
        }
        
        /// <summary>
        /// 获取步骤描述文本
        /// </summary>
        public virtual string GetDescription()
        {
            return Data?.description ?? DisplayName;
        }
        
        /// <summary>
        /// 获取步骤提示文本
        /// </summary>
        public virtual string GetHintText()
        {
            return string.Empty;
        }
    }
    
    /// <summary>
    /// 开场剧情步骤
    /// </summary>
    public class OpeningCinematicStep : TutorialStep
    {
        public override string StepId => "opening_cinematic";
        public override string DisplayName => "开场剧情";
        public override bool CanSkip => true;
        public override bool ShouldPauseGame => true;
        
        private bool cinematicEnded = false;
        
        public override void OnEnter()
        {
            base.OnEnter();
            
            // 播放开场动画
            // CinematicPlayer.Play("Opening_Cinematic", OnCinematicEnd);
            
            // 模拟：3秒后自动结束
            // 实际项目中使用动画事件或回调
        }
        
        private void OnCinematicEnd()
        {
            cinematicEnded = true;
            Complete();
        }
        
        public override void Update()
        {
            base.Update();
            
            // 按任意键跳过
            if (CanSkip && Input.anyKeyDown && StepTimer > 1f)
            {
                // CinematicPlayer.Skip();
                OnCinematicEnd();
            }
        }
    }
    
    /// <summary>
    /// 移动教学步骤
    /// </summary>
    public class MovementTutorialStep : TutorialStep
    {
        public override string StepId => "movement_tutorial";
        public override string DisplayName => "基础移动";
        public override bool CanSkip => true;
        
        private bool hasMoved = false;
        private bool hasAimed = false;
        private Vector3 startPosition;
        
        public override void OnEnter()
        {
            base.OnEnter();
            
            // 记录起始位置
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                startPosition = player.transform.position;
            }
        }
        
        public override void Update()
        {
            base.Update();
            
            // 检测移动
            if (!hasMoved)
            {
                float moveInput = Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical"));
                if (moveInput > 0.1f)
                {
                    hasMoved = true;
                }
            }
            
            // 检测瞄准（鼠标移动）
            if (!hasAimed)
            {
                if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
                {
                    hasAimed = true;
                }
            }
            
            // 完成条件：移动并瞄准
            if (hasMoved && hasAimed)
            {
                // 延迟完成，给玩家反应时间
                if (StepTimer > 2f)
                {
                    Complete();
                }
            }
        }
        
        public override string GetHintText()
        {
            if (!hasMoved)
                return "使用 WASD 移动机甲";
            if (!hasAimed)
                return "移动鼠标瞄准";
            return "很好！继续练习移动";
        }
    }
    
    /// <summary>
    /// 交互教学步骤
    /// </summary>
    public class InteractionTutorialStep : TutorialStep
    {
        public override string StepId => "interaction_tutorial";
        public override string DisplayName => "交互操作";
        public override bool CanSkip => true;
        
        private bool hasInteracted = false;
        
        public override void OnEnter()
        {
            base.OnEnter();
            
            // 高亮可交互对象
            HighlightInteractables();
        }
        
        public override void Update()
        {
            base.Update();
            
            // 检测交互按键
            if (!hasInteracted && Input.GetKeyDown(KeyCode.E))
            {
                hasInteracted = true;
                Complete();
            }
        }
        
        private void HighlightInteractables()
        {
            // 找到附近的可交互对象并高亮
            var interactables = GameObject.FindGameObjectsWithTag("Interactable");
            foreach (var obj in interactables)
            {
                // 添加高亮效果
                // HighlightEffect.Add(obj);
            }
        }
        
        public override string GetHintText()
        {
            return "靠近可交互对象，按 E 键交互";
        }
    }
    
    /// <summary>
    /// 采集教学步骤
    /// </summary>
    public class CollectionTutorialStep : TutorialStep
    {
        public override string StepId => "collection_tutorial";
        public override string DisplayName => "资源采集";
        public override bool CanSkip => true;
        
        private int collectedCount = 0;
        private int targetCount = 1;
        
        public override void OnEnter()
        {
            base.OnEnter();
            
            // 订阅采集事件
            // EventBus.Subscribe<ResourceCollectedEvent>(OnResourceCollected);
            
            // 高亮附近的资源
            HighlightResources();
        }
        
        public override void OnExit()
        {
            base.OnExit();
            // EventBus.Unsubscribe<ResourceCollectedEvent>(OnResourceCollected);
        }
        
        private void OnResourceCollected(ResourceCollectedEvent evt)
        {
            collectedCount++;
            if (collectedCount >= targetCount)
            {
                Complete();
            }
        }
        
        private void HighlightResources()
        {
            var resources = GameObject.FindGameObjectsWithTag("Resource");
            foreach (var res in resources)
            {
                // 高亮最近的资源
                // HighlightEffect.Add(res);
            }
        }
        
        public override string GetHintText()
        {
            return "按住鼠标左键采集资源";
        }
    }
    
    /// <summary>
    /// 战斗教学步骤
    /// </summary>
    public class CombatTutorialStep : TutorialStep
    {
        public override string StepId => "combat_tutorial";
        public override string DisplayName => "基础战斗";
        public override bool CanSkip => true;
        
        private int enemiesDefeated = 0;
        private int targetDefeats = 1;
        
        public override void OnEnter()
        {
            base.OnEnter();
            // EventBus.Subscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
        }
        
        public override void OnExit()
        {
            base.OnExit();
            // EventBus.Unsubscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
        }
        
        private void OnEnemyDefeated(EnemyDefeatedEvent evt)
        {
            enemiesDefeated++;
            if (enemiesDefeated >= targetDefeats)
            {
                Complete();
            }
        }
        
        public override string GetHintText()
        {
            return "点击鼠标左键攻击敌人";
        }
    }
    
    /// <summary>
    /// UI说明步骤
    /// </summary>
    public class UIExplainStep : TutorialStep
    {
        public override string StepId => "ui_explain";
        public override string DisplayName => "界面说明";
        public override bool CanSkip => true;
        public override bool ShouldPauseGame => true;
        
        private int currentExplainIndex = 0;
        private string[] uiElements = { "HealthBar", "EnergyBar", "Minimap", "Inventory" };
        
        public override void OnEnter()
        {
            base.OnEnter();
            HighlightUIElement(uiElements[0]);
        }
        
        public override void Update()
        {
            base.Update();
            
            // 点击继续
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                currentExplainIndex++;
                if (currentExplainIndex >= uiElements.Length)
                {
                    Complete();
                }
                else
                {
                    HighlightUIElement(uiElements[currentExplainIndex]);
                }
            }
        }
        
        private void HighlightUIElement(string elementName)
        {
            // TutorialUI.Instance.HighlightElement(elementName);
        }
    }
    
    /// <summary>
    /// 自由练习步骤
    /// </summary>
    public class FreePlayStep : TutorialStep
    {
        public override string StepId => "free_play";
        public override string DisplayName => "自由练习";
        public override bool CanSkip => true;
        
        private float practiceDuration = 30f; // 30秒自由练习
        
        public override void Update()
        {
            base.Update();
            
            if (StepTimer >= practiceDuration)
            {
                Complete();
            }
        }
        
        public override string GetHintText()
        {
            float remaining = Mathf.Max(0, practiceDuration - StepTimer);
            return $"自由练习中... ({remaining:F0}秒)";
        }
    }
    
    // 事件定义
    public struct ResourceCollectedEvent { public string ResourceId; public int Amount; }
    public struct EnemyDefeatedEvent { public string EnemyId; }
}
