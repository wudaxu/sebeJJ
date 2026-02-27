using UnityEngine;
using System.Collections;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 战斗反馈系统 - FB-001~006
    /// 单例模式，提供全局访问
    /// </summary>
    public class CombatFeedback : MonoBehaviour
    {
        public static CombatFeedback Instance { get; private set; }

        [Header("屏幕震动 - FB-001")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float defaultShakeDuration = 0.2f;
        [SerializeField] private float defaultShakeIntensity = 0.3f;

        [Header("时间缩放 - FB-002")]
        [SerializeField] private float defaultTimeScale = 0.3f;
        [SerializeField] private float defaultTimeScaleDuration = 0.1f;

        [Header("命中停顿 - FB-003")]
        [SerializeField] private float defaultHitStopDuration = 0.05f;

        [Header("命中特效 - FB-004")]
        [SerializeField] private GameObject defaultHitEffect;
        [SerializeField] private GameObject criticalHitEffect;
        [SerializeField] private GameObject shieldHitEffect;

        [Header("伤害数字 - DM-004")]
        [SerializeField] private DamageNumber damageNumberPrefab;

        // 状态
        private Vector3 cameraOriginalPosition;
        private Coroutine currentShakeCoroutine;
        private Coroutine currentTimeScaleCoroutine;
        private Coroutine currentHitStopCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (mainCamera == null)
                mainCamera = Camera.main;
        }

        #region 屏幕震动 - FB-001

        /// <summary>
        /// 触发屏幕震动
        /// </summary>
        public void TriggerScreenShake(float intensity = -1, float duration = -1)
        {
            float shakeIntensity = intensity > 0 ? intensity : defaultShakeIntensity;
            float shakeDuration = duration > 0 ? duration : defaultShakeDuration;

            if (currentShakeCoroutine != null)
                StopCoroutine(currentShakeCoroutine);

            currentShakeCoroutine = StartCoroutine(ScreenShakeCoroutine(shakeIntensity, shakeDuration));
        }

        private IEnumerator ScreenShakeCoroutine(float intensity, float duration)
        {
            if (mainCamera == null) yield break;

            cameraOriginalPosition = mainCamera.transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float damper = 1f - (elapsed / duration);
                
                Vector3 shakeOffset = Random.insideUnitSphere * intensity * damper;
                shakeOffset.z = 0; // 2D游戏不震动Z轴

                mainCamera.transform.localPosition = cameraOriginalPosition + shakeOffset;
                yield return null;
            }

            mainCamera.transform.localPosition = cameraOriginalPosition;
        }

        #endregion

        #region 时间缩放 - FB-002

        /// <summary>
        /// 触发时间缩放(慢动作)
        /// </summary>
        public void TriggerTimeScale(float timeScale = -1, float duration = -1)
        {
            float targetScale = timeScale > 0 ? timeScale : defaultTimeScale;
            float scaleDuration = duration > 0 ? duration : defaultTimeScaleDuration;

            if (currentTimeScaleCoroutine != null)
                StopCoroutine(currentTimeScaleCoroutine);

            currentTimeScaleCoroutine = StartCoroutine(TimeScaleCoroutine(targetScale, scaleDuration));
        }

        private IEnumerator TimeScaleCoroutine(float targetScale, float duration)
        {
            Time.timeScale = targetScale;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
        }

        #endregion

        #region 命中停顿 - FB-003

        /// <summary>
        /// 触发命中停顿(顿帧效果)
        /// </summary>
        public void TriggerHitStop(float duration = -1)
        {
            float stopDuration = duration > 0 ? duration : defaultHitStopDuration;

            if (currentHitStopCoroutine != null)
                StopCoroutine(currentHitStopCoroutine);

            currentHitStopCoroutine = StartCoroutine(HitStopCoroutine(stopDuration));
        }

        private IEnumerator HitStopCoroutine(float duration)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
        }

        #endregion

        #region 命中特效 - FB-004

        /// <summary>
        /// 在位置生成命中特效
        /// </summary>
        public void SpawnHitEffect(Vector2 position, bool isCritical = false, bool isShieldHit = false)
        {
            GameObject effectPrefab = null;

            if (isShieldHit && shieldHitEffect != null)
                effectPrefab = shieldHitEffect;
            else if (isCritical && criticalHitEffect != null)
                effectPrefab = criticalHitEffect;
            else
                effectPrefab = defaultHitEffect;

            if (effectPrefab != null)
            {
                var effect = Instantiate(effectPrefab, position, Quaternion.identity);
                Destroy(effect, 1f);
            }
        }

        /// <summary>
        /// 生成自定义特效
        /// </summary>
        public void SpawnCustomEffect(GameObject effectPrefab, Vector2 position, Quaternion rotation)
        {
            if (effectPrefab != null)
            {
                var effect = Instantiate(effectPrefab, position, rotation);
                Destroy(effect, 2f);
            }
        }

        #endregion

        #region 伤害数字 - DM-004

        /// <summary>
        /// 显示伤害数字
        /// </summary>
        public void ShowDamageNumber(float damage, Vector2 position, bool isCritical = false, 
            bool isHeal = false)
        {
            if (damageNumberPrefab == null) return;

            var damageNumber = Instantiate(damageNumberPrefab, position, Quaternion.identity);
            damageNumber.Initialize(damage, isCritical, isHeal);
        }

        #endregion

        #region 综合反馈

        /// <summary>
        /// 触发受击反馈(震动+停顿)
        /// </summary>
        public void TriggerImpactFeedback(float shakeIntensity = 0.3f, float hitStopDuration = 0.05f)
        {
            TriggerScreenShake(shakeIntensity);
            TriggerHitStop(hitStopDuration);
        }

        /// <summary>
        /// 触发暴击反馈(强震动+慢动作+特效)
        /// </summary>
        public void TriggerCriticalFeedback(Vector2 position)
        {
            TriggerScreenShake(0.5f, 0.3f);
            TriggerTimeScale(0.2f, 0.15f);
            SpawnHitEffect(position, true);
        }

        /// <summary>
        /// 触发击杀反馈 - FB-006
        /// </summary>
        public void TriggerKillFeedback(Vector2 position)
        {
            TriggerScreenShake(0.4f, 0.25f);
            TriggerTimeScale(0.3f, 0.2f);
            // 可以在这里添加击杀确认音效/特效
        }

        /// <summary>
        /// 触发护盾击破反馈
        /// </summary>
        public void TriggerShieldBreakFeedback(Vector2 position)
        {
            TriggerScreenShake(0.35f, 0.2f);
            TriggerHitStop(0.08f);
            SpawnHitEffect(position, false, true);
        }

        #endregion
    }
}