using UnityEngine;
using System.Collections.Generic;

namespace SebeJJ.Upgrade
{
    /// <summary>
    /// 材料数据库 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "MaterialDatabase", menuName = "SebeJJ/Material Database")]
    public class MaterialDatabase : ScriptableObject
    {
        [Header("材料定义")]
        public List<MaterialData> materials = new List<MaterialData>();
        
        [Header("合成配方")]
        public List<CraftingRecipe> recipes = new List<CraftingRecipe>();
        
        [Header("资源转换")]
        public List<ResourceConversion> resourceConversions = new List<ResourceConversion>();
        
        [Header("敌人掉落")]
        public List<EnemyDropTable> enemyDropTables = new List<EnemyDropTable>();
        
        /// <summary>
        /// 获取材料
        /// </summary>
        public MaterialData GetMaterial(string materialId)
        {
            return materials.Find(m => m.materialId == materialId);
        }
        
        /// <summary>
        /// 获取配方
        /// </summary>
        public CraftingRecipe GetRecipe(string recipeId)
        {
            return recipes.Find(r => r.recipeId == recipeId);
        }
        
        /// <summary>
        /// 获取资源转换配置
        /// </summary>
        public ResourceConversion GetResourceConversion(string resourceType)
        {
            return resourceConversions.Find(r => r.resourceType == resourceType);
        }
        
        /// <summary>
        /// 获取敌人掉落表
        /// </summary>
        public EnemyDropTable GetEnemyDropTable(string enemyType)
        {
            return enemyDropTables.Find(e => e.enemyType == enemyType);
        }
        
        /// <summary>
        /// 初始化默认配置
        /// </summary>
        public void InitializeDefaults()
        {
            // 基础材料
            materials.Add(CreateMaterial("scrap_metal", "废金属", "从深海残骸中回收的金属材料", 
                MaterialType.Metal, MaterialRarity.Common, 100));
            
            materials.Add(CreateMaterial("scrap_circuit", "废弃电路", "损坏的电子元件，仍有回收价值", 
                MaterialType.Metal, MaterialRarity.Common, 150));
            
            materials.Add(CreateMaterial("coral_fragment", "珊瑚碎片", "深海珊瑚的碎片，具有独特的结构", 
                MaterialType.Organic, MaterialRarity.Common, 80));
            
            // 进阶材料
            materials.Add(CreateMaterial("energy_crystal", "能量水晶", "蕴含能量的天然水晶", 
                MaterialType.Crystal, MaterialRarity.Uncommon, 300));
            
            materials.Add(CreateMaterial("bioluminescent_gland", "发光腺体", "深海生物的发光器官", 
                MaterialType.Organic, MaterialRarity.Uncommon, 250));
            
            materials.Add(CreateMaterial("pressure_alloy", "耐压合金", "能承受深海高压的特殊合金", 
                MaterialType.Metal, MaterialRarity.Uncommon, 400));
            
            // 稀有材料
            materials.Add(CreateMaterial("ancient_core", "古代核心", "未知文明留下的能量核心", 
                MaterialType.Ancient, MaterialRarity.Rare, 1000));
            
            materials.Add(CreateMaterial("void_shard", "虚空碎片", "来自深海的神秘碎片", 
                MaterialType.Crystal, MaterialRarity.Rare, 800));
            
            materials.Add(CreateMaterial("abyssal_essence", "深渊精华", "浓缩的深海能量", 
                MaterialType.Energy, MaterialRarity.Rare, 1200));
            
            // 史诗材料
            materials.Add(CreateMaterial("titan_plate", "泰坦装甲板", "古代巨型机甲的装甲残片", 
                MaterialType.Ancient, MaterialRarity.Epic, 3000));
            
            materials.Add(CreateMaterial("neural_processor", "神经处理器", "超越现代科技的处理单元", 
                MaterialType.Ancient, MaterialRarity.Epic, 3500));
            
            // 传说材料
            materials.Add(CreateMaterial("heart_of_abyss", "深渊之心", "传说中的深海至宝", 
                MaterialType.Special, MaterialRarity.Legendary, 10000));
            
            // 初始化资源转换
            InitializeResourceConversions();
            
            // 初始化敌人掉落
            InitializeEnemyDrops();
            
            // 初始化合成配方
            InitializeRecipes();
            
            Debug.Log("[MaterialDatabase] 默认配置已初始化");
        }
        
        private MaterialData CreateMaterial(string id, string name, string desc, 
            MaterialType type, MaterialRarity rarity, int value)
        {
            return new MaterialData
            {
                materialId = id,
                materialName = name,
                description = desc,
                materialType = type,
                rarity = rarity,
                baseValue = value,
                maxStack = rarity >= MaterialRarity.Rare ? 99 : 999
            };
        }
        
        private void InitializeResourceConversions()
        {
            // 金属资源
            resourceConversions.Add(new ResourceConversion
            {
                resourceType = "Metal",
                outputs = new List<ConversionOutput>
                {
                    new ConversionOutput { materialId = "scrap_metal", amountMultiplier = 1f },
                    new ConversionOutput { materialId = "scrap_circuit", amountMultiplier = 0.3f, bonusChance = 0.2f }
                }
            });
            
            // 能量资源
            resourceConversions.Add(new ResourceConversion
            {
                resourceType = "Energy",
                outputs = new List<ConversionOutput>
                {
                    new ConversionOutput { materialId = "energy_crystal", amountMultiplier = 0.5f },
                    new ConversionOutput { materialId = "scrap_circuit", amountMultiplier = 0.5f }
                }
            });
            
            // 有机资源
            resourceConversions.Add(new ResourceConversion
            {
                resourceType = "Organic",
                outputs = new List<ConversionOutput>
                {
                    new ConversionOutput { materialId = "coral_fragment", amountMultiplier = 0.8f },
                    new ConversionOutput { materialId = "bioluminescent_gland", amountMultiplier = 0.2f, bonusChance = 0.15f }
                }
            });
            
            // 古代遗物
            resourceConversions.Add(new ResourceConversion
            {
                resourceType = "Ancient",
                outputs = new List<ConversionOutput>
                {
                    new ConversionOutput { materialId = "ancient_core", amountMultiplier = 0.3f },
                    new ConversionOutput { materialId = "void_shard", amountMultiplier = 0.5f }
                }
            });
        }
        
        private void InitializeEnemyDrops()
        {
            // 机械鲨鱼掉落
            enemyDropTables.Add(new EnemyDropTable
            {
                enemyType = "MechShark",
                possibleDrops = new List<DropEntry>
                {
                    new DropEntry { materialId = "scrap_metal", dropChance = 0.8f, minAmount = 2, maxAmount = 5 },
                    new DropEntry { materialId = "pressure_alloy", dropChance = 0.3f, minAmount = 1, maxAmount = 2 },
                    new DropEntry { materialId = "energy_crystal", dropChance = 0.15f, minAmount = 1, maxAmount = 1 }
                }
            });
            
            // 深海章鱼掉落
            enemyDropTables.Add(new EnemyDropTable
            {
                enemyType = "DeepOctopus",
                possibleDrops = new List<DropEntry>
                {
                    new DropEntry { materialId = "bioluminescent_gland", dropChance = 0.7f, minAmount = 1, maxAmount = 3 },
                    new DropEntry { materialId = "void_shard", dropChance = 0.2f, minAmount = 1, maxAmount = 1 },
                    new DropEntry { materialId = "abyssal_essence", dropChance = 0.1f, minAmount = 1, maxAmount = 1 }
                }
            });
        }
        
        private void InitializeRecipes()
        {
            // 耐压合金合成
            recipes.Add(new CraftingRecipe
            {
                recipeId = "craft_pressure_alloy",
                recipeName = "合成耐压合金",
                description = "将废金属精炼为耐压合金",
                inputs = new List<MaterialRequirement>
                {
                    new MaterialRequirement("scrap_metal", 5),
                    new MaterialRequirement("energy_crystal", 1)
                },
                outputs = new List<MaterialRequirement>
                {
                    new MaterialRequirement("pressure_alloy", 1)
                }
            });
            
            // 能量核心合成
            recipes.Add(new CraftingRecipe
            {
                recipeId = "craft_energy_core",
                recipeName = "合成能量核心",
                description = "将能量水晶压缩成核心",
                inputs = new List<MaterialRequirement>
                {
                    new MaterialRequirement("energy_crystal", 3),
                    new MaterialRequirement("scrap_circuit", 2)
                },
                outputs = new List<MaterialRequirement>
                {
                    new MaterialRequirement("ancient_core", 1)
                }
            });
            
            // 高级电路合成
            recipes.Add(new CraftingRecipe
            {
                recipeId = "craft_advanced_circuit",
                recipeName = "合成高级电路",
                description = "修复并升级废弃电路",
                inputs = new List<MaterialRequirement>
                {
                    new MaterialRequirement("scrap_circuit", 3),
                    new MaterialRequirement("bioluminescent_gland", 1)
                },
                outputs = new List<MaterialRequirement>
                {
                    new MaterialRequirement("neural_processor", 1)
                },
                requiredLevel = 5
            });
        }
    }
}