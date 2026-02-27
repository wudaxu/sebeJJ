using System;
using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Core
{
    /// <summary>
    /// 服务定位器 - 全局服务管理 (CS-001: 系统解耦)
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();
        private static readonly Dictionary<Type, Func<object>> serviceFactories = new Dictionary<Type, Func<object>>();
        
        /// <summary>
        /// 注册服务实例
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            services[type] = service;
            Debug.Log($"[ServiceLocator] 注册服务: {type.Name}");
        }
        
        /// <summary>
        /// 注册服务工厂（延迟创建）
        /// </summary>
        public static void RegisterFactory<T>(Func<T> factory) where T : class
        {
            var type = typeof(T);
            serviceFactories[type] = () => factory();
            Debug.Log($"[ServiceLocator] 注册服务工厂: {type.Name}");
        }
        
        /// <summary>
        /// 获取服务
        /// </summary>
        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            
            // 先查找已注册的服务
            if (services.TryGetValue(type, out object service))
            {
                return service as T;
            }
            
            // 尝试使用工厂创建
            if (serviceFactories.TryGetValue(type, out Func<object> factory))
            {
                var newService = factory() as T;
                services[type] = newService; // 缓存创建的服务
                return newService;
            }
            
            Debug.LogWarning($"[ServiceLocator] 未找到服务: {type.Name}");
            return null;
        }
        
        /// <summary>
        /// 检查服务是否已注册
        /// </summary>
        public static bool Has<T>() where T : class
        {
            var type = typeof(T);
            return services.ContainsKey(type) || serviceFactories.ContainsKey(type);
        }
        
        /// <summary>
        /// 注销服务
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            services.Remove(type);
            serviceFactories.Remove(type);
            Debug.Log($"[ServiceLocator] 注销服务: {type.Name}");
        }
        
        /// <summary>
        /// 清空所有服务
        /// </summary>
        public static void Clear()
        {
            services.Clear();
            serviceFactories.Clear();
            Debug.Log("[ServiceLocator] 清空所有服务");
        }
    }
    
    // 服务接口定义
    public interface IResourceService
    {
        float CurrentOxygen { get; }
        float MaxOxygen { get; }
        float CurrentEnergy { get; }
        float MaxEnergy { get; }
        int Credits { get; }
        bool ConsumeOxygen(float amount);
        bool ConsumeEnergy(float amount);
        void AddCredits(int amount);
    }
    
    public interface IMissionService
    {
        int GetActiveMissionCount();
        bool AcceptMission(string missionId);
        void UpdateMissionProgress(string targetId, int amount);
    }
    
    public interface ISaveService
    {
        bool SaveGame(string slotName);
        bool LoadGame(string slotName);
        bool SaveExists(string slotName);
        SaveInfo[] GetAllSaves();
    }
    
    public interface IUIService
    {
        void ShowNotification(string message, float duration = 2f);
        void UpdateOxygenBar(float current, float max);
        void UpdateEnergyBar(float current, float max);
        void UpdateCollectProgress(float progress);
    }
}
