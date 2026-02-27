using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SebeJJ.Tests.Core
{
    /// <summary>
    /// GameManager 单元测试
    /// </summary>
    public class GameManagerTests
    {
        private GameManager _gameManager;

        [SetUp]
        public void Setup()
        {
            // 创建测试用的 GameManager
            var go = new GameObject("GameManager");
            _gameManager = go.AddComponent<GameManager>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_gameManager.gameObject);
        }

        [Test]
        public void GameManager_Singleton_ReturnsSameInstance()
        {
            // Act
            var instance1 = GameManager.Instance;
            var instance2 = GameManager.Instance;

            // Assert
            Assert.IsNotNull(instance1);
            Assert.AreSame(instance1, instance2);
        }

        [Test]
        public void GameManager_InitialState_IsMainMenu()
        {
            // Assert
            Assert.AreEqual(GameState.MainMenu, _gameManager.CurrentState);
        }

        [Test]
        public void GameManager_ChangeState_UpdatesCurrentState()
        {
            // Act
            _gameManager.ChangeState(GameState.Playing);

            // Assert
            Assert.AreEqual(GameState.Playing, _gameManager.CurrentState);
        }

        [Test]
        public void GameManager_PauseGame_SetsTimeScaleToZero()
        {
            // Arrange
            _gameManager.ChangeState(GameState.Playing);
            Time.timeScale = 1f;

            // Act
            _gameManager.PauseGame();

            // Assert
            Assert.AreEqual(0f, Time.timeScale);
            Assert.AreEqual(GameState.Paused, _gameManager.CurrentState);
        }

        [Test]
        public void GameManager_ResumeGame_RestoresTimeScale()
        {
            // Arrange
            _gameManager.ChangeState(GameState.Playing);
            _gameManager.PauseGame();

            // Act
            _gameManager.ResumeGame();

            // Assert
            Assert.AreEqual(1f, Time.timeScale);
            Assert.AreEqual(GameState.Playing, _gameManager.CurrentState);
        }

        [Test]
        public void GameManager_OnStateChanged_EventFires()
        {
            // Arrange
            bool eventFired = false;
            GameState newState = GameState.MainMenu;
            
            _gameManager.OnStateChanged += (state) =>
            {
                eventFired = true;
                newState = state;
            };

            // Act
            _gameManager.ChangeState(GameState.Playing);

            // Assert
            Assert.IsTrue(eventFired);
            Assert.AreEqual(GameState.Playing, newState);
        }
    }

    /// <summary>
    /// 游戏状态枚举
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Loading
    }

    /// <summary>
    /// 单例基类（简化版）
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                }
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// GameManager 简化实现（用于测试）
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        public System.Action<GameState> OnStateChanged;

        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                Time.timeScale = 0f;
                ChangeState(GameState.Paused);
            }
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                Time.timeScale = 1f;
                ChangeState(GameState.Playing);
            }
        }
    }
}
