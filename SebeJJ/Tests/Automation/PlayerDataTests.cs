using NUnit.Framework;

namespace SebeJJ.Tests.Player
{
    /// <summary>
    /// PlayerData 单元测试
    /// </summary>
    public class PlayerDataTests
    {
        private PlayerData _playerData;

        [SetUp]
        public void Setup()
        {
            _playerData = new PlayerData
            {
                maxHealth = 100f,
                maxEnergy = 100f,
                maxOxygen = 100f,
                health = 100f,
                energy = 100f,
                oxygen = 100f
            };
        }

        [Test]
        public void PlayerData_TakeDamage_ReducesHealth()
        {
            // Act
            _playerData.TakeDamage(20f);

            // Assert
            Assert.AreEqual(80f, _playerData.health);
        }

        [Test]
        public void PlayerData_TakeDamage_DoesNotGoBelowZero()
        {
            // Act
            _playerData.TakeDamage(150f);

            // Assert
            Assert.AreEqual(0f, _playerData.health);
        }

        [Test]
        public void PlayerData_Heal_IncreasesHealth()
        {
            // Arrange
            _playerData.health = 50f;

            // Act
            _playerData.Heal(30f);

            // Assert
            Assert.AreEqual(80f, _playerData.health);
        }

        [Test]
        public void PlayerData_Heal_DoesNotExceedMax()
        {
            // Arrange
            _playerData.health = 90f;

            // Act
            _playerData.Heal(30f);

            // Assert
            Assert.AreEqual(100f, _playerData.health);
        }

        [Test]
        public void PlayerData_ConsumeEnergy_ReducesEnergy()
        {
            // Act
            _playerData.ConsumeEnergy(25f);

            // Assert
            Assert.AreEqual(75f, _playerData.energy);
        }

        [Test]
        public void PlayerData_ConsumeEnergy_DoesNotGoBelowZero()
        {
            // Act
            _playerData.ConsumeEnergy(150f);

            // Assert
            Assert.AreEqual(0f, _playerData.energy);
        }

        [Test]
        public void PlayerData_HasEnoughEnergy_ReturnsCorrectValue()
        {
            // Arrange
            _playerData.energy = 30f;

            // Assert
            Assert.IsTrue(_playerData.HasEnoughEnergy(30f));
            Assert.IsTrue(_playerData.HasEnoughEnergy(20f));
            Assert.IsFalse(_playerData.HasEnoughEnergy(31f));
        }

        [Test]
        public void PlayerData_ConsumeOxygen_ReducesOxygen()
        {
            // Act
            _playerData.ConsumeOxygen(10f);

            // Assert
            Assert.AreEqual(90f, _playerData.oxygen);
        }

        [Test]
        public void PlayerData_IsDead_WhenHealthIsZero()
        {
            // Arrange
            _playerData.health = 0f;

            // Assert
            Assert.IsTrue(_playerData.IsDead);
        }

        [Test]
        public void PlayerData_IsDead_WhenHealthIsAboveZero()
        {
            // Arrange
            _playerData.health = 1f;

            // Assert
            Assert.IsFalse(_playerData.IsDead);
        }

        [Test]
        public void PlayerData_HealthPercentage_ReturnsCorrectValue()
        {
            // Arrange
            _playerData.health = 75f;
            _playerData.maxHealth = 100f;

            // Assert
            Assert.AreEqual(0.75f, _playerData.HealthPercentage);
        }

        [Test]
        public void PlayerData_Reset_ResetsAllValues()
        {
            // Arrange
            _playerData.health = 50f;
            _playerData.energy = 30f;
            _playerData.oxygen = 20f;

            // Act
            _playerData.Reset();

            // Assert
            Assert.AreEqual(100f, _playerData.health);
            Assert.AreEqual(100f, _playerData.energy);
            Assert.AreEqual(100f, _playerData.oxygen);
        }
    }

    /// <summary>
    /// 玩家数据类（扩展版）
    /// </summary>
    public class PlayerData
    {
        public float maxHealth = 100f;
        public float maxEnergy = 100f;
        public float maxOxygen = 100f;
        
        public float health;
        public float energy;
        public float oxygen;

        public bool IsDead => health <= 0f;
        public float HealthPercentage => maxHealth > 0 ? health / maxHealth : 0f;
        public float EnergyPercentage => maxEnergy > 0 ? energy / maxEnergy : 0f;
        public float OxygenPercentage => maxOxygen > 0 ? oxygen / maxOxygen : 0f;

        public PlayerData()
        {
            Reset();
        }

        public void TakeDamage(float damage)
        {
            health = Mathf.Max(0f, health - damage);
        }

        public void Heal(float amount)
        {
            health = Mathf.Min(maxHealth, health + amount);
        }

        public void ConsumeEnergy(float amount)
        {
            energy = Mathf.Max(0f, energy - amount);
        }

        public void RestoreEnergy(float amount)
        {
            energy = Mathf.Min(maxEnergy, energy + amount);
        }

        public bool HasEnoughEnergy(float amount)
        {
            return energy >= amount;
        }

        public void ConsumeOxygen(float amount)
        {
            oxygen = Mathf.Max(0f, oxygen - amount);
        }

        public void RestoreOxygen(float amount)
        {
            oxygen = Mathf.Min(maxOxygen, oxygen + amount);
        }

        public void Reset()
        {
            health = maxHealth;
            energy = maxEnergy;
            oxygen = maxOxygen;
        }
    }

    // 补充 Mathf 类（用于测试环境）
    public static class Mathf
    {
        public static float Max(float a, float b) => a > b ? a : b;
        public static float Min(float a, float b) => a < b ? a : b;
    }
}
