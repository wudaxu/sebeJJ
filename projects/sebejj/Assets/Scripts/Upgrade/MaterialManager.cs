using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SebeJJ.Upgrade
{
    /// <summary>
    /// 材料管理器 - 单例模式
    /// 负责管理所有升级材料的获取、消耗和合成
    /// </summary>
    public class MaterialManager : MonoBehaviour
    {
        public static MaterialManager Instance { get; private set; }
        
        [Header("材料配置")]
        public MaterialDatabase materialDatabase;
        
        [Header("当前材料")]
        [SerializeField] private Dictionary<string, int> inventory = new Dictionary<string, int>();
        
        // 事件
        public event Action<string, int> OnMaterialAdded;
        public event Action<string, int> OnMaterialRemoved;
        public event Action<string, int> OnMaterialConsumed;
        public event Action OnInventoryChanged;
        
        // 属性
        public IReadOnlyDictionary<string, int> Inventory => inventory;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeMaterials();
        }
        
        /// <summary>
        /// 初始化材料
        /// </summary>
        private void InitializeMaterials()
        {
            if (materialDatabase == null)
            {
                Debug.LogWarning("[MaterialManager] 材料数据库未设置，使用默认配置");
                CreateDefaultMaterials();
            }
            
            // 确保所有材料都有条目
            foreach (var material in materialDatabase.materials)
            {
                if (!inventory.ContainsKey(material.materialId))
                {
                    inventory[material.materialId] = 0;
                }
            }
        }
        
        /// <summary>
        /// 创建默认材料
        /// </summary>
        private void CreateDefaultMaterials()
        {
            materialDatabase = ScriptableObject.CreateInstance<MaterialDatabase>();
            materialDatabase.InitializeDefaults();
        }
        
        #region 材料获取
        
        /// <summary>
        /// 添加材料
        /// </summary>
        public void AddMaterial(string materialId, int amount)
        {
            if (amount <= 0) return;
            if (!IsValidMaterial(materialId))
            {
                Debug.LogWarning($"[MaterialManager] 尝试添加无效材料: {materialId}");
                return;
            }
            
            if (!inventory.ContainsKey(materialId))
            {
                inventory[materialId] = 0;
            }
            
            inventory[materialId] += amount;
            
            OnMaterialAdded?.Invoke(materialId, amount);
            OnInventoryChanged?.Invoke();
            
            Debug.Log($"[MaterialManager] 获得材料: {GetMaterialName(materialId)} x{amount}");
        }
        
        /// <summary>
        /// 批量添加材料
        /// </summary>
        public void AddMaterials(List<MaterialRequirement> materials)
        {
            foreach (var req in materials)
            {
                AddMaterial(req.materialId, req.amount);
            }
        }
        
        /// <summary>
        /// 从资源采集获取材料
        /// </summary>
        public void AddMaterialFromResource(string resourceType, int baseAmount)
        {
            // 根据资源类型转换为材料
            var conversion = materialDatabase?.GetResourceConversion(resourceType);
            if (conversion != null)
            {
                foreach (var output in conversion.outputs)
                {
                    int amount = Mathf.RoundToInt(baseAmount * output.amountMultiplier);
                    AddMaterial(output.materialId, amount);
                }
            }
            else
            {
                // 默认转换
                string defaultMaterialId = $"scrap_{resourceType.ToLower()}";
                AddMaterial(defaultMaterialId, baseAmount);
            }
        }
        
        /// <summary>
        /// 从敌人掉落获取材料
        /// </summary>
        public void AddMaterialFromEnemy(string enemyType, int enemyLevel)
        {
            var dropTable = materialDatabase?.GetEnemyDropTable(enemyType);
            if (dropTable != null)
            {
                foreach (var drop in dropTable.possibleDrops)
                {
                    float roll = UnityEngine.Random.value;
                    float adjustedChance = drop.dropChance + (enemyLevel * 0.02f); // 等级增加掉落率
                    
                    if (roll <= adjustedChance)
                    {
                        int amount = UnityEngine.Random.Range(drop.minAmount, drop.maxAmount + 1);
                        AddMaterial(drop.materialId, amount);
                    }
                }
            }
        }
        
        #endregion
        
        #region 材料消耗
        
        /// <summary>
        /// 检查是否有足够材料
        /// </summary>
        public bool HasMaterial(string materialId, int amount)
        {
            return inventory.TryGetValue(materialId, out int current) && current >= amount;
        }
        
        /// <summary>
        /// 检查是否有足够材料列表
        /// </summary>
        public bool HasMaterials(List<MaterialRequirement> requirements)
        {
            if (requirements == null) return true;
            
            return requirements.All(req => HasMaterial(req.materialId, req.amount));
        }
        
        /// <summary>
        /// 消耗材料
        /// </summary>
        public bool ConsumeMaterial(string materialId, int amount)
        {
            if (!HasMaterial(materialId, amount))
            {
                Debug.LogWarning($"[MaterialManager] 材料不足: {GetMaterialName(materialId)}");
                return false;
            }
            
            inventory[materialId] -= amount;
            
            OnMaterialConsumed?.Invoke(materialId, amount);
            OnInventoryChanged?.Invoke();
            
            Debug.Log($"[MaterialManager] 消耗材料: {GetMaterialName(materialId)} x{amount}");
            return true;
        }
        
        /// <summary>
        /// 批量消耗材料
        /// </summary>
        public bool ConsumeMaterials(List<MaterialRequirement> requirements)
        {
            if (!HasMaterials(requirements))
            {
                return false;
            }
            
            foreach (var req in requirements)
            {
                ConsumeMaterial(req.materialId, req.amount);
            }
            
            return true;
        }
        
        /// <summary>
        /// 移除材料（不触发消耗事件）
        /// </summary>
        public void RemoveMaterial(string materialId, int amount)
        {
            if (!inventory.ContainsKey(materialId)) return;
            
            inventory[materialId] = Mathf.Max(0, inventory[materialId] - amount);
            
            OnMaterialRemoved?.Invoke(materialId, amount);
            OnInventoryChanged?.Invoke();
        }
        
        #endregion
        
        #region 材料合成
        
        /// <summary>
        /// 检查是否可以合成
        /// </summary>
        public bool CanCraft(string recipeId)
        {
            var recipe = materialDatabase?.GetRecipe(recipeId);
            if (recipe == null) return false;
            
            return HasMaterials(recipe.inputs);
        }
        
        /// <summary>
        /// 执行合成
        /// </summary>
        public bool Craft(string recipeId)
        {
            var recipe = materialDatabase?.GetRecipe(recipeId);
            if (recipe == null)
            {
                Debug.LogWarning($"[MaterialManager] 配方不存在: {recipeId}");
                return false;
            }
            
            if (!CanCraft(recipeId))
            {
                Debug.LogWarning($"[MaterialManager] 材料不足，无法合成: {recipe.recipeName}");
                return false;
            }
            
            // 消耗输入材料
            foreach (var input in recipe.inputs)
            {
                ConsumeMaterial(input.materialId, input.amount);
            }
            
            // 产出结果
            foreach (var output in recipe.outputs)
            {
                AddMaterial(output.materialId, output.amount);
            }
            
            Debug.Log($"[MaterialManager] 合成成功: {recipe.recipeName}");
            return true;
        }
        
        /// <summary>
        /// 获取所有可用配方
        /// </summary>
        public List<CraftingRecipe> GetAvailableRecipes()
        {
            return materialDatabase?.recipes.FindAll(r => CanCraft(r.recipeId)) ?? new List<CraftingRecipe>();
        }
        
        /// <summary>
        /// 获取配方列表
        /// </summary>
        public List<CraftingRecipe> GetAllRecipes()
        {
            return materialDatabase?.recipes ?? new List<CraftingRecipe>();
        }
        
        #endregion
        
        #region 查询方法
        
        /// <summary>
        /// 获取材料数量
        /// </summary>
        public int GetMaterialCount(string materialId)
        {
            return inventory.TryGetValue(materialId, out int count) ? count : 0;
        }
        
        /// <summary>
        /// 获取材料信息
        /// </summary>
        public MaterialData GetMaterialInfo(string materialId)
        {
            return materialDatabase?.GetMaterial(materialId);
        }
        
        /// <summary>
        /// 获取材料名称
        /// </summary>
        public string GetMaterialName(string materialId)
        {
            var material = GetMaterialInfo(materialId);
            return material?.materialName ?? materialId;
        }
        
        /// <summary>
        /// 验证材料是否有效
        /// </summary>
        public bool IsValidMaterial(string materialId)
        {
            return materialDatabase?.GetMaterial(materialId) != null;
        }
        
        /// <summary>
        /// 获取所有材料
        /// </summary>
        public List<MaterialData> GetAllMaterials()
        {
            return materialDatabase?.materials ?? new List<MaterialData>();
        }
        
        /// <summary>
        /// 按稀有度获取材料
        /// </summary>
        public List<MaterialData> GetMaterialsByRarity(MaterialRarity rarity)
        {
            return materialDatabase?.materials.FindAll(m => m.rarity == rarity) ?? new List<MaterialData>();
        }
        
        /// <summary>
        /// 按类型获取材料
        /// </summary>
        public List<MaterialData> GetMaterialsByType(MaterialType type)
        {
            return materialDatabase?.materials.FindAll(m => m.materialType == type) ?? new List<MaterialData>();
        }
        
        #endregion
        
        #region 存档/读档
        
        /// <summary>
        /// 获取存档数据
        /// </summary>
        public Dictionary<string, int> GetSaveData()
        {
            return new Dictionary<string, int>(inventory);
        }
        
        /// <summary>
        /// 加载存档数据
        /// </summary>
        public void LoadSaveData(Dictionary<string, int> saveData)
        {
            if (saveData == null) return;
            
            inventory = new Dictionary<string, int>(saveData);
            OnInventoryChanged?.Invoke();
            
            Debug.Log("[MaterialManager] 材料数据加载完成");
        }
        
        /// <summary>
        /// 重置材料
        /// </summary>
        public void ResetMaterials()
        {
            inventory.Clear();
            InitializeMaterials();
            OnInventoryChanged?.Invoke();
        }
        
        #endregion
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}