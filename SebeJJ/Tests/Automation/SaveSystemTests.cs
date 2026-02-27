using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace SebeJJ.Tests.Core
{
    /// <summary>
    /// SaveSystem 单元测试
    /// </summary>
    public class SaveSystemTests
    {
        private string _testSavePath;
        private SaveSystem _saveSystem;

        [SetUp]
        public void Setup()
        {
            _testSavePath = Path.Combine(Application.persistentDataPath, "TestSaves");
            Directory.CreateDirectory(_testSavePath);
            
            var go = new GameObject("SaveSystem");
            _saveSystem = go.AddComponent<SaveSystem>();
            _saveSystem.SetSavePath(_testSavePath);
        }

        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(_testSavePath))
            {
                Directory.Delete(_testSavePath, true);
            }
            
            if (_saveSystem != null)
            {
                Object.DestroyImmediate(_saveSystem.gameObject);
            }
        }

        [Test]
        public void SaveSystem_SaveGame_CreatesFile()
        {
            // Arrange
            var playerData = CreateTestPlayerData();

            // Act
            bool result = _saveSystem.SaveGame(playerData, "test_save");

            // Assert
            Assert.IsTrue(result);
            string filePath = Path.Combine(_testSavePath, "test_save.json");
            Assert.IsTrue(File.Exists(filePath));
        }

        [Test]
        public void SaveSystem_LoadGame_ReturnsCorrectData()
        {
            // Arrange
            var originalData = CreateTestPlayerData();
            originalData.playerName = "TestPlayer123";
            originalData.currentDepth = 150.5f;
            _saveSystem.SaveGame(originalData, "test_save");

            // Act
            var loadedData = _saveSystem.LoadGame("test_save");

            // Assert
            Assert.IsNotNull(loadedData);
            Assert.AreEqual(originalData.playerName, loadedData.playerName);
            Assert.AreEqual(originalData.currentDepth, loadedData.currentDepth);
        }

        [Test]
        public void SaveSystem_LoadNonExistent_ReturnsNull()
        {
            // Act
            var loadedData = _saveSystem.LoadGame("non_existent_save");

            // Assert
            Assert.IsNull(loadedData);
        }

        [Test]
        public void SaveSystem_DeleteSave_RemovesFile()
        {
            // Arrange
            var playerData = CreateTestPlayerData();
            _saveSystem.SaveGame(playerData, "test_save");
            string filePath = Path.Combine(_testSavePath, "test_save.json");
            Assert.IsTrue(File.Exists(filePath));

            // Act
            bool result = _saveSystem.DeleteSave("test_save");

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(filePath));
        }

        [Test]
        public void SaveSystem_OverwriteSave_UpdatesData()
        {
            // Arrange
            var originalData = CreateTestPlayerData();
            originalData.playerName = "Original";
            _saveSystem.SaveGame(originalData, "test_save");

            // Act
            var newData = CreateTestPlayerData();
            newData.playerName = "Updated";
            _saveSystem.SaveGame(newData, "test_save");

            // Assert
            var loadedData = _saveSystem.LoadGame("test_save");
            Assert.AreEqual("Updated", loadedData.playerName);
        }

        [Test]
        public void SaveSystem_SaveWithInventory_PreservesItems()
        {
            // Arrange
            var playerData = CreateTestPlayerData();
            playerData.inventory = new System.Collections.Generic.List<InventoryItem>
            {
                new InventoryItem { type = ResourceType.CopperOre, count = 5 },
                new InventoryItem { type = ResourceType.ScrapMetal, count = 10 }
            };

            // Act
            _saveSystem.SaveGame(playerData, "inventory_test");
            var loadedData = _saveSystem.LoadGame("inventory_test");

            // Assert
            Assert.IsNotNull(loadedData.inventory);
            Assert.AreEqual(2, loadedData.inventory.Count);
            Assert.AreEqual(ResourceType.CopperOre, loadedData.inventory[0].type);
            Assert.AreEqual(5, loadedData.inventory[0].count);
        }

        [Test]
        public void SaveSystem_ListSaves_ReturnsAllSaves()
        {
            // Arrange
            var playerData = CreateTestPlayerData();
            _saveSystem.SaveGame(playerData, "save1");
            _saveSystem.SaveGame(playerData, "save2");
            _saveSystem.SaveGame(playerData, "save3");

            // Act
            var saves = _saveSystem.ListAllSaves();

            // Assert
            Assert.AreEqual(3, saves.Count);
            Assert.Contains("save1", saves);
            Assert.Contains("save2", saves);
            Assert.Contains("save3", saves);
        }

        private PlayerData CreateTestPlayerData()
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
                inventory = new System.Collections.Generic.List<InventoryItem>(),
                equippedUpgrades = new System.Collections.Generic.List<string>(),
                discoveredAreas = new System.Collections.Generic.List<string>(),
                completedTutorials = new System.Collections.Generic.List<string>()
            };
        }
    }

    /// <summary>
    /// 玩家数据类
    /// </summary>
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
        public System.Collections.Generic.List<InventoryItem> inventory;
        public System.Collections.Generic.List<string> equippedUpgrades;
        public System.Collections.Generic.List<string> discoveredAreas;
        public System.Collections.Generic.List<string> completedTutorials;
    }

    /// <summary>
    /// 背包物品
    /// </summary>
    [System.Serializable]
    public class InventoryItem
    {
        public ResourceType type;
        public int count;
    }

    /// <summary>
    /// 存档系统
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        private string _savePath;

        public void SetSavePath(string path)
        {
            _savePath = path;
        }

        private string GetSaveFilePath(string saveName)
        {
            return Path.Combine(_savePath ?? Application.persistentDataPath, $"{saveName}.json");
        }

        public bool SaveGame(PlayerData data, string saveName)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                string filePath = GetSaveFilePath(saveName);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public PlayerData LoadGame(string saveName)
        {
            string filePath = GetSaveFilePath(saveName);
            
            if (!File.Exists(filePath))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonUtility.FromJson<PlayerData>(json);
            }
            catch
            {
                return null;
            }
        }

        public bool DeleteSave(string saveName)
        {
            string filePath = GetSaveFilePath(saveName);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            
            return false;
        }

        public System.Collections.Generic.List<string> ListAllSaves()
        {
            var saves = new System.Collections.Generic.List<string>();
            string path = _savePath ?? Application.persistentDataPath;
            
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, "*.json");
                foreach (var file in files)
                {
                    saves.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
            
            return saves;
        }
    }
}
