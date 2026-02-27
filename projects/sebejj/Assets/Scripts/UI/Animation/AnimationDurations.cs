namespace SebeJJ.UI.Animation
{
    /// <summary>
    /// 动画时长标准 - 定义所有UI动画的时间规范
    /// </summary>
    public static class AnimationDurations
    {
        // ==================== 即时反馈 ====================
        /// <summary>按钮点击反馈</summary>
        public const float BUTTON_CLICK = 0.1f;
        
        /// <summary>悬停状态变化</summary>
        public const float HOVER_TRANSITION = 0.15f;
        
        /// <summary>开关切换</summary>
        public const float TOGGLE_SWITCH = 0.25f;
        
        /// <summary>滑块值变化</summary>
        public const float SLIDER_CHANGE = 0.2f;
        
        // ==================== 界面过渡 ====================
        /// <summary>面板打开</summary>
        public const float PANEL_OPEN = 0.4f;
        
        /// <summary>面板关闭</summary>
        public const float PANEL_CLOSE = 0.3f;
        
        /// <summary>菜单切换</summary>
        public const float MENU_TRANSITION = 0.35f;
        
        /// <summary>弹窗显示</summary>
        public const float POPUP_SHOW = 0.3f;
        
        /// <summary>弹窗隐藏</summary>
        public const float POPUP_HIDE = 0.2f;
        
        // ==================== 列表动画 ====================
        /// <summary>列表项进入间隔</summary>
        public const float LIST_ITEM_STAGGER = 0.03f;
        
        /// <summary>列表项动画时长</summary>
        public const float LIST_ITEM_ANIMATION = 0.25f;
        
        /// <summary>列表滚动平滑时间</summary>
        public const float LIST_SCROLL_SMOOTH = 0.3f;
        
        // ==================== 战斗UI ====================
        /// <summary>伤害数字弹出</summary>
        public const float DAMAGE_NUMBER_POP = 0.15f;
        
        /// <summary>伤害数字浮动</summary>
        public const float DAMAGE_NUMBER_FLOAT = 0.8f;
        
        /// <summary>伤害数字消失</summary>
        public const float DAMAGE_NUMBER_FADE = 0.3f;
        
        /// <summary>血条值变化</summary>
        public const float HEALTHBAR_CHANGE = 0.3f;
        
        /// <summary>血条延迟填充</summary>
        public const float HEALTHBAR_DELAYED_FILL = 0.5f;
        
        /// <summary>护盾破碎裂纹</summary>
        public const float SHIELD_CRACK = 0.3f;
        
        /// <summary>护盾破碎飞散</summary>
        public const float SHIELD_SHATTER = 0.5f;
        
        /// <summary>连击计数变化</summary>
        public const float COMBO_SCALE = 0.15f;
        
        /// <summary>连击超时</summary>
        public const float COMBO_TIMEOUT = 3f;
        
        // ==================== 反馈动画 ====================
        /// <summary>资源飞行</summary>
        public const float RESOURCE_FLY = 0.8f;
        
        /// <summary>资源起始弹出</summary>
        public const float RESOURCE_POP = 0.2f;
        
        /// <summary>庆祝动画标题</summary>
        public const float CELEBRATE_TITLE = 0.5f;
        
        /// <summary>庆祝动画奖励间隔</summary>
        public const float CELEBRATE_REWARD_STAGGER = 0.15f;
        
        /// <summary>升级数字计数</summary>
        public const float LEVELUP_COUNT = 1f;
        
        /// <summary>升级光环填充</summary>
        public const float LEVELUP_RING_FILL = 1.5f;
        
        /// <summary>警告闪烁</summary>
        public const float WARNING_BLINK = 0.3f;
        
        /// <summary>严重警告闪烁</summary>
        public const float WARNING_CRITICAL_BLINK = 0.15f;
        
        /// <summary>震动时长</summary>
        public const float SHAKE_DURATION = 0.1f;
        
        // ==================== 脉冲循环 ====================
        /// <summary>选中状态脉冲</summary>
        public const float PULSE_SELECTED = 1f;
        
        /// <summary>低血量警告脉冲</summary>
        public const float PULSE_LOW_HEALTH = 0.5f;
        
        /// <summary>发光脉冲</summary>
        public const float PULSE_GLOW = 0.8f;
        
        /// <summary>呼吸效果</summary>
        public const float PULSE_BREATH = 2f;
    }
}
