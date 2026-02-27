using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SebeJJ.Shop.UI
{
    /// <summary>
    /// 购物车单项UI
    /// </summary>
    public class CartItemUI : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI unitPriceText;
        [SerializeField] private TextMeshProUGUI totalPriceText;
        [SerializeField] private TextMeshProUGUI quantityText;
        
        [Header("按钮")]
        [SerializeField] private Button decreaseButton;
        [SerializeField] private Button increaseButton;
        [SerializeField] private Button removeButton;
        
        private CartItem _cartItem;
        
        public event System.Action<ShopItemData> OnRemoveClicked;
        public event System.Action<ShopItemData, int> OnQuantityChanged;
        
        private void Awake()
        {
            if (decreaseButton != null)
                decreaseButton.onClick.AddListener(OnDecreaseClicked);
            
            if (increaseButton != null)
                increaseButton.onClick.AddListener(OnIncreaseClicked);
            
            if (removeButton != null)
                removeButton.onClick.AddListener(OnRemoveClickedHandler);
        }
        
        private void OnDestroy()
        {
            if (decreaseButton != null)
                decreaseButton.onClick.RemoveAllListeners();
            if (increaseButton != null)
                increaseButton.onClick.RemoveAllListeners();
            if (removeButton != null)
                removeButton.onClick.RemoveAllListeners();
        }
        
        /// <summary>
        /// 设置购物车项
        /// </summary>
        public void SetCartItem(CartItem cartItem)
        {
            _cartItem = cartItem;
            UpdateUI();
        }
        
        /// <summary>
        /// 更新UI
        /// </summary>
        private void UpdateUI()
        {
            if (_cartItem?.ItemData == null) return;
            
            if (itemIcon != null)
                itemIcon.sprite = _cartItem.ItemData.icon;
            
            if (itemNameText != null)
                itemNameText.text = _cartItem.ItemData.itemName;
            
            if (unitPriceText != null)
                unitPriceText.text = $"单价: {_cartItem.UnitPrice} CR";
            
            if (totalPriceText != null)
                totalPriceText.text = $"小计: {_cartItem.TotalPrice} CR";
            
            if (quantityText != null)
                quantityText.text = $"x{_cartItem.Quantity}";
            
            // 更新按钮状态
            if (decreaseButton != null)
                decreaseButton.interactable = _cartItem.Quantity > 1;
        }
        
        /// <summary>
        /// 减少数量
        /// </summary>
        private void OnDecreaseClicked()
        {
            if (_cartItem != null && _cartItem.Quantity > 1)
            {
                OnQuantityChanged?.Invoke(_cartItem.ItemData, _cartItem.Quantity - 1);
            }
        }
        
        /// <summary>
        /// 增加数量
        /// </summary>
        private void OnIncreaseClicked()
        {
            if (_cartItem != null)
            {
                OnQuantityChanged?.Invoke(_cartItem.ItemData, _cartItem.Quantity + 1);
            }
        }
        
        /// <summary>
        /// 移除按钮点击
        /// </summary>
        private void OnRemoveClickedHandler()
        {
            if (_cartItem != null)
            {
                OnRemoveClicked?.Invoke(_cartItem.ItemData);
            }
        }
    }
}