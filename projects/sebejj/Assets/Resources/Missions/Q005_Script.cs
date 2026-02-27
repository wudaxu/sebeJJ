using UnityEngine;
using System.Collections;

public class Q005_Script : MissionBase
{
    private int defeatedCount = 0;
    private int targetCount = 5;
    private string enemyType = "mechanical_fish";
    
    public override void OnMissionStart()
    {
        base.OnMissionStart();
        Debug.Log("[Q005] 清除威胁任务开始");
        
        UIManager.Instance.ShowMissionBrief("清除威胁", $"击败{targetCount}只机械鱼。");
        
        // 订阅敌人死亡事件
        EventManager.Instance.OnEnemyDefeated += OnEnemyDefeated;
        
        // 生成机械鱼
        SpawnMechanicalFish();
    }
    
    private void SpawnMechanicalFish()
    {
        // 在任务区域生成机械鱼
        for (int i = 0; i < targetCount + 2; i++) // 多生成几只
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            EnemyManager.Instance.SpawnEnemy(enemyType, spawnPos);
        }
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        // 在50-150米深度范围内随机生成
        float depth = Random.Range(50f, 150f);
        float x = Random.Range(-50f, 50f);
        float z = Random.Range(-50f, 50f);
        return new Vector3(x, -depth, z);
    }
    
    private void OnEnemyDefeated(string type)
    {
        if (type == enemyType)
        {
            defeatedCount++;
            UIManager.Instance.UpdateMissionProgress($"机械鱼: {defeatedCount}/{targetCount}");
            
            if (defeatedCount >= targetCount)
            {
                UpdateObjective(1, true);
                CompleteMission();
            }
        }
    }
    
    public override void OnMissionComplete()
    {
        base.OnMissionComplete();
        
        // 取消订阅事件
        EventManager.Instance.OnEnemyDefeated -= OnEnemyDefeated;
        
        Debug.Log("[Q005] 清除威胁任务完成！");
        UIManager.Instance.ShowMissionComplete("清除威胁", RewardCredits);
        
        // 解锁战斗委托类型
        GameProgress.Instance.UnlockMissionType("combat_missions");
    }
    
    public override void OnMissionFail(string reason)
    {
        base.OnMissionFail(reason);
        EventManager.Instance.OnEnemyDefeated -= OnEnemyDefeated;
        UIManager.Instance.ShowMissionFail("清除威胁", reason);
    }
}
