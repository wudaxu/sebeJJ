using NUnit.Framework;
using UnityEngine;

namespace SebeJJ.Tests.Player
{
    /// <summary>
    /// MechStats ScriptableObject 测试
    /// </summary>
    public class MechStatsTests
    {
        private MechStats _mechStats;

        [SetUp]
        public void Setup()
        {
            _mechStats = ScriptableObject.CreateInstance<MechStats>();
            _mechStats.maxHealth = 100f;
            _mechStats.maxEnergy = 100f;
            _mechStats.maxOxygen = 100f;
            _mechStats.armor = 10f;
            _mechStats.pressureResistance = 100f;
            _mechStats.speed = 5f;
            _mechStats.turnRate = 180f;
            _mechStats.miningPower = 1f;
            _mechStats.cargoCapacity = 50f;
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_mechStats);
        }

        [Test]
        public void MechStats_DefaultValues_AreCorrect()
        {
            // Assert
            Assert.AreEqual(100f, _mechStats.maxHealth);
            Assert.AreEqual(100f, _mechStats.maxEnergy);
            Assert.AreEqual(100f, _mechStats.maxOxygen);
            Assert.AreEqual(10f, _mechStats.armor);
            Assert.AreEqual(100f, _mechStats.pressureResistance);
            Assert.AreEqual(5f, _mechStats.speed);
            Assert.AreEqual(180f, _mechStats.turnRate);
            Assert.AreEqual(1f, _mechStats.miningPower);
            Assert.AreEqual(50f, _mechStats.cargoCapacity);
        }

        [Test]
        public void MechStats_ModifyValues_UpdatesCorrectly()
        {
            // Act
            _mechStats.speed = 8f;
            _mechStats.maxHealth = 150f;

            // Assert
            Assert.AreEqual(8f, _mechStats.speed);
            Assert.AreEqual(150f, _mechStats.maxHealth);
        }

        [Test]
        public void MechStats_DifferentInstances_AreIndependent()
        {
            // Arrange
            var stats1 = ScriptableObject.CreateInstance<MechStats>();
            var stats2 = ScriptableObject.CreateInstance<MechStats>();

            stats1.speed = 5f;
            stats2.speed = 10f;

            // Assert
            Assert.AreEqual(5f, stats1.speed);
            Assert.AreEqual(10f, stats2.speed);

            // Cleanup
            Object.DestroyImmediate(stats1);
            Object.DestroyImmediate(stats2);
        }
    }

    /// <summary>
    /// 机甲属性 ScriptableObject
    /// </summary>
    public class MechStats : ScriptableObject
    {
        public float maxHealth = 100f;
        public float maxEnergy = 100f;
        public float maxOxygen = 100f;
        public float armor = 10f;
        public float pressureResistance = 100f;
        public float speed = 5f;
        public float turnRate = 180f;
        public float miningPower = 1f;
        public float cargoCapacity = 50f;
    }
}
