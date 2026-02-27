using UnityEngine;
using UnityEngine.UI;

namespace SebeJJ.Upgrade.UI
{
    /// <summary>
    /// 升级树节点UI
    /// </summary>
    public class UpgradeNode : MonoBehaviour
    {
        [Header("UI组件")]
        public Image iconImage;
        public Image backgroundImage;
        public Image progressFill;
        public Text levelText;
        public Text nameText;
        public GameObject maxLevelBadge;
        public GameObject availableBadge;
        
        [Header("状态颜色")]
        public Color lockedColor = new Color(0.3f, 0.3f, 0.3f);
        public Color availableColor = new Color(0.2f, 0.6f, 1f);
        public Color unlockedColor = new Color(0.2f, 0.8f, 0.2f);
        public Color maxLevelColor = new Color(1f, 0.8f, 0.2f);
        
        private UpgradeNodeData nodeData;
        private System.Action<UpgradeNodeData> onSelected;
        private Button button;
        
        private void Awake()
        {
            button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnClick);
            }
        }
        
        /// <summary>
        /// 初始化节点
        /// </summary>
        public void Initialize(UpgradeNodeData data, System.Action<UpgradeNodeData> callback)
        {
            nodeData = data;
            onSelected = callback;
            
            UpdateVisuals();
        }
        
        /// <summary>
        /// 刷新节点显示
        /// </summary>
        public void Refresh()
        {
            UpdateVisuals();
        }
        
        /// <summary>
        /// 更新视觉效果
        /// </summary>
        private void UpdateVisuals()
        {
            if (nodeData == null) return;
            
            int currentLevel = 0;
            int maxLevel = 1;
            bool canUpgrade = false;
            bool isMaxLevel = false;
            
            if (nodeData.isMechaUpgrade)
            {
                var upgradeData = nodeData.upgradeData as MechaUpgradeData;
                if (upgradeData != null)
                {
                    currentLevel = UpgradeManager.Instance?.GetMechaUpgradeLevel(nodeData.mechaType) ?? 0;
                    maxLevel = upgradeData.maxLevel;
                    canUpgrade = UpgradeManager.Instance?.CanUpgradeMecha(nodeData.mechaType) ?? false;
                    isMaxLevel = currentLevel >= maxLevel;
                    
                    if (nameText != null) nameText.text = upgradeData.upgradeName;
                    if (iconImage != null) iconImage.sprite = upgradeData.icon;
                }
            }
            else
            {
                var upgradeData = nodeData.upgradeData as WeaponUpgradeEntry;
                if (upgradeData != null)
                {
                    currentLevel = UpgradeManager.Instance?.GetWeaponUpgradeLevel(nodeData.weaponId, nodeData.weaponType) ?? 0;
                    maxLevel = upgradeData.maxLevel;
                    canUpgrade = UpgradeManager.Instance?.CanUpgradeWeapon(nodeData.weaponId, nodeData.weaponType) ?? false;
                    isMaxLevel = currentLevel >= maxLevel;
                    
                    if (nameText != null) nameText.text = upgradeData.upgradeName;
                    if (iconImage != null) iconImage.sprite = upgradeData.icon;
                }
            }
            
            // 更新等级显示
            if (levelText != null)
            {
                levelText.text = isMaxLevel ? "MAX" : $"Lv.{currentLevel}/{maxLevel}";
            }
            
            // 更新进度条
            if (progressFill != null)
            {
                progressFill.fillAmount = maxLevel > 0 ? (float)currentLevel / maxLevel : 0;
            }
            
            // 更新背景颜色
            if (backgroundImage != null)
            {
                if (isMaxLevel)
                    backgroundImage.color = maxLevelColor;
                else if (canUpgrade)
                    backgroundImage.color = availableColor;
                else if (currentLevel > 0)
                    backgroundImage.color = unlockedColor;
                else
                    backgroundImage.color = lockedColor;
            }
            
            // 更新徽章
            if (maxLevelBadge != null)
                maxLevelBadge.SetActive(isMaxLevel);
            if (availableBadge != null)
                availableBadge.SetActive(canUpgrade && !isMaxLevel);
        }
        
        /// <summary>
        /// 点击事件
        /// </summary>
        private void OnClick()
        {
            onSelected?.Invoke(nodeData);
        }
        
        /// <summary>
        /// 播放升级动画
        /// </summary>
        public void PlayUpgradeAnimation()
        {
            // 缩放动画
            LeanTween.scale(gameObject, Vector3.one * 1.2f, 0.15f)
                .setEaseOutQuad()
                .setOnComplete(() =>
                {
                    LeanTween.scale(gameObject, Vector3.one, 0.15f)
                        .setEaseInQuad();
                });
            
            // 颜色闪烁
            if (backgroundImage != null)
            {
                var originalColor = backgroundImage.color;
                LeanTween.color(backgroundImage.rectTransform, Color.white, 0.1f)
                    .setOnComplete(() =>
                    {
                        LeanTween.color(backgroundImage.rectTransform, originalColor, 0.3f);
                    });
            }
        }
        
        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClick);
            }
        }
    }
}