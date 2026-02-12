using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Models = CursorCompanionFinish.Models;
using Services = CursorCompanionFinish.Services;

namespace CursorCompanionFinish
{
    public partial class MainForm : Form
    {
        private Services.WallpaperService _wallpaperService;
        private Services.LogService _logService;
        private Models.Settings _settings;
        private List<Models.PetSkin> _skins;
        private PetAI _currentPet;
        private NotifyIcon _trayIcon;
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
            _fixedBasePath = Application.StartupPath;

            Debug.WriteLine("==========================================");
            Debug.WriteLine("НАЧАЛО РАБОТЫ ПРОГРАММЫ");
            Debug.WriteLine("Application.StartupPath: " + Application.StartupPath);
            Debug.WriteLine("==========================================");

            CreateFolders();
            CheckFiles();

            // InitializeComponent(); // ← ЭТО УДАЛИТЬ! ОН В .Designer.cs!

            InitializeServices();
            InitializeUI();
            LoadSkins();
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
            _settings = new Models.Settings();
        }

        // ========== ЕДИНЫЙ СТИЛЬ: МЕТОД СОЗДАНИЯ КНОПОК ==========
        private Button CreateNavButton(string text)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.FlatStyle = FlatStyle.Flat;
            btn.ForeColor = Color.White;
            btn.BackColor = Color.Transparent;
            btn.Size = new Size(150, 45);
            btn.Margin = new Padding(0, 0, 0, 10);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;

            // Закругление
            MakeButtonRounded(btn, 12);

            // Эффект при наведении
            btn.MouseEnter += (s, e) =>
            {
                if (btn.BackColor == Color.Transparent)
                    btn.BackColor = Color.FromArgb(60, 60, 70);
            };
            btn.MouseLeave += (s, e) =>
            {
                if (btn.BackColor != Color.FromArgb(154, 75, 255) &&
                    btn.BackColor != Color.FromArgb(59, 200, 207) &&
                    btn.BackColor != Color.FromArgb(254, 185, 182))
                    btn.BackColor = Color.Transparent;
            };

            return btn;
        }

        // ========== МЕТОД ЗАКРУГЛЕНИЯ КНОПОК ==========
        private void MakeButtonRounded(Button button, int radius)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;

            var path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(button.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(button.Width - radius, button.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, button.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            button.Region = new Region(path);

            button.SizeChanged += (s, e) =>
            {
                var btn = s as Button;
                if (btn == null) return;

                var newPath = new GraphicsPath();
                newPath.AddArc(0, 0, radius, radius, 180, 90);
                newPath.AddArc(btn.Width - radius, 0, radius, radius, 270, 90);
                newPath.AddArc(btn.Width - radius, btn.Height - radius, radius, radius, 0, 90);
                newPath.AddArc(0, btn.Height - radius, radius, radius, 90, 90);
                newPath.CloseFigure();
                btn.Region = new Region(newPath);
            };
        }

        private void InitializeUI()
        {
            // Настройка формы
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
            navigationPanel.Padding = new Padding(15, 20, 15, 20);

            // FlowLayoutPanel для автоматического расположения кнопок
            FlowLayoutPanel flowPanel = new FlowLayoutPanel();
            flowPanel.Dock = DockStyle.Fill;
            flowPanel.FlowDirection = FlowDirection.TopDown;
            flowPanel.WrapContents = false;
            flowPanel.AutoScroll = true;
            flowPanel.BackColor = Color.Transparent;

            // 1. Кнопка "Проверить фоны" - УВЕЛИЧИЛИ ШИРИНУ
            Button btnCheckBackgrounds = CreateNavButton("🎨 Проверить\nфоны"); // перенос строки
            btnCheckBackgrounds.Size = new Size(150, 50); // шире и выше
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

            // Панель для нижних кнопок
            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 160;
            bottomPanel.BackColor = Color.Transparent;

            // Кнопка "Применить" - ФИОЛЕТОВАЯ
            Button btnApply = new Button();
            btnApply.Text = "✓ Применить";
            btnApply.Size = new Size(150, 45);
            btnApply.Location = new Point(0, 5);
            btnApply.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnApply.FlatStyle = FlatStyle.Flat;
            btnApply.FlatAppearance.BorderSize = 0;
            btnApply.BackColor = Color.FromArgb(154, 75, 255); // ФИОЛЕТОВЫЙ
            btnApply.ForeColor = Color.White;
            btnApply.Cursor = Cursors.Hand;
            MakeButtonRounded(btnApply, 15);
            btnApply.Click += (s, e) => ApplySettings();
            bottomPanel.Controls.Add(btnApply);

            // Кнопка "Свернуть" - БИРЮЗОВАЯ
            Button btnMinimize = new Button();
            btnMinimize.Text = "▼ Свернуть";
            btnMinimize.Size = new Size(150, 45);
            btnMinimize.Location = new Point(0, 60);
            btnMinimize.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnMinimize.FlatStyle = FlatStyle.Flat;
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.BackColor = Color.FromArgb(59, 200, 207); // БИРЮЗОВЫЙ
            btnMinimize.ForeColor = Color.White;
            btnMinimize.Cursor = Cursors.Hand;
            MakeButtonRounded(btnMinimize, 15);
            btnMinimize.Click += (s, e) => MinimizeToTray();
            bottomPanel.Controls.Add(btnMinimize);

            // Кнопка "На главную" - ПАСТЕЛЬНО-РОЗОВАЯ
            Button btnHome = new Button();
            btnHome.Text = "🏠 На главную";
            btnHome.Size = new Size(150, 45);
            btnHome.Location = new Point(0, 115);
            btnHome.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnHome.FlatStyle = FlatStyle.Flat;
            btnHome.FlatAppearance.BorderSize = 0;
            btnHome.BackColor = Color.FromArgb(254, 185, 182); // ПАСТЕЛЬНО-РОЗОВЫЙ
            btnHome.ForeColor = Color.FromArgb(44, 62, 80); // ТЁМНО-СИНИЙ
            btnHome.Cursor = Cursors.Hand;
            MakeButtonRounded(btnHome, 15);
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

            InitializeMenu();
            ShowGallery();
        }

        private void GoToHome()
        {
            if (_wallpaperService != null)
            {
                _wallpaperService.RestoreOriginalWallpaper();
            }

            if (_currentPet != null)
            {
                _currentPet.Stop();
                _currentPet = null;
            }

            this.Close();

            SplashForm splashForm = new SplashForm();
            splashForm.Show();
        }

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

            ToolStripMenuItem fileMenu = new ToolStripMenuItem("Файл");
            fileMenu.DropDownItems.Add("Свернуть в трей", null, (s, e) => MinimizeToTray());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Выход", null, (s, e) => ExitApplication());

            ToolStripMenuItem viewMenu = new ToolStripMenuItem("Вид");
            viewMenu.DropDownItems.Add("Галерея", null, (s, e) => ShowGallery());
            viewMenu.DropDownItems.Add("Настройки", null, (s, e) => ShowSettings());

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

            // ФИОЛЕТОВЫЙ ФОН
            contentPanel.BackColor = Color.FromArgb(197, 149, 255);

            // Градиент
            contentPanel.Paint += (sender, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    contentPanel.ClientRectangle,
                    Color.FromArgb(197, 149, 255),
                    Color.FromArgb(217, 169, 255),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, contentPanel.ClientRectangle);
                }
            };

            // Заголовок
            Label lblTitle = new Label();
            lblTitle.Text = "🎮 ВЫБЕРИТЕ ПИТОМЦА";
            lblTitle.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(44, 62, 80);
            lblTitle.Location = new Point(30, 30);
            lblTitle.AutoSize = true;
            lblTitle.BackColor = Color.Transparent;
            contentPanel.Controls.Add(lblTitle);

            // Список скинов
            skinListBox = new ListBox();
            skinListBox.Size = new Size(250, 400);
            skinListBox.Location = new Point(30, 80);
            skinListBox.BackColor = Color.White;
            skinListBox.ForeColor = Color.FromArgb(44, 62, 80);
            skinListBox.BorderStyle = BorderStyle.FixedSingle;
            skinListBox.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            skinListBox.SelectedIndexChanged += SkinListBox_SelectedIndexChanged;
            contentPanel.Controls.Add(skinListBox);

            // ========== ПРЕВЬЮ С ЗАКРУГЛЁННЫМИ УГЛАМИ ==========
            skinPreview = new PictureBox();
            skinPreview.Size = new Size(400, 300);
            skinPreview.Location = new Point(293, 80);
            skinPreview.BorderStyle = BorderStyle.None;
            skinPreview.SizeMode = PictureBoxSizeMode.Zoom;
            skinPreview.BackColor = Color.FromArgb(45, 45, 48); // ТЁМНО-СЕРЫЙ
            skinPreview.Padding = new Padding(10);

            // --- ЗАКРУГЛЯЕМ (радиус 10) ---
            MakePictureBoxRounded(skinPreview, 10);

            contentPanel.Controls.Add(skinPreview);

            // Описание
            skinDescription = new Label();
            skinDescription.Location = new Point(293, 390);
            skinDescription.Size = new Size(400, 80);
            skinDescription.ForeColor = Color.FromArgb(44, 62, 80);
            skinDescription.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            skinDescription.Text = "Выберите скин для просмотра";
            skinDescription.BackColor = Color.Transparent;
            contentPanel.Controls.Add(skinDescription);

            // Кнопка активации
            btnActivate = new Button();
            btnActivate.Text = "🎮 АКТИВИРОВАТЬ";
            btnActivate.Size = new Size(180, 50);
            btnActivate.Location = new Point(293, 480);
            btnActivate.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnActivate.BackColor = Color.FromArgb(154, 75, 255);
            btnActivate.ForeColor = Color.White;
            btnActivate.FlatStyle = FlatStyle.Flat;
            btnActivate.FlatAppearance.BorderSize = 0;
            btnActivate.Cursor = Cursors.Hand;
            MakeButtonRounded(btnActivate, 18);
            btnActivate.Click += BtnActivate_Click;
            contentPanel.Controls.Add(btnActivate);

            // Убираем обработчик Paint
            contentPanel.Paint -= contentPanel_Paint;

            LoadSkinsToListBox();
        }


        // Заглушка для удаления обработчика
        private void contentPanel_Paint(object sender, PaintEventArgs e) { }

        private void LoadSkins()
        {
            try
            {
                Debug.WriteLine("Начинаем загрузку скинов...");

                _skins = new List<Models.PetSkin>();

                for (int i = 1; i <= 7; i++)
                {
                    Models.PetSkin skin = new Models.PetSkin();
                    skin.Id = i;
                    skin.IsUnlocked = true;

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
                    skin.BasePosition = new Point(1000, 550);

                    _skins.Add(skin);
                    Debug.WriteLine($"Добавлен скин: {skin.Name} (ID: {skin.Id})");
                }

                Debug.WriteLine($"Всего загружено скинов: {_skins.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ОШИБКА в LoadSkins: {ex.Message}");
                _skins = new List<Models.PetSkin>();
            }
        }

        private string GetSkinDescription(int skinId)
        {
            switch (skinId)
            {
                case 1: return "Ящерица с цепким хвостом. Любит греться на камнях.";
                case 2: return "Шустрая крыска с длинным хвостом. Любит исследовать новые места.";
                case 3: return "Голубая змейка. Действительно редкий внешний вид!";
                case 4: return "Розовый аксолотль. Жить не может без воды!";
                case 5: return "Ленивый котик. Часто засыпает в своем домике, мягко мурча.";
                case 6: return "Веселый хорёк. Любит играть и бегать по экрану.";
                case 7: return "Золотой дракон. Иногда вы можете увидеть его на ночном небе.";
                default: return "Интерактивный питомец для вашего курсора!";
            }
        }

        private Color GetThemeColor(int skinId)
        {
            switch (skinId)
            {
                case 1: return Color.FromArgb(76, 175, 80);
                case 2: return Color.FromArgb(121, 85, 72);
                case 3: return Color.FromArgb(139, 195, 74);
                case 4: return Color.FromArgb(33, 150, 243);
                case 5: return Color.FromArgb(158, 158, 158);
                case 6: return Color.FromArgb(255, 152, 0);
                case 7: return Color.FromArgb(233, 30, 99);
                default: return Color.Purple;
            }
        }

        private void LoadSkinsToListBox()
        {
            if (skinListBox == null)
            {
                Debug.WriteLine("Ошибка: skinListBox не инициализирован");
                return;
            }

            if (_skins == null)
            {
                Debug.WriteLine("_skins равен null, вызываем LoadSkins()");
                LoadSkins();
            }

            try
            {
                skinListBox.Items.Clear();

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

            if (skinPreview != null)
            {
                
            }

            if (skinDescription != null)
            {
                skinDescription.Text = skin.Description + "\n\nЦвет темы: " + skin.ThemeColor.Name;
            }

            string relativePath = Path.Combine("Skins", "skin_" + skin.Id.ToString(), "preview.png");
            string imagePath = GetCorrectPath(relativePath);

            Debug.WriteLine($"Попытка загрузить превью: {imagePath}");
            Debug.WriteLine($"Файл существует: {File.Exists(imagePath)}");

            if (File.Exists(imagePath))
            {
                try
                {
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
            Bitmap bmp = new Bitmap(400, 300);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (LinearGradientBrush brush = new LinearGradientBrush(
                    new Point(0, 0),
                    new Point(400, 300),
                    Color.White,
                    skin.ThemeColor))
                {
                    g.FillRectangle(brush, 0, 0, 400, 300);
                }

                using (Font font = new Font("Segoe UI", 24, FontStyle.Bold))
                using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(44, 62, 80)))
                {
                    g.DrawString(skin.Name, font, textBrush, 50, 100);
                }

                using (Font font = new Font("Segoe UI", 12))
                using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(44, 62, 80)))
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

                if (_currentPet != null)
                {
                    _currentPet.Stop();
                    _currentPet = null;
                    UpdateStatus("Текущий питомец остановлен");
                }

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

                Debug.WriteLine("Вызываем ChangeWallpaper...");
                bool wallpaperChanged = _wallpaperService.ChangeWallpaper(bgPath);

                if (wallpaperChanged)
                {
                    Debug.WriteLine($"✓ Обои успешно изменены на: {Path.GetFileName(bgPath)}");
                    UpdateStatus($"Обои изменены: {Path.GetFileName(bgPath)}");
                    CheckIfWallpaperChanged(bgPath);
                }
                else
                {
                    Debug.WriteLine($"✗ Не удалось изменить обои");
                    UpdateStatus("Не удалось изменить обои");
                    TryAlternativeWallpaperChange(bgPath);
                }

                try
                {
                    Debug.WriteLine($"Создаем PetAI с настройками: ExploreWaitTime={_settings.ExploreWaitTime}, FreeRoamEnabled={_settings.FreeRoamEnabled}");
                    _currentPet = new PetAI(skin.Id, _settings);
                    _currentPet.Start();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"✗ Ошибка запуска питомца: {ex.Message}");
                    UpdateStatus($"Ошибка запуска питомца: {ex.Message}");
                    MessageBox.Show($"Не удалось запустить питомца: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _logService.LogActivity(skin.Id, $"Скин '{skin.Name}' активирован");

                _settings.CurrentSkinId = skin.Id;
                SaveSettings();

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
            _settings.RestZoneX = x;
            _settings.RestZoneY = y;
            _settings.RestZoneWidth = width;
            _settings.RestZoneHeight = height;

            SaveSettings();

            if (_currentPet != null && _settings.CurrentSkinId > 0)
            {
                _currentPet.Stop();
                _currentPet = null;

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
                Debug.WriteLine("Способ 1: Через Process.Start...");
                System.Diagnostics.Process.Start("rundll32.exe", $"user32.dll,UpdatePerUserSystemParameters 1, {imagePath}");
                Debug.WriteLine("Команда выполнена");

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

            string[] bgNames = {
                "lizard", "rat", "snake", "axolotl", "cat", "ferret", "dragon"
            };

            if (skinId > 0 && skinId <= bgNames.Length)
            {
                string bgFileName = bgNames[skinId - 1] + "_bg.png";
                Debug.WriteLine($"Имя файла фона: {bgFileName}");

                string[] possiblePaths = {
                    Path.Combine(_fixedBasePath, "Backgrounds", bgFileName),
                    Path.Combine("Backgrounds", bgFileName),
                    Path.Combine(Directory.GetCurrentDirectory(), "Backgrounds", bgFileName),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backgrounds", bgFileName),
                    Path.Combine(Application.StartupPath, "Backgrounds", bgFileName),
                    @"C:\Users\user\Desktop\Cursor Companion\CursorCompanionFinish\CursorCompanionFinish\bin\Debug\Backgrounds\" + bgFileName,
                    @"C:\Users\user\Desktop\Cursor Companion\CursorCompanionFinish\CursorCompanionFinish\Backgrounds\" + bgFileName
                };

                for (int i = 0; i < possiblePaths.Length; i++)
                {
                    bool exists = File.Exists(possiblePaths[i]);
                    Debug.WriteLine($"Путь {i + 1}: {possiblePaths[i]}");
                    Debug.WriteLine($"  Существует: {exists}");
                }

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

                string directory = Path.GetDirectoryName(outputPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Debug.WriteLine($"Создана папка: {directory}");
                }

                string skinName = "Неизвестный скин";
                Color themeColor = Color.Purple;

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

                    using (Font titleFont = new Font("Arial", 72, FontStyle.Bold))
                    using (SolidBrush titleBrush = new SolidBrush(Color.White))
                    {
                        graphics.DrawString(skinName, titleFont, titleBrush, 100, 100);
                    }

                    using (Font infoFont = new Font("Arial", 36))
                    using (SolidBrush infoBrush = new SolidBrush(Color.FromArgb(220, 255, 255, 255)))
                    {
                        graphics.DrawString("Cursor Companion", infoFont, infoBrush, 100, 220);
                        graphics.DrawString($"Скин ID: {skinId}", infoFont, infoBrush, 100, 280);
                        graphics.DrawString("Тестовый фон", infoFont, infoBrush, 100, 340);
                        graphics.DrawString(DateTime.Now.ToString("dd.MM.yyyy HH:mm"), infoFont, infoBrush, 100, 400);
                    }

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
            contentPanel.BackColor = Color.FromArgb(197, 149, 255); // ФИОЛЕТОВЫЙ

            Label lblTitle = new Label();
            lblTitle.Text = "📊 СТАТИСТИКА";
            lblTitle.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(44, 62, 80); // ТЁМНО-СИНИЙ
            lblTitle.Location = new Point(30, 30);
            lblTitle.AutoSize = true;
            lblTitle.BackColor = Color.Transparent;
            contentPanel.Controls.Add(lblTitle);

            TextBox statsText = new TextBox();
            statsText.Multiline = true;
            statsText.ReadOnly = true;
            statsText.Size = new Size(500, 300);
            statsText.Location = new Point(30, 80);
            statsText.BackColor = Color.White;
            statsText.ForeColor = Color.FromArgb(44, 62, 80);
            statsText.Font = new Font("Consolas", 10);
            statsText.ScrollBars = ScrollBars.Vertical;

            statsText.Text = _logService.GetStatistics(_settings.CurrentSkinId);
            contentPanel.Controls.Add(statsText);

            Button btnBack = new Button();
            btnBack.Text = "← Назад в галерею";
            btnBack.Size = new Size(200, 40);
            btnBack.Location = new Point(30, 400);
            btnBack.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.ForeColor = Color.White;
            btnBack.BackColor = Color.FromArgb(154, 75, 255); // ФИОЛЕТОВЫЙ
            btnBack.Cursor = Cursors.Hand;
            MakeButtonRounded(btnBack, 15);
            btnBack.Click += (s, e) => ShowGallery();
            contentPanel.Controls.Add(btnBack);

            UpdateStatus("Открыта статистика");
        }

       

        /// <summary>
        /// Делает PictureBox со скруглёнными углами
        /// </summary>
        /// <param name="pictureBox">PictureBox</param>
        /// <param name="radius">Радиус скругления в пикселях</param>
        private void MakePictureBoxRounded(PictureBox pictureBox, int radius)
{
    // Создаём путь со скруглёнными углами
    var path = new GraphicsPath();
    path.AddArc(0, 0, radius, radius, 180, 90);
    path.AddArc(pictureBox.Width - radius, 0, radius, radius, 270, 90);
    path.AddArc(pictureBox.Width - radius, pictureBox.Height - radius, radius, radius, 0, 90);
    path.AddArc(0, pictureBox.Height - radius, radius, radius, 90, 90);
    path.CloseFigure();
    
    // Применяем регион
    pictureBox.Region = new Region(path);
    
    // Подписываемся на изменение размера
    pictureBox.SizeChanged += (s, e) =>
    {
        var pb = s as PictureBox;
        if (pb == null) return;
        
        var newPath = new GraphicsPath();
        newPath.AddArc(0, 0, radius, radius, 180, 90);
        newPath.AddArc(pb.Width - radius, 0, radius, radius, 270, 90);
        newPath.AddArc(pb.Width - radius, pb.Height - radius, radius, radius, 0, 90);
        newPath.AddArc(0, pb.Height - radius, radius, radius, 90, 90);
        newPath.CloseFigure();
        pb.Region = new Region(newPath);
    };
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
            if (_wallpaperService != null)
            {
                _wallpaperService.RestoreOriginalWallpaper();
            }
            else
            {
                Services.WallpaperService tempService = new Services.WallpaperService();
                tempService.RestoreOriginalWallpaper();
            }

            if (_currentPet != null)
            {
                _currentPet.Stop();
                _currentPet = null;
            }

            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }

            this.Close();

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
                if (_wallpaperService != null)
                {
                    _wallpaperService.RestoreOriginalWallpaper();
                }
            }
            base.Dispose(disposing);
        }
    }
}