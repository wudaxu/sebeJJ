/** 
 * @file AIUnitTests.cs
 * @brief AI单元测试 - 提供测试用例
 * @description 测试AI系统的各个组件
 * @author AI系统架构师
 * @date 2026-02-27
 */

using UnityEngine;
using UnityEngine.Assertions;
using NUnit.Framework;
using SebeJJ.AI;
using SebeJJ.AI.Pathfinding;
using SebeJJ.Enemies;

namespace SebeJJ.AI.Test
{
    /// <summary>
    /// AI系统单元测试
    /// </summary>
    public class AIUnitTests
    {
        #region 状态机测试
        
        /// <summary>
        /// 测试状态机初始化
        /// </summary>
        [Test]
        public void StateMachine_Initialization()
        {
            // 创建测试对象
            GameObject testObj = new GameObject("TestAI");
            AIStateMachine stateMachine = testObj.AddComponent<AIStateMachine>();
            
            // 验证初始状态
            Assert.AreEqual(EnemyState.Idle, stateMachine.CurrentState);
            Assert.IsTrue(stateMachine.IsInitialized);
            
            // 清理
            Object.DestroyImmediate(testObj);
        }
        
        /// <summary>
        /// 测试状态注册
        /// </summary>
        [Test]
        public void StateMachine_StateRegistration()
        {
            GameObject testObj = new GameObject("TestAI");
            AIStateMachine stateMachine = testObj.AddComponent<AIStateMachine>();
            
            // 注册测试状态
            TestState testState = new TestState();
            bool registered = stateMachine.RegisterState(EnemyState.Idle, testState);
            
            Assert.IsTrue(registered);
            Assert.IsTrue(stateMachine.HasState(EnemyState.Idle));
            
            // 清理
            Object.DestroyImmediate(testObj);
        }
        
        /// <summary>
        /// 测试状态切换
        /// </summary>
        [Test]
        public void StateMachine_StateTransition()
        {
            GameObject testObj = new GameObject("TestAI");
            AIStateMachine stateMachine = testObj.AddComponent<AIStateMachine>();
            
            // 注册两个状态
            TestState idleState = new TestState();
            TestState chaseState = new TestState();
            
            stateMachine.RegisterState(EnemyState.Idle, idleState);
            stateMachine.RegisterState(EnemyState.Chase, chaseState);
            
            // 切换到追击状态
            bool changed = stateMachine.ChangeState(EnemyState.Chase);
            
            Assert.IsTrue(changed);
            Assert.AreEqual(EnemyState.Chase, stateMachine.CurrentState);
            Assert.AreEqual(EnemyState.Idle, stateMachine.PreviousState);
            
            // 清理
            Object.DestroyImmediate(testObj);
        }
        
        /// <summary>
        /// 测试状态转换条件
        /// </summary>
        [Test]
        public void StateMachine_TransitionCondition()
        {
            GameObject testObj = new GameObject("TestAI");
            AIStateMachine stateMachine = testObj.AddComponent<AIStateMachine>();
            
            // 注册状态
            TestState idleState = new TestState();
            TestState deadState = new TestState();
            
            stateMachine.RegisterState(EnemyState.Idle, idleState);
            stateMachine.RegisterState(EnemyState.Dead, deadState);
            
            // 注册转换条件：只有生命值低于0才能进入死亡状态
            bool canDie = false;
            stateMachine.RegisterTransitionCondition((from, to) => canDie, EnemyState.Dead, EnemyState.Idle);
            
            // 尝试切换到死亡状态（应该失败）
            bool changed = stateMachine.ChangeState(EnemyState.Dead);
            Assert.IsFalse(changed);
            
            // 设置条件为true
            canDie = true;
            changed = stateMachine.ChangeState(EnemyState.Dead);
            Assert.IsTrue(changed);
            
            // 清理
            Object.DestroyImmediate(testObj);
        }
        
        #endregion

        #region 感知系统测试
        
        /// <summary>
        /// 测试感知系统初始化
        /// </summary>
        [Test]
        public void Perception_Initialization()
        {
            GameObject testObj = new GameObject("TestAI");
            AIPerception perception = testObj.AddComponent<AIPerception>();
            
            // 验证初始状态
            Assert.IsFalse(perception.HasTarget);
            Assert.AreEqual(0, perception.DetectedTargetCount);
            
            // 清理
            Object.DestroyImmediate(testObj);
        }
        
        /// <summary>
        /// 测试目标检测
        /// </summary>
        [Test]
        public void Perception_TargetDetection()
        {
            // 创建AI
            GameObject aiObj = new GameObject("TestAI");
            AIPerception perception = aiObj.AddComponent<AIPerception>();
            
            // 创建目标
            GameObject targetObj = new GameObject("Target");
            targetObj.tag = "Player";
            targetObj.transform.position = aiObj.transform.position + Vector3.right * 5f;
            
            // 强制设置目标
            perception.ForceSetTarget(targetObj.transform);
            
            // 验证目标已设置
            Assert.IsTrue(perception.HasTarget);
            Assert.IsNotNull(perception.PrimaryTarget);
            
            // 清理
            Object.DestroyImmediate(aiObj);
            Object.DestroyImmediate(targetObj);
        }
        
        #endregion

        #region 寻路系统测试
        
        /// <summary>
        /// 测试寻路系统初始化
        /// </summary>
        [Test]
        public void Pathfinding_Initialization()
        {
            GameObject testObj = new GameObject("Pathfinder");
            AStarPathfinding pathfinding = testObj.AddComponent<AStarPathfinding>();
            
            // 验证网格已初始化
            Assert.Greater(pathfinding.GridSizeX, 0);
            Assert.Greater(pathfinding.GridSizeY, 0);
            
            // 清理
            Object.DestroyImmediate(testObj);
        }
        
        /// <summary>
        /// 测试路径计算
        /// </summary>
        [Test]
        public void Pathfinding_PathCalculation()
        {
            GameObject testObj = new GameObject("Pathfinder");
            AStarPathfinding pathfinding = testObj.AddComponent<AStarPathfinding>();
            
            // 计算路径
            Vector3 startPos = Vector3.zero;
            Vector3 endPos = Vector3.right * 5f;
            
            var (path, success) = pathfinding.FindPathImmediate(startPos, endPos);
            
            // 验证路径
            Assert.IsTrue(success);
            Assert.IsNotNull(path);
            Assert.Greater(path.Length, 0);
            
            // 验证路径起点和终点
            Assert.AreEqual(startPos, path[0]);
            
            // 清理
            Object.DestroyImmediate(testObj);
        }
        
        /// <summary>
        /// 测试路径缓存
        /// </summary>
        [Test]
        public void Pathfinding_PathCache()
        {
            GameObject testObj = new GameObject("Pathfinder");
            AStarPathfinding pathfinding = testObj.AddComponent<AStarPathfinding>();
            
            Vector3 startPos = Vector3.zero;
            Vector3 endPos = Vector3.right * 5f;
            
            // 第一次计算
            var (path1, success1) = pathfinding.FindPathImmediate(startPos, endPos);
            
            // 第二次计算（应该从缓存获取）
            var (path2, success2) = pathfinding.FindPathImmediate(startPos, endPos);
            
            Assert.IsTrue(success1);
            Assert.IsTrue(success2);
            Assert.AreEqual(path1, path2); // 应该返回相同的缓存路径
            
            // 清理
            Object.DestroyImmediate(testObj);
        }
        
        #endregion

        #region 敌人基类测试
        
        /// <summary>
        /// 测试敌人初始化
        /// </summary>
        [Test]
        public void EnemyBase_Initialization()
        {
            GameObject testObj = new GameObject("TestEnemy");
            TestEnemy enemy = testObj.AddComponent<TestEnemy>();
            
            // 验证组件存在
            Assert.IsNotNull(enemy.GetComponent<AIStateMachine>());
            Assert.IsNotNull(enemy.GetComponent<AIPerception>());
            
            // 验证生命值
            Assert.AreEqual(enemy.MaxHealth, enemy.CurrentHealth);
            Assert.IsFalse(enemy.IsDead);
            
            // 清理
            Object.DestroyImmediate(testObj);
        }
        
        /// <summary>
        /// 测试受到伤害
        /// </summary>
        [Test]
        public void EnemyBase_TakeDamage()
        {
            GameObject testObj = new GameObject("TestEnemy");
            TestEnemy enemy = testObj.AddComponent<TestEnemy>();
            
            float initialHealth = enemy.CurrentHealth;
            float damage = 20f;
            
            enemy.TakeDamage(damage);
            
            Assert.AreEqual(initialHealth - damage, enemy.CurrentHealth);
            
            // 清理
            Object.DestroyImmediate(testObj);
        }
        
        /// <summary>
        /// 测试死亡
        /// </summary>
        [Test]
        public void EnemyBase_Death()
        {
            GameObject testObj = new GameObject("TestEnemy");
            TestEnemy enemy = testObj.AddComponent<TestEnemy>();
            
            // 造成致命伤害
            enemy.TakeDamage(enemy.MaxHealth + 10f);
            
            Assert.IsTrue(enemy.IsDead);
            Assert.AreEqual(0, enemy.CurrentHealth);
            
            // 清理
            Object.DestroyImmediate(testObj);
        }
        
        /// <summary>
        /// 测试治疗
        /// </summary>
        [Test]
        public void EnemyBase_Heal()
        {
            GameObject testObj = new GameObject("TestEnemy");
            TestEnemy enemy = testObj.AddComponent<TestEnemy>();
            
            // 先造成伤害
            enemy.TakeDamage(50f);
            float healthAfterDamage = enemy.CurrentHealth;
            
            // 治疗
            enemy.Heal(20f);
            
            Assert.AreEqual(healthAfterDamage + 20f, enemy.CurrentHealth);
            
            // 测试过量治疗
            enemy.Heal(999f);
            Assert.AreEqual(enemy.MaxHealth, enemy.CurrentHealth);
            
            // 清理
            Object.DestroyImmediate(testObj);
        }
        
        #endregion

        #region 具体敌人测试
        
        /// <summary>
        /// 测试机械鱼冲撞
        /// </summary>
        [Test]
        public void MechFish_ChargeAttack()
        {
            GameObject testObj = new GameObject("MechFish");
            MechFishAI fish = testObj.AddComponent<MechFishAI>();
            
            // 验证初始状态
            Assert.IsFalse(fish.CanCharge); // 刚创建时应该不能冲撞
            
            // 清理
            Object.DestroyImmediate(testObj);
        }
        
        /// <summary>
        /// 测试机械蟹防御
        /// </summary>
        [Test]
        public void MechCrab_Defend()
        {
            GameObject testObj = new GameObject("MechCrab");
            MechCrabAI crab = testObj.AddComponent<MechCrabAI>();
            
            // 验证初始状态
            Assert.IsFalse(crab.IsDefending);
            
            // 清理
            Object.DestroyImmediate(testObj);
        }
        
        /// <summary>
        /// 测试机械水母电击
        /// </summary>
        [Test]
        public void MechJellyfish_PulseAttack()
        {
            GameObject testObj = new GameObject("MechJellyfish");
            MechJellyfishAI jellyfish = testObj.AddComponent<MechJellyfishAI>();
            
            // 验证初始状态
            Assert.IsFalse(jellyfish.IsCharging);
            
            // 清理
            Object.DestroyImmediate(testObj);
        }
        
        #endregion
    }

    #region 测试辅助类

    /// <summary>
    /// 测试状态
    /// </summary>
    public class TestState : AIStateBase
    {
        public bool EnterCalled { get; private set; }
        public bool ExitCalled { get; private set; }
        public int UpdateCount { get; private set; }
        
        public override void OnEnter()
        {
            EnterCalled = true;
        }
        
        public override void OnUpdate(float deltaTime)
        {
            UpdateCount++;
        }
        
        public override void OnExit()
        {
            ExitCalled = true;
        }
    }

    /// <summary>
    /// 测试敌人
    /// </summary>
    public class TestEnemy : EnemyBase
    {
        protected override void InitializeStates()
        {
            // 测试用，不注册实际状态
        }
        
        public override void PerformAttack()
        {
            // 测试用，不执行实际攻击
        }
    }

    #endregion
}
