using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
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
            // ========== ОКНО ==========
            this.Text = "Cursor Companion - Добро пожаловать";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            // Базовый цвет (будет перекрыт градиентом)
            this.BackColor = Color.FromArgb(255, 243, 158);

            // ========== ЗАГОЛОВОК ==========
            Label lblTitle = new Label();
            lblTitle.Text = "Cursor Companion";
            lblTitle.Font = new Font("Segoe UI", 28, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(255, 103, 168); // розовый
            lblTitle.Location = new Point(100, 50);
            lblTitle.Size = new Size(500, 60);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.BackColor = Color.Transparent; // ПРОЗРАЧНЫЙ!
            this.Controls.Add(lblTitle);

            // ========== ПОДЗАГОЛОВОК ==========
            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Кастомизируйте свой курсор и рабочий стол\nзабавными интерактивными питомцами!";
            lblSubtitle.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            lblSubtitle.ForeColor = Color.FromArgb(100, 45, 160); // фиолетовый
            lblSubtitle.Location = new Point(100, 130);
            lblSubtitle.Size = new Size(500, 80);
            lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            lblSubtitle.AutoSize = false;
            lblSubtitle.BackColor = Color.Transparent; // ПРОЗРАЧНЫЙ!
            this.Controls.Add(lblSubtitle);

            // ========== КНОПКА «МЕНЮ НАСТРОЙКИ» ==========
            Button btnMenu = new Button();
            btnMenu.Text = "🎮 Меню настройки";
            btnMenu.Size = new Size(250, 55);
            btnMenu.Location = new Point(225, 260);
            btnMenu.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            btnMenu.BackColor = Color.FromArgb(254, 185, 182); // пастельно-розовый
            btnMenu.ForeColor = Color.White;
            btnMenu.FlatStyle = FlatStyle.Flat;
            btnMenu.FlatAppearance.BorderSize = 0;
            btnMenu.Cursor = Cursors.Hand;
            MakeButtonRounded(btnMenu, 20);
            btnMenu.Click += (s, e) =>
            {
                this.Hide();
                MainForm mainForm = new MainForm();
                mainForm.FormClosed += (sender, args) => this.Close();
                mainForm.Show();
            };
            this.Controls.Add(btnMenu);

            // ========== КНОПКА «ВЫХОД» ==========
            Button btnExit = new Button();
            btnExit.Text = "Выход";
            btnExit.Size = new Size(100, 40);
            btnExit.Location = new Point(300, 327);
            btnExit.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnExit.BackColor = Color.FromArgb(59, 200, 207); // бирюзовый
            btnExit.ForeColor = Color.White;
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Cursor = Cursors.Hand;
            MakeButtonRounded(btnExit, 15);
            btnExit.Click += (s, e) =>
            {
                RestoreSystemWallpaper();
                Application.Exit();
            };
            this.Controls.Add(btnExit);

            // ========== РАЗРАБОТЧИК ==========
            Label lblDeveloper = new Label();
            lblDeveloper.Text = "Разработчик: Сыцевич Мария";
            lblDeveloper.Font = new Font("Segoe UI", 9);
            lblDeveloper.ForeColor = Color.FromArgb(93, 109, 126);
            lblDeveloper.Location = new Point(250, 400);
            lblDeveloper.Size = new Size(200, 30);
            lblDeveloper.TextAlign = ContentAlignment.MiddleCenter;
            lblDeveloper.BackColor = Color.Transparent; // ПРОЗРАЧНЫЙ!
            this.Controls.Add(lblDeveloper);

            // ========== ДЕКОРАТИВНЫЕ ЛИНИИ ==========
            Panel topLine = new Panel();
            topLine.Size = new Size(500, 3);
            topLine.Location = new Point(100, 120);
            topLine.BackColor = Color.FromArgb(154, 75, 255); // фиолетовый
            topLine.BringToFront(); // Чтобы линии были ПОВЕРХ градиента
            this.Controls.Add(topLine);

            Panel bottomLine = new Panel();
            bottomLine.Size = new Size(500, 3);
            bottomLine.Location = new Point(100, 380);
            bottomLine.BackColor = Color.FromArgb(154, 75, 255);
            bottomLine.BringToFront(); // Чтобы линии были ПОВЕРХ градиента
            this.Controls.Add(bottomLine);

            // ========== ГРАДИЕНТНЫЙ ФОН ==========
            this.Paint += (sender, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    this.ClientRectangle,
                    Color.FromArgb(255, 243, 158),      // жёлтый (правый верхний)
                    Color.FromArgb(197, 149, 255),      // фиолетовый (левый нижний)
                    LinearGradientMode.ForwardDiagonal)) // от правого верхнего к левому нижнему
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }
                 
            };

            // ========== ДОПОЛНИТЕЛЬНО: делаем ВСЕ надписи прозрачными ==========
            // (на случай, если где-то пропустили)
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Label)
                {
                    ctrl.BackColor = Color.Transparent;
                }
            }
        }

        /// <summary>
        /// Создаёт запасной логотип, если файл не найден
        /// </summary>
        private void CreateFallbackLogo(PictureBox logoBox)
        {
            Bitmap fallbackLogo = new Bitmap(120, 120);
            using (Graphics g = Graphics.FromImage(fallbackLogo))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Внешний круг - розовый
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(255, 103, 168)))
                {
                    g.FillEllipse(brush, 0, 0, 120, 120);
                }

                // Внутренний круг - полупрозрачный
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(30, 30, 30, 30)))
                {
                    g.FillEllipse(brush, 10, 10, 100, 100);
                }

                // Ящерица - бирюзовая
                Point[] lizard = {
            new Point(30, 50),
            new Point(60, 30),
            new Point(90, 50),
            new Point(70, 80),
            new Point(50, 70),
            new Point(30, 50)
        };

                using (SolidBrush brush = new SolidBrush(Color.FromArgb(59, 200, 207)))
                {
                    g.FillClosedCurve(brush, lizard);
                }

                // Текст "CC"
                using (Font font = new Font("Arial", 24, FontStyle.Bold))
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    g.DrawString("CC", font, textBrush, new RectangleF(0, 0, 120, 120), sf);
                }
            }

            logoBox.Image = fallbackLogo;
            Debug.WriteLine("Создан запасной логотип");
        }

        /// <summary>
        /// Делает кнопку круглой/со скруглёнными углами
        /// </summary>
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
    }
}