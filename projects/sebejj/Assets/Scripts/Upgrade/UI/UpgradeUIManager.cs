using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace SebeJJ.Upgrade.UI
{
    /// <summary>
    /// 升级界面管理器
    /// 负责升级树的展示和交互
    /// </summary>
    public class UpgradeUIManager : MonoBehaviour
    {
        public static UpgradeUIManager Instance { get; private set; }
        
        [Header("界面引用")]
        public GameObject upgradePanel;
        public Transform mechaUpgradeContainer;
        public Transform weaponUpgradeContainer;
        public GameObject upgradeNodePrefab;
        public GameObject materialSlotPrefab;
        
        [Header("详情面板")]
        public GameObject detailPanel;
        public Image detailIcon;
        public Text detailName;
        public Text detailDescription;
        public Text detailCurrentValue;
        public Text detailNextValue;
        public Transform detailMaterialsContainer;
        public Button upgradeButton;
        public Text upgradeButtonText;
        
        [Header("预览面板")]
        public GameObject previewPanel;
        public UpgradePreviewUI previewUI;
        
        [Header("动画")]
        public UpgradeAnimationController animationController;
        
        // 当前选中的升级
        private UpgradeNodeData currentSelectedUpgrade;
        private bool isMechaUpgrade;
        private string currentWeaponId;
        
        // 节点缓存
        private List<UpgradeNode> mechaNodes = new List<UpgradeNode>();
        private Dictionary<string, List<UpgradeNode>> weaponNodes = new Dictionary<string, List<UpgradeNode>>();
        
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
            InitializeUpgradeUI();
            SubscribeToEvents();
        }
        
        private void SubscribeToEvents()
        {
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OnMechaUpgraded += OnMechaUpgraded;
                UpgradeManager.Instance.OnWeaponUpgraded += OnWeaponUpgraded;
                UpgradeManager.Instance.OnUpgradeFailed += OnUpgradeFailed;
            }
        }
        
        /// <summary>
        /// 初始化升级界面
        /// </summary>
        private void InitializeUpgradeUI()
        {
            CreateMechaUpgradeNodes();
            CreateWeaponUpgradeNodes();
            
            if (detailPanel != null)
                detailPanel.SetActive(false);
            if (previewPanel != null)
                previewPanel.SetActive(false);
        }
        
        /// <summary>
        /// 创建机甲升级节点
        /// </summary>
        private void CreateMechaUpgradeNodes()
        {
            if (mechaUpgradeContainer == null) return;
            
            // 清除旧节点
            foreach (Transform child in mechaUpgradeContainer)
            {
                Destroy(child.gameObject);
            }
            mechaNodes.Clear();
            
            // 创建新节点
            foreach (MechaUpgradeType type in System.Enum.GetValues(typeof(MechaUpgradeType)))
            {
                var upgradeData = UpgradeManager.Instance?.GetMechaUpgradeData(type);
                if (upgradeData == null) continue;
                
                var nodeObj = Instantiate(upgradeNodePrefab, mechaUpgradeContainer);
                var node = nodeObj.GetComponent<UpgradeNode>();
                
                if (node != null)
                {
                    node.Initialize(new UpgradeNodeData
                    {
                        isMechaUpgrade = true,
                        mechaType = type,
                        upgradeData = upgradeData
                    }, OnNodeSelected);
                    
                    mechaNodes.Add(node);
                }
            }
        }
        
        /// <summary>
        /// 创建武器升级节点
        /// </summary>
        private void CreateWeaponUpgradeNodes()
        {
            if (weaponUpgradeContainer == null) return;
            
            // 清除旧节点
            foreach (Transform child in weaponUpgradeContainer)
            {
                Destroy(child.gameObject);
            }
            weaponNodes.Clear();
            
            // 获取所有武器升级数据
            var config = UpgradeManager.Instance?.upgradeConfig;
            if (config == null) return;
            
            foreach (var weaponData in config.weaponUpgrades)
            {
                var weaponNodeList = new List<UpgradeNode>();
                
                // 为每种升级类型创建节点
                foreach (var upgrade in weaponData.upgrades)
                {
                    var nodeObj = Instantiate(upgradeNodePrefab, weaponUpgradeContainer);
                    var node = nodeObj.GetComponent<UpgradeNode>();
                    
                    if (node != null)
                    {
                        node.Initialize(new UpgradeNodeData
                        {
                            isMechaUpgrade = false,
                            weaponId = weaponData.weaponId,
                            weaponType = upgrade.upgradeType,
                            upgradeData = upgrade
                        }, OnNodeSelected);
                        
                        weaponNodeList.Add(node);
                    }
                }
                
                weaponNodes[weaponData.weaponId] = weaponNodeList;
            }
        }
        
        /// <summary>
        /// 节点选中回调
        /// </summary>
        private void OnNodeSelected(UpgradeNodeData nodeData)
        {
            currentSelectedUpgrade = nodeData;
            isMechaUpgrade = nodeData.isMechaUpgrade;
            currentWeaponId = nodeData.weaponId;
            
            ShowUpgradeDetail(nodeData);
        }
        
        /// <summary>
        /// 显示升级详情
        /// </summary>
        private void ShowUpgradeDetail(UpgradeNodeData nodeData)
        {
            if (detailPanel == null) return;
            
            detailPanel.SetActive(true);
            
            UpgradePreview preview = null;
            
            if (nodeData.isMechaUpgrade)
            {
                preview = UpgradeManager.Instance?.GetMechaUpgradePreview(nodeData.mechaType);
            }
            else
            {
                preview = UpgradeManager.Instance?.GetWeaponUpgradePreview(nodeData.weaponId, nodeData.weaponType);
            }
            
            if (preview == null)
            {
                ShowMaxLevelDetail(nodeData);
                return;
            }
            
            // 更新UI
            if (detailIcon != null) detailIcon.sprite = preview.icon;
            if (detailName != null) detailName.text = preview.upgradeName;
            if (detailDescription != null) detailDescription.text = preview.description;
            if (detailCurrentValue != null) 
                detailCurrentValue.text = $"当前: {preview.currentValue:F1}";
            if (detailNextValue != null) 
                detailNextValue.text = $"升级后: {preview.nextValue:F1} (+{preview.ValueIncreasePercent:F0}%)";
            
            // 显示材料需求
            UpdateMaterialRequirements(preview.requirements);
            
            // 更新升级按钮
            UpdateUpgradeButton(preview);
        }
        
        /// <summary>
        /// 显示满级详情
        /// </summary>
        private void ShowMaxLevelDetail(UpgradeNodeData nodeData)
        {
            if (detailName != null) detailName.text = nodeData.upgradeName;
            if (detailDescription != null) detailDescription.text = "已达到最高等级";
            if (detailCurrentValue != null) detailCurrentValue.text = "MAX";
            if (detailNextValue != null) detailNextValue.text = "-";
            
            // 清空材料显示
            foreach (Transform child in detailMaterialsContainer)
            {
                Destroy(child.gameObject);
            }
            
            // 禁用升级按钮
            if (upgradeButton != null)
            {
                upgradeButton.interactable = false;
                if (upgradeButtonText != null) upgradeButtonText.text = "已满级";
            }
        }
        
        /// <summary>
        /// 更新材料需求显示
        /// </summary>
        private void UpdateMaterialRequirements(List<MaterialRequirement> requirements)
        {
            if (detailMaterialsContainer == null) return;
            
            // 清除旧条目
            foreach (Transform child in detailMaterialsContainer)
            {
                Destroy(child.gameObject);
            }
            
            if (requirements == null || requirements.Count == 0)
            {
                var emptyText = new GameObject("EmptyText").AddComponent<Text>();
                emptyText.text = "无需材料";
                emptyText.transform.SetParent(detailMaterialsContainer);
                return;
            }
            
            // 创建材料槽
            foreach (var req in requirements)
            {
                var slotObj = Instantiate(materialSlotPrefab, detailMaterialsContainer);
                var slot = slotObj.GetComponent<MaterialSlotUI>();
                
                if (slot != null)
                {
                    bool hasEnough = MaterialManager.Instance?.HasMaterial(req.materialId, req.amount) ?? false;
                    slot.Initialize(req, hasEnough);
                }
            }
        }
        
        /// <summary>
        /// 更新升级按钮状态
        /// </summary>
        private void UpdateUpgradeButton(UpgradePreview preview)
        {
            if (upgradeButton == null) return;
            
            bool canUpgrade = false;
            
            if (isMechaUpgrade)
            {
                canUpgrade = UpgradeManager.Instance?.CanUpgradeMecha(currentSelectedUpgrade.mechaType) ?? false;
            }
            else
            {
                canUpgrade = UpgradeManager.Instance?.CanUpgradeWeapon(currentWeaponId, currentSelectedUpgrade.weaponType) ?? false;
            }
            
            upgradeButton.interactable = canUpgrade;
            
            if (upgradeButtonText != null)
            {
                upgradeButtonText.text = canUpgrade ? "升级" : "材料不足";
            }
            
            // 绑定点击事件
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        }
        
        /// <summary>
        /// 升级按钮点击
        /// </summary>
        private void OnUpgradeButtonClicked()
        {
            if (currentSelectedUpgrade == null) return;
            
            bool success = false;
            
            if (isMechaUpgrade)
            {
                success = UpgradeManager.Instance?.UpgradeMecha(currentSelectedUpgrade.mechaType) ?? false;
            }
            else
            {
                success = UpgradeManager.Instance?.UpgradeWeapon(currentWeaponId, currentSelectedUpgrade.weaponType) ?? false;
            }
            
            if (success)
            {
                // 播放升级动画
                animationController?.PlayUpgradeAnimation(currentSelectedUpgrade);
                
                // 刷新UI
                RefreshUI();
            }
        }
        
        /// <summary>
        /// 升级成功回调
        /// </summary>
        private void OnMechaUpgraded(MechaUpgradeType type, int newLevel)
        {
            RefreshUI();
            
            // 显示升级预览
            var preview = UpgradeManager.Instance?.GetMechaUpgradePreview(type);
            if (preview != null)
            {
                ShowUpgradeSuccess(preview);
            }
        }
        
        /// <summary>
        /// 武器升级成功回调
        /// </summary>
        private void OnWeaponUpgraded(string weaponId, WeaponUpgradeType type, int newLevel)
        {
            RefreshUI();
            
            var preview = UpgradeManager.Instance?.GetWeaponUpgradePreview(weaponId, type);
            if (preview != null)
            {
                ShowUpgradeSuccess(preview);
            }
        }
        
        /// <summary>
        /// 升级失败回调
        /// </summary>
        private void OnUpgradeFailed()
        {
            animationController?.PlayUpgradeFailAnimation();
        }
        
        /// <summary>
        /// 显示升级成功
        /// </summary>
        private void ShowUpgradeSuccess(UpgradePreview preview)
        {
            if (previewUI != null)
            {
                previewUI.Show(preview);
            }
        }
        
        /// <summary>
        /// 刷新UI
        /// </summary>
        private void RefreshUI()
        {
            // 刷新所有节点
            foreach (var node in mechaNodes)
            {
                node.Refresh();
            }
            
            foreach (var kvp in weaponNodes)
            {
                foreach (var node in kvp.Value)
                {
                    node.Refresh();
                }
            }
            
            // 刷新详情面板
            if (currentSelectedUpgrade != null)
            {
                ShowUpgradeDetail(currentSelectedUpgrade);
            }
        }
        
        #region 界面控制
        
        /// <summary>
        /// 打开升级界面
        /// </summary>
        public void OpenUpgradePanel()
        {
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(true);
                RefreshUI();
            }
        }
        
        /// <summary>
        /// 关闭升级界面
        /// </summary>
        public void CloseUpgradePanel()
        {
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
            }
        }
        
        /// <summary>
        /// 切换升级界面
        /// </summary>
        public void ToggleUpgradePanel()
        {
            if (upgradePanel != null)
            {
                if (upgradePanel.activeSelf)
                    CloseUpgradePanel();
                else
                    OpenUpgradePanel();
            }
        }
        
        #endregion
        
        private void OnDestroy()
        {
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OnMechaUpgraded -= OnMechaUpgraded;
                UpgradeManager.Instance.OnWeaponUpgraded -= OnWeaponUpgraded;
                UpgradeManager.Instance.OnUpgradeFailed -= OnUpgradeFailed;
            }
            
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
    
    /// <summary>
    /// 升级节点数据
    /// </summary>
    public class UpgradeNodeData
    {
        public bool isMechaUpgrade;
        public MechaUpgradeType mechaType;
        public string weaponId;
        public WeaponUpgradeType weaponType;
        public string upgradeName;
        public object upgradeData;
    }
}