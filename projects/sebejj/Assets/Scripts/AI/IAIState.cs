/** 
 * @file IAIState.cs
 * @brief AI状态接口 - 任务AI-002
 * @description 定义AI状态的完整生命周期接口
 * @author AI系统架构师
 * @date 2026-02-27
 */

using UnityEngine;

namespace SebeJJ.AI
{
    /// <summary>
    /// AI状态接口 - 所有AI状态必须实现此接口
    /// </summary>
    public interface IAIState
    {
        /// <summary>
        /// 初始化状态
        /// </summary>
        /// <param name="stateMachine">所属状态机</param>
        /// <param name="stateType">状态类型</param>
        void Initialize(AIStateMachine stateMachine, EnemyState stateType);
        
        /// <summary>
        /// 进入状态时调用
        /// </summary>
        void OnEnter();
        
        /// <summary>
        /// 每帧更新
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        void OnUpdate(float deltaTime);
        
        /// <summary>
        /// 固定时间间隔更新（物理更新）
        /// </summary>
        /// <param name="fixedDeltaTime">固定时间增量</param>
        void OnFixedUpdate(float fixedDeltaTime);
        
        /// <summary>
        /// 退出状态时调用
        /// </summary>
        void OnExit();
        
        /// <summary>
        /// 销毁状态时调用
        /// </summary>
        void OnDispose();
        
        /// <summary>
        /// 绘制调试Gizmos
        /// </summary>
        void OnDrawGizmos();
        
        /// <summary>
        /// 绘制选中时的调试Gizmos
        /// </summary>
        void OnDrawGizmosSelected();
    }

    /// <summary>
    /// AI状态基类 - 提供默认实现
    /// </summary>
    public abstract class AIStateBase : IAIState
    {
        #region 属性
        
        /// <summary>
        /// 所属状态机
        /// </summary>
        protected AIStateMachine StateMachine { get; private set; }
        
        /// <summary>
        /// 状态类型
        /// </summary>
        protected EnemyState StateType { get; private set; }
        
        /// <summary>
        /// 状态拥有者（游戏对象）
        /// </summary>
        protected GameObject Owner => StateMachine?.gameObject;
        
        /// <summary>
        /// 状态拥有者的Transform
        /// </summary>
        protected Transform Transform => Owner?.transform;
        
        /// <summary>
        /// 当前状态持续时间
        /// </summary>
        protected float StateDuration => StateMachine?.CurrentStateDuration ?? 0f;
        
        #endregion

        #region 接口实现
        
        public virtual void Initialize(AIStateMachine stateMachine, EnemyState stateType)
        {
            StateMachine = stateMachine;
            StateType = stateType;
        }
        
        public abstract void OnEnter();
        public abstract void OnUpdate(float deltaTime);
        
        public virtual void OnFixedUpdate(float fixedDeltaTime)
        {
            // 默认空实现，子类可重写
        }
        
        public abstract void OnExit();
        
        public virtual void OnDispose()
        {
            // 默认空实现，子类可重写
        }
        
        public virtual void OnDrawGizmos()
        {
            // 默认空实现，子类可重写
        }
        
        public virtual void OnDrawGizmosSelected()
        {
            // 默认空实现，子类可重写
        }
        
        #endregion

        #region 工具方法
        
        /// <summary>
        /// 切换到指定状态
        /// </summary>
        /// <param name="newState">目标状态</param>
        /// <param name="force">是否强制切换</param>
        /// <returns>是否切换成功</returns>
        protected bool ChangeState(EnemyState newState, bool force = false)
        {
            return StateMachine?.ChangeState(newState, force) ?? false;
        }
        
        /// <summary>
        /// 检查是否可以转换到指定状态
        /// </summary>
        /// <param name="targetState">目标状态</param>
        /// <returns>是否可以转换</returns>
        protected bool CanTransitionTo(EnemyState targetState)
        {
            return StateMachine?.CanTransitionTo(targetState) ?? false;
        }
        
        /// <summary>
        /// 获取组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns>组件实例</returns>
        protected T GetComponent<T>() where T : Component
        {
            return Owner?.GetComponent<T>();
        }
        
        /// <summary>
        /// 记录调试日志
        /// </summary>
        /// <param name="message">消息</param>
        protected void Log(string message)
        {
            Debug.Log($"[{StateType}] {message}", Owner);
        }
        
        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="message">消息</param>
        protected void LogWarning(string message)
        {
            Debug.LogWarning($"[{StateType}] {message}", Owner);
        }
        
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">消息</param>
        protected void LogError(string message)
        {
            Debug.LogError($"[{StateType}] {message}", Owner);
        }
        
        #endregion
    }
}
