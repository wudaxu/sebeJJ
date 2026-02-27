using UnityEngine;

namespace SebeJJ.Enemies
{
    /// <summary>
    /// CL-001修复: 此类已被弃用，请使用AI/EnemyBase.cs
    /// 为了保持向后兼容，此类现在继承自AI.EnemyBase
    /// </summary>    [System.Obsolete("请使用SebeJJ.Enemies.EnemyBase (位于AI文件夹) 或直接使用AI命名空间下的EnemyBase")]
    public abstract class EnemyBaseLegacy : AI.EnemyBase
    {
        // 此类仅用于向后兼容，所有功能已移至AI/EnemyBase.cs
    }
    
    /// <summary>
    /// 敌人基类 - 所有敌人的抽象基类
    /// CL-001修复: 此类现在作为AI.EnemyBase的别名，保持向后兼容
    /// </summary>    public abstract class EnemyBase : AI.EnemyBase
    {
        // 所有实现继承自AI.EnemyBase
    }
}
