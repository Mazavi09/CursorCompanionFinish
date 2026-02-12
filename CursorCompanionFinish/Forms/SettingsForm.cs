using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Models = CursorCompanionFinish.Models;

namespace CursorCompanionFinish
{
    public partial class SettingsForm : Form
    {
        private Models.Settings _settings;

        // Элементы управления
        private NumericUpDown numIdleTimeout;
        private CheckBox cbFreeRoam;
        private TrackBar trackRoamSpeed;
        private CheckBox cbAutoReturn;
        private CheckBox cbMinimizeToTray;
        private CheckBox cbEnableSounds;
        private CheckBox cbReactToClicks;
        private NumericUpDown numPosX;
        private NumericUpDown numPosY;
        private NumericUpDown numWidth;
        private NumericUpDown numHeight;
        private Label lblExploreWait;
        private NumericUpDown numExploreWait;

        public SettingsForm(Models.Settings settings)
        {
            _settings = settings;
            InitializeComponent();
            InitializeUI();
            LoadSettingsToUI();
        }

        // ========== ОСНОВНОЙ МЕТОД ОТРИСОВКИ ==========
        private void InitializeUI()
        {
            // --- ОКНО ---
            this.Text = "Cursor Companion — Настройки";
            this.Size = new Size(550, 720);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            // ВАШ НОВЫЙ ФОН (светло-жёлтый)
            this.BackColor = Color.FromArgb(255, 243, 158);

            // --- ЗАГОЛОВОК "НАСТРОЙКИ ПОВЕДЕНИЯ" ---
            Label lblTitle1 = new Label();
            lblTitle1.Text = "⚙ НАСТРОЙКИ ПОВЕДЕНИЯ";
            lblTitle1.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTitle1.ForeColor = Color.FromArgb(255, 103, 168); // розовый
            lblTitle1.Location = new Point(20, 20);
            lblTitle1.AutoSize = true;
            this.Controls.Add(lblTitle1);

            // --- ТАЙМАУТ БЕЗДЕЙСТВИЯ ---
            Label lblIdle = new Label();
            lblIdle.Text = "Время до сна (секунд):";
            lblIdle.ForeColor = Color.FromArgb(44, 62, 80); // тёмно-синий
            lblIdle.Location = new Point(40, 70);
            lblIdle.AutoSize = true;
            this.Controls.Add(lblIdle);

            numIdleTimeout = new NumericUpDown();
            numIdleTimeout.Minimum = 5;
            numIdleTimeout.Maximum = 60;
            numIdleTimeout.Value = _settings.IdleTimeout;
            numIdleTimeout.Location = new Point(280, 68);
            numIdleTimeout.Width = 100;
            numIdleTimeout.BackColor = Color.White;
            numIdleTimeout.ForeColor = Color.FromArgb(44, 62, 80);
            this.Controls.Add(numIdleTimeout);

            // --- АВТОВОЗВРАТ ---
            cbAutoReturn = new CheckBox();
            cbAutoReturn.Text = "Автоматически возвращаться на место отдыха";
            cbAutoReturn.Checked = _settings.AutoReturnToBase;
            cbAutoReturn.ForeColor = Color.FromArgb(44, 62, 80);
            cbAutoReturn.Location = new Point(40, 110);
            cbAutoReturn.AutoSize = true;
            this.Controls.Add(cbAutoReturn);

            // --- СВОБОДНОЕ ИССЛЕДОВАНИЕ ---
            cbFreeRoam = new CheckBox();
            cbFreeRoam.Text = "Режим свободного исследования";
            cbFreeRoam.Checked = _settings.FreeRoamEnabled;
            cbFreeRoam.ForeColor = Color.FromArgb(44, 62, 80);
            cbFreeRoam.Location = new Point(40, 150);
            cbFreeRoam.AutoSize = true;
            this.Controls.Add(cbFreeRoam);

            // --- СКОРОСТЬ ИССЛЕДОВАНИЯ ---
            Label lblSpeed = new Label();
            lblSpeed.Text = "Скорость исследования:";
            lblSpeed.ForeColor = Color.FromArgb(44, 62, 80);
            lblSpeed.Location = new Point(40, 190);
            lblSpeed.AutoSize = true;
            this.Controls.Add(lblSpeed);

            trackRoamSpeed = new TrackBar();
            trackRoamSpeed.Minimum = 1;
            trackRoamSpeed.Maximum = 10;
            trackRoamSpeed.Value = _settings.RoamSpeed;
            trackRoamSpeed.Location = new Point(250, 185);
            trackRoamSpeed.Width = 150;
            trackRoamSpeed.BackColor = this.BackColor; // сливается с фоном
            this.Controls.Add(trackRoamSpeed);

            // --- ДЕКОРАТИВНАЯ ЛИНИЯ 1 (фиолетовая) ---
            Panel line1 = new Panel();
            line1.Size = new Size(500, 2);
            line1.Location = new Point(20, 235);
            line1.BackColor = Color.FromArgb(154, 75, 255);
            this.Controls.Add(line1);

            // --- ЗАГОЛОВОК "НАСТРОЙКИ ПРИЛОЖЕНИЯ" ---
            Label lblTitle2 = new Label();
            lblTitle2.Text = "⚙ НАСТРОЙКИ ПРИЛОЖЕНИЯ";
            lblTitle2.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTitle2.ForeColor = Color.FromArgb(255, 103, 168);
            lblTitle2.Location = new Point(20, 260);
            lblTitle2.AutoSize = true;
            this.Controls.Add(lblTitle2);

            // --- СВОРАЧИВАНИЕ В ТРЕЙ ---
            cbMinimizeToTray = new CheckBox();
            cbMinimizeToTray.Text = "Сворачивать в трей при активации";
            cbMinimizeToTray.Checked = _settings.MinimizeToTray;
            cbMinimizeToTray.ForeColor = Color.FromArgb(44, 62, 80);
            cbMinimizeToTray.Location = new Point(40, 300);
            cbMinimizeToTray.AutoSize = true;
            this.Controls.Add(cbMinimizeToTray);

            // --- ЗВУКИ ---
            cbEnableSounds = new CheckBox();
            cbEnableSounds.Text = "Включить звуковые эффекты";
            cbEnableSounds.Checked = _settings.EnableSounds;
            cbEnableSounds.ForeColor = Color.FromArgb(44, 62, 80);
            cbEnableSounds.Location = new Point(40, 330);
            cbEnableSounds.AutoSize = true;
            this.Controls.Add(cbEnableSounds);

            // --- РЕАКЦИЯ НА КЛИКИ ---
            cbReactToClicks = new CheckBox();
            cbReactToClicks.Text = "Реагировать на клики мыши";
            cbReactToClicks.Checked = _settings.ReactToClicks;
            cbReactToClicks.ForeColor = Color.FromArgb(44, 62, 80);
            cbReactToClicks.Location = new Point(40, 360);
            cbReactToClicks.AutoSize = true;
            this.Controls.Add(cbReactToClicks);

            // --- ДЕКОРАТИВНАЯ ЛИНИЯ 2 (фиолетовая) ---
            Panel line2 = new Panel();
            line2.Size = new Size(500, 2);
            line2.Location = new Point(20, 395);
            line2.BackColor = Color.FromArgb(154, 75, 255);
            this.Controls.Add(line2);

            // --- ЗАГОЛОВОК "ЗОНА ОТДЫХА" ---
            Label lblTitle3 = new Label();
            lblTitle3.Text = "🛏 НАСТРОЙКИ ЗОНЫ ОТДЫХА";
            lblTitle3.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTitle3.ForeColor = Color.FromArgb(255, 103, 168);
            lblTitle3.Location = new Point(20, 420);
            lblTitle3.AutoSize = true;
            this.Controls.Add(lblTitle3);

            // --- ПОЗИЦИЯ X ---
            Label lblPosX = new Label();
            lblPosX.Text = "Позиция X (левый верхний угол):";
            lblPosX.ForeColor = Color.FromArgb(44, 62, 80);
            lblPosX.Location = new Point(40, 460);
            lblPosX.AutoSize = true;
            this.Controls.Add(lblPosX);

            numPosX = new NumericUpDown();
            numPosX.Minimum = 0;
            numPosX.Maximum = Screen.PrimaryScreen.Bounds.Width - 100;
            numPosX.Value = _settings.RestZoneX;
            numPosX.Location = new Point(280, 458); // оставляем исходную позицию
            numPosX.Width = 100;
            numPosX.BackColor = Color.White;
            numPosX.ForeColor = Color.FromArgb(44, 62, 80);
            this.Controls.Add(numPosX);

            // --- ПОЗИЦИЯ Y ---
            Label lblPosY = new Label();
            lblPosY.Text = "Позиция Y (левый верхний угол):";
            lblPosY.ForeColor = Color.FromArgb(44, 62, 80);
            lblPosY.Location = new Point(40, 490);
            lblPosY.AutoSize = true;
            this.Controls.Add(lblPosY);

            numPosY = new NumericUpDown();
            numPosY.Minimum = 0;
            numPosY.Maximum = Screen.PrimaryScreen.Bounds.Height - 100;
            numPosY.Value = _settings.RestZoneY;
            numPosY.Location = new Point(280, 488);
            numPosY.Width = 100;
            numPosY.BackColor = Color.White;
            numPosY.ForeColor = Color.FromArgb(44, 62, 80);
            this.Controls.Add(numPosY);

            // --- ШИРИНА ---
            Label lblWidth = new Label();
            lblWidth.Text = "Ширина зоны:";
            lblWidth.ForeColor = Color.FromArgb(44, 62, 80);
            lblWidth.Location = new Point(40, 520);
            lblWidth.AutoSize = true;
            this.Controls.Add(lblWidth);

            numWidth = new NumericUpDown();
            numWidth.Minimum = 50;
            numWidth.Maximum = 1000;
            numWidth.Value = _settings.RestZoneWidth;
            numWidth.Location = new Point(280, 518);
            numWidth.Width = 100;
            numWidth.BackColor = Color.White;
            numWidth.ForeColor = Color.FromArgb(44, 62, 80);
            this.Controls.Add(numWidth);

            // --- ВЫСОТА ---
            Label lblHeight = new Label();
            lblHeight.Text = "Высота зоны:";
            lblHeight.ForeColor = Color.FromArgb(44, 62, 80);
            lblHeight.Location = new Point(40, 550);
            lblHeight.AutoSize = true;
            this.Controls.Add(lblHeight);

            numHeight = new NumericUpDown();
            numHeight.Minimum = 50;
            numHeight.Maximum = 1000;
            numHeight.Value = _settings.RestZoneHeight;
            numHeight.Location = new Point(280, 548);
            numHeight.Width = 100;
            numHeight.BackColor = Color.White;
            numHeight.ForeColor = Color.FromArgb(44, 62, 80);
            this.Controls.Add(numHeight);

            // --- ВРЕМЯ ДО ИССЛЕДОВАНИЯ ---
            lblExploreWait = new Label();
            lblExploreWait.Text = "Время до исследования (сек):";
            lblExploreWait.ForeColor = Color.FromArgb(44, 62, 80);
            lblExploreWait.Location = new Point(40, 585);
            lblExploreWait.AutoSize = true;
            this.Controls.Add(lblExploreWait);

            numExploreWait = new NumericUpDown();
            numExploreWait.Minimum = 5;
            numExploreWait.Maximum = 60;
            numExploreWait.Value = _settings.ExploreWaitTime;
            numExploreWait.Location = new Point(280, 583);
            numExploreWait.Width = 100;
            numExploreWait.BackColor = Color.White;
            numExploreWait.ForeColor = Color.FromArgb(44, 62, 80);
            this.Controls.Add(numExploreWait);

            // --- КНОПКА "СОХРАНИТЬ" (ИСХОДНЫЙ РАЗМЕР) ---
            Button btnSave = new Button();
            btnSave.Text = "Сохранить";
            btnSave.Size = new Size(110, 35); // исходный размер
            btnSave.Location = new Point(300, 630); // исходная позиция
            btnSave.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnSave.BackColor = Color.FromArgb(59, 200, 207); // бирюзовый
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Cursor = Cursors.Hand;
            MakeButtonRounded(btnSave, 18); // закругление
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            // --- КНОПКА "ПРИМЕНИТЬ К ПИТОМЦУ" ---
            Button btnApplyToPet = new Button();
            btnApplyToPet.Text = "Применить к питомцу";
            btnApplyToPet.Size = new Size(200, 35);
            btnApplyToPet.Location = new Point(10, 630);
            btnApplyToPet.Font = new Font("Segoe UI", 12, FontStyle.Bold); // ← ЖИРНЫЙ
            btnApplyToPet.BackColor = Color.FromArgb(154, 75, 255);
            btnApplyToPet.ForeColor = Color.White;
            btnApplyToPet.FlatStyle = FlatStyle.Flat;
            btnApplyToPet.FlatAppearance.BorderSize = 0;
            btnApplyToPet.Cursor = Cursors.Hand;
            MakeButtonRounded(btnApplyToPet, 18);
            btnApplyToPet.Click += (s, e) => ApplyToCurrentPet();
            this.Controls.Add(btnApplyToPet);

            // --- КНОПКА "ОТМЕНА" ---
            Button btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Size = new Size(100, 35);
            btnCancel.Location = new Point(420, 630);
            btnCancel.Font = new Font("Segoe UI", 12, FontStyle.Bold); // ← ЖИРНЫЙ
            btnCancel.BackColor = Color.FromArgb(254, 185, 182);
            btnCancel.ForeColor = Color.FromArgb(44, 62, 80);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Cursor = Cursors.Hand;
            MakeButtonRounded(btnCancel, 18);
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);

            // --- ПОДПИСЬ РАЗРАБОТЧИКА (опционально, для единства стиля) ---
            Label lblDev = new Label();
            lblDev.Text = "Cursor Companion — настройки питомцев";
            lblDev.Font = new Font("Segoe UI", 8, FontStyle.Italic);
            lblDev.ForeColor = Color.FromArgb(93, 109, 126);
            lblDev.Location = new Point(20, 690);
            lblDev.AutoSize = true;
            this.Controls.Add(lblDev);
        }

        // ========== МЕТОД ДЛЯ ЗАКРУГЛЕНИЯ КНОПОК ==========
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

        // ========== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ (ПРИВЕДЕНЫ К НОВОЙ ЦВЕТОВОЙ СХЕМЕ) ==========
        private Label CreateTitleLabel(string text)
        {
            var label = new Label();
            label.Text = text;
            label.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            label.ForeColor = Color.FromArgb(255, 103, 168); // розовый
            label.Margin = new Padding(0, 20, 0, 10);
            label.AutoSize = true;
            return label;
        }

        private Panel CreateSettingRow(string labelText, out NumericUpDown numeric,
            int value, int min, int max)
        {
            Panel panel = new Panel();
            panel.Width = 460;
            panel.Height = 30;
            panel.Margin = new Padding(0, 0, 0, 10);

            var label = new Label();
            label.Text = labelText;
            label.ForeColor = Color.FromArgb(44, 62, 80); // тёмно-синий
            label.Location = new Point(0, 5);
            label.AutoSize = true;

            numeric = new NumericUpDown();
            numeric.Minimum = min;
            numeric.Maximum = max;
            numeric.Value = value;
            numeric.Location = new Point(250, 3);
            numeric.Width = 100;
            numeric.BackColor = Color.White;
            numeric.ForeColor = Color.FromArgb(44, 62, 80);

            panel.Controls.Add(label);
            panel.Controls.Add(numeric);
            return panel;
        }

        private CheckBox CreateCheckBox(string text, bool isChecked)
        {
            var cb = new CheckBox();
            cb.Text = text;
            cb.Checked = isChecked;
            cb.ForeColor = Color.FromArgb(44, 62, 80);
            cb.Margin = new Padding(0, 0, 0, 10);
            cb.AutoSize = true;
            return cb;
        }

        private Panel CreateTrackbarRow(string labelText, out TrackBar trackBar,
            int value, int min, int max)
        {
            Panel panel = new Panel();
            panel.Width = 460;
            panel.Height = 50;
            panel.Margin = new Padding(0, 0, 0, 10);

            var label = new Label();
            label.Text = labelText;
            label.ForeColor = Color.FromArgb(44, 62, 80);
            label.Location = new Point(0, 5);
            label.AutoSize = true;

            trackBar = new TrackBar();
            trackBar.Minimum = min;
            trackBar.Maximum = max;
            trackBar.Value = value;
            trackBar.Location = new Point(200, 0);
            trackBar.Width = 150;
            trackBar.BackColor = this.BackColor; // сливается с фоном

            panel.Controls.Add(label);
            panel.Controls.Add(trackBar);
            return panel;
        }

        // ========== ЛОГИКА РАБОТЫ (БЕЗ ИЗМЕНЕНИЙ) ==========
        private void LoadSettingsToUI() { /* уже загружено */ }
        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveUIToSettings();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void SaveUIToSettings()
        {
            _settings.IdleTimeout = (int)numIdleTimeout.Value;
            _settings.AutoReturnToBase = cbAutoReturn.Checked;
            _settings.FreeRoamEnabled = cbFreeRoam.Checked;
            _settings.RoamSpeed = trackRoamSpeed.Value;
            _settings.MinimizeToTray = cbMinimizeToTray.Checked;
            _settings.EnableSounds = cbEnableSounds.Checked;
            _settings.ReactToClicks = cbReactToClicks.Checked;
            _settings.RestZoneX = (int)numPosX.Value;
            _settings.RestZoneY = (int)numPosY.Value;
            _settings.RestZoneWidth = (int)numWidth.Value;
            _settings.RestZoneHeight = (int)numHeight.Value;
            _settings.ExploreWaitTime = (int)numExploreWait.Value;
            Debug.WriteLine($"Settings Saved: FreeRoamEnabled={_settings.FreeRoamEnabled}, ExploreWaitTime={_settings.ExploreWaitTime}");
        }

        private void ApplyToCurrentPet()
        {
            MainForm mainForm = Application.OpenForms["MainForm"] as MainForm;
            if (mainForm != null)
            {
                SaveUIToSettings();
                mainForm.ApplyRestZoneSettings(
                    _settings.RestZoneX,
                    _settings.RestZoneY,
                    _settings.RestZoneWidth,
                    _settings.RestZoneHeight
                );
                MessageBox.Show("Настройки зоны отдыха применены к текущему питомцу!",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Главное окно не найдено. Настройки будут применены при следующем запуске питомца.",
                    "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public Models.Settings GetSettings() => _settings;
    }
}