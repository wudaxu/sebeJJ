using UnityEngine;
using System.Collections.Generic;
using SebeJJ.Player;

namespace SebeJJ.Weapons
{
    /// <summary>
    /// 武器管理器 - 管理所有武器
    /// </summary>
    public class WeaponManager : MonoBehaviour
    {
        [Header("武器槽位")]
        [SerializeField] private List<Weapon> weapons = new List<Weapon>();
        [SerializeField] private Transform weaponPivot;
        [SerializeField] private Transform firePoint;

        [Header("瞄准")]
        [SerializeField] private float aimSmoothness = 15f;
        [SerializeField] private bool useMouseAim = true;

        [Header("能量")]
        [SerializeField] private MechStatus mechStatus;

        // 状态
        private int _currentWeaponIndex = 0;
        private Vector2 _aimDirection = Vector2.right;
        private bool _isFiring;

        // 属性
        public Weapon CurrentWeapon => weapons.Count > 0 && _currentWeaponIndex < weapons.Count 
            ? weapons[_currentWeaponIndex] 
            : null;
        public int WeaponCount => weapons.Count;
        public int CurrentWeaponIndex => _currentWeaponIndex;
        public Vector2 AimDirection => _aimDirection;

        // 事件
        public System.Action<Weapon> OnWeaponChanged;
        public System.Action<int> OnWeaponSwitched;

        private void Awake()
        {
            if (mechStatus == null)
            {
                mechStatus = GetComponent<MechStatus>();
            }

            // 初始化武器
            InitializeWeapons();
        }

        private void Update()
        {
            HandleAiming();
            HandleFiring();
            HandleWeaponSwitching();
        }

        /// <summary>
        /// 初始化武器
        /// </summary>
        private void InitializeWeapons()
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                if (weapons[i] != null)
                {
                    weapons[i].SetFirePoint(firePoint);
                    weapons[i].gameObject.SetActive(i == 0);
                }
            }

            if (weapons.Count > 0 && weapons[0] != null)
            {
                weapons[0].Equip();
            }
        }

        /// <summary>
        /// 处理瞄准
        /// </summary>
        private void HandleAiming()
        {
            Vector2 targetDirection;

            if (useMouseAim)
            {
                // 鼠标瞄准
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                targetDirection = (mousePos - weaponPivot.position).normalized;
            }
            else
            {
                // 右摇杆瞄准
                float horizontal = Input.GetAxis("RightStickHorizontal");
                float vertical = Input.GetAxis("RightStickVertical");
                
                if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
                {
                    targetDirection = new Vector2(horizontal, vertical).normalized;
                }
                else
                {
                    targetDirection = _aimDirection;
                }
            }

            // 平滑转向
            _aimDirection = Vector2.Lerp(_aimDirection, targetDirection, aimSmoothness * Time.deltaTime);

            // 旋转武器枢轴
            float angle = Mathf.Atan2(_aimDirection.y, _aimDirection.x) * Mathf.Rad2Deg;
            weaponPivot.rotation = Quaternion.Euler(0, 0, angle);
        }

        /// <summary>
        /// 处理射击
        /// </summary>
        private void HandleFiring()
        {
            if (CurrentWeapon == null) return;

            bool fireInput = Input.GetButton("Fire1");
            
            if (fireInput)
            {
                // 检查能量
                if (mechStatus != null && mechStatus.CurrentEnergy < CurrentWeapon.EnergyCost)
                {
                    // 能量不足
                    return;
                }

                bool fired = CurrentWeapon.TryFire(_aimDirection);
                
                if (fired && mechStatus != null)
                {
                    mechStatus.ConsumeEnergy(CurrentWeapon.EnergyCost);
                }

                _isFiring = true;
            }
            else
            {
                if (_isFiring)
                {
                    CurrentWeapon.StopFiring();
                    _isFiring = false;
                }
            }

            // 更新武器（用于持续型武器）
            CurrentWeapon.UpdateWeapon(_aimDirection);
        }

        /// <summary>
        /// 处理武器切换
        /// </summary>
        private void HandleWeaponSwitching()
        {
            // 数字键切换
            for (int i = 0; i < weapons.Count; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SwitchToWeapon(i);
                    return;
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

            // Q/E切换
            if (Input.GetKeyDown(KeyCode.Q))
            {
                SwitchToPreviousWeapon();
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                SwitchToNextWeapon();
            }
        }

        /// <summary>
        /// 切换到指定武器
        /// </summary>
        public void SwitchToWeapon(int index)
        {
            if (index < 0 || index >= weapons.Count) return;
            if (index == _currentWeaponIndex) return;

            // 卸下当前武器
            if (CurrentWeapon != null)
            {
                CurrentWeapon.Unequip();
            }

            // 切换到新武器
            _currentWeaponIndex = index;
            CurrentWeapon.Equip();

            OnWeaponChanged?.Invoke(CurrentWeapon);
            OnWeaponSwitched?.Invoke(_currentWeaponIndex);
        }

        /// <summary>
        /// 切换到下一个武器
        /// </summary>
        public void SwitchToNextWeapon()
        {
            int nextIndex = (_currentWeaponIndex + 1) % weapons.Count;
            SwitchToWeapon(nextIndex);
        }

        /// <summary>
        /// 切换到上一个武器
        /// </summary>
        public void SwitchToPreviousWeapon()
        {
            int prevIndex = _currentWeaponIndex - 1;
            if (prevIndex < 0) prevIndex = weapons.Count - 1;
            SwitchToWeapon(prevIndex);
        }

        /// <summary>
        /// 添加武器
        /// </summary>
        public void AddWeapon(Weapon weapon)
        {
            if (weapon == null || weapons.Contains(weapon)) return;

            weapons.Add(weapon);
            weapon.SetFirePoint(firePoint);
            weapon.gameObject.SetActive(false);

            // 如果是第一个武器，自动装备
            if (weapons.Count == 1)
            {
                SwitchToWeapon(0);
            }
        }

        /// <summary>
        /// 移除武器
        /// </summary>
        public void RemoveWeapon(Weapon weapon)
        {
            if (weapon == null) return;

            int index = weapons.IndexOf(weapon);
            if (index >= 0)
            {
                if (index == _currentWeaponIndex)
                {
                    weapon.Unequip();
                    SwitchToNextWeapon();
                }
                
                weapons.RemoveAt(index);
            }
        }

        /// <summary>
        /// 获取武器
        /// </summary>
        public Weapon GetWeapon(int index)
        {
            if (index >= 0 && index < weapons.Count)
            {
                return weapons[index];
            }
            return null;
        }

        /// <summary>
        /// 设置瞄准方向（用于AI）
        /// </summary>
        public void SetAimDirection(Vector2 direction)
        {
            _aimDirection = direction.normalized;
            float angle = Mathf.Atan2(_aimDirection.y, _aimDirection.x) * Mathf.Rad2Deg;
            weaponPivot.rotation = Quaternion.Euler(0, 0, angle);
        }

        /// <summary>
        /// 尝试开火（用于AI）
        /// </summary>
        public bool TryFire()
        {
            if (CurrentWeapon == null) return false;
            return CurrentWeapon.TryFire(_aimDirection);
        }

        /// <summary>
        /// 设置是否使用鼠标瞄准
        /// </summary>
        public void SetUseMouseAim(bool useMouse)
        {
            useMouseAim = useMouse;
        }
    }
}
