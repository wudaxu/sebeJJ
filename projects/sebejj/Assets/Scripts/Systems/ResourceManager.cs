using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SebeJJ.Systems
{
    /// <summary>
    /// 资源管理器 - 管理氧气、能源、背包和货币
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }
        
        [Header("氧气设置")]
        public float maxOxygen = 100f;
        public float oxygenDepletionRate = 1f; // 每秒消耗
        public float oxygenWarningThreshold = 20f;
        
        [Header("能源设置")]
        public float maxEnergy = 100f;
        public float energyRechargeRate = 2f; // 每秒恢复
        
        [Header("背包设置")]
        public float maxWeight = 50f;
        
        // 当前数值
        public float CurrentOxygen { get; private set; }
        public float CurrentEnergy { get; private set; }
        public int Credits { get; private set; }
        public Inventory Inventory { get; private set; }
        
        // 属性
        public float MaxOxygen => maxOxygen;
        public float MaxEnergy => maxEnergy;
        public float CurrentWeight { get; private set; }
        public float MaxWeight => maxWeight;
        public bool IsOxygenLow => CurrentOxygen <= oxygenWarningThreshold;
        public bool IsInventoryFull => CurrentWeight >= maxWeight;
        
        // 事件
        public event Action<float> OnOxygenChanged;
        public event Action<float> OnEnergyChanged;
        public event Action<int> OnCreditsChanged;
        public event Action OnOxygenDepleted;
        public event Action OnInventoryChanged;
        public event Action OnOxygenLowWarning;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            Inventory = new Inventory();
        }
        
        private void Start()
        {
            InitializeNewGame();
        }
        
        private void Update()
        {
            UpdateOxygen();
            UpdateEnergy();
            UpdateUI();
        }
        
        /// <summary>
        /// 初始化新游戏
        /// </summary>
        public void InitializeNewGame()
        {
            CurrentOxygen = maxOxygen;
            CurrentEnergy = maxEnergy;
            Credits = 0;
            CurrentWeight = 0f;
            Inventory.Clear();
            
            OnOxygenChanged?.Invoke(CurrentOxygen);
            OnEnergyChanged?.Invoke(CurrentEnergy);
            OnCreditsChanged?.Invoke(Credits);
        }
        
        /// <summary>
        /// 更新氧气
        /// </summary>
        private void UpdateOxygen()
        {
            if (CurrentOxygen > 0)
            {
                float depletion = oxygenDepletionRate * Time.deltaTime;
                
                // 深度影响氧气消耗
                var diveManager = Core.GameManager.Instance?.diveManager;
                if (diveManager != null)
                {
                    depletion *= diveManager.GetOxygenConsumptionMultiplier();
                }
                
                CurrentOxygen = Mathf.Max(0f, CurrentOxygen - depletion);
                OnOxygenChanged?.Invoke(CurrentOxygen);
                
                // 低氧气警告
                if (CurrentOxygen <= oxygenWarningThreshold && 
                    CurrentOxygen > oxygenWarningThreshold - oxygenDepletionRate * Time.deltaTime)
                {
                    OnOxygenLowWarning?.Invoke();
                }
                
                // 氧气耗尽
                if (CurrentOxygen <= 0)
                {
                    OnOxygenDepleted?.Invoke();
                }
            }
        }
        
        /// <summary>
        /// 更新能源
        /// </summary>
        private void UpdateEnergy()
        {
            if (CurrentEnergy < maxEnergy)
            {
                CurrentEnergy = Mathf.Min(maxEnergy, CurrentEnergy + energyRechargeRate * Time.deltaTime);
                OnEnergyChanged?.Invoke(CurrentEnergy);
            }
        }
        
        /// <summary>
        /// 消耗氧气
        /// </summary>
        public bool ConsumeOxygen(float amount)
        {
            if (CurrentOxygen >= amount)
            {
                CurrentOxygen -= amount;
                OnOxygenChanged?.Invoke(CurrentOxygen);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 补充氧气
        /// </summary>
        public void RefillOxygen(float amount)
        {
            CurrentOxygen = Mathf.Min(maxOxygen, CurrentOxygen + amount);
            OnOxygenChanged?.Invoke(CurrentOxygen);
        }
        
        /// <summary>
        /// 消耗能源
        /// </summary>
        public bool ConsumeEnergy(float amount)
        {
            if (CurrentEnergy >= amount)
            {
                CurrentEnergy -= amount;
                OnEnergyChanged?.Invoke(CurrentEnergy);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 补充能源
        /// </summary>
        public void RechargeEnergy(float amount)
        {
            CurrentEnergy = Mathf.Min(maxEnergy, CurrentEnergy + amount);
            OnEnergyChanged?.Invoke(CurrentEnergy);
        }
        
        /// <summary>
        /// 增加货币
        /// </summary>
        public void AddCredits(int amount)
        {
            Credits += amount;
            OnCreditsChanged?.Invoke(Credits);
            Debug.Log($"[ResourceManager] 获得 {amount} 信用点，当前: {Credits}");
        }
        
        /// <summary>
        /// 消耗货币
        /// </summary>
        public bool SpendCredits(int amount)
        {
            if (Credits >= amount)
            {
                Credits -= amount;
                OnCreditsChanged?.Invoke(Credits);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 添加物品到背包
        /// </summary>
        public bool AddToInventory(Data.InventoryItem item)
        {
            // BUG-002, BUG-009 修复: 正确计算背包重量
            float itemTotalWeight = item.weight * item.quantity;
            if (CurrentWeight + itemTotalWeight > maxWeight)
            {
                Debug.LogWarning("[ResourceManager] 背包超重，无法添加物品");
                return false;
            }
            
            Inventory.AddItem(item);
            CurrentWeight = Inventory.GetTotalWeight(); // 重新计算总重量
            
            OnInventoryChanged?.Invoke();
            Debug.Log($"[ResourceManager] 添加物品: {item.itemName} x{item.quantity}");
            
            return true;
        }
        
        /// <summary>
        /// 从背包移除物品
        /// </summary>
        public bool RemoveFromInventory(string itemId, int quantity = 1)
        {
            var item = Inventory.GetItem(itemId);
            if (item == null || item.quantity < quantity) return false;
            
            Inventory.RemoveItem(itemId, quantity);
            CurrentWeight = Inventory.GetTotalWeight(); // BUG-009 修复: 重新计算总重量
            
            OnInventoryChanged?.Invoke();
            return true;
        }
        
        /// <summary>
        /// 清空背包
        /// </summary>
        public void ClearInventory()
        {
            Inventory.Clear();
            CurrentWeight = 0f;
            OnInventoryChanged?.Invoke();
        }
        
        /// <summary>
        /// 出售背包中所有物品
        /// </summary>
        public int SellAllItems()
        {
            int totalValue = Inventory.GetTotalValue();
            AddCredits(totalValue);
            ClearInventory();
            
            Debug.Log($"[ResourceManager] 出售所有物品，获得 {totalValue} 信用点");
            return totalValue;
        }
        
        /// <summary>
        /// 更新UI
        /// </summary>
        private void UpdateUI()
        {
            var uiManager = Core.UIManager.Instance;
            if (uiManager != null)
            {
                uiManager.UpdateOxygenBar(CurrentOxygen, maxOxygen);
                uiManager.UpdateEnergyBar(CurrentEnergy, maxEnergy);
            }
        }
        
        /// <summary>
        /// 从存档加载
        /// </summary>
        public void LoadFromSave(Core.GameSaveData saveData)
        {
            CurrentOxygen = saveData.oxygen;
            CurrentEnergy = saveData.energy;
            Credits = saveData.credits;
            maxOxygen = saveData.maxOxygen;
            maxEnergy = saveData.maxEnergy;
            
            // 加载背包
            if (!string.IsNullOrEmpty(saveData.inventoryData))
            {
                Inventory = JsonUtility.FromJson<Inventory>(saveData.inventoryData);
                RecalculateWeight();
            }
            
            OnOxygenChanged?.Invoke(CurrentOxygen);
            OnEnergyChanged?.Invoke(CurrentEnergy);
            OnCreditsChanged?.Invoke(Credits);
            OnInventoryChanged?.Invoke();
        }
        
        /// <summary>
        /// 获取背包存档数据
        /// </summary>
        public string GetInventorySaveData()
        {
            return JsonUtility.ToJson(Inventory);
        }
        
        /// <summary>
        /// 获取物品数量
        /// </summary>
        public int GetItemCount(string itemId)
        {
            return Inventory.GetItemCount(itemId);
        }
        
        /// <summary>
        /// 检查背包中是否有指定物品
        /// </summary>
        public bool HasItem(string itemId, int quantity = 1)
        {
            return Inventory.GetItemCount(itemId) >= quantity;
        }
        
        /// <summary>
        /// 获取背包物品列表
        /// </summary>
        public IReadOnlyList<Data.InventoryItem> GetInventoryItems()
        {
            return Inventory.Items;
        }
        
        private void RecalculateWeight()
        {
            CurrentWeight = Inventory.GetTotalWeight();
        }
    }
    
    /// <summary>
    /// 背包系统
    /// </summary>
    [Serializable]
    public class Inventory
    {
        [SerializeField]
        private List<Data.InventoryItem> items = new List<Data.InventoryItem>();
        
        public IReadOnlyList<Data.InventoryItem> Items => items;
        
        public void AddItem(Data.InventoryItem item)
        {
            var existing = items.Find(i => i.itemId == item.itemId);
            if (existing != null)
            {
                existing.quantity += item.quantity;
            }
            else
            {
                items.Add(item);
            }
        }
        
        public void RemoveItem(string itemId, int quantity)
        {
            var item = items.Find(i => i.itemId == itemId);
            if (item != null)
            {
                item.quantity -= quantity;
                if (item.quantity <= 0)
                {
                    items.Remove(item);
                }
            }
        }
        
        public Data.InventoryItem GetItem(string itemId)
        {
            return items.Find(i => i.itemId == itemId);
        }
        
        public int GetItemCount(string itemId)
        {
            var item = items.Find(i => i.itemId == itemId);
            return item?.quantity ?? 0;
        }
        
        public void Clear()
        {
            items.Clear();
        }
        
        public float GetTotalWeight()
        {
            float weight = 0f;
            foreach (var item in items)
            {
                weight += item.weight * item.quantity;
            }
            return weight;
        }
        
        public int GetTotalValue()
        {
            int value = 0;
            foreach (var item in items)
            {
                value += item.value * item.quantity;
            }
            return value;
        }
    }
}
