using UnityEngine;

namespace SebeJJ.UI.Animation
{
    /// <summary>
    /// 颜色变化规则 - 定义UI动画中的颜色规范
    /// </summary>
    public static class ColorConstants
    {
        // ==================== 主题色 ====================
        /// <summary>主色调 - 赛博青</summary>
        public static readonly Color PRIMARY = new Color(0.2f, 0.9f, 1f, 1f);
        
        /// <summary>次要色 - 霓虹紫</summary>
        public static readonly Color SECONDARY = new Color(0.8f, 0.2f, 0.9f, 1f);
        
        /// <summary>强调色 - 能量橙</summary>
        public static readonly Color ACCENT = new Color(1f, 0.6f, 0.1f, 1f);
        
        /// <summary>背景色 - 深空黑</summary>
        public static readonly Color BACKGROUND = new Color(0.05f, 0.05f, 0.08f, 1f);
        
        /// <summary>面板色 - 半透明黑</summary>
        public static readonly Color PANEL = new Color(0.1f, 0.1f, 0.15f, 0.9f);
        
        // ==================== 状态色 ====================
        /// <summary>正常/可用</summary>
        public static readonly Color STATE_NORMAL = new Color(0.2f, 0.9f, 0.3f, 1f);
        
        /// <summary>警告/注意</summary>
        public static readonly Color STATE_WARNING = new Color(0.95f, 0.8f, 0.1f, 1f);
        
        /// <summary>危险/错误</summary>
        public static readonly Color STATE_DANGER = new Color(0.95f, 0.2f, 0.1f, 1f);
        
        /// <summary>信息/提示</summary>
        public static readonly Color STATE_INFO = new Color(0.2f, 0.6f, 1f, 1f);
        
        /// <summary>成功/完成</summary>
        public static readonly Color STATE_SUCCESS = new Color(0.2f, 1f, 0.4f, 1f);
        
        /// <summary>禁用/锁定</summary>
        public static readonly Color STATE_DISABLED = new Color(0.4f, 0.4f, 0.4f, 0.7f);
        
        // ==================== 血条颜色 ====================
        /// <summary>高血量 - 绿色</summary>
        public static readonly Color HEALTH_HIGH = new Color(0.2f, 0.9f, 0.3f, 1f);
        
        /// <summary>中血量 - 黄色</summary>
        public static readonly Color HEALTH_MEDIUM = new Color(0.95f, 0.8f, 0.1f, 1f);
        
        /// <summary>低血量 - 红色</summary>
        public static readonly Color HEALTH_LOW = new Color(0.95f, 0.2f, 0.1f, 1f);
        
        /// <summary>治疗 - 亮绿</summary>
        public static readonly Color HEALTH_HEAL = new Color(0.2f, 1f, 0.4f, 0.4f);
        
        /// <summary>受伤 - 暗红</summary>
        public static readonly Color HEALTH_DAMAGE = new Color(1f, 0.2f, 0.2f, 0.5f);
        
        // ==================== 护盾颜色 ====================
        /// <summary>护盾正常 - 青色</summary>
        public static readonly Color SHIELD_NORMAL = new Color(0.3f, 0.7f, 1f, 1f);
        
        /// <summary>护盾受损 - 橙色</summary>
        public static readonly Color SHIELD_DAMAGED = new Color(1f, 0.5f, 0.2f, 1f);
        
        /// <summary>护盾破碎 - 裂纹色</summary>
        public static readonly Color SHIELD_CRACK = new Color(0.8f, 0.9f, 1f, 0.8f);
        
        // ==================== 伤害数字颜色 ====================
        /// <summary>普通伤害 - 米白</summary>
        public static readonly Color DAMAGE_NORMAL = new Color(1f, 0.9f, 0.7f, 1f);
        
        /// <summary>暴击伤害 - 橙红</summary>
        public static readonly Color DAMAGE_CRITICAL = new Color(1f, 0.3f, 0.1f, 1f);
        
        /// <summary>护盾伤害 - 青色</summary>
        public static readonly Color DAMAGE_SHIELD = new Color(0.3f, 0.7f, 1f, 1f);
        
        /// <summary>治疗 - 绿色</summary>
        public static readonly Color DAMAGE_HEAL = new Color(0.2f, 1f, 0.4f, 1f);
        
        // ==================== 连击颜色 ====================
        /// <summary>连击 0-5 - 白色</summary>
        public static readonly Color COMBO_0 = new Color(1f, 1f, 1f, 1f);
        
        /// <summary>连击 6-10 - 黄色</summary>
        public static readonly Color COMBO_1 = new Color(1f, 0.9f, 0.3f, 1f);
        
        /// <summary>连击 11-20 - 橙色</summary>
        public static readonly Color COMBO_2 = new Color(1f, 0.5f, 0.1f, 1f);
        
        /// <summary>连击 21-30 - 红色</summary>
        public static readonly Color COMBO_3 = new Color(1f, 0.2f, 0.2f, 1f);
        
        /// <summary>连击 31-50 - 紫色</summary>
        public static readonly Color COMBO_4 = new Color(0.8f, 0.2f, 0.8f, 1f);
        
        /// <summary>连击 50+ - 青色</summary>
        public static readonly Color COMBO_5 = new Color(0.2f, 0.8f, 1f, 1f);
        
        /// <summary>连击颜色数组</summary>
        public static readonly Color[] COMBO_COLORS = new Color[]
        {
            COMBO_0, COMBO_1, COMBO_2, COMBO_3, COMBO_4, COMBO_5
        };
        
        // ==================== 稀有度颜色 ====================
        /// <summary>普通 - 灰</summary>
        public static readonly Color RARITY_COMMON = new Color(0.7f, 0.7f, 0.7f, 1f);
        
        /// <summary>稀有 - 绿</summary>
        public static readonly Color RARITY_UNCOMMON = new Color(0.2f, 0.8f, 0.2f, 1f);
        
        /// <summary>史诗 - 蓝</summary>
        public static readonly Color RARITY_RARE = new Color(0.2f, 0.4f, 1f, 1f);
        
        /// <summary>传说 - 紫</summary>
        public static readonly Color RARITY_EPIC = new Color(0.8f, 0.2f, 0.8f, 1f);
        
        /// <summary>神话 - 橙</summary>
        public static readonly Color RARITY_LEGENDARY = new Color(1f, 0.6f, 0.1f, 1f);
        
        /// <summary>稀有度颜色数组</summary>
        public static readonly Color[] RARITY_COLORS = new Color[]
        {
            RARITY_COMMON, RARITY_UNCOMMON, RARITY_RARE, RARITY_EPIC, RARITY_LEGENDARY
        };
        
        // ==================== 任务状态颜色 ====================
        /// <summary>可接取 - 绿色</summary>
        public static readonly Color QUEST_AVAILABLE = new Color(0.2f, 0.8f, 0.3f, 0.2f);
        
        /// <summary>进行中 - 黄色</summary>
        public static readonly Color QUEST_IN_PROGRESS = new Color(0.9f, 0.7f, 0.2f, 0.2f);
        
        /// <summary>已完成 - 蓝色</summary>
        public static readonly Color QUEST_COMPLETED = new Color(0.2f, 0.6f, 1f, 0.2f);
        
        /// <summary>锁定 - 灰色</summary>
        public static readonly Color QUEST_LOCKED = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        
        /// <summary>紧急 - 红色</summary>
        public static readonly Color QUEST_URGENT = new Color(0.9f, 0.2f, 0.2f, 0.3f);
        
        // ==================== UI交互颜色 ====================
        /// <summary>悬停高亮</summary>
        public static readonly Color UI_HOVER = new Color(0.3f, 0.9f, 1f, 0.5f);
        
        /// <summary>选中高亮</summary>
        public static readonly Color UI_SELECTED = new Color(0f, 1f, 0.8f, 1f);
        
        /// <summary>按下状态</summary>
        public static readonly Color UI_PRESSED = new Color(0.1f, 0.7f, 0.9f, 1f);
        
        /// <summary>发光效果</summary>
        public static readonly Color UI_GLOW = new Color(0.2f, 0.9f, 1f, 0.6f);
        
        /// <summary>边框高亮</summary>
        public static readonly Color UI_BORDER_HIGHLIGHT = new Color(0f, 1f, 0.8f, 1f);
        
        // ==================== 发光强度 ====================
        /// <summary>正常发光</summary>
        public const float GLOW_NORMAL = 0.5f;
        
        /// <summary>悬停发光</summary>
        public const float GLOW_HOVER = 0.8f;
        
        /// <summary>选中发光</summary>
        public const float GLOW_SELECTED = 1f;
        
        /// <summary>警告发光</summary>
        public const float GLOW_WARNING = 1.2f;
        
        /// <summary>最大发光</summary>
        public const float GLOW_MAX = 1.5f;
    }
    
    /// <summary>
    /// 颜色工具扩展
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// 获取连击对应颜色
        /// </summary>
        public static Color GetComboColor(int combo)
        {
            if (combo < 6) return ColorConstants.COMBO_0;
            if (combo < 11) return ColorConstants.COMBO_1;
            if (combo < 21) return ColorConstants.COMBO_2;
            if (combo < 31) return ColorConstants.COMBO_3;
            if (combo < 51) return ColorConstants.COMBO_4;
            return ColorConstants.COMBO_5;
        }
        
        /// <summary>
        /// 获取稀有度对应颜色
        /// </summary>
        public static Color GetRarityColor(int rarity)
        {
            rarity = Mathf.Clamp(rarity, 0, ColorConstants.RARITY_COLORS.Length - 1);
            return ColorConstants.RARITY_COLORS[rarity];
        }
        
        /// <summary>
        /// 根据血量比例获取颜色
        /// </summary>
        public static Color GetHealthColor(float healthPercent)
        {
            if (healthPercent > 0.6f) return ColorConstants.HEALTH_HIGH;
            if (healthPercent > 0.3f) return ColorConstants.HEALTH_MEDIUM;
            return ColorConstants.HEALTH_LOW;
        }
        
        /// <summary>
        /// 设置颜色Alpha
        /// </summary>
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
        
        /// <summary>
        /// 混合两种颜色
        /// </summary>
        public static Color Blend(this Color color1, Color color2, float t)
        {
            return Color.Lerp(color1, color2, t);
        }
        
        /// <summary>
        /// 增加亮度
        /// </summary>
        public static Color Brighten(this Color color, float amount)
        {
            return new Color(
                Mathf.Min(1f, color.r + amount),
                Mathf.Min(1f, color.g + amount),
                Mathf.Min(1f, color.b + amount),
                color.a
            );
        }
        
        /// <summary>
        /// 降低亮度
        /// </summary>
        public static Color Darken(this Color color, float amount)
        {
            return new Color(
                Mathf.Max(0f, color.r - amount),
                Mathf.Max(0f, color.g - amount),
                Mathf.Max(0f, color.b - amount),
                color.a
            );
        }
    }
}
