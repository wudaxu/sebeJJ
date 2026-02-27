using UnityEngine;
using System.Collections.Generic;
using SebeJJ.Core;

namespace SebeJJ.Mech
{
    /// <summary>
    /// 资源库存组件
    /// </summary>
    public class ResourceInventory : MonoBehaviour
    {
        [Header("容量")]
        [SerializeField] private int maxCapacity = 100;
        [SerializeField] private int currentCapacity = 0;

        [Header("设置")]
        [SerializeField] private bool autoNotifyUI = true;

        // 资源存储
        private Dictionary<ResourceType, int> _resources = new Dictionary<ResourceType, int>();

        // 属性
        public int MaxCapacity => maxCapacity;
        public int CurrentCapacity => currentCapacity;
        public int RemainingCapacity => maxCapacity - currentCapacity;
        public bool IsFull => currentCapacity >= maxCapacity;
        public bool IsEmpty => currentCapacity <= 0;

        // 事件
        public System.Action<ResourceType, int> OnResourceAdded;
        public System.Action<ResourceType, int> OnResourceRemoved;
        public System.Action OnInventoryFull;
        public System.Action<int, int> OnCapacityChanged; // 当前, 最大

        private void Start()
        {
            // 初始化所有资源类型
            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                if (!_resources.ContainsKey(type))
                {
                    _resources[type] = 0;
                }
            }

            if (autoNotifyUI)
            {
                NotifyCapacityChanged();
            }
        }

        /// <summary>
        /// 添加资源
        /// </summary>
        public bool AddResource(ResourceType type, int amount)
        {
            if (amount <= 0) return false;
            if (IsFull) return false;
            if (currentCapacity + amount > maxCapacity)
            {
                amount = maxCapacity - currentCapacity;
            }

            if (!_resources.ContainsKey(type))
            {
                _resources[type] = 0;
            }

            _resources[type] += amount;
            currentCapacity += amount;

            OnResourceAdded?.Invoke(type, amount);
            
            if (autoNotifyUI)
            {
                NotifyCapacityChanged();
            }

            if (IsFull)
            {
                OnInventoryFull?.Invoke();
            }

            return true;
        }

        /// <summary>
        /// 移除资源
        /// </summary>
        public bool RemoveResource(ResourceType type, int amount)
        {
            if (amount <= 0) return false;
            if (!_resources.ContainsKey(type) || _resources[type] < amount) return false;

            _resources[type] -= amount;
            currentCapacity -= amount;

            OnResourceRemoved?.Invoke(type, amount);
            
            if (autoNotifyUI)
            {
                NotifyCapacityChanged();
            }

            return true;
        }

        /// <summary>
        /// 获取资源数量
        /// </summary>
        public int GetResourceAmount(ResourceType type)
        {
            return _resources.ContainsKey(type) ? _resources[type] : 0;
        }

        /// <summary>
        /// 检查是否有足够资源
        /// </summary>
        public bool HasResource(ResourceType type, int amount)
        {
            return GetResourceAmount(type) >= amount;
        }

        /// <summary>
        /// 获取所有资源
        /// </summary>
        public Dictionary<ResourceType, int> GetAllResources()
        {
            return new Dictionary<ResourceType, int>(_resources);
        }

        /// <summary>
        /// 清空库存
        /// </summary>
        public void ClearInventory()
        {
            _resources.Clear();
            currentCapacity = 0;

            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                _resources[type] = 0;
            }

            if (autoNotifyUI)
            {
                NotifyCapacityChanged();
            }
        }

        /// <summary>
        /// 设置最大容量
        /// </summary>
        public void SetMaxCapacity(int newCapacity)
        {
            maxCapacity = newCapacity;
            currentCapacity = Mathf.Min(currentCapacity, maxCapacity);
            
            if (autoNotifyUI)
            {
                NotifyCapacityChanged();
            }
        }

        /// <summary>
        /// 通知UI更新
        /// </summary>
        private void NotifyCapacityChanged()
        {
            OnCapacityChanged?.Invoke(currentCapacity, maxCapacity);
            GameEvents.OnCargoChanged?.Invoke(currentCapacity, maxCapacity);
        }

        /// <summary>
        /// 计算资源总价值
        /// </summary>
        public int CalculateTotalValue()
        {
            int totalValue = 0;
            
            foreach (var kvp in _resources)
            {
                int value = GetResourceValue(kvp.Key);
                totalValue += kvp.Value * value;
            }

            return totalValue;
        }

        /// <summary>
        /// 获取资源价值（可扩展为配置表）
        /// </summary>
        private int GetResourceValue(ResourceType type)
        {
            return type switch
            {
                ResourceType.ScrapMetal => 10,
                ResourceType.CopperOre => 20,
                ResourceType.IronOre => 25,
                ResourceType.GoldOre => 100,
                ResourceType.CrystalShard => 50,
                ResourceType.Uranium => 200,
                ResourceType.BioSample => 75,
                ResourceType.DataFragment => 150,
                ResourceType.AncientTech => 500,
                _ => 0
            };
        }
    }
}
