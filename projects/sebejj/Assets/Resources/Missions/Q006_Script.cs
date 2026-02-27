using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Q006_Script : MissionBase
{
    private int collectedCount = 0;
    private int targetCount = 3;
    private string itemType = "titanium_alloy";
    private List<Vector3> spawnPoints = new List<Vector3>();
    
    public override void OnMissionStart()
    {
        base.OnMissionStart();
        Debug.Log("[Q006] 稀有金属任务开始");
        
        UIManager.Instance.ShowMissionBrief("稀有金属", $"在80米深度采集{targetCount}个钛合金。");
        
        // 设置采集点
        SetupCollectionPoints();
        
        // 订阅物品收集事件
        EventManager.Instance.OnItemCollected += OnItemCollected;
    }
    
    private void SetupCollectionPoints()
    {
        // 在80米深度设置3个钛合金采集点
        spawnPoints.Add(new Vector3(-20f, -80f, 10f));
        spawnPoints.Add(new Vector3(15f, -80f, -25f));
        spawnPoints.Add(new Vector3(30f, -80f, 20f));
        
        foreach (var point in spawnPoints)
        {
            ItemManager.Instance.SpawnCollectible(itemType, point);
        }
        
        // 在地图上标记采集点
        Minimap.Instance.MarkPoints(spawnPoints, "titanium_marker");
    }
    
    private void OnItemCollected(string type, int amount)
    {
        if (type == itemType)
        {
            collectedCount += amount;
            UIManager.Instance.UpdateMissionProgress($"钛合金: {collectedCount}/{targetCount}");
            
            // 播放收集音效
            AudioManager.Instance.PlaySFX("item_collect");
            
            if (collectedCount >= targetCount)
            {
                UpdateObjective(1, true);
                CompleteMission();
            }
        }
    }
    
    public override void OnMissionComplete()
    {
        base.OnMissionComplete();
        
        EventManager.Instance.OnItemCollected -= OnItemCollected;
        Minimap.Instance.ClearMarks();
        
        Debug.Log("[Q006] 稀有金属任务完成！");
        UIManager.Instance.ShowMissionComplete("稀有金属", RewardCredits);
    }
    
    public override void OnMissionFail(string reason)
    {
        base.OnMissionFail(reason);
        EventManager.Instance.OnItemCollected -= OnItemCollected;
        Minimap.Instance.ClearMarks();
        UIManager.Instance.ShowMissionFail("稀有金属", reason);
    }
}
