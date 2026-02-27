using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Tests
{
    /// <summary>
    /// 测试辅助工具类
    /// </summary>
    public static class TestUtils
    {
        private static List<GameObject> _testObjects = new List<GameObject>();

        /// <summary>
        /// 创建测试用的 GameObject
        /// </summary>
        public static GameObject CreateTestObject(string name = "TestObject")
        {
            var go = new GameObject(name);
            _testObjects.Add(go);
            return go;
        }

        /// <summary>
        /// 创建带有组件的测试对象
        /// </summary>
        public static T CreateTestObjectWithComponent<T>(string name = "TestObject") where T : Component
        {
            var go = CreateTestObject(name);
            return go.AddComponent<T>();
        }

        /// <summary>
        /// 清理所有测试对象
        /// </summary>
        public static void CleanupTestObjects()
        {
            foreach (var obj in _testObjects)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }
            _testObjects.Clear();
        }

        /// <summary>
        /// 创建测试用的 ScriptableObject
        /// </summary>
        public static T CreateTestScriptableObject<T>() where T : ScriptableObject
        {
            return ScriptableObject.CreateInstance<T>();
        }

        /// <summary>
        /// 等待指定帧数
        /// </summary>
        public static IEnumerator<object> WaitFrames(int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return null;
            }
        }

        /// <summary>
        /// 等待指定时间（秒）
        /// </summary>
        public static IEnumerator<object> WaitSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }

        /// <summary>
        /// 比较两个浮点数是否近似相等
        /// </summary>
        public static bool Approximately(float a, float b, float tolerance = 0.001f)
        {
            return Mathf.Abs(a - b) < tolerance;
        }

        /// <summary>
        /// 创建临时测试目录
        /// </summary>
        public static string CreateTestDirectory(string directoryName)
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, directoryName);
            System.IO.Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// 删除测试目录
        /// </summary>
        public static void DeleteTestDirectory(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                System.IO.Directory.Delete(path, true);
            }
        }
    }

    /// <summary>
    /// 模拟数据工厂
    /// </summary>
    public static class MockFactory
    {
        /// <summary>
        /// 创建标准测试机甲属性
        /// </summary>
        public static MechStats CreateStandardMechStats()
        {
            var stats = ScriptableObject.CreateInstance<MechStats>();
            stats.maxHealth = 100f;
            stats.maxEnergy = 100f;
            stats.maxOxygen = 100f;
            stats.armor = 10f;
            stats.pressureResistance = 100f;
            stats.speed = 5f;
            stats.turnRate = 180f;
            stats.miningPower = 1f;
            stats.cargoCapacity = 50f;
            return stats;
        }

        /// <summary>
        /// 创建轻型机甲属性
        /// </summary>
        public static MechStats CreateLightMechStats()
        {
            var stats = ScriptableObject.CreateInstance<MechStats>();
            stats.maxHealth = 70f;
            stats.maxEnergy = 120f;
            stats.maxOxygen = 80f;
            stats.armor = 5f;
            stats.pressureResistance = 80f;
            stats.speed = 8f;
            stats.turnRate = 240f;
            stats.miningPower = 0.8f;
            stats.cargoCapacity = 30f;
            return stats;
        }

        /// <summary>
        /// 创建重型机甲属性
        /// </summary>
        public static MechStats CreateHeavyMechStats()
        {
            var stats = ScriptableObject.CreateInstance<MechStats>();
            stats.maxHealth = 150f;
            stats.maxEnergy = 80f;
            stats.maxOxygen = 120f;
            stats.armor = 20f;
            stats.pressureResistance = 150f;
            stats.speed = 3f;
            stats.turnRate = 120f;
            stats.miningPower = 1.5f;
            stats.cargoCapacity = 80f;
            return stats;
        }

        /// <summary>
        /// 创建新游戏玩家数据
        /// </summary>
        public static PlayerData CreateNewGamePlayerData()
        {
            return new PlayerData
            {
                playerName = "TestPlayer",
                saveVersion = 1,
                playTime = 0,
                currentDepth = 0f,
                position = new Vector2(0, 0),
                health = 100f,
                energy = 100f,
                oxygen = 100f,
                currency = 0,
                inventory = new List<InventoryItem>(),
                equippedUpgrades = new List<string>(),
                discoveredAreas = new List<string>(),
                completedTutorials = new List<string>()
            };
        }

        /// <summary>
        /// 创建中期游戏玩家数据
        /// </summary>
        public static PlayerData CreateMidGamePlayerData()
        {
            return new PlayerData
            {
                playerName = "SebeHunter",
                saveVersion = 1,
                playTime = 3600,
                currentDepth = 250.5f,
                position = new Vector2(45.5f, -250.5f),
                health = 85f,
                energy = 60f,
                oxygen = 45f,
                currency = 1250,
                inventory = new List<InventoryItem>
                {
                    new InventoryItem { type = ResourceType.CopperOre, count = 15 },
                    new InventoryItem { type = ResourceType.ScrapMetal, count = 8 },
                    new InventoryItem { type = ResourceType.CrystalShard, count = 3 },
                    new InventoryItem { type = ResourceType.BioSample, count = 2 }
                },
                equippedUpgrades = new List<string> { "SpeedBoost", "OxygenTankV1" },
                discoveredAreas = new List<string> { "Shallows", "CoralReef", "DeepTrench" },
                completedTutorials = new List<string> { "Movement", "Mining", "Combat" }
            };
        }
    }

    // 补充类型定义
    public class MechStats : ScriptableObject
    {
        public float maxHealth = 100f;
        public float maxEnergy = 100f;
        public float maxOxygen = 100f;
        public float armor = 10f;
        public float pressureResistance = 100f;
        public float speed = 5f;
        public float turnRate = 180f;
        public float miningPower = 1f;
        public float cargoCapacity = 50f;
    }

    [System.Serializable]
    public class PlayerData
    {
        public string playerName;
        public int saveVersion;
        public int playTime;
        public float currentDepth;
        public Vector2 position;
        public float health;
        public float energy;
        public float oxygen;
        public int currency;
        public List<InventoryItem> inventory;
        public List<string> equippedUpgrades;
        public List<string> discoveredAreas;
        public List<string> completedTutorials;
    }

    [System.Serializable]
    public class InventoryItem
    {
        public ResourceType type;
        public int count;
    }

    public enum ResourceType
    {
        ScrapMetal, CopperOre, IronOre, GoldOre,
        CrystalShard, Uranium, BioSample, DataFragment, AncientTech
    }
}
