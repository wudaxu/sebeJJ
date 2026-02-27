using UnityEngine;
using UnityEngine.UI;

namespace SebeJJ.Upgrade.UI
{
    /// <summary>
    /// 材料槽UI
    /// </summary>
    public class MaterialSlotUI : MonoBehaviour
    {
        [Header("UI组件")]
        public Image iconImage;
        public Text nameText;
        public Text amountText;
        public Image backgroundImage;
        public GameObject insufficientIndicator;
        
        [Header("颜色")]
        public Color sufficientColor = Color.white;
        public Color insufficientColor = new Color(1f, 0.3f, 0.3f);
        
        private string materialId;
        private int requiredAmount;
        
        /// <summary>
        /// 初始化材料槽
        /// </summary>
        public void Initialize(MaterialRequirement requirement, bool hasEnough)
        {
            materialId = requirement.materialId;
            requiredAmount = requirement.amount;
            
            var materialInfo = MaterialManager.Instance?.GetMaterialInfo(materialId);
            int currentAmount = MaterialManager.Instance?.GetMaterialCount(materialId) ?? 0;
            
            // 设置图标
            if (iconImage != null)
            {
                iconImage.sprite = materialInfo?.icon;
                if (materialInfo != null)
                {
                    iconImage.color = materialInfo.GetRarityColor();
                }
            }
            
            // 设置名称
            if (nameText != null)
            {
                nameText.text = materialInfo?.materialName ?? materialId;
            }
            
            // 设置数量
            if (amountText != null)
            {
                string colorTag = hasEnough ? "#00FF00" : "#FF0000";
                amountText.text = $"{currentAmount}/<color={colorTag}>{requiredAmount}</color>";
            }
            
            // 更新背景颜色
            if (backgroundImage != null)
            {
                backgroundImage.color = hasEnough ? sufficientColor : insufficientColor;
            }
            
            // 显示/隐藏不足指示器
            if (insufficientIndicator != null)
            {
                insufficientIndicator.SetActive(!hasEnough);
            }
        }
        
        /// <summary>
        /// 刷新显示
        /// </summary>
        public void Refresh()
        {
            int currentAmount = MaterialManager.Instance?.GetMaterialCount(materialId) ?? 0;
            bool hasEnough = currentAmount >= requiredAmount;
            
            if (amountText != null)
            {
                string colorTag = hasEnough ? "#00FF00" : "#FF0000";
                amountText.text = $"{currentAmount}/<color={colorTag}>{requiredAmount}</color>";
            }
            
            if (backgroundImage != null)
            {
                backgroundImage.color = hasEnough ? sufficientColor : insufficientColor;
            }
            
            if (insufficientIndicator != null)
            {
                insufficientIndicator.SetActive(!hasEnough);
            }
        }
    }
}