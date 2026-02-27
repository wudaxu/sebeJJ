using UnityEngine;
using System;
using System.Collections.Generic;

namespace SebeJJ.Core
{
    /// <summary>
    /// 游戏全局管理器 - 单例模式
    /// 负责协调所有子系统的生命周期
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("系统引用")]
        public UIManager uiManager;
        public SaveManager saveManager;
        public MissionManager missionManager;
        public ResourceManager resourceManager;
        public DiveManager diveManager;
        
        [Header("游戏状态")]
        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        
        // 游戏状态变更事件
        public event Action<GameState> OnGameStateChanged;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeSystems();
        }
        
        private void InitializeSystems()
        {
            // 确保所有管理器都已初始化
            if (saveManager == null) saveManager = FindObjectOfType<SaveManager>();
            if (missionManager == null) missionManager = FindObjectOfType<MissionManager>();
            if (resourceManager == null) resourceManager = FindObjectOfType<ResourceManager>();
            if (diveManager == null) diveManager = FindObjectOfType<DiveManager>();
            if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
            
            Debug.Log("[GameManager] 所有系统初始化完成");
        }
        
        /// <summary>
        /// 更改游戏状态
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;
            
            Debug.Log($"[GameManager] 状态变更: {CurrentState} -> {newState}");
            CurrentState = newState;
            OnGameStateChanged?.Invoke(newState);
        }
        
        /// <summary>
        /// 开始新游戏
        /// </summary>
        public void StartNewGame()
        {
            ChangeState(GameState.Playing);
            resourceManager?.InitializeNewGame();
            missionManager?.InitializeNewGame();
            diveManager?.InitializeNewGame();
        }
        
        /// <summary>
        /// 加载游戏
        /// </summary>
        public void LoadGame(string saveSlot)
        {
            if (saveManager?.LoadGame(saveSlot) == true)
            {
                ChangeState(GameState.Playing);
            }
        }
        
        /// <summary>
        /// 保存游戏
        /// </summary>
        public void SaveGame(string saveSlot)
        {
            saveManager?.SaveGame(saveSlot);
        }
        
        /// <summary>
        /// 返回主菜单
        /// </summary>
        public void ReturnToMainMenu()
        {
            ChangeState(GameState.MainMenu);
        }
        
        /// <summary>
        /// 暂停/恢复游戏
        /// </summary>
        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
                Time.timeScale = 0f;
            }
            else if (CurrentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
                Time.timeScale = 1f;
            }
        }
        
        /// <summary>
        /// 退出游戏
        /// </summary>
        public void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        private void OnDestroy()
        {
            // CO-007 修复: 清理事件订阅
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
    
    /// <summary>
    /// 游戏状态枚举
    /// </summary>
    public enum GameState
    {
        MainMenu,       // 主菜单
        Playing,        // 游戏中
        Paused,         // 暂停
        MissionSelect,  // 任务选择
        Shop,           // 商店
        GameOver        // 游戏结束
    }
}
