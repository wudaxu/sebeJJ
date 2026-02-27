using System;
using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Core
{
    /// <summary>
    /// 类型安全的事件系统 (CS-002: 事件系统完善)
    /// </summary>
    public static class EventSystem
    {
        private static readonly Dictionary<Type, Delegate> events = new Dictionary<Type, Delegate>();
        private static readonly Dictionary<Type, List<EventHandlerWrapper>> handlerPriorities = new Dictionary<Type, List<EventHandlerWrapper>>();
        
        /// <summary>
        /// 订阅事件
        /// </summary>
        public static void Subscribe<T>(Action<T> handler, int priority = 0) where T : GameEvent
        {
            var type = typeof(T);
            
            if (events.ContainsKey(type))
            {
                events[type] = Delegate.Combine(events[type], handler);
            }
            else
            {
                events[type] = handler;
            }
            
            // 记录优先级
            if (!handlerPriorities.ContainsKey(type))
            {
                handlerPriorities[type] = new List<EventHandlerWrapper>();
            }
            handlerPriorities[type].Add(new EventHandlerWrapper { Handler = handler, Priority = priority });
            handlerPriorities[type].Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }
        
        /// <summary>
        /// 取消订阅事件
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : GameEvent
        {
            var type = typeof(T);
            
            if (events.ContainsKey(type))
            {
                events[type] = Delegate.Remove(events[type], handler);
                
                // 移除优先级记录
                if (handlerPriorities.TryGetValue(type, out var wrappers))
                {
                    wrappers.RemoveAll(w => w.Handler.Equals(handler));
                }
            }
        }
        
        /// <summary>
        /// 触发事件
        /// </summary>
        public static void Trigger<T>(T eventData) where T : GameEvent
        {
            var type = typeof(T);
            
            if (events.TryGetValue(type, out Delegate del))
            {
                var handlers = del.GetInvocationList();
                
                // 按优先级排序调用
                if (handlerPriorities.TryGetValue(type, out var wrappers))
                {
                    foreach (var wrapper in wrappers)
                    {
                        try
                        {
                            (wrapper.Handler as Action<T>)?.Invoke(eventData);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[EventSystem] 事件处理异常: {e.Message}");
                        }
                    }
                }
                else
                {
                    // 无优先级，直接调用
                    foreach (var handler in handlers)
                    {
                        try
                        {
                            (handler as Action<T>)?.Invoke(eventData);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[EventSystem] 事件处理异常: {e.Message}");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 清空所有事件
        /// </summary>
        public static void Clear()
        {
            events.Clear();
            handlerPriorities.Clear();
            Debug.Log("[EventSystem] 清空所有事件");
        }
        
        /// <summary>
        /// 获取事件订阅数量
        /// </summary>
        public static int GetSubscriberCount<T>() where T : GameEvent
        {
            var type = typeof(T);
            if (events.TryGetValue(type, out Delegate del))
            {
                return del.GetInvocationList().Length;
            }
            return 0;
        }
        
        private class EventHandlerWrapper
        {
            public Delegate Handler;
            public int Priority;
        }
    }
    
    /// <summary>
    /// 事件基类
    /// </summary>
    public abstract class GameEvent
    {
        public float Timestamp { get; private set; } = Time.time;
        public string EventId { get; private set; } = Guid.NewGuid().ToString("N").Substring(0, 8);
    }
    
    // 具体事件定义
    
    /// <summary>
    /// 玩家受伤事件
    /// </summary>
    public class PlayerDamagedEvent : GameEvent
    {
        public float Damage { get; set; }
        public Vector2 Position { get; set; }
        public DamageType Type { get; set; }
        public string SourceId { get; set; }
    }
    
    /// <summary>
    /// 物品收集事件
    /// </summary>
    public class ItemCollectedEvent : GameEvent
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public Vector2 Position { get; set; }
        public float Value { get; set; }
    }
    
    /// <summary>
    /// 资源变化事件
    /// </summary>
    public class ResourceChangedEvent : GameEvent
    {
        public ResourceType Type { get; set; }
        public float CurrentValue { get; set; }
        public float MaxValue { get; set; }
        public float Delta { get; set; }
    }
    
    /// <summary>
    /// 委托状态变化事件
    /// </summary>
    public class MissionStateChangedEvent : GameEvent
    {
        public string MissionId { get; set; }
        public string MissionTitle { get; set; }
        public MissionState OldState { get; set; }
        public MissionState NewState { get; set; }
        public int Reward { get; set; }
    }
    
    /// <summary>
    /// 深度变化事件
    /// </summary>
    public class DepthChangedEvent : GameEvent
    {
        public float CurrentDepth { get; set; }
        public float PreviousDepth { get; set; }
        public float MaxDepthReached { get; set; }
        public DiveZone CurrentZone { get; set; }
    }
    
    /// <summary>
    /// 游戏状态变化事件
    /// </summary>
    public class GameStateChangedEvent : GameEvent
    {
        public GameState OldState { get; set; }
        public GameState NewState { get; set; }
    }
    
    /// <summary>
    /// 敌人死亡事件
    /// </summary>
    public class EnemyDiedEvent : GameEvent
    {
        public string EnemyId { get; set; }
        public string EnemyName { get; set; }
        public Vector2 Position { get; set; }
        public int ExperienceGained { get; set; }
    }
    
    /// <summary>
    /// 存档事件
    /// </summary>
    public class GameSavedEvent : GameEvent
    {
        public string SlotName { get; set; }
        public bool IsAutoSave { get; set; }
        public float PlayTime { get; set; }
    }
    
    // 枚举定义
    public enum DamageType
    {
        Kinetic,
        Energy,
        Explosive,
        Corrosive
    }
    
    public enum ResourceType
    {
        Oxygen,
        Energy,
        Credits,
        Health
    }
    
    public enum MissionState
    {
        Available,
        Active,
        Completed,
        Failed
    }
}
