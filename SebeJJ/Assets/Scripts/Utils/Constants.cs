namespace SebeJJ.Utils
{
    /// <summary>
    /// 游戏常量定义
    /// </summary>
    public static class Constants
    {
        #region Scene Names

        public const string MAIN_MENU_SCENE = "MainMenu";
        public const string GAME_SCENE = "MainScene";
        public const string LOADING_SCENE = "Loading";

        #endregion

        #region Layer Names

        public const string LAYER_PLAYER = "Player";
        public const string LAYER_ENEMY = "Enemy";
        public const string LAYER_RESOURCE = "Resource";
        public const string LAYER_OBSTACLE = "Obstacle";
        public const string LAYER_PROJECTILE = "Projectile";

        #endregion

        #region Tag Names

        public const string TAG_PLAYER = "Player";
        public const string TAG_ENEMY = "Enemy";
        public const string TAG_RESOURCE = "Resource";
        public const string TAG_SPAWN_POINT = "SpawnPoint";

        #endregion

        #region Input Names

        public const string INPUT_HORIZONTAL = "Horizontal";
        public const string INPUT_VERTICAL = "Vertical";
        public const string INPUT_ROTATE = "Rotate";
        public const string INPUT_BOOST = "Boost";
        public const string INPUT_FIRE = "Fire1";
        public const string INPUT_SECONDARY_FIRE = "Fire2";
        public const string INPUT_INTERACT = "Interact";
        public const string INPUT_PAUSE = "Pause";
        public const string INPUT_INVENTORY = "Inventory";
        public const string INPUT_MAP = "Map";

        #endregion

        #region Default Values

        public const float DEFAULT_MOVE_SPEED = 5f;
        public const float DEFAULT_ROTATION_SPEED = 180f;
        public const float DEFAULT_MAX_HEALTH = 100f;
        public const float DEFAULT_MAX_ENERGY = 100f;
        public const float DEFAULT_MAX_OXYGEN = 100f;
        public const int DEFAULT_CARGO_CAPACITY = 50;

        #endregion

        #region Gameplay Values

        public const float OXYGEN_DEPLETION_RATE = 1f;
        public const float ENERGY_REGEN_RATE = 5f;
        public const float PRESSURE_DAMAGE_RATE = 2f;
        public const float BUOYANCY_FORCE = 2f;
        public const float DEPTH_DAMAGE_THRESHOLD = 100f;

        #endregion

        #region UI Values

        public const float UI_FADE_DURATION = 0.2f;
        public const float WARNING_DURATION = 3f;
        public const float NOTIFICATION_DURATION = 3f;
        public const float DAMAGE_INDICATOR_DURATION = 0.5f;

        #endregion

        #region Save Keys

        public const string SAVE_KEY_PLAYER_DATA = "PlayerData";
        public const string SAVE_KEY_SETTINGS = "GameSettings";
        public const string SAVE_KEY_MISSIONS = "MissionData";
        public const string SAVE_KEY_INVENTORY = "InventoryData";

        #endregion

        #region Audio

        public const string AUDIO_MIXER_MASTER = "Master";
        public const string AUDIO_MIXER_MUSIC = "Music";
        public const string AUDIO_MIXER_SFX = "SFX";
        public const string AUDIO_MIXER_AMBIENCE = "Ambience";

        #endregion
    }
}