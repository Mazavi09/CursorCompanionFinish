using System.Drawing;

namespace CursorCompanionFinish.Models
{
    /// <summary>
    /// Класс для описания скина питомца
    /// </summary>
    public class PetSkin
    {
        public int Id { get; set; }                        // ID скина (1-7)
        public string Name { get; set; }                   // Название скина
        public string Description { get; set; }            // Описание скина
        public Color ThemeColor { get; set; }              // Цвет темы скина
        public Point BasePosition { get; set; }            // Позиция места отдыха
        public bool IsUnlocked { get; set; } = true;       // Разблокирован ли скин

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public PetSkin()
        {
            Name = "Без имени";
            Description = "Описание отсутствует";
            ThemeColor = Color.Purple;
            BasePosition = new Point(100, 100);
        }

        /// <summary>
        /// Конструктор с параметрами
        /// </summary>
        public PetSkin(int id, string name, string description, Color themeColor, Point basePosition)
        {
            Id = id;
            Name = name;
            Description = description;
            ThemeColor = themeColor;
            BasePosition = basePosition;
            IsUnlocked = true;
        }
    }
}