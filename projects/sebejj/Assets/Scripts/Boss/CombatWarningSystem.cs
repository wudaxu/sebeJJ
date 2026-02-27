/**
 * @file CombatWarningSystem.cs
 * @brief 战斗预警系统
 * @description 为Boss攻击提供视觉预警
 * @author Boss战设计师
 * @date 2026-02-27
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Boss
{
    /// <summary>
    /// 预警类型
    /// </summary>
    public enum WarningType
    {
        Attack,     // 普通攻击
        Charge,     // 冲撞
        Laser,      // 激光
        Earthquake, // 地震
        Area,       // 范围攻击
        Danger      // 危险区域
    }

    /// <summary>
    /// 战斗预警系统
    /// </summary>
    public class CombatWarningSystem : MonoBehaviour
    {
        public static CombatWarningSystem Instance { get; private set; }

        [Header("=== 预警预制体 ===")]
        [SerializeField] private GameObject attackWarningPrefab;
        [SerializeField] private GameObject chargeWarningPrefab;
        [SerializeField] private GameObject laserWarningPrefab;
        [SerializeField] private GameObject earthquakeWarningPrefab;
        [SerializeField] private GameObject areaWarningPrefab;
        
        [Header("=== 预警设置 ===")]
        [SerializeField] private float defaultWarningDuration = 1f;
        [SerializeField] private Color warningColor = new Color(1f, 0.3f, 0.3f, 0.7f);
        [SerializeField] private Color dangerColor = new Color(1f, 0f, 0f, 0.9f);
        
        [Header("=== 音频 ===")]
        [SerializeField] private AudioClip warningSound;
        
        private List<GameObject> _activeWarnings = new List<GameObject>();

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
        public void TriggerWarning(Transform source, WarningType type, float duration)
        {
            GameObject warningObj = CreateWarning(source, type);
            if (warningObj != null)
            {
                StartCoroutine(RemoveWarningCoroutine(warningObj, duration));
                
                // 播放预警音效
                if (warningSound != null)
                {
                    AudioManager.Instance?.PlaySFX(warningSound);
                }
            }
        }

        /// <summary>
        /// 创建预警
        /// </summary>
        private GameObject CreateWarning(Transform source, WarningType type)
        {
            GameObject prefab = null;
            
            switch (type)
            {
                case WarningType.Attack:
                    prefab = attackWarningPrefab;
                    break;
                case WarningType.Charge:
                    prefab = chargeWarningPrefab;
                    break;
                case WarningType.Laser:
                    prefab = laserWarningPrefab;
                    break;
                case WarningType.Earthquake:
                    prefab = earthquakeWarningPrefab;
                    break;
                case WarningType.Area:
                case WarningType.Danger:
                    prefab = areaWarningPrefab;
                    break;
            }
            
            if (prefab == null)
            {
                // 如果没有预制体，使用默认可视化
                return CreateDefaultWarning(source, type);
            }
            
            GameObject warning = Instantiate(prefab, source.position, source.rotation);
            warning.transform.SetParent(source);
            _activeWarnings.Add(warning);
            
            return warning;
        }

        /// <summary>
        /// 创建默认预警（使用Debug绘制）
        /// </summary>
        private GameObject CreateDefaultWarning(Transform source, WarningType type)
        {
            GameObject warningObj = new GameObject($"Warning_{type}");
            warningObj.transform.SetParent(source);
            warningObj.transform.localPosition = Vector3.zero;
            
            var visual = warningObj.AddComponent<WarningVisual>();
            visual.Initialize(type, warningColor);
            
            _activeWarnings.Add(warningObj);
            
            return warningObj;
        }

        /// <summary>
        /// 清除所有预警
        /// </summary>
        public void ClearAllWarnings()
        {
            foreach (var warning in _activeWarnings)
            {
                if (warning != null)
                {
                    Destroy(warning);
                }
            }
            _activeWarnings.Clear();
        }

        private IEnumerator RemoveWarningCoroutine(GameObject warning, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (warning != null)
            {
                _activeWarnings.Remove(warning);
                Destroy(warning);
            }
        }
    }

    /// <summary>
    /// 预警可视化组件
    /// </summary>
    public class WarningVisual : MonoBehaviour
    {
        private WarningType _type;
        private Color _color;
        private LineRenderer _lineRenderer;
        private SpriteRenderer _spriteRenderer;

        public void Initialize(WarningType type, Color color)
        {
            _type = type;
            _color = color;
            
            CreateVisual();
        }

        private void CreateVisual()
        {
            switch (_type)
            {
                case WarningType.Charge:
                    CreateChargeWarning();
                    break;
                case WarningType.Laser:
                    CreateLaserWarning();
                    break;
                case WarningType.Earthquake:
                    CreateEarthquakeWarning();
                    break;
                default:
                    CreateDefaultVisual();
                    break;
            }
        }

        private void CreateChargeWarning()
        {
            // 创建冲撞预警线
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.startWidth = 0.2f;
            _lineRenderer.endWidth = 0.2f;
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _lineRenderer.startColor = _color;
            _lineRenderer.endColor = _color;
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(0, Vector3.zero);
            _lineRenderer.SetPosition(1, Vector3.right * 10f);
            
            // 添加闪烁动画
            StartCoroutine(FlashCoroutine());
        }

        private void CreateLaserWarning()
        {
            // 创建激光预警扇形
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.startWidth = 0.1f;
            _lineRenderer.endWidth = 0.1f;
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _lineRenderer.startColor = Color.yellow;
            _lineRenderer.endColor = Color.red;
            
            // 绘制扇形边界
            int segments = 20;
            _lineRenderer.positionCount = segments * 2;
            
            float angle = 60f;
            float radius = 15f;
            
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / (segments - 1);
                float currentAngle = Mathf.Lerp(-angle / 2f, angle / 2f, t);
                Vector3 pos = Quaternion.Euler(0, 0, currentAngle) * Vector3.right * radius;
                
                _lineRenderer.SetPosition(i * 2, Vector3.zero);
                _lineRenderer.SetPosition(i * 2 + 1, pos);
            }
            
            StartCoroutine(FlashCoroutine());
        }

        private void CreateEarthquakeWarning()
        {
            // 创建地震预警（地面震动指示）
            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            // 创建一个简单的圆形精灵或纹理
            _spriteRenderer.color = new Color(_color.r, _color.g, _color.b, 0.3f);
            
            // 缩放动画
            StartCoroutine(PulseScaleCoroutine());
        }

        private void CreateDefaultVisual()
        {
            // 默认预警（红色圆圈）
            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            _spriteRenderer.color = _color;
            
            StartCoroutine(FlashCoroutine());
        }

        private IEnumerator FlashCoroutine()
        {
            float flashSpeed = 5f;
            
            while (true)
            {
                float alpha = 0.3f + Mathf.PingPong(Time.time * flashSpeed, 0.7f);
                
                if (_lineRenderer != null)
                {
                    _lineRenderer.startColor = new Color(_color.r, _color.g, _color.b, alpha);
                    _lineRenderer.endColor = new Color(_color.r, _color.g, _color.b, alpha);
                }
                
                if (_spriteRenderer != null)
                {
                    _spriteRenderer.color = new Color(_color.r, _color.g, _color.b, alpha);
                }
                
                yield return null;
            }
        }

        private IEnumerator PulseScaleCoroutine()
        {
            Vector3 baseScale = transform.localScale;
            
            while (true)
            {
                float scale = 1f + Mathf.PingPong(Time.time * 2f, 0.3f);
                transform.localScale = baseScale * scale;
                yield return null;
            }
        }
    }
}
