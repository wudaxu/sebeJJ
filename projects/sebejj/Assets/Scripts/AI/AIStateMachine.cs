/** 
 * @file AIStateMachine.cs
 * @brief AI状态机基类 - 任务AI-001~003
 * @description 提供完整的AI状态机框架，支持状态切换、事件驱动和平滑过渡
 * @author AI系统架构师
 * @date 2026-02-27
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.AI
{
    /// <summary>
    /// AI状态枚举 - 定义敌人可能处于的所有状态
    /// </summary>
    public enum EnemyState
    {
        Idle,           // 待机 - 原地不动，随机观察
        Patrol,         // 巡逻 - 在区域内移动
        Alert,          // 警戒 - 发现可疑目标，提高警惕
        Chase,          // 追击 - 主动追击目标
        Attack,         // 攻击 - 执行攻击动作
        Flee,           // 逃跑 - 生命值低时逃离
        Stunned,        // 眩晕 - 被控制无法行动
        Defend,         // 防御 - 采取防御姿态
        Special,        // 特殊技能 - Boss特殊攻击
        Dead            // 死亡 - 已被击败
    }

    /// <summary>
    /// 状态转换条件委托
    /// </summary>
    /// <param name="from">源状态</param>
    /// <param name="to">目标状态</param>
    /// <returns>是否允许转换</returns>
    public delegate bool StateTransitionCondition(EnemyState from, EnemyState to);

    /// <summary>
    /// AI状态机基类 - 管理所有AI状态的切换和生命周期
    /// </summary>
    public class AIStateMachine : MonoBehaviour
    {
        #region 事件定义
        
        /// <summary>
        /// 状态改变事件
        /// </summary>
        public event Action<EnemyState, EnemyState> OnStateChanged;
        
        /// <summary>
        /// 状态机更新事件（每帧）
        /// </summary>
        public event Action<float> OnStateMachineUpdate;
        
        #endregion

        #region 序列化字段
        
        [Header("状态机配置")]
        [SerializeField] private EnemyState initialState = EnemyState.Idle;
        [SerializeField] private bool enableDebugLog = false;
        [SerializeField] private bool enableGizmos = true;
        
        [Header("状态切换")]
        [SerializeField] private float stateTransitionDuration = 0.2f;
        [SerializeField] private bool allowSelfTransition = false;
        
        #endregion

        #region 私有字段
        
        /// <summary>
        /// 状态字典 - 存储所有注册的状态
        /// </summary>
        private Dictionary<EnemyState, IAIState> _states = new Dictionary<EnemyState, IAIState>();
        
        /// <summary>
        /// 当前状态
        /// </summary>
        private IAIState _currentState;
        
        /// <summary>
        /// 当前状态类型
        /// </summary>
        private EnemyState _currentStateType;
        
        /// <summary>
        /// 上一个状态类型
        /// </summary>
        private EnemyState _previousStateType;
        
        /// <summary>
        /// 状态转换条件字典
        /// </summary>
        private Dictionary<Tuple<EnemyState, EnemyState>, StateTransitionCondition> _transitionConditions = 
            new Dictionary<Tuple<EnemyState, EnemyState>, StateTransitionCondition>();
        
        /// <summary>
        /// 是否正在状态转换中
        /// </summary>
        private bool _isTransitioning = false;
        
        /// <summary>
        /// BC-006修复: 状态切换计数器，防止无限循环
        /// </summary>
        private int stateChangeCount = 0;
        
        /// <summary>
        /// BC-006修复: 每帧最大状态切换次数
        /// </summary>
        private const int MAX_STATE_CHANGES_PER_FRAME = 5;
        
        /// <summary>
        /// AF-003修复: 状态切换冷却时间
        /// </summary>
        [Header("状态切换冷却")]
        [SerializeField] private float stateSwitchCooldown = 0.5f;
        private float lastStateSwitchTime;
        
        /// <summary>
        /// 状态转换计时器
        /// </summary>
        private float _transitionTimer = 0f;
        
        /// <summary>
        /// 待切换的目标状态
        /// </summary>
        private EnemyState? _pendingState = null;
        
        /// <summary>
        /// 状态机是否已初始化
        /// </summary>
        private bool _isInitialized = false;
        
        #endregion

        #region 公共属性
        
        /// <summary>
        /// 当前状态类型
        /// </summary>
        public EnemyState CurrentState => _currentStateType;
        
        /// <summary>
        /// 上一个状态类型
        /// </summary>
        public EnemyState PreviousState => _previousStateType;
        
        /// <summary>
        /// 状态机是否已初始化
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// 是否正在状态转换中
        /// </summary>
        public bool IsTransitioning => _isTransitioning;
        
        /// <summary>
        /// 当前状态持续时间
        /// </summary>
        public float CurrentStateDuration { get; private set; } = 0f;
        
        /// <summary>
        /// 状态机运行总时间
        /// </summary>
        public float TotalRunningTime { get; private set; } = 0f;
        
        #endregion

        #region Unity生命周期
        
        protected virtual void Awake()
        {
            InitializeStateMachine();
        }
        
        protected virtual void Start()
        {
            // 切换到初始状态
            if (_states.ContainsKey(initialState))
            {
                ChangeState(initialState);
            }
            else
            {
                Debug.LogWarning($"[AIStateMachine] 初始状态 {initialState} 未注册，请确保先注册状态");
            }
        }
        
        protected virtual void Update()
        {
            if (!_isInitialized) return;
            
            // BC-006修复: 每帧重置状态切换计数器
            stateChangeCount = 0;
            
            float deltaTime = Time.deltaTime;
            TotalRunningTime += deltaTime;
            
            // 处理状态转换
            if (_isTransitioning)
            {
                ProcessTransition(deltaTime);
                return;
            }
            
            // 更新当前状态
            if (_currentState != null)
            {
                CurrentStateDuration += deltaTime;
                _currentState.OnUpdate(deltaTime);
            }
            
            OnStateMachineUpdate?.Invoke(deltaTime);
        }
        
        protected virtual void FixedUpdate()
        {
            if (!_isInitialized || _isTransitioning) return;
            
            // 物理更新
            if (_currentState != null)
            {
                _currentState.OnFixedUpdate(Time.fixedDeltaTime);
            }
        }
        
        protected virtual void OnDestroy()
        {
            // 清理所有状态
            foreach (var state in _states.Values)
            {
                state?.OnDispose();
            }
            _states.Clear();
        }
        
        #endregion

        #region 状态机初始化
        
        /// <summary>
        /// 初始化状态机
        /// </summary>
        private void InitializeStateMachine()
        {
            if (_isInitialized) return;
            
            // 注册默认状态转换条件
            RegisterDefaultTransitionConditions();
            
            _isInitialized = true;
            
            if (enableDebugLog)
            {
                Debug.Log($"[AIStateMachine] 状态机初始化完成 - {gameObject.name}");
            }
        }
        
        /// <summary>
        /// 注册默认状态转换条件
        /// </summary>
        private void RegisterDefaultTransitionConditions()
        {
            // 死亡状态可以从任何状态进入
            RegisterTransitionCondition((from, to) => true, EnemyState.Dead);
            
            // 眩晕状态可以从任何状态进入（除了死亡）
            RegisterTransitionCondition((from, to) => from != EnemyState.Dead, EnemyState.Stunned);
        }
        
        #endregion

        #region 状态注册
        
        /// <summary>
        /// 注册状态
        /// </summary>
        /// <param name="stateType">状态类型</param>
        /// <param name="state">状态实例</param>
        /// <returns>是否注册成功</returns>
        public bool RegisterState(EnemyState stateType, IAIState state)
        {
            if (state == null)
            {
                Debug.LogError($"[AIStateMachine] 尝试注册空状态: {stateType}");
                return false;
            }
            
            if (_states.ContainsKey(stateType))
            {
                Debug.LogWarning($"[AIStateMachine] 状态 {stateType} 已存在，将被覆盖");
                _states[stateType]?.OnDispose();
            }
            
            _states[stateType] = state;
            state.Initialize(this, stateType);
            
            if (enableDebugLog)
            {
                Debug.Log($"[AIStateMachine] 注册状态: {stateType}");
            }
            
            return true;
        }
        
        /// <summary>
        /// 批量注册状态
        /// </summary>
        /// <param name="states">状态字典</param>
        public void RegisterStates(Dictionary<EnemyState, IAIState> states)
        {
            foreach (var kvp in states)
            {
                RegisterState(kvp.Key, kvp.Value);
            }
        }
        
        /// <summary>
        /// 注销状态
        /// </summary>
        /// <param name="stateType">状态类型</param>
        /// <returns>是否注销成功</returns>
        public bool UnregisterState(EnemyState stateType)
        {
            if (!_states.ContainsKey(stateType))
            {
                return false;
            }
            
            // 如果当前正在使用该状态，先退出
            if (_currentStateType == stateType)
            {
                _currentState?.OnExit();
                _currentState = null;
            }
            
            _states[stateType]?.OnDispose();
            _states.Remove(stateType);
            
            return true;
        }
        
        /// <summary>
        /// 获取状态
        /// </summary>
        /// <param name="stateType">状态类型</param>
        /// <returns>状态实例</returns>
        public IAIState GetState(EnemyState stateType)
        {
            return _states.TryGetValue(stateType, out var state) ? state : null;
        }
        
        /// <summary>
        /// 检查状态是否已注册
        /// </summary>
        /// <param name="stateType">状态类型</param>
        /// <returns>是否已注册</returns>
        public bool HasState(EnemyState stateType)
        {
            return _states.ContainsKey(stateType);
        }
        
        #endregion

        #region 状态转换
        
        /// <summary>
        /// 切换状态
        /// BC-006修复: 添加最大状态切换次数限制
        /// AF-003修复: 添加状态切换冷却
        /// </summary>
        /// <param name="newState">目标状态</param>
        /// <param name="force">是否强制切换（忽略条件）</param>
        /// <returns>是否切换成功</returns>
        public bool ChangeState(EnemyState newState, bool force = false)
        {
            // BC-006修复: 检查状态切换次数限制
            stateChangeCount++;
            if (stateChangeCount > MAX_STATE_CHANGES_PER_FRAME)
            {
                Debug.LogError($"[AIStateMachine] 状态切换次数超过限制，可能存在循环! 对象: {gameObject.name}");
                return false;
            }
            
            // AF-003修复: 检查冷却时间
            if (!force && Time.time < lastStateSwitchTime + stateSwitchCooldown)
                return false;
            
            // 检查是否允许自转换
            if (!allowSelfTransition && _currentStateType == newState)
            {
                return false;
            }
            
            // 检查状态是否已注册
            if (!_states.ContainsKey(newState))
            {
                Debug.LogError($"[AIStateMachine] 尝试切换到未注册的状态: {newState}");
                return false;
            }
            
            // 检查转换条件
            if (!force && !CanTransitionTo(newState))
            {
                if (enableDebugLog)
                {
                    Debug.Log($"[AIStateMachine] 状态转换被拒绝: {_currentStateType} -> {newState}");
                }
                return false;
            }
            
            // 如果正在转换中，缓存目标状态
            if (_isTransitioning)
            {
                _pendingState = newState;
                return true;
            }
            
            // 开始状态转换
            BeginStateTransition(newState);
            
            // AF-003修复: 记录状态切换时间
            lastStateSwitchTime = Time.time;
            
            return true;
        }
        
        /// <summary>
        /// 检查是否可以转换到指定状态
        /// </summary>
        /// <param name="targetState">目标状态</param>
        /// <returns>是否可以转换</returns>
        public bool CanTransitionTo(EnemyState targetState)
        {
            var key = Tuple.Create(_currentStateType, targetState);
            
            if (_transitionConditions.TryGetValue(key, out var condition))
            {
                return condition(_currentStateType, targetState);
            }
            
            // 如果没有特定条件，检查通用条件
            key = Tuple.Create(EnemyState.Idle, targetState);
            if (_transitionConditions.TryGetValue(key, out var genericCondition))
            {
                return genericCondition(_currentStateType, targetState);
            }
            
            // 默认允许转换
            return true;
        }
        
        /// <summary>
        /// 注册状态转换条件
        /// </summary>
        /// <param name="condition">条件委托</param>
        /// <param name="toState">目标状态（null表示所有状态）</param>
        /// <param name="fromState">源状态（null表示所有状态）</param>
        public void RegisterTransitionCondition(StateTransitionCondition condition, 
            EnemyState? toState = null, EnemyState? fromState = null)
        {
            var key = Tuple.Create(fromState ?? _currentStateType, toState ?? EnemyState.Idle);
            _transitionConditions[key] = condition;
        }
        
        /// <summary>
        /// 开始状态转换
        /// </summary>
        /// <param name="newState">新状态</param>
        private void BeginStateTransition(EnemyState newState)
        {
            _isTransitioning = true;
            _transitionTimer = 0f;
            _pendingState = newState;
            
            if (stateTransitionDuration <= 0f)
            {
                // 立即完成转换
                CompleteStateTransition();
            }
        }
        
        /// <summary>
        /// 处理状态转换过程
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        private void ProcessTransition(float deltaTime)
        {
            _transitionTimer += deltaTime;
            
            if (_transitionTimer >= stateTransitionDuration)
            {
                CompleteStateTransition();
            }
        }
        
        /// <summary>
        /// 完成状态转换
        /// </summary>
        private void CompleteStateTransition()
        {
            if (!_pendingState.HasValue) return;
            
            EnemyState newState = _pendingState.Value;
            
            // 退出当前状态
            if (_currentState != null)
            {
                _currentState.OnExit();
            }
            
            // 记录上一个状态
            _previousStateType = _currentStateType;
            
            // 进入新状态
            _currentStateType = newState;
            _currentState = _states[newState];
            CurrentStateDuration = 0f;
            
            _currentState.OnEnter();
            
            _isTransitioning = false;
            _pendingState = null;
            
            // 触发事件
            OnStateChanged?.Invoke(_previousStateType, newState);
            
            if (enableDebugLog)
            {
                Debug.Log($"[AIStateMachine] 状态切换: {_previousStateType} -> {newState}");
            }
        }
        
        /// <summary>
        /// 返回上一个状态
        /// </summary>
        /// <returns>是否切换成功</returns>
        public bool ReturnToPreviousState()
        {
            return ChangeState(_previousStateType);
        }
        
        #endregion

        #region 调试与可视化
        
        protected virtual void OnDrawGizmos()
        {
            if (!enableGizmos || !Application.isPlaying) return;
            
            // 绘制当前状态信息
            string stateText = _currentStateType.ToString();
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                $"State: {stateText}\nTime: {CurrentStateDuration:F1}s");
            #endif
            
            // 绘制当前状态的Gizmos
            _currentState?.OnDrawGizmos();
        }
        
        protected virtual void OnDrawGizmosSelected()
        {
            if (!enableGizmos) return;
            
            // 绘制所有状态的调试信息
            foreach (var state in _states.Values)
            {
                state?.OnDrawGizmosSelected();
            }
        }
        
        #endregion

        #region 工具方法
        
        /// <summary>
        /// 获取状态机状态字符串
        /// </summary>
        /// <returns>状态信息</returns>
        public override string ToString()
        {
            return $"AIStateMachine[{gameObject.name}]: Current={_currentStateType}, " +
                   $"Previous={_previousStateType}, Duration={CurrentStateDuration:F2}s";
        }
        
        #endregion
    }
}
