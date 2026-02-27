using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace SebeJJ.Tests
{
    /// <summary>
    /// 状态机系统测试 - Week 2 敌人AI
    /// </summary>
    public class StateMachineTests
    {
        private StateMachine _stateMachine;
        private TestState _idleState;
        private TestState _patrolState;
        private TestState _chaseState;
        private TestState _attackState;

        [SetUp]
        public void Setup()
        {
            _stateMachine = new StateMachine();
            _idleState = new TestState("Idle");
            _patrolState = new TestState("Patrol");
            _chaseState = new TestState("Chase");
            _attackState = new TestState("Attack");
            
            _stateMachine.AddState(_idleState);
            _stateMachine.AddState(_patrolState);
            _stateMachine.AddState(_chaseState);
            _stateMachine.AddState(_attackState);
        }

        [TearDown]
        public void Teardown()
        {
            _stateMachine = null;
        }

        #region 状态机初始化

        [Test]
        [Category("AI")]
        [Category("StateMachine")]
        public void StateMachine_AddState_StateRegistered()
        {
            // Arrange
            var newState = new TestState("Test");
            
            // Act
            _stateMachine.AddState(newState);
            
            // Assert
            Assert.IsTrue(_stateMachine.HasState<TestState>());
        }

        [Test]
        [Category("AI")]
        [Category("StateMachine")]
        public void StateMachine_InitialState_IsNull()
        {
            // Assert
            Assert.IsNull(_stateMachine.CurrentState);
        }

        #endregion

        #region 状态切换

        [Test]
        [Category("AI")]
        [Category("StateMachine")]
        public void StateMachine_ChangeState_SetsCurrentState()
        {
            // Act
            _stateMachine.ChangeState<TestState>(_idleState);
            
            // Assert
            Assert.AreEqual(_idleState, _stateMachine.CurrentState);
        }

        [Test]
        [Category("AI")]
        [Category("StateMachine")]
        public void StateMachine_ChangeState_CallsEnterOnNewState()
        {
            // Act
            _stateMachine.ChangeState<TestState>(_idleState);
            
            // Assert
            Assert.IsTrue(_idleState.EnterCalled);
        }

        [Test]
        [Category("AI")]
        [Category("StateMachine")]
        public void StateMachine_ChangeState_CallsExitOnOldState()
        {
            // Arrange
            _stateMachine.ChangeState<TestState>(_idleState);
            
            // Act
            _stateMachine.ChangeState<TestState>(_patrolState);
            
            // Assert
            Assert.IsTrue(_idleState.ExitCalled);
        }

        [Test]
        [Category("AI")]
        [Category("StateMachine")]
        public void StateMachine_ChangeState_ToSameState_DoesNothing()
        {
            // Arrange
            _stateMachine.ChangeState<TestState>(_idleState);
            _idleState.EnterCallCount = 0;
            
            // Act
            _stateMachine.ChangeState<TestState>(_idleState);
            
            // Assert
            Assert.AreEqual(0, _idleState.EnterCallCount);
        }

        #endregion

        #region 状态生命周期

        [Test]
        [Category("AI")]
        [Category("StateMachine")]
        public void StateMachine_Update_CallsUpdateOnCurrentState()
        {
            // Arrange
            _stateMachine.ChangeState<TestState>(_idleState);
            
            // Act
            _stateMachine.Update();
            _stateMachine.Update();
            
            // Assert
            Assert.AreEqual(2, _idleState.UpdateCallCount);
        }

        [Test]
        [Category("AI")]
        [Category("StateMachine")]
        public void StateMachine_Update_NoCurrentState_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _stateMachine.Update());
        }

        #endregion

        #region 状态历史

        [Test]
        [Category("AI")]
        [Category("StateMachine")]
        public void StateMachine_StateHistory_TracksTransitions()
        {
            // Act
            _stateMachine.ChangeState<TestState>(_idleState);
            _stateMachine.ChangeState<TestState>(_patrolState);
            _stateMachine.ChangeState<TestState>(_chaseState);
            
            // Assert
            var history = _stateMachine.GetStateHistory();
            Assert.AreEqual(3, history.Count);
            Assert.AreEqual("Idle", history[0].StateName);
            Assert.AreEqual("Patrol", history[1].StateName);
            Assert.AreEqual("Chase", history[2].StateName);
        }

        #endregion

        #region 状态堆栈

        [Test]
        [Category("AI")]
        [Category("StateMachine")]
        public void StateMachine_PushState_AddsToStack()
        {
            // Arrange
            _stateMachine.ChangeState<TestState>(_idleState);
            
            // Act
            _stateMachine.PushState(_patrolState);
            
            // Assert
            Assert.AreEqual(_patrolState, _stateMachine.CurrentState);
            Assert.IsTrue(_idleState.ExitCalled);
        }

        [Test]
        [Category("AI")]
        [Category("StateMachine")]
        public void StateMachine_PopState_ReturnsToPrevious()
        {
            // Arrange
            _stateMachine.ChangeState<TestState>(_idleState);
            _stateMachine.PushState(_patrolState);
            _idleState.EnterCallCount = 0;
            
            // Act
            _stateMachine.PopState();
            
            // Assert
            Assert.AreEqual(_idleState, _stateMachine.CurrentState);
            Assert.AreEqual(1, _idleState.EnterCallCount);
        }

        #endregion

        #region 状态事件

        [Test]
        [Category("AI")]
        [Category("StateMachine")]
        public void StateMachine_OnStateChanged_EventFired()
        {
            // Arrange
            bool eventFired = false;
            IState newState = null;
            _stateMachine.OnStateChanged += (state) => 
            {
                eventFired = true;
                newState = state;
            };
            
            // Act
            _stateMachine.ChangeState<TestState>(_idleState);
            
            // Assert
            Assert.IsTrue(eventFired);
            Assert.AreEqual(_idleState, newState);
        }

        #endregion

        #region 状态优先级

        [Test]
        [Category("AI")]
        [Category("StateMachine")]
        public void StateMachine_Priority_LowerPriorityCannotInterrupt()
        {
            // Arrange
            _idleState.Priority = 10;
            _patrolState.Priority = 5;
            _stateMachine.ChangeState<TestState>(_idleState);
            
            // Act
            bool changed = _stateMachine.TryChangeState(_patrolState);
            
            // Assert
            Assert.IsFalse(changed);
            Assert.AreEqual(_idleState, _stateMachine.CurrentState);
        }

        [Test]
        [Category("AI")]
        [Category("StateMachine")]
        public void StateMachine_Priority_HigherPriorityCanInterrupt()
        {
            // Arrange
            _idleState.Priority = 5;
            _attackState.Priority = 10;
            _stateMachine.ChangeState<TestState>(_idleState);
            
            // Act
            bool changed = _stateMachine.TryChangeState(_attackState);
            
            // Assert
            Assert.IsTrue(changed);
            Assert.AreEqual(_attackState, _stateMachine.CurrentState);
        }

        #endregion

        #region 状态复用

        [Test]
        [Category("AI")]
        [Category("StateMachine")]
        public void StateMachine_ReenterState_CallsEnterAgain()
        {
            // Arrange
            _stateMachine.ChangeState<TestState>(_idleState);
            _stateMachine.ChangeState<TestState>(_patrolState);
            _idleState.EnterCallCount = 0;
            
            // Act
            _stateMachine.ChangeState<TestState>(_idleState);
            
            // Assert
            Assert.AreEqual(1, _idleState.EnterCallCount);
        }

        #endregion

        #region 性能测试

        [Test]
        [Category("AI")]
        [Category("StateMachine")]
        [Category("Performance")]
        public void StateMachine_Performance_100StateMachines()
        {
            // Arrange
            var stateMachines = new List<StateMachine>();
            for (int i = 0; i < 100; i++)
            {
                var sm = new StateMachine();
                sm.AddState(new TestState("Idle"));
                sm.ChangeState<TestState>(new TestState("Idle"));
                stateMachines.Add(sm);
            }
            
            float startTime = Time.realtimeSinceStartup;
            
            // Act
            for (int i = 0; i < 1000; i++)
            {
                foreach (var sm in stateMachines)
                {
                    sm.Update();
                }
            }
            
            float elapsed = Time.realtimeSinceStartup - startTime;
            
            // Assert
            Assert.Less(elapsed, 0.1f, "100 state machines updating 1000 times should take less than 100ms");
        }

        #endregion

        #region 测试辅助类

        private class TestState : IState
        {
            public string StateName { get; }
            public int Priority { get; set; } = 0;
            
            public bool EnterCalled { get; private set; }
            public bool ExitCalled { get; private set; }
            public int EnterCallCount { get; set; }
            public int UpdateCallCount { get; private set; }

            public TestState(string name)
            {
                StateName = name;
            }

            public void Enter()
            {
                EnterCalled = true;
                EnterCallCount++;
            }

            public void Update()
            {
                UpdateCallCount++;
            }

            public void Exit()
            {
                ExitCalled = true;
            }
        }

        #endregion
    }

    #region 状态机接口和实现

    public interface IState
    {
        string StateName { get; }
        int Priority { get; }
        void Enter();
        void Update();
        void Exit();
    }

    public class StateMachine
    {
        private IState _currentState;
        private Dictionary<System.Type, IState> _states = new();
        private Stack<IState> _stateStack = new();
        private List<StateHistoryEntry> _history = new();

        public IState CurrentState => _currentState;
        public event System.Action<IState> OnStateChanged;

        public void AddState(IState state)
        {
            _states[state.GetType()] = state;
        }

        public void ChangeState<T>(T state) where T : IState
        {
            if (_currentState == state) return;

            _currentState?.Exit();
            _currentState = state;
            _currentState.Enter();
            
            _history.Add(new StateHistoryEntry 
            { 
                StateName = state.StateName, 
                Timestamp = Time.time 
            });
            
            OnStateChanged?.Invoke(_currentState);
        }

        public bool TryChangeState(IState newState)
        {
            if (_currentState != null && newState.Priority <= _currentState.Priority)
                return false;

            ChangeState(newState);
            return true;
        }

        public void PushState(IState state)
        {
            if (_currentState != null)
                _stateStack.Push(_currentState);
            
            _currentState?.Exit();
            _currentState = state;
            _currentState.Enter();
            OnStateChanged?.Invoke(_currentState);
        }

        public void PopState()
        {
            if (_stateStack.Count > 0)
            {
                _currentState?.Exit();
                _currentState = _stateStack.Pop();
                _currentState.Enter();
                OnStateChanged?.Invoke(_currentState);
            }
        }

        public void Update()
        {
            _currentState?.Update();
        }

        public bool HasState<T>() where T : IState
        {
            return _states.ContainsKey(typeof(T));
        }

        public List<StateHistoryEntry> GetStateHistory()
        {
            return new List<StateHistoryEntry>(_history);
        }
    }

    public struct StateHistoryEntry
    {
        public string StateName;
        public float Timestamp;
    }

    #endregion
}
