using UnityEngine;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 击杀反馈控制器 - FB-006
    /// </summary>
    public class KillFeedbackController : MonoBehaviour
    {
        [Header("连击设置")]
        [SerializeField] private float comboResetTime = 3f;       // 连击重置时间
        [SerializeField] private int[] comboMilestones = { 3, 5, 10, 15, 20 }; // 连击里程碑

        [Header("特效")]
        [SerializeField] private GameObject killConfirmEffect;     // 击杀确认特效
        [SerializeField] private GameObject[] comboEffects;        // 连击里程碑特效

        // 状态
        private int currentCombo = 0;
        private float lastKillTime;
        private bool isComboActive = false;

        // 事件
        public System.Action<int> OnComboChanged;      // 连击数变化
        public System.Action<int> OnComboMilestone;    // 达到连击里程碑
        public System.Action OnComboReset;              // 连击重置

        // 属性
        public int CurrentCombo => currentCombo;
        public bool IsComboActive => isComboActive;
        public float ComboProgress => isComboActive ? 
            Mathf.Clamp01((Time.time - lastKillTime) / comboResetTime) : 0;

        private void Update()
        {
            CheckComboReset();
        }

        /// <summary>
        /// 注册击杀
        /// </summary>
        public void RegisterKill(Vector2 position)
        {
            // 检查连击是否继续
            if (Time.time - lastKillTime <= comboResetTime)
            {
                currentCombo++;
            }
            else
            {
                currentCombo = 1;
            }

            lastKillTime = Time.time;
            isComboActive = true;

            // 触发事件
            OnComboChanged?.Invoke(currentCombo);

            // 检查里程碑
            CheckComboMilestone();

            // 播放击杀特效
            PlayKillEffects(position);

            // 触发全局反馈
            CombatFeedback.Instance?.TriggerKillFeedback(position);
        }

        /// <summary>
        /// 检查连击里程碑
        /// </summary>
        private void CheckComboMilestone()
        {
            foreach (int milestone in comboMilestones)
            {
                if (currentCombo == milestone)
                {
                    OnComboMilestone?.Invoke(milestone);
                    PlayComboEffect(milestone);
                    break;
                }
            }
        }

        /// <summary>
        /// 检查连击重置
        /// </summary>
        private void CheckComboReset()
        {
            if (!isComboActive) return;
            if (Time.time - lastKillTime > comboResetTime)
            {
                ResetCombo();
            }
        }

        /// <summary>
        /// 重置连击
        /// </summary>
        public void ResetCombo()
        {
            if (currentCombo > 0)
            {
                OnComboReset?.Invoke();
            }
            
            currentCombo = 0;
            isComboActive = false;
            OnComboChanged?.Invoke(0);
        }

        /// <summary>
        /// 播放击杀特效
        /// </summary>
        private void PlayKillEffects(Vector2 position)
        {
            if (killConfirmEffect != null)
            {
                var effect = Instantiate(killConfirmEffect, position, Quaternion.identity);
                Destroy(effect, 1f);
            }
        }

        /// <summary>
        /// 播放连击特效
        /// </summary>
        private void PlayComboEffect(int milestone)
        {
            int effectIndex = System.Array.IndexOf(comboMilestones, milestone);
            if (effectIndex >= 0 && effectIndex < comboEffects.Length && comboEffects[effectIndex] != null)
            {
                // 在屏幕中央或玩家位置播放
                Vector2 spawnPos = transform.position;
                var effect = Instantiate(comboEffects[effectIndex], spawnPos, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }

        /// <summary>
        /// 获取连击评级
        /// </summary>
        public string GetComboRating()
        {
            if (currentCombo >= 20) return "SSS";
            if (currentCombo >= 15) return "SS";
            if (currentCombo >= 10) return "S";
            if (currentCombo >= 5) return "A";
            if (currentCombo >= 3) return "B";
            return "";
        }

        /// <summary>
        /// 获取连击倍率(可用于分数计算)
        /// </summary>
        public float GetComboMultiplier()
        {
            return 1f + (currentCombo * 0.1f);
        }
    }
}