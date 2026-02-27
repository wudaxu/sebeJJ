using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace SebeJJ.Shop.UI
{
    /// <summary>
    /// 商品详情面板UI
    /// </summary>
    public class ItemDetailPanel : MonoBehaviour
    {
        [Header("基础信息")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image rarityBackground;
        
        [Header("属性显示")]
        [SerializeField] private Transform statsContainer;
        [SerializeField] private GameObject statItemPrefab;
        
        [Header("价格与购买")]
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI stockText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button addToCartButton;
        [SerializeField] private Slider quantitySlider;
        [SerializeField] private TextMeshProUGUI quantityText;
        
        [Header("解锁信息")]
        [SerializeField] private GameObject lockedPanel;
        [SerializeField] private TextMeshProUGUI unlockConditionText;
        
        private ShopStockItem _currentStockItem;
        private int _selectedQuantity = 1;
        
        public event System.Action<ShopItemData, int> OnPurchaseClicked;
        public event System.Action<ShopItemData, int> OnAddToCartClicked;
        
        private void Awake()
        {
            if (purchaseButton != null)
                purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
            
            if (addToCartButton != null)
                addToCartButton.onClick.AddListener(OnAddToCartButtonClicked);
            
            if (quantitySlider != null)
                quantitySlider.onValueChanged.AddListener(OnQuantityChanged);
        }
        
        private void OnDestroy()
        {
            if (purchaseButton != null)
                purchaseButton.onClick.RemoveAllListeners();
            if (addToCartButton != null)
                addToCartButton.onClick.RemoveAllListeners();
            if (quantitySlider != null)
                quantitySlider.onValueChanged.RemoveAllListeners();
        }
        
        /// <summary>
        /// 显示商品详情
        /// </summary>
        public void ShowItemDetail(ShopStockItem stockItem)
        {
            _currentStockItem = stockItem;
            _selectedQuantity = 1;
            
            if (quantitySlider != null)
            {
                int maxStock = GetAvailableStock();
                quantitySlider.maxValue = maxStock;
                quantitySlider.value = 1;
            }
            
            UpdateUI();
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// 更新UI
        /// </summary>
        private void UpdateUI()
        {
            if (_currentStockItem == null) return;
            
            var item = _currentStockItem.ItemData;
            
            // 基础信息
            if (itemIcon != null)
                itemIcon.sprite = item.icon;
            
            if (itemNameText != null)
                itemNameText.text = item.itemName;
            
            if (rarityText != null)
            {
                rarityText.text = GetRarityName(item.rarity);
                rarityText.color = item.GetRarityColor();
            }
            
            if (rarityBackground != null)
                rarityBackground.color = item.GetRarityColor() * 0.3f;
            
            if (descriptionText != null)
                descriptionText.text = item.description;
            
            // 属性
            UpdateStats(item.stats);
            
            // 价格
            int unitPrice = ShopManager.Instance.GetItemPrice(item);
            int totalPrice = unitPrice * _selectedQuantity;
            
            if (priceText != null)
                priceText.text = $"{totalPrice} CR";
            
            // 库存
            if (stockText != null)
            {
                int stock = GetAvailableStock();
                stockText.text = $"库存: {stock}";
            }
            
            // 数量
            if (quantityText != null)
                quantityText.text = $"x{_selectedQuantity}";
            
            // 锁定状态
            bool isLocked = !_currentStockItem.IsUnlocked;
            if (lockedPanel != null)
                lockedPanel.SetActive(isLocked);
            
            if (unlockConditionText != null && isLocked)
            {
                unlockConditionText.text = GetUnlockConditionsText(item);
            }
            
            // 按钮状态
            bool canPurchase = !isLocked && GetAvailableStock() >= _selectedQuantity;
            if (purchaseButton != null)
                purchaseButton.interactable = canPurchase;
            if (addToCartButton != null)
                addToCartButton.interactable = canPurchase;
        }
        
        /// <summary>
        /// 更新属性显示
        /// </summary>
        private void UpdateStats(ItemStats stats)
        {
            if (statsContainer == null || statItemPrefab == null) return;
            
            // 清除旧属性
            foreach (Transform child in statsContainer)
            {
                Destroy(child.gameObject);
            }
            
            // 添加属性
            AddStatIfNotZero("伤害", stats.damageBonus);
            AddStatIfNotZero("防御", stats.defenseBonus);
            AddStatIfNotZero("速度", stats.speedBonus);
            AddStatIfNotZero("能量", stats.energyBonus);
            AddStatIfNotZero("氧气", stats.oxygenBonus);
            AddStatIfNotZero("射速", stats.fireRate, "发/秒");
            AddStatIfNotZero("射程", stats.range, "米");
            AddStatIfNotZero("弹容", stats.ammoCapacity);
            AddStatIfNotZero("耐久", stats.durability);
            AddStatIfNotZero("效率", stats.efficiency, "%");
            AddStatIfNotZero("恢复", stats.restoreAmount);
            AddStatIfNotZero("持续时间", stats.effectDuration, "秒");
            AddStatIfNotZero("倍率", stats.upgradeMultiplier, "x");
        }
        
        /// <summary>
        /// 添加属性项（如果不为零）
        /// </summary>
        private void AddStatIfNotZero(string name, float value, string suffix = "")
        {
            if (Mathf.Approximately(value, 0)) return;
            
            var statObj = Instantiate(statItemPrefab, statsContainer);
            var texts = statObj.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                texts[0].text = name;
                texts[1].text = $"{(value > 0 ? "+" : "")}{value}{suffix}";
                texts[1].color = value > 0 ? Color.green : Color.red;
            }
        }
        
        /// <summary>
        /// 获取可用库存
        /// </summary>
        private int GetAvailableStock()
        {
            return _currentStockItem.IsLimited 
                ? _currentStockItem.LimitedStockRemaining 
                : _currentStockItem.CurrentStock;
        }
        
        /// <summary>
        /// 获取稀有度名称
        /// </summary>
        private string GetRarityName(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => "普通",
                ItemRarity.Uncommon => "稀有",
                ItemRarity.Rare => "史诗",
                ItemRarity.Legendary => "传说",
                ItemRarity.Mythic => "神话",
                _ => "未知"
            };
        }
        
        /// <summary>
        /// 获取解锁条件文本
        /// </summary>
        private string GetUnlockConditionsText(ShopItemData item)
        {
            var conditions = new List<string>();
            
            if (item.requiredPlayerLevel > 1)
                conditions.Add($"需要等级 {item.requiredPlayerLevel}");
            
            if (!string.IsNullOrEmpty(item.requiredAchievementId))
                conditions.Add("需要特定成就");
            
            if (item.requiredItems != null && item.requiredItems.Length > 0)
            {
                foreach (var req in item.requiredItems)
                {
                    if (req != null)
                        conditions.Add($"需要: {req.itemName}");
                }
            }
            
            return conditions.Count > 0 
                ? string.Join("\n", conditions) 
                : "完成特定任务解锁";
        }
        
        /// <summary>
        /// 数量变更回调
        /// </summary>
        private void OnQuantityChanged(float value)
        {
            _selectedQuantity = Mathf.RoundToInt(value);
            UpdateUI();
        }
        
        /// <summary>
        /// 购买按钮点击
        /// </summary>
        private void OnPurchaseButtonClicked()
        {
            if (_currentStockItem != null)
            {
                OnPurchaseClicked?.Invoke(_currentStockItem.ItemData, _selectedQuantity);
            }
        }
        
        /// <summary>
        /// 添加到购物车按钮点击
        /// </summary>
        private void OnAddToCartButtonClicked()
        {
            if (_currentStockItem != null)
            {
                OnAddToCartClicked?.Invoke(_currentStockItem.ItemData, _selectedQuantity);
            }
        }
        
        /// <summary>
        /// 关闭面板
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}