namespace SebeJJ.Core
{
    /// <summary>
    /// 游戏状态枚举
    /// </summary>
    public enum GameState
    {
        None,
        MainMenu,
        Loading,
        Playing,
        Paused,
        GameOver,
        Victory
    }

    /// <summary>
    /// 资源类型枚举
    /// </summary>
    public enum ResourceType
    {
        None,
        ScrapMetal,     // 废金属
        CopperOre,      // 铜矿
        IronOre,        // 铁矿
        GoldOre,        // 金矿
        CrystalShard,   // 水晶碎片
        Uranium,        // 铀
        BioSample,      // 生物样本
        DataFragment,   // 数据碎片
        AncientTech     // 古代科技
    }

    /// <summary>
    /// 伤害类型枚举
    /// </summary>
    public enum DamageType
    {
        Physical,
        Energy,
        Explosive,
        Pressure,
        Corrosive
    }

    /// <summary>
    /// 任务状态枚举
    /// </summary>
    public enum MissionStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Failed,
        Expired
    }

    /// <summary>
    /// 任务类型枚举
    /// </summary>
    public enum MissionType
    {
        Collection,     // 收集资源
        Exploration,    // 探索区域
        Combat,         // 战斗任务
        Delivery,       // 运送任务
        Escort,         // 护送任务
        Boss            // Boss战
    }
}
