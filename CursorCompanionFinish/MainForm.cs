using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Models = CursorCompanionFinish.Models; // Псевдоним
using Services = CursorCompanionFinish.Services; // Псевдоним

namespace CursorCompanionFinish
{
    public partial class MainForm : Form
    {
        private Services.WallpaperService _wallpaperService;
        private Services.LogService _logService;
        private Models.Settings _settings; // Используем Models.Settings
        private List<Models.PetSkin> _skins; // Используем Models.PetSkin
        private PetAI _currentPet;
        private NotifyIcon _trayIcon;

        // ДОБАВЬТЕ ЭТУ СТРОКУ:
        private string _fixedBasePath;

        // Элементы интерфейса
        private Panel navigationPanel;
        private Panel contentPanel;
        private ListBox skinListBox;
        private PictureBox skinPreview;
        private Label skinDescription;
        private Button btnActivate;
        private Label lblStatus;

        public MainForm()
        {
            // ДОБАВЬТЕ ЭТУ СТРОКУ:
            _fixedBasePath = Application.StartupPath;

            // ОТЛАДОЧНАЯ ИНФОРМАЦИЯ ПЕРВЫМ ДЕЛОМ
            Debug.WriteLine("==========================================");
            Debug.WriteLine("НАЧАЛО РАБОТЫ ПРОГРАММЫ");
            Debug.WriteLine("Время: " + DateTime.Now.ToString("HH:mm:ss"));
            Debug.WriteLine("Текущая директория: " + Directory.GetCurrentDirectory());
            Debug.WriteLine("BaseDirectory: " + AppDomain.CurrentDomain.BaseDirectory);
            Debug.WriteLine("Application.StartupPath: " + Application.StartupPath);
            Debug.WriteLine("Environment.CurrentDirectory: " + Environment.CurrentDirectory);
            Debug.WriteLine("FixedBasePath: " + _fixedBasePath); // Добавьте эту строку
            Debug.WriteLine("==========================================");

            // ... остальной код конструктора



            // Сначала создаем папки и проверяем файлы
            CreateFolders();
            CheckFiles();

            InitializeComponent();
            InitializeServices();
            InitializeUI();
            LoadSkins();

            // ДОБАВЬТЕ ЭТУ ПРОВЕРКУ ДЛЯ ДЕБАГА
            if (_skins == null)
            {
                Debug.WriteLine("ОШИБКА: _skins не был инициализирован!");
            }

            LoadSettings();
            SetupTrayIcon();

            UpdateStatus("Готов к работе");
        }

        private void CreateFolders()
        {
            try
            {
                string[] folders = { "Data", "Backgrounds", "Skins", "Logs" };
                foreach (string folder in folders)
                {
                    string fullPath = Path.Combine(Directory.GetCurrentDirectory(), folder);
                    if (!Directory.Exists(fullPath))
                    {
                        Directory.CreateDirectory(fullPath);
                        Debug.WriteLine("Создана папка: " + fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка создания папок: " + ex.Message);
            }
        }

        private void CheckFiles()
        {
            Debug.WriteLine("=== ПРОВЕРКА ФАЙЛОВ ===");

            string backgroundsPath = GetCorrectPath("Backgrounds");
            string skinsPath = GetCorrectPath("Skins");

            Debug.WriteLine("Путь к Backgrounds: " + backgroundsPath);
            Debug.WriteLine("Путь к Skins: " + skinsPath);

            // Проверяем Backgrounds
            if (Directory.Exists(backgroundsPath))
            {
                var files = Directory.GetFiles(backgroundsPath, "*.png");
                Debug.WriteLine("Фонов найдено: " + files.Length);
                foreach (string file in files)
                {
                    Debug.WriteLine("  - " + Path.GetFileName(file));
                }
            }
            else
            {
                Debug.WriteLine("Папка Backgrounds не найдена по пути: " + backgroundsPath);
            }

            // Проверяем Skins
            if (Directory.Exists(skinsPath))
            {
                for (int i = 1; i <= 7; i++)
                {
                    string skinPath = Path.Combine(skinsPath, "skin_" + i.ToString());
                    if (Directory.Exists(skinPath))
                    {
                        var spriteFiles = Directory.GetFiles(skinPath, "sprite_*.png");
                        var previewFiles = Directory.GetFiles(skinPath, "preview.png");
                        Debug.WriteLine("skin_" + i + ": " + spriteFiles.Length + " спрайтов, превью: " + (previewFiles.Length > 0 ? "Да" : "Нет"));
                    }
                    else
                    {
                        Debug.WriteLine("skin_" + i + ": папка не найдена");
                    }
                }
            }
            else
            {
                Debug.WriteLine("Папка Skins не найдена");
            }

            Debug.WriteLine("=== КОНЕЦ ПРОВЕРКИ ===");
        }

        private string GetCorrectPath(string relativePath)
        {
            // Пробуем несколько возможных путей
            string[] possiblePaths = {
                relativePath,
                Path.Combine(Directory.GetCurrentDirectory(), relativePath),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath),
                Path.Combine(Application.StartupPath, relativePath),
                Path.Combine(@"C:\Users\user\Desktop\Cursor Companion\CursorCompanionFinish\CursorCompanionFinish\bin\Debug", relativePath)
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path) || Directory.Exists(path))
                {
                    Debug.WriteLine($"GetCorrectPath: найдено по пути {path}");
                    return path;
                }
            }

            Debug.WriteLine($"GetCorrectPath: не найдено ни по одному пути: {relativePath}");
            return Path.Combine(Application.StartupPath, relativePath);
        }

        private void InitializeServices()
        {
            _wallpaperService = new Services.WallpaperService();
            _logService = new Services.LogService();
            _settings = new Models.Settings(); // Models.Settings
        }


        private void InitializeUI()
        {
            // Настройка формы - НА ВЕСЬ ЭКРАН ПО ВЫСОТЕ
            this.Text = "Cursor Companion";
            Screen screen = Screen.PrimaryScreen;
            Rectangle screenBounds = screen.Bounds;
            this.Size = new Size(900, screenBounds.Height - 40);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Панель навигации (слева)
            navigationPanel = new Panel();
            navigationPanel.BackColor = Color.FromArgb(45, 45, 48);
            navigationPanel.Dock = DockStyle.Left;
            navigationPanel.Width = 180;
            navigationPanel.Padding = new Padding(15, 20, 15, 20); // Отступы внутри панели

            // FlowLayoutPanel для автоматического расположения кнопок
            FlowLayoutPanel flowPanel = new FlowLayoutPanel();
            flowPanel.Dock = DockStyle.Fill;
            flowPanel.FlowDirection = FlowDirection.TopDown;
            flowPanel.WrapContents = false;
            flowPanel.AutoScroll = true;
            flowPanel.BackColor = Color.Transparent;

            // 1. Кнопка "Проверить фоны"
            Button btnCheckBackgrounds = CreateNavButton("🎨 Проверить фоны");
            btnCheckBackgrounds.BackColor = Color.FromArgb(100, 100, 200);
            btnCheckBackgrounds.Click += (s, e) => CheckAllBackgrounds();
            flowPanel.Controls.Add(btnCheckBackgrounds);

            // 2. Кнопка "Галерея"
            Button btnGallery = CreateNavButton("🐉 Галерея");
            btnGallery.Tag = "gallery";
            btnGallery.Click += NavigationButton_Click;
            flowPanel.Controls.Add(btnGallery);

            // 3. Кнопка "Настройки"
            Button btnSettings = CreateNavButton("⚙ Настройки");
            btnSettings.Tag = "settings";
            btnSettings.Click += NavigationButton_Click;
            flowPanel.Controls.Add(btnSettings);

            // 4. Кнопка "Статистика"
            Button btnStats = CreateNavButton("📊 Статистика");
            btnStats.Tag = "stats";
            btnStats.Click += NavigationButton_Click;
            flowPanel.Controls.Add(btnStats);

            // 5. Кнопка "Справка"
            Button btnHelp = CreateNavButton("❓ Справка");
            btnHelp.Tag = "help";
            btnHelp.Click += NavigationButton_Click;
            flowPanel.Controls.Add(btnHelp);

            // 6. Кнопка "О программе"
            Button btnAbout = CreateNavButton("ℹ О программе");
            btnAbout.Tag = "about";
            btnAbout.Click += NavigationButton_Click;
            flowPanel.Controls.Add(btnAbout);

            // 7. Кнопка "Отладка"
            Button btnDebug = CreateNavButton("🐛 Отладка");
            btnDebug.BackColor = Color.FromArgb(100, 100, 100);
            btnDebug.Click += (s, e) => ShowDebugInfo();
            flowPanel.Controls.Add(btnDebug);

            // Панель для нижних кнопок (Применить, Свернуть и На главную)
            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 160; // Увеличили высоту для третьей кнопки
            bottomPanel.BackColor = Color.Transparent;

            // Кнопка "Применить" - ПЕРВАЯ (верхняя)
            Button btnApply = new Button();
            btnApply.Text = "✓ Применить";
            btnApply.Size = new Size(150, 45);
            btnApply.Location = new Point(0, 5); // Было 10
            btnApply.FlatStyle = FlatStyle.Flat;
            btnApply.BackColor = Color.FromArgb(0, 120, 215);
            btnApply.ForeColor = Color.White;
            btnApply.Click += (s, e) => ApplySettings();
            bottomPanel.Controls.Add(btnApply);

            // Кнопка "Свернуть" - ВТОРАЯ (посередине)
            Button btnMinimize = new Button();
            btnMinimize.Text = "▼ Свернуть";
            btnMinimize.Size = new Size(150, 45);
            btnMinimize.Location = new Point(0, 60); // Было 65
            btnMinimize.FlatStyle = FlatStyle.Flat;
            btnMinimize.BackColor = Color.FromArgb(64, 64, 64);
            btnMinimize.ForeColor = Color.White;
            btnMinimize.Click += (s, e) => MinimizeToTray();
            bottomPanel.Controls.Add(btnMinimize);

            // НОВАЯ кнопка "На главную" - ТРЕТЬЯ (самая нижняя)
            Button btnHome = new Button();
            btnHome.Text = "🏠 На главную";
            btnHome.Size = new Size(150, 45);
            btnHome.Location = new Point(0, 115); // Было 120
            btnHome.FlatStyle = FlatStyle.Flat;
            btnHome.BackColor = Color.FromArgb(100, 100, 100);
            btnHome.ForeColor = Color.White;
            btnHome.Click += (s, e) => GoToHome();
            bottomPanel.Controls.Add(btnHome);

            // Добавляем все в панель навигации
            navigationPanel.Controls.Add(flowPanel);
            navigationPanel.Controls.Add(bottomPanel);

            // Основная панель контента
            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.FromArgb(30, 30, 30);

            // Статус бар внизу
            lblStatus = new Label();
            lblStatus.Text = "Готово";
            lblStatus.ForeColor = Color.LightGray;
            lblStatus.BackColor = Color.FromArgb(45, 45, 48);
            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Height = 30;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.Padding = new Padding(10, 0, 0, 0);
            lblStatus.Font = new Font("Segoe UI", 9);

            // Добавляем панели на форму
            this.Controls.Add(contentPanel);
            this.Controls.Add(navigationPanel);
            this.Controls.Add(lblStatus);

            // Создаем меню
            InitializeMenu();

            // Показываем галерею по умолчанию
            ShowGallery();
        }

        // Упрощенный метод создания кнопок
        private Button CreateNavButton(string text)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.FlatStyle = FlatStyle.Flat;
            btn.ForeColor = Color.White;
            btn.BackColor = Color.Transparent;
            btn.Size = new Size(150, 45);
            btn.Margin = new Padding(0, 0, 0, 10); // Отступ снизу
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Font = new Font("Segoe UI", 10);
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void GoToHome()
        {
            // Восстанавливаем обои перед переходом на главную
            if (_wallpaperService != null)
            {
                _wallpaperService.RestoreOriginalWallpaper();
            }

            // Останавливаем питомца
            if (_currentPet != null)
            {
                _currentPet.Stop();
                _currentPet = null;
            }

            // Закрываем текущее главное окно
            this.Close();

            // Открываем окно входа
            SplashForm splashForm = new SplashForm();
            splashForm.Show();
        }

        // Обновляем метод ExitApplication() чтобы не закрывать программу полностью


        private void CheckAllBackgrounds()
        {
            string result = "=== ПРОВЕРКА ВСЕХ ФОНОВ ===\n\n";

            for (int i = 1; i <= 7; i++)
            {
                string bgPath = GetBackgroundPath(i);
                bool exists = File.Exists(bgPath);

                result += $"Скин {i}:\n";
                result += $"  Путь: {bgPath}\n";
                result += $"  Существует: {(exists ? "✅" : "❌")}\n";

                if (exists)
                {
                    FileInfo fi = new FileInfo(bgPath);
                    result += $"  Размер: {fi.Length / 1024} KB\n";
                    result += $"  Дата: {fi.LastWriteTime:dd.MM.yyyy HH:mm}\n";
                }

                result += "\n";
            }

            // Проверяем папку Backgrounds
            string backgroundsPath = Path.Combine(_fixedBasePath, "Backgrounds");
            result += $"Папка Backgrounds: {backgroundsPath}\n";
            result += $"Существует: {(Directory.Exists(backgroundsPath) ? "✅" : "❌")}\n";

            if (Directory.Exists(backgroundsPath))
            {
                string[] files = Directory.GetFiles(backgroundsPath, "*.png");
                result += $"Файлов найдено: {files.Length}\n";
                foreach (string file in files)
                {
                    result += $"  - {Path.GetFileName(file)}\n";
                }
            }

            MessageBox.Show(result, "Проверка фонов", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Версия метода с позицией Y:
        private Button CreateNavButton(string text, string tag, int y)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Tag = tag;
            btn.FlatStyle = FlatStyle.Flat;
            btn.ForeColor = Color.White;
            btn.BackColor = Color.Transparent;
            btn.Size = new Size(150, 45);
            btn.Location = new Point(15, y); // Используем переданный Y
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Font = new Font("Segoe UI", 10);
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void ShowDebugInfo()
        {
            string info = "=== ИНФОРМАЦИЯ О ПУТЯХ ===\n\n";

            info += "1. Application.StartupPath:\n";
            info += Application.StartupPath + "\n\n";

            info += "2. Directory.GetCurrentDirectory():\n";
            info += Directory.GetCurrentDirectory() + "\n\n";

            info += "3. AppDomain.CurrentDomain.BaseDirectory:\n";
            info += AppDomain.CurrentDomain.BaseDirectory + "\n\n";

            info += "4. Проверка Backgrounds:\n";
            string bgPath = GetCorrectPath("Backgrounds");
            info += "Путь: " + bgPath + "\n";
            info += "Существует: " + Directory.Exists(bgPath) + "\n";
            if (Directory.Exists(bgPath))
            {
                var files = Directory.GetFiles(bgPath, "*.png");
                info += "Файлов найдено: " + files.Length + "\n";
                foreach (string file in files.Take(5))
                {
                    info += "  - " + Path.GetFileName(file) + "\n";
                }
            }

            info += "\n5. Проверка Skins/skin_1:\n";
            string skinPath = GetCorrectPath(Path.Combine("Skins", "skin_1"));
            info += "Путь: " + skinPath + "\n";
            info += "Существует: " + Directory.Exists(skinPath) + "\n";
            if (Directory.Exists(skinPath))
            {
                string previewPath = Path.Combine(skinPath, "preview.png");
                info += "preview.png существует: " + File.Exists(previewPath) + "\n";
                var spriteFiles = Directory.GetFiles(skinPath, "sprite_*.png");
                info += "Спрайтов найдено: " + spriteFiles.Length + "\n";
            }

            info += "\n6. Текущий скин: " + (_settings?.CurrentSkinId.ToString() ?? "не задан");

            MessageBox.Show(info, "Отладочная информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void InitializeMenu()
        {
            MenuStrip menuStrip = new MenuStrip();

            // Меню Файл
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("Файл");
            fileMenu.DropDownItems.Add("Свернуть в трей", null, (s, e) => MinimizeToTray());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Выход", null, (s, e) => ExitApplication());

            // Меню Вид
            ToolStripMenuItem viewMenu = new ToolStripMenuItem("Вид");
            viewMenu.DropDownItems.Add("Галерея", null, (s, e) => ShowGallery());
            viewMenu.DropDownItems.Add("Настройки", null, (s, e) => ShowSettings());

            // Меню Помощь
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("Помощь");
            helpMenu.DropDownItems.Add("Справка", null, (s, e) => ShowHelp());
            helpMenu.DropDownItems.Add("О программе", null, (s, e) => ShowAbout());

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(viewMenu);
            menuStrip.Items.Add(helpMenu);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private void InitializeGallery()
        {
            contentPanel.Controls.Clear();

            // Заголовок
            Label lblTitle = new Label();
            lblTitle.Text = "🎮 ВЫБЕРИТЕ ПИТОМЦА";
            lblTitle.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(30, 30);
            lblTitle.AutoSize = true;
            contentPanel.Controls.Add(lblTitle);

            // Список скинов слева - УВЕЛИЧИЛИ
            skinListBox = new ListBox();
            skinListBox.Size = new Size(250, 400);
            skinListBox.Location = new Point(30, 80);
            skinListBox.BackColor = Color.FromArgb(45, 45, 48);
            skinListBox.ForeColor = Color.White;
            skinListBox.BorderStyle = BorderStyle.FixedSingle;
            skinListBox.Font = new Font("Segoe UI", 11);
            skinListBox.SelectedIndexChanged += SkinListBox_SelectedIndexChanged;
            contentPanel.Controls.Add(skinListBox);

            // Превью справа - УВЕЛИЧИЛИ И СМЕСТИЛИ
            skinPreview = new PictureBox();
            skinPreview.Size = new Size(400, 300);
            skinPreview.Location = new Point(300, 80);
            skinPreview.BorderStyle = BorderStyle.FixedSingle;
            skinPreview.SizeMode = PictureBoxSizeMode.Zoom;
            skinPreview.BackColor = Color.FromArgb(20, 20, 20);
            contentPanel.Controls.Add(skinPreview);

            // Описание - УВЕЛИЧИЛИ И СМЕСТИЛИ
            skinDescription = new Label();
            skinDescription.Location = new Point(300, 390);
            skinDescription.Size = new Size(400, 80);
            skinDescription.ForeColor = Color.White;
            skinDescription.Font = new Font("Segoe UI", 10);
            skinDescription.Text = "Выберите скин для просмотра";
            contentPanel.Controls.Add(skinDescription);

            // Кнопка активации - СМЕСТИЛИ
            btnActivate = new Button();
            btnActivate.Text = "🎮 АКТИВИРОВАТЬ";
            btnActivate.Size = new Size(180, 50);
            btnActivate.Location = new Point(300, 480);
            btnActivate.BackColor = Color.FromArgb(0, 150, 100);
            btnActivate.ForeColor = Color.White;
            btnActivate.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnActivate.FlatStyle = FlatStyle.Flat;
            btnActivate.Click += BtnActivate_Click;
            contentPanel.Controls.Add(btnActivate);

            // Загружаем скины в список
            LoadSkinsToListBox();
        }

        private void LoadSkins()
        {
            try
            {
                Debug.WriteLine("Начинаем загрузку скинов...");

                // ВАЖНО: Всегда создаем новый список
                _skins = new List<Models.PetSkin>();

                // Создаем 7 скинов
                for (int i = 1; i <= 7; i++)
                {
                    Models.PetSkin skin = new Models.PetSkin();
                    skin.Id = i;
                    skin.IsUnlocked = true;

                    // Устанавливаем имя в зависимости от ID
                    switch (i)
                    {
                        case 1: skin.Name = "Ящерица"; break;
                        case 2: skin.Name = "Крыса"; break;
                        case 3: skin.Name = "Змея"; break;
                        case 4: skin.Name = "Аксолотль"; break;
                        case 5: skin.Name = "Кот"; break;
                        case 6: skin.Name = "Хорек"; break;
                        case 7: skin.Name = "Дракон"; break;
                        default: skin.Name = "Скин " + i; break;
                    }

                    skin.Description = GetSkinDescription(i);
                    skin.ThemeColor = GetThemeColor(i);
                    skin.BasePosition = new Point(1000, 550); // Фиксированное место отдыха

                    _skins.Add(skin);
                    Debug.WriteLine($"Добавлен скин: {skin.Name} (ID: {skin.Id})");
                }

                Debug.WriteLine($"Всего загружено скинов: {_skins.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ОШИБКА в LoadSkins: {ex.Message}");
                _skins = new List<Models.PetSkin>(); // Всегда оставляем пустой список вместо null
            }
        }

        private string GetSkinDescription(int skinId)
        {
            switch (skinId)
            {
                case 1: return "Зеленая ящерица с цепким хвостом. Любит греться на камнях.";
                case 2: return "Шустрая крыска с длинным хвостом. Любит исследовать новые места.";
                case 3: return "Гладкая змейка. Плавно скользит за курсором.";
                case 4: return "Водный дракон. Всегда улыбается и любит воду.";
                case 5: return "Ленивый котик. Часто засыпает на своем камне.";
                case 6: return "Веселый хорёк. Любит играть и бегать по экрану.";
                case 7: return "Маленький дракончик. Иногда выпускает дымок.";
                default: return "Интерактивный питомец для вашего курсора.";
            }
        }

        private Color GetThemeColor(int skinId)
        {
            switch (skinId)
            {
                case 1: return Color.FromArgb(76, 175, 80);     // Ящерица - зеленый
                case 2: return Color.FromArgb(121, 85, 72);     // Крыса - коричневый
                case 3: return Color.FromArgb(139, 195, 74);    // Змея - салатовый
                case 4: return Color.FromArgb(33, 150, 243);    // Аксолотль - синий
                case 5: return Color.FromArgb(158, 158, 158);   // Кот - серый
                case 6: return Color.FromArgb(255, 152, 0);     // Хорек - оранжевый
                case 7: return Color.FromArgb(233, 30, 99);     // Дракон - розовый
                default: return Color.Purple;
            }
        }

        private void LoadSkinsToListBox()
        {
            // Проверяем, существует ли ListBox
            if (skinListBox == null)
            {
                Debug.WriteLine("Ошибка: skinListBox не инициализирован");
                return;
            }

            // Проверяем, инициализирован ли список скинов
            if (_skins == null)
            {
                Debug.WriteLine("_skins равен null, вызываем LoadSkins()");
                LoadSkins();
            }

            try
            {
                skinListBox.Items.Clear();

                // Проверяем, есть ли скины для отображения
                if (_skins == null || _skins.Count == 0)
                {
                    Debug.WriteLine("Список скинов пуст!");
                    skinListBox.Items.Add("Нет доступных скинов");
                    return;
                }

                foreach (Models.PetSkin skin in _skins)
                {
                    skinListBox.Items.Add(skin.Name);
                }

                if (skinListBox.Items.Count > 0)
                    skinListBox.SelectedIndex = 0;

                Debug.WriteLine($"Загружено {_skins.Count} скинов в ListBox");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка в LoadSkinsToListBox: {ex.Message}");
                skinListBox.Items.Clear();
                skinListBox.Items.Add($"Ошибка: {ex.Message}");
            }
        }

        private void SkinListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_skins == null || skinListBox == null || skinListBox.SelectedIndex < 0 || skinListBox.SelectedIndex >= _skins.Count)
                return;

            var skin = _skins[skinListBox.SelectedIndex];

            // Обновляем превью
            if (skinPreview != null)
            {
                skinPreview.BackColor = Color.FromArgb(20, 20, 20);
            }

            if (skinDescription != null)
            {
                skinDescription.Text = skin.Description + "\n\nЦвет темы: " + skin.ThemeColor.Name;
            }

            // Пытаемся загрузить изображение скина
            string relativePath = Path.Combine("Skins", "skin_" + skin.Id.ToString(), "preview.png");
            string imagePath = GetCorrectPath(relativePath);

            Debug.WriteLine($"Попытка загрузить превью: {imagePath}");
            Debug.WriteLine($"Файл существует: {File.Exists(imagePath)}");

            if (File.Exists(imagePath))
            {
                try
                {
                    // Освобождаем старую картинку
                    if (skinPreview != null && skinPreview.Image != null)
                    {
                        skinPreview.Image.Dispose();
                        skinPreview.Image = null;
                    }

                    if (skinPreview != null)
                    {
                        skinPreview.Image = Image.FromFile(imagePath);
                        UpdateStatus("Загружено превью: " + skin.Name);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Ошибка загрузки превью: " + ex.Message);
                    CreatePreviewImage(skin);
                    UpdateStatus("Ошибка загрузки превью: " + ex.Message);
                }
            }
            else
            {
                Debug.WriteLine("Превью не найдено!");
                CreatePreviewImage(skin);
                UpdateStatus("Превью не найдено, создано тестовое");
            }
        }

        private void CreatePreviewImage(Models.PetSkin skin)
        {
            // Создаем тестовое изображение
            Bitmap bmp = new Bitmap(400, 300);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Градиентный фон
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    new Point(0, 0),
                    new Point(400, 300),
                    Color.FromArgb(30, 30, 40),
                    skin.ThemeColor))
                {
                    g.FillRectangle(brush, 0, 0, 400, 300);
                }

                // Название скина
                using (Font font = new Font("Arial", 24, FontStyle.Bold))
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    g.DrawString(skin.Name, font, textBrush, 50, 100);
                }

                // Сообщение
                using (Font font = new Font("Arial", 12))
                using (SolidBrush textBrush = new SolidBrush(Color.LightGray))
                {
                    g.DrawString("Тестовое превью", font, textBrush, 120, 160);
                    g.DrawString("ID: " + skin.Id, font, textBrush, 120, 180);
                }
            }

            if (skinPreview != null)
            {
                skinPreview.Image = bmp;
            }
            else
            {
                bmp.Dispose();
            }
        }

        private void LoadSettings()
        {
            try
            {
                string settingsFile = GetCorrectPath(Path.Combine("Data", "settings.ini"));
                if (File.Exists(settingsFile))
                {
                    Debug.WriteLine("Загружаем настройки из: " + settingsFile);
                    string[] lines = File.ReadAllLines(settingsFile);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("IdleTimeout="))
                        {
                            string value = line.Substring("IdleTimeout=".Length);
                            if (int.TryParse(value, out int timeout))
                                _settings.IdleTimeout = timeout;
                        }
                        else if (line.StartsWith("LastSkinId="))
                        {
                            string value = line.Substring("LastSkinId=".Length);
                            if (int.TryParse(value, out int skinId))
                                _settings.CurrentSkinId = skinId;
                        }
                        else if (line.StartsWith("FreeRoamEnabled="))
                        {
                            string value = line.Substring("FreeRoamEnabled=".Length);
                            if (bool.TryParse(value, out bool enabled))
                                _settings.FreeRoamEnabled = enabled;
                        }
                        else if (line.StartsWith("MinimizeToTray="))
                        {
                            string value = line.Substring("MinimizeToTray=".Length);
                            if (bool.TryParse(value, out bool minimize))
                                _settings.MinimizeToTray = minimize;
                        }
                        // ДОБАВЛЯЕМ ЗАГРУЗКУ НАСТРОЕК ЗОНЫ
                        else if (line.StartsWith("RestZoneX="))
                        {
                            string value = line.Substring("RestZoneX=".Length);
                            if (int.TryParse(value, out int x))
                                _settings.RestZoneX = x;
                        }
                        else if (line.StartsWith("RestZoneY="))
                        {
                            string value = line.Substring("RestZoneY=".Length);
                            if (int.TryParse(value, out int y))
                                _settings.RestZoneY = y;
                        }
                        else if (line.StartsWith("RestZoneWidth="))
                        {
                            string value = line.Substring("RestZoneWidth=".Length);
                            if (int.TryParse(value, out int width))
                                _settings.RestZoneWidth = width;
                        }
                        else if (line.StartsWith("RestZoneHeight="))
                        {
                            string value = line.Substring("RestZoneHeight=".Length);
                            if (int.TryParse(value, out int height))
                                _settings.RestZoneHeight = height;
                        }
                    }
                    Debug.WriteLine("Настройки загружены успешно");
                }
                else
                {
                    Debug.WriteLine("Файл настроек не найден, используем значения по умолчанию");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка загрузки настроек: " + ex.Message);
            }
        }

        private void SaveSettings()
        {
            try
            {
                string settingsDir = GetCorrectPath("Data");
                string settingsFile = Path.Combine(settingsDir, "settings.ini");

                Debug.WriteLine("Сохраняем настройки в: " + settingsFile);

                string settings =
                    "[Behavior]\r\n" +
                    "IdleTimeout=" + _settings.IdleTimeout.ToString() + "\r\n" +
                    "FreeRoamEnabled=" + _settings.FreeRoamEnabled.ToString() + "\r\n" +
                    "RoamSpeed=" + _settings.RoamSpeed.ToString() + "\r\n" +
                    "Sensitivity=" + _settings.Sensitivity.ToString("0.0") + "\r\n" +
                    "AutoReturnToBase=" + _settings.AutoReturnToBase.ToString() + "\r\n" +
                    "\r\n" +
                    "[App]\r\n" +
                    "LastSkinId=" + _settings.CurrentSkinId.ToString() + "\r\n" +
                    "MinimizeToTray=" + _settings.MinimizeToTray.ToString() + "\r\n" +
                    "EnableSounds=" + _settings.EnableSounds.ToString() + "\r\n" +
                    "ReactToClicks=" + _settings.ReactToClicks.ToString() + "\r\n" +
                    "\r\n" +
                    "[RestZone]\r\n" +
                    "RestZoneX=" + _settings.RestZoneX.ToString() + "\r\n" +
                    "RestZoneY=" + _settings.RestZoneY.ToString() + "\r\n" +
                    "RestZoneWidth=" + _settings.RestZoneWidth.ToString() + "\r\n" +
                    "RestZoneHeight=" + _settings.RestZoneHeight.ToString();

                File.WriteAllText(settingsFile, settings);
                Debug.WriteLine("Настройки сохранены успешно");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка сохранения настроек: " + ex.Message);
            }
        }
        
        private void SetupTrayIcon()
        {
            _trayIcon = new NotifyIcon();
            _trayIcon.Icon = SystemIcons.Application;
            _trayIcon.Text = "Cursor Companion";
            _trayIcon.Visible = true;

            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Открыть", null, (s, e) =>
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                UpdateStatus("Приложение восстановлено из трея");
            });
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Выйти", null, (s, e) => ExitApplication());

            _trayIcon.ContextMenuStrip = contextMenu;
            _trayIcon.DoubleClick += (s, e) =>
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                UpdateStatus("Приложение восстановлено из трея");
            };
        }

        private void BtnActivate_Click(object sender, EventArgs e)
        {
            // ДОБАВЛЕНО: Проверка на null
            if (_skins == null || skinListBox == null || skinListBox.SelectedIndex < 0 || skinListBox.SelectedIndex >= _skins.Count)
            {
                MessageBox.Show("Пожалуйста, выберите скин!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var skin = _skins[skinListBox.SelectedIndex];
            ActivateSkin(skin);
        }

        private void ActivateSkin(Models.PetSkin skin)
        {
            {
                Debug.WriteLine($"=== АКТИВАЦИЯ СКИНА {skin.Id} ({skin.Name}) ===");
                UpdateStatus($"Активация скина: {skin.Name}");

                // Останавливаем текущего питомца
                if (_currentPet != null)
                {
                    _currentPet.Stop();
                    _currentPet = null;
                    UpdateStatus("Текущий питомец остановлен");
                }

                // Получаем путь к фону
                string bgPath = GetBackgroundPath(skin.Id);

                Debug.WriteLine($"Путь к фону: {bgPath}");
                Debug.WriteLine($"Файл существует: {File.Exists(bgPath)}");

                if (File.Exists(bgPath))
                {
                    Debug.WriteLine($"Информация о файле:");
                    FileInfo fi = new FileInfo(bgPath);
                    Debug.WriteLine($"  Размер: {fi.Length} байт");
                    Debug.WriteLine($"  Дата создания: {fi.CreationTime}");
                    Debug.WriteLine($"  Расширение: {fi.Extension}");
                }
                else
                {
                    Debug.WriteLine($"Файл не найден! Будет создан тестовый.");
                    UpdateStatus("Фон не найден, создаем тестовый");
                }

                // Меняем обои
                Debug.WriteLine("Вызываем ChangeWallpaper...");
                bool wallpaperChanged = _wallpaperService.ChangeWallpaper(bgPath);

                if (wallpaperChanged)
                {
                    Debug.WriteLine($"✓ Обои успешно изменены на: {Path.GetFileName(bgPath)}");
                    UpdateStatus($"Обои изменены: {Path.GetFileName(bgPath)}");

                    // Проверяем, действительно ли поменялись обои
                    CheckIfWallpaperChanged(bgPath);
                }
                else
                {
                    Debug.WriteLine($"✗ Не удалось изменить обои");
                    UpdateStatus("Не удалось изменить обои");

                    // Пробуем альтернативный способ
                    TryAlternativeWallpaperChange(bgPath);
                }

                // Создаем нового питомца С НАСТРОЙКАМИ
                try
                {
                    Debug.WriteLine($"Создаем PetAI с настройками: ExploreWaitTime={_settings.ExploreWaitTime}, FreeRoamEnabled={_settings.FreeRoamEnabled}");
                    _currentPet = new PetAI(skin.Id, _settings);
                    _currentPet.Start();
                    // ... остальной код
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"✗ Ошибка запуска питомца: {ex.Message}");
                    UpdateStatus($"Ошибка запуска питомца: {ex.Message}");
                    MessageBox.Show($"Не удалось запустить питомца: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Логируем активацию
                _logService.LogActivity(skin.Id, $"Скин '{skin.Name}' активирован");

                // Обновляем текущий скин в настройках
                _settings.CurrentSkinId = skin.Id;
                SaveSettings();

                // Скрываем окно
                if (_settings.MinimizeToTray)
                {
                    this.Hide();
                    this.ShowInTaskbar = false;
                    UpdateStatus("Приложение свернуто в трей");
                }

                MessageBox.Show($"Скин '{skin.Name}' активирован!\n" +
                               $"Приложение {(_settings.MinimizeToTray ? "свернуто в трей" : "осталось открытым")}\n" +
                               $"\nФон: {Path.GetFileName(bgPath)}\n" +
                               $"Место отдыха находится в правом нижнем углу экрана",
                               "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Debug.WriteLine($"=== АКТИВАЦИЯ ЗАВЕРШЕНА ===");
            }
        }
        public void ApplyRestZoneSettings(int x, int y, int width, int height)
        {
            // Обновляем настройки
            _settings.RestZoneX = x;
            _settings.RestZoneY = y;
            _settings.RestZoneWidth = width;
            _settings.RestZoneHeight = height;

            // Сохраняем в файл
            SaveSettings();

            // Если есть активный питомец - перезапускаем его с новыми настройками
            if (_currentPet != null && _settings.CurrentSkinId > 0)
            {
                // Останавливаем текущего
                _currentPet.Stop();
                _currentPet = null;

                // Запускаем нового с обновленными настройками
                try
                {
                    _currentPet = new PetAI(_settings.CurrentSkinId, _settings);
                    _currentPet.Start();
                    UpdateStatus("Зона отдыха обновлена");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка перезапуска питомца: {ex.Message}");
                    UpdateStatus($"Ошибка: {ex.Message}");
                }
            }
        }
        private void CheckIfWallpaperChanged(string expectedPath)
        {
            try
            {
                Debug.WriteLine("=== ПРОВЕРКА ИЗМЕНЕНИЯ ОБОЕВ ===");

                // Читаем текущие обои из реестра
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop"))
                {
                    if (key != null)
                    {
                        string currentWallpaper = key.GetValue("Wallpaper") as string;
                        Debug.WriteLine($"Текущие обои в реестре: {currentWallpaper}");
                        Debug.WriteLine($"Ожидаемые обои: {expectedPath}");

                        if (!string.IsNullOrEmpty(currentWallpaper))
                        {
                            if (currentWallpaper.Equals(expectedPath, StringComparison.OrdinalIgnoreCase))
                            {
                                Debug.WriteLine("✓ Обои успешно изменены (проверено через реестр)");
                            }
                            else
                            {
                                Debug.WriteLine("✗ Обои не изменились или изменились на другой файл");
                                Debug.WriteLine($"  Разница: {currentWallpaper} != {expectedPath}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка проверки обоев: {ex.Message}");
            }
        }

        private void TryAlternativeWallpaperChange(string imagePath)
        {
            Debug.WriteLine("=== ПРОБУЕМ АЛЬТЕРНАТИВНЫЙ СПОСОБ ИЗМЕНЕНИЯ ОБОЕВ ===");

            try
            {
                // Способ 1: Через Process.Start
                Debug.WriteLine("Способ 1: Через Process.Start...");
                System.Diagnostics.Process.Start("rundll32.exe", $"user32.dll,UpdatePerUserSystemParameters 1, {imagePath}");
                Debug.WriteLine("Команда выполнена");

                // Способ 2: Через PowerShell
                Debug.WriteLine("Способ 2: Через PowerShell...");
                string powerShellCommand = $"powershell -command \"Set-ItemProperty -Path 'HKCU:\\Control Panel\\Desktop' -Name Wallpaper -Value '{imagePath}'; rundll32.exe user32.dll, UpdatePerUserSystemParameters 1, True\"";
                System.Diagnostics.Process.Start("cmd.exe", $"/c {powerShellCommand}");
                Debug.WriteLine("PowerShell команда выполнена");

                UpdateStatus("Использован альтернативный способ смены обоев");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка альтернативного способа: {ex.Message}");
            }
        }

        private string GetBackgroundPath(int skinId)
        {
            Debug.WriteLine($"=== ПОИСК ФОНА ДЛЯ СКИНА {skinId} ===");

            // Массив имен фонов в правильном порядке (по вашей нумерации)
            string[] bgNames = {
        "lizard",     // skin_1 - ящерица
        "rat",        // skin_2 - крыса
        "snake",      // skin_3 - змея
        "axolotl",    // skin_4 - аксолотль
        "cat",        // skin_5 - кот
        "ferret",     // skin_6 - хорек
        "dragon"      // skin_7 - дракон
    };

            if (skinId > 0 && skinId <= bgNames.Length)
            {
                string bgFileName = bgNames[skinId - 1] + "_bg.png";
                Debug.WriteLine($"Имя файла фона: {bgFileName}");

                // Пробуем ВСЕ возможные пути по порядку
                string[] possiblePaths = {
            // 1. Фиксированный путь
            Path.Combine(_fixedBasePath, "Backgrounds", bgFileName),
            
            // 2. Относительные пути
            Path.Combine("Backgrounds", bgFileName),
            Path.Combine(Directory.GetCurrentDirectory(), "Backgrounds", bgFileName),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backgrounds", bgFileName),
            Path.Combine(Application.StartupPath, "Backgrounds", bgFileName),
            
            // 3. Абсолютные пути
            @"C:\Users\user\Desktop\Cursor Companion\CursorCompanionFinish\CursorCompanionFinish\bin\Debug\Backgrounds\" + bgFileName,
            @"C:\Users\user\Desktop\Cursor Companion\CursorCompanionFinish\CursorCompanionFinish\Backgrounds\" + bgFileName
        };

                // Показываем все пути для отладки
                for (int i = 0; i < possiblePaths.Length; i++)
                {
                    bool exists = File.Exists(possiblePaths[i]);
                    Debug.WriteLine($"Путь {i + 1}: {possiblePaths[i]}");
                    Debug.WriteLine($"  Существует: {exists}");
                    Debug.WriteLine($"  Полный путь: {Path.GetFullPath(possiblePaths[i])}");
                }

                // Ищем первый существующий файл
                foreach (string path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        Debug.WriteLine($"✓ Найден фон по пути: {path}");
                        Debug.WriteLine($"  Размер файла: {new FileInfo(path).Length} байт");
                        return path;
                    }
                }

                Debug.WriteLine($"✗ Фон {bgFileName} не найден ни по одному пути!");

                // Если фон не найден - создаем тестовый
                string testPath = Path.Combine(_fixedBasePath, "Backgrounds", bgFileName);
                CreateTestBackgroundForSkin(skinId, testPath);
                return testPath;
            }

            Debug.WriteLine($"✗ Неверный skinId: {skinId}");
            return Path.Combine(_fixedBasePath, "Backgrounds", "default_bg.png");
        }

        private void CreateTestBackgroundForSkin(int skinId, string outputPath)
        {
            try
            {
                Debug.WriteLine($"Создаю тестовый фон для скина {skinId} по пути: {outputPath}");

                // Убедимся, что папка существует
                string directory = Path.GetDirectoryName(outputPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Debug.WriteLine($"Создана папка: {directory}");
                }

                string skinName = "Неизвестный скин";
                Color themeColor = Color.Purple;

                // Определяем имя и цвет по skinId
                switch (skinId)
                {
                    case 1: skinName = "Ящерица"; themeColor = Color.FromArgb(76, 175, 80); break;
                    case 2: skinName = "Крыса"; themeColor = Color.FromArgb(121, 85, 72); break;
                    case 3: skinName = "Змея"; themeColor = Color.FromArgb(139, 195, 74); break;
                    case 4: skinName = "Аксолотль"; themeColor = Color.FromArgb(33, 150, 243); break;
                    case 5: skinName = "Кот"; themeColor = Color.FromArgb(158, 158, 158); break;
                    case 6: skinName = "Хорек"; themeColor = Color.FromArgb(255, 152, 0); break;
                    case 7: skinName = "Дракон"; themeColor = Color.FromArgb(233, 30, 99); break;
                }

                using (Bitmap bitmap = new Bitmap(1920, 1080))
                using (Graphics graphics = Graphics.FromImage(bitmap))
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    new Point(0, 0),
                    new Point(1920, 1080),
                    Color.FromArgb(30, 30, 40),
                    themeColor))
                {
                    graphics.FillRectangle(brush, 0, 0, 1920, 1080);

                    // Заголовок
                    using (Font titleFont = new Font("Arial", 72, FontStyle.Bold))
                    using (SolidBrush titleBrush = new SolidBrush(Color.White))
                    {
                        graphics.DrawString(skinName, titleFont, titleBrush, 100, 100);
                    }

                    // Информация
                    using (Font infoFont = new Font("Arial", 36))
                    using (SolidBrush infoBrush = new SolidBrush(Color.FromArgb(220, 255, 255, 255)))
                    {
                        graphics.DrawString("Cursor Companion", infoFont, infoBrush, 100, 220);
                        graphics.DrawString($"Скин ID: {skinId}", infoFont, infoBrush, 100, 280);
                        graphics.DrawString("Тестовый фон", infoFont, infoBrush, 100, 340);
                        graphics.DrawString(DateTime.Now.ToString("dd.MM.yyyy HH:mm"), infoFont, infoBrush, 100, 400);
                    }

                    // Сохраняем
                    bitmap.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);
                    Debug.WriteLine($"✓ Создан тестовый фон: {outputPath}");
                    Debug.WriteLine($"  Размер: {bitmap.Width}x{bitmap.Height}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✗ Ошибка создания тестового фона: {ex.Message}");
            }
        }

        private void CreateTestBackground(Models.PetSkin skin)
        {
            string bgPath = GetBackgroundPath(skin.Id);
            string bgDir = Path.GetDirectoryName(bgPath);

            try
            {
                if (!Directory.Exists(bgDir))
                {
                    Directory.CreateDirectory(bgDir);
                }

                using (Bitmap bitmap = new Bitmap(1920, 1080))
                using (Graphics graphics = Graphics.FromImage(bitmap))
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    new Point(0, 0),
                    new Point(1920, 1080),
                    Color.FromArgb(30, 30, 40),
                    skin.ThemeColor))
                {
                    graphics.FillRectangle(brush, 0, 0, 1920, 1080);

                    // Название скина
                    using (Font font = new Font("Arial", 48, FontStyle.Bold))
                    using (SolidBrush textBrush = new SolidBrush(Color.White))
                    {
                        graphics.DrawString(skin.Name, font, textBrush, 100, 100);
                    }

                    // Инструкция
                    using (Font font = new Font("Arial", 24))
                    using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
                    {
                        graphics.DrawString("Cursor Companion", font, textBrush, 100, 200);
                        graphics.DrawString("Двойной клик по месту отдыха → сон", font, textBrush, 100, 250);
                        graphics.DrawString("Правый нижний угол экрана", font, textBrush, 100, 300);
                    }

                    // Сохраняем как PNG
                    bitmap.Save(bgPath, System.Drawing.Imaging.ImageFormat.Png);
                    Debug.WriteLine("Создан тестовый фон: " + bgPath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка создания фона: " + ex.Message);
            }
        }

        private void NavigationButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button != null && button.Tag != null)
            {
                string tag = button.Tag.ToString();

                switch (tag)
                {
                    case "gallery":
                        ShowGallery();
                        break;
                    case "settings":
                        ShowSettings();
                        break;
                    case "stats":
                        ShowStatistics();
                        break;
                    case "help":
                        ShowHelp();
                        break;
                    case "about":
                        ShowAbout();
                        break;
                }
            }
        }

        private void ShowGallery()
        {
            InitializeGallery();
            if (skinListBox != null && _settings.CurrentSkinId > 0 && _skins != null && _settings.CurrentSkinId <= _skins.Count)
            {
                skinListBox.SelectedIndex = _settings.CurrentSkinId - 1;
            }
            UpdateStatus("Открыта галерея скинов");
        }

        private void ShowSettings()
        {
            SettingsForm settingsForm = new SettingsForm(_settings);
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                _settings = settingsForm.GetSettings();
                SaveSettings();
                UpdateStatus("Настройки сохранены");

                // Обновляем настройки текущего питомца
                if (_currentPet != null)
                {
                    _currentPet.SetIdleTimeout(_settings.IdleTimeout);
                    _currentPet.SetFreeRoam(_settings.FreeRoamEnabled);
                }
            }
        }

        private void ShowStatistics()
        {
            contentPanel.Controls.Clear();

            Label lblTitle = new Label();
            lblTitle.Text = "📊 СТАТИСТИКА";
            lblTitle.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(30, 30);
            lblTitle.AutoSize = true;
            contentPanel.Controls.Add(lblTitle);

            TextBox statsText = new TextBox();
            statsText.Multiline = true;
            statsText.ReadOnly = true;
            statsText.Size = new Size(500, 300);
            statsText.Location = new Point(30, 80);
            statsText.BackColor = Color.FromArgb(45, 45, 48);
            statsText.ForeColor = Color.White;
            statsText.Font = new Font("Consolas", 10);
            statsText.ScrollBars = ScrollBars.Vertical;

            // Загружаем статистику для текущего скина
            statsText.Text = _logService.GetStatistics(_settings.CurrentSkinId);
            contentPanel.Controls.Add(statsText);

            Button btnBack = new Button();
            btnBack.Text = "← Назад в галерею";
            btnBack.Size = new Size(150, 40);
            btnBack.Location = new Point(30, 400);
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.ForeColor = Color.White;
            btnBack.BackColor = Color.FromArgb(64, 64, 64);
            btnBack.Click += (s, e) => ShowGallery();
            contentPanel.Controls.Add(btnBack);

            UpdateStatus("Открыта статистика");
        }

        private void ShowHelp()
        {
            HelpForm helpForm = new HelpForm();
            helpForm.ShowDialog();
            UpdateStatus("Открыта справка");
        }

        private void ShowAbout()
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.ShowDialog();
            UpdateStatus("Открыто окно 'О программе'");
        }

        private void ApplySettings()
        {
            SaveSettings();
            MessageBox.Show("Настройки сохранены!", "Успех",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            UpdateStatus("Настройки применены");
        }

        private void MinimizeToTray()
        {
            this.Hide();
            this.ShowInTaskbar = false;
            UpdateStatus("Приложение свернуто в трей");
        }

        private void ExitApplication()
        {
            // Восстанавливаем оригинальные обои
            if (_wallpaperService != null)
            {
                _wallpaperService.RestoreOriginalWallpaper();
            }
            else
            {
                // Если сервис не инициализирован, создаем новый
                Services.WallpaperService tempService = new Services.WallpaperService();
                tempService.RestoreOriginalWallpaper();
            }

            // Останавливаем питомца
            if (_currentPet != null)
            {
                _currentPet.Stop();
                _currentPet = null;
            }

            // Скрываем иконку в трее
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }

            // Закрываем главное окно (программа продолжится, так как есть окно входа)
            this.Close();

            // Если это последнее окно - выходим из приложения
            if (Application.OpenForms.Count == 0)
            {
                Application.Exit();
            }
        }

        private void UpdateStatus(string message)
        {
            if (lblStatus != null && !lblStatus.IsDisposed)
            {
                lblStatus.Text = message;
                Debug.WriteLine("Статус: " + message);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                MinimizeToTray();
            }
            else
            {
                // Если окно закрывается по другой причине (например, при выходе)
                // Восстанавливаем обои
                if (_wallpaperService != null)
                {
                    _wallpaperService.RestoreOriginalWallpaper();
                }
            }
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_trayIcon != null)
                {
                    _trayIcon.Dispose();
                }
                if (_currentPet != null)
                {
                    _currentPet.Dispose();
                }
                // Гарантируем восстановление обоев
                if (_wallpaperService != null)
                {
                    _wallpaperService.RestoreOriginalWallpaper();
                }
            }
            base.Dispose(disposing);
        }
    }
}