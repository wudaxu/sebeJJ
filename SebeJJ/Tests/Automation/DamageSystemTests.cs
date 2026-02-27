using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace SebeJJ.Tests
{
    /// <summary>
    /// 伤害系统测试 - Week 2 战斗系统
    /// </summary>
    public class DamageSystemTests
    {
        private DamageSystem _damageSystem;
        private TestDamageable _testTarget;

        [SetUp]
        public void Setup()
        {
            _damageSystem = new DamageSystem();
            _testTarget = new TestDamageable { MaxHealth = 100f, CurrentHealth = 100f };
        }

        [TearDown]
        public void Teardown()
        {
            _damageSystem = null;
            _testTarget = null;
        }

        #region 基础伤害计算

        [Test]
        [Category("Combat")]
        [Category("Damage")]
        public void DamageSystem_CalculateDamage_BasicDamage_ReturnsCorrectValue()
        {
            // Arrange
            float baseDamage = 50f;
            
            // Act
            float result = _damageSystem.CalculateDamage(baseDamage, 0f, 0f);
            
            // Assert
            Assert.AreEqual(50f, result);
        }

        [Test]
        [Category("Combat")]
        [Category("Damage")]
        public void DamageSystem_CalculateDamage_WithArmor_ReducesDamage()
        {
            // Arrange
            float baseDamage = 100f;
            float armor = 20f;
            
            // Act
            float result = _damageSystem.CalculateDamage(baseDamage, armor, 0f);
            
            // Assert
            Assert.AreEqual(80f, result);
        }

        [Test]
        [Category("Combat")]
        [Category("Damage")]
        public void DamageSystem_CalculateDamage_ArmorCannotReduceBelowOne()
        {
            // Arrange
            float baseDamage = 10f;
            float armor = 20f;
            
            // Act
            float result = _damageSystem.CalculateDamage(baseDamage, armor, 0f);
            
            // Assert
            Assert.AreEqual(1f, result);
        }

        [Test]
        [Category("Combat")]
        [Category("Damage")]
        [TestCase(100f, 0f, 100f)]
        [TestCase(100f, 0.3f, 70f)]
        [TestCase(100f, 0.5f, 50f)]
        [TestCase(100f, 1f, 0f)]
        public void DamageSystem_CalculateDamage_WithResistance_AppliesCorrectly(
            float baseDamage, float resistance, float expected)
        {
            // Act
            float result = _damageSystem.CalculateDamage(baseDamage, 0f, resistance);
            
            // Assert
            Assert.AreEqual(expected, result);
        }

        #endregion

        #region 伤害类型测试

        [Test]
        [Category("Combat")]
        [Category("Damage")]
        public void DamageSystem_ApplyDamage_PhysicalType_AppliesCorrectly()
        {
            // Arrange
            var damageInfo = new DamageInfo
            {
                Amount = 50f,
                Type = DamageType.Physical,
                Source = null
            };
            
            // Act
            _damageSystem.ApplyDamage(_testTarget, damageInfo);
            
            // Assert
            Assert.AreEqual(50f, _testTarget.CurrentHealth);
        }

        [Test]
        [Category("Combat")]
        [Category("Damage")]
        public void DamageSystem_ApplyDamage_EnergyType_UsesEnergyResistance()
        {
            // Arrange
            _testTarget.EnergyResistance = 0.5f;
            var damageInfo = new DamageInfo
            {
                Amount = 100f,
                Type = DamageType.Energy,
                Source = null
            };
            
            // Act
            _damageSystem.ApplyDamage(_testTarget, damageInfo);
            
            // Assert
            Assert.AreEqual(50f, _testTarget.CurrentHealth);
        }

        [Test]
        [Category("Combat")]
        [Category("Damage")]
        public void DamageSystem_ApplyDamage_ExplosiveType_UsesExplosiveResistance()
        {
            // Arrange
            _testTarget.ExplosiveResistance = 0.2f;
            var damageInfo = new DamageInfo
            {
                Amount = 100f,
                Type = DamageType.Explosive,
                Source = null
            };
            
            // Act
            _damageSystem.ApplyDamage(_testTarget, damageInfo);
            
            // Assert
            Assert.AreEqual(80f, _testTarget.CurrentHealth);
        }

        #endregion

        #region 暴击和弱点

        [Test]
        [Category("Combat")]
        [Category("Damage")]
        public void DamageSystem_ApplyDamage_CriticalHit_AppliesMultiplier()
        {
            // Arrange
            var damageInfo = new DamageInfo
            {
                Amount = 50f,
                Type = DamageType.Physical,
                IsCritical = true,
                CriticalMultiplier = 2f,
                Source = null
            };
            
            // Act
            _damageSystem.ApplyDamage(_testTarget, damageInfo);
            
            // Assert
            Assert.AreEqual(0f, _testTarget.CurrentHealth);
        }

        [Test]
        [Category("Combat")]
        [Category("Damage")]
        public void DamageSystem_ApplyDamage_WeakPointHit_AppliesMultiplier()
        {
            // Arrange
            var damageInfo = new DamageInfo
            {
                Amount = 50f,
                Type = DamageType.Physical,
                IsWeakPoint = true,
                WeakPointMultiplier = 1.5f,
                Source = null
            };
            
            // Act
            _damageSystem.ApplyDamage(_testTarget, damageInfo);
            
            // Assert
            Assert.AreEqual(25f, _testTarget.CurrentHealth);
        }

        #endregion

        #region 治疗系统

        [Test]
        [Category("Combat")]
        [Category("Healing")]
        public void DamageSystem_Heal_BasicHeal_IncreasesHealth()
        {
            // Arrange
            _testTarget.CurrentHealth = 50f;
            float healAmount = 30f;
            
            // Act
            _damageSystem.Heal(_testTarget, healAmount);
            
            // Assert
            Assert.AreEqual(80f, _testTarget.CurrentHealth);
        }

        [Test]
        [Category("Combat")]
        [Category("Healing")]
        public void DamageSystem_Heal_CannotExceedMaxHealth()
        {
            // Arrange
            _testTarget.CurrentHealth = 90f;
            float healAmount = 30f;
            
            // Act
            _damageSystem.Heal(_testTarget, healAmount);
            
            // Assert
            Assert.AreEqual(100f, _testTarget.CurrentHealth);
        }

        [Test]
        [Category("Combat")]
        [Category("Healing")]
        public void DamageSystem_Heal_FullHealth_NoChange()
        {
            // Arrange
            float healAmount = 30f;
            
            // Act
            _damageSystem.Heal(_testTarget, healAmount);
            
            // Assert
            Assert.AreEqual(100f, _testTarget.CurrentHealth);
        }

        #endregion

        #region 死亡事件

        [Test]
        [Category("Combat")]
        [Category("Damage")]
        public void DamageSystem_ApplyDamage_LethalDamage_TriggersDeathEvent()
        {
            // Arrange
            bool deathTriggered = false;
            _testTarget.OnDeath += () => deathTriggered = true;
            
            var damageInfo = new DamageInfo
            {
                Amount = 150f,
                Type = DamageType.Physical,
                Source = null
            };
            
            // Act
            _damageSystem.ApplyDamage(_testTarget, damageInfo);
            
            // Assert
            Assert.IsTrue(deathTriggered);
            Assert.IsFalse(_testTarget.IsAlive);
        }

        [Test]
        [Category("Combat")]
        [Category("Damage")]
        public void DamageSystem_ApplyDamage_ExactlyLethal_TriggersDeathEvent()
        {
            // Arrange
            bool deathTriggered = false;
            _testTarget.OnDeath += () => deathTriggered = true;
            
            var damageInfo = new DamageInfo
            {
                Amount = 100f,
                Type = DamageType.Physical,
                Source = null
            };
            
            // Act
            _damageSystem.ApplyDamage(_testTarget, damageInfo);
            
            // Assert
            Assert.IsTrue(deathTriggered);
            Assert.AreEqual(0f, _testTarget.CurrentHealth);
        }

        #endregion

        #region 免疫状态

        [Test]
        [Category("Combat")]
        [Category("Damage")]
        public void DamageSystem_ApplyDamage_Invulnerable_NoDamage()
        {
            // Arrange
            _testTarget.IsInvulnerable = true;
            var damageInfo = new DamageInfo
            {
                Amount = 50f,
                Type = DamageType.Physical,
                Source = null
            };
            
            // Act
            _damageSystem.ApplyDamage(_testTarget, damageInfo);
            
            // Assert
            Assert.AreEqual(100f, _testTarget.CurrentHealth);
        }

        #endregion

        #region 持续伤害(DOT)

        [UnityTest]
        [Category("Combat")]
        [Category("Damage")]
        [Category("DOT")]
        public IEnumerator DamageSystem_ApplyDOT_DamagesOverTime()
        {
            // Arrange
            var dotInfo = new DOTInfo
            {
                DamagePerTick = 10f,
                TickInterval = 0.5f,
                Duration = 1.5f,
                Type = DamageType.Corrosive
            };
            
            // Act
            _damageSystem.ApplyDOT(_testTarget, dotInfo);
            
            // Wait for first tick
            yield return new WaitForSeconds(0.6f);
            Assert.AreEqual(90f, _testTarget.CurrentHealth);
            
            // Wait for second tick
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(80f, _testTarget.CurrentHealth);
            
            // Wait for third tick
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(70f, _testTarget.CurrentHealth);
        }

        #endregion

        #region 测试辅助类

        private class TestDamageable : IDamageable
        {
            public float MaxHealth { get; set; }
            public float CurrentHealth { get; set; }
            public bool IsAlive => CurrentHealth > 0;
            public bool IsInvulnerable { get; set; }
            public float Armor { get; set; }
            public float PhysicalResistance { get; set; }
            public float EnergyResistance { get; set; }
            public float ExplosiveResistance { get; set; }
            public float PressureResistance { get; set; }
            public float CorrosiveResistance { get; set; }
            
            public event System.Action OnDeath;
            public event System.Action<float> OnDamageTaken;
            public event System.Action<float> OnHealed;

            public void TakeDamage(DamageInfo damageInfo)
            {
                if (IsInvulnerable) return;
                
                float damage = damageInfo.Amount;
                
                // Apply resistance based on damage type
                damage *= (1 - GetResistance(damageInfo.Type));
                
                // Apply critical multiplier
                if (damageInfo.IsCritical)
                    damage *= damageInfo.CriticalMultiplier;
                
                // Apply weak point multiplier
                if (damageInfo.IsWeakPoint)
                    damage *= damageInfo.WeakPointMultiplier;
                
                CurrentHealth -= damage;
                OnDamageTaken?.Invoke(damage);
                
                if (CurrentHealth <= 0)
                {
                    CurrentHealth = 0;
                    OnDeath?.Invoke();
                }
            }

            public void Heal(float amount)
            {
                CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
                OnHealed?.Invoke(amount);
            }

            private float GetResistance(DamageType type)
            {
                return type switch
                {
                    DamageType.Physical => PhysicalResistance,
                    DamageType.Energy => EnergyResistance,
                    DamageType.Explosive => ExplosiveResistance,
                    DamageType.Pressure => PressureResistance,
                    DamageType.Corrosive => CorrosiveResistance,
                    _ => 0f
                };
            }
        }

        #endregion
    }

    #region 接口和数据结构

    public interface IDamageable
    {
        void TakeDamage(DamageInfo damageInfo);
        void Heal(float amount);
        bool IsAlive { get; }
    }

    public struct DamageInfo
    {
        public float Amount;
        public DamageType Type;
        public Vector2 Direction;
        public GameObject Source;
        public bool IsCritical;
        public float CriticalMultiplier;
        public bool IsWeakPoint;
        public float WeakPointMultiplier;
    }

    public struct DOTInfo
    {
        public float DamagePerTick;
        public float TickInterval;
        public float Duration;
        public DamageType Type;
    }

    public enum DamageType
    {
        Physical,
        Energy,
        Explosive,
        Pressure,
        Corrosive
    }

    public class DamageSystem
    {
        public float CalculateDamage(float baseDamage, float armor, float resistance)
        {
            float damage = baseDamage - armor;
            damage = Mathf.Max(damage, 1f); // Minimum 1 damage
            damage *= (1 - resistance);
            return damage;
        }

        public void ApplyDamage(IDamageable target, DamageInfo damageInfo)
        {
            target.TakeDamage(damageInfo);
        }

        public void Heal(IDamageable target, float amount)
        {
            target.Heal(amount);
        }

        public void ApplyDOT(IDamageable target, DOTInfo dotInfo)
        {
            // Implementation would use a DOT manager
            // For testing, we'll apply immediate damage
            int ticks = Mathf.FloorToInt(dotInfo.Duration / dotInfo.TickInterval);
            for (int i = 0; i < ticks; i++)
            {
                target.TakeDamage(new DamageInfo 
                { 
                    Amount = dotInfo.DamagePerTick, 
                    Type = dotInfo.Type 
                });
            }
        }
    }

    #endregion
}
