using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace CursorCompanionFinish
{
    public partial class SplashForm : Form
    {
        public SplashForm()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void RestoreSystemWallpaper()
        {
            try
            {
                // Используем тот же сервис, что и в MainForm
                Services.WallpaperService wallpaperService = new Services.WallpaperService();
                wallpaperService.RestoreOriginalWallpaper();

                Debug.WriteLine("Системные обои восстановлены из SplashForm");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка восстановления обоев: " + ex.Message);
            }
        }

        private void InitializeUI()
        {
            this.Text = "Cursor Companion - Добро пожаловать";
            this.Size = new Size(700, 500); // УВЕЛИЧИЛИ ШИРИНУ с 600 до 700
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 40);

            // Заголовок программы
            Label lblTitle = new Label();
            lblTitle.Text = "Cursor Companion";
            lblTitle.Font = new Font("Segoe UI", 28, FontStyle.Bold); // УМЕНЬШИЛИ с 32 до 28
            lblTitle.ForeColor = Color.FromArgb(0, 150, 136);
            lblTitle.Location = new Point(100, 50);
            lblTitle.Size = new Size(500, 60); // УВЕЛИЧИЛИ ширину
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblTitle);

            // Подзаголовок
            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Кастомизируйте свой курсор и рабочий стол\nс забавными интерактивными питомцами!";
            lblSubtitle.Font = new Font("Segoe UI", 14);
            lblSubtitle.ForeColor = Color.White;
            lblSubtitle.Location = new Point(100, 130); // Сдвинули
            lblSubtitle.Size = new Size(500, 80);
            lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            lblSubtitle.AutoSize = false;
            this.Controls.Add(lblSubtitle);

            // Кнопка "Меню настройки"
            Button btnMenu = new Button();
            btnMenu.Text = "🎮 Меню настройки";
            btnMenu.Size = new Size(300, 60);
            btnMenu.Location = new Point(200, 230); // Сдвинули по центру
            btnMenu.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            btnMenu.BackColor = Color.FromArgb(0, 150, 136);
            btnMenu.ForeColor = Color.White;
            btnMenu.FlatStyle = FlatStyle.Flat;
            btnMenu.FlatAppearance.BorderSize = 0;
            btnMenu.Cursor = Cursors.Hand;
            btnMenu.Click += (s, e) =>
            {
                this.Hide();
                MainForm mainForm = new MainForm();
                mainForm.FormClosed += (sender, args) => this.Close();
                mainForm.Show();
            };
            this.Controls.Add(btnMenu);

            // Кнопка "Выход"
            Button btnExit = new Button();
            btnExit.Text = "Выход";
            btnExit.Size = new Size(300, 40);
            btnExit.Location = new Point(200, 310);
            btnExit.Font = new Font("Segoe UI", 12);
            btnExit.BackColor = Color.FromArgb(64, 64, 64);
            btnExit.ForeColor = Color.White;
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Cursor = Cursors.Hand;
            btnExit.Click += (s, e) =>
            {
                // Восстанавливаем системные обои перед выходом
                RestoreSystemWallpaper();
                Application.Exit();
            };
            this.Controls.Add(btnExit);

            // Разработчик (внизу)
            Label lblDeveloper = new Label();
            lblDeveloper.Text = "Разработчик: Сыцевич Мария";
            lblDeveloper.Font = new Font("Segoe UI", 9);
            lblDeveloper.ForeColor = Color.LightGray;
            lblDeveloper.Location = new Point(250, 400); // Сдвинули по центру
            lblDeveloper.Size = new Size(200, 30);
            lblDeveloper.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblDeveloper);

            // Декоративные элементы
            Panel topLine = new Panel();
            topLine.Size = new Size(500, 2); // Увеличили
            topLine.Location = new Point(100, 120);
            topLine.BackColor = Color.FromArgb(0, 150, 136);
            this.Controls.Add(topLine);

            Panel bottomLine = new Panel();
            bottomLine.Size = new Size(500, 2); // Увеличили
            bottomLine.Location = new Point(100, 380);
            bottomLine.BackColor = Color.FromArgb(0, 150, 136);
            this.Controls.Add(bottomLine);
        }
    }
}