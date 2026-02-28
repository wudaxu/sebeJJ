using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Utils
{
    /// <summary>
    /// Boss特效管理器 - 统一管理Boss战的各种特效
    /// </summary>
    public class BossEffectManager : MonoBehaviour
    {
        public static BossEffectManager Instance { get; private set; }
        
        [Header("冲撞攻击特效")]
        public GameObject chargeWarningLinePrefab;      // 预警红线
        public GameObject chargeImpactPrefab;           // 撞击特效
        public float chargeWarningLineDuration = 1.5f;
        
        [Header("地震波特效")]
        public GameObject earthquakeWavePrefab;         // 地震波扩散特效
        public GameObject earthquakeImpactPrefab;       // 地面冲击特效
        public int earthquakeWaveCount = 3;
        public float earthquakeWaveInterval = 0.3f;
        
        [Header("激光特效")]
        public GameObject laserBeamPrefab;              // 激光束
        public GameObject laserChargePrefab;            // 激光充能
        
        [Header("召唤特效")]
        public GameObject summonCirclePrefab;           // 召唤阵
        public GameObject summonPortalPrefab;           // 召唤传送门
        
        [Header("阶段转换特效")]
        public GameObject phaseTransitionPrefab;        // 阶段转换
        public GameObject enrageEffectPrefab;           // 狂暴特效
        
        [Header("弱点特效")]
        public GameObject weakPointMarkerPrefab;        // 弱点标记
        public GameObject weakPointGlowPrefab;          // 弱点发光
        
        [Header("伤害数字")]
        public GameObject damageNumberPrefab;           // 伤害数字
        public Transform damageNumberCanvas;            // 伤害数字画布
        
        // 对象池
        private Dictionary<string, Queue<GameObject>> effectPools = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<GameObject, string> activeEffects = new Dictionary<GameObject, string>();
        
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        #region 冲撞攻击特效
        
        /// <summary>
        /// 显示冲撞预警线
        /// </summary>
    public void ShowChargeWarningLine(Vector3 startPos, Vector3 direction, float distance, float duration)
        {
            if (chargeWarningLinePrefab == null) return;
            
            GameObject warningLine = GetFromPool("chargeWarning", chargeWarningLinePrefab);
            warningLine.transform.position = startPos;
            warningLine.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
            
            // 设置线长度
            LineRenderer lineRenderer = warningLine.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, Vector3.zero);
                lineRenderer.SetPosition(1, Vector3.right * distance);
            }
            
            // 闪烁效果
            StartCoroutine(WarningLineFlash(warningLine, duration));
        }
        
        private IEnumerator WarningLineFlash(GameObject warningLine, float duration)
        {
            SpriteRenderer sr = warningLine.GetComponentInChildren<SpriteRenderer>();
            float timer = 0f;
            
            while (timer < duration)
            {
                timer += Time.deltaTime;
                if (sr != null)
                {
                    float alpha = Mathf.PingPong(timer * 5f, 1f);
                    sr.color = new Color(1f, 0f, 0f, alpha);
                }
                yield return null;
            }
            
            ReturnToPool("chargeWarning", warningLine);
        }
        
        /// <summary>
        /// 播放冲撞撞击特效
        /// </summary>
    public void PlayChargeImpact(Vector3 position, Vector3 normal)
        {
            if (chargeImpactPrefab == null) return;
            
            GameObject impact = GetFromPool("chargeImpact", chargeImpactPrefab);
            impact.transform.position = position;
            impact.transform.rotation = Quaternion.LookRotation(normal);
            
            // 播放粒子
            ParticleSystem ps = impact.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                StartCoroutine(ReturnToPoolAfterDelay("chargeImpact", impact, ps.main.duration));
            }
            
            // 屏幕震动
            CameraShake?.Invoke(0.3f, 0.2f);
        }
        
        #endregion
        
        #region 地震波特效
        
        /// <summary>
        /// 播放地震波特效
        /// </summary>
    public void PlayEarthquakeWave(Vector3 center, float radius, int waveCount = 3)
        {
            StartCoroutine(EarthquakeWaveCoroutine(center, radius, waveCount));
        }
        
        private IEnumerator EarthquakeWaveCoroutine(Vector3 center, float maxRadius, int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject wave = GetFromPool("earthquake", earthquakeWavePrefab);
                wave.transform.position = center;
                
                // 扩散动画
                StartCoroutine(ExpandWave(wave, maxRadius, 1f));
                
                yield return new WaitForSeconds(earthquakeWaveInterval);
            }
        }
        
        private IEnumerator ExpandWave(GameObject wave, float maxRadius, float duration)
        {
            Transform waveTransform = wave.transform;
            Vector3 startScale = Vector3.zero;
            Vector3 endScale = Vector3.one * maxRadius;
            
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                waveTransform.localScale = Vector3.Lerp(startScale, endScale, t);
                
                // 淡出
                SpriteRenderer sr = wave.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = new Color(1f, 0.5f, 0f, 1f - t);
                }
                
                yield return null;
            }
            
            ReturnToPool("earthquake", wave);
        }
        
        #endregion
        
        #region 伤害数字
        
        /// <summary>
        /// 显示伤害数字
        /// </summary>
    public void ShowDamageNumber(Vector3 worldPosition, float damage, bool isCritical = false)
        {
            if (damageNumberPrefab == null) return;
            
            // 转换到屏幕坐标
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            
            GameObject damageObj = Instantiate(damageNumberPrefab, damageNumberCanvas);
            damageObj.transform.position = screenPos;
            
            // 设置数值
            TMPro.TextMeshProUGUI text = damageObj.GetComponent<TMPro.TextMeshProUGUI>();
            if (text != null)
            {
                text.text = Mathf.RoundToInt(damage).ToString();
                text.color = isCritical ? Color.red : Color.white;
                if (isCritical)
                {
                    text.fontSize *= 1.5f;
                    text.text += "!";
                }
            }
            
            // 飘动动画
            StartCoroutine(FloatDamageNumber(damageObj));
        }
        
        private IEnumerator FloatDamageNumber(GameObject damageObj)
        {
            RectTransform rect = damageObj.GetComponent<RectTransform>();
            Vector2 startPos = rect.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(Random.Range(-30f, 30f), 50f);
            
            float timer = 0f;
            float duration = 1f;
            
            CanvasGroup canvasGroup = damageObj.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = damageObj.AddComponent<CanvasGroup>();
            
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                
                rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                canvasGroup.alpha = 1f - t;
                
                yield return null;
            }
            
            Destroy(damageObj);
        }
        
        #endregion
        
        #region 对象池
        
        private GameObject GetFromPool(string poolName, GameObject prefab)
        {
            if (!effectPools.ContainsKey(poolName))
            {
                effectPools[poolName] = new Queue<GameObject>();
            }
            
            if (effectPools[poolName].Count > 0)
            {
                GameObject obj = effectPools[poolName].Dequeue();
                obj.SetActive(true);
                activeEffects[obj] = poolName;
                return obj;
            }
            
            GameObject newObj = Instantiate(prefab, transform);
            activeEffects[newObj] = poolName;
            return newObj;
        }
        
        private void ReturnToPool(string poolName, GameObject obj)
        {
            obj.SetActive(false);
            
            if (!effectPools.ContainsKey(poolName))
            {
                effectPools[poolName] = new Queue<GameObject>();
            }
            
            effectPools[poolName].Enqueue(obj);
            activeEffects.Remove(obj);
        }
        
        private IEnumerator ReturnToPoolAfterDelay(string poolName, GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnToPool(poolName, obj);
        }
        
        #endregion
        
        // 相机震动事件
        public static System.Action<float, float> CameraShake;
    }
}
