using UnityEngine;
using System;
using System.Collections.Generic;

namespace SebeJJ.Core
{
    /// <summary>
    /// 系统调度器 - BUG-018修复
    /// 管理高负载下的系统优先级和异步处理
    /// </summary>
    public class SystemScheduler : MonoBehaviour
    {
        public static SystemScheduler Instance { get; private set; }
        
        [Header("调度设置")]
        [SerializeField] private float maxTimePerFrame = 5f; // 毫秒
        [SerializeField] private int maxTasksPerFrame = 10;
        [SerializeField] private bool enablePriorityQueue = true;
        
        [Header("系统优先级")]
        [SerializeField] private SystemPriority combatPriority = SystemPriority.Critical;
        [SerializeField] private SystemPriority aiPriority = SystemPriority.High;
        [SerializeField] private SystemPriority uiPriority = SystemPriority.Critical;
        [SerializeField] private SystemPriority physicsPriority = SystemPriority.High;
        
        // 任务队列
        private Queue<SystemTask> taskQueue = new Queue<SystemTask>();
        private List<ScheduledUpdate> scheduledUpdates = new List<ScheduledUpdate>();
        
        // 性能监控
        private float frameTimeAccumulator;
        private int frameCount;
        private float averageFrameTime;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Update()
        {
            float frameStartTime = Time.realtimeSinceStartup * 1000;
            
            // 处理高优先级任务
            ProcessHighPriorityTasks();
            
            // 处理常规任务队列
            ProcessTaskQueue();
            
            // 执行计划更新
            ExecuteScheduledUpdates();
            
            // 记录帧时间
            float frameEndTime = Time.realtimeSinceStartup * 1000;
            float frameTime = frameEndTime - frameStartTime;
            
            frameTimeAccumulator += frameTime;
            frameCount++;
            
            if (frameCount >= 60)
            {
                averageFrameTime = frameTimeAccumulator / frameCount;
                frameTimeAccumulator = 0;
                frameCount = 0;
            }
        }
        
        /// <summary>
        /// 调度任务
        /// </summary>
        public void ScheduleTask(Action task, SystemPriority priority = SystemPriority.Normal)
        {
            var systemTask = new SystemTask
            {
                action = task,
                priority = priority,
                enqueueTime = Time.time
            };
            
            if (enablePriorityQueue && priority >= SystemPriority.High)
            {
                // 高优先级任务插队
                var tempQueue = new Queue<SystemTask>();
                tempQueue.Enqueue(systemTask);
                while (taskQueue.Count > 0)
                {
                    tempQueue.Enqueue(taskQueue.Dequeue());
                }
                taskQueue = tempQueue;
            }
            else
            {
                taskQueue.Enqueue(systemTask);
            }
        }
        
        /// <summary>
        /// 注册计划更新
        /// </summary>
        public void RegisterScheduledUpdate(Action updateAction, float interval, SystemPriority priority = SystemPriority.Normal)
        {
            scheduledUpdates.Add(new ScheduledUpdate
            {
                action = updateAction,
                interval = interval,
                priority = priority,
                lastUpdateTime = Time.time
            });
        }
        
        /// <summary>
        /// 取消注册计划更新
        /// </summary>
        public void UnregisterScheduledUpdate(Action updateAction)
        {
            scheduledUpdates.RemoveAll(s => s.action == updateAction);
        }
        
        /// <summary>
        /// 处理高优先级任务
        /// </summary>
        private void ProcessHighPriorityTasks()
        {
            // 高优先级任务立即执行
            while (taskQueue.Count > 0)
            {
                var task = taskQueue.Peek();
                if (task.priority < SystemPriority.High) break;
                
                taskQueue.Dequeue();
                
                try
                {
                    task.action?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SystemScheduler] 任务执行错误: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// 处理任务队列
        /// </summary>
        private void ProcessTaskQueue()
        {
            float startTime = Time.realtimeSinceStartup * 1000;
            int tasksProcessed = 0;
            
            while (taskQueue.Count > 0 && tasksProcessed < maxTasksPerFrame)
            {
                float currentTime = Time.realtimeSinceStartup * 1000;
                if (currentTime - startTime > maxTimePerFrame)
                {
                    // 超出时间限制，留到下一帧
                    break;
                }
                
                var task = taskQueue.Dequeue();
                
                try
                {
                    task.action?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SystemScheduler] 任务执行错误: {e.Message}");
                }
                
                tasksProcessed++;
            }
        }
        
        /// <summary>
        /// 执行计划更新
        /// </summary>
        private void ExecuteScheduledUpdates()
        {
            float currentTime = Time.time;
            
            // 按优先级排序
            scheduledUpdates.Sort((a, b) => b.priority.CompareTo(a.priority));
            
            foreach (var update in scheduledUpdates)
            {
                if (currentTime - update.lastUpdateTime >= update.interval)
                {
                    try
                    {
                        update.action?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[SystemScheduler] 计划更新错误: {e.Message}");
                    }
                    
                    update.lastUpdateTime = currentTime;
                }
            }
        }
        
        /// <summary>
        /// 异步执行任务
        /// </summary>
        public void ExecuteAsync(Action task, Action onComplete = null)
        {
            StartCoroutine(AsyncExecuteCoroutine(task, onComplete));
        }
        
        private System.Collections.IEnumerator AsyncExecuteCoroutine(Action task, Action onComplete)
        {
            yield return null; // 延迟到下一帧
            
            try
            {
                task?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SystemScheduler] 异步任务错误: {e.Message}");
            }
            
            onComplete?.Invoke();
        }
        
        /// <summary>
        /// 获取平均帧时间
        /// </summary>
        public float GetAverageFrameTime()
        {
            return averageFrameTime;
        }
        
        /// <summary>
        /// 获取待处理任务数
        /// </summary>
        public int GetPendingTaskCount()
        {
            return taskQueue.Count;
        }
        
        /// <summary>
        /// 清空任务队列
        /// </summary>
        public void ClearTaskQueue()
        {
            taskQueue.Clear();
        }
    }
    
    #region 数据类
    
    /// <summary>
    /// 系统优先级
    /// </summary>
    public enum SystemPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }
    
    /// <summary>
    /// 系统任务
    /// </summary>
    public class SystemTask
    {
        public Action action;
        public SystemPriority priority;
        public float enqueueTime;
    }
    
    /// <summary>
    /// 计划更新
    /// </summary>
    public class ScheduledUpdate
    {
        public Action action;
        public float interval;
        public SystemPriority priority;
        public float lastUpdateTime;
    }
    
    #endregion
}
