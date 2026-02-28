/**
 * @file BossTestScene.cs
 * @brief Boss战测试场景设置
 * @description 快速测试Boss战的场景配置
 * @author Boss战设计师
 * @date 2026-02-27
 */

using UnityEngine;
using SebeJJ.AI;
using SebeJJ.Combat;

namespace SebeJJ.Boss
{
    /// <summary>
    /// Boss测试场景管理器
    /// </summary>
    public class BossTestScene : MonoBehaviour
    {
        [Header("=== 测试配置 ===")]
        [SerializeField] private bool autoStartBossFight = true;
        [SerializeField] private bool infinitePlayerHealth = false;
        [SerializeField] private bool showDebugInfo = true;
        
        [Header("=== 引用 ===")]
        [SerializeField] private IronClawBeastBoss boss;
        [SerializeField] private BossArena arena;
        [SerializeField] private Transform playerSpawnPoint;
        
        [Header("=== 调试快捷键 ===")]
        [SerializeField] private KeyCode damageBossKey = KeyCode.F1;
        [SerializeField] private KeyCode healBossKey = KeyCode.F2;
        [SerializeField] private KeyCode skipToPhase2Key = KeyCode.F3;
        [SerializeField] private KeyCode skipToPhase3Key = KeyCode.F4;
        [SerializeField] private KeyCode killBossKey = KeyCode.F5;
        [SerializeField] private KeyCode resetBossKey = KeyCode.F6;

        private void Start()
        {
            InitializeTestScene();
        }

        private void Update()
        {
            HandleDebugInput();
        }

        private void InitializeTestScene()
        {
            // 确保Boss存在
            if (boss == null)
            {
                boss = FindObjectOfType<IronClawBeastBoss>();
            }
            
            // 确保场景管理器存在
            if (arena == null)
            {
                arena = FindObjectOfType<BossArena>();
            }
            
            // 创建预警系统
            if (CombatWarningSystem.Instance == null)
            {
                GameObject warningSystemObj = new GameObject("CombatWarningSystem");
                warningSystemObj.AddComponent<CombatWarningSystem>();
            }
            
            // 创建相机震动
            if (CameraShake.Instance == null)
            {
                GameObject cameraObj = new GameObject("CameraShake");
                cameraObj.AddComponent<CameraShake>();
            }
            
            Debug.Log("[BossTestScene] 测试场景初始化完成");
        }

        private void HandleDebugInput()
        {
            if (boss == null) return;
            
            // 对Boss造成伤害
            if (Input.GetKeyDown(damageBossKey))
            {
                boss.TakeDamage(500f, null);
                Debug.Log("[Debug] 对Boss造成500点伤害");
            }
            
            // 治疗Boss
            if (Input.GetKeyDown(healBossKey))
            {
                // 通过反射或直接修改（实际项目中应有Heal方法）
                Debug.Log("[Debug] 治疗Boss（需要在Boss类中添加Heal方法）");
            }
            
            // 跳到第二阶段
            if (Input.GetKeyDown(skipToPhase2Key))
            {
                float damageNeeded = boss.MaxHealth * (1f - 0.6f);
                boss.TakeDamage(damageNeeded - (boss.MaxHealth - boss.CurrentHealth), null);
                Debug.Log("[Debug] 跳到第二阶段");
            }
            
            // 跳到第三阶段
            if (Input.GetKeyDown(skipToPhase3Key))
            {
                float damageNeeded = boss.MaxHealth * (1f - 0.3f);
                boss.TakeDamage(damageNeeded - (boss.MaxHealth - boss.CurrentHealth), null);
                Debug.Log("[Debug] 跳到第三阶段");
            }
            
            // 击杀Boss
            if (Input.GetKeyDown(killBossKey))
            {
                boss.TakeDamage(boss.CurrentHealth, null);
                Debug.Log("[Debug] 击杀Boss");
            }
            
            // 重置Boss
            if (Input.GetKeyDown(resetBossKey))
            {
                // 重新加载场景或重置Boss状态
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }

        private void OnGUI()
        {
            if (!showDebugInfo) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("=== Boss战测试工具 ===", GUILayout.Height(30));
            
            if (boss != null)
            {
                GUILayout.Label($"Boss血量: {boss.CurrentHealth:0}/{boss.MaxHealth:0}");
                GUILayout.Label($"血量百分比: {boss.HealthPercent:P0}");
                GUILayout.Label($"当前阶段: {boss.CurrentPhase}");
                GUILayout.Label($"是否狂暴: {boss.IsEnraged}");
                GUILayout.Label($"是否防御: {boss.IsDefending}");
                GUILayout.Label($"弱点暴露: {boss.IsWeakPointExposed}");
                GUILayout.Label($"当前连击: {boss.CurrentCombo}");
            }
            
            GUILayout.Space(10);
            GUILayout.Label("=== 调试快捷键 ===");
            GUILayout.Label($"{damageBossKey}: 对Boss造成500伤害");
            GUILayout.Label($"{healBossKey}: 治疗Boss");
            GUILayout.Label($"{skipToPhase2Key}: 跳到第二阶段");
            GUILayout.Label($"{skipToPhase3Key}: 跳到第三阶段");
            GUILayout.Label($"{killBossKey}: 击杀Boss");
            GUILayout.Label($"{resetBossKey}: 重置场景");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
