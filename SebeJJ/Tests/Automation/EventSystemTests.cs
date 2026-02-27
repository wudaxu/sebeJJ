using System;
using NUnit.Framework;

namespace SebeJJ.Tests.Core
{
    /// <summary>
    /// EventSystem 单元测试
    /// </summary>
    public class EventSystemTests
    {
        [SetUp]
        public void Setup()
        {
            // 清理所有事件订阅
            GameEvents.ClearAllEvents();
        }

        [TearDown]
        public void Teardown()
        {
            GameEvents.ClearAllEvents();
        }

        [Test]
        public void GameEvents_SubscribeAndInvoke_CallbackFires()
        {
            // Arrange
            bool callbackFired = false;
            GameEvents.OnGameStart += () => callbackFired = true;

            // Act
            GameEvents.TriggerOnGameStart();

            // Assert
            Assert.IsTrue(callbackFired);
        }

        [Test]
        public void GameEvents_Unsubscribe_CallbackDoesNotFire()
        {
            // Arrange
            bool callbackFired = false;
            Action callback = () => callbackFired = true;
            GameEvents.OnGameStart += callback;
            GameEvents.OnGameStart -= callback;

            // Act
            GameEvents.TriggerOnGameStart();

            // Assert
            Assert.IsFalse(callbackFired);
        }

        [Test]
        public void GameEvents_MultipleSubscribers_AllFire()
        {
            // Arrange
            int callCount = 0;
            GameEvents.OnGameStart += () => callCount++;
            GameEvents.OnGameStart += () => callCount++;
            GameEvents.OnGameStart += () => callCount++;

            // Act
            GameEvents.TriggerOnGameStart();

            // Assert
            Assert.AreEqual(3, callCount);
        }

        [Test]
        public void GameEvents_HealthChanged_PassesCorrectValue()
        {
            // Arrange
            float receivedHealth = 0f;
            GameEvents.OnHealthChanged += (health) => receivedHealth = health;

            // Act
            GameEvents.TriggerOnHealthChanged(75.5f);

            // Assert
            Assert.AreEqual(75.5f, receivedHealth);
        }

        [Test]
        public void GameEvents_ResourceCollected_PassesCorrectParameters()
        {
            // Arrange
            ResourceType receivedType = ResourceType.ScrapMetal;
            int receivedCount = 0;
            
            GameEvents.OnResourceCollected += (type, count) =>
            {
                receivedType = type;
                receivedCount = count;
            };

            // Act
            GameEvents.TriggerOnResourceCollected(ResourceType.CopperOre, 5);

            // Assert
            Assert.AreEqual(ResourceType.CopperOre, receivedType);
            Assert.AreEqual(5, receivedCount);
        }

        [Test]
        public void GameEvents_NoSubscribers_DoesNotThrow()
        {
            // Act & Assert - 不应抛出异常
            Assert.DoesNotThrow(() => GameEvents.TriggerOnGameStart());
            Assert.DoesNotThrow(() => GameEvents.TriggerOnHealthChanged(50f));
            Assert.DoesNotThrow(() => GameEvents.TriggerOnResourceCollected(ResourceType.ScrapMetal, 1));
        }

        [Test]
        public void GameEvents_PlayerDeath_MultipleListeners()
        {
            // Arrange
            bool listener1Fired = false;
            bool listener2Fired = false;
            
            GameEvents.OnPlayerDeath += () => listener1Fired = true;
            GameEvents.OnPlayerDeath += () => listener2Fired = true;

            // Act
            GameEvents.TriggerOnPlayerDeath();

            // Assert
            Assert.IsTrue(listener1Fired);
            Assert.IsTrue(listener2Fired);
        }
    }

    /// <summary>
    /// 资源类型枚举
    /// </summary>
    public enum ResourceType
    {
        ScrapMetal,
        CopperOre,
        IronOre,
        GoldOre,
        CrystalShard,
        Uranium,
        BioSample,
        DataFragment,
        AncientTech
    }

    /// <summary>
    /// 全局事件系统
    /// </summary>
    public static class GameEvents
    {
        // 玩家事件
        public static Action OnGameStart;
        public static Action OnGamePause;
        public static Action OnGameResume;
        public static Action OnGameOver;
        public static Action OnPlayerDeath;
        public static Action<float> OnHealthChanged;
        public static Action<float> OnEnergyChanged;
        public static Action<float> OnOxygenChanged;
        public static Action<ResourceType, int> OnResourceCollected;

        // 游戏事件
        public static Action OnLevelCompleted;
        public static Action<int> OnDepthChanged;

        // UI事件
        public static Action<string> OnShowDialogue;
        public static Action<string> OnShowWarning;

        // 触发方法
        public static void TriggerOnGameStart() => OnGameStart?.Invoke();
        public static void TriggerOnGamePause() => OnGamePause?.Invoke();
        public static void TriggerOnGameResume() => OnGameResume?.Invoke();
        public static void TriggerOnGameOver() => OnGameOver?.Invoke();
        public static void TriggerOnPlayerDeath() => OnPlayerDeath?.Invoke();
        public static void TriggerOnHealthChanged(float health) => OnHealthChanged?.Invoke(health);
        public static void TriggerOnEnergyChanged(float energy) => OnEnergyChanged?.Invoke(energy);
        public static void TriggerOnOxygenChanged(float oxygen) => OnOxygenChanged?.Invoke(oxygen);
        public static void TriggerOnResourceCollected(ResourceType type, int count) => OnResourceCollected?.Invoke(type, count);
        public static void TriggerOnLevelCompleted() => OnLevelCompleted?.Invoke();
        public static void TriggerOnDepthChanged(int depth) => OnDepthChanged?.Invoke(depth);
        public static void TriggerOnShowDialogue(string text) => OnShowDialogue?.Invoke(text);
        public static void TriggerOnShowWarning(string message) => OnShowWarning?.Invoke(message);

        /// <summary>
        /// 清理所有事件订阅（仅用于测试）
        /// </summary>
        public static void ClearAllEvents()
        {
            OnGameStart = null;
            OnGamePause = null;
            OnGameResume = null;
            OnGameOver = null;
            OnPlayerDeath = null;
            OnHealthChanged = null;
            OnEnergyChanged = null;
            OnOxygenChanged = null;
            OnResourceCollected = null;
            OnLevelCompleted = null;
            OnDepthChanged = null;
            OnShowDialogue = null;
            OnShowWarning = null;
        }
    }
}
