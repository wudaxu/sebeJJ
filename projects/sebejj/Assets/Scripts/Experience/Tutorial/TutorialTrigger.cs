using System;
using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Experience.Tutorial
{
    /// <summary>
    /// 引导触发器 - 用于在特定条件下触发引导
    /// </summary>
    public class TutorialTrigger : MonoBehaviour
    {
        public enum TriggerType
        {
            OnEnter,        // 进入区域触发
            OnInteract,     // 交互时触发
            OnEvent,        // 特定事件触发
            OnCondition     // 满足条件触发
        }
        
        [Header("触发配置")]
        [SerializeField] private TriggerType triggerType = TriggerType.OnEnter;
        [SerializeField] private string triggerId;
        [SerializeField] private bool triggerOnce = true;
        
        [Header("触发条件")]
        [SerializeField] private int requiredLevel = 0;
        [SerializeField] private string requiredMissionId;
        [SerializeField] private float requiredDepth = 0;
        
        [Header("触发动作")]
        [SerializeField] private string startTutorialStepId;
        [SerializeField] private string unlockSystemId;
        [SerializeField] private string unlockSystemName;
        [SerializeField] private string unlockDescription;
        
        private bool hasTriggered = false;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggerType != TriggerType.OnEnter) return;
            if (!other.CompareTag("Player")) return;
            
            TryTrigger();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (triggerType != TriggerType.OnEnter) return;
            if (!other.CompareTag("Player")) return;
            
            TryTrigger();
        }
        
        /// <summary>
        /// 尝试触发引导
        /// </summary>
        public void TryTrigger()
        {
            if (hasTriggered && triggerOnce) return;
            if (!CheckConditions()) return;
            
            hasTriggered = true;
            
            // 触发引导步骤
            if (!string.IsNullOrEmpty(startTutorialStepId))
            {
                // TutorialManager.Instance.StartSpecificStep(startTutorialStepId);
            }
            
            // 解锁系统
            if (!string.IsNullOrEmpty(unlockSystemId))
            {
                TutorialManager.Instance.UnlockSystem(unlockSystemId, unlockSystemName, unlockDescription);
            }
            
            // 发送事件
            // EventBus.Publish(new TutorialTriggerEvent { TriggerId = triggerId });
        }
        
        private bool CheckConditions()
        {
            // 检查等级
            if (requiredLevel > 0)
            {
                // if (PlayerManager.Instance.Level < requiredLevel) return false;
            }
            
            // 检查委托
            if (!string.IsNullOrEmpty(requiredMissionId))
            {
                // if (!MissionManager.Instance.IsMissionCompleted(requiredMissionId)) return false;
            }
            
            // 检查深度
            if (requiredDepth > 0)
            {
                // if (DiveManager.Instance.CurrentDepth < requiredDepth) return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 从外部触发（如按钮点击）
        /// </summary>
        public void TriggerFromExternal()
        {
            TryTrigger();
        }
        
        private void OnDrawGizmos()
        {
            // 绘制触发器范围
            Gizmos.color = Color.cyan;
            
            var collider2D = GetComponent<Collider2D>();
            if (collider2D != null)
            {
                Gizmos.DrawWireCube(collider2D.bounds.center, collider2D.bounds.size);
            }
            
            var collider3D = GetComponent<Collider>();
            if (collider3D != null)
            {
                Gizmos.DrawWireCube(collider3D.bounds.center, collider3D.bounds.size);
            }
        }
    }
    
    /// <summary>
    /// 条件触发器 - 持续检查条件
    /// </summary>
    public class TutorialConditionTrigger : MonoBehaviour
    {
        [System.Serializable]
        public class Condition
        {
            public ConditionType type;
            public string targetId;
            public int targetValue;
            public bool invert;
        }
        
        public enum ConditionType
        {
            PlayerLevel,
            MissionCompleted,
            ResourceCollected,
            EnemyDefeated,
            DepthReached,
            EquipmentOwned,
            SystemUnlocked
        }
        
        [SerializeField] private List<Condition> conditions;
        [SerializeField] private bool checkAll = true; // true = AND, false = OR
        [SerializeField] private float checkInterval = 1f;
        
        [SerializeField] private string tutorialStepId;
        [SerializeField] private bool triggerOnce = true;
        
        private bool hasTriggered = false;
        private float timer = 0;
        
        private void Update()
        {
            if (hasTriggered && triggerOnce) return;
            
            timer += Time.deltaTime;
            if (timer >= checkInterval)
            {
                timer = 0;
                CheckConditions();
            }
        }
        
        private void CheckConditions()
        {
            bool result = checkAll ? true : false;
            
            foreach (var condition in conditions)
            {
                bool conditionMet = EvaluateCondition(condition);
                
                if (checkAll)
                {
                    result = result && conditionMet;
                    if (!result) break; // 早期退出
                }
                else
                {
                    result = result || conditionMet;
                    if (result) break; // 早期退出
                }
            }
            
            if (result)
            {
                Trigger();
            }
        }
        
        private bool EvaluateCondition(Condition condition)
        {
            bool result = false;
            
            switch (condition.type)
            {
                case ConditionType.PlayerLevel:
                    // result = PlayerManager.Instance.Level >= condition.targetValue;
                    break;
                    
                case ConditionType.MissionCompleted:
                    // result = MissionManager.Instance.IsMissionCompleted(condition.targetId);
                    break;
                    
                case ConditionType.ResourceCollected:
                    // result = InventoryManager.Instance.GetItemCount(condition.targetId) >= condition.targetValue;
                    break;
                    
                case ConditionType.EnemyDefeated:
                    // result = StatisticsManager.Instance.GetEnemyKillCount(condition.targetId) >= condition.targetValue;
                    break;
                    
                case ConditionType.DepthReached:
                    // result = DiveManager.Instance.MaxDepthReached >= condition.targetValue;
                    break;
                    
                case ConditionType.EquipmentOwned:
                    // result = EquipmentManager.Instance.HasEquipment(condition.targetId);
                    break;
                    
                case ConditionType.SystemUnlocked:
                    result = TutorialManager.Instance.IsSystemUnlocked(condition.targetId);
                    break;
            }
            
            return condition.invert ? !result : result;
        }
        
        private void Trigger()
        {
            hasTriggered = true;
            
            if (!string.IsNullOrEmpty(tutorialStepId))
            {
                // TutorialManager.Instance.StartSpecificStep(tutorialStepId);
            }
            
            Debug.Log($"[TutorialConditionTrigger] 条件满足，触发引导: {tutorialStepId}");
        }
    }
    
    public struct TutorialTriggerEvent
    {
        public string TriggerId;
    }
}
