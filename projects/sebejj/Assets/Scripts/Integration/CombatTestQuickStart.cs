using UnityEngine;
using UnityEngine.SceneManagement;

namespace SebeJJ.Integration
{
    /// <summary>
    /// 战斗测试场景快速启动器
    /// 附加到场景中的空物体即可快速启动测试
    /// </summary>
    public class CombatTestQuickStart : MonoBehaviour
    {
        [Header("快速启动设置")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private float initDelay = 0.5f;

        [Header("测试配置")]
        [SerializeField] private int mechFishCount = 2;
        [SerializeField] private int mechCrabCount = 1;
        [SerializeField] private bool spawnBoundaries = true;

        private void Start()
        {
            if (autoInitialize)
            {
                Invoke(nameof(Initialize), initDelay);
            }
        }

        private void Initialize()
        {
            Debug.Log("[CombatTestQuickStart] 初始化50米深度测试区...");

            // 1. 创建集成系统
            CreateIntegrationSystems();

            // 2. 创建测试生成器
            CreateTestSpawner();

            // 3. 设置相机
            SetupCamera();

            Debug.Log("[CombatTestQuickStart] 初始化完成! 按以下键位开始测试:");
            Debug.Log("  - WASD/方向键: 移动");
            Debug.Log("  - 鼠标左键: 攻击");
            Debug.Log("  - 1/2/3: 切换武器");
            Debug.Log("  - 鼠标滚轮: 切换武器");
        }

        private void CreateIntegrationSystems()
        {
            // CombatIntegrationSystem
            if (FindObjectOfType<CombatIntegrationSystem>() == null)
            {
                var go = new GameObject("CombatIntegrationSystem");
                go.AddComponent<CombatIntegrationSystem>();
            }

            // LootDropSystem
            if (FindObjectOfType<LootDropSystem>() == null)
            {
                var go = new GameObject("LootDropSystem");
                go.AddComponent<LootDropSystem>();
            }

            // CombatFeedback
            if (FindObjectOfType<CombatFeedback>() == null)
            {
                var go = new GameObject("CombatFeedback");
                go.AddComponent<CombatFeedback>();
            }
        }

        private void CreateTestSpawner()
        {
            var existingSpawner = FindObjectOfType<TestSceneSpawner>();
            if (existingSpawner != null)
            {
                existingSpawner.InitializeTestScene();
                return;
            }

            var go = new GameObject("TestSceneSpawner");
            var spawner = go.AddComponent<TestSceneSpawner>();
            
            // 配置生成器
            spawner.SetPrivateField("mechFishCount", mechFishCount);
            spawner.SetPrivateField("mechCrabCount", mechCrabCount);
            spawner.SetPrivateField("createBoundaries", spawnBoundaries);
            spawner.SetPrivateField("spawnOnStart", false);

            spawner.InitializeTestScene();
        }

        private void SetupCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var camObj = new GameObject("MainCamera");
                camera = camObj.AddComponent<Camera>();
                camera.tag = "MainCamera";
                camera.orthographic = true;
                camera.orthographicSize = 10;
            }

            // 添加相机跟随脚本
            var follow = camera.GetComponent<CameraFollow>();
            if (follow == null)
            {
                follow = camera.gameObject.AddComponent<CameraFollow>();
            }

            // 设置跟随目标
            if (MechController.Instance != null)
            {
                follow.SetPrivateField("target", MechController.Instance.transform);
            }
        }

        private void Update()
        {
            // 调试快捷键
            HandleDebugInput();
        }

        private void HandleDebugInput()
        {
            // R - 重新生成敌人
            if (Input.GetKeyDown(KeyCode.R))
            {
                var spawner = FindObjectOfType<TestSceneSpawner>();
                spawner?.RespawnScene();
            }

            // K - 杀死所有敌人
            if (Input.GetKeyDown(KeyCode.K))
            {
                var enemies = FindObjectsOfType<EnemyBase>();
                foreach (var enemy in enemies)
                {
                    enemy.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
                }
            }

            // H - 治疗玩家
            if (Input.GetKeyDown(KeyCode.H))
            {
                CombatIntegrationSystem.Instance?.HealPlayer(50);
            }

            // F1 - 显示帮助
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ShowHelp();
            }
        }

        private void ShowHelp()
        {
            Debug.Log(@"
========== 战斗测试帮助 ==========
移动: WASD / 方向键
攻击: 鼠标左键
切换武器: 1/2/3 或 鼠标滚轮
扫描: 空格
采集: E

调试快捷键:
R - 重新生成敌人
K - 杀死所有敌人
H - 治疗玩家50点
F1 - 显示此帮助
==================================
");
        }
    }

    /// <summary>
    /// 相机跟随脚本
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
    }

    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class CombatTestExtensions
    {
        public static void SetPrivateField<T>(this T obj, string fieldName, object value)
        {
            var field = typeof(T).GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public);
            
            if (field != null)
            {
                field.SetValue(obj, value);
            }
        }
    }
}
