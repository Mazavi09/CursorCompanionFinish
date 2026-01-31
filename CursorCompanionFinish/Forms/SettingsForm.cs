using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

// Добавляем эту строку, если её нет:
using Models = CursorCompanionFinish.Models;

namespace CursorCompanionFinish
{
    public partial class SettingsForm : Form
    {
        private Models.Settings _settings; // Используем Models.Settings

        // Существующие элементы
        private NumericUpDown numIdleTimeout;
        private CheckBox cbFreeRoam;
        private TrackBar trackRoamSpeed;
        private CheckBox cbAutoReturn;
        private CheckBox cbMinimizeToTray;
        private CheckBox cbEnableSounds;
        private CheckBox cbReactToClicks;

        // Новые элементы для зоны отдыха
        private NumericUpDown numPosX;
        private NumericUpDown numPosY;
        private NumericUpDown numWidth;
        private NumericUpDown numHeight;

        // ДОБАВЬТЕ ЭТИ ДВЕ СТРОКИ:
        private Label lblExploreWait;
        private NumericUpDown numExploreWait;

        public SettingsForm(Models.Settings settings)
        {
            _settings = settings;
            InitializeComponent();
            InitializeUI();
            LoadSettingsToUI();
        }

        private void InitializeUI()
        {
            this.Text = "Настройки";
            this.Size = new Size(550, 720); // УВЕЛИЧИЛИ с 650 до 720
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(45, 45, 48);

            // Заголовок "Время до исследования"
            lblExploreWait = new Label(); // Без var!
            lblExploreWait.Text = "Время до исследования (сек):";
            lblExploreWait.ForeColor = Color.White;
            lblExploreWait.Location = new Point(40, 580);
            lblExploreWait.AutoSize = true;
            this.Controls.Add(lblExploreWait);

            numExploreWait = new NumericUpDown(); // Без var!
            numExploreWait.Minimum = 5;
            numExploreWait.Maximum = 60;
            numExploreWait.Value = _settings.ExploreWaitTime;
            numExploreWait.Location = new Point(250, 578);
            numExploreWait.Width = 100;
            this.Controls.Add(numExploreWait);

            // Заголовок
            var lblTitle = new Label();
            lblTitle.Text = "⚙ НАСТРОЙКИ ПОВЕДЕНИЯ";
            lblTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 20);
            lblTitle.AutoSize = true;
            this.Controls.Add(lblTitle);

            // Таймаут бездействия
            var lblIdle = new Label();
            lblIdle.Text = "Время до сна (секунд):";
            lblIdle.ForeColor = Color.White;
            lblIdle.Location = new Point(40, 70);
            lblIdle.AutoSize = true;
            this.Controls.Add(lblIdle);

            numIdleTimeout = new NumericUpDown();
            numIdleTimeout.Minimum = 5;
            numIdleTimeout.Maximum = 60;
            numIdleTimeout.Value = _settings.IdleTimeout;
            numIdleTimeout.Location = new Point(200, 68);
            numIdleTimeout.Width = 100;
            this.Controls.Add(numIdleTimeout);

            // Авто-возврат на базу
            cbAutoReturn = new CheckBox();
            cbAutoReturn.Text = "Автоматически возвращаться на место отдыха";
            cbAutoReturn.Checked = _settings.AutoReturnToBase;
            cbAutoReturn.ForeColor = Color.White;
            cbAutoReturn.Location = new Point(40, 110);
            cbAutoReturn.AutoSize = true;
            this.Controls.Add(cbAutoReturn);

            // Режим исследования
            cbFreeRoam = new CheckBox();
            cbFreeRoam.Text = "Режим свободного исследования";
            cbFreeRoam.Checked = _settings.FreeRoamEnabled;
            cbFreeRoam.ForeColor = Color.White;
            cbFreeRoam.Location = new Point(40, 150);
            cbFreeRoam.AutoSize = true;
            this.Controls.Add(cbFreeRoam);

            // Скорость исследования
            var lblSpeed = new Label();
            lblSpeed.Text = "Скорость исследования:";
            lblSpeed.ForeColor = Color.White;
            lblSpeed.Location = new Point(40, 190);
            lblSpeed.AutoSize = true;
            this.Controls.Add(lblSpeed);

            trackRoamSpeed = new TrackBar();
            trackRoamSpeed.Minimum = 1;
            trackRoamSpeed.Maximum = 10;
            trackRoamSpeed.Value = _settings.RoamSpeed;
            trackRoamSpeed.Location = new Point(200, 185);
            trackRoamSpeed.Width = 150;
            this.Controls.Add(trackRoamSpeed);

            // Заголовок 2
            var lblTitle2 = new Label();
            lblTitle2.Text = "⚙ НАСТРОЙКИ ПРИЛОЖЕНИЯ";
            lblTitle2.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTitle2.ForeColor = Color.White;
            lblTitle2.Location = new Point(20, 250);
            lblTitle2.AutoSize = true;
            this.Controls.Add(lblTitle2);

            // Сворачивание в трей
            cbMinimizeToTray = new CheckBox();
            cbMinimizeToTray.Text = "Сворачивать в трей при активации";
            cbMinimizeToTray.Checked = _settings.MinimizeToTray;
            cbMinimizeToTray.ForeColor = Color.White;
            cbMinimizeToTray.Location = new Point(40, 300);
            cbMinimizeToTray.AutoSize = true;
            this.Controls.Add(cbMinimizeToTray);

            // Звуки
            cbEnableSounds = new CheckBox();
            cbEnableSounds.Text = "Включить звуковые эффекты";
            cbEnableSounds.Checked = _settings.EnableSounds;
            cbEnableSounds.ForeColor = Color.White;
            cbEnableSounds.Location = new Point(40, 330);
            cbEnableSounds.AutoSize = true;
            this.Controls.Add(cbEnableSounds);

            // Реакция на клики
            cbReactToClicks = new CheckBox();
            cbReactToClicks.Text = "Реагировать на клики мыши";
            cbReactToClicks.Checked = _settings.ReactToClicks;
            cbReactToClicks.ForeColor = Color.White;
            cbReactToClicks.Location = new Point(40, 360);
            cbReactToClicks.AutoSize = true;
            this.Controls.Add(cbReactToClicks);

            // Заголовок 3: Зона отдыха
            var lblTitle3 = new Label();
            lblTitle3.Text = "🛏 НАСТРОЙКИ ЗОНЫ ОТДЫХА";
            lblTitle3.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTitle3.ForeColor = Color.White;
            lblTitle3.Location = new Point(20, 420);
            lblTitle3.AutoSize = true;
            this.Controls.Add(lblTitle3);

            // Позиция X
            var lblPosX = new Label();
            lblPosX.Text = "Позиция X (левый верхний угол):";
            lblPosX.ForeColor = Color.White;
            lblPosX.Location = new Point(40, 460);
            lblPosX.AutoSize = true;
            this.Controls.Add(lblPosX);

            numPosX = new NumericUpDown();
            numPosX.Minimum = 0;
            numPosX.Maximum = Screen.PrimaryScreen.Bounds.Width - 100;
            numPosX.Value = _settings.RestZoneX;
            numPosX.Location = new Point(250, 458);
            numPosX.Width = 100;
            this.Controls.Add(numPosX);

            // Позиция Y
            var lblPosY = new Label();
            lblPosY.Text = "Позиция Y (левый верхний угол):";
            lblPosY.ForeColor = Color.White;
            lblPosY.Location = new Point(40, 490);
            lblPosY.AutoSize = true;
            this.Controls.Add(lblPosY);

            numPosY = new NumericUpDown();
            numPosY.Minimum = 0;
            numPosY.Maximum = Screen.PrimaryScreen.Bounds.Height - 100;
            numPosY.Value = _settings.RestZoneY;
            numPosY.Location = new Point(250, 488);
            numPosY.Width = 100;
            this.Controls.Add(numPosY);

            // Ширина
            var lblWidth = new Label();
            lblWidth.Text = "Ширина зоны:";
            lblWidth.ForeColor = Color.White;
            lblWidth.Location = new Point(40, 520);
            lblWidth.AutoSize = true;
            this.Controls.Add(lblWidth);

            numWidth = new NumericUpDown();
            numWidth.Minimum = 50;
            numWidth.Maximum = 1000;
            numWidth.Value = _settings.RestZoneWidth;
            numWidth.Location = new Point(250, 518);
            numWidth.Width = 100;
            this.Controls.Add(numWidth);

            // Высота
            var lblHeight = new Label();
            lblHeight.Text = "Высота зоны:";
            lblHeight.ForeColor = Color.White;
            lblHeight.Location = new Point(40, 550);
            lblHeight.AutoSize = true;
            this.Controls.Add(lblHeight);

            numHeight = new NumericUpDown();
            numHeight.Minimum = 50;
            numHeight.Maximum = 1000;
            numHeight.Value = _settings.RestZoneHeight;
            numHeight.Location = new Point(250, 548);
            numHeight.Width = 100;
            this.Controls.Add(numHeight);

            // Кнопка "Применить к текущему питомцу"
            var btnApplyToPet = new Button();
            btnApplyToPet.Text = "Применить к питомцу";
            btnApplyToPet.Size = new Size(150, 30);
            btnApplyToPet.Location = new Point(360, 530);
            btnApplyToPet.BackColor = Color.FromArgb(0, 120, 215);
            btnApplyToPet.ForeColor = Color.White;
            btnApplyToPet.FlatStyle = FlatStyle.Flat;
            btnApplyToPet.Font = new Font("Segoe UI", 9);
            btnApplyToPet.Click += (s, e) => ApplyToCurrentPet();
            this.Controls.Add(btnApplyToPet);

           
            // Кнопки Сохранить/Отмена - СДВИГАЕМ НИЖЕ
            var btnSave = new Button();
            btnSave.Text = "Сохранить";
            btnSave.Size = new Size(100, 35);
            btnSave.Location = new Point(250, 620); // Было 600
            btnSave.BackColor = Color.FromArgb(0, 120, 215);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            var btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Size = new Size(100, 35);
            btnCancel.Location = new Point(360, 620); // Было 600
            btnCancel.BackColor = Color.FromArgb(64, 64, 64);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);
        }

        // Вспомогательные методы
        private Label CreateTitleLabel(string text)
        {
            var label = new Label();
            label.Text = text;
            label.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            label.ForeColor = Color.White;
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
            label.ForeColor = Color.White;
            label.Location = new Point(0, 5);
            label.AutoSize = true;

            numeric = new NumericUpDown();
            numeric.Minimum = min;
            numeric.Maximum = max;
            numeric.Value = value;
            numeric.Location = new Point(250, 3);
            numeric.Width = 100;

            panel.Controls.Add(label);
            panel.Controls.Add(numeric);
            return panel;
        }

        private CheckBox CreateCheckBox(string text, bool isChecked)
        {
            var cb = new CheckBox();
            cb.Text = text;
            cb.Checked = isChecked;
            cb.ForeColor = Color.White;
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
            label.ForeColor = Color.White;
            label.Location = new Point(0, 5);
            label.AutoSize = true;

            trackBar = new TrackBar();
            trackBar.Minimum = min;
            trackBar.Maximum = max;
            trackBar.Value = value;
            trackBar.Location = new Point(200, 0);
            trackBar.Width = 150;

            panel.Controls.Add(label);
            panel.Controls.Add(trackBar);
            return panel;
        }

        private void LoadSettingsToUI()
        {
            // Уже загружено в конструкторах элементов
        }

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

            // ДОБАВИТЬ ОТЛАДКУ
            Debug.WriteLine($"Settings Saved: FreeRoamEnabled={_settings.FreeRoamEnabled}, ExploreWaitTime={_settings.ExploreWaitTime}");
        }

        private void ApplyToCurrentPet()
        {
            // Получаем главную форму
            MainForm mainForm = Application.OpenForms["MainForm"] as MainForm;
            if (mainForm != null)
            {
                // Обновляем настройки
                SaveUIToSettings();

                // Применяем к текущему питомцу через MainForm
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

        public Models.Settings GetSettings()
        {
            return _settings;
        }
    }
}