using UnityEngine;
using System;
using System.Collections.Generic;

namespace SebeJJ.Enemies
{
    /// <summary>
    /// 状态机基类
    /// </summary>
    public class StateMachine
    {
        private IState _currentState;
        private Dictionary<Type, IState> _states = new Dictionary<Type, IState>();

        public IState CurrentState => _currentState;
        public Type CurrentStateType => _currentState?.GetType();

        public event Action<IState, IState> OnStateChanged; // 新状态, 旧状态

        /// <summary>
        /// 添加状态
        /// </summary>
        public void AddState(IState state)
        {
            Type stateType = state.GetType();
            if (!_states.ContainsKey(stateType))
            {
                _states[stateType] = state;
            }
        }

        /// <summary>
        /// 切换到指定状态
        /// </summary>
        public void ChangeState<T>() where T : IState
        {
            ChangeState(typeof(T));
        }

        /// <summary>
        /// 切换到指定状态（通过类型）
        /// </summary>
        public void ChangeState(Type stateType)
        {
            if (!_states.ContainsKey(stateType)) return;

            IState newState = _states[stateType];
            
            if (_currentState == newState) return;

            // 退出当前状态
            IState previousState = _currentState;
            _currentState?.Exit();

            // 进入新状态
            _currentState = newState;
            _currentState.Enter();

            OnStateChanged?.Invoke(_currentState, previousState);
        }

        /// <summary>
        /// 更新当前状态
        /// </summary>
        public void Update()
        {
            _currentState?.Update();
        }

        /// <summary>
        /// 固定更新当前状态
        /// </summary>
        public void FixedUpdate()
        {
            _currentState?.FixedUpdate();
        }

        /// <summary>
        /// 检查当前状态
        /// </summary>
        public bool IsInState<T>() where T : IState
        {
            return _currentState is T;
        }
    }

    /// <summary>
    /// 状态接口
    /// </summary>
    public interface IState
    {
        void Enter();
        void Update();
        void FixedUpdate();
        void Exit();
    }

    /// <summary>
    /// 状态基类
    /// </summary>
    public abstract class StateBase : IState
    {
        protected EnemyBase _enemy;

        public StateBase(EnemyBase enemy)
        {
            _enemy = enemy;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void Exit() { }
    }
}
