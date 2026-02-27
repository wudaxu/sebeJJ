using UnityEngine;
using System.Collections.Generic;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 武器管理器 - WP-004 武器切换系统
    /// </summary>
    public class WeaponManager : MonoBehaviour
    {
        [Header("武器槽位")]
        [SerializeField] private List<WeaponSlot> weaponSlots = new List<WeaponSlot>();
        [SerializeField] private int maxWeaponSlots = 3;

        [Header("武器挂载点")]
        [SerializeField] private Transform weaponParent;
        [SerializeField] private Transform firePoint;

        [Header("输入设置")]
        [SerializeField] private KeyCode[] weaponHotkeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };

        // 当前状态
        private int currentWeaponIndex = -1;
        private WeaponBase currentWeapon;
        private bool canSwitchWeapon = true;
        private float weaponSwitchCooldown = 0.2f;
        private float lastSwitchTime;

        // 事件
        public System.Action<WeaponBase, int> OnWeaponChanged;
        public System.Action<int> OnWeaponUnlocked;
        public System.Action OnWeaponSwitchFailed;

        // 属性
        public WeaponBase CurrentWeapon => currentWeapon;
        public int CurrentWeaponIndex => currentWeaponIndex;
        public int WeaponCount => weaponSlots.Count;
        public bool CanSwitchWeapon => canSwitchWeapon && Time.time >= lastSwitchTime + weaponSwitchCooldown;

        private void Awake()
        {
            if (weaponParent == null) weaponParent = transform;
        }

        private void Update()
        {
            HandleWeaponSwitchInput();
        }

        /// <summary>
        /// 处理武器切换输入
        /// </summary>
        private void HandleWeaponSwitchInput()
        {
            for (int i = 0; i < weaponHotkeys.Length && i < weaponSlots.Count; i++)
            {
                if (Input.GetKeyDown(weaponHotkeys[i]))
                {
                    SwitchToWeapon(i);
                }
            }

            // 滚轮切换
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0)
            {
                SwitchToNextWeapon();
            }
            else if (scroll < 0)
            {
                SwitchToPreviousWeapon();
            }
        }

        /// <summary>
        /// 切换到指定武器
        /// </summary>
        public bool SwitchToWeapon(int index)
        {
            if (!CanSwitchWeapon) return false;
            if (index < 0 || index >= weaponSlots.Count) return false;
            if (index == currentWeaponIndex) return true;

            var slot = weaponSlots[index];
            if (slot.Weapon == null || !slot.Weapon.IsUnlocked)
            {
                OnWeaponSwitchFailed?.Invoke();
                return false;
            }

            // 卸下当前武器
            if (currentWeapon != null)
            {
                currentWeapon.Unequip();
            }

            // 装备新武器
            currentWeaponIndex = index;
            currentWeapon = slot.Weapon;
            currentWeapon.Equip();

            lastSwitchTime = Time.time;
            OnWeaponChanged?.Invoke(currentWeapon, index);

            return true;
        }

        /// <summary>
        /// 切换到下一个武器
        /// </summary>
        public bool SwitchToNextWeapon()
        {
            for (int i = 1; i <= weaponSlots.Count; i++)
            {
                int nextIndex = (currentWeaponIndex + i) % weaponSlots.Count;
                if (weaponSlots[nextIndex].Weapon != null && weaponSlots[nextIndex].Weapon.IsUnlocked)
                {
                    return SwitchToWeapon(nextIndex);
                }
            }
            return false;
        }

        /// <summary>
        /// 切换到上一个武器
        /// </summary>
        public bool SwitchToPreviousWeapon()
        {
            for (int i = 1; i <= weaponSlots.Count; i++)
            {
                int prevIndex = (currentWeaponIndex - i + weaponSlots.Count) % weaponSlots.Count;
                if (weaponSlots[prevIndex].Weapon != null && weaponSlots[prevIndex].Weapon.IsUnlocked)
                {
                    return SwitchToWeapon(prevIndex);
                }
            }
            return false;
        }

        /// <summary>
        /// PF-001修复: 添加武器预制体缺失检查
        /// </summary>
        public bool AddWeapon(WeaponData weaponData, bool autoEquip = false)
        {
            if (weaponSlots.Count >= maxWeaponSlots) return false;

            // PF-001修复: 检查weaponData是否为null
            if (weaponData == null)
            {
                Debug.LogError("[WeaponManager] 尝试添加null武器数据");
                return false;
            }

            // 创建武器实例
            GameObject weaponObj = null;
            if (weaponData.weaponPrefab != null)
            {
                weaponObj = Instantiate(weaponData.weaponPrefab, weaponParent);
            }
            else
            {
                // PF-001修复: 如果预制体为null，创建空物体但记录警告
                Debug.LogWarning($"[WeaponManager] {weaponData.weaponName} 没有设置预制体，创建空物体");
                weaponObj = new GameObject(weaponData.weaponName);
                weaponObj.transform.SetParent(weaponParent);
            }

            // 添加武器组件
            WeaponBase weapon;
            if (weaponData.weaponType == WeaponType.Melee)
            {
                weapon = weaponObj.GetComponent<MeleeWeapon>() ?? weaponObj.AddComponent<MeleeWeapon>();
            }
            else
            {
                weapon = weaponObj.GetComponent<RangedWeapon>() ?? weaponObj.AddComponent<RangedWeapon>();
            }

            // 设置数据
            var dataField = typeof(WeaponBase).GetField("weaponData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            dataField?.SetValue(weapon, weaponData);

            weapon.Initialize(transform, firePoint);
            weapon.Unlock();
            weapon.gameObject.SetActive(false);

            // 添加到槽位
            var slot = new WeaponSlot { Weapon = weapon, Data = weaponData };
            weaponSlots.Add(slot);

            if (autoEquip || currentWeapon == null)
            {
                SwitchToWeapon(weaponSlots.Count - 1);
            }

            return true;
        }

        /// <summary>
        /// 移除武器
        /// </summary>
        public bool RemoveWeapon(int index)
        {
            if (index < 0 || index >= weaponSlots.Count) return false;

            var slot = weaponSlots[index];
            if (slot.Weapon == currentWeapon)
            {
                currentWeapon.Unequip();
                currentWeapon = null;
                currentWeaponIndex = -1;
            }

            Destroy(slot.Weapon.gameObject);
            weaponSlots.RemoveAt(index);

            return true;
        }

        /// <summary>
        /// 升级武器 - WP-005
        /// </summary>
        public bool UpgradeWeapon(int index)
        {
            if (index < 0 || index >= weaponSlots.Count) return false;

            var weapon = weaponSlots[index].Weapon;
            if (weapon == null) return false;

            return weapon.Upgrade();
        }

        /// <summary>
        /// 获取武器
        /// </summary>
        public WeaponBase GetWeapon(int index)
        {
            if (index < 0 || index >= weaponSlots.Count) return null;
            return weaponSlots[index].Weapon;
        }

        /// <summary>
        /// 尝试攻击
        /// </summary>
        public bool TryAttack(Vector2 direction)
        {
            if (currentWeapon == null) return false;
            return currentWeapon.TryAttack(direction);
        }

        /// <summary>
        /// 设置切换冷却
        /// </summary>
        public void SetSwitchCooldown(float cooldown)
        {
            weaponSwitchCooldown = cooldown;
        }
    }

    /// <summary>
    /// 武器槽位数据
    /// </summary>
    [System.Serializable]
    public class WeaponSlot
    {
        public WeaponBase Weapon;
        public WeaponData Data;
    }
}