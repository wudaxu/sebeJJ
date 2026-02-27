/**
 * @file TestSceneSpawner.cs
 * @brief 测试场景快速生成器
 * @description 用于快速设置50米深度测试区的敌人和环境
 * @author 系统集成工程师
 * @date 2026-02-27
 */

using UnityEngine;
using SebeJJ.Enemies;
using SebeJJ.Player;
using System.Collections.Generic;

namespace SebeJJ.Integration
{
    /// <summary>
    /// 测试场景快速生成器
    /// </summary>
    public class TestSceneSpawner : MonoBehaviour
    {
        [Header("敌人预制体")]
        [SerializeField] private GameObject mechFishPrefab;
        [SerializeField] private GameObject mechCrabPrefab;
        [SerializeField] private GameObject mechJellyfishPrefab;

        [Header("生成设置")]
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private int mechFishCount = 2;
        [SerializeField] private int mechCrabCount = 1;
        [SerializeField] private int mechJellyfishCount = 0;
        [SerializeField] private float spawnRadius = 10f;
        [SerializeField] private float minSpawnDistance = 5f;

        [Header("玩家设置")]
        [SerializeField] private bool spawnPlayer = true;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Vector3 playerSpawnPosition = Vector3.zero;

        [Header("环境设置")]
        [SerializeField] private bool createBoundaries = true;
        [SerializeField] private float boundarySize = 30f;
        [SerializeField] private Material boundaryMaterial;

        [Header("调试")]
        [SerializeField] private bool showGizmos = true;

        private List<GameObject> spawnedEnemies = new List<GameObject>();

        private void Start()
        {
            if (spawnOnStart)
            {
                InitializeTestScene();
            }
        }

        /// <summary>
        /// 初始化测试场景
        /// </summary>
        public void InitializeTestScene()
        {
            Debug.Log("[TestSceneSpawner] 初始化50米深度测试区...");

            // 创建边界
            if (createBoundaries)
            {
                CreateBoundaries();
            }

            // 生成玩家
            if (spawnPlayer)
            {
                SpawnPlayer();
            }

            // 生成敌人
            SpawnEnemies();

            // 设置集成系统
            SetupIntegrationSystems();

            Debug.Log("[TestSceneSpawner] 测试场景初始化完成!");
        }

        #region 边界创建

        /// <summary>
        /// 创建场景边界
        /// </summary>
        private void CreateBoundaries()
        {
            // 创建边界墙
            CreateWall("TopWall", Vector3.up * boundarySize, new Vector2(boundarySize * 2, 1));
            CreateWall("BottomWall", Vector3.down * boundarySize, new Vector2(boundarySize * 2, 1));
            CreateWall("LeftWall", Vector3.left * boundarySize, new Vector2(1, boundarySize * 2));
            CreateWall("RightWall", Vector3.right * boundarySize, new Vector2(1, boundarySize * 2));

            Debug.Log($"[TestSceneSpawner] 创建边界: {boundarySize}米");
        }

        /// <summary>
        /// 创建单个墙壁
        /// </summary>
        private void CreateWall(string name, Vector3 position, Vector2 size)
        {
            var wall = new GameObject(name);
            wall.transform.position = position;
            wall.tag = "Boundary";
            wall.layer = LayerMask.NameToLayer("Obstacle");

            // 添加碰撞器
            var collider = wall.AddComponent<BoxCollider2D>();
            collider.size = size;

            // 添加视觉效果（可选）
            if (boundaryMaterial != null)
            {
                var sr = wall.AddComponent<SpriteRenderer>();
                sr.material = boundaryMaterial;
                sr.color = new Color(0.2f, 0.2f, 0.3f, 0.5f);
                sr.sortingOrder = -10;
            }
        }

        #endregion

        #region 玩家生成

        /// <summary>
        /// 生成玩家
        /// </summary>
        private void SpawnPlayer()
        {
            // 检查是否已有玩家
            if (MechController.Instance != null)
            {
                MechController.Instance.transform.position = playerSpawnPosition;
                Debug.Log("[TestSceneSpawner] 使用现有玩家实例");
                return;
            }

            // 如果没有预制体，创建一个基本的玩家
            if (playerPrefab == null)
            {
                CreateDefaultPlayer();
            }
            else
            {
                var player = Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity);
                player.name = "Player_Mech";
            }

            Debug.Log($"[TestSceneSpawner] 生成玩家在: {playerSpawnPosition}");
        }

        /// <summary>
        /// 创建默认玩家
        /// </summary>
        private void CreateDefaultPlayer()
        {
            var playerObj = new GameObject("Player_Mech");
            playerObj.transform.position = playerSpawnPosition;
            playerObj.tag = "Player";
            playerObj.layer = LayerMask.NameToLayer("Player");

            // 添加必要组件
            var rb = playerObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.drag = 2;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = playerObj.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;

            // 添加机甲控制器
            var mechController = playerObj.AddComponent<MechController>();

            // 添加战斗组件
            var combatStats = playerObj.AddComponent<CombatStats>();
            var weaponManager = playerObj.AddComponent<WeaponManager>();
            var mechCombat = playerObj.AddComponent<MechCombatController>();

            // 添加视觉
            var sr = playerObj.AddComponent<SpriteRenderer>();
            sr.color = Color.cyan;

            // 创建武器挂载点
            var weaponPivot = new GameObject("WeaponPivot").transform;
            weaponPivot.SetParent(playerObj.transform);
            weaponPivot.localPosition = Vector3.right * 0.5f;

            Debug.Log("[TestSceneSpawner] 创建默认玩家");
        }

        #endregion

        #region 敌人生成

        /// <summary>
        /// 生成所有敌人
        /// </summary>
        private void SpawnEnemies()
        {
            // 生成机械鱼
            for (int i = 0; i < mechFishCount; i++)
            {
                SpawnEnemy(mechFishPrefab, EnemyType.MechFish, $"MechFish_{i}");
            }

            // 生成机械蟹
            for (int i = 0; i < mechCrabCount; i++)
            {
                SpawnEnemy(mechCrabPrefab, EnemyType.MechCrab, $"MechCrab_{i}");
            }

            // 生成机械水母
            for (int i = 0; i < mechJellyfishCount; i++)
            {
                SpawnEnemy(mechJellyfishPrefab, EnemyType.MechJellyfish, $"MechJellyfish_{i}");
            }

            Debug.Log($"[TestSceneSpawner] 生成敌人: {mechFishCount}机械鱼, {mechCrabCount}机械蟹, {mechJellyfishCount}机械水母");
        }

        /// <summary>
        /// 生成单个敌人
        /// </summary>
        private void SpawnEnemy(GameObject prefab, EnemyType type, string name)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"[TestSceneSpawner] {type} 预制体为空，跳过生成");
                return;
            }

            // 计算生成位置
            Vector3 spawnPos = GetValidSpawnPosition();

            // 创建敌人
            var enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
            enemy.name = name;

            // 确保有EnemyBase组件
            var enemyBase = enemy.GetComponent<EnemyBase>();
            if (enemyBase == null)
            {
                // 根据类型添加对应的AI
                switch (type)
                {
                    case EnemyType.MechFish:
                        enemyBase = enemy.AddComponent<MechFishAI>();
                        break;
                    default:
                        Debug.LogWarning($"[TestSceneSpawner] 未实现的敌人类型: {type}");
                        break;
                }
            }

            // 添加伤害桥接器
            var bridge = enemy.GetComponent<EnemyDamageBridge>();
            if (bridge == null)
            {
                bridge = enemy.AddComponent<EnemyDamageBridge>();
            }

            spawnedEnemies.Add(enemy);

            // 注册到集成系统
            if (enemyBase != null)
            {
                CombatIntegrationSystem.Instance?.RegisterEnemy(enemyBase);
            }
        }

        /// <summary>
        /// 获取有效的生成位置
        /// </summary>
        private Vector3 GetValidSpawnPosition()
        {
            Vector3 spawnPos;
            int attempts = 0;
            const int maxAttempts = 50;

            do
            {
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                spawnPos = playerSpawnPosition + new Vector3(randomCircle.x, randomCircle.y, 0);
                attempts++;
            }
            while (Vector3.Distance(spawnPos, playerSpawnPosition) < minSpawnDistance 
                && attempts < maxAttempts);

            return spawnPos;
        }

        #endregion

        #region 集成系统设置

        /// <summary>
        /// 设置集成系统
        /// </summary>
        private void SetupIntegrationSystems()
        {
            // 确保有CombatIntegrationSystem
            var combatIntegration = FindObjectOfType<CombatIntegrationSystem>();
            if (combatIntegration == null)
            {
                var go = new GameObject("CombatIntegrationSystem");
                combatIntegration = go.AddComponent<CombatIntegrationSystem>();
            }

            // 确保有LootDropSystem
            var lootSystem = FindObjectOfType<LootDropSystem>();
            if (lootSystem == null)
            {
                var go = new GameObject("LootDropSystem");
                lootSystem = go.AddComponent<LootDropSystem>();
            }

            // 确保有CombatFeedback
            var combatFeedback = FindObjectOfType<CombatFeedback>();
            if (combatFeedback == null)
            {
                var go = new GameObject("CombatFeedback");
                combatFeedback = go.AddComponent<CombatFeedback>();
            }

            Debug.Log("[TestSceneSpawner] 集成系统设置完成");
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 清除所有生成的敌人
        /// </summary>
        public void ClearEnemies()
        {
            foreach (var enemy in spawnedEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            spawnedEnemies.Clear();
        }

        /// <summary>
        /// 重新生成场景
        /// </summary>
        public void RespawnScene()
        {
            ClearEnemies();
            SpawnEnemies();
        }

        /// <summary>
        /// 生成指定类型的敌人（测试用）
        /// </summary>
        public void SpawnEnemyOfType(EnemyType type)
        {
            GameObject prefab = null;
            switch (type)
            {
                case EnemyType.MechFish:
                    prefab = mechFishPrefab;
                    break;
                case EnemyType.MechCrab:
                    prefab = mechCrabPrefab;
                    break;
                case EnemyType.MechJellyfish:
                    prefab = mechJellyfishPrefab;
                    break;
            }

            if (prefab != null)
            {
                SpawnEnemy(prefab, type, $"{type}_Test");
            }
        }

        #endregion

        #region 调试

        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            // 绘制生成区域
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerSpawnPosition, spawnRadius);

            // 绘制最小距离
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerSpawnPosition, minSpawnDistance);

            // 绘制边界
            if (createBoundaries)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(boundarySize * 2, boundarySize * 2, 0));
            }

            // 绘制生成的敌人
            Gizmos.color = Color.red;
            foreach (var enemy in spawnedEnemies)
            {
                if (enemy != null)
                {
                    Gizmos.DrawSphere(enemy.transform.position, 0.3f);
                }
            }
        }

        #endregion
    }
}
