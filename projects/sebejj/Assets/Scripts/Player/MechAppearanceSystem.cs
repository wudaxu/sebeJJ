using UnityEngine;
using System;
using System.Collections.Generic;

namespace SebeJJ.Player
{
    /// <summary>
    /// 机甲外观系统 - BUG-023修复
    /// 管理机甲的外观定制
    /// </summary>
    public class MechAppearanceSystem : MonoBehaviour
    {
        public static MechAppearanceSystem Instance { get; private set; }
        
        [Header("外观组件")]
        [SerializeField] private Renderer[] primaryRenderers;
        [SerializeField] private Renderer[] secondaryRenderers;
        [SerializeField] private Renderer[] detailRenderers;
        [SerializeField] private Transform[] customizableParts;
        
        [Header("默认外观")]
        [SerializeField] private Color defaultPrimaryColor = Color.blue;
        [SerializeField] private Color defaultSecondaryColor = Color.gray;
        [SerializeField] private Material defaultMaterial;
        
        [Header("可用皮肤")]
        [SerializeField] private List<MechSkin> availableSkins = new List<MechSkin>();
        
        [Header("可用部件")]
        [SerializeField] private List<MechPart> availableParts = new List<MechPart>();
        
        // 当前外观数据
        private AppearanceData currentAppearance;
        
        // 事件
        public event Action OnAppearanceChanged;
        public event Action<Color> OnPrimaryColorChanged;
        public event Action<Color> OnSecondaryColorChanged;
        public event Action<string> OnSkinChanged;
        public event Action<string> OnPartEquipped;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // 初始化默认外观
            currentAppearance = new AppearanceData
            {
                primaryColor = defaultPrimaryColor,
                secondaryColor = defaultSecondaryColor,
                skinId = "default",
                equippedParts = new List<string>()
            };
        }
        
        private void Start()
        {
            // 加载保存的外观
            LoadAppearance();
            
            // 应用外观
            ApplyAppearance(currentAppearance);
        }
        
        #region 颜色设置
        
        /// <summary>
        /// 设置主色调
        /// </summary>
        public void SetPrimaryColor(Color color)
        {
            currentAppearance.primaryColor = color;
            
            foreach (var renderer in primaryRenderers)
            {
                if (renderer != null)
                {
                    renderer.material.color = color;
                }
            }
            
            OnPrimaryColorChanged?.Invoke(color);
            OnAppearanceChanged?.Invoke();
            
            SaveAppearance();
        }
        
        /// <summary>
        /// 设置副色调
        /// </summary>
        public void SetSecondaryColor(Color color)
        {
            currentAppearance.secondaryColor = color;
            
            foreach (var renderer in secondaryRenderers)
            {
                if (renderer != null)
                {
                    renderer.material.color = color;
                }
            }
            
            OnSecondaryColorChanged?.Invoke(color);
            OnAppearanceChanged?.Invoke();
            
            SaveAppearance();
        }
        
        /// <summary>
        /// 获取当前主色调
        /// </summary>
        public Color GetPrimaryColor()
        {
            return currentAppearance.primaryColor;
        }
        
        /// <summary>
        /// 获取当前副色调
        /// </summary>
        public Color GetSecondaryColor()
        {
            return currentAppearance.secondaryColor;
        }
        
        #endregion
        
        #region 皮肤系统
        
        /// <summary>
        /// 应用皮肤
        /// </summary>
        public void ApplySkin(string skinId)
        {
            var skin = availableSkins.Find(s => s.skinId == skinId);
            if (skin == null)
            {
                Debug.LogWarning($"[MechAppearance] 皮肤 {skinId} 不存在");
                return;
            }
            
            currentAppearance.skinId = skinId;
            
            // 应用皮肤材质
            if (skin.skinMaterial != null)
            {
                foreach (var renderer in primaryRenderers)
                {
                    if (renderer != null)
                    {
                        renderer.material = skin.skinMaterial;
                    }
                }
            }
            
            // 应用皮肤颜色
            if (skin.overrideColors)
            {
                SetPrimaryColor(skin.primaryColor);
                SetSecondaryColor(skin.secondaryColor);
            }
            
            OnSkinChanged?.Invoke(skinId);
            OnAppearanceChanged?.Invoke();
            
            SaveAppearance();
            
            Debug.Log($"[MechAppearance] 已应用皮肤: {skin.skinName}");
        }
        
        /// <summary>
        /// 获取可用皮肤
        /// </summary>
        public List<MechSkin> GetAvailableSkins()
        {
            return new List<MechSkin>(availableSkins);
        }
        
        /// <summary>
        /// 解锁皮肤
        /// </summary>
        public void UnlockSkin(string skinId)
        {
            var skin = availableSkins.Find(s => s.skinId == skinId);
            if (skin != null)
            {
                skin.isUnlocked = true;
                Debug.Log($"[MechAppearance] 解锁皮肤: {skin.skinName}");
            }
        }
        
        #endregion
        
        #region 部件系统
        
        /// <summary>
        /// 装备部件
        /// </summary>
        public void EquipPart(string partId)
        {
            var part = availableParts.Find(p => p.partId == partId);
            if (part == null)
            {
                Debug.LogWarning($"[MechAppearance] 部件 {partId} 不存在");
                return;
            }
            
            if (!part.isUnlocked)
            {
                Debug.LogWarning($"[MechAppearance] 部件 {partId} 未解锁");
                return;
            }
            
            // 移除同类型已装备部件
            var sameTypePart = currentAppearance.equippedParts
                .Find(id => availableParts.Find(p => p.partId == id)?.partType == part.partType);
            
            if (sameTypePart != null)
            {
                UnequipPart(sameTypePart);
            }
            
            // 装备新部件
            currentAppearance.equippedParts.Add(partId);
            
            // 实例化部件模型
            if (part.partPrefab != null && part.attachPoint != null)
            {
                var attachPoint = FindAttachPoint(part.attachPoint);
                if (attachPoint != null)
                {
                    var instance = Instantiate(part.partPrefab, attachPoint);
                    instance.name = $"Part_{partId}";
                }
            }
            
            OnPartEquipped?.Invoke(partId);
            OnAppearanceChanged?.Invoke();
            
            SaveAppearance();
            
            Debug.Log($"[MechAppearance] 已装备部件: {part.partName}");
        }
        
        /// <summary>
        /// 卸下部件
        /// </summary>
        public void UnequipPart(string partId)
        {
            if (!currentAppearance.equippedParts.Contains(partId)) return;
            
            currentAppearance.equippedParts.Remove(partId);
            
            // 销毁部件实例
            var partObj = transform.Find($"Part_{partId}");
            if (partObj != null)
            {
                Destroy(partObj.gameObject);
            }
            
            OnAppearanceChanged?.Invoke();
            SaveAppearance();
        }
        
        /// <summary>
        /// 查找附着点
        /// </summary>
        private Transform FindAttachPoint(string attachPointName)
        {
            return transform.Find(attachPointName);
        }
        
        /// <summary>
        /// 获取可用部件
        /// </summary>
        public List<MechPart> GetAvailableParts(PartType? type = null)
        {
            if (type.HasValue)
            {
                return availableParts.FindAll(p => p.partType == type.Value);
            }
            return new List<MechPart>(availableParts);
        }
        
        /// <summary>
        /// 解锁部件
        /// </summary>
        public void UnlockPart(string partId)
        {
            var part = availableParts.Find(p => p.partId == partId);
            if (part != null)
            {
                part.isUnlocked = true;
                Debug.Log($"[MechAppearance] 解锁部件: {part.partName}");
            }
        }
        
        #endregion
        
        #region 外观应用与保存
        
        /// <summary>
        /// 应用外观数据
        /// </summary>
        public void ApplyAppearance(AppearanceData data)
        {
            if (data == null) return;
            
            currentAppearance = data;
            
            // 应用颜色
            SetPrimaryColor(data.primaryColor);
            SetSecondaryColor(data.secondaryColor);
            
            // 应用皮肤
            if (!string.IsNullOrEmpty(data.skinId) && data.skinId != "default")
            {
                ApplySkin(data.skinId);
            }
            
            // 应用部件
            foreach (var partId in data.equippedParts)
            {
                EquipPart(partId);
            }
        }
        
        /// <summary>
        /// 获取当前外观数据
        /// </summary>
        public AppearanceData GetCurrentAppearance()
        {
            return currentAppearance;
        }
        
        /// <summary>
        /// 保存外观
        /// </summary>
        private void SaveAppearance()
        {
            string json = JsonUtility.ToJson(currentAppearance);
            PlayerPrefs.SetString("MechAppearance", json);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// 加载外观
        /// </summary>
        private void LoadAppearance()
        {
            string json = PlayerPrefs.GetString("MechAppearance", "");
            if (!string.IsNullOrEmpty(json))
            {
                var data = JsonUtility.FromJson<AppearanceData>(json);
                if (data != null)
                {
                    currentAppearance = data;
                }
            }
        }
        
        /// <summary>
        /// 重置为默认外观
        /// </summary>
        public void ResetToDefault()
        {
            // 卸下所有部件
            var partsToRemove = new List<string>(currentAppearance.equippedParts);
            foreach (var partId in partsToRemove)
            {
                UnequipPart(partId);
            }
            
            // 重置颜色
            SetPrimaryColor(defaultPrimaryColor);
            SetSecondaryColor(defaultSecondaryColor);
            
            // 重置皮肤
            ApplySkin("default");
        }
        
        #endregion
    }
    
    #region 数据类
    
    /// <summary>
    /// 外观数据
    /// </summary>
    [Serializable]
    public class AppearanceData
    {
        public Color primaryColor;
        public Color secondaryColor;
        public string skinId;
        public List<string> equippedParts = new List<string>();
    }
    
    /// <summary>
    /// 机甲皮肤
    /// </summary>
    [Serializable]
    public class MechSkin
    {
        public string skinId;
        public string skinName;
        public string description;
        public Material skinMaterial;
        public bool overrideColors;
        public Color primaryColor;
        public Color secondaryColor;
        public int unlockCost;
        public bool isUnlocked;
        public Sprite previewImage;
    }
    
    /// <summary>
    /// 机甲部件
    /// </summary>
    [Serializable]
    public class MechPart
    {
        public string partId;
        public string partName;
        public PartType partType;
        public string description;
        public GameObject partPrefab;
        public string attachPoint;
        public int unlockCost;
        public bool isUnlocked;
        public Sprite previewImage;
    }
    
    /// <summary>
    /// 部件类型
    /// </summary>
    public enum PartType
    {
        Head,       // 头部
        Torso,      // 躯干
        LeftArm,    // 左臂
        RightArm,   // 右臂
        Legs,       // 腿部
        Backpack,   // 背包
        Weapon      // 武器外观
    }
    
    #endregion
}
