using UnityEngine;

namespace SebeJJ.Shop
{
    /// <summary>
    /// 商店物品数据库 - 预配置所有商品
    /// </summary>
    [CreateAssetMenu(fileName = "ShopDatabase", menuName = "SebeJJ/Shop/Database")]
    public class ShopDatabase : ScriptableObject
    {
        [Header("武器装备")]
        public ShopItemData laserCannon;
        public ShopItemData missileLauncher;
        public ShopItemData plasmaRifle;
        public ShopItemData railgun;
        public ShopItemData flamethrower;
        public ShopItemData empBlaster;
        
        [Header("机甲部件")]
        public ShopItemData engineMk1;
        public ShopItemData engineMk2;
        public ShopItemData engineMk3;
        public ShopItemData armorMk1;
        public ShopItemData armorMk2;
        public ShopItemData armorMk3;
        public ShopItemData drillMk1;
        public ShopItemData drillMk2;
        public ShopItemData drillMk3;
        
        [Header("消耗品")]
        public ShopItemData oxygenTankSmall;
        public ShopItemData oxygenTankLarge;
        public ShopItemData energyBatterySmall;
        public ShopItemData energyBatteryLarge;
        public ShopItemData repairKitBasic;
        public ShopItemData repairKitAdvanced;
        
        [Header("模块升级")]
        public ShopItemData efficiencyModule;
        public ShopItemData reinforcementModule;
        public ShopItemData overclockModule;
        
        /// <summary>
        /// 获取所有商品
        /// </summary>
        public ShopItemData[] GetAllItems()
        {
            return new[]
            {
                // 武器
                laserCannon, missileLauncher, plasmaRifle, railgun, flamethrower, empBlaster,
                // 部件
                engineMk1, engineMk2, engineMk3, armorMk1, armorMk2, armorMk3, drillMk1, drillMk2, drillMk3,
                // 消耗品
                oxygenTankSmall, oxygenTankLarge, energyBatterySmall, energyBatteryLarge, repairKitBasic, repairKitAdvanced,
                // 模块
                efficiencyModule, reinforcementModule, overclockModule
            };
        }
        
        /// <summary>
        /// 获取默认商品配置（用于初始化ScriptableObject）
        /// </summary>
        public static void CreateDefaultItemConfigs()
        {
            // 武器装备配置
            Debug.Log("创建武器配置...");
            Debug.Log("1. 激光炮 - 高射速能量武器");
            Debug.Log("2. 导弹发射器 - 范围伤害");
            Debug.Log("3. 等离子步枪 - 持续伤害");
            Debug.Log("4. 轨道炮 - 高伤害单发");
            Debug.Log("5. 火焰喷射器 - 近战范围");
            Debug.Log("6. EMP冲击波 - 电子设备瘫痪");
            
            // 机甲部件配置
            Debug.Log("创建机甲部件配置...");
            Debug.Log("引擎: Mk1/Mk2/Mk3 - 提升移动速度");
            Debug.Log("装甲: Mk1/Mk2/Mk3 - 提升防御力");
            Debug.Log("钻头: Mk1/Mk2/Mk3 - 提升采集效率");
            
            // 消耗品配置
            Debug.Log("创建消耗品配置...");
            Debug.Log("氧气罐: 小型/大型 - 恢复氧气");
            Debug.Log("能量电池: 小型/大型 - 恢复能量");
            Debug.Log("修理包: 基础/高级 - 修复机甲");
            
            // 模块升级配置
            Debug.Log("创建模块升级配置...");
            Debug.Log("效率模块 - 降低能耗");
            Debug.Log("强化模块 - 提升属性");
            Debug.Log("超频模块 - 极限性能");
        }
    }
}