/** 
 * @file AIDebugger.cs
 * @brief AI调试可视化工具 - 任务AI-007
 * @description 提供AI状态机的实时调试信息和可视化
 * @author AI系统架构师
 * @date 2026-02-27
 */

using UnityEngine;
using UnityEngine.UI;
using System.Text;
using SebeJJ.AI;
using SebeJJ.Enemies;

namespace SebeJJ.AI.Debug
{
    /// <summary>
    /// AI调试器 - 用于实时监控和调试AI行为
    /// </summary>
    public class AIDebugger : MonoBehaviour
    {
        #region 单例模式
        
        private static AIDebugger _instance;
        public static AIDebugger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AIDebugger>();
                }
                return _instance;
            }
        }
        
        #endregion

        #region 序列化字段
        
        [Header("UI组件")]
        [SerializeField] private Canvas debugCanvas;
        [SerializeField] private Text debugText;
        [SerializeField] private Toggle enableDebugToggle;
        
        [Header("调试配置")]
        [SerializeField] private bool enableDebugByDefault = false;
        [SerializeField] private KeyCode toggleKey = KeyCode.F12;
        [SerializeField] private float updateInterval = 0.5f;
        
        [Header("可视化配置")]
        [SerializeField] private bool showStateLabels = true;
        [SerializeField] private bool showPathLines = true;
        [SerializeField] private bool showPerceptionRanges = true;
        [SerializeField] private Color stateLabelColor = Color.white;
        [SerializeField] private Color pathLineColor = Color.green;
        
        #endregion

        #region 私有字段
        
        private bool _isDebugEnabled = false;
        private float _updateTimer = 0f;
        private StringBuilder _stringBuilder = new StringBuilder();
        private AIStateMachine _selectedAI;
        
        #endregion

        #region Unity生命周期
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _isDebugEnabled = enableDebugByDefault;
            
            if (debugCanvas != null)
            {
                debugCanvas.gameObject.SetActive(_isDebugEnabled);
            }
        }
        
        private void Update()
        {
            // 切换调试显示
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleDebug();
            }
            
            if (!_isDebugEnabled) return;
            
            // 更新调试信息
            _updateTimer += Time.deltaTime;
            if (_updateTimer >= updateInterval)
            {
                UpdateDebugInfo();
                _updateTimer = 0f;
            }
        }
        
        private void OnGUI()
        {
            if (!_isDebugEnabled) return;
            
            DrawDebugGUI();
        }
        
        #endregion

        #region 调试控制
        
        /// <summary>
        /// 切换调试显示
        /// </summary>
        public void ToggleDebug()
        {
            _isDebugEnabled = !_isDebugEnabled;
            
            if (debugCanvas != null)
            {
                debugCanvas.gameObject.SetActive(_isDebugEnabled);
            }
            
            if (enableDebugToggle != null)
            {
                enableDebugToggle.isOn = _isDebugEnabled;
            }
            
            UnityEngine.Debug.Log($"[AIDebugger] 调试模式: {(_isDebugEnabled ? "开启" : "关闭")}");
        }
        
        /// <summary>
        /// 设置调试启用状态
        /// </summary>
        /// <param name="enabled">是否启用</param>
        public void SetDebugEnabled(bool enabled)
        {
            _isDebugEnabled = enabled;
            
            if (debugCanvas != null)
            {
                debugCanvas.gameObject.SetActive(_isDebugEnabled);
            }
        }
        
        #endregion

        #region 调试信息更新
        
        /// <summary>
        /// 更新调试信息
        /// </summary>
        private void UpdateDebugInfo()
        {
            if (debugText == null) return;
            
            _stringBuilder.Clear();
            
            // 全局信息
            _stringBuilder.AppendLine("=== AI调试信息 ===");
            _stringBuilder.AppendLine($"时间: {Time.time:F2}");
            _stringBuilder.AppendLine($"帧率: {1f / Time.deltaTime:F0}");
            _stringBuilder.AppendLine();
            
            // 获取所有AI
            var allAI = FindObjectsOfType<AIStateMachine>();
            _stringBuilder.AppendLine($"AI数量: {allAI.Length}");
            _stringBuilder.AppendLine();
            
            // 显示每个AI的状态
            foreach (var ai in allAI)
            {
                AppendAIInfo(ai);
            }
            
            debugText.text = _stringBuilder.ToString();
        }
        
        /// <summary>
        /// 添加AI信息
        /// </summary>
        private void AppendAIInfo(AIStateMachine ai)
        {
            if (ai == null) return;
            
            _stringBuilder.AppendLine($"[{ai.gameObject.name}]");
            _stringBuilder.AppendLine($"  状态: {ai.CurrentState}");
            _stringBuilder.AppendLine($"  持续时间: {ai.CurrentStateDuration:F2}s");
            
            // 感知信息
            var perception = ai.GetComponent<AIPerception>();
            if (perception != null)
            {
                _stringBuilder.AppendLine($"  目标: {(perception.HasTarget ? perception.PrimaryTarget?.Target.name : "无")}");
                _stringBuilder.AppendLine($"  检测数: {perception.DetectedTargetCount}");
            }
            
            // 敌人特有信息
            var enemy = ai.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                _stringBuilder.AppendLine($"  生命值: {enemy.CurrentHealth:F0}/{enemy.MaxHealth:F0}");
                _stringBuilder.AppendLine($"  类型: {enemy.Type}");
            }
            
            _stringBuilder.AppendLine();
        }
        
        #endregion

        #region GUI绘制
        
        /// <summary>
        /// 绘制调试GUI
        /// </summary>
        private void DrawDebugGUI()
        {
            if (!showStateLabels) return;
            
            // 绘制所有AI的状态标签
            var allAI = FindObjectsOfType<AIStateMachine>();
            
            foreach (var ai in allAI)
            {
                if (ai == null) continue;
                
                Vector3 screenPos = Camera.main.WorldToScreenPoint(ai.transform.position);
                
                if (screenPos.z > 0)
                {
                    Rect labelRect = new Rect(screenPos.x - 50f, Screen.height - screenPos.y - 50f, 100f, 40f);
                    GUI.color = stateLabelColor;
                    GUI.Label(labelRect, $"{ai.CurrentState}\n{ai.CurrentStateDuration:F1}s");
                }
            }
        }
        
        #endregion

        #region 可视化方法
        
        /// <summary>
        /// 绘制路径线
        /// </summary>
        /// <param name="path">路径点数组</param>
        /// <param name="duration">持续时间</param>
        public void DrawPath(Vector3[] path, float duration = 2f)
        {
            if (!showPathLines || path == null || path.Length < 2) return;
            
            for (int i = 0; i < path.Length - 1; i++)
            {
                UnityEngine.Debug.DrawLine(path[i], path[i + 1], pathLineColor, duration);
            }
        }
        
        /// <summary>
        /// 绘制感知范围
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="radius">半径</param>
        /// <param name="color">颜色</param>
        /// <param name="duration">持续时间</param>
        public void DrawPerceptionRange(Vector3 position, float radius, Color color, float duration = 0.1f)
        {
            if (!showPerceptionRanges) return;
            
            // 绘制圆形
            int segments = 32;
            float angleStep = 360f / segments;
            
            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep * Mathf.Deg2Rad;
                float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;
                
                Vector3 point1 = position + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0) * radius;
                Vector3 point2 = position + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0) * radius;
                
                UnityEngine.Debug.DrawLine(point1, point2, color, duration);
            }
        }
        
        #endregion

        #region 日志方法
        
        /// <summary>
        /// 记录AI状态转换
        /// </summary>
        /// <param name="ai">AI状态机</param>
        /// <param name="from">源状态</param>
        /// <param name="to">目标状态</param>
        public void LogStateTransition(AIStateMachine ai, EnemyState from, EnemyState to)
        {
            if (!_isDebugEnabled) return;
            
            UnityEngine.Debug.Log($"[AI] {ai.gameObject.name}: {from} -> {to}");
        }
        
        /// <summary>
        /// 记录AI事件
        /// </summary>
        /// <param name="ai">AI状态机</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="message">消息</param>
        public void LogAIEvent(AIStateMachine ai, string eventName, string message)
        {
            if (!_isDebugEnabled) return;
            
            UnityEngine.Debug.Log($"[AI:{eventName}] {ai.gameObject.name}: {message}");
        }
        
        #endregion
    }
}
