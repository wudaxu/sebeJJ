using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SebeJJ.Shop
{
    /// <summary>
    /// 购物车项
    /// </summary>
    [Serializable]
    public class CartItem
    {
        public ShopItemData ItemData;
        public int Quantity;
        public int UnitPrice;
        
        public int TotalPrice => UnitPrice * Quantity;
    }
    
    /// <summary>
    /// 购买结果
    /// </summary>
    public class PurchaseResult
    {
        public bool Success;
        public string Message;
        public List<ShopItemData> PurchasedItems;
        public int TotalCost;
        public CurrencyType CurrencyUsed;
    }
    
    /// <summary>
    /// 商品库存项
    /// </summary>
    [Serializable]
    public class ShopStockItem
    {
        public ShopItemData ItemData;
        public int CurrentStock;
        public bool IsUnlocked;
        public bool IsLimited;
        public int LimitedStockRemaining;
    }
    
    /// <summary>
    /// 商店管理器 - 单例
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }
        
        [Header("商店配置")]
        [SerializeField] private List<ShopItemData> allShopItems = new();
        [SerializeField] private float globalDiscountRate = 0f;
        [SerializeField] private CurrencyType defaultCurrency = CurrencyType.Credits;
        
        // 库存管理
        private Dictionary<string, ShopStockItem> _stockDatabase = new();
        
        // 购物车
        private List<CartItem> _cart = new();
        
        // 事件
        public event Action<ShopItemData> OnItemUnlocked;
        public event Action<CartItem> OnCartUpdated;
        public event Action OnCartCleared;
        public event Action<PurchaseResult> OnPurchaseCompleted;
        public event Action<ShopStockItem> OnStockChanged;
        
        // 属性
        public IReadOnlyList<CartItem> CartItems => _cart.AsReadOnly();
        public int CartItemCount => _cart.Sum(item => item.Quantity);
        public int CartTotalCost => _cart.Sum(item => item.TotalPrice);
        public float GlobalDiscountRate 
        { 
            get => globalDiscountRate;
            set => globalDiscountRate = Mathf.Clamp01(value);
        }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeStock();
        }
        
        /// <summary>
        /// 初始化库存
        /// </summary>
        private void InitializeStock()
        {
            _stockDatabase.Clear();
            
            foreach (var item in allShopItems)
            {
                if (item == null) continue;
                
                var stockItem = new ShopStockItem
                {
                    ItemData = item,
                    CurrentStock = item.maxStock,
                    IsUnlocked = !item.isLockedByDefault,
                    IsLimited = item.isLimitedStock,
                    LimitedStockRemaining = item.isLimitedStock ? item.limitedStockCount : -1
                };
                
                _stockDatabase[item.itemId] = stockItem;
            }
        }
        
        #region 商品查询
        
        /// <summary>
        /// 获取所有商品
        /// </summary>
        public List<ShopStockItem> GetAllItems()
        {
            return _stockDatabase.Values.ToList();
        }
        
        /// <summary>
        /// 按类型获取商品
        /// </summary>
        public List<ShopStockItem> GetItemsByType(ItemType type)
        {
            return _stockDatabase.Values
                .Where(s => s.ItemData.itemType == type)
                .ToList();
        }
        
        /// <summary>
        /// 按子类型获取商品
        /// </summary>
        public List<ShopStockItem> GetItemsBySubType(ItemSubType subType)
        {
            return _stockDatabase.Values
                .Where(s => s.ItemData.subType == subType)
                .ToList();
        }
        
        /// <summary>
        /// 按稀有度获取商品
        /// </summary>
        public List<ShopStockItem> GetItemsByRarity(ItemRarity rarity)
        {
            return _stockDatabase.Values
                .Where(s => s.ItemData.rarity == rarity)
                .ToList();
        }
        
        /// <summary>
        /// 获取已解锁的商品
        /// </summary>
        public List<ShopStockItem> GetUnlockedItems()
        {
            return _stockDatabase.Values
                .Where(s => s.IsUnlocked)
                .ToList();
        }
        
        /// <summary>
        /// 搜索商品
        /// </summary>
        public List<ShopStockItem> SearchItems(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return GetAllItems();
            
            keyword = keyword.ToLower();
            return _stockDatabase.Values
                .Where(s => s.ItemData.itemName.ToLower().Contains(keyword) ||
                           s.ItemData.description.ToLower().Contains(keyword))
                .ToList();
        }
        
        /// <summary>
        /// 获取单个商品库存信息
        /// </summary>
        public ShopStockItem GetStockItem(string itemId)
        {
            return _stockDatabase.TryGetValue(itemId, out var stock) ? stock : null;
        }
        
        /// <summary>
        /// 获取商品当前价格
        /// </summary>
        public int GetItemPrice(ShopItemData item)
        {
            if (item == null) return 0;
            return item.GetCurrentPrice(globalDiscountRate);
        }
        
        #endregion
        
        #region 购物车操作
        
        /// <summary>
        /// 添加商品到购物车
        /// </summary>
        public bool AddToCart(ShopItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;
            
            var stockItem = GetStockItem(item.itemId);
            if (stockItem == null || !stockItem.IsUnlocked) return false;
            
            // 检查库存
            int currentInCart = _cart.FirstOrDefault(c => c.ItemData == item)?.Quantity ?? 0;
            int availableStock = stockItem.IsLimited 
                ? stockItem.LimitedStockRemaining 
                : stockItem.CurrentStock;
            
            if (currentInCart + quantity > availableStock)
            {
                Debug.LogWarning($"库存不足: {item.itemName}");
                return false;
            }
            
            // 添加到购物车
            var cartItem = _cart.FirstOrDefault(c => c.ItemData == item);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                _cart.Add(new CartItem
                {
                    ItemData = item,
                    Quantity = quantity,
                    UnitPrice = GetItemPrice(item)
                });
            }
            
            OnCartUpdated?.Invoke(cartItem ?? _cart.Last());
            return true;
        }
        
        /// <summary>
        /// 从购物车移除商品
        /// </summary>
        public bool RemoveFromCart(ShopItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;
            
            var cartItem = _cart.FirstOrDefault(c => c.ItemData == item);
            if (cartItem == null) return false;
            
            if (cartItem.Quantity <= quantity)
            {
                _cart.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity -= quantity;
            }
            
            OnCartUpdated?.Invoke(cartItem);
            return true;
        }
        
        /// <summary>
        /// 更新购物车中商品数量
        /// </summary>
        public bool UpdateCartQuantity(ShopItemData item, int newQuantity)
        {
            if (item == null || newQuantity < 0) return false;
            
            if (newQuantity == 0)
            {
                return RemoveFromCart(item, int.MaxValue);
            }
            
            var stockItem = GetStockItem(item.itemId);
            int availableStock = stockItem.IsLimited 
                ? stockItem.LimitedStockRemaining 
                : stockItem.CurrentStock;
            
            if (newQuantity > availableStock)
            {
                Debug.LogWarning($"库存不足: {item.itemName}");
                return false;
            }
            
            var cartItem = _cart.FirstOrDefault(c => c.ItemData == item);
            if (cartItem != null)
            {
                cartItem.Quantity = newQuantity;
                OnCartUpdated?.Invoke(cartItem);
                return true;
            }
            
            return AddToCart(item, newQuantity);
        }
        
        /// <summary>
        /// 清空购物车
        /// </summary>
        public void ClearCart()
        {
            _cart.Clear();
            OnCartCleared?.Invoke();
        }
        
        #endregion
        
        #region 购买逻辑
        
        /// <summary>
        /// 购买购物车中的所有商品
        /// </summary>
        public PurchaseResult PurchaseCart(CurrencyType? currencyType = null)
        {
            var currency = currencyType ?? defaultCurrency;
            var result = new PurchaseResult
            {
                Success = false,
                PurchasedItems = new List<ShopItemData>(),
                TotalCost = CartTotalCost,
                CurrencyUsed = currency
            };
            
            if (_cart.Count == 0)
            {
                result.Message = "购物车为空";
                OnPurchaseCompleted?.Invoke(result);
                return result;
            }
            
            // 检查货币
            if (!CurrencySystem.Instance.HasEnoughCurrency(currency, result.TotalCost))
            {
                result.Message = "信用点不足";
                OnPurchaseCompleted?.Invoke(result);
                return result;
            }
            
            // 检查库存
            foreach (var cartItem in _cart)
            {
                var stockItem = GetStockItem(cartItem.ItemData.itemId);
                if (stockItem == null)
                {
                    result.Message = $"商品不存在: {cartItem.ItemData.itemName}";
                    OnPurchaseCompleted?.Invoke(result);
                    return result;
                }
                
                int availableStock = stockItem.IsLimited 
                    ? stockItem.LimitedStockRemaining 
                    : stockItem.CurrentStock;
                
                if (cartItem.Quantity > availableStock)
                {
                    result.Message = $"库存不足: {cartItem.ItemData.itemName}";
                    OnPurchaseCompleted?.Invoke(result);
                    return result;
                }
            }
            
            // 扣除货币
            if (!CurrencySystem.Instance.DeductCurrency(currency, result.TotalCost, "购买商品"))
            {
                result.Message = "货币扣除失败";
                OnPurchaseCompleted?.Invoke(result);
                return result;
            }
            
            // 扣除库存并发放商品
            foreach (var cartItem in _cart)
            {
                var stockItem = GetStockItem(cartItem.ItemData.itemId);
                
                if (stockItem.IsLimited)
                {
                    stockItem.LimitedStockRemaining -= cartItem.Quantity;
                }
                else
                {
                    stockItem.CurrentStock -= cartItem.Quantity;
                }
                
                result.PurchasedItems.Add(cartItem.ItemData);
                OnStockChanged?.Invoke(stockItem);
                
                // 触发购买效果
                TriggerPurchaseEffect(cartItem.ItemData);
            }
            
            // 清空购物车
            _cart.Clear();
            OnCartCleared?.Invoke();
            
            result.Success = true;
            result.Message = "购买成功！";
            OnPurchaseCompleted?.Invoke(result);
            
            return result;
        }
        
        /// <summary>
        /// 直接购买单个商品
        /// </summary>
        public PurchaseResult PurchaseItem(ShopItemData item, int quantity = 1, CurrencyType? currencyType = null)
        {
            ClearCart();
            if (AddToCart(item, quantity))
            {
                return PurchaseCart(currencyType);
            }
            
            return new PurchaseResult
            {
                Success = false,
                Message = "无法添加到购物车"
            };
        }
        
        /// <summary>
        /// 触发购买效果
        /// </summary>
        private void TriggerPurchaseEffect(ShopItemData item)
        {
            if (item.purchaseSound != null)
            {
                AudioSource.PlayClipAtPoint(item.purchaseSound, Camera.main?.transform.position ?? Vector3.zero);
            }
            
            if (item.purchaseEffectPrefab != null)
            {
                // 效果播放逻辑（可由UI层处理）
            }
        }
        
        #endregion
        
        #region 解锁机制
        
        /// <summary>
        /// 解锁商品
        /// </summary>
        public bool UnlockItem(string itemId)
        {
            if (!_stockDatabase.TryGetValue(itemId, out var stockItem))
                return false;
            
            if (stockItem.IsUnlocked) return true;
            
            stockItem.IsUnlocked = true;
            OnItemUnlocked?.Invoke(stockItem.ItemData);
            return true;
        }
        
        /// <summary>
        /// 检查商品解锁条件
        /// </summary>
        public bool CheckUnlockConditions(ShopItemData item, int playerLevel, List<string> ownedAchievements, List<string> ownedItemIds)
        {
            if (item == null) return false;
            
            // 检查等级要求
            if (playerLevel < item.requiredPlayerLevel)
                return false;
            
            // 检查成就要求
            if (!string.IsNullOrEmpty(item.requiredAchievementId))
            {
                if (ownedAchievements == null || !ownedAchievements.Contains(item.requiredAchievementId))
                    return false;
            }
            
            // 检查前置商品
            if (item.requiredItems != null && item.requiredItems.Length > 0)
            {
                if (ownedItemIds == null) return false;
                
                foreach (var requiredItem in item.requiredItems)
                {
                    if (requiredItem == null) continue;
                    if (!ownedItemIds.Contains(requiredItem.itemId))
                        return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 刷新所有商品解锁状态
        /// </summary>
        public void RefreshUnlockStatus(int playerLevel, List<string> ownedAchievements, List<string> ownedItemIds)
        {
            foreach (var stockItem in _stockDatabase.Values)
            {
                if (!stockItem.IsUnlocked && stockItem.ItemData.isLockedByDefault)
                {
                    if (CheckUnlockConditions(stockItem.ItemData, playerLevel, ownedAchievements, ownedItemIds))
                    {
                        UnlockItem(stockItem.ItemData.itemId);
                    }
                }
            }
        }
        
        #endregion
        
        #region 库存管理
        
        /// <summary>
        /// 补货
        /// </summary>
        public void RestockItem(string itemId, int amount)
        {
            if (!_stockDatabase.TryGetValue(itemId, out var stockItem)) return;
            
            if (stockItem.IsLimited)
            {
                stockItem.LimitedStockRemaining = Mathf.Min(
                    stockItem.LimitedStockRemaining + amount,
                    stockItem.ItemData.limitedStockCount
                );
            }
            else
            {
                stockItem.CurrentStock = Mathf.Min(
                    stockItem.CurrentStock + amount,
                    stockItem.ItemData.maxStock
                );
            }
            
            OnStockChanged?.Invoke(stockItem);
        }
        
        /// <summary>
        /// 完全补货所有商品
        /// </summary>
        public void RestockAll()
        {
            foreach (var stockItem in _stockDatabase.Values)
            {
                if (stockItem.IsLimited)
                {
                    stockItem.LimitedStockRemaining = stockItem.ItemData.limitedStockCount;
                }
                else
                {
                    stockItem.CurrentStock = stockItem.ItemData.maxStock;
                }
                OnStockChanged?.Invoke(stockItem);
            }
        }
        
        #endregion
        
        #region 保存/加载
        
        /// <summary>
        /// 保存商店数据
        /// </summary>
        public void SaveShopData()
        {
            foreach (var stockItem in _stockDatabase.Values)
            {
                string key = $"Shop_{stockItem.ItemData.itemId}";
                PlayerPrefs.SetInt($"{key}_Stock", stockItem.IsLimited 
                    ? stockItem.LimitedStockRemaining 
                    : stockItem.CurrentStock);
                PlayerPrefs.SetInt($"{key}_Unlocked", stockItem.IsUnlocked ? 1 : 0);
            }
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// 加载商店数据
        /// </summary>
        public void LoadShopData()
        {
            foreach (var stockItem in _stockDatabase.Values)
            {
                string key = $"Shop_{stockItem.ItemData.itemId}";
                
                if (stockItem.IsLimited)
                {
                    stockItem.LimitedStockRemaining = PlayerPrefs.GetInt($"{key}_Stock", stockItem.ItemData.limitedStockCount);
                }
                else
                {
                    stockItem.CurrentStock = PlayerPrefs.GetInt($"{key}_Stock", stockItem.ItemData.maxStock);
                }
                
                bool wasUnlocked = PlayerPrefs.GetInt($"{key}_Unlocked", stockItem.IsUnlocked ? 1 : 0) == 1;
                if (wasUnlocked && !stockItem.IsUnlocked)
                {
                    stockItem.IsUnlocked = true;
                }
            }
        }
        
        #endregion
    }
}