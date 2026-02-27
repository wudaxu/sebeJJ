using System;
using System.Collections.Generic;
using UnityEngine;
using SebeJJ.Utils;
using SebeJJ.Core;

namespace SebeJJ.Economy
{
    /// <summary>
    /// 资源数据
    /// </summary>
    [Serializable]
    public class ResourceData
    {
        public ResourceType type;
        public string displayName;
        public string description;
        public Sprite icon;
        public int baseValue;
        public int maxStack;
        public float weight;
        public bool isRare;
        public bool isQuestItem;
    }

    /// <summary>
    /// 库存物品
    /// </summary>
    [Serializable]
    public class InventoryItem
    {
        public ResourceType type;
        public int count;
        public int quality;  // 1-100

        public InventoryItem(ResourceType type, int count, int quality = 100)
        {
            this.type = type;
            this.count = count;
            this.quality = quality;
        }
    }

    /// <summary>
    /// 资源管理器 - 管理所有资源相关的逻辑
    /// </summary>
    public class ResourceManager : Singleton<ResourceManager>
    {
        [Header("Settings")]
        [SerializeField] private int baseCargoCapacity = 50;
        [SerializeField] private bool unlimitedCargo = false;

        [Header("Resource Database")]
        [SerializeField] private List<ResourceData> resourceDatabase = new List<ResourceData>();

        [Header("Events")]
        [SerializeField] private bool showDebugLogs = true;

        // 库存
        private List<InventoryItem> _inventory = new List<InventoryItem>();
        private int _currentCargoCapacity;

        // 属性
        public IReadOnlyList<InventoryItem> Inventory => _inventory;
        public int CurrentCargoWeight { get; private set; }
        public int MaxCargoCapacity => _currentCargoCapacity;
        public int AvailableSpace => _currentCargoCapacity - CurrentCargoWeight;
        public bool IsCargoFull => CurrentCargoWeight >= _currentCargoCapacity;
        public bool IsInventoryEmpty => _inventory.Count == 0;

        #region Unity Lifecycle

        protected override void OnAwake()
        {
            base.OnAwake();
            _currentCargoCapacity = baseCargoCapacity;
        }

        private void Start()
        {
            InitializeResourceDatabase();
        }

        #endregion

        #region Initialization

        private void InitializeResourceDatabase()
        {
            // 如果数据库为空，创建默认资源
            if (resourceDatabase.Count == 0)
            {
                CreateDefaultResources();
            }
        }

        private void CreateDefaultResources()
        {
            resourceDatabase.Add(new ResourceData
            {
                type = ResourceType.ScrapMetal,
                displayName = "废金属",
                description = "废弃的金属碎片，可以回收使用。",
                baseValue = 10,
                maxStack = 99,
                weight = 1
            });

            resourceDatabase.Add(new ResourceData
            {
                type = ResourceType.CopperOre,
                displayName = "铜矿",
                description = "含有铜元素的矿石。",
                baseValue = 25,
                maxStack = 50,
                weight = 2
            });

            resourceDatabase.Add(new ResourceData
            {
                type = ResourceType.IronOre,
                displayName = "铁矿",
                description = "含有铁元素的矿石。",
                baseValue = 30,
                maxStack = 50,
                weight = 2
            });

            resourceDatabase.Add(new ResourceData
            {
                type = ResourceType.GoldOre,
                displayName = "金矿",
                description = "珍贵的金矿石。",
                baseValue = 100,
                maxStack = 20,
                weight = 3,
                isRare = true
            });

            resourceDatabase.Add(new ResourceData
            {
                type = ResourceType.CrystalShard,
                displayName = "水晶碎片",
                description = "散发着微弱光芒的神秘水晶。",
                baseValue = 150,
                maxStack = 10,
                weight = 1,
                isRare = true
            });

            resourceDatabase.Add(new ResourceData
            {
                type = ResourceType.Uranium,
                displayName = "铀",
                description = "放射性元素，用于高级能源。",
                baseValue = 200,
                maxStack = 5,
                weight = 5,
                isRare = true
            });

            resourceDatabase.Add(new ResourceData
            {
                type = ResourceType.BioSample,
                displayName = "生物样本",
                description = "深海生物的组织样本。",
                baseValue = 75,
                maxStack = 20,
                weight = 1
            });

            resourceDatabase.Add(new ResourceData
            {
                type = ResourceType.DataFragment,
                displayName = "数据碎片",
                description = "损坏的数据存储设备，可能包含有用信息。",
                baseValue = 50,
                maxStack = 30,
                weight = 0
            });

            resourceDatabase.Add(new ResourceData
            {
                type = ResourceType.AncientTech,
                displayName = "古代科技",
                description = "来历不明的古代科技遗物。",
                baseValue = 500,
                maxStack = 5,
                weight = 2,
                isRare = true
            });
        }

        #endregion

        #region Resource Database

        /// <summary>
        /// 获取资源数据
        /// </summary>
        public ResourceData GetResourceData(ResourceType type)
        {
            return resourceDatabase.Find(r => r.type == type);
        }

        /// <summary>
        /// 添加资源定义
        /// </summary>
        public void AddResourceData(ResourceData data)
        {
            if (data != null && !resourceDatabase.Exists(r => r.type == data.type))
            {
                resourceDatabase.Add(data);
            }
        }

        #endregion

        #region Inventory Management

        /// <summary>
        /// 添加资源到库存
        /// </summary>
        public bool AddResource(ResourceType type, int amount, int quality = 100)
        {
            if (type == ResourceType.None || amount <= 0) return false;

            var resourceData = GetResourceData(type);
            if (resourceData == null)
            {
                LogWarning($"Resource type {type} not found in database.");
                return false;
            }

            // 检查货舱空间
            int totalWeight = amount * resourceData.weight;
            if (!unlimitedCargo && CurrentCargoWeight + totalWeight > _currentCargoCapacity)
            {
                Log("Cargo full! Cannot add resource.");
                GameEvents.OnShowWarning?.Invoke("货舱已满！");
                return false;
            }

            // 查找是否已有相同类型的物品
            var existingItem = _inventory.Find(item => item.type == type);
            if (existingItem != null)
            {
                // 检查堆叠限制
                int canAdd = Mathf.Min(amount, resourceData.maxStack - existingItem.count);
                if (canAdd > 0)
                {
                    existingItem.count += canAdd;
                    CurrentCargoWeight += canAdd * resourceData.weight;
                    
                    // 更新存档
                    SaveManager.Instance?.AddResource(type, canAdd);
                    
                    Log($"Added {canAdd} {resourceData.displayName} to existing stack.");
                    
                    // 如果还有剩余，尝试创建新堆叠
                    if (canAdd < amount)
                    {
                        return AddResource(type, amount - canAdd, quality);
                    }
                    
                    UpdateCargoUI();
                    return true;
                }
                else
                {
                    // 创建新堆叠
                    return CreateNewStack(type, amount, quality);
                }
            }
            else
            {
                return CreateNewStack(type, amount, quality);
            }
        }

        private bool CreateNewStack(ResourceType type, int amount, int quality)
        {
            var resourceData = GetResourceData(type);
            if (resourceData == null) return false;

            int stackAmount = Mathf.Min(amount, resourceData.maxStack);
            var newItem = new InventoryItem(type, stackAmount, quality);
            _inventory.Add(newItem);
            
            CurrentCargoWeight += stackAmount * resourceData.weight;
            
            // 更新存档
            SaveManager.Instance?.AddResource(type, stackAmount);
            
            Log($"Created new stack of {stackAmount} {resourceData.displayName}.");
            
            // 如果还有剩余，继续添加
            if (stackAmount < amount)
            {
                return AddResource(type, amount - stackAmount, quality);
            }
            
            UpdateCargoUI();
            return true;
        }

        /// <summary>
        /// 移除资源
        /// </summary>
        public bool RemoveResource(ResourceType type, int amount)
        {
            if (type == ResourceType.None || amount <= 0) return false;

            var item = _inventory.Find(i => i.type == type);
            if (item == null || item.count < amount) return false;

            var resourceData = GetResourceData(type);
            
            item.count -= amount;
            if (resourceData != null)
            {
                CurrentCargoWeight -= amount * resourceData.weight;
            }

            if (item.count <= 0)
            {
                _inventory.Remove(item);
            }

            Log($"Removed {amount} {type}.");
            UpdateCargoUI();
            return true;
        }

        /// <summary>
        /// 获取资源数量
        /// </summary>
        public int GetResourceCount(ResourceType type)
        {
            var item = _inventory.Find(i => i.type == type);
            return item?.count ?? 0;
        }

        /// <summary>
        /// 检查是否有足够资源
        /// </summary>
        public bool HasResource(ResourceType type, int amount)
        {
            return GetResourceCount(type) >= amount;
        }

        /// <summary>
        /// 清空库存
        /// </summary>
        public void ClearInventory()
        {
            _inventory.Clear();
            CurrentCargoWeight = 0;
            UpdateCargoUI();
            Log("Inventory cleared.");
        }

        #endregion

        #region Cargo Capacity

        /// <summary>
        /// 设置货舱容量
        /// </summary>
        public void SetCargoCapacity(int capacity)
        {
            _currentCargoCapacity = Mathf.Max(1, capacity);
            UpdateCargoUI();
        }

        /// <summary>
        /// 增加货舱容量
        /// </summary>
        public void AddCargoCapacity(int amount)
        {
            _currentCargoCapacity += amount;
            UpdateCargoUI();
        }

        /// <summary>
        /// 获取当前货舱使用率 (0-1)
        /// </summary>
        public float GetCargoUsagePercent()
        {
            if (_currentCargoCapacity <= 0) return 0f;
            return (float)CurrentCargoWeight / _currentCargoCapacity;
        }

        #endregion

        #region Selling

        /// <summary>
        /// 出售资源
        /// </summary>
        public int SellResource(ResourceType type, int amount, float priceMultiplier = 1f)
        {
            if (!RemoveResource(type, amount)) return 0;

            var resourceData = GetResourceData(type);
            if (resourceData == null) return 0;

            int totalValue = Mathf.RoundToInt(resourceData.baseValue * amount * priceMultiplier);
            SaveManager.Instance?.AddCurrency(totalValue);

            Log($"Sold {amount} {resourceData.displayName} for {totalValue} credits.");
            return totalValue;
        }

        /// <summary>
        /// 出售所有资源
        /// </summary>
        public int SellAllResources(float priceMultiplier = 1f)
        {
            int totalValue = 0;
            var itemsToSell = new List<InventoryItem>(_inventory);

            foreach (var item in itemsToSell)
            {
                totalValue += SellResource(item.type, item.count, priceMultiplier);
            }

            return totalValue;
        }

        /// <summary>
        /// 获取库存总价值
        /// </summary>
        public int GetInventoryValue()
        {
            int totalValue = 0;
            foreach (var item in _inventory)
            {
                var resourceData = GetResourceData(item.type);
                if (resourceData != null)
                {
                    totalValue += resourceData.baseValue * item.count;
                }
            }
            return totalValue;
        }

        #endregion

        #region UI Updates

        private void UpdateCargoUI()
        {
            GameEvents.OnCargoChanged?.Invoke(CurrentCargoWeight, _currentCargoCapacity);
        }

        #endregion

        #region Save/Load

        /// <summary>
        /// 从存档加载库存
        /// </summary>
        public void LoadInventory(List<InventoryItem> items)
        {
            _inventory.Clear();
            CurrentCargoWeight = 0;

            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item.type != ResourceType.None && item.count > 0)
                    {
                        _inventory.Add(item);
                        var resourceData = GetResourceData(item.type);
                        if (resourceData != null)
                        {
                            CurrentCargoWeight += item.count * resourceData.weight;
                        }
                    }
                }
            }

            UpdateCargoUI();
        }

        /// <summary>
        /// 获取库存数据用于存档
        /// </summary>
        public List<InventoryItem> GetInventoryData()
        {
            return new List<InventoryItem>(_inventory);
        }

        #endregion

        #region Debug

        private void Log(string message)
        {
            if (showDebugLogs)
                Debug.Log($"[ResourceManager] {message}");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[ResourceManager] {message}");
        }

        #endregion
    }
}