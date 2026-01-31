using System;
using System.Drawing;
using System.Windows.Forms;

namespace CursorCompanionFinish
{
    public partial class HelpForm : Form
    {
        public HelpForm()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Справка - Cursor Companion";
            this.Size = new Size(700, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 30, 30);

            TabControl tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            // Вкладка 1: Основы
            TabPage basicsPage = new TabPage("Основы");
            basicsPage.BackColor = Color.FromArgb(45, 45, 48);
            basicsPage.ForeColor = Color.White;

            TextBox basicsText = new TextBox();
            basicsText.Multiline = true;
            basicsText.ReadOnly = true;
            basicsText.Dock = DockStyle.Fill;
            basicsText.BackColor = Color.FromArgb(45, 45, 48);
            basicsText.ForeColor = Color.White;
            basicsText.Font = new Font("Segoe UI", 10);
            basicsText.Text = @"🎮 КАК ПОЛЬЗОВАТЬСЯ:

1. ВЫБОР ПИТОМЦА:
   • Откройте вкладку 'Галерея'
   • Выберите понравившегося питомца (7 вариантов)
   • Нажмите 'Активировать'

2. ВЗАИМОДЕЙСТВИЕ:
   • Питомец следует за курсором
   • Если не двигать мышь 10 секунд - питомец уснет
   • Двойной клик по 'месту отдыха' отправит питомца спать
   • Включите 'Свободное исследование' в настройках

3. МЕСТО ОТДЫХА:
   • Находится в правом нижнем углу экрана (1000x550)
   • Размер: 200x200 пикселей
   • Полупрозрачная область для сна питомца
   • Двойной клик - отправить спать

4. НАСТРОЙКИ:
   • Время до сна: 5-60 секунд
   • Режим свободного исследования
   • Скорость исследования
   • Авто-возврат на место отдыха
   • Звуковые эффекты

5. СИСТЕМНЫЙ ТРЕЙ:
   • Приложение сворачивается в трей
   • Двойной клик по иконке - открыть
   • Правый клик - меню";

            basicsPage.Controls.Add(basicsText);

            // Вкладка 2: Советы
            TabPage tipsPage = new TabPage("Советы");
            tipsPage.BackColor = Color.FromArgb(45, 45, 48);
            tipsPage.ForeColor = Color.White;

            TextBox tipsText = new TextBox();
            tipsText.Multiline = true;
            tipsText.ReadOnly = true;
            tipsText.Dock = DockStyle.Fill;
            tipsText.BackColor = Color.FromArgb(45, 45, 48);
            tipsText.ForeColor = Color.White;
            tipsText.Font = new Font("Segoe UI", 10);
            tipsText.Text = @"💡 ПОЛЕЗНЫЕ СОВЕТЫ:

• Каждый питомец имеет уникальный фон рабочего стола
• 'Место отдыха' всегда отображается на экране
• В режиме исследования питомец сам изучает экран
• Статистика ведется для каждого питомца отдельно
• Приложение автоматически восстанавливает обои

🎯 БЫСТРЫЕ КОМАНДЫ:
• Двойной клик по иконке в трее - открыть
• Двойной клик по 'месту отдыха' - отправить спать
• В режиме исследования питомец ждет 3 секунды на точке

⚠ УСТРАНЕНИЕ ПРОБЛЕМ:
1. Если питомец не двигается - перезапустите приложение
2. Если не меняются обои - проверьте права администратора
3. Для лучшей производительности используйте PNG спрайты
4. Если файлы не загружаются - используйте кнопку 'Отладка'

🎨 КАСТОМИЗАЦИЯ:
• Создайте свои спрайты в папках Skins/skin_X/
• Формат: sprite_0.png до sprite_6.png
• Размер: 48x48 пикселей рекомендуется
• Прозрачный фон (маджента) будет удален

📁 СТРУКТУРА ФАЙЛОВ:
• Backgrounds/ - обои для каждого скина
• Skins/skin_1 до skin_7/ - спрайты питомцев
• Data/settings.ini - настройки
• Logs/ - логи активности";

            tipsPage.Controls.Add(tipsText);

            // Добавляем вкладки
            tabControl.TabPages.Add(basicsPage);
            tabControl.TabPages.Add(tipsPage);

            this.Controls.Add(tabControl);

            // Кнопка закрытия
            Button btnClose = new Button();
            btnClose.Text = "Закрыть";
            btnClose.Size = new Size(100, 30);
            btnClose.Location = new Point(300, 500);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.BackColor = Color.FromArgb(64, 64, 64);
            btnClose.ForeColor = Color.White;
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(btnClose);
        }
    }
}