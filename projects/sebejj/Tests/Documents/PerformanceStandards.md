# 性能测试标准 - 赛博机甲 SebeJJ

**版本**: v1.0  
**日期**: 2026-02-26  

---

## 1. 性能目标

### 1.1 帧率标准
| 场景 | 目标帧率 | 最低可接受帧率 | 测试条件 |
|------|----------|----------------|----------|
| 主菜单 | 60 FPS | 60 FPS | 静态界面 |
| 浅海区域 | 60 FPS | 45 FPS | 正常游戏，少量物体 |
| 深海区域 | 60 FPS | 30 FPS | 高密度资源区 |
| 危险区域 | 60 FPS | 30 FPS | 特效密集场景 |
| UI界面 | 60 FPS | 60 FPS | 所有UI面板 |

### 1.2 内存使用
| 指标 | 目标值 | 最大限制 | 说明 |
|------|--------|----------|------|
| 运行时内存 | < 1GB | 1.5GB | 游戏运行时的内存占用 |
| 纹理内存 | < 512MB | 800MB | 所有纹理资源 |
| 音频内存 | < 100MB | 200MB | 音频资源缓存 |
| 堆内存分配 | < 10MB/帧 | 20MB/帧 | 每帧GC分配 |

### 1.3 加载时间
| 操作 | 目标时间 | 最大时间 |
|------|----------|----------|
| 游戏启动 | < 5秒 | 10秒 |
| 存档加载 | < 3秒 | 5秒 |
| 场景切换 | < 2秒 | 4秒 |
| 打开UI面板 | < 100ms | 200ms |

---

## 2. 性能测试场景

### 2.1 基准测试场景
```
场景A: 浅海平静区
- 深度: 0-100m
- 物体数量: 20-50
- 特效: 无
- 测试时长: 60秒

场景B: 中密资源区
- 深度: 100-500m
- 物体数量: 50-100
- 特效: 中
- 测试时长: 60秒

场景C: 深海密集区
- 深度: 500-1000m
- 物体数量: 100-200
- 特效: 高
- 测试时长: 60秒

场景D: 危险区域
- 深度: 800m+
- 物体数量: 80-150
- 特效: 极高（热液、电流等）
- 测试时长: 60秒
```

### 2.2 压力测试场景
```
压力测试1: 最大物体密度
- 在场景中生成最大允许数量的可采集物
- 持续移动和扫描
- 测试时长: 300秒

压力测试2: 长时间运行
- 正常游戏流程
- 连续运行4小时
- 监控内存增长

压力测试3: 快速操作
- 快速打开/关闭所有UI面板
- 连续快速存档/读档
- 测试时长: 300秒
```

---

## 3. 性能测试方法

### 3.1 自动化性能测试脚本
```csharp
// PerformanceTestRunner.cs 框架示例
public class PerformanceTestRunner : MonoBehaviour
{
    [Header("Test Configuration")]
    public float testDuration = 60f;
    public List<TestScene> testScenes;
    
    [Header("Metrics")]
    public float targetFPS = 60f;
    public long maxMemoryMB = 1024;
    
    private PerformanceMetrics metrics;
    
    public void RunBenchmark()
    {
        // 1. 预热
        // 2. 收集数据
        // 3. 分析结果
        // 4. 生成报告
    }
}
```

### 3.2 关键性能指标(KPI)
| 指标 | 测量方法 | 通过标准 |
|------|----------|----------|
| 平均帧率 | Unity Profiler | ≥ 目标值 |
| 最低帧率 | Unity Profiler | ≥ 最低可接受值 |
| 帧时间稳定性 | 标准差 < 5ms | 无明显卡顿 |
| 内存峰值 | Profiler Memory | < 最大限制 |
| GC频率 | Profiler CPU | < 1次/10秒 |
| 加载时间 | 秒表测量 | < 目标时间 |

---

## 4. 性能优化检查清单

### 4.1 渲染优化
- [ ] 使用对象池管理动态物体
- [ ] 合理设置摄像机裁剪距离
- [ ] 使用图集减少Draw Call
- [ ] 避免运行时材质创建
- [ ] 粒子系统使用GPU Instancing

### 4.2 内存优化
- [ ] 资源使用Addressables管理
- [ ] 大纹理使用压缩格式
- [ ] 音频使用压缩格式
- [ ] 及时释放不用的资源
- [ ] 避免闭包和装箱操作

### 4.3 代码优化
- [ ] 使用StringBuilder替代字符串拼接
- [ ] 避免在Update中使用GetComponent
- [ ] 使用Job System处理大量计算
- [ ] 缓存频繁访问的组件引用
- [ ] 使用对象池替代Instantiate/Destroy

---

## 5. 性能测试报告模板

```markdown
# 性能测试报告 - [日期]

## 测试环境
- Unity版本: [版本号]
- 平台: [Windows/Mac/Linux]
- 硬件: [CPU/GPU/内存]

## 测试结果摘要
| 场景 | 平均FPS | 最低FPS | 内存峰值 | 状态 |
|------|---------|---------|----------|------|
| 场景A | 60 | 58 | 450MB | ✅通过 |
| 场景B | 55 | 42 | 680MB | ⚠️警告 |
| 场景C | 45 | 28 | 920MB | ❌失败 |

## 详细数据
[Profiler截图/数据导出]

## 问题分析
1. [问题描述] - [原因分析] - [建议方案]

## 优化建议
1. [建议1]
2. [建议2]
```

---

## 6. 性能监控工具

### 6.1 Unity内置工具
- **Profiler**: CPU、内存、渲染分析
- **Frame Debugger**: 渲染流程分析
- **Memory Profiler**: 详细内存分析

### 6.2 自定义工具
- **FPS Counter**: 实时帧率显示
- **Memory Tracker**: 内存使用监控
- **Load Time Logger**: 加载时间记录

### 6.3 自动化测试集成
```yaml
# performance-test.yml 示例
performance_tests:
  - name: benchmark_all_scenes
    scenes: [ShallowSea, DeepSea, DangerZone]
    duration: 60
    metrics: [fps, memory, draw_calls]
    
  - name: stress_test_4h
    scene: DeepSea
    duration: 14400
    metrics: [memory_leak, stability]
```
