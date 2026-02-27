using UnityEngine;
using System.Collections;

public class Q007_Script : MissionBase
{
    private GameObject scientistNPC;
    private Vector3 destination = new Vector3(0f, -120f, 0f);
    private bool hasReachedDestination = false;
    private float npcFollowDistance = 5f;
    private float scientistMaxHealth = 100f;
    private float scientistCurrentHealth = 100f;
    
    public override void OnMissionStart()
    {
        base.OnMissionStart();
        Debug.Log("[Q007] 护送科学家任务开始");
        
        UIManager.Instance.ShowMissionBrief("护送科学家", "护送科学家安全到达120米研究站。");
        
        // 生成科学家NPC
        SpawnScientist();
        
        // 在地图上标记目的地
        Minimap.Instance.MarkDestination(destination, "research_station");
        
        // 启动护送逻辑
        StartCoroutine(EscortLogic());
        StartCoroutine(MonitorScientistHealth());
    }
    
    private void SpawnScientist()
    {
        Vector3 spawnPos = PlayerController.Instance.transform.position + Vector3.right * 3f;
        scientistNPC = NPCManager.Instance.SpawnNPC("scientist", spawnPos);
        
        // 设置NPC跟随参数
        NPCFollow followComponent = scientistNPC.GetComponent<NPCFollow>();
        if (followComponent != null)
        {
            followComponent.SetTarget(PlayerController.Instance.transform);
            followComponent.SetFollowDistance(npcFollowDistance);
            followComponent.SetSpeed(0.8f);
        }
        
        UIManager.Instance.ShowNotification("科学家已加入队伍，请保护他的安全！");
    }
    
    private IEnumerator EscortLogic()
    {
        while (!hasReachedDestination && scientistCurrentHealth > 0)
        {
            if (scientistNPC != null)
            {
                float distanceToDestination = Vector3.Distance(scientistNPC.transform.position, destination);
                
                if (distanceToDestination < 5f)
                {
                    hasReachedDestination = true;
                    UpdateObjective(1, true);
                    CompleteMission();
                }
            }
            
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    private IEnumerator MonitorScientistHealth()
    {
        while (!IsCompleted)
        {
            if (scientistNPC != null)
            {
                NPCHealth healthComponent = scientistNPC.GetComponent<NPCHealth>();
                if (healthComponent != null)
                {
                    scientistCurrentHealth = healthComponent.GetCurrentHealth();
                    UIManager.Instance.UpdateNPCHealthBar(scientistCurrentHealth / scientistMaxHealth);
                    
                    if (scientistCurrentHealth <= 0)
                    {
                        FailMission("科学家阵亡");
                    }
                }
            }
            
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    public override void OnMissionComplete()
    {
        base.OnMissionComplete();
        
        // 科学家到达目的地后的行为
        if (scientistNPC != null)
        {
            NPCFollow followComponent = scientistNPC.GetComponent<NPCFollow>();
            if (followComponent != null)
            {
                followComponent.StopFollowing();
            }
            
            // 播放感谢动画
            Animator animator = scientistNPC.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Thank");
            }
        }
        
        Minimap.Instance.ClearMarks();
        Debug.Log("[Q007] 护送科学家任务完成！");
        UIManager.Instance.ShowMissionComplete("护送科学家", RewardCredits);
        
        // 解锁研究站访问权限
        GameProgress.Instance.UnlockZone("research_station_access");
    }
    
    public override void OnMissionFail(string reason)
    {
        base.OnMissionFail(reason);
        Minimap.Instance.ClearMarks();
        UIManager.Instance.ShowMissionFail("护送科学家", reason);
    }
}
