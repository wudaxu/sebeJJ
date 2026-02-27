# SebeJJ - 第三周开发计划

## 目标
实现地图生成系统、资源采集和探索玩法

---

## 里程碑
- [ ] 程序化地图生成
- [ ] 房间系统实现
- [ ] 资源采集系统
- [ ] 背包/库存系统
- [ ] 探索循环可玩

---

## 每日任务分解

### Day 15: 地图生成框架
**目标:** 建立程序化地图生成系统

#### 任务清单
- [ ] 实现 `MapGenerator` 核心类
- [ ] 实现房间布局算法 (随机散布+连通)
- [ ] 实现走廊连接系统
- [ ] 创建地图数据结构
- [ ] 实现地图可视化调试

#### 地图生成算法
```csharp
public class MapGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    public int mapWidth = 100;
    public int mapHeight = 100;
    public int roomCount = 15;
    public int minRoomSize = 5;
    public int maxRoomSize = 15;
    
    private MapData currentMap;
    
    public void GenerateLevel(int depth)
    {
        // 1. 初始化空地图
        currentMap = new MapData(mapWidth, mapHeight);
        
        // 2. 随机生成房间
        GenerateRooms();
        
        // 3. 使用最小生成树连接房间
        ConnectRooms();
        
        // 4. 添加额外连接
        AddExtraConnections();
        
        // 5. 放置内容
        PopulateRooms(depth);
    }
}
```

#### 验收标准
- [ ] 地图生成无错误
- [ ] 房间布局合理
- [ ] 所有房间可达
- [ ] 生成时间 < 2秒

---

### Day 16: 房间系统
**目标:** 实现房间类型和内容系统

#### 任务清单
- [ ] 定义 `RoomType` 枚举
- [ ] 实现 `Room` 类
- [ ] 实现房间内容生成
- [ ] 实现房间触发器 (进入/离开)
- [ ] 实现特殊房间 (商店、安全区)

#### 房间类型
```csharp
public enum RoomType
{
    Empty,          // 空房间
    ResourceRich,   // 资源丰富 - 大量矿物
    EnemyNest,      // 敌巢 - 更多敌人
    HazardZone,     // 危险区域 - 环境危害
    Secret,         // 隐藏房间 - 稀有奖励
    Boss,           // Boss房间
    SafeZone,       // 安全区 - 可存档/恢复
    Shop            // 商店
}

public class Room : MonoBehaviour
{
    public RoomType type;
    public Bounds bounds;
    public List<Room> connectedRooms;
    public bool isExplored;
    public bool isCleared;
    
    public void OnPlayerEnter()
    {
        isExplored = true;
        GameEvents.OnRoomEntered?.Invoke(this);
        
        // 根据房间类型触发事件
        switch(type)
        {
            case RoomType.EnemyNest:
                SpawnEnemies();
                break;
            case RoomType.Shop:
                OpenShop();
                break;
        }
    }
}
```

#### 验收标准
- [ ] 房间类型差异化明显
- [ ] 进入/离开事件触发正常
- [ ] 房间内容生成正确
- [ ] 已探索状态保存

---

### Day 17: 资源系统
**目标:** 实现资源类型和采集机制

#### 任务清单
- [ ] 定义 `ResourceType` 枚举
- [ ] 创建 `ResourceData` ScriptableObject
- [ ] 实现 `ResourceNode` 资源节点
- [ ] 实现采集交互
- [ ] 实现资源稀有度系统

#### 资源类型
```csharp
public enum ResourceType
{
    // 普通 (白色)
    ScrapMetal,     // 废金属 - 基础材料
    CopperOre,      // 铜矿 - 导电材料
    IronOre,        // 铁矿 - 结构材料
    
    // 稀有 (绿色)
    GoldOre,        // 金矿 - 高价值
    CrystalShard,   // 水晶碎片 - 能量材料
    
    // 史诗 (蓝色)
    Uranium,        // 铀 - 高级能源
    BioSample,      // 生物样本 - 研究材料
    
    // 传说 (紫色)
    DataFragment,   // 数据碎片 - 剧情相关
    AncientTech     // 古代科技 - 最强升级
}

[CreateAssetMenu(fileName = "Resource", menuName = "SebeJJ/Resource")]
public class ResourceData : ScriptableObject
{
    public ResourceType type;
    public string displayName;
    public Sprite icon;
    public int baseValue;
    public float weight;
    public float miningDifficulty;
    public Rarity rarity;
}
```

#### 验收标准
- [ ] 资源类型完整
- [ ] 稀有度系统生效
- [ ] 采集交互流畅
- [ ] 资源数据可配置

---

### Day 18: 采集与采矿
**目标:** 实现采矿玩法和工具交互

#### 任务清单
- [ ] 实现采矿激光效果
- [ ] 实现资源节点生命值
- [ ] 实现采集进度条
- [ ] 实现资源掉落
- [ ] 实现采集工具升级影响

#### 采矿系统
```csharp
public class ResourceNode : MonoBehaviour, IDamageable
{
    public ResourceData resourceData;
    public float maxHealth = 100f;
    public int minDropAmount = 1;
    public int maxDropAmount = 3;
    
    private float currentHealth;
    private bool isBeingMined;
    
    public void TakeDamage(DamageInfo damageInfo)
    {
        // 只有采矿武器能造成伤害
        if (damageInfo.source.GetComponent<MechController>() == null)
            return;
            
        currentHealth -= damageInfo.amount;
        
        if (currentHealth <= 0)
        {
            DropResources();
            Destroy(gameObject);
        }
    }
    
    private void DropResources()
    {
        int amount = Random.Range(minDropAmount, maxDropAmount + 1);
        for (int i = 0; i < amount; i++)
        {
            // 生成资源掉落物
            var pickup = Instantiate(resourcePickupPrefab, transform.position, Quaternion.identity);
            pickup.SetResource(resourceData);
        }
    }
}
```

#### 验收标准
- [ ] 采矿有进度感
- [ ] 资源掉落合理
- [ ] 不同工具效率不同
- [ ] 视觉效果清晰

---

### Day 19: 背包系统
**目标:** 实现库存管理和UI

#### 任务清单
- [ ] 实现 `Inventory` 类
- [ ] 实现物品堆叠逻辑
- [ ] 实现背包容量限制
- [ ] 实现 `InventoryUI`
- [ ] 实现拖拽/整理功能

#### 背包系统
```csharp
[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int quantity;
    public bool IsEmpty => item == null || quantity <= 0;
    
    public bool CanAdd(ItemData newItem, int amount)
    {
        if (IsEmpty) return true;
        if (item != newItem) return false;
        return quantity + amount <= item.maxStack;
    }
}

public class Inventory : MonoBehaviour
{
    [SerializeField] private int capacity = 30;
    [SerializeField] private float maxWeight = 100f;
    
    private InventorySlot[] slots;
    private float currentWeight;
    
    public bool AddItem(ItemData item, int amount = 1);
    public bool RemoveItem(ItemData item, int amount = 1);
    public bool HasItem(ItemData item, int amount = 1);
    public int GetItemCount(ItemData item);
}
```

#### 验收标准
- [ ] 物品添加/移除正常
- [ ] 堆叠逻辑正确
- [ ] 容量限制生效
- [ ] UI响应流畅

---

### Day 20: 迷雾与探索
**目标:** 实现战争迷雾和探索记录

#### 任务清单
- [ ] 实现 `FogOfWar` 系统
- [ ] 实现视野范围计算
- [ ] 实现小地图系统
- [ ] 实现地图标记
- [ ] 实现已探索区域记录

#### 迷雾系统
```csharp
public class FogOfWar : MonoBehaviour
{
    [Header("Settings")]
    public int mapWidth = 100;
    public int mapHeight = 100;
    public float revealRadius = 8f;
    public float updateInterval = 0.1f;
    
    private Texture2D fogTexture;
    private Color[] fogPixels;
    private Transform player;
    
    private void Update()
    {
        // 根据玩家位置更新迷雾
        Vector2Int playerGrid = WorldToGrid(player.position);
        RevealArea(playerGrid, revealRadius);
    }
    
    private void RevealArea(Vector2Int center, float radius)
    {
        // 圆形区域揭示
        int r = Mathf.CeilToInt(radius);
        for (int y = -r; y <= r; y++)
        {
            for (int x = -r; x <= r; x++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    Vector2Int pos = center + new Vector2Int(x, y);
                    if (IsValid(pos))
                    {
                        fogPixels[pos.y * mapWidth + pos.x].a = 0;
                    }
                }
            }
        }
        fogTexture.SetPixels(fogPixels);
        fogTexture.Apply();
    }
}
```

#### 验收标准
- [ ] 迷雾效果正确
- [ ] 视野范围合理
- [ ] 小地图显示准确
- [ ] 探索进度保存

---

### Day 21: 整合与测试
**目标:** 整合探索系统，进行全面测试

#### 任务清单
- [ ] 整合地图生成与房间系统
- [ ] 整合资源与背包系统
- [ ] 实现返回基地机制
- [ ] 进行探索流程测试
- [ ] 优化生成性能
- [ ] 修复Bug

#### 返回基地
```csharp
public class EscapePod : MonoBehaviour
{
    public void OnInteract()
    {
        // 保存收集的资源
        SaveCollectedResources();
        
        // 返回基地场景
        GameManager.Instance.LoadScene("BaseScene");
        
        // 触发返回事件
        GameEvents.OnReturnToBase?.Invoke();
    }
}
```

#### 验收标准
- [ ] 完整探索循环可玩
- [ ] 地图生成稳定
- [ ] 资源采集流畅
- [ ] 背包管理方便
- [ ] 返回基地功能正常

---

## 技术依赖

| 依赖项 | 用途 | 状态 |
|--------|------|------|
| 第二周代码 | 战斗系统 | 依赖 |
| 2D Tilemap | 地图绘制 | 推荐 |
| RenderTexture | 小地图 | 推荐 |

---

## 风险与应对

| 风险 | 可能性 | 应对措施 |
|------|--------|----------|
| 地图生成算法复杂 | 中 | 使用成熟算法，简化需求 |
| 迷雾性能问题 | 中 | 降低更新频率，优化Shader |
| 背包UI复杂 | 低 | 参考成熟游戏设计 |

---

## 成功标准

- [ ] 地图生成多样化
- [ ] 房间内容丰富
- [ ] 采集玩法有趣
- [ ] 背包易用
- [ ] 探索循环完整

---

**计划版本:** 1.0  
**创建日期:** 2026-02-26  
**负责人:** 架构师 Agent
