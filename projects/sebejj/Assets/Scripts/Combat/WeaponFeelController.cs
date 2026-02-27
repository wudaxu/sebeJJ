using UnityEngine;
using System;
using System.Collections;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 武器手感控制器 - WP-009
    /// 管理攻击前摇、后摇、硬直和取消系统
    /// </summary>
    public class WeaponFeelController : MonoBehaviour
    {
        [Header("攻击时机")]
        [SerializeField] private float attackWindup = 0.15f;         // 攻击前摇
        [SerializeField] private float attackActive = 0.2f;          // 攻击判定持续时间
        [SerializeField] private float attackRecovery = 0.25f;       // 攻击后摇
        
        [Header("取消窗口")]
        [SerializeField] private float windupCancelWindow = 0.3f;    // 前摇取消窗口(百分比)
        [SerializeField] private float recoveryCancelWindow = 0.5f;  // 后摇取消窗口(百分比)
        
        [Header("硬直设置")]
        [SerializeField] private float hitStunDuration = 0.08f;      // 命中硬直
        [SerializeField] private float hitStopDuration = 0.05f;      // 命中停顿
        
        [Header("震动设置")]
        [SerializeField] private float hitShakeIntensity = 0.2f;     // 命中震动强度
        [SerializeField] private float hitShakeDuration = 0.15f;     // 命中震动时长
        [SerializeField] private float criticalShakeIntensity = 0.4f; // 暴击震动强度
        [SerializeField] private float criticalShakeDuration = 0.2f;  // 暴击震动时长
        
        // 运行时状态
        private float attackTimer = 0f;
        private float currentPhaseDuration = 0f;
        private AttackPhase currentPhase = AttackPhase.Idle;
        
        // 事件
        public event Action OnWindupStart;      // 前摇开始
        public event Action OnActiveStart;      // 判定开始
        public event Action OnRecoveryStart;    // 后摇开始
        public event Action OnAttackComplete;   // 攻击完成
        public event Action OnAttackCancelled;  // 攻击取消
        
        // 属性
        public AttackPhase CurrentPhase => currentPhase;
        public float PhaseProgress => currentPhaseDuration > 0 ? attackTimer / currentPhaseDuration : 0f;
        public bool IsAttacking => currentPhase != AttackPhase.Idle;
        public bool CanCancel => CanCancelAttack();
        public float TotalAttackDuration => attackWindup + attackActive + attackRecovery;
        
        /// <summary>
        /// 攻击阶段枚举
        /// </summary>
        public enum AttackPhase
        {
            Idle,       // 待机
            Windup,     // 前摇
            Active,     // 判定中
            Recovery    // 后摇
        }
        
        /// <summary>
        /// 开始攻击
        /// </summary>
        public void StartAttack()
        {
            if (currentPhase != AttackPhase.Idle) return;
            
            currentPhase = AttackPhase.Windup;
            attackTimer = 0f;
            currentPhaseDuration = attackWindup;
            OnWindupStart?.Invoke();
        }
        
        /// <summary>
        /// 取消攻击
        /// </summary>
        public bool TryCancelAttack()
        {
            if (!CanCancel) return false;
            
            StopAllCoroutines();
            currentPhase = AttackPhase.Idle;
            attackTimer = 0f;
            OnAttackCancelled?.Invoke();
            return true;
        }
        
        /// <summary>
        /// 检查是否可以取消
        /// </summary>
        private bool CanCancelAttack()
        {
            switch (currentPhase)
            {
                case AttackPhase.Windup:
                    return PhaseProgress <= windupCancelWindow;
                case AttackPhase.Recovery:
                    return PhaseProgress >= (1f - recoveryCancelWindow);
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// 触发命中反馈
        /// </summary>
        public void TriggerHitFeedback(bool isCritical = false, bool isKill = false)
        {
            // 命中停顿
            float stopDuration = isKill ? hitStopDuration * 2f : hitStopDuration;
            CombatFeedback.Instance?.TriggerHitStop(stopDuration);
            
            // 屏幕震动
            float shakeIntensity = isCritical ? criticalShakeIntensity : hitShakeIntensity;
            float shakeDuration = isCritical ? criticalShakeDuration : hitShakeDuration;
            
            if (isKill)
            {
                shakeIntensity *= 1.25f;
                shakeDuration *= 1.25f;
            }
            
            CombatFeedback.Instance?.TriggerScreenShake(shakeIntensity, shakeDuration);
        }
        
        /// <summary>
        /// 更新攻击阶段
        /// </summary>
        private void Update()
        {
            if (currentPhase == AttackPhase.Idle) return;
            
            attackTimer += Time.deltaTime;
            
            switch (currentPhase)
            {
                case AttackPhase.Windup:
                    if (attackTimer >= attackWindup)
                    {
                        currentPhase = AttackPhase.Active;
                        attackTimer = 0f;
                        currentPhaseDuration = attackActive;
                        OnActiveStart?.Invoke();
                    }
                    break;
                    
                case AttackPhase.Active:
                    if (attackTimer >= attackActive)
                    {
                        currentPhase = AttackPhase.Recovery;
                        attackTimer = 0f;
                        currentPhaseDuration = attackRecovery;
                        OnRecoveryStart?.Invoke();
                    }
                    break;
                    
                case AttackPhase.Recovery:
                    if (attackTimer >= attackRecovery)
                    {
                        currentPhase = AttackPhase.Idle;
                        attackTimer = 0f;
                        OnAttackComplete?.Invoke();
                    }
                    break;
            }
        }
        
        /// <summary>
        /// 获取当前阶段名称
        /// </summary>
        public string GetPhaseName()
        {
            switch (currentPhase)
            {
                case AttackPhase.Windup: return "前摇";
                case AttackPhase.Active: return "判定中";
                case AttackPhase.Recovery: return "后摇";
                default: return "待机";
            }
        }
        
        /// <summary>
        /// 设置攻击时机参数
        /// </summary>
        public void SetAttackTiming(float windup, float active, float recovery)
        {
            attackWindup = windup;
            attackActive = active;
            attackRecovery = recovery;
        }
        
        /// <summary>
        /// 重置状态
        /// </summary>
        public void Reset()
        {
            currentPhase = AttackPhase.Idle;
            attackTimer = 0f;
        }
    }
}
