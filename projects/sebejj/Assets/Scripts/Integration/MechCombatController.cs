/**
 * @file MechCombatController.cs
 * @brief 机甲战斗控制器 - 连接MechController与CombatStats
 * @description 整合机甲移动、战斗、武器系统
 * @author 系统集成工程师
 * @date 2026-02-27
 */

using UnityEngine;
using SebeJJ.Combat;
using SebeJJ.Player;
using SebeJJ.Core;

namespace SebeJJ.Integration
{
    /// <summary>
    /// 机甲战斗控制器
    /// 整合机甲的移动控制和战斗系统
    /// </summary>
    [RequireComponent(typeof(MechController))]
    [RequireComponent(typeof(CombatStats))]
    public class MechCombatController : MonoBehaviour
    {
        [Header("组件引用")]
        [SerializeField] private MechController mechController;
        [SerializeField] private CombatStats combatStats;
        [SerializeField] private WeaponManager weaponManager;

        [Header("战斗设置")]
        [SerializeField] private float attackInputBuffer = 0.1f;
        [SerializeField] private bool autoAim = false;
        [SerializeField] private float autoAimRange = 10f;
        [SerializeField] private LayerMask enemyLayers;

        [Header("视觉效果")]
        [SerializeField] private Transform weaponPivot;
        [SerializeField] private ParticleSystem damageEffect;
        [SerializeField] private ParticleSystem shieldEffect;

        // 状态
        private float lastAttackInputTime;
        private bool isAttacking;
        private Transform currentTarget;

        private void Awake()
        {
            // 获取组件
            mechController = GetComponent<MechController>();
            combatStats = GetComponent<CombatStats>();
            weaponManager = GetComponent<WeaponManager>();

            // 如果没有武器管理器，添加一个
            if (weaponManager == null)
            {
                weaponManager = gameObject.AddComponent<WeaponManager>();
            }
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void Update()
        {
            HandleCombatInput();
            UpdateAutoAim();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #region 初始化

        /// <summary>
        /// 订阅事件
        /// </summary>
        private void SubscribeToEvents()
        {
            if (combatStats != null)
            {
                combatStats.OnDamageTaken += HandleDamageTaken;
                combatStats.OnDeath += HandleDeath;
                combatStats.OnShieldBroken += HandleShieldBroken;
            }
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (combatStats != null)
            {
                combatStats.OnDamageTaken -= HandleDamageTaken;
                combatStats.OnDeath -= HandleDeath;
                combatStats.OnShieldBroken -= HandleShieldBroken;
            }
        }

        #endregion

        #region 输入处理

        /// <summary>
        /// 处理战斗输入
        /// </summary>
        private void HandleCombatInput()
        {
            // 攻击输入
            if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.J))
            {
                lastAttackInputTime = Time.time;
                TryAttack();
            }

            // 武器切换
            HandleWeaponSwitch();

            // 瞄准方向
            UpdateWeaponAim();
        }

        /// <summary>
        /// 尝试攻击
        /// </summary>
        private void TryAttack()
        {
            if (weaponManager == null) return;
            if (isAttacking) return;

            Vector2 attackDirection = GetAttackDirection();
            
            if (weaponManager.TryAttack(attackDirection))
            {
                isAttacking = true;
                Invoke(nameof(ResetAttack), 0.1f);
            }
        }

        private void ResetAttack()
        {
            isAttacking = false;
        }

        /// <summary>
        /// 获取攻击方向
        /// </summary>
        private Vector2 GetAttackDirection()
        {
            // 如果有自动瞄准目标，朝向目标
            if (autoAim && currentTarget != null)
            {
                return (currentTarget.position - transform.position).normalized;
            }

            // 使用鼠标方向
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return (mousePos - transform.position).normalized;
        }

        /// <summary>
        /// 更新武器瞄准
        /// </summary>
        private void UpdateWeaponAim()
        {
            if (weaponPivot == null) return;

            Vector2 aimDirection = GetAttackDirection();
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            weaponPivot.rotation = Quaternion.Euler(0, 0, angle);
        }

        /// <summary>
        /// 处理武器切换
        /// </summary>
        private void HandleWeaponSwitch()
        {
            if (weaponManager == null) return;

            // 数字键切换
            if (Input.GetKeyDown(KeyCode.Alpha1))
                weaponManager.SwitchToWeapon(0);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                weaponManager.SwitchToWeapon(1);
            if (Input.GetKeyDown(KeyCode.Alpha3))
                weaponManager.SwitchToWeapon(2);

            // 滚轮切换
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0)
                weaponManager.SwitchToNextWeapon();
            else if (scroll < 0)
                weaponManager.SwitchToPreviousWeapon();
        }

        /// <summary>
        /// 更新自动瞄准
        /// </summary>
        private void UpdateAutoAim()
        {
            if (!autoAim) return;

            // 寻找最近的敌人
            Collider2D[] enemies = Physics2D.OverlapCircleAll(
                transform.position, autoAimRange, enemyLayers);

            float closestDist = float.MaxValue;
            Transform closestEnemy = null;

            foreach (var enemy in enemies)
            {
                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestEnemy = enemy.transform;
                }
            }

            currentTarget = closestEnemy;
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 处理受到伤害
        /// </summary>
        private void HandleDamageTaken(object sender, DamageEventArgs e)
        {
            // 播放受伤特效
            if (damageEffect != null)
            {
                damageEffect.Play();
            }

            // 触发屏幕震动
            if (CombatFeedback.Instance != null)
            {
                float shakeIntensity = Mathf.Clamp01(e.FinalDamage / 50f) * 0.5f;
                CombatFeedback.Instance.TriggerScreenShake(shakeIntensity, 0.2f);
            }
        }

        /// <summary>
        /// 处理死亡
        /// </summary>
        private void HandleDeath(object sender, System.EventArgs e)
        {
            Debug.Log("[MechCombatController] 机甲被摧毁!");

            // 禁用控制
            enabled = false;
            mechController.enabled = false;

            // 播放死亡特效
            if (damageEffect != null)
            {
                damageEffect.Play();
            }

            // 触发游戏结束
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.GameOver);
            }
            
            // 显示游戏结束UI
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowGameOverScreen();
            }
        }

        /// <summary>
        /// 处理护盾破碎
        /// </summary>
        private void HandleShieldBroken(object sender, System.EventArgs e)
        {
            Debug.Log("[MechCombatController] 护盾破碎!");

            // 播放护盾破碎特效
            if (shieldEffect != null)
            {
                shieldEffect.Play();
            }

            // 触发护盾破碎反馈
            if (CombatFeedback.Instance != null)
            {
                CombatFeedback.Instance.TriggerShieldBreakFeedback(transform.position);
            }
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 装备武器
        /// </summary>
        public void EquipWeapon(WeaponData weaponData)
        {
            weaponManager?.AddWeapon(weaponData, true);
        }

        /// <summary>
        /// 获取当前武器
        /// </summary>
        public WeaponBase GetCurrentWeapon()
        {
            return weaponManager?.CurrentWeapon;
        }

        /// <summary>
        /// 设置自动瞄准
        /// </summary>
        public void SetAutoAim(bool enabled)
        {
            autoAim = enabled;
        }

        /// <summary>
        /// 获取战斗状态
        /// </summary>
        public CombatStats GetCombatStats()
        {
            return combatStats;
        }

        #endregion
    }
}
