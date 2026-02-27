/** 
 * @file AIStressTest.cs
 * @brief AI压力测试 - 任务AI-008
 * @description 测试AI系统的性能和稳定性
 * @author AI系统架构师
 * @date 2026-02-27
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using SebeJJ.AI;
using SebeJJ.Enemies;

namespace SebeJJ.AI.Test
{
    /// <summary>
    /// AI压力测试器
    /// </summary>
    public class AIStressTest : MonoBehaviour
    {
        #region 序列化字段
        
        [Header("测试配置")]
        [SerializeField] private int testEnemyCount = 50;
        [SerializeField] private float spawnRadius = 30f;
        [SerializeField] private float testDuration = 60f;
        
        [Header("预制体")]
        [SerializeField] private GameObject mechFishPrefab;
        [SerializeField] private GameObject mechCrabPrefab;
        [SerializeField] private GameObject mechJellyfishPrefab;
        
        [Header("目标")]
        [SerializeField] private Transform playerTarget;
        
        #endregion

        #region 私有字段
        
        private List<GameObject> _spawnedEnemies = new List<GameObject>();
        private Stopwatch _stopwatch = new Stopwatch();
        private float _testStartTime;
        private bool _isTesting = false;
        
        // 性能数据
        private float _avgFrameTime = 0f;
        private float _minFrameTime = float.MaxValue;
        private float _maxFrameTime = 0f;
        private int _frameCount = 0;
        
        #endregion

        #region Unity生命周期
        
        private void Update()
        {
            if (!_isTesting) return;
            
            // 记录帧时间
            float frameTime = Time.unscaledDeltaTime * 1000f; // 转换为毫秒
            _avgFrameTime += frameTime;
            _minFrameTime = Mathf.Min(_minFrameTime, frameTime);
            _maxFrameTime = Mathf.Max(_maxFrameTime, frameTime);
            _frameCount++;
            
            // 检查测试是否结束
            if (Time.time - _testStartTime >= testDuration)
            {
                EndTest();
            }
        }
        
        #endregion

        #region 测试控制
        
        /// <summary>
        /// 开始压力测试
        /// </summary>
        [ContextMenu("开始压力测试")]
        public void StartTest()
        {
            if (_isTesting)
            {
                UnityEngine.Debug.LogWarning("[AIStressTest] 测试已在进行中");
                return;
            }
            
            _isTesting = true;
            _testStartTime = Time.time;
            _frameCount = 0;
            _avgFrameTime = 0f;
            _minFrameTime = float.MaxValue;
            _maxFrameTime = 0f;
            
            UnityEngine.Debug.Log("[AIStressTest] ========== 开始AI压力测试 ==========");
            UnityEngine.Debug.Log($"[AIStressTest] 测试敌人数量: {testEnemyCount}");
            UnityEngine.Debug.Log($"[AIStressTest] 测试持续时间: {testDuration}秒");
            
            // 生成敌人
            SpawnTestEnemies();
            
            _stopwatch.Restart();
        }
        
        /// <summary>
        /// 结束测试
        /// </summary>
        private void EndTest()
        {
            _isTesting = false;
            _stopwatch.Stop();
            
            // 计算平均帧时间
            _avgFrameTime /= _frameCount;
            
            // 输出结果
            PrintResults();
            
            // 清理
            Cleanup();
        }
        
        /// <summary>
        /// 生成测试敌人
        /// </summary>
        private void SpawnTestEnemies()
        {
            int fishCount = testEnemyCount / 3;
            int crabCount = testEnemyCount / 3;
            int jellyfishCount = testEnemyCount - fishCount - crabCount;
            
            // 生成机械鱼
            for (int i = 0; i < fishCount; i++)
            {
                SpawnEnemy(mechFishPrefab, $"MechFish_{i}");
            }
            
            // 生成机械蟹
            for (int i = 0; i < crabCount; i++)
            {
                SpawnEnemy(mechCrabPrefab, $"MechCrab_{i}");
            }
            
            // 生成机械水母
            for (int i = 0; i < jellyfishCount; i++)
            {
                SpawnEnemy(mechJellyfishPrefab, $"MechJellyfish_{i}");
            }
            
            UnityEngine.Debug.Log($"[AIStressTest] 生成了 {_spawnedEnemies.Count} 个敌人");
        }
        
        /// <summary>
        /// 生成单个敌人
        /// </summary>
        private void SpawnEnemy(GameObject prefab, string name)
        {
            if (prefab == null) return;
            
            Vector2 randomPos = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomPos.x, randomPos.y, 0);
            
            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
            enemy.name = name;
            
            // 设置目标
            var perception = enemy.GetComponent<AIPerception>();
            if (perception != null && playerTarget != null)
            {
                perception.ForceSetTarget(playerTarget);
            }
            
            _spawnedEnemies.Add(enemy);
        }
        
        /// <summary>
        /// 清理测试对象
        /// </summary>
        private void Cleanup()
        {
            foreach (var enemy in _spawnedEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            
            _spawnedEnemies.Clear();
        }
        
        #endregion

        #region 结果输出
        
        /// <summary>
        /// 打印测试结果
        /// </summary>
        private void PrintResults()
        {
            UnityEngine.Debug.Log("[AIStressTest] ========== 测试结果 ==========");
            UnityEngine.Debug.Log($"[AIStressTest] 总帧数: {_frameCount}");
            UnityEngine.Debug.Log($"[AIStressTest] 平均帧时间: {_avgFrameTime:F2}ms");
            UnityEngine.Debug.Log($"[AIStressTest] 最小帧时间: {_minFrameTime:F2}ms");
            UnityEngine.Debug.Log($"[AIStressTest] 最大帧时间: {_maxFrameTime:F2}ms");
            UnityEngine.Debug.Log($"[AIStressTest] 平均FPS: {1000f / _avgFrameTime:F1}");
            UnityEngine.Debug.Log($"[AIStressTest] 测试用时: {_stopwatch.Elapsed.TotalSeconds:F2}秒");
            
            // 性能评估
            float targetFrameTime = 16.67f; // 60FPS = 16.67ms
            float performance = _avgFrameTime / targetFrameTime;
            
            if (performance <= 1f)
            {
                UnityEngine.Debug.Log("[AIStressTest] 性能评估: 优秀 ✓");
            }
            else if (performance <= 1.5f)
            {
                UnityEngine.Debug.Log("[AIStressTest] 性能评估: 良好");
            }
            else if (performance <= 2f)
            {
                UnityEngine.Debug.Log("[AIStressTest] 性能评估: 一般");
            }
            else
            {
                UnityEngine.Debug.Log("[AIStressTest] 性能评估: 需要优化 ✗");
            }
            
            UnityEngine.Debug.Log("[AIStressTest] ========== 测试结束 ==========");
        }
        
        #endregion

        #region 性能监控
        
        /// <summary>
        /// 获取当前FPS
        /// </summary>
        public float CurrentFPS => _isTesting ? 1f / Time.unscaledDeltaTime : 0f;
        
        /// <summary>
        /// 获取平均帧时间
        /// </summary>
        public float AverageFrameTime => _isTesting && _frameCount > 0 ? _avgFrameTime / _frameCount : 0f;
        
        /// <summary>
        /// 是否正在测试
        /// </summary>
        public bool IsTesting => _isTesting;
        
        #endregion
    }
}
