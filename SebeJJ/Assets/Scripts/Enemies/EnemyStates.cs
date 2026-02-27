using UnityEngine;
using System.Collections;

namespace SebeJJ.Enemies
{
    /// <summary>
    /// 巡逻状态
    /// </summary>
    public class PatrolState : StateBase
    {
        private Vector2 _currentPatrolPoint;
        private float _waitTimer;
        private bool _isWaiting;

        public PatrolState(EnemyBase enemy) : base(enemy) { }

        public override void Enter()
        {
            _isWaiting = false;
            SetNewPatrolPoint();
        }

        public override void Update()
        {
            // 检查是否发现玩家
            if (_enemy.Target != null)
            {
                _enemy.StateMachine.ChangeState<ChaseState>();
                return;
            }

            if (_isWaiting)
            {
                _waitTimer -= Time.deltaTime;
                if (_waitTimer <= 0)
                {
                    _isWaiting = false;
                    SetNewPatrolPoint();
                }
            }
        }

        public override void FixedUpdate()
        {
            if (_isWaiting)
            {
                _enemy.StopMoving();
                return;
            }

            // 移动到巡逻点
            float distanceToPoint = Vector2.Distance(_enemy.transform.position, _currentPatrolPoint);
            
            if (distanceToPoint < 0.5f)
            {
                // 到达巡逻点，开始等待
                _isWaiting = true;
                _waitTimer = _enemy is DeepJellyfish ? 1f : 2f; // 水母等待时间更短
            }
            else
            {
                _enemy.MoveTo(_currentPatrolPoint);
            }
        }

        private void SetNewPatrolPoint()
        {
            // 在巡逻范围内随机选择一个点
            Vector2 randomOffset = Random.insideUnitCircle * 5f; // 默认巡逻半径
            
            // 如果是特定敌人类型，使用其巡逻半径
            if (_enemy is DeepJellyfish jellyfish)
            {
                randomOffset = Random.insideUnitCircle * jellyfish.PatrolRadius;
            }
            else if (_enemy is SecurityDrone drone)
            {
                randomOffset = Random.insideUnitCircle * drone.PatrolRadius;
            }
            
            _currentPatrolPoint = _enemy.SpawnPosition + randomOffset;
        }
    }

    /// <summary>
    /// 追击状态
    /// </summary>
    public class ChaseState : StateBase
    {
        public ChaseState(EnemyBase enemy) : base(enemy) { }

        public override void Enter()
        {
            // 可以在这里播放发现玩家的音效或动画
        }

        public override void Update()
        {
            // 检查目标是否丢失
            if (_enemy.Target == null)
            {
                _enemy.StateMachine.ChangeState<PatrolState>();
                return;
            }

            // 检查是否进入攻击范围
            float distanceToTarget = Vector2.Distance(_enemy.transform.position, _enemy.Target.position);
            
            if (distanceToTarget <= _enemy.AttackRange)
            {
                _enemy.StateMachine.ChangeState<AttackState>();
                return;
            }
        }

        public override void FixedUpdate()
        {
            if (_enemy.Target != null)
            {
                // 追击玩家
                _enemy.MoveTo(_enemy.Target.position, 1.5f);
            }
        }

        public override void Exit()
        {
            // 清理
        }
    }

    /// <summary>
    /// 攻击状态
    /// </summary>
    public class AttackState : StateBase
    {
        private float _attackTimer;

        public AttackState(EnemyBase enemy) : base(enemy) { }

        public override void Enter()
        {
            _attackTimer = 0f;
        }

        public override void Update()
        {
            // 检查目标是否丢失
            if (_enemy.Target == null)
            {
                _enemy.StateMachine.ChangeState<PatrolState>();
                return;
            }

            // 检查目标是否离开攻击范围
            float distanceToTarget = Vector2.Distance(_enemy.transform.position, _enemy.Target.position);
            
            if (distanceToTarget > _enemy.AttackRange * 1.5f)
            {
                _enemy.StateMachine.ChangeState<ChaseState>();
                return;
            }

            // 尝试攻击
            _enemy.TryAttack();
        }

        public override void FixedUpdate()
        {
            // 攻击时可能停止移动或缓慢移动
            if (_enemy.Target != null)
            {
                // 面向目标
                Vector2 direction = (_enemy.Target.position - _enemy.transform.position).normalized;
                float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                
                // 缓慢接近或保持距离
                float distanceToTarget = Vector2.Distance(_enemy.transform.position, _enemy.Target.position);
                
                if (distanceToTarget > _enemy.AttackRange * 0.8f)
                {
                    _enemy.MoveTo(_enemy.Target.position, 0.5f);
                }
                else
                {
                    _enemy.StopMoving();
                }
            }
        }
    }

    /// <summary>
    /// 待机状态
    /// </summary>
    public class IdleState : StateBase
    {
        private float _idleTimer;
        private float _idleDuration;

        public IdleState(EnemyBase enemy) : base(enemy) { }

        public override void Enter()
        {
            _idleDuration = Random.Range(1f, 3f);
            _idleTimer = 0f;
        }

        public override void Update()
        {
            _idleTimer += Time.deltaTime;

            // 检查是否发现玩家
            if (_enemy.Target != null)
            {
                _enemy.StateMachine.ChangeState<ChaseState>();
                return;
            }

            // 待机结束，进入巡逻
            if (_idleTimer >= _idleDuration)
            {
                _enemy.StateMachine.ChangeState<PatrolState>();
            }
        }

        public override void FixedUpdate()
        {
            _enemy.StopMoving();
        }
    }

    /// <summary>
    /// 逃跑状态
    /// </summary>
    public class FleeState : StateBase
    {
        private float _fleeDuration = 3f;
        private float _fleeTimer;

        public FleeState(EnemyBase enemy) : base(enemy) { }

        public override void Enter()
        {
            _fleeTimer = 0f;
        }

        public override void Update()
        {
            _fleeTimer += Time.deltaTime;

            if (_fleeTimer >= _fleeDuration)
            {
                _enemy.StateMachine.ChangeState<PatrolState>();
            }
        }

        public override void FixedUpdate()
        {
            if (_enemy.Target != null)
            {
                // 远离目标
                Vector2 fleeDirection = ((Vector2)_enemy.transform.position - (Vector2)_enemy.Target.position).normalized;
                Vector2 fleePosition = (Vector2)_enemy.transform.position + fleeDirection * 5f;
                _enemy.MoveTo(fleePosition, 1.5f);
            }
        }
    }
}
