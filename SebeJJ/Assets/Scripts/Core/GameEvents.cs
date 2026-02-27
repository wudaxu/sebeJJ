using System;
using SebeJJ.Core;

namespace SebeJJ.Core
{
    /// <summary>
    /// 全局事件系统 - 发布订阅模式
    /// </summary>
    public static class GameEvents
    {
        #region 游戏状态事件
        public static Action OnGameStart;
        public static Action OnGamePause;
        public static Action OnGameResume;
        public static Action OnGameOver;
        public static Action OnGameVictory;
        public static Action<GameState> OnGameStateChanged;
        #endregion

        #region 玩家事件
        public static Action<float, float> OnHealthChanged;     // 当前值, 最大值
        public static Action<float, float> OnEnergyChanged;     // 当前值, 最大值
        public static Action<float, float> OnOxygenChanged;     // 当前值, 最大值
        public static Action<float> OnDepthChanged;             // 当前深度
        public static Action<ResourceType, int> OnResourceCollected;  // 资源类型, 数量
        public static Action OnPlayerDeath;
        public static Action OnPlayerRespawn;
        #endregion

        #region 任务事件
        public static Action<string> OnMissionStarted;          // 任务ID
        public static Action<string> OnMissionCompleted;        // 任务ID
        public static Action<string> OnMissionFailed;           // 任务ID
        public static Action<string, float> OnMissionProgress;  // 任务ID, 进度(0-1)
        #endregion

        #region 经济事件
        public static Action<int> OnCurrencyChanged;            // 当前金额
        public static Action<int, int> OnCargoChanged;          // 当前容量, 最大容量
        #endregion

        #region UI事件
        public static Action<string> OnShowDialogue;
        public static Action<string> OnShowWarning;
        public static Action<string, float> OnShowNotification; // 消息, 持续时间
        #endregion

        #region 场景事件
        public static Action<string> OnSceneLoadStarted;
        public static Action<string> OnSceneLoadCompleted;
        public static Action OnLevelCompleted;
        #endregion

        /// <summary>
        /// 清除所有事件订阅（谨慎使用，主要用于场景切换）
        /// </summary>
        public static void ClearAllEvents()
        {
            OnGameStart = null;
            OnGamePause = null;
            OnGameResume = null;
            OnGameOver = null;
            OnGameVictory = null;
            OnGameStateChanged = null;
            
            OnHealthChanged = null;
            OnEnergyChanged = null;
            OnOxygenChanged = null;
            OnDepthChanged = null;
            OnResourceCollected = null;
            OnPlayerDeath = null;
            OnPlayerRespawn = null;
            
            OnMissionStarted = null;
            OnMissionCompleted = null;
            OnMissionFailed = null;
            OnMissionProgress = null;
            
            OnCurrencyChanged = null;
            OnCargoChanged = null;
            
            OnShowDialogue = null;
            OnShowWarning = null;
            OnShowNotification = null;
            
            OnSceneLoadStarted = null;
            OnSceneLoadCompleted = null;
            OnLevelCompleted = null;
        }
    }
}
