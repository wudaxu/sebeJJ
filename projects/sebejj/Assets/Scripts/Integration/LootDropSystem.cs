/**
 * @file LootDropSystem.cs
 * @brief 击杀掉落系统
 * @description 处理敌人死亡后的资源掉落
 * @author 系统集成工程师
 * @date 2026-02-27
 */

using UnityEngine;
using SebeJJ.Enemies;
using SebeJJ.Player;
using System.Collections.Generic;

namespace SebeJJ.Integration
{
    /// <summary>
    /// 掉落物品数据
    /// </summary>
    [System.Serializable]
    public class LootItem
    {
        public string itemId;
        public string itemName;
        public ResourceType resourceType;
        public int minQuantity = 1;
        public int maxQuantity = 1;
        public float dropChance = 1f; // 0-1
        public float weight = 0.5f;
        public int value = 10;
        public GameObject pickupPrefab;
    }

    /// <summary>
    /// 敌人类型掉落表
    /// </summary>
    [System.Serializable]
    public class EnemyLootTable
    {
        public EnemyType enemyType;
        public List<LootItem> guaranteedDrops = new List<LootItem>(); // 必掉物品
        public List<LootItem> randomDrops = new List<LootItem>();    // 随机掉落
        public int maxRandomDrops = 2; // 最大随机掉落数量
        public float creditsDrop = 10f; // 基础信用点掉落
    }

    /// <summary>
    /// 掉落物拾取组件
    /// </summary>
    public class LootPickup : MonoBehaviour
    {
        [Header("拾取设置")]
        [SerializeField] private float pickupRadius = 1.5f;
        [SerializeField] private float magnetRadius = 5f;
        [SerializeField] private float magnetSpeed = 5f;
        [SerializeField] private float lifetime = 30f;
        [SerializeField] private float fadeStartTime = 25f;

        [Header("视觉效果")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private ParticleSystem pickupEffect;
        [SerializeField] private float bobAmplitude = 0.2f;
        [SerializeField] private float bobFrequency = 2f;

        private Data.InventoryItem itemData;
        private int creditsValue;
        private bool isCredits;
        private Transform target;
        private float spawnTime;
        private Vector3 startPos;

        public void Initialize(Data.InventoryItem item)
        {
            itemData = item;
            isCredits = false;
            spawnTime = Time.time;
            startPos = transform.position;
        }

        public void InitializeCredits(int credits)
        {
            creditsValue = credits;
            isCredits = true;
            spawnTime = Time.time;
            startPos = transform.position;
        }

        private void Update()
        {
            float elapsed = Time.time - spawnTime;

            // 生命周期结束
            if (elapsed >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            // 淡出效果
            if (elapsed > fadeStartTime && spriteRenderer != null)
            {
                float fade = 1f - (elapsed - fadeStartTime) / (lifetime - fadeStartTime);
                Color color = spriteRenderer.color;
                color.a = fade;
                spriteRenderer.color = color;
            }

            // 悬浮动画
            float bob = Mathf.Sin(elapsed * bobFrequency) * bobAmplitude;
            transform.position = new Vector3(transform.position.x, startPos.y + bob, transform.position.z);

            // 磁力吸引
            if (target != null)
            {
                Vector3 dir = (target.position - transform.position).normalized;
                transform.position += dir * magnetSpeed * Time.deltaTime;

                // 检查拾取
                if (Vector3.Distance(transform.position, target.position) < pickupRadius)
                {
                    Collect();
                }
            }
            else
            {
                // 寻找玩家
                FindPlayer();
            }
        }

        private void FindPlayer()
        {
            if (MechController.Instance != null)
            {
                float dist = Vector3.Distance(transform.position, MechController.Instance.transform.position);
                if (dist < magnetRadius)
                {
                    target = MechController.Instance.transform;
                }
            }
        }

        private void Collect()
        {
            if (isCredits)
            {
                // 添加信用点
                var resourceManager = Core.GameManager.Instance?.resourceManager;
                if (resourceManager != null)
                {
                    resourceManager.AddCredits(creditsValue);
                    Debug.Log($"[LootPickup] 拾取 {creditsValue} 信用点");
                }
            }
            else if (itemData != null)
            {
                // 添加物品到背包
                var resourceManager = Core.GameManager.Instance?.resourceManager;
                if (resourceManager != null)
                {
                    if (resourceManager.AddToInventory(itemData))
                    {
                        Debug.Log($"[LootPickup] 拾取 {itemData.itemName} x{itemData.quantity}");
                    }
                    else
                    {
                        Debug.Log("[LootPickup] 背包已满，无法拾取");
                        return; // 不销毁，让玩家有机会清理背包后再拾取
                    }
                }
            }

            // 播放特效
            if (pickupEffect != null)
            {
                pickupEffect.Play();
                pickupEffect.transform.SetParent(null);
                Destroy(pickupEffect.gameObject, 1f);
            }

            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<MechController>() != null)
            {
                Collect();
            }
        }
    }

    /// <summary>
    /// 掉落系统
    /// </summary>
    public class LootDropSystem : MonoBehaviour
    {
        public static LootDropSystem Instance { get; private set; }

        [Header("掉落表")]
        [SerializeField] private List<EnemyLootTable> lootTables = new List<EnemyLootTable>();

        [Header("掉落设置")]
        [SerializeField] private float dropSpreadRadius = 1f;
        [SerializeField] private float dropUpwardForce = 2f;
        [SerializeField] private int maxDropsPerEnemy = 5;

        [Header("默认掉落物")]
        [SerializeField] private GameObject defaultPickupPrefab;
        [SerializeField] private GameObject creditsPickupPrefab;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // 初始化默认掉落表
            InitializeDefaultLootTables();
        }

        /// <summary>
        /// 初始化默认掉落表
        /// </summary>
        private void InitializeDefaultLootTables()
        {
            if (lootTables.Count > 0) return;

            // 机械鱼掉落表
            var fishTable = new EnemyLootTable
            {
                enemyType = EnemyType.MechFish,
                creditsDrop = 15f,
                maxRandomDrops = 2
            };

            fishTable.guaranteedDrops.Add(new LootItem
            {
                itemId = "fish_scrap",
                itemName = "机械鱼残骸",
                resourceType = ResourceType.TechScrap,
                minQuantity = 1,
                maxQuantity = 2,
                dropChance = 1f,
                weight = 2f,
                value = 5
            });

            fishTable.randomDrops.Add(new LootItem
            {
                itemId = "metal_fragment",
                itemName = "金属碎片",
                resourceType = ResourceType.Mineral,
                minQuantity = 1,
                maxQuantity = 3,
                dropChance = 0.5f,
                weight = 0.5f,
                value = 3
            });

            fishTable.randomDrops.Add(new LootItem
            {
                itemId = "energy_cell",
                itemName = "能量电池",
                resourceType = ResourceType.TechScrap,
                minQuantity = 1,
                maxQuantity = 1,
                dropChance = 0.3f,
                weight = 0.3f,
                value = 15
            });

            lootTables.Add(fishTable);

            // 机械蟹掉落表
            var crabTable = new EnemyLootTable
            {
                enemyType = EnemyType.MechCrab,
                creditsDrop = 25f,
                maxRandomDrops = 3
            };

            crabTable.guaranteedDrops.Add(new LootItem
            {
                itemId = "crab_shell",
                itemName = "机械蟹壳",
                resourceType = ResourceType.TechScrap,
                minQuantity = 1,
                maxQuantity = 1,
                dropChance = 1f,
                weight = 3f,
                value = 10
            });

            crabTable.randomDrops.Add(new LootItem
            {
                itemId = "armor_plate",
                itemName = "装甲板",
                resourceType = ResourceType.TechScrap,
                minQuantity = 1,
                maxQuantity = 2,
                dropChance = 0.4f,
                weight = 1f,
                value = 8
            });

            lootTables.Add(crabTable);

            // 机械水母掉落表
            var jellyfishTable = new EnemyLootTable
            {
                enemyType = EnemyType.MechJellyfish,
                creditsDrop = 20f,
                maxRandomDrops = 2
            };

            jellyfishTable.guaranteedDrops.Add(new LootItem
            {
                itemId = "jellyfish_tentacle",
                itemName = "机械触手",
                resourceType = ResourceType.BioMaterial,
                minQuantity = 1,
                maxQuantity = 2,
                dropChance = 1f,
                weight = 1f,
                value = 8
            });

            jellyfishTable.randomDrops.Add(new LootItem
            {
                itemId = "bio_gel",
                itemName = "生物凝胶",
                resourceType = ResourceType.BioMaterial,
                minQuantity = 1,
                maxQuantity = 2,
                dropChance = 0.6f,
                weight = 0.5f,
                value = 12
            });

            lootTables.Add(jellyfishTable);
        }

        /// <summary>
        /// 生成掉落
        /// </summary>
        public void SpawnLoot(Vector3 position, EnemyType enemyType)
        {
            var table = lootTables.Find(t => t.enemyType == enemyType);
            if (table == null)
            {
                Debug.LogWarning($"[LootDropSystem] 未找到 {enemyType} 的掉落表");
                return;
            }

            int dropCount = 0;

            // 生成必掉物品
            foreach (var item in table.guaranteedDrops)
            {
                if (Random.value <= item.dropChance && dropCount < maxDropsPerEnemy)
                {
                    SpawnLootItem(position, item);
                    dropCount++;
                }
            }

            // 生成随机掉落
            int randomDropCount = 0;
            foreach (var item in table.randomDrops)
            {
                if (randomDropCount >= table.maxRandomDrops) break;
                if (dropCount >= maxDropsPerEnemy) break;

                if (Random.value <= item.dropChance)
                {
                    SpawnLootItem(position, item);
                    dropCount++;
                    randomDropCount++;
                }
            }

            // 生成信用点
            int credits = Mathf.RoundToInt(table.creditsDrop * Random.Range(0.8f, 1.2f));
            SpawnCredits(position, credits);

            Debug.Log($"[LootDropSystem] {enemyType} 掉落 {dropCount} 个物品和 {credits} 信用点");
        }

        /// <summary>
        /// 生成掉落物品
        /// </summary>
        private void SpawnLootItem(Vector3 position, LootItem lootItem)
        {
            // 计算随机位置
            Vector2 randomOffset = Random.insideUnitCircle * dropSpreadRadius;
            Vector3 spawnPos = position + new Vector3(randomOffset.x, randomOffset.y, 0);

            // 创建掉落物
            GameObject pickupObj;
            if (lootItem.pickupPrefab != null)
            {
                pickupObj = Instantiate(lootItem.pickupPrefab, spawnPos, Quaternion.identity);
            }
            else if (defaultPickupPrefab != null)
            {
                pickupObj = Instantiate(defaultPickupPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                pickupObj = CreateDefaultPickup(spawnPos);
            }

            // 初始化掉落物
            var pickup = pickupObj.GetComponent<LootPickup>();
            if (pickup == null)
            {
                pickup = pickupObj.AddComponent<LootPickup>();
            }

            int quantity = Random.Range(lootItem.minQuantity, lootItem.maxQuantity + 1);
            var item = new Data.InventoryItem
            {
                itemId = lootItem.itemId,
                itemName = lootItem.itemName,
                resourceType = lootItem.resourceType,
                quantity = quantity,
                weight = lootItem.weight,
                value = lootItem.value
            };

            pickup.Initialize(item);

            // 添加初始速度
            var rb = pickupObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                rb.linearVelocity = randomDir * Random.Range(1f, 3f) + Vector2.up * dropUpwardForce;
            }
        }

        /// <summary>
        /// 生成信用点掉落
        /// </summary>
        private void SpawnCredits(Vector3 position, int credits)
        {
            Vector2 randomOffset = Random.insideUnitCircle * dropSpreadRadius;
            Vector3 spawnPos = position + new Vector3(randomOffset.x, randomOffset.y, 0);

            GameObject pickupObj;
            if (creditsPickupPrefab != null)
            {
                pickupObj = Instantiate(creditsPickupPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                pickupObj = CreateDefaultPickup(spawnPos);
            }

            var pickup = pickupObj.GetComponent<LootPickup>();
            if (pickup == null)
            {
                pickup = pickupObj.AddComponent<LootPickup>();
            }

            pickup.InitializeCredits(credits);

            // 添加初始速度
            var rb = pickupObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                rb.linearVelocity = randomDir * Random.Range(1f, 3f) + Vector2.up * dropUpwardForce;
            }
        }

        /// <summary>
        /// 创建默认拾取物
        /// </summary>
        private GameObject CreateDefaultPickup(Vector3 position)
        {
            var go = new GameObject("LootPickup");
            go.transform.position = position;

            // 添加碰撞器
            var collider = go.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.3f;

            // 添加刚体
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0.5f;

            // 添加精灵渲染器
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateDefaultSprite();
            sr.color = Color.yellow;

            return go;
        }

        /// <summary>
        /// 创建默认精灵
        /// </summary>
        private Sprite CreateDefaultSprite()
        {
            // 创建一个简单的圆形纹理
            int size = 32;
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dx = x - size / 2f;
                    float dy = y - size / 2f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    
                    if (dist < size / 2f)
                    {
                        pixels[y * size + x] = Color.white;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }
    }
}
