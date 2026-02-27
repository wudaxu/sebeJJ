using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SebeJJ.Utils;

namespace SebeJJ.Core
{
    /// <summary>
    /// 游戏管理器 - 管理游戏全局状态和流程
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [Header("Game Settings")]
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private string gameScene = "MainScene";
        [SerializeField] private bool autoSaveEnabled = true;
        [SerializeField] private float autoSaveInterval = 300f; // 5分钟

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        public GameState CurrentState { get; private set; } = GameState.None;
        public GameState PreviousState { get; private set; } = GameState.None;
        public bool IsPaused => CurrentState == GameState.Paused;
        public bool IsPlaying => CurrentState == GameState.Playing;

        private float _autoSaveTimer;
        private bool _isInitialized = false;

        #region Unity Lifecycle

        protected override void OnAwake()
        {
            base.OnAwake();
            Initialize();
        }

        private void Update()
        {
            if (autoSaveEnabled && IsPlaying)
            {
                HandleAutoSave();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause && IsPlaying)
            {
                SaveManager.Instance?.AutoSave();
            }
        }

        private void OnApplicationQuit()
        {
            if (IsPlaying)
            {
                SaveManager.Instance?.AutoSave();
            }
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            if (_isInitialized) return;

            Log("Initializing GameManager...");
            
            // 初始化其他管理器
            InitializeManagers();
            
            // 订阅事件
            SubscribeToEvents();
            
            _isInitialized = true;
            Log("GameManager initialized.");
        }

        private void InitializeManagers()
        {
            // 确保其他管理器实例已创建
            var saveManager = SaveManager.Instance;
            var uiManager = UIManager.Instance;
            var resourceManager = ResourceManager.Instance;
            var missionManager = MissionManager.Instance;
            
            Log("All managers initialized.");
        }

        private void SubscribeToEvents()
        {
            // 订阅需要的事件
        }

        #endregion

        #region State Management

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            PreviousState = CurrentState;
            CurrentState = newState;

            Log($"Game state changed: {PreviousState} -> {CurrentState}");

            HandleStateChange(newState);
            GameEvents.OnGameStateChanged?.Invoke(newState);
        }

        private void HandleStateChange(GameState newState)
        {
            switch (newState)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    break;
                    
                case GameState.Loading:
                    Time.timeScale = 1f;
                    break;
                    
                case GameState.Playing:
                    Time.timeScale = 1f;
                    GameEvents.OnGameResume?.Invoke();
                    break;
                    
                case GameState.Paused:
                    Time.timeScale = 0f;
                    GameEvents.OnGamePause?.Invoke();
                    break;
                    
                case GameState.GameOver:
                    Time.timeScale = 0f;
                    GameEvents.OnGameOver?.Invoke();
                    break;
                    
                case GameState.Victory:
                    Time.timeScale = 0f;
                    GameEvents.OnGameVictory?.Invoke();
                    break;
            }
        }

        public void PauseGame()
        {
            if (IsPlaying)
            {
                ChangeState(GameState.Paused);
            }
        }

        public void ResumeGame()
        {
            if (IsPaused)
            {
                ChangeState(GameState.Playing);
            }
        }

        public void TogglePause()
        {
            if (IsPaused)
                ResumeGame();
            else
                PauseGame();
        }

        #endregion

        #region Scene Management

        public void LoadMainMenu()
        {
            ChangeState(GameState.Loading);
            StartCoroutine(LoadSceneAsync(mainMenuScene));
        }

        public void StartNewGame()
        {
            Log("Starting new game...");
            SaveManager.Instance.CreateNewSave();
            LoadGameScene();
        }

        public void ContinueGame()
        {
            if (SaveManager.Instance.HasSaveData())
            {
                Log("Continuing game...");
                SaveManager.Instance.LoadGame();
                LoadGameScene();
            }
            else
            {
                LogWarning("No save data found. Starting new game.");
                StartNewGame();
            }
        }

        public void LoadGameScene()
        {
            ChangeState(GameState.Loading);
            StartCoroutine(LoadSceneAsync(gameScene, () =>
            {
                ChangeState(GameState.Playing);
                GameEvents.OnGameStart?.Invoke();
            }));
        }

        public void RestartLevel()
        {
            LoadGameScene();
        }

        public void QuitGame()
        {
            Log("Quitting game...");
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        private IEnumerator LoadSceneAsync(string sceneName, Action onComplete = null)
        {
            GameEvents.OnSceneLoadStarted?.Invoke(sceneName);
            
            var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                // 可以在这里更新加载进度UI
                yield return null;
            }

            GameEvents.OnSceneLoadCompleted?.Invoke(sceneName);
            onComplete?.Invoke();
        }

        #endregion

        #region Auto Save

        private void HandleAutoSave()
        {
            _autoSaveTimer += Time.deltaTime;
            
            if (_autoSaveTimer >= autoSaveInterval)
            {
                SaveManager.Instance.AutoSave();
                _autoSaveTimer = 0f;
            }
        }

        public void TriggerManualSave()
        {
            SaveManager.Instance.SaveGame();
        }

        #endregion

        #region Game Over

        public void GameOver()
        {
            ChangeState(GameState.GameOver);
        }

        public void Victory()
        {
            ChangeState(GameState.Victory);
        }

        #endregion

        #region Debug

        private void Log(string message)
        {
            if (showDebugLogs)
                Debug.Log($"[GameManager] {message}");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[GameManager] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[GameManager] {message}");
        }

        #endregion
    }
}
