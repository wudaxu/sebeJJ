using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SebeJJ.Shop.UI
{
    /// <summary>
    /// 商品列表项UI
    /// </summary>
    public class ShopItemUI : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI stockText;
        [SerializeField] private Image rarityBorder;
        [SerializeField] private Image lockOverlay;
        [SerializeField] private GameObject newTag;
        [SerializeField] private GameObject discountTag;
        [SerializeField] private TextMeshProUGUI discountText;
        
        [Header("按钮")]
        [SerializeField] private Button itemButton;
        [SerializeField] private Button addToCartButton;
        
        private ShopStockItem _stockItem;
        private ShopItemData _itemData;
        
        public ShopItemData ItemData => _itemData;
        
        public event System.Action<ShopItemData> OnItemClicked;
        public event System.Action<ShopItemData> OnAddToCartClicked;
        
        private void Awake()
        {
            if (itemButton != null)
                itemButton.onClick.AddListener(() => OnItemClicked?.Invoke(_itemData));
            
            if (addToCartButton != null)
                addToCartButton.onClick.AddListener(() => OnAddToCartClicked?.Invoke(_itemData));
        }
        
        private void OnDestroy()
        {
            if (itemButton != null)
                itemButton.onClick.RemoveAllListeners();
            if (addToCartButton != null)
                addToCartButton.onClick.RemoveAllListeners();
        }
        
        /// <summary>
        /// 设置商品数据
        /// </summary>
        public void SetItem(ShopStockItem stockItem)
        {
            _stockItem = stockItem;
            _itemData = stockItem.ItemData;
            
            UpdateUI();
        }
        
        /// <summary>
        /// 更新UI显示
        /// </summary>
        public void UpdateUI()
        {
            if (_itemData == null) return;
            
            // 图标和名称
            if (itemIcon != null)
                itemIcon.sprite = _itemData.icon;
            
            if (itemNameText != null)
                itemNameText.text = _itemData.itemName;
            
            // 价格
            int price = ShopManager.Instance.GetItemPrice(_itemData);
            if (priceText != null)
                priceText.text = $"{price} CR";
            
            // 稀有度边框
            if (rarityBorder != null)
                rarityBorder.color = _itemData.GetRarityColor();
            
            // 库存显示
            if (stockText != null)
            {
                int stock = _stockItem.IsLimited 
                    ? _stockItem.LimitedStockRemaining 
                    : _stockItem.CurrentStock;
                stockText.text = _stockItem.IsLimited ? $"限量: {stock}" : $"库存: {stock}";
            }
            
            // 锁定状态
            if (lockOverlay != null)
                lockOverlay.gameObject.SetActive(!_stockItem.IsUnlocked);
            
            if (addToCartButton != null)
                addToCartButton.interactable = _stockItem.IsUnlocked && GetAvailableStock() > 0;
        }
        
        /// <summary>
        /// 获取可用库存
        /// </summary>
        private int GetAvailableStock()
        {
            return _stockItem.IsLimited 
                ? _stockItem.LimitedStockRemaining 
                : _stockItem.CurrentStock;
        }
        
        /// <summary>
        /// 设置折扣显示
        /// </summary>
        public void SetDiscount(float discountRate)
        {
            if (discountTag != null)
            {
                discountTag.SetActive(discountRate > 0);
                if (discountText != null)
                    discountText.text = $"-{discountRate * 100:0}%";
            }
        }
        
        /// <summary>
        /// 设置新品标签
        /// </summary>
        public void SetNewTag(bool isNew)
        {
            if (newTag != null)
                newTag.SetActive(isNew);
        }
    }
}