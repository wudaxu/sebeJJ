using UnityEngine;
using System.Collections;

namespace SebeJJ.Upgrade.UI
{
    /// <summary>
    /// 升级动画控制器
    /// </summary>
    public class UpgradeAnimationController : MonoBehaviour
    {
        [Header("特效预制体")]
        public GameObject upgradeSuccessEffect;
        public GameObject upgradeFailEffect;
        public GameObject levelUpEffect;
        
        [Header("音效")]
        public AudioClip upgradeSuccessSound;
        public AudioClip upgradeFailSound;
        public AudioClip levelUpSound;
        
        [Header("动画设置")]
        public float effectDuration = 2f;
        public float cameraShakeDuration = 0.3f;
        public float cameraShakeIntensity = 0.1f;
        
        private Camera mainCamera;
        
        private void Start()
        {
            mainCamera = Camera.main;
        }
        
        /// <summary>
        /// 播放升级成功动画
        /// </summary>
        public void PlayUpgradeAnimation(UpgradeNodeData nodeData)
        {
            // 播放音效
            if (upgradeSuccessSound != null)
            {
                AudioSource.PlayClipAtPoint(upgradeSuccessSound, mainCamera.transform.position);
            }
            
            // 实例化特效
            if (upgradeSuccessEffect != null)
            {
                var effect = Instantiate(upgradeSuccessEffect, transform.position, Quaternion.identity);
                Destroy(effect, effectDuration);
            }
            
            // 相机震动
            StartCoroutine(ShakeCamera());
            
            // 播放等级提升特效
            if (levelUpEffect != null)
            {
                var levelEffect = Instantiate(levelUpEffect, transform.position, Quaternion.identity);
                Destroy(levelEffect, effectDuration * 1.5f);
            }
            
            // 播放升级音效
            if (levelUpSound != null)
            {
                AudioSource.PlayClipAtPoint(levelUpSound, mainCamera.transform.position, 0.7f);
            }
        }
        
        /// <summary>
        /// 播放升级失败动画
        /// </summary>
        public void PlayUpgradeFailAnimation()
        {
            // 播放音效
            if (upgradeFailSound != null)
            {
                AudioSource.PlayClipAtPoint(upgradeFailSound, mainCamera.transform.position);
            }
            
            // 实例化特效
            if (upgradeFailEffect != null)
            {
                var effect = Instantiate(upgradeFailEffect, transform.position, Quaternion.identity);
                Destroy(effect, effectDuration);
            }
            
            // 屏幕抖动（错误提示）
            StartCoroutine(ShakeScreen());
        }
        
        /// <summary>
        /// 相机震动
        /// </summary>
        private IEnumerator ShakeCamera()
        {
            if (mainCamera == null) yield break;
            
            Vector3 originalPosition = mainCamera.transform.position;
            float elapsed = 0f;
            
            while (elapsed < cameraShakeDuration)
            {
                float x = Random.Range(-1f, 1f) * cameraShakeIntensity;
                float y = Random.Range(-1f, 1f) * cameraShakeIntensity;
                
                mainCamera.transform.position = originalPosition + new Vector3(x, y, 0);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            mainCamera.transform.position = originalPosition;
        }
        
        /// <summary>
        /// 屏幕抖动（错误提示）
        /// </summary>
        private IEnumerator ShakeScreen()
        {
            if (mainCamera == null) yield break;
            
            Vector3 originalPosition = mainCamera.transform.position;
            float elapsed = 0f;
            float shakeDuration = 0.2f;
            float shakeIntensity = 0.05f;
            
            while (elapsed < shakeDuration)
            {
                // 水平抖动
                float x = Mathf.Sin(elapsed * 50f) * shakeIntensity;
                
                mainCamera.transform.position = originalPosition + new Vector3(x, 0, 0);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            mainCamera.transform.position = originalPosition;
        }
        
        /// <summary>
        /// 播放材料消耗动画
        /// </summary>
        public void PlayMaterialConsumeAnimation(Vector3 startPosition, string materialId, int amount)
        {
            // 可以在这里实现材料飞入动画
            // 从材料图标位置飞向升级按钮位置
        }
        
        /// <summary>
        /// 播放升级解锁动画
        /// </summary>
        public void PlayUnlockAnimation(Vector3 position)
        {
            if (upgradeSuccessEffect != null)
            {
                var effect = Instantiate(upgradeSuccessEffect, position, Quaternion.identity);
                
                // 调整特效大小
                var particles = effect.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    var main = particles.main;
                    main.startSize = main.startSize.constant * 0.5f;
                }
                
                Destroy(effect, effectDuration);
            }
        }
    }
}