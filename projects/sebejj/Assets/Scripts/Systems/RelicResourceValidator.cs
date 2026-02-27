using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SebeJJ.Systems
{
    /// <summary>
    /// 遗物资源验证器 - BUG-002修复
    /// 检查并修复遗物资源引用问题
    /// </summary>
    public class RelicResourceValidator : MonoBehaviour
    {
        public static RelicResourceValidator Instance { get; private set; }
        
        [Header("默认资源")]
        [SerializeField] private GameObject defaultRelicModel;
        [SerializeField] private Material defaultRelicMaterial;
        [SerializeField] private Sprite defaultRelicIcon;
        
        [Header("验证设置")]
        [SerializeField] private bool validateOnStart = true;
        [SerializeField] private bool autoFixMissingResources = true;
        [SerializeField] private bool logValidationResults = true;
        
        // 资源缓存
        private Dictionary<string, GameObject> relicModelCache = new Dictionary<string, GameObject>();
        private Dictionary<string, Material> relicMaterialCache = new Dictionary<string, Material>();
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            if (validateOnStart)
            {
                ValidateAllRelicResources();
            }
        }
        
        /// <summary>
        /// 验证所有遗物资源
        /// </summary>
        public void ValidateAllRelicResources()
        {
            Debug.Log("[RelicValidator] 开始验证遗物资源...");
            
            int totalRelics = 0;
            int fixedRelics = 0;
            int missingModels = 0;
            int missingMaterials = 0;
            int missingIcons = 0;
            
            // 查找所有遗物对象
            var relics = FindObjectsOfType<RelicObject>();
            totalRelics = relics.Length;
            
            foreach (var relic in relics)
            {
                bool needsFix = false;
                
                // 检查模型
                if (relic.model == null)
                {
                    missingModels++;
                    needsFix = true;
                    
                    if (autoFixMissingResources)
                    {
                        relic.model = LoadDefaultRelicModel();
                        Debug.LogWarning($"[RelicValidator] 遗物 '{relic.name}' 模型缺失，已使用默认模型替换");
                    }
                }
                
                // 检查材质
                if (relic.material == null)
                {
                    missingMaterials++;
                    needsFix = true;
                    
                    if (autoFixMissingResources)
                    {
                        relic.material = defaultRelicMaterial;
                        Debug.LogWarning($"[RelicValidator] 遗物 '{relic.name}' 材质缺失，已使用默认材质替换");
                    }
                }
                
                // 检查图标
                if (relic.icon == null)
                {
                    missingIcons++;
                    needsFix = true;
                    
                    if (autoFixMissingResources)
                    {
                        relic.icon = defaultRelicIcon;
                        Debug.LogWarning($"[RelicValidator] 遗物 '{relic.name}' 图标缺失，已使用默认图标替换");
                    }
                }
                
                if (needsFix)
                {
                    fixedRelics++;
                }
            }
            
            // 验证任务中的遗物引用
            ValidateMissionRelicReferences();
            
            if (logValidationResults)
            {
                Debug.Log($"[RelicValidator] 验证完成: 总计 {totalRelics} 个遗物, " +
                         $"修复 {fixedRelics} 个, 缺失模型 {missingModels}, " +
                         $"缺失材质 {missingMaterials}, 缺失图标 {missingIcons}");
            }
        }
        
        /// <summary>
        /// 验证任务中的遗物引用
        /// </summary>
        private void ValidateMissionRelicReferences()
        {
            // 检查Q013任务
            var q013Script = FindObjectOfType<Q013_Script>();
            if (q013Script != null)
            {
                if (q013Script.relicObject == null)
                {
                    Debug.LogError("[RelicValidator] Q013任务遗物引用缺失!");
                    
                    // 尝试在场景中查找
                    var foundRelic = FindObjectOfType<RelicObject>();
                    if (foundRelic != null)
                    {
                        q013Script.relicObject = foundRelic;
                        Debug.Log("[RelicValidator] 已自动修复Q013遗物引用");
                    }
                }
            }
        }
        
        /// <summary>
        /// 加载默认遗物模型
        /// </summary>
        public GameObject LoadDefaultRelicModel()
        {
            if (defaultRelicModel != null)
            {
                return defaultRelicModel;
            }
            
            // 尝试从Resources加载
            var model = Resources.Load<GameObject>("Models/Relics/DefaultRelic");
            if (model != null)
            {
                return model;
            }
            
            // 创建基本模型
            return CreateBasicRelicModel();
        }
        
        /// <summary>
        /// 创建基本遗物模型
        /// </summary>
        private GameObject CreateBasicRelicModel()
        {
            GameObject basicModel = new GameObject("BasicRelicModel");
            
            // 添加基本几何体
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(basicModel.transform);
            cube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            
            // 添加发光效果
            var light = basicModel.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = Color.cyan;
            light.intensity = 1f;
            light.range = 3f;
            
            // 添加旋转动画
            var rotator = basicModel.AddComponent<RelicRotator>();
            
            return basicModel;
        }
        
        /// <summary>
        /// 获取遗物模型
        /// </summary>
        public GameObject GetRelicModel(string relicId)
        {
            if (relicModelCache.TryGetValue(relicId, out var model))
            {
                return model;
            }
            
            // 尝试从Resources加载
            model = Resources.Load<GameObject>($"Models/Relics/{relicId}");
            if (model != null)
            {
                relicModelCache[relicId] = model;
                return model;
            }
            
            // 返回默认模型
            return LoadDefaultRelicModel();
        }
        
        /// <summary>
        /// 注册遗物模型
        /// </summary>
        public void RegisterRelicModel(string relicId, GameObject model)
        {
            if (!string.IsNullOrEmpty(relicId) && model != null)
            {
                relicModelCache[relicId] = model;
            }
        }
        
        /// <summary>
        /// 检查遗物资源是否完整
        /// </summary>
        public bool IsRelicResourceValid(RelicObject relic)
        {
            if (relic == null) return false;
            
            return relic.model != null && 
                   relic.material != null && 
                   relic.icon != null;
        }
        
        /// <summary>
        /// 修复特定遗物资源
        /// </summary>
        public void FixRelicResource(RelicObject relic)
        {
            if (relic == null) return;
            
            if (relic.model == null)
            {
                relic.model = LoadDefaultRelicModel();
            }
            
            if (relic.material == null)
            {
                relic.material = defaultRelicMaterial;
            }
            
            if (relic.icon == null)
            {
                relic.icon = defaultRelicIcon;
            }
            
            Debug.Log($"[RelicValidator] 已修复遗物 '{relic.name}' 的资源");
        }
    }
    
    /// <summary>
    /// 遗物对象组件
    /// </summary>
    public class RelicObject : MonoBehaviour
    {
        public string relicId;
        public string relicName;
        [TextArea(2, 4)]
        public string description;
        public int value = 1000;
        public RelicRarity rarity = RelicRarity.Common;
        
        [Header("资源引用")]
        public GameObject model;
        public Material material;
        public Sprite icon;
        
        [Header("效果")]
        public ParticleSystem collectionEffect;
        public AudioClip collectionSound;
        
        private void Start()
        {
            // 自动验证资源
            if (model == null || material == null)
            {
                RelicResourceValidator.Instance?.FixRelicResource(this);
            }
            
            ApplyResources();
        }
        
        private void ApplyResources()
        {
            if (model != null)
            {
                // 实例化模型
                var instance = Instantiate(model, transform);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
            }
            
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.material = material;
            }
        }
    }
    
    /// <summary>
    /// 遗物旋转动画
    /// </summary>
    public class RelicRotator : MonoBehaviour
    {
        public float rotationSpeed = 30f;
        public float floatAmplitude = 0.1f;
        public float floatSpeed = 2f;
        
        private Vector3 startPosition;
        
        private void Start()
        {
            startPosition = transform.position;
        }
        
        private void Update()
        {
            // 旋转
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            
            // 浮动
            float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = startPosition + new Vector3(0, yOffset, 0);
        }
    }
    
    public enum RelicRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}
