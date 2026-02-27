# 赛博机甲 SebeJJ 商店系统设计文档

## 1. 系统概述

### 1.1 设计目标
- 提供直观、流畅的购物体验
- 支持多种商品类型和分类管理
- 实现完整的货币系统和交易流程
- 提供商品解锁和进度机制

### 1.2 核心功能
- 商品浏览、筛选、排序
- 购物车系统
- 货币管理（信用点）
- 商品解锁机制
- 库存管理

---

## 2. 架构设计

### 2.1 核心类图

```
ShopManager (单例)
├── ShopStockItem[] 库存数据
├── CartItem[] 购物车
├── 商品查询方法
├── 购买逻辑
└── 解锁机制

CurrencySystem (单例)
├── Credits 信用点
├── PremiumCredits 高级信用点
├── Scrap 废料
├── Reputation 声望
└── 货币操作方法

ShopItemData (ScriptableObject)
├── 基础信息 (ID/名称/描述/图标)
├── 分类 (Type/SubType/Rarity)
├── 价格与库存
├── 解锁条件
└── 属性加成 (ItemStats)
```

### 2.2 UI层架构

```
ShopUI (主界面)
├── ShopItemUI[] 商品列表项
├── ItemDetailPanel 详情面板
├── CartUI 购物车界面
└── 筛选/排序控件

CartUI
├── CartItemUI[] 购物车项
└── 确认弹窗
```

---

## 3. 商品类型系统

### 3.1 商品类型 (ItemType)
| 类型 | 说明 |
|------|------|
| Weapon | 武器装备 |
| MechaPart | 机甲部件 |
| Consumable | 消耗品 |
| ModuleUpgrade | 模块升级 |

### 3.2 商品子类型 (ItemSubType)

#### 武器装备 (6种)
| 子类型 | 英文名 | 特点 |
|--------|--------|------|
| 激光炮 | LaserCannon | 高射速能量武器 |
| 导弹发射器 | MissileLauncher | 范围伤害 |
| 等离子步枪 | PlasmaRifle | 持续伤害 |
| 轨道炮 | Railgun | 高伤害单发 |
| 火焰喷射器 | Flamethrower | 近战范围 |
| EMP冲击波 | EMPBlaster | 电子设备瘫痪 |

#### 机甲部件 (3类 × 3等级)
| 子类型 | Mk1 | Mk2 | Mk3 |
|--------|-----|-----|-----|
| 引擎 | 基础速度 | 中等速度 | 极速 |
| 装甲 | 基础防御 | 中等防御 | 重甲 |
| 钻头 | 基础采集 | 中等采集 | 高效采集 |

#### 消耗品 (3种 × 2规格)
| 子类型 | 小型 | 大型 |
|--------|------|------|
| 氧气罐 | 恢复30% | 恢复100% |
| 能量电池 | 恢复30% | 恢复100% |
| 修理包 | 修复30% | 修复100% |

#### 模块升级 (3种)
| 子类型 | 效果 |
|--------|------|
| 效率模块 | 降低20%能耗 |
| 强化模块 | 提升15%全属性 |
| 超频模块 | 提升30%性能，增加10%故障率 |

### 3.3 稀有度系统 (ItemRarity)
| 稀有度 | 颜色 | 说明 |
|--------|------|------|
| Common | 灰色 | 普通商品 |
| Uncommon | 绿色 | 稀有商品 |
| Rare | 蓝色 | 史诗商品 |
| Legendary | 金色 | 传说商品 |
| Mythic | 粉色 | 神话商品 |

---

## 4. 货币系统

### 4.1 货币类型
| 货币 | 用途 | 获取方式 |
|------|------|----------|
| Credits (CR) | 主要货币，购买大部分商品 | 任务奖励、出售物品 |
| PremiumCredits (PCR) | 高级货币，购买特殊商品 | 充值、特殊活动 |
| Scrap | 次要货币，购买基础材料 | 分解装备 |
| Reputation | 声望，解锁特定商品 | 完成任务、提升关系 |

### 4.2 初始值
- Credits: 1000
- PremiumCredits: 0
- Scrap: 0
- Reputation: 0

---

## 5. 商店界面功能

### 5.1 商品列表
- **分类筛选**: 全部/武器/部件/消耗品/升级
- **搜索功能**: 按名称/描述搜索
- **排序方式**:
  - 默认排序
  - 价格: 低到高 / 高到低
  - 稀有度: 低到高 / 高到低
  - 名称: A-Z / Z-A

### 5.2 商品详情面板
- 商品图标和名称
- 稀有度显示
- 详细描述
- 属性加成列表
- 价格显示
- 库存显示
- 数量选择器
- 购买/加入购物车按钮
- 解锁条件显示（锁定状态）

### 5.3 购物车系统
- 显示已选商品列表
- 调整数量
- 移除商品
- 显示总价
- 清空购物车
- 确认购买弹窗

---

## 6. 购买逻辑

### 6.1 购买流程
1. 添加商品到购物车
2. 检查库存是否充足
3. 检查货币是否足够
4. 扣除货币
5. 扣除库存
6. 发放商品到背包
7. 触发购买效果（音效/特效）
8. 发送通知

### 6.2 购买结果
```csharp
public class PurchaseResult
{
    public bool Success;           // 是否成功
    public string Message;         // 结果消息
    public List<ShopItemData> PurchasedItems;  // 购买的商品
    public int TotalCost;          // 总花费
    public CurrencyType CurrencyUsed;  // 使用的货币
}
```

### 6.3 失败原因
- 购物车为空
- 信用点不足
- 库存不足
- 商品未解锁
- 商品不存在

---

## 7. 商品解锁机制

### 7.1 解锁条件
| 条件类型 | 说明 |
|----------|------|
| 等级要求 | 玩家达到指定等级 |
| 成就要求 | 完成特定成就 |
| 前置商品 | 拥有指定商品 |
| 默认锁定 | 初始锁定，满足条件后解锁 |

### 7.2 解锁检查
```csharp
bool CheckUnlockConditions(
    ShopItemData item,
    int playerLevel,
    List<string> ownedAchievements,
    List<string> ownedItemIds
)
```

---

## 8. 库存系统

### 8.1 库存类型
- **普通库存**: 有最大库存上限，可自动补货
- **限量库存**: 特殊商品，数量有限，售完即止

### 8.2 库存管理
- 购买时扣除库存
- 支持手动补货
- 支持完全补货（重置所有库存）
- 库存变更事件通知

---

## 9. 事件系统

### 9.1 ShopManager 事件
| 事件 | 触发时机 |
|------|----------|
| OnItemUnlocked | 商品解锁时 |
| OnCartUpdated | 购物车更新时 |
| OnCartCleared | 购物车清空时 |
| OnPurchaseCompleted | 购买完成时 |
| OnStockChanged | 库存变更时 |

### 9.2 CurrencySystem 事件
| 事件 | 触发时机 |
|------|----------|
| OnCurrencyChanged | 货币变更时 |

---

## 10. 音效系统

### 10.1 音效类型
- 打开商店
- 关闭商店
- 商品悬停
- 商品点击
- 添加到购物车
- 从购物车移除
- 购买成功
- 购买失败
- 商品解锁

---

## 11. 通知系统

### 11.1 通知类型
- 商品解锁通知
- 购买成功通知
- 购买失败通知
- 购物车更新通知
- 新商品上架通知
- 促销开始/结束通知

---

## 12. 文件结构

```
Assets/Scripts/Shop/
├── ShopManager.cs              # 商店管理器（单例）
├── CurrencySystem.cs           # 货币系统（单例）
├── ShopItemData.cs             # 商品数据配置（ScriptableObject）
├── ShopDatabase.cs             # 商品数据库
├── ShopAudioManager.cs         # 商店音效管理
├── ShopNotificationSystem.cs   # 商店通知系统
├── ShopUI.cs                   # 商店主界面
├── ShopItemUI.cs               # 商品列表项UI
├── ItemDetailPanel.cs          # 商品详情面板
├── CartUI.cs                   # 购物车UI
└── CartItemUI.cs               # 购物车项UI
```

---

## 13. 使用示例

### 13.1 打开商店
```csharp
ShopUI.Instance.OpenShop();
```

### 13.2 添加商品到购物车
```csharp
ShopManager.Instance.AddToCart(itemData, quantity);
```

### 13.3 购买购物车
```csharp
var result = ShopManager.Instance.PurchaseCart();
if (result.Success) {
    Debug.Log("购买成功！");
}
```

### 13.4 检查并解锁商品
```csharp
ShopManager.Instance.RefreshUnlockStatus(
    playerLevel,
    ownedAchievements,
    ownedItemIds
);
```

### 13.5 添加货币
```csharp
CurrencySystem.Instance.AddCurrency(CurrencyType.Credits, 500, "任务奖励");
```

---

## 14. 配置说明

### 14.1 创建商品数据
1. 在Project窗口右键
2. Create > SebeJJ > Shop > Shop Item
3. 配置商品属性
4. 将商品添加到ShopManager的allShopItems列表

### 14.2 配置商品属性
- **基础信息**: ID、名称、描述、图标
- **分类**: 选择Type和SubType
- **价格**: 设置basePrice
- **库存**: 设置maxStock或limitedStockCount
- **解锁**: 设置isLockedByDefault和解锁条件
- **属性**: 配置ItemStats中的各项加成

---

## 15. 扩展建议

### 15.1 可能的扩展功能
- 限时折扣系统
- 每日特惠商品
- 商品推荐算法
- 玩家交易行
- 拍卖系统
- 商品预览（3D模型）
- 购买历史记录

### 15.2 性能优化
- 对象池优化UI项
- 分页加载商品列表
- 异步加载商品图标
- 缓存商品数据

---

**文档版本**: 1.0  
**创建日期**: 2026-02-27  
**作者**: 商店系统工程师