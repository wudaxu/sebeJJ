using UnityEngine;

namespace SebeJJ.Combat
{
    /// <summary>
    /// EMP波纹特效组件
    /// </summary>
    public class EMPWaveEffect : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer waveRenderer;
        [SerializeField] private ParticleSystem sparkParticles;
        
        private float maxRadius;
        private float expandSpeed;
        private float duration;
        private float elapsedTime = 0f;
        private bool isInitialized = false;

        public void Initialize(float radius, float speed, float lifeDuration)
        {
            maxRadius = radius;
            expandSpeed = speed;
            duration = lifeDuration;
            isInitialized = true;
            
            // 初始化大小
            transform.localScale = Vector3.zero;
            
            // 设置颜色
            if (waveRenderer != null)
            {
                waveRenderer.color = new Color(0.3f, 0.7f, 1f, 0.8f);
            }
            
            // 播放粒子
            if (sparkParticles != null)
            {
                sparkParticles.Play();
            }
        }

        private void Update()
        {
            if (!isInitialized) return;
            
            elapsedTime += Time.deltaTime;
            
            // 计算当前半径
            float currentRadius = Mathf.Min(elapsedTime * expandSpeed, maxRadius);
            float normalizedTime = elapsedTime / duration;
            
            // 更新大小
            transform.localScale = Vector3.one * (currentRadius * 2f);
            
            // 淡出效果
            if (waveRenderer != null)
            {
                Color color = waveRenderer.color;
                color.a = Mathf.Lerp(0.8f, 0f, normalizedTime);
                waveRenderer.color = color;
            }
            
            // 销毁
            if (elapsedTime >= duration)
            {
                Destroy(gameObject);
            }
        }
    }
}