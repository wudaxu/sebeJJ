using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 战斗预警系统 - CB-001
    /// 提供敌人攻击预警、危险提示等功能
    /// </summary>
    public class CombatWarningSystem : MonoBehaviour
    {
        public static CombatWarningSystem Instance { get; private set; }
        
        [Header("预警设置")]
        [SerializeField] private float defaultWarningTime = 0.5f;    // 默认预警时间
        [SerializeField] private float minWarningTime = 0.2f;        // 最小预警时间
        [SerializeField] private float maxWarningTime = 1.0f;        // 最大预警时间
        
        [Header("视觉预警")]
        [SerializeField] private Color warningColor = Color.red;
        [SerializeField] private float warningFlashRate = 5f;        // 闪烁频率
        [SerializeField] private float warningOpacity = 0.5f;        // 预警透明度
        
        [Header("声音预警")]
        [SerializeField] private AudioClip warningSound;
        [SerializeField] private float warningSoundVolume = 0.7f;
        
        [Header("UI预警")]
        [SerializeField] private bool showScreenEdgeWarning = true;  // 屏幕边缘预警
        [SerializeField] private float screenEdgeWarningDistance = 0.1f; // 边缘距离
        
        // 运行时状态
        private List<ActiveWarning> activeWarnings = new List<ActiveWarning>();
        private Dictionary<Transform, SpriteRenderer> enemyRenderers = new Dictionary<Transform, SpriteRenderer>();
        
        // 事件
        public event Action<Transform, WarningType> OnWarningTriggered;
        public event Action<Transform> OnWarningEnded;
        
        // 预警类型
        public enum WarningType
        {
            Attack,         // 攻击预警
            Charge,         // 蓄力预警
            RangedLock,     // 远程锁定
            DangerZone,     // 危险区域
            ScreenEdge      // 屏幕边缘
        }
        
        // 活跃预警数据
        private class ActiveWarning
        {
            public Transform Source;
            public WarningType Type;
            public float Duration;
            public float Elapsed;
            public Color OriginalColor;
            public SpriteRenderer Renderer;
        }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        /// <summary>
        /// 触发预警
        /// </summary>
        public void TriggerWarning(Transform source, WarningType type, float duration = -1)
        {
            if (source == null) return;
            
            float warningDuration = duration > 0 ? duration : defaultWarningTime;
            warningDuration = Mathf.Clamp(warningDuration, minWarningTime, maxWarningTime);
            
            // 检查是否已有同类型预警
            var existing = activeWarnings.Find(w => w.Source == source && w.Type == type);
            if (existing != null)
            {
                existing.Duration = warningDuration;
                existing.Elapsed = 0f;
                return;
            }
            
            // 获取渲染器
            SpriteRenderer renderer = GetEnemyRenderer(source);
            Color originalColor = renderer != null ? renderer.color : Color.white;
            
            // 创建新预警
            var warning = new ActiveWarning
            {
                Source = source,
                Type = type,
                Duration = warningDuration,
                Elapsed = 0f,
                OriginalColor = originalColor,
                Renderer = renderer
            };
            
            activeWarnings.Add(warning);
            
            // 播放声音
            if (warningSound != null)
            {
                AudioSource.PlayClipAtPoint(warningSound, source.position, warningSoundVolume);
            }
            
            OnWarningTriggered?.Invoke(source, type);
            
            // 启动预警效果
            StartCoroutine(WarningEffectCoroutine(warning));
        }
        
        /// <summary>
        /// 预警效果协程
        /// </summary>
        private IEnumerator WarningEffectCoroutine(ActiveWarning warning)
        {
            while (warning.Elapsed < warning.Duration)
            {
                warning.Elapsed += Time.deltaTime;
                float progress = warning.Elapsed / warning.Duration;
                
                // 闪烁效果
                if (warning.Renderer != null)
                {
                    float flash = Mathf.PingPong(warning.Elapsed * warningFlashRate, 1f);
                    Color flashColor = Color.Lerp(warning.OriginalColor, warningColor, flash * warningOpacity);
                    warning.Renderer.color = flashColor;
                }
                
                yield return null;
            }
            
            // 恢复颜色
            if (warning.Renderer != null)
            {
                warning.Renderer.color = warning.OriginalColor;
            }
            
            // 移除预警
            activeWarnings.Remove(warning);
            OnWarningEnded?.Invoke(warning.Source);
        }
        
        /// <summary>
        /// 获取敌人渲染器
        /// </summary>
        private SpriteRenderer GetEnemyRenderer(Transform enemy)
        {
            if (enemyRenderers.TryGetValue(enemy, out var renderer))
            {
                return renderer;
            }
            
            renderer = enemy.GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                enemyRenderers[enemy] = renderer;
            }
            
            return renderer;
        }
        
        /// <summary>
        /// 取消预警
        /// </summary>
        public void CancelWarning(Transform source)
        {
            var warnings = activeWarnings.FindAll(w => w.Source == source);
            foreach (var warning in warnings)
            {
                if (warning.Renderer != null)
                {
                    warning.Renderer.color = warning.OriginalColor;
                }
                activeWarnings.Remove(warning);
            }
        }
        
        /// <summary>
        /// 检查是否有活跃预警
        /// </summary>
        public bool HasActiveWarning(Transform source)
        {
            return activeWarnings.Exists(w => w.Source == source);
        }
        
        /// <summary>
        /// 获取预警进度
        /// </summary>
        public float GetWarningProgress(Transform source)
        {
            var warning = activeWarnings.Find(w => w.Source == source);
            if (warning == null) return 0f;
            
            return Mathf.Clamp01(warning.Elapsed / warning.Duration);
        }
        
        /// <summary>
        /// 清除所有预警
        /// </summary>
        public void ClearAllWarnings()
        {
            foreach (var warning in activeWarnings)
            {
                if (warning.Renderer != null)
                {
                    warning.Renderer.color = warning.OriginalColor;
                }
            }
            activeWarnings.Clear();
        }
        
        private void OnDestroy()
        {
            ClearAllWarnings();
        }
    }
}
