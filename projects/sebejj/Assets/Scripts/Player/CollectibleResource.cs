using UnityEngine;
using System;

namespace SebeJJ.Player
{
    /// <summary>
    /// 可采集资源 - 附着在资源物体上
    /// </summary>
    public class CollectibleResource : MonoBehaviour
    {
        [Header("资源属性")]
        public string resourceId;
        public string resourceName = "未知资源";
        public ResourceType resourceType = ResourceType.Mineral;
        public int quantity = 1;
        public int value = 10;
        public float weight = 1f;
        
        [Header("采集设置")]
        public bool requiresScan = true;
        public bool isScanned = false;
        public bool isCollected = false;
        
        [Header("视觉效果")]
        public SpriteRenderer spriteRenderer;
        public Color unscannedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        public Color scannedColor = Color.white;
        public Color highlightedColor = Color.cyan;
        
        [Header("扫描特效")]
        public GameObject scanEffectPrefab;
        public AudioClip scanSound;
        public float scanEffectDuration = 1f;
        
        // 事件
        public event Action OnResourceScanned;
        public event Action OnResourceCollected;
        
        private void Start()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            UpdateVisualState();
        }
        
        /// <summary>
        /// 是否可以采集
        /// </summary>
        public bool CanCollect()
        {
            if (isCollected) return false;
            if (requiresScan && !isScanned) return false;
            return true;
        }
        
        /// <summary>
        /// 被扫描到时调用
        /// </summary>
        public void OnScanned()
        {
            if (isScanned) return;
            
            isScanned = true;
            UpdateVisualState();
            
            Debug.Log($"[CollectibleResource] 发现资源: {resourceName}");
            
            // 显示扫描效果
            ShowScanEffect();
            
            OnResourceScanned?.Invoke();
        }
        
        /// <summary>
        /// 采集资源
        /// </summary>
        public bool Collect()
        {
            if (!CanCollect()) return false;
            
            isCollected = true;
            
            Debug.Log($"[CollectibleResource] 采集成功: {resourceName} x{quantity}");
            
            // 添加到背包
            var resourceManager = Core.GameManager.Instance?.resourceManager;
            if (resourceManager != null)
            {
                var item = new Data.InventoryItem
                {
                    itemId = resourceId,
                    itemName = resourceName,
                    quantity = quantity,
                    weight = weight,
                    value = value
                };
                
                if (resourceManager.AddToInventory(item))
                {
                    OnResourceCollected?.Invoke();
                    Destroy(gameObject);
                    return true;
                }
                else
                {
                    isCollected = false;
                    Debug.LogWarning("[CollectibleResource] 背包已满，无法采集");
                    return false;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 更新视觉效果
        /// </summary>
        private void UpdateVisualState()
        {
            if (spriteRenderer == null) return;
            
            if (isCollected)
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, 0f);
            }
            else if (isScanned || !requiresScan)
            {
                spriteRenderer.color = scannedColor;
            }
            else
            {
                spriteRenderer.color = unscannedColor;
            }
        }
        
        /// <summary>
        /// 显示扫描效果
        /// </summary>
        private void ShowScanEffect()
        {
            // 实例化扫描特效
            if (scanEffectPrefab != null)
            {
                GameObject effect = Instantiate(scanEffectPrefab, transform.position, Quaternion.identity);
                effect.transform.SetParent(transform);
                Destroy(effect, scanEffectDuration);
            }
            
            // 播放音效
            if (scanSound != null)
            {
                AudioManager.Instance?.PlaySFX(scanSound);
            }
            
            Debug.Log($"[CollectibleResource] 播放扫描特效: {resourceName}");
        }
        
        /// <summary>
        /// 高亮显示
        /// </summary>
        public void SetHighlighted(bool highlighted)
        {
            if (spriteRenderer == null) return;
            
            if (highlighted && (isScanned || !requiresScan))
            {
                spriteRenderer.color = highlightedColor;
            }
            else
            {
                UpdateVisualState();
            }
        }
        
        public string ResourceName => resourceName;
        public ResourceType Type => resourceType;
    }
    
    /// <summary>
    /// 资源类型枚举
    /// </summary>
    public enum ResourceType
    {
        Mineral,        // 矿物
        Crystal,        // 水晶
        BioMaterial,    // 生物材料
        TechScrap,      // 科技废料
        RareArtifact    // 稀有遗物
    }
}
