namespace CursorCompanionFinish.Models
{
    public class Settings
    {
        public int IdleTimeout { get; set; } = 10;
        public bool FreeRoamEnabled { get; set; } = false;
        public int RoamSpeed { get; set; } = 5;
        public float Sensitivity { get; set; } = 0.2f;
        public bool AutoReturnToBase { get; set; } = true;
        public bool MinimizeToTray { get; set; } = true;
        public int CurrentSkinId { get; set; } = 1;
        public bool EnableSounds { get; set; } = false;
        public bool ReactToClicks { get; set; } = true;

        // Обновляем значения по умолчанию
        public int RestZoneX { get; set; } = 1000;
        public int RestZoneY { get; set; } = 550;
        public int RestZoneWidth { get; set; } = 700;
        public int RestZoneHeight { get; set; } = 400;

        // Новая настройка
        public int ExploreWaitTime { get; set; } = 15; // Секунд до исследования

        public Settings() { }

        public void CopyFrom(Settings other)
        {
            if (other == null) return;

            IdleTimeout = other.IdleTimeout;
            FreeRoamEnabled = other.FreeRoamEnabled;
            RoamSpeed = other.RoamSpeed;
            Sensitivity = other.Sensitivity;
            AutoReturnToBase = other.AutoReturnToBase;
            MinimizeToTray = other.MinimizeToTray;
            CurrentSkinId = other.CurrentSkinId;
            EnableSounds = other.EnableSounds;
            ReactToClicks = other.ReactToClicks;

            // Копируем новые поля
            RestZoneX = other.RestZoneX;
            RestZoneY = other.RestZoneY;
            RestZoneWidth = other.RestZoneWidth;
            RestZoneHeight = other.RestZoneHeight;
            ExploreWaitTime = other.ExploreWaitTime;
        }
    }
}