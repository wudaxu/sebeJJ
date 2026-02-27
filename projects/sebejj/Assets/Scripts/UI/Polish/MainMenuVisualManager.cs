using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

namespace SebeJJ.UI.Polish
{
    /// <summary>
    /// 主菜单视觉管理器 - 动态背景、粒子效果、赛博朋克风格
    /// </summary>
    public class MainMenuVisualManager : MonoBehaviour
    {
        public static MainMenuVisualManager Instance { get; private set; }
        
        [Header("动态背景")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Material cyberGridMaterial;
        [SerializeField] private Material nebulaMaterial;
        
        [Header("粒子系统")]
        [SerializeField] private ParticleSystem ambientParticles;
        [SerializeField] private ParticleSystem energyParticles;
        [SerializeField] private ParticleSystem dataStreamParticles;
        
        [Header("装饰元素")]
        [SerializeField] private RectTransform cornerDecorations;
        [SerializeField] private RectTransform scanLines;
        [SerializeField] private RectTransform vignetteOverlay;
        
        [Header("动画设置")]
        [SerializeField] private float gridScrollSpeed = 0.5f;
        [SerializeField] private float colorShiftSpeed = 0.2f;
        [SerializeField] private float particleEmissionRate = 50f;
        
        [Header("赛博朋克配色")]
        [SerializeField] private Color primaryColor = new Color(0.2f, 0.9f, 1f, 1f);
        [SerializeField] private Color secondaryColor = new Color(0.8f, 0.2f, 0.9f, 1f);
        [SerializeField] private Color accentColor = new Color(1f, 0.6f, 0.1f, 1f);
        
        private Material runtimeGridMaterial;
        private Material runtimeNebulaMaterial;
        private float time;
        private bool isActive = true;
        
        // 对象池
        private Queue<GameObject> particlePool = new Queue<GameObject>();
        private const int MAX_POOL_SIZE = 50;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            InitializeMaterials();
            InitializeParticles();
            InitializeDecorations();
            StartAmbientAnimations();
        }
        
        private void Update()
        {
            if (!isActive) return;
            
            time += Time.deltaTime;
            UpdateBackgroundEffects();
            UpdateParticleEffects();
        }
        
        /// <summary>
        /// 初始化材质
        /// </summary>
        private void InitializeMaterials()
        {
            if (cyberGridMaterial != null)
            {
                runtimeGridMaterial = new Material(cyberGridMaterial);
                runtimeGridMaterial.SetColor("_PrimaryColor", primaryColor);
                runtimeGridMaterial.SetColor("_SecondaryColor", secondaryColor);
                runtimeGridMaterial.SetFloat("_GridSpeed", gridScrollSpeed);
            }
            
            if (nebulaMaterial != null)
            {
                runtimeNebulaMaterial = new Material(nebulaMaterial);
                runtimeNebulaMaterial.SetColor("_Color1", primaryColor.WithAlpha(0.3f));
                runtimeNebulaMaterial.SetColor("_Color2", secondaryColor.WithAlpha(0.2f));
            }
            
            if (backgroundImage != null && runtimeNebulaMaterial != null)
            {
                backgroundImage.material = runtimeNebulaMaterial;
            }
        }
        
        /// <summary>
        /// 初始化粒子系统
        /// </summary>
        private void InitializeParticles()
        {
            // 环境粒子 - 漂浮的数据碎片
            if (ambientParticles != null)
            {
                var main = ambientParticles.main;
                main.startColor = primaryColor.WithAlpha(0.6f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
                main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 10f);
                
                var emission = ambientParticles.emission;
                emission.rateOverTime = particleEmissionRate * 0.5f;
                
                var velocity = ambientParticles.velocityOverLifetime;
                velocity.enabled = true;
                velocity.speedModifier = 0.5f;
            }
            
            // 能量粒子 - 流动的能量流
            if (energyParticles != null)
            {
                var main = energyParticles.main;
                main.startColor = new ParticleSystem.MinMaxGradient(accentColor, secondaryColor);
                main.startSize = 0.05f;
                main.startSpeed = 2f;
                
                var trails = energyParticles.trails;
                trails.enabled = true;
                trails.colorOverTrail = accentColor.WithAlpha(0.5f);
            }
            
            // 数据流粒子 - 代码雨效果
            if (dataStreamParticles != null)
            {
                var main = dataStreamParticles.main;
                main.startColor = primaryColor.WithAlpha(0.8f);
                main.gravityModifier = 0.5f;
                
                var shape = dataStreamParticles.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.scale = new Vector3(20f, 1f, 10f);
            }
        }
        
        /// <summary>
        /// 初始化装饰元素
        /// </summary>
        private void InitializeDecorations()
        {
            // 角落装饰动画
            if (cornerDecorations != null)
            {
                foreach (Transform corner in cornerDecorations)
                {
                    corner.DOScale(1.1f, 2f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo);
                    
                    corner.DORotate(new Vector3(0, 0, 5f), 3f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo);
                }
            }
            
            // 扫描线效果
            if (scanLines != null)
            {
                var scanLineImages = scanLines.GetComponentsInChildren<Image>();
                foreach (var line in scanLineImages)
                {
                    line.color = primaryColor.WithAlpha(0.05f);
                    
                    line.rectTransform.DOAnchorPosY(-1080f, 3f)
                        .SetEase(Ease.Linear)
                        .SetLoops(-1, LoopType.Restart)
                        .From(new Vector2(0, 1080f));
                }
            }
            
            // 暗角效果
            if (vignetteOverlay != null)
            {
                var vignetteImage = vignetteOverlay.GetComponent<Image>();
                if (vignetteImage != null)
                {
                    vignetteImage.color = Color.black.WithAlpha(0.3f);
                    
                    vignetteOverlay.DOScale(1.05f, 4f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo);
                }
            }
        }
        
        /// <summary>
        /// 启动环境动画
        /// </summary>
        private void StartAmbientAnimations()
        {
            // 背景颜色渐变
            if (runtimeNebulaMaterial != null)
            {
                DOTween.To(() => 0f, x => {
                    float hue = (x * colorShiftSpeed) % 1f;
                    Color shiftedColor = Color.HSVToRGB(hue, 0.8f, 1f);
                    runtimeNebulaMaterial.SetColor("_ColorShift", shiftedColor);
                }, 1f, 10f)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);
            }
        }
        
        /// <summary>
        /// 更新背景效果
        /// </summary>
        private void UpdateBackgroundEffects()
        {
            if (runtimeGridMaterial != null)
            {
                runtimeGridMaterial.SetFloat("_Time", time);
            }
            
            if (runtimeNebulaMaterial != null)
            {
                runtimeNebulaMaterial.SetFloat("_Time", time * 0.1f);
            }
        }
        
        /// <summary>
        /// 更新粒子效果
        /// </summary>
        private void UpdateParticleEffects()
        {
            // 根据时间动态调整粒子发射
            if (ambientParticles != null)
            {
                var emission = ambientParticles.emission;
                float pulse = Mathf.Sin(time * 0.5f) * 0.3f + 0.7f;
                emission.rateOverTime = particleEmissionRate * pulse;
            }
        }
        
        /// <summary>
        /// 设置主题色
        /// </summary>
        public void SetThemeColors(Color primary, Color secondary, Color accent)
        {
            primaryColor = primary;
            secondaryColor = secondary;
            accentColor = accent;
            
            InitializeMaterials();
            InitializeParticles();
        }
        
        /// <summary>
        /// 播放进入动画
        /// </summary>
        public void PlayEnterAnimation()
        {
            isActive = true;
            
            // 背景淡入
            if (backgroundImage != null)
            {
                backgroundImage.color = Color.clear;
                backgroundImage.DOFade(1f, 1f).SetEase(Ease.OutQuad);
            }
            
            // 粒子系统淡入
            if (ambientParticles != null)
            {
                var main = ambientParticles.main;
                DOTween.To(() => 0f, x => {
                    var m = ambientParticles.main;
                    m.startColor = primaryColor.WithAlpha(x);
                }, 0.6f, 1f);
            }
            
            // 装饰元素依次进入
            if (cornerDecorations != null)
            {
                int index = 0;
                foreach (Transform corner in cornerDecorations)
                {
                    corner.localScale = Vector3.zero;
                    corner.DOScale(1f, 0.5f)
                        .SetEase(Ease.OutBack)
                        .SetDelay(index * 0.1f);
                    index++;
                }
            }
        }
        
        /// <summary>
        /// 播放退出动画
        /// </summary>
        public void PlayExitAnimation(System.Action onComplete = null)
        {
            isActive = false;
            
            // 背景淡出
            if (backgroundImage != null)
            {
                backgroundImage.DOFade(0f, 0.5f).SetEase(Ease.InQuad);
            }
            
            // 粒子系统淡出
            if (ambientParticles != null)
            {
                ambientParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            
            // 装饰元素退出
            if (cornerDecorations != null)
            {
                int index = 0;
                foreach (Transform corner in cornerDecorations)
                {
                    corner.DOScale(0f, 0.3f)
                        .SetEase(Ease.InBack)
                        .SetDelay(index * 0.05f);
                    index++;
                }
            }
            
            DOVirtual.DelayedCall(0.8f, () => onComplete?.Invoke());
        }
        
        /// <summary>
        /// 创建特效粒子
        /// </summary>
        public void SpawnEffectParticle(Vector3 position, Color color, float size = 0.1f)
        {
            GameObject particle = GetPooledParticle();
            if (particle == null) return;
            
            particle.transform.position = position;
            particle.SetActive(true);
            
            var image = particle.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
            }
            
            var rectTransform = particle.GetComponent<RectTransform>();
            rectTransform.localScale = Vector3.one * size;
            
            // 动画
            rectTransform.DOScale(0f, 0.5f).SetEase(Ease.InQuad);
            image?.DOFade(0f, 0.5f).SetEase(Ease.InQuad)
                .OnComplete(() => ReturnParticleToPool(particle));
        }
        
        private GameObject GetPooledParticle()
        {
            if (particlePool.Count > 0)
            {
                return particlePool.Dequeue();
            }
            return CreateParticle();
        }
        
        private void ReturnParticleToPool(GameObject particle)
        {
            if (particlePool.Count < MAX_POOL_SIZE)
            {
                particle.SetActive(false);
                particlePool.Enqueue(particle);
            }
            else
            {
                Destroy(particle);
            }
        }
        
        private GameObject CreateParticle()
        {
            GameObject particle = new GameObject("EffectParticle");
            particle.AddComponent<RectTransform>();
            var image = particle.AddComponent<Image>();
            image.sprite = Resources.Load<Sprite>("UI/Particle");
            particle.SetActive(false);
            particle.transform.SetParent(transform, false);
            return particle;
        }
        
        private void OnDestroy()
        {
            if (runtimeGridMaterial != null) Destroy(runtimeGridMaterial);
            if (runtimeNebulaMaterial != null) Destroy(runtimeNebulaMaterial);
        }
    }
    
    /// <summary>
    /// 颜色扩展
    /// </summary>
    public static class ColorExtensions
    {
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
}
