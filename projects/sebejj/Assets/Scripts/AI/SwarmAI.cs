using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SebeJJ.AI
{
    /// <summary>
    /// 群体AI系统 - BUG-010修复
    /// 管理多个敌人的协同行为
    /// </summary>
    public class SwarmAI : MonoBehaviour
    {
        public static SwarmAI Instance { get; private set; }
        
        [Header("群体设置")]
        [SerializeField] private float communicationRange = 30f;
        [SerializeField] private float coordinatedAttackCooldown = 5f;
        [SerializeField] private int maxCoordinatedAttackers = 3;
        [SerializeField] private float attackStaggerDelay = 0.3f;
        
        [Header("阵型设置")]
        [SerializeField] private bool enableFormation = true;
        [SerializeField] private float formationSpacing = 2f;
        
        // 群体成员
        private List<SwarmMember> swarmMembers = new List<SwarmMember>();
        
        // 协同攻击计时
        private float lastCoordinatedAttackTime;
        private bool isCoordinatedAttackInProgress;
        
        // 事件
        public event Action<List<Enemies.EnemyBase>> OnCoordinatedAttack;
        public event Action OnSwarmAlerted;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        /// <summary>
        /// 注册群体成员
        /// </summary>
        public void RegisterMember(Enemies.EnemyBase enemy)
        {
            if (enemy == null) return;
            
            var member = new SwarmMember
            {
                enemy = enemy,
                lastAttackTime = -999f,
                isInFormation = false,
                formationPosition = Vector3.zero
            };
            
            swarmMembers.Add(member);
            
            // 订阅敌人事件
            enemy.OnTakeDamage += (damage) => OnMemberDamaged(enemy, damage);
            enemy.OnDeath += () => UnregisterMember(enemy);
        }
        
        /// <summary>
        /// 注销群体成员
        /// </summary>
        public void UnregisterMember(Enemies.EnemyBase enemy)
        {
            var member = swarmMembers.Find(m => m.enemy == enemy);
            if (member != null)
            {
                swarmMembers.Remove(member);
            }
        }
        
        /// <summary>
        /// 成员受到伤害回调
        /// </summary>
        private void OnMemberDamaged(Enemies.EnemyBase enemy, float damage)
        {
            // 通知附近的同伴
            AlertNearbyMembers(enemy.transform.position);
        }
        
        /// <summary>
        /// 通知附近成员
        /// </summary>
        private void AlertNearbyMembers(Vector3 alertPosition)
        {
            foreach (var member in swarmMembers)
            {
                if (member.enemy == null) continue;
                
                float distance = Vector3.Distance(member.enemy.transform.position, alertPosition);
                if (distance <= communicationRange)
                {
                    // 唤醒敌人
                    var stateMachine = member.enemy.GetComponent<AIStateMachine>();
                    if (stateMachine != null && stateMachine.CurrentState == EnemyState.Idle)
                    {
                        stateMachine.ChangeState(EnemyState.Chase);
                    }
                }
            }
            
            OnSwarmAlerted?.Invoke();
        }
        
        /// <summary>
        /// 请求协同攻击
        /// </summary>
        public bool RequestCoordinatedAttack(Transform target)
        {
            if (isCoordinatedAttackInProgress) return false;
            if (Time.time - lastCoordinatedAttackTime < coordinatedAttackCooldown) return false;
            
            // 选择攻击者
            var attackers = SelectCoordinatedAttackers(target);
            if (attackers.Count < 2) return false; // 至少需要2个攻击者
            
            // 执行协同攻击
            StartCoroutine(ExecuteCoordinatedAttack(attackers));
            
            return true;
        }
        
        /// <summary>
        /// 选择协同攻击者
        /// </summary>
        private List<Enemies.EnemyBase> SelectCoordinatedAttackers(Transform target)
        {
            var candidates = swarmMembers
                .Where(m => m.enemy != null && !m.enemy.IsDead)
                .Where(m => m.enemy.CanAttack)
                .Select(m => new
                {
                    member = m,
                    distance = Vector3.Distance(m.enemy.transform.position, target.position)
                })
                .OrderBy(x => x.distance)
                .Take(maxCoordinatedAttackers)
                .Select(x => x.member.enemy)
                .ToList();
            
            return candidates;
        }
        
        /// <summary>
        /// 执行协同攻击
        /// </summary>
        private System.Collections.IEnumerator ExecuteCoordinatedAttack(List<Enemies.EnemyBase> attackers)
        {
            isCoordinatedAttackInProgress = true;
            lastCoordinatedAttackTime = Time.time;
            
            OnCoordinatedAttack?.Invoke(attackers);
            
            Debug.Log($"[SwarmAI] 开始协同攻击，{attackers.Count} 个敌人参与");
            
            // 错开攻击时间
            for (int i = 0; i < attackers.Count; i++)
            {
                if (attackers[i] != null && !attackers[i].IsDead)
                {
                    yield return new WaitForSeconds(attackStaggerDelay);
                    
                    if (attackers[i] != null && !attackers[i].IsDead)
                    {
                        attackers[i].PerformAttack();
                    }
                }
            }
            
            isCoordinatedAttackInProgress = false;
        }
        
        /// <summary>
        /// 获取阵型位置
        /// </summary>
        public Vector3 GetFormationPosition(Enemies.EnemyBase enemy, Transform target)
        {
            if (!enableFormation || target == null) return enemy.transform.position;
            
            int index = swarmMembers.FindIndex(m => m.enemy == enemy);
            if (index < 0) return enemy.transform.position;
            
            // 计算阵型位置（扇形分布）
            float angle = (index % 6) * 60f; // 6个位置一个循环
            float radius = formationSpacing * (1 + index / 6);
            
            Vector3 offset = Quaternion.Euler(0, 0, angle) * Vector3.right * radius;
            return target.position + offset;
        }
        
        /// <summary>
        /// 获取最近的同伴
        /// </summary>
        public Enemies.EnemyBase GetNearestAlly(Enemies.EnemyBase enemy)
        {
            Enemies.EnemyBase nearest = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var member in swarmMembers)
            {
                if (member.enemy == null || member.enemy == enemy) continue;
                if (member.enemy.IsDead) continue;
                
                float distance = Vector3.Distance(enemy.transform.position, member.enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = member.enemy;
                }
            }
            
            return nearest;
        }
        
        /// <summary>
        /// 获取群体中心位置
        /// </summary>
        public Vector3 GetSwarmCenter()
        {
            if (swarmMembers.Count == 0) return Vector3.zero;
            
            Vector3 center = Vector3.zero;
            int count = 0;
            
            foreach (var member in swarmMembers)
            {
                if (member.enemy != null && !member.enemy.IsDead)
                {
                    center += member.enemy.transform.position;
                    count++;
                }
            }
            
            return count > 0 ? center / count : Vector3.zero;
        }
        
        /// <summary>
        /// 获取活跃成员数量
        /// </summary>
        public int GetActiveMemberCount()
        {
            return swarmMembers.Count(m => m.enemy != null && !m.enemy.IsDead);
        }
        
        /// <summary>
        /// 广播目标位置
        /// </summary>
        public void BroadcastTargetPosition(Vector3 position)
        {
            foreach (var member in swarmMembers)
            {
                if (member.enemy != null)
                {
                    var perception = member.enemy.GetComponent<AIPerception>();
                    perception?.ForceSetTargetAtPosition(position);
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            // 绘制通信范围
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, communicationRange);
            
            // 绘制成员连线
            Gizmos.color = Color.blue;
            for (int i = 0; i < swarmMembers.Count; i++)
            {
                for (int j = i + 1; j < swarmMembers.Count; j++)
                {
                    if (swarmMembers[i].enemy != null && swarmMembers[j].enemy != null)
                    {
                        Gizmos.DrawLine(
                            swarmMembers[i].enemy.transform.position,
                            swarmMembers[j].enemy.transform.position
                        );
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 群体成员数据
    /// </summary>
    public class SwarmMember
    {
        public Enemies.EnemyBase enemy;
        public float lastAttackTime;
        public bool isInFormation;
        public Vector3 formationPosition;
    }
}
