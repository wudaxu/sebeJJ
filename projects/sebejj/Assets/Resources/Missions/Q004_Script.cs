using UnityEngine;
using System.Collections;

public class Q004_Script : MissionBase
{
    private bool hasReachedTargetDepth = false;
    private bool hasReturned = false;
    private float targetDepth = 100f;
    
    public override void OnMissionStart()
    {
        base.OnMissionStart();
        Debug.Log("[Q004] 深海初探任务开始");
        
        // 显示任务简报
        UIManager.Instance.ShowMissionBrief("深海初探", "到达100米深度并安全返回水面。");
        
        // 启动深度监测
        StartCoroutine(MonitorDepth());
    }
    
    private IEnumerator MonitorDepth()
    {
        while (!IsCompleted)
        {
            float currentDepth = PlayerController.Instance.GetCurrentDepth();
            
            // 检测是否到达目标深度
            if (!hasReachedTargetDepth && currentDepth >= targetDepth)
            {
                hasReachedTargetDepth = true;
                UpdateObjective(1, true);
                UIManager.Instance.ShowNotification("已到达100米深度！现在返回水面。");
                
                // 触发环境数据收集事件
                CollectEnvironmentalData();
            }
            
            // 检测是否安全返回
            if (hasReachedTargetDepth && !hasReturned && currentDepth <= 0)
            {
                hasReturned = true;
                UpdateObjective(2, true);
                CompleteMission();
            }
            
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    private void CollectEnvironmentalData()
    {
        // 收集环境数据逻辑
        Debug.Log("[Q004] 正在收集100米深度环境数据...");
        // 可以在这里添加粒子效果或音效
        AudioManager.Instance.PlaySFX("data_collect");
    }
    
    public override void OnMissionComplete()
    {
        base.OnMissionComplete();
        Debug.Log("[Q004] 深海初探任务完成！");
        UIManager.Instance.ShowMissionComplete("深海初探", RewardCredits);
        
        // 解锁100米深度区域
        GameProgress.Instance.UnlockZone("depth_100_zone");
    }
    
    public override void OnMissionFail(string reason)
    {
        base.OnMissionFail(reason);
        Debug.Log($"[Q004] 任务失败: {reason}");
        UIManager.Instance.ShowMissionFail("深海初探", reason);
    }
}
