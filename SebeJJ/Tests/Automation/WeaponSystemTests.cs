using NUnit.Framework;
using UnityEngine;

namespace SebeJJ.Tests
{
    /// <summary>
    /// 武器系统测试 - Week 2 战斗系统
    /// </summary>
    public class WeaponSystemTests
    {
        private WeaponSystem _weaponSystem;
        private TestWeapon _testWeapon;

        [SetUp]
        public void Setup()
        {
            _weaponSystem = new WeaponSystem();
            _testWeapon = new TestWeapon
            {
                WeaponName = "TestWeapon",
                Damage = 50f,
                FireRate = 2f,
                Range = 20f,
                EnergyCost = 10f,
                DamageType = DamageType.Energy
            };
        }

        [TearDown]
        public void Teardown()
        {
            _weaponSystem = null;
            _testWeapon = null;
        }

        #region 武器基础功能

        [Test]
        [Category("Weapon")]
        public void WeaponSystem_EquipWeapon_WeaponEquipped()
        {
            // Act
            _weaponSystem.EquipWeapon(_testWeapon);
            
            // Assert
            Assert.AreEqual(_testWeapon, _weaponSystem.CurrentWeapon);
            Assert.IsTrue(_testWeapon.IsEquipped);
        }

        [Test]
        [Category("Weapon")]
        public void WeaponSystem_Fire_CreatesProjectile()
        {
            // Arrange
            _weaponSystem.EquipWeapon(_testWeapon);
            bool projectileCreated = false;
            _weaponSystem.OnProjectileFired += (proj) => projectileCreated = true;
            
            // Act
            bool fired = _weaponSystem.Fire(Vector2.right);
            
            // Assert
            Assert.IsTrue(fired);
            Assert.IsTrue(projectileCreated);
        }

        [Test]
        [Category("Weapon")]
        public void WeaponSystem_Fire_RespectsFireRate()
        {
            // Arrange
            _weaponSystem.EquipWeapon(_testWeapon);
            _weaponSystem.Fire(Vector2.right);
            
            // Act
            bool secondFire = _weaponSystem.Fire(Vector2.right);
            
            // Assert - should be on cooldown
            Assert.IsFalse(secondFire);
        }

        [Test]
        [Category("Weapon")]
        public void WeaponSystem_Fire_ConsumesEnergy()
        {
            // Arrange
            _weaponSystem.EquipWeapon(_testWeapon);
            _weaponSystem.CurrentEnergy = 100f;
            
            // Act
            _weaponSystem.Fire(Vector2.right);
            
            // Assert
            Assert.AreEqual(90f, _weaponSystem.CurrentEnergy);
        }

        [Test]
        [Category("Weapon")]
        public void WeaponSystem_Fire_InsufficientEnergy_DoesNotFire()
        {
            // Arrange
            _weaponSystem.EquipWeapon(_testWeapon);
            _weaponSystem.CurrentEnergy = 5f; // Less than cost of 10
            
            // Act
            bool fired = _weaponSystem.Fire(Vector2.right);
            
            // Assert
            Assert.IsFalse(fired);
        }

        #endregion

        #region 武器切换

        [Test]
        [Category("Weapon")]
        public void WeaponSystem_SwitchWeapon_ChangesCurrentWeapon()
        {
            // Arrange
            var weapon1 = new TestWeapon { WeaponName = "Weapon1" };
            var weapon2 = new TestWeapon { WeaponName = "Weapon2" };
            _weaponSystem.AddWeapon(weapon1);
            _weaponSystem.AddWeapon(weapon2);
            _weaponSystem.EquipWeapon(weapon1);
            
            // Act
            _weaponSystem.SwitchToWeapon(1);
            
            // Assert
            Assert.AreEqual(weapon2, _weaponSystem.CurrentWeapon);
        }

        [Test]
        [Category("Weapon")]
        public void WeaponSystem_SwitchWeapon_UnequipsOld()
        {
            // Arrange
            var weapon1 = new TestWeapon { WeaponName = "Weapon1" };
            var weapon2 = new TestWeapon { WeaponName = "Weapon2" };
            _weaponSystem.AddWeapon(weapon1);
            _weaponSystem.AddWeapon(weapon2);
            _weaponSystem.EquipWeapon(weapon1);
            
            // Act
            _weaponSystem.SwitchToWeapon(1);
            
            // Assert
            Assert.IsFalse(weapon1.IsEquipped);
            Assert.IsTrue(weapon2.IsEquipped);
        }

        [Test]
        [Category("Weapon")]
        public void WeaponSystem_SwitchWeapon_TriggersEvent()
        {
            // Arrange
            var weapon1 = new TestWeapon { WeaponName = "Weapon1" };
            var weapon2 = new TestWeapon { WeaponName = "Weapon2" };
            _weaponSystem.AddWeapon(weapon1);
            _weaponSystem.AddWeapon(weapon2);
            _weaponSystem.EquipWeapon(weapon1);
            
            bool eventTriggered = false;
            _weaponSystem.OnWeaponSwitched += (w) => eventTriggered = true;
            
            // Act
            _weaponSystem.SwitchToWeapon(1);
            
            // Assert
            Assert.IsTrue(eventTriggered);
        }

        #endregion

        #region 能量管理

        [Test]
        [Category("Weapon")]
        [Category("Energy")]
        public void WeaponSystem_EnergyRegen_RegeneratesOverTime()
        {
            // Arrange
            _weaponSystem.CurrentEnergy = 50f;
            _weaponSystem.MaxEnergy = 100f;
            _weaponSystem.EnergyRegenRate = 10f;
            
            // Act
            _weaponSystem.Update(1f); // Simulate 1 second
            
            // Assert
            Assert.AreEqual(60f, _weaponSystem.CurrentEnergy);
        }

        [Test]
        [Category("Weapon")]
        [Category("Energy")]
        public void WeaponSystem_EnergyRegen_DoesNotExceedMax()
        {
            // Arrange
            _weaponSystem.CurrentEnergy = 95f;
            _weaponSystem.MaxEnergy = 100f;
            _weaponSystem.EnergyRegenRate = 10f;
            
            // Act
            _weaponSystem.Update(1f);
            
            // Assert
            Assert.AreEqual(100f, _weaponSystem.CurrentEnergy);
        }

        [Test]
        [Category("Weapon")]
        [Category("Energy")]
        public void WeaponSystem_EnergyRatio_ReturnsCorrectValue()
        {
            // Arrange
            _weaponSystem.CurrentEnergy = 50f;
            _weaponSystem.MaxEnergy = 100f;
            
            // Act
            float ratio = _weaponSystem.GetEnergyRatio();
            
            // Assert
            Assert.AreEqual(0.5f, ratio);
        }

        #endregion

        #region 武器冷却

        [Test]
        [Category("Weapon")]
        public void WeaponSystem_Cooldown_UpdatesOverTime()
        {
            // Arrange
            _weaponSystem.EquipWeapon(_testWeapon);
            _weaponSystem.Fire(Vector2.right);
            
            // Act
            _weaponSystem.Update(0.6f); // Fire rate is 2/s = 0.5s cooldown
            
            // Assert
            Assert.IsTrue(_weaponSystem.CanFire());
        }

        [Test]
        [Category("Weapon")]
        public void WeaponSystem_CooldownProgress_ReturnsCorrectValue()
        {
            // Arrange
            _weaponSystem.EquipWeapon(_testWeapon);
            _weaponSystem.Fire(Vector2.right);
            
            // Act
            _weaponSystem.Update(0.25f); // Halfway through 0.5s cooldown
            
            // Assert
            Assert.AreEqual(0.5f, _weaponSystem.GetCooldownProgress(), 0.1f);
        }

        #endregion

        #region 武器属性

        [Test]
        [Category("Weapon")]
        public void WeaponSystem_GetWeaponDamage_ReturnsCorrectValue()
        {
            // Arrange
            _weaponSystem.EquipWeapon(_testWeapon);
            
            // Act
            float damage = _weaponSystem.GetCurrentWeaponDamage();
            
            // Assert
            Assert.AreEqual(50f, damage);
        }

        [Test]
        [Category("Weapon")]
        public void WeaponSystem_GetWeaponRange_ReturnsCorrectValue()
        {
            // Arrange
            _weaponSystem.EquipWeapon(_testWeapon);
            
            // Act
            float range = _weaponSystem.GetCurrentWeaponRange();
            
            // Assert
            Assert.AreEqual(20f, range);
        }

        #endregion

        #region 连续射击武器

        [Test]
        [Category("Weapon")]
        public void WeaponSystem_ContinuousWeapon_FiresWhileHeld()
        {
            // Arrange
            var continuousWeapon = new TestWeapon
            {
                WeaponName = "Laser",
                IsContinuous = true,
                Damage = 10f,
                EnergyCost = 5f
            };
            _weaponSystem.EquipWeapon(continuousWeapon);
            
            int fireCount = 0;
            _weaponSystem.OnProjectileFired += (_) => fireCount++;
            
            // Act
            _weaponSystem.StartContinuousFire(Vector2.right);
            _weaponSystem.Update(0.5f);
            _weaponSystem.StopContinuousFire();
            
            // Assert
            Assert.Greater(fireCount, 1);
        }

        #endregion

        #region 测试辅助类

        private class TestWeapon
        {
            public string WeaponName { get; set; }
            public float Damage { get; set; }
            public float FireRate { get; set; } = 1f;
            public float Range { get; set; } = 10f;
            public float EnergyCost { get; set; } = 10f;
            public DamageType DamageType { get; set; } = DamageType.Physical;
            public bool IsContinuous { get; set; } = false;
            public bool IsEquipped { get; set; }
            public float CooldownRemaining { get; set; }
        }

        #endregion
    }

    #region 武器系统实现

    public class WeaponSystem
    {
        private TestWeapon _currentWeapon;
        private System.Collections.Generic.List<TestWeapon> _weapons = new();
        private float _currentCooldown;
        private bool _isContinuousFiring;
        private Vector2 _continuousFireDirection;

        public TestWeapon CurrentWeapon => _currentWeapon;
        public float CurrentEnergy { get; set; } = 100f;
        public float MaxEnergy { get; set; } = 100f;
        public float EnergyRegenRate { get; set; } = 10f;

        public event System.Action<object> OnProjectileFired;
        public event System.Action<TestWeapon> OnWeaponSwitched;

        public void EquipWeapon(TestWeapon weapon)
        {
            if (_currentWeapon != null)
                _currentWeapon.IsEquipped = false;
            
            _currentWeapon = weapon;
            _currentWeapon.IsEquipped = true;
        }

        public void AddWeapon(TestWeapon weapon)
        {
            _weapons.Add(weapon);
        }

        public bool Fire(Vector2 direction)
        {
            if (_currentWeapon == null) return false;
            if (_currentCooldown > 0) return false;
            if (CurrentEnergy < _currentWeapon.EnergyCost) return false;

            CurrentEnergy -= _currentWeapon.EnergyCost;
            _currentCooldown = 1f / _currentWeapon.FireRate;
            
            OnProjectileFired?.Invoke(null);
            return true;
        }

        public void StartContinuousFire(Vector2 direction)
        {
            _isContinuousFiring = true;
            _continuousFireDirection = direction;
        }

        public void StopContinuousFire()
        {
            _isContinuousFiring = false;
        }

        public void SwitchToWeapon(int index)
        {
            if (index >= 0 && index < _weapons.Count)
            {
                EquipWeapon(_weapons[index]);
                OnWeaponSwitched?.Invoke(_currentWeapon);
            }
        }

        public void Update(float deltaTime)
        {
            // Update cooldown
            if (_currentCooldown > 0)
                _currentCooldown -= deltaTime;

            // Regen energy
            CurrentEnergy = Mathf.Min(CurrentEnergy + EnergyRegenRate * deltaTime, MaxEnergy);

            // Handle continuous fire
            if (_isContinuousFiring && _currentWeapon?.IsContinuous == true)
            {
                Fire(_continuousFireDirection);
            }
        }

        public bool CanFire()
        {
            return _currentCooldown <= 0 && CurrentEnergy >= (_currentWeapon?.EnergyCost ?? float.MaxValue);
        }

        public float GetCooldownProgress()
        {
            if (_currentWeapon == null || _currentCooldown <= 0) return 1f;
            float totalCooldown = 1f / _currentWeapon.FireRate;
            return 1f - (_currentCooldown / totalCooldown);
        }

        public float GetEnergyRatio()
        {
            return CurrentEnergy / MaxEnergy;
        }

        public float GetCurrentWeaponDamage()
        {
            return _currentWeapon?.Damage ?? 0f;
        }

        public float GetCurrentWeaponRange()
        {
            return _currentWeapon?.Range ?? 0f;
        }
    }

    #endregion
}
