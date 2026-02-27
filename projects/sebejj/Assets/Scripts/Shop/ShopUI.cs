using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace SebeJJ.Shop.UI
{
    /// <summary>
    /// 商品排序方式
    /// </summary>
    public enum SortType
    {
        Default,        // 默认
        PriceAsc,       // 价格升序
        PriceDesc,      // 价格降序
        RarityAsc,      // 稀有度升序
        RarityDesc,     // 稀有度降序
        NameAsc,        // 名称升序
        NameDesc        // 名称降序
    }
    
    /// <summary>
    /// 商店主界面
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        [Header("商品列表")]
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private GameObject shopItemPrefab;
        [SerializeField] private ScrollRect itemsScrollRect;
        
        [Header("分类筛选")]
        [SerializeField] private Button allButton;
        [SerializeField] private Button weaponsButton;
        [SerializeField] private Button partsButton;
        [SerializeField] private Button consumablesButton;
        [SerializeField] private Button upgradesButton;
        
        [Header("搜索与排序")]
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private TMP_Dropdown sortDropdown;
        [SerializeField] private Button searchButton;
        [SerializeField] private Button clearSearchButton;
        
        [Header("货币显示")]
        [SerializeField] private TextMeshProUGUI creditsText;
        [SerializeField] private TextMeshProUGUI premiumCreditsText;
        
        [Header("详情面板")]
        [SerializeField] private ItemDetailPanel detailPanel;
        
        [Header("购物车")]
        [SerializeField] private CartUI cartUI;
        [SerializeField] private Button toggleCartButton;
        [SerializeField] private GameObject cartPanel;
        [SerializeField] private TextMeshProUGUI cartBadgeText;
        
        private List<ShopItemUI> _itemUIs = new();
        private ItemType _currentFilter = ItemType.Weapon;
        private bool _showAllTypes = true;
        private SortType _currentSort = SortType.Default;
        private string _searchKeyword = "";
        
        private void Awake()
        {
            SetupButtons();
            SetupSortDropdown();
        }
        
        private void Start()
        {
            // 订阅事件
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnItemUnlocked += OnItemUnlocked;
                ShopManager.Instance.OnStockChanged += OnStockChanged;
                ShopManager.Instance.OnCartUpdated += OnCartUpdated;
                ShopManager.Instance.OnCartCleared += OnCartCleared;
            }
            
            if (CurrencySystem.Instance != null)
            {
                CurrencySystem.Instance.OnCurrencyChanged += OnCurrencyChanged;
            }
            
            // 初始化UI
            RefreshShopItems();
            UpdateCurrencyDisplay();
            UpdateCartBadge();
        }
        
        private void OnDestroy()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnItemUnlocked -= OnItemUnlocked;
                ShopManager.Instance.OnStockChanged -= OnStockChanged;
                ShopManager.Instance.OnCartUpdated -= OnCartUpdated;
                ShopManager.Instance.OnCartCleared -= OnCartCleared;
            }
            
            if (CurrencySystem.Instance != null)
            {
                CurrencySystem.Instance.OnCurrencyChanged -= OnCurrencyChanged;
            }
        }
        
        #region 初始化设置
        
        private void SetupButtons()
        {
            if (allButton != null)
                allButton.onClick.AddListener(() => SetFilter(ItemType.Weapon, true));
            
            if (weaponsButton != null)
                weaponsButton.onClick.AddListener(() => SetFilter(ItemType.Weapon));
            
            if (partsButton != null)
                partsButton.onClick.AddListener(() => SetFilter(ItemType.MechaPart));
            
            if (consumablesButton != null)
                consumablesButton.onClick.AddListener(() => SetFilter(ItemType.Consumable));
            
            if (upgradesButton != null)
                upgradesButton.onClick.AddListener(() => SetFilter(ItemType.ModuleUpgrade));
            
            if (searchButton != null)
                searchButton.onClick.AddListener(OnSearch);
            
            if (clearSearchButton != null)
                clearSearchButton.onClick.AddListener(ClearSearch);
            
            if (toggleCartButton != null)
                toggleCartButton.onClick.AddListener(ToggleCart);
            
            if (searchInput != null)
            {
                searchInput.onEndEdit.AddListener(OnSearchInputChanged);
            }
        }
        
        private void SetupSortDropdown()
        {
            if (sortDropdown == null) return;
            
            sortDropdown.ClearOptions();
            sortDropdown.AddOptions(new List<string>
            {
                "默认排序",
                "价格: 低到高",
                "价格: 高到低",
                "稀有度: 低到高",
                "稀有度: 高到低",
                "名称: A-Z",
                "名称: Z-A"
            });
            
            sortDropdown.onValueChanged.AddListener(OnSortChanged);
        }
        
        #endregion
        
        #region 筛选与排序
        
        private void SetFilter(ItemType type, bool showAll = false)
        {
            _showAllTypes = showAll;
            _currentFilter = type;
            RefreshShopItems();
        }
        
        private void OnSortChanged(int index)
        {
            _currentSort = (SortType)index;
            RefreshShopItems();
        }
        
        private void OnSearch()
        {
            _searchKeyword = searchInput?.text ?? "";
            RefreshShopItems();
        }
        
        private void OnSearchInputChanged(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                ClearSearch();
            }
        }
        
        private void ClearSearch()
        {
            _searchKeyword = "";
            if (searchInput != null)
                searchInput.text = "";
            RefreshShopItems();
        }
        
        #endregion
        
        #region 商品列表刷新
        
        private void RefreshShopItems()
        {
            if (ShopManager.Instance == null) return;
            
            // 清除旧项
            foreach (var ui in _itemUIs)
            {
                if (ui != null)
                    Destroy(ui.gameObject);
            }
            _itemUIs.Clear();
            
            // 获取商品列表
            List<ShopStockItem> items;
            
            if (!string.IsNullOrWhiteSpace(_searchKeyword))
            {
                items = ShopManager.Instance.SearchItems(_searchKeyword);
            }
            else if (_showAllTypes)
            {
                items = ShopManager.Instance.GetAllItems();
            }
            else
            {
                items = ShopManager.Instance.GetItemsByType(_currentFilter);
            }
            
            // 排序
            items = SortItems(items);
            
            // 创建UI
            foreach (var stockItem in items)
            {
                if (shopItemPrefab != null && itemsContainer != null)
                {
                    var obj = Instantiate(shopItemPrefab, itemsContainer);
                    var itemUI = obj.GetComponent<ShopItemUI>();
                    if (itemUI != null)
                    {
                        itemUI.SetItem(stockItem);
                        itemUI.OnItemClicked += OnItemClicked;
                        itemUI.OnAddToCartClicked += OnAddToCartClicked;
                        _itemUIs.Add(itemUI);
                    }
                }
            }
            
            // 重置滚动位置
            if (itemsScrollRect != null)
                itemsScrollRect.normalizedPosition = new Vector2(0, 1);
        }
        
        private List<ShopStockItem> SortItems(List<ShopStockItem> items)
        {
            return _currentSort switch
            {
                SortType.PriceAsc => items.OrderBy(i => i.ItemData.basePrice).ToList(),
                SortType.PriceDesc => items.OrderByDescending(i => i.ItemData.basePrice).ToList(),
                SortType.RarityAsc => items.OrderBy(i => (int)i.ItemData.rarity).ToList(),
                SortType.RarityDesc => items.OrderByDescending(i => (int)i.ItemData.rarity).ToList(),
                SortType.NameAsc => items.OrderBy(i => i.ItemData.itemName).ToList(),
                SortType.NameDesc => items.OrderByDescending(i => i.ItemData.itemName).ToList(),
                _ => items
            };
        }
        
        #endregion
        
        #region 事件处理
        
        private void OnItemClicked(ShopItemData item)
        {
            if (detailPanel != null && ShopManager.Instance != null)
            {
                var stockItem = ShopManager.Instance.GetStockItem(item.itemId);
                if (stockItem != null)
                {
                    detailPanel.ShowItemDetail(stockItem);
                    detailPanel.OnPurchaseClicked += OnDetailPurchaseClicked;
                    detailPanel.OnAddToCartClicked += OnDetailAddToCartClicked;
                }
            }
        }
        
        private void OnAddToCartClicked(ShopItemData item)
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.AddToCart(item);
            }
        }
        
        private void OnDetailPurchaseClicked(ShopItemData item, int quantity)
        {
            if (ShopManager.Instance != null)
            {
                var result = ShopManager.Instance.PurchaseItem(item, quantity);
                if (!result.Success)
                {
                    Debug.LogWarning(result.Message);
                }
            }
            
            if (detailPanel != null)
            {
                detailPanel.OnPurchaseClicked -= OnDetailPurchaseClicked;
                detailPanel.OnAddToCartClicked -= OnDetailAddToCartClicked;
                detailPanel.Close();
            }
        }
        
        private void OnDetailAddToCartClicked(ShopItemData item, int quantity)
        {
            if (ShopManager.Instance != null)
            {
                for (int i = 0; i < quantity; i++)
                {
                    ShopManager.Instance.AddToCart(item);
                }
            }
            
            if (detailPanel != null)
            {
                detailPanel.OnPurchaseClicked -= OnDetailPurchaseClicked;
                detailPanel.OnAddToCartClicked -= OnDetailAddToCartClicked;
                detailPanel.Close();
            }
        }
        
        private void OnItemUnlocked(ShopItemData item)
        {
            RefreshShopItems();
        }
        
        private void OnStockChanged(ShopStockItem stockItem)
        {
            // 更新对应UI项
            var itemUI = _itemUIs.FirstOrDefault(ui => ui.ItemData == stockItem.ItemData);
            if (itemUI != null)
            {
                itemUI.UpdateUI();
            }
        }
        
        private void OnCartUpdated(CartItem cartItem)
        {
            UpdateCartBadge();
        }
        
        private void OnCartCleared()
        {
            UpdateCartBadge();
        }
        
        private void OnCurrencyChanged(CurrencyChangedEvent evt)
        {
            UpdateCurrencyDisplay();
        }
        
        #endregion
        
        #region UI更新
        
        private void UpdateCurrencyDisplay()
        {
            if (CurrencySystem.Instance == null) return;
            
            if (creditsText != null)
                creditsText.text = $"{CurrencySystem.Instance.Credits:N0} CR";
            
            if (premiumCreditsText != null)
                premiumCreditsText.text = $"{CurrencySystem.Instance.PremiumCredits:N0} PCR";
        }
        
        private void UpdateCartBadge()
        {
            if (ShopManager.Instance == null) return;
            
            int count = ShopManager.Instance.CartItemCount;
            if (cartBadgeText != null)
            {
                cartBadgeText.text = count.ToString();
                cartBadgeText.transform.parent.gameObject.SetActive(count > 0);
            }
        }
        
        private void ToggleCart()
        {
            if (cartPanel != null)
            {
                cartPanel.SetActive(!cartPanel.activeSelf);
            }
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 打开商店
        /// </summary>
        public void OpenShop()
        {
            gameObject.SetActive(true);
            RefreshShopItems();
            UpdateCurrencyDisplay();
        }
        
        /// <summary>
        /// 关闭商店
        /// </summary>
        public void CloseShop()
        {
            gameObject.SetActive(false);
        }
        
        #endregion
    }
}