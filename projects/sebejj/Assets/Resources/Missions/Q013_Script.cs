using UnityEngine;
using System.Collections;

/// <summary>
/// Q013古代遗物任务脚本 - BUG-002修复
/// 添加遗物资源引用检查
/// </summary>
public class Q013_Script : MissionBase
{
    [Header("遗物设置")]
    public RelicObject relicObject;
    public Transform relicSpawnPoint;
    public Vector3 relicDestination = new Vector3(50f, -100f, 0f);
    
    [Header("任务阶段")]
    private bool hasFoundRelic = false;
    private bool hasDeliveredRelic = false;
    
    [Header("特效")]
    public ParticleSystem discoveryEffect;
    public AudioClip discoverySound;
    
    public override void OnMissionStart()
    {
        base.OnMissionStart();
        Debug.Log("[Q013] 古代遗物任务开始");
        
        // BUG-002修复: 验证遗物资源
        ValidateRelicResources();
        
        UIManager.Instance.ShowMissionBrief("古代遗物", "探索深海遗迹，找到并带回古代遗物。");
        
        // 生成遗物
        SpawnRelic();
        
        // 标记目标区域
        Minimap.Instance.MarkArea(relicSpawnPoint.position, 20f, "relic_area");
    }
    
    /// <summary>
    /// 验证遗物资源 - BUG-002修复
    /// </summary>
    private void ValidateRelicResources()
    {
        // 检查遗物对象
        if (relicObject == null)
        {
            Debug.LogError("[Q013] 遗物对象引用缺失!");
            
            // 尝试在场景中查找
            var foundRelic = FindObjectOfType<RelicObject>();
            if (foundRelic != null)
            {
                relicObject = foundRelic;
                Debug.Log("[Q013] 已自动修复遗物引用");
            }
            else
            {
                // 创建临时遗物
                CreateFallbackRelic();
            }
        }
        
        // 检查遗物模型
        if (relicObject != null && relicObject.model == null)
        {
            Debug.LogWarning("[Q013] 遗物模型缺失，使用默认模型");
            
            // 使用验证器修复
            var validator = SebeJJ.Systems.RelicResourceValidator.Instance;
            if (validator != null)
            {
                validator.FixRelicResource(relicObject);
            }
        }
    }
    
    /// <summary>
    /// 生成遗物
    /// </summary>
    private void SpawnRelic()
    {
        if (relicObject == null)
        {
            Debug.LogError("[Q013] 无法生成遗物 - 遗物对象为空");
            return;
        }
        
        // 设置遗物位置
        if (relicSpawnPoint != null)
        {
            relicObject.transform.position = relicSpawnPoint.position;
        }
        
        // 确保遗物可交互
        var collider = relicObject.GetComponent<Collider2D>();
        if (collider == null)
        {
            relicObject.gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
        }
        
        Debug.Log("[Q013] 遗物已生成在位置: " + relicObject.transform.position);
    }
    
    /// <summary>
    /// 创建备用遗物
    /// </summary>
    private void CreateFallbackRelic()
    {
        GameObject fallbackRelic = new GameObject("FallbackRelic");
        fallbackRelic.transform.position = relicSpawnPoint?.position ?? Vector3.zero;
        
        relicObject = fallbackRelic.AddComponent<RelicObject>();
        relicObject.relicId = "Q013_AncientRelic";
        relicObject.relicName = "古代遗物";
        relicObject.description = "一个神秘的古代遗物，散发着微弱的能量波动。";
        relicObject.value = 2000;
        
        // 添加基本模型
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(fallbackRelic.transform);
        cube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        // 添加发光效果
        var light = fallbackRelic.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = Color.cyan;
        light.intensity = 1f;
        light.range = 3f;
        
        Debug.Log("[Q013] 已创建备用遗物");
    }
    
    /// <summary>
    /// 当玩家发现遗物
    /// </summary>
    public void OnRelicDiscovered()
    {
        if (hasFoundRelic) return;
        
        hasFoundRelic = true;
        
        // 播放发现特效
        if (discoveryEffect != null)
        {
            Instantiate(discoveryEffect, relicObject.transform.position, Quaternion.identity);
        }
        
        // 播放音效
        if (discoverySound != null)
        {
            AudioManager.Instance?.PlaySFX(discoverySound);
        }
        
        // 更新任务目标
        UpdateObjective(0, true);
        UIManager.Instance.ShowNotification("发现了古代遗物! 将其带回基地。");
        
        // 更新小地图标记
        Minimap.Instance.ClearMarks();
        Minimap.Instance.MarkDestination(relicDestination, "base");
        
        Debug.Log("[Q013] 玩家发现了遗物");
    }
    
    /// <summary>
    /// 当遗物被采集
    /// </summary>
    public void OnRelicCollected()
    {
        if (!hasFoundRelic)
        {
            OnRelicDiscovered();
        }
        
        // 遗物已添加到背包
        UIManager.Instance.ShowNotification("古代遗物已放入背包");
        
        Debug.Log("[Q013] 玩家采集了遗物");
    }
    
    /// <summary>
    /// 检查是否到达目的地
    /// </summary>
    private void CheckDelivery()
    {
        if (hasDeliveredRelic) return;
        
        float distanceToDestination = Vector3.Distance(
            PlayerController.Instance.transform.position, 
            relicDestination);
        
        if (distanceToDestination < 5f && hasFoundRelic)
        {
            hasDeliveredRelic = true;
            CompleteMission();
        }
    }
    
    private void Update()
    {
        base.Update();
        
        if (hasFoundRelic && !hasDeliveredRelic)
        {
            CheckDelivery();
        }
    }
    
    public override void OnMissionComplete()
    {
        base.OnMissionComplete();
        
        Minimap.Instance.ClearMarks();
        
        UIManager.Instance.ShowMissionComplete("古代遗物", RewardCredits);
        
        // 解锁后续内容
        GameProgress.Instance?.UnlockZone("ancient_ruins_access");
        
        Debug.Log("[Q013] 古代遗物任务完成!");
    }
    
    public override void OnMissionFail(string reason)
    {
        base.OnMissionFail(reason);
        
        Minimap.Instance.ClearMarks();
        UIManager.Instance.ShowMissionFail("古代遗物", reason);
    }
}

/// <summary>
/// 遗物对象组件
/// </summary>
public class RelicObject : MonoBehaviour
{
    public string relicId;
    public string relicName;
    [TextArea(2, 4)]
    public string description;
    public int value = 1000;
    public SebeJJ.Systems.RelicRarity rarity = SebeJJ.Systems.RelicRarity.Rare;
    
    [Header("资源引用")]
    public GameObject model;
    public Material material;
    public Sprite icon;
    
    [Header("效果")]
    public ParticleSystem collectionEffect;
    public AudioClip collectionSound;
    
    private void Start()
    {
        // 自动验证资源
        if (model == null || material == null)
        {
            SebeJJ.Systems.RelicResourceValidator.Instance?.FixRelicResource(this);
        }
        
        ApplyResources();
    }
    
    private void ApplyResources()
    {
        if (model != null)
        {
            var instance = Instantiate(model, transform);
            instance.transform.localPosition = Vector3.zero;
        }
        
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.material = material;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var mission = FindObjectOfType<Q013_Script>();
            if (mission != null)
            {
                mission.OnRelicDiscovered();
                mission.OnRelicCollected();
            }
        }
    }
}
