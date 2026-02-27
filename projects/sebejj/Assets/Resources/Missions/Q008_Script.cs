using UnityEngine;
using System.Collections;

public class Q008_Script : MissionBase
{
    private GameObject bossInstance;
    private string bossName = "铁钳巨兽";
    private string bossId = "iron_claw_behemoth";
    private Vector3 bossSpawnPos = new Vector3(0f, -180f, 0f);
    private bool bossDefeated = false;
    
    // Boss战斗阶段
    private enum BossPhase { Phase1, Phase2, Phase3 }
    private BossPhase currentPhase = BossPhase.Phase1;
    
    public override void OnMissionStart()
    {
        base.OnMissionStart();
        Debug.Log("[Q008] 巨型机械蟹任务开始");
        
        UIManager.Instance.ShowMissionBrief("巨型机械蟹", $"击败Boss'{bossName}'！");
        
        // 播放Boss登场动画
        StartCoroutine(BossIntroSequence());
    }
    
    private IEnumerator BossIntroSequence()
    {
        // 锁定玩家控制
        PlayerController.Instance.LockControls(true);
        
        // 镜头移动到Boss位置
        CameraController.Instance.FocusOnPoint(bossSpawnPos, 2f);
        
        yield return new WaitForSeconds(2f);
        
        // 生成Boss
        bossInstance = BossManager.Instance.SpawnBoss(bossId, bossSpawnPos);
        
        // 播放登场动画
        Animator bossAnimator = bossInstance.GetComponent<Animator>();
        if (bossAnimator != null)
        {
            bossAnimator.SetTrigger("Intro");
        }
        
        yield return new WaitForSeconds(3f);
        
        // 恢复玩家控制
        PlayerController.Instance.LockControls(false);
        CameraController.Instance.ReturnToPlayer();
        
        // 开始Boss战
        StartCoroutine(BossBattle());
    }
    
    private IEnumerator BossBattle()
    {
        BossHealth bossHealth = bossInstance.GetComponent<BossHealth>();
        
        while (!bossDefeated)
        {
            if (bossInstance == null)
            {
                bossDefeated = true;
                UpdateObjective(1, true);
                CompleteMission();
                yield break;
            }
            
            if (bossHealth != null)
            {
                float healthPercent = bossHealth.GetHealthPercent();
                
                // 根据血量切换阶段
                if (healthPercent <= 0.3f && currentPhase != BossPhase.Phase3)
                {
                    currentPhase = BossPhase.Phase3;
                    EnterPhase3();
                }
                else if (healthPercent <= 0.6f && currentPhase != BossPhase.Phase2 && currentPhase != BossPhase.Phase3)
                {
                    currentPhase = BossPhase.Phase2;
                    EnterPhase2();
                }
            }
            
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    private void EnterPhase2()
    {
        Debug.Log("[Q008] Boss进入第二阶段！");
        UIManager.Instance.ShowNotification("铁钳巨兽进入狂暴状态！");
        
        // 增加攻击频率
        BossAI bossAI = bossInstance.GetComponent<BossAI>();
        if (bossAI != null)
        {
            bossAI.SetAttackSpeed(1.3f);
        }
        
        // 播放阶段转换特效
        EffectManager.Instance.PlayEffect("phase_transition", bossInstance.transform.position);
    }
    
    private void EnterPhase3()
    {
        Debug.Log("[Q008] Boss进入第三阶段！");
        UIManager.Instance.ShowNotification("铁钳巨兽释放全部力量！");
        
        // 大幅增加攻击力和速度
        BossAI bossAI = bossInstance.GetComponent<BossAI>();
        if (bossAI != null)
        {
            bossAI.SetAttackSpeed(1.6f);
            bossAI.SetDamageMultiplier(1.5f);
        }
        
        // 播放最终阶段特效
        EffectManager.Instance.PlayEffect("final_phase", bossInstance.transform.position);
        AudioManager.Instance.PlayBGM("boss_final_phase");
    }
    
    public override void OnMissionComplete()
    {
        base.OnMissionComplete();
        
        // 播放胜利动画
        StartCoroutine(VictorySequence());
    }
    
    private IEnumerator VictorySequence()
    {
        // 慢动作效果
        Time.timeScale = 0.3f;
        yield return new WaitForSeconds(1f);
        Time.timeScale = 1f;
        
        // Boss掉落物品
        DropRewards();
        
        Debug.Log("[Q008] 巨型机械蟹任务完成！");
        UIManager.Instance.ShowMissionComplete("巨型机械蟹", RewardCredits);
        
        // 解锁高级委托和传奇装备
        GameProgress.Instance.UnlockMissionType("advanced_missions");
        GameProgress.Instance.UnlockGearTier("legendary_gear");
    }
    
    private void DropRewards()
    {
        // 生成掉落物
        ItemManager.Instance.SpawnDrop("behemoth_claw", bossSpawnPos + Vector3.left * 2f);
        ItemManager.Instance.SpawnDrop("rare_core", bossSpawnPos + Vector3.right * 2f);
        
        // 额外战利品
        ItemManager.Instance.SpawnDrop("credits_large", bossSpawnPos + Vector3.up * 1f);
    }
    
    public override void OnMissionFail(string reason)
    {
        base.OnMissionFail(reason);
        Time.timeScale = 1f; // 确保时间正常
        UIManager.Instance.ShowMissionFail("巨型机械蟹", reason);
    }
}
