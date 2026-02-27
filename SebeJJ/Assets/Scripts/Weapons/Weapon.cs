using UnityEngine;

namespace SebeJJ.Weapons
{
    /// <summary>
    /// 武器基类
    /// </summary>
    public abstract class Weapon : MonoBehaviour
    {
        [Header("基础设置")]
        [SerializeField] protected string weaponName = "Weapon";
        [SerializeField] protected float damage = 10f;
        [SerializeField] protected float fireRate = 1f;  // 每秒射击次数
        [SerializeField] protected float range = 10f;
        [SerializeField] protected float energyCost = 5f;

        [Header("投射物")]
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected float projectileSpeed = 20f;

        [Header("特效")]
        [SerializeField] protected GameObject muzzleFlashPrefab;
        [SerializeField] protected AudioClip fireSound;
        [SerializeField] protected float recoilForce = 0.5f;

        [Header("发射点")]
        [SerializeField] protected Transform firePoint;

        // 状态
        protected float _lastFireTime;
        protected bool _isReloading;
        protected bool _isEquipped;

        // 属性
        public string WeaponName => weaponName;
        public float Damage => damage;
        public float FireRate => fireRate;
        public float Range => range;
        public float EnergyCost => energyCost;
        public bool IsReady => Time.time >= _lastFireTime + (1f / fireRate);
        public bool IsEquipped => _isEquipped;

        // 事件
        public System.Action OnFire;
        public System.Action OnReloadStart;
        public System.Action OnReloadComplete;

        /// <summary>
        /// 装备武器
        /// </summary>
        public virtual void Equip()
        {
            _isEquipped = true;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 卸下武器
        /// </summary>
        public virtual void Unequip()
        {
            _isEquipped = false;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 尝试开火
        /// </summary>
        public virtual bool TryFire(Vector2 direction)
        {
            if (!_isEquipped || _isReloading) return false;
            if (Time.time < _lastFireTime + (1f / fireRate)) return false;

            if (Fire(direction))
            {
                _lastFireTime = Time.time;
                OnFire?.Invoke();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 实际开火逻辑（子类实现）
        /// </summary>
        protected abstract bool Fire(Vector2 direction);

        /// <summary>
        /// 设置发射点
        /// </summary>
        public void SetFirePoint(Transform point)
        {
            firePoint = point;
        }

        /// <summary>
        /// 获取发射位置
        /// </summary>
        protected Vector2 GetFirePosition()
        {
            return firePoint != null ? firePoint.position : transform.position;
        }

        /// <summary>
        /// 播放枪口特效
        /// </summary>
        protected void PlayMuzzleFlash()
        {
            if (muzzleFlashPrefab != null && firePoint != null)
            {
                Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            }
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        protected void PlayFireSound()
        {
            if (fireSound != null)
            {
                AudioSource.PlayClipAtPoint(fireSound, transform.position);
            }
        }

        /// <summary>
        /// 应用后坐力
        /// </summary>
        protected void ApplyRecoil(Vector2 direction)
        {
            if (recoilForce > 0 && TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.AddForce(-direction * recoilForce, ForceMode2D.Impulse);
            }
        }

        /// <summary>
        /// 更新武器（用于持续型武器如激光）
        /// </summary>
        public virtual void UpdateWeapon(Vector2 aimDirection) { }

        /// <summary>
        /// 停止射击（用于持续型武器）
        /// </summary>
        public virtual void StopFiring() { }
    }
}
