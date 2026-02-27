using UnityEngine;
using System;

namespace SebeJJ.Shop
{
    /// <summary>
    /// 商店通知类型
    /// </summary>
    public enum ShopNotificationType
    {
        ItemUnlocked,       // 商品解锁
        PurchaseSuccess,    // 购买成功
        PurchaseFailed,     // 购买失败
        CartUpdated,        // 购物车更新
        NewItemsAvailable,  // 新商品上架
        SaleStarted,        // 促销开始
        SaleEnded           // 促销结束
    }
    
    /// <summary>
    /// 商店通知数据
    /// </summary>
    public class ShopNotification
    {
        public ShopNotificationType Type;
        public string Title;
        public string Message;
        public Sprite Icon;
        public float Duration;
        public Action OnClick;
    }
    
    /// <summary>
    /// 商店通知系统
    /// </summary>
    public class ShopNotificationSystem : MonoBehaviour
    {
        public static ShopNotificationSystem Instance { get; private set; }
        
        [Header("设置")]
        [SerializeField] private float defaultDuration = 3f;
        [SerializeField] private bool showPurchaseNotifications = true;
        [SerializeField] private bool showUnlockNotifications = true;
        
        public event Action<ShopNotification> OnNotificationReceived;
        
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
            // 订阅商店事件
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnItemUnlocked += OnItemUnlocked;
                ShopManager.Instance.OnPurchaseCompleted += OnPurchaseCompleted;
            }
        }
        
        private void OnDestroy()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnItemUnlocked -= OnItemUnlocked;
                ShopManager.Instance.OnPurchaseCompleted -= OnPurchaseCompleted;
            }
        }
        
        private void OnItemUnlocked(ShopItemData item)
        {
            if (!showUnlockNotifications) return;
            
            SendNotification(new ShopNotification
            {
                Type = ShopNotificationType.ItemUnlocked,
                Title = "新商品解锁",
                Message = $"{item.itemName} 已解锁！",
                Icon = item.icon,
                Duration = defaultDuration
            });
        }
        
        private void OnPurchaseCompleted(PurchaseResult result)
        {
            if (!showPurchaseNotifications) return;
            
            if (result.Success)
            {
                SendNotification(new ShopNotification
                {
                    Type = ShopNotificationType.PurchaseSuccess,
                    Title = "购买成功",
                    Message = $"成功购买 {result.PurchasedItems.Count} 件商品，花费 {result.TotalCost} CR",
                    Duration = defaultDuration
                });
            }
            else
            {
                SendNotification(new ShopNotification
                {
                    Type = ShopNotificationType.PurchaseFailed,
                    Title = "购买失败",
                    Message = result.Message,
                    Duration = defaultDuration
                });
            }
        }
        
        /// <summary>
        /// 发送通知
        /// </summary>
        public void SendNotification(ShopNotification notification)
        {
            OnNotificationReceived?.Invoke(notification);
        }
        
        /// <summary>
        /// 发送自定义通知
        /// </summary>
        public void SendCustomNotification(string title, string message, ShopNotificationType type = ShopNotificationType.PurchaseSuccess)
        {
            SendNotification(new ShopNotification
            {
                Type = type,
                Title = title,
                Message = message,
                Duration = defaultDuration
            });
        }
    }
}