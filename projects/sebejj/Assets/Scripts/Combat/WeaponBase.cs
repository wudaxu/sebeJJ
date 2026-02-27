using UnityEngine;
using System;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 武器基类 - WP-001
    /// </summary>
    public abstract class WeaponBase : MonoBehaviour
    {
        [Header("武器数据")]
        [SerializeField] protected WeaponData weaponData;
        
        [Header("武器状态")]
        [SerializeField] protected int currentLevel = 1;
        [SerializeField] protected bool isUnlocked = false;
        
        // 运行时状态
        protected float lastAttackTime;
        protected bool isAttacking = false;
        protected Transform owner;
        protected Transform firePoint;
        
        // 事件
        public event Action OnAttackStart;
        public event Action OnAttackEnd;
        public event Action OnWeaponEquipped;
        public event Action OnWeaponUnequipped;
        public event Action<int> OnWeaponUpgraded; // 参数: 新等级

        // 属性
        public WeaponData WeaponData => weaponData;
        public int CurrentLevel => currentLevel;
        public bool IsUnlocked => isUnlocked;
        public bool IsAttacking => isAttacking;
        public bool CanAttack => Time.time >= lastAttackTime + GetCurrentCooldown();
        public float CooldownProgress => Mathf.Clamp01((Time.time - lastAttackTime) / GetCurrentCooldown());

        /// <summary>
        /// 初始化武器
        /// </summary>
        public virtual void Initialize(Transform weaponOwner, Transform weaponFirePoint = null)
        {
            owner = weaponOwner;
            firePoint = weaponFirePoint ?? transform;
        }

        /// <summary>
        /// 装备武器
        /// </summary>
        public virtual void Equip()
        {
            gameObject.SetActive(true);
            OnWeaponEquipped?.Invoke();
        }

        /// <summary>
        /// 卸下武器
        /// </summary>
        public virtual void Unequip()
        {
            gameObject.SetActive(false);
            OnWeaponUnequipped?.Invoke();
        }

        /// <summary>
        /// 尝试攻击
        /// </summary>
        public virtual bool TryAttack(Vector2 direction)
        {
            if (!CanAttack || isAttacking) return false;
            
            PerformAttack(direction);
            return true;
        }

        /// <summary>
        /// 执行攻击(子类实现)
        /// </summary>
        protected abstract void PerformAttack(Vector2 direction);

        /// <summary>
        /// 升级武器 - WP-005
        /// </summary>
        public virtual bool Upgrade()
        {
            if (!weaponData.canUpgrade) return false;
            if (currentLevel >= weaponData.maxLevel) return false;
            if (!isUnlocked) return false;

            currentLevel++;
            OnWeaponUpgraded?.Invoke(currentLevel);
            return true;
        }

        /// <summary>
        /// 解锁武器
        /// </summary>
        public virtual void Unlock()
        {
            isUnlocked = true;
        }

        /// <summary>
        /// 锁定武器
        /// </summary>
        public virtual void Lock()
        {
            isUnlocked = false;
        }

        /// <summary>
        /// 设置等级
        /// </summary>
        public virtual void SetLevel(int level)
        {
            currentLevel = Mathf.Clamp(level, 1, weaponData.maxLevel);
        }

        /// <summary>
        /// 获取当前伤害
        /// </summary>
        public virtual float GetCurrentDamage()
        {
            return weaponData.GetDamageAtLevel(currentLevel);
        }

        /// <summary>
        /// 获取当前范围
        /// </summary>
        public virtual float GetCurrentRange()
        {
            return weaponData.GetRangeAtLevel(currentLevel);
        }

        /// <summary>
        /// 获取当前冷却
        /// </summary>
        public virtual float GetCurrentCooldown()
        {
            return weaponData.GetCooldownAtLevel(currentLevel);
        }

        /// <summary>
        /// 创建伤害信息
        /// </summary>
        protected virtual DamageInfo CreateDamageInfo(Vector2 hitPosition, Vector2 hitDirection)
        {
            var damage = DamageCalculator.CreateDamageWithCritical(
                GetCurrentDamage(), 
                weaponData.damageType,
                weaponData.criticalChance,
                weaponData.criticalMultiplier);

            damage.Attacker = owner?.gameObject;
            damage.HitPosition = hitPosition;
            damage.HitDirection = hitDirection;
            damage.KnockbackForce = weaponData.knockbackForce;

            return damage;
        }

        /// <summary>
        /// 播放攻击特效 - WP-006
        /// PF-002修复: 添加特效预制体缺失检查
        /// </summary>
        protected virtual void SpawnAttackEffect(Vector2 position, Vector2 direction)
        {
            if (weaponData == null) return;
            
            if (weaponData.attackEffectPrefab != null)
            {
                var effect = Instantiate(weaponData.attackEffectPrefab, position, Quaternion.identity);
                effect.transform.up = direction;
                Destroy(effect, 1f);
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning($"[{GetType().Name}] 未设置攻击特效预制体");
            }
            #endif
        }

        /// <summary>
        /// 播放命中特效
        /// PF-002修复: 添加特效预制体缺失检查
        /// </summary>
        protected virtual void SpawnHitEffect(Vector2 position, Vector2 direction)
        {
            if (weaponData == null) return;
            
            if (weaponData.hitEffectPrefab != null)
            {
                var effect = Instantiate(weaponData.hitEffectPrefab, position, Quaternion.identity);
                effect.transform.up = -direction;
                Destroy(effect, 1f);
            }
            #if UNITY_EDITOR
            else
            {
                Debug.LogWarning($"[{GetType().Name}] 未设置命中特效预制体");
            }
            #endif
        }

        /// <summary>
        /// 播放攻击音效 - WP-007
        /// </summary>
        protected virtual void PlayAttackSound()
        {
            if (weaponData.attackSound != null)
            {
                AudioSource.PlayClipAtPoint(weaponData.attackSound, transform.position);
            }
        }

        /// <summary>
        /// 播放命中音效
        /// </summary>
        protected virtual void PlayHitSound(Vector2 position)
        {
            if (weaponData.hitSound != null)
            {
                AudioSource.PlayClipAtPoint(weaponData.hitSound, position);
            }
        }

        /// <summary>
        /// 攻击开始
        /// </summary>
        protected virtual void OnAttackStarted()
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            OnAttackStart?.Invoke();
            PlayAttackSound();
        }

        /// <summary>
        /// 攻击结束
        /// </summary>
        protected virtual void OnAttackEnded()
        {
            isAttacking = false;
            OnAttackEnd?.Invoke();
        }
    }
}