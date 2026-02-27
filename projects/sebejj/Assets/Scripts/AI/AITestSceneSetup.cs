/** 
 * @file AITestSceneSetup.cs
 * @brief AI测试场景设置
 * @description 快速设置AI测试场景
 * @author AI系统架构师
 * @date 2026-02-27
 */

using UnityEngine;
using SebeJJ.Enemies;

namespace SebeJJ.AI.Test
{
    /// <summary>
    /// AI测试场景设置器
    /// </summary>
    public class AITestSceneSetup : MonoBehaviour
    {
        #region 序列化字段
        
        [Header("测试对象预制体")]
        [SerializeField] private GameObject mechFishPrefab;
        [SerializeField] private GameObject mechCrabPrefab;
        [SerializeField] private GameObject mechJellyfishPrefab;
        [SerializeField] private GameObject playerPrefab;
        
        [Header("测试配置")]
        [SerializeField] private bool autoSetup = true;
        [SerializeField] private Vector3 playerSpawnPos = Vector3.zero;
        [SerializeField] private Vector3 fishSpawnPos = new Vector3(10f, 0f, 0f);
        [SerializeField] private Vector3 crabSpawnPos = new Vector3(-10f, 0f, 0f);
        [SerializeField] private Vector3 jellyfishSpawnPos = new Vector3(0f, 10f, 0f);
        
        [Header("环境")]
        [SerializeField] private bool createGround = true;
        [SerializeField] private Vector2 groundSize = new Vector2(50f, 30f);
        
        #endregion

        #region Unity生命周期
        
        private void Start()
        {
            if (autoSetup)
            {
                SetupScene();
            }
        }
        
        #endregion

        #region 场景设置
        
        /// <summary>
        /// 设置测试场景
        /// </summary>
        [ContextMenu("设置测试场景")]
        public void SetupScene()
        {
            UnityEngine.Debug.Log("[AITestSceneSetup] 开始设置AI测试场景...");
            
            // 创建地面
            if (createGround)
            {
                CreateGround();
            }
            
            // 创建玩家
            GameObject player = CreatePlayer();
            
            // 创建敌人
            CreateMechFish(player?.transform);
            CreateMechCrab(player?.transform);
            CreateMechJellyfish(player?.transform);
            
            // 创建寻路系统
            CreatePathfinding();
            
            // 创建调试器
            CreateDebugger();
            
            UnityEngine.Debug.Log("[AITestSceneSetup] AI测试场景设置完成！");
        }
        
        /// <summary>
        /// 创建地面
        /// </summary>
        private void CreateGround()
        {
            GameObject ground = new GameObject("Ground");
            ground.tag = "Ground";
            ground.layer = LayerMask.NameToLayer("Ground");
            
            // 添加碰撞器
            BoxCollider2D collider = ground.AddComponent<BoxCollider2D>();
            collider.size = groundSize;
            
            // 添加视觉（可选）
            SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.2f, 0.2f, 0.2f);
            
            UnityEngine.Debug.Log("[AITestSceneSetup] 地面创建完成");
        }
        
        /// <summary>
        /// 创建玩家
        /// </summary>
        /// <returns>玩家对象</returns>
        private GameObject CreatePlayer()
        {
            if (playerPrefab == null)
            {
                // 创建简单玩家
                GameObject player = new GameObject("Player");
                player.tag = "Player";
                player.transform.position = playerSpawnPos;
                
                // 添加碰撞器
                CircleCollider2D collider = player.AddComponent<CircleCollider2D>();
                collider.radius = 0.5f;
                
                // 添加刚体
                Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.freezeRotation = true;
                
                // 添加视觉
                SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
                sr.color = Color.blue;
                
                UnityEngine.Debug.Log("[AITestSceneSetup] 玩家创建完成");
                return player;
            }
            else
            {
                GameObject player = Instantiate(playerPrefab, playerSpawnPos, Quaternion.identity);
                player.name = "Player";
                UnityEngine.Debug.Log("[AITestSceneSetup] 玩家创建完成（从预制体）");
                return player;
            }
        }
        
        /// <summary>
        /// 创建机械鱼
        /// </summary>
        /// <param name="player">玩家Transform</param>
        private void CreateMechFish(Transform player)
        {
            if (mechFishPrefab == null)
            {
                UnityEngine.Debug.LogWarning("[AITestSceneSetup] 机械鱼预制体未设置");
                return;
            }
            
            GameObject fish = Instantiate(mechFishPrefab, fishSpawnPos, Quaternion.identity);
            fish.name = "MechFish_Test";
            
            // 设置目标
            var perception = fish.GetComponent<AIPerception>();
            if (perception != null && player != null)
            {
                perception.ForceSetTarget(player);
            }
            
            UnityEngine.Debug.Log("[AITestSceneSetup] 机械鱼创建完成");
        }
        
        /// <summary>
        /// 创建机械蟹
        /// </summary>
        /// <param name="player">玩家Transform</param>
        private void CreateMechCrab(Transform player)
        {
            if (mechCrabPrefab == null)
            {
                UnityEngine.Debug.LogWarning("[AITestSceneSetup] 机械蟹预制体未设置");
                return;
            }
            
            GameObject crab = Instantiate(mechCrabPrefab, crabSpawnPos, Quaternion.identity);
            crab.name = "MechCrab_Test";
            
            // 设置目标
            var perception = crab.GetComponent<AIPerception>();
            if (perception != null && player != null)
            {
                perception.ForceSetTarget(player);
            }
            
            UnityEngine.Debug.Log("[AITestSceneSetup] 机械蟹创建完成");
        }
        
        /// <summary>
        /// 创建机械水母
        /// </summary>
        /// <param name="player">玩家Transform</param>
        private void CreateMechJellyfish(Transform player)
        {
            if (mechJellyfishPrefab == null)
            {
                UnityEngine.Debug.LogWarning("[AITestSceneSetup] 机械水母预制体未设置");
                return;
            }
            
            GameObject jellyfish = Instantiate(mechJellyfishPrefab, jellyfishSpawnPos, Quaternion.identity);
            jellyfish.name = "MechJellyfish_Test";
            
            // 设置目标
            var perception = jellyfish.GetComponent<AIPerception>();
            if (perception != null && player != null)
            {
                perception.ForceSetTarget(player);
            }
            
            UnityEngine.Debug.Log("[AITestSceneSetup] 机械水母创建完成");
        }
        
        /// <summary>
        /// 创建寻路系统
        /// </summary>
        private void CreatePathfinding()
        {
            GameObject pathfinderObj = new GameObject("AStarPathfinding");
            var pathfinding = pathfinderObj.AddComponent<AStarPathfinding>();
            
            UnityEngine.Debug.Log("[AITestSceneSetup] 寻路系统创建完成");
        }
        
        /// <summary>
        /// 创建调试器
        /// </summary>
        private void CreateDebugger()
        {
            GameObject debuggerObj = new GameObject("AIDebugger");
            debuggerObj.AddComponent<AIDebugger>();
            
            UnityEngine.Debug.Log("[AITestSceneSetup] 调试器创建完成");
        }
        
        #endregion
    }
}
