using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace SebeJJ.Shop.UI
{
    /// <summary>
    /// 购物车UI
    /// </summary>
    public class CartUI : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private Transform cartItemsContainer;
        [SerializeField] private GameObject cartItemPrefab;
        [SerializeField] private TextMeshProUGUI totalPriceText;
        [SerializeField] private TextMeshProUGUI itemCountText;
        [SerializeField] private Button checkoutButton;
        [SerializeField] private Button clearCartButton;
        [SerializeField] private GameObject emptyCartMessage;
        
        [Header("确认弹窗")]
        [SerializeField] private GameObject confirmDialog;
        [SerializeField] private TextMeshProUGUI confirmTotalText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        
        private List<CartItemUI> _cartItemUIs = new();
        
        private void Awake()
        {
            if (checkoutButton != null)
                checkoutButton.onClick.AddListener(ShowConfirmDialog);
            
            if (clearCartButton != null)
                clearCartButton.onClick.AddListener(ClearCart);
            
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmPurchase);
            
            if (cancelButton != null)
                cancelButton.onClick.AddListener(HideConfirmDialog);
        }
        
        private void Start()
        {
            // 订阅商店事件
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnCartUpdated += OnCartUpdated;
                ShopManager.Instance.OnCartCleared += OnCartCleared;
                ShopManager.Instance.OnPurchaseCompleted += OnPurchaseCompleted;
            }
            
            RefreshCartUI();
        }
        
        private void OnDestroy()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnCartUpdated -= OnCartUpdated;
                ShopManager.Instance.OnCartCleared -= OnCartCleared;
                ShopManager.Instance.OnPurchaseCompleted -= OnPurchaseCompleted;
            }
            
            if (checkoutButton != null)
                checkoutButton.onClick.RemoveAllListeners();
            if (clearCartButton != null)
                clearCartButton.onClick.RemoveAllListeners();
            if (confirmButton != null)
                confirmButton.onClick.RemoveAllListeners();
            if (cancelButton != null)
                cancelButton.onClick.RemoveAllListeners();
        }
        
        /// <summary>
        /// 购物车更新回调
        /// </summary>
        private void OnCartUpdated(CartItem cartItem)
        {
            RefreshCartUI();
        }
        
        /// <summary>
        /// 购物车清空回调
        /// </summary>
        private void OnCartCleared()
        {
            RefreshCartUI();
        }
        
        /// <summary>
        /// 购买完成回调
        /// </summary>
        private void OnPurchaseCompleted(PurchaseResult result)
        {
            HideConfirmDialog();
            
            if (result.Success)
            {
                ShowPurchaseSuccessMessage(result);
            }
            else
            {
                ShowPurchaseFailedMessage(result.Message);
            }
        }
        
        /// <summary>
        /// 刷新购物车UI
        /// </summary>
        private void RefreshCartUI()
        {
            if (ShopManager.Instance == null) return;
            
            // 清除旧UI
            foreach (var ui in _cartItemUIs)
            {
                if (ui != null)
                    Destroy(ui.gameObject);
            }
            _cartItemUIs.Clear();
            
            var cartItems = ShopManager.Instance.CartItems;
            
            // 显示/隐藏空购物车提示
            if (emptyCartMessage != null)
                emptyCartMessage.SetActive(cartItems.Count == 0);
            
            if (checkoutButton != null)
                checkoutButton.interactable = cartItems.Count > 0;
            
            if (clearCartButton != null)
                clearCartButton.interactable = cartItems.Count > 0;
            
            // 创建购物车项UI
            foreach (var cartItem in cartItems)
            {
                if (cartItemPrefab != null && cartItemsContainer != null)
                {
                    var obj = Instantiate(cartItemPrefab, cartItemsContainer);
                    var cartItemUI = obj.GetComponent<CartItemUI>();
                    if (cartItemUI != null)
                    {
                        cartItemUI.SetCartItem(cartItem);
                        cartItemUI.OnRemoveClicked += OnRemoveCartItem;
                        cartItemUI.OnQuantityChanged += OnCartItemQuantityChanged;
                        _cartItemUIs.Add(cartItemUI);
                    }
                }
            }
            
            // 更新总价
            UpdateTotalDisplay();
        }
        
        /// <summary>
        /// 更新总价显示
        /// </summary>
        private void UpdateTotalDisplay()
        {
            if (ShopManager.Instance == null) return;
            
            int total = ShopManager.Instance.CartTotalCost;
            int count = ShopManager.Instance.CartItemCount;
            
            if (totalPriceText != null)
                totalPriceText.text = $"总计: {total} CR";
            
            if (itemCountText != null)
                itemCountText.text = $"共 {count} 件商品";
        }
        
        /// <summary>
        /// 移除购物车项
        /// </summary>
        private void OnRemoveCartItem(ShopItemData item)
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.RemoveFromCart(item);
            }
        }
        
        /// <summary>
        /// 购物车项数量变更
        /// </summary>
        private void OnCartItemQuantityChanged(ShopItemData item, int newQuantity)
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.UpdateCartQuantity(item, newQuantity);
            }
        }
        
        /// <summary>
        /// 清空购物车
        /// </summary>
        private void ClearCart()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.ClearCart();
            }
        }
        
        /// <summary>
        /// 显示确认弹窗
        /// </summary>
        private void ShowConfirmDialog()
        {
            if (confirmDialog != null)
            {
                if (confirmTotalText != null)
                    confirmTotalText.text = $"总计: {ShopManager.Instance?.CartTotalCost ?? 0} CR";
                
                confirmDialog.SetActive(true);
            }
        }
        
        /// <summary>
        /// 隐藏确认弹窗
        /// </summary>
        private void HideConfirmDialog()
        {
            if (confirmDialog != null)
                confirmDialog.SetActive(false);
        }
        
        /// <summary>
        /// 确认购买
        /// </summary>
        private void OnConfirmPurchase()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.PurchaseCart();
            }
        }
        
        /// <summary>
        /// 显示购买成功消息
        /// </summary>
        private void ShowPurchaseSuccessMessage(PurchaseResult result)
        {
            Debug.Log($"购买成功！共花费 {result.TotalCost} CR");
            // 可以在这里触发UI动画或弹窗
        }
        
        /// <summary>
        /// 显示购买失败消息
        /// </summary>
        private void ShowPurchaseFailedMessage(string message)
        {
            Debug.LogWarning($"购买失败: {message}");
            // 可以在这里触发错误提示
        }
    }
}