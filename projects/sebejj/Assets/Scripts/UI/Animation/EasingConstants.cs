using DG.Tweening;
using UnityEngine;

namespace SebeJJ.UI.Animation
{
    /// <summary>
    /// 缓动函数规范 - 定义UI动画的缓动标准
    /// </summary>
    public static class EasingConstants
    {
        // ==================== 进入动画 ====================
        /// <summary>弹入效果 - 用于面板、弹窗打开</summary>
        public static readonly Ease EASE_IN_POP = Ease.OutBack;
        
        /// <summary>滑入效果 - 用于侧边栏、列表项</summary>
        public static readonly Ease EASE_IN_SLIDE = Ease.OutQuad;
        
        /// <summary>淡入效果 - 用于背景、提示信息</summary>
        public static readonly Ease EASE_IN_FADE = Ease.OutQuad;
        
        /// <summary>缩放进入 - 用于小元素、图标</summary>
        public static readonly Ease EASE_IN_SCALE = Ease.OutBack;
        
        // ==================== 退出动画 ====================
        /// <summary>弹出消失 - 用于面板、弹窗关闭</summary>
        public static readonly Ease EASE_OUT_POP = Ease.InBack;
        
        /// <summary>滑出消失 - 用于侧边栏、列表项</summary>
        public static readonly Ease EASE_OUT_SLIDE = Ease.InQuad;
        
        /// <summary>淡出消失 - 用于背景、提示信息</summary>
        public static readonly Ease EASE_OUT_FADE = Ease.InQuad;
        
        /// <summary>缩放消失 - 用于小元素、图标</summary>
        public static readonly Ease EASE_OUT_SCALE = Ease.InBack;
        
        // ==================== 交互反馈 ====================
        /// <summary>按钮按下 - 快速收缩</summary>
        public static readonly Ease EASE_BUTTON_PRESS = Ease.OutQuad;
        
        /// <summary>按钮释放 - 弹性恢复</summary>
        public static readonly Ease EASE_BUTTON_RELEASE = Ease.OutBack;
        
        /// <summary>悬停进入 - 平滑放大</summary>
        public static readonly Ease EASE_HOVER_IN = Ease.OutBack;
        
        /// <summary>悬停退出 - 平滑恢复</summary>
        public static readonly Ease EASE_HOVER_OUT = Ease.OutQuad;
        
        /// <summary>选中状态 - 弹性效果</summary>
        public static readonly Ease EASE_SELECT = Ease.OutBack;
        
        /// <summary>开关切换 - 带过冲</summary>
        public static readonly Ease EASE_TOGGLE = Ease.OutBack;
        
        // ==================== 数值变化 ====================
        /// <summary>血条变化 - 平滑过渡</summary>
        public static readonly Ease EASE_HEALTH_CHANGE = Ease.OutQuad;
        
        /// <summary>数值计数 - 线性增长</summary>
        public static readonly Ease EASE_NUMBER_COUNT = Ease.Linear;
        
        /// <summary>滑块拖动 - 即时响应</summary>
        public static readonly Ease EASE_SLIDER_DRAG = Ease.OutQuad;
        
        // ==================== 特殊效果 ====================
        /// <summary>震动效果 - 弹性衰减</summary>
        public static readonly Ease EASE_SHAKE = Ease.OutElastic;
        
        /// <summary>脉冲循环 - 正弦波</summary>
        public static readonly Ease EASE_PULSE = Ease.InOutSine;
        
        /// <summary>弹跳效果 - 弹性</summary>
        public static readonly Ease EASE_BOUNCE = Ease.OutBounce;
        
        /// <summary>弹性过冲 - 强调效果</summary>
        public static readonly Ease EASE_OVERSHOOT = Ease.OutBack;
        
        // ==================== 战斗动画 ====================
        /// <summary>伤害数字弹出 - 快速过冲</summary>
        public static readonly Ease EASE_DAMAGE_POP = Ease.OutBack;
        
        /// <summary>伤害数字浮动 - 减速</summary>
        public static readonly Ease EASE_DAMAGE_FLOAT = Ease.OutQuad;
        
        /// <summary>伤害数字消失 - 加速</summary>
        public static readonly Ease EASE_DAMAGE_FADE = Ease.InQuad;
        
        /// <summary>连击缩放 - 弹性</summary>
        public static readonly Ease EASE_COMBO_SCALE = Ease.OutBack;
        
        /// <summary>护盾破碎 - 加速</summary>
        public static readonly Ease EASE_SHIELD_BREAK = Ease.InBack;
        
        // ==================== 过冲参数 ====================
        /// <summary>默认过冲幅度</summary>
        public const float OVERSHOOT_DEFAULT = 1.1f;
        
        /// <summary>强调过冲幅度</summary>
        public const float OVERSHOOT_EMPHASIS = 1.3f;
        
        /// <summary>轻微过冲幅度</summary>
        public const float OVERSHOOT_SUBTLE = 1.05f;
        
        // ==================== 弹性参数 ====================
        /// <summary>默认弹性</summary>
        public const float ELASTICITY_DEFAULT = 1f;
        
        /// <summary>高弹性</summary>
        public const float ELASTICITY_HIGH = 1.5f;
        
        /// <summary>低弹性</summary>
        public const float ELASTICITY_LOW = 0.5f;
    }
    
    /// <summary>
    /// 缓动函数扩展方法
    /// </summary>
    public static class EasingExtensions
    {
        /// <summary>
        /// 应用标准进入缓动
        /// </summary>
        public static Tween SetEaseIn(this Tween tween, EaseType type = EaseType.Pop)
        {
            switch (type)
            {
                case EaseType.Pop:
                    return tween.SetEase(EasingConstants.EASE_IN_POP);
                case EaseType.Slide:
                    return tween.SetEase(EasingConstants.EASE_IN_SLIDE);
                case EaseType.Fade:
                    return tween.SetEase(EasingConstants.EASE_IN_FADE);
                case EaseType.Scale:
                    return tween.SetEase(EasingConstants.EASE_IN_SCALE);
                default:
                    return tween.SetEase(Ease.OutQuad);
            }
        }
        
        /// <summary>
        /// 应用标准退出缓动
        /// </summary>
        public static Tween SetEaseOut(this Tween tween, EaseType type = EaseType.Pop)
        {
            switch (type)
            {
                case EaseType.Pop:
                    return tween.SetEase(EasingConstants.EASE_OUT_POP);
                case EaseType.Slide:
                    return tween.SetEase(EasingConstants.EASE_OUT_SLIDE);
                case EaseType.Fade:
                    return tween.SetEase(EasingConstants.EASE_OUT_FADE);
                case EaseType.Scale:
                    return tween.SetEase(EasingConstants.EASE_OUT_SCALE);
                default:
                    return tween.SetEase(Ease.InQuad);
            }
        }
        
        /// <summary>
        /// 应用过冲效果
        /// </summary>
        public static Tween SetOvershoot(this Tween tween, float overshoot)
        {
            if (tween is Tweener tweener)
            {
                return tweener.SetEase(Ease.OutBack, overshoot);
            }
            return tween;
        }
    }
    
    /// <summary>
    /// 缓动类型枚举
    /// </summary>
    public enum EaseType
    {
        Pop,    // 弹出/弹入
        Slide,  // 滑动
        Fade,   // 淡入淡出
        Scale   // 缩放
    }
}
