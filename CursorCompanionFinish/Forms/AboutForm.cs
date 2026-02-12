using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace CursorCompanionFinish
{
    public partial class AboutForm : Form
    {
        private int secretClickCount = 0;
        private DateTime lastClickTime = DateTime.Now;

        public AboutForm()
        {
            InitializeUI(); // ← ТОЛЬКО ЭТО!
        }

        private void InitializeUI()
        {
            // ========== ОКНО ==========
            this.Text = "О программе - Cursor Companion";
            this.Size = new Size(550, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(255, 243, 158);

            // ========== ГРАДИЕНТНЫЙ ФОН (ЗЕРКАЛЬНЫЙ) ==========
            this.Paint += (sender, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    this.ClientRectangle,
                    Color.FromArgb(197, 149, 255),      // фиолетовый (левый верхний)
                    Color.FromArgb(255, 243, 158),      // жёлтый (правый нижний)
                    LinearGradientMode.BackwardDiagonal))
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }
            };

            // ========== ЛОГОТИП ИЗ ФАЙЛА ==========
            PictureBox logoBox = new PictureBox();
            logoBox.Size = new Size(120, 120);
            logoBox.Location = new Point(215, 20);
            logoBox.BackColor = Color.Transparent;
            logoBox.Cursor = Cursors.Hand;
            logoBox.SizeMode = PictureBoxSizeMode.Zoom;
            logoBox.Padding = new Padding(5);

            // Загружаем изображение
            try
            {
                string[] possiblePaths = {
                    "CCicon.png",
                    Path.Combine(Application.StartupPath, "CCicon.png"),
                    Path.Combine(Application.StartupPath, "Resources", "CCicon.png"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CCicon.png"),
                    Path.Combine(Directory.GetCurrentDirectory(), "CCicon.png"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Resources", "CCicon.png")
                };

                string iconPath = null;
                foreach (string path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        iconPath = path;
                        break;
                    }
                }

                if (iconPath != null)
                {
                    using (FileStream fs = new FileStream(iconPath, FileMode.Open, FileAccess.Read))
                    {
                        logoBox.Image = Image.FromStream(fs);
                    }
                }
                else
                {
                    CreateFallbackLogo(logoBox);
                }
            }
            catch
            {
                CreateFallbackLogo(logoBox);
            }

            logoBox.Click += LogoBox_Click;
            this.Controls.Add(logoBox);

            // ========== НАЗВАНИЕ ПРОГРАММЫ ==========
            Label lblTitle = new Label();
            lblTitle.Text = "Cursor Companion";
            lblTitle.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(44, 62, 80);
            lblTitle.Location = new Point(0, 150);
            lblTitle.Size = new Size(550, 40);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.BackColor = Color.Transparent;
            this.Controls.Add(lblTitle);

            // ========== ВЕРСИЯ ==========
            Label lblVersion = new Label();
            lblVersion.Text = "Версия 2.1.0";
            lblVersion.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblVersion.ForeColor = Color.FromArgb(93, 109, 126);
            lblVersion.Location = new Point(0, 195);
            lblVersion.Size = new Size(550, 25);
            lblVersion.TextAlign = ContentAlignment.MiddleCenter;
            lblVersion.BackColor = Color.Transparent;
            this.Controls.Add(lblVersion);

            // ========== ОПИСАНИЕ ==========
            Label lblDescription = new Label();
            lblDescription.Text = "Интерактивный компаньон для курсора\n" +
                                  "Дипломный проект по производственной практике";
            lblDescription.Font = new Font("Segoe UI", 10);
            lblDescription.ForeColor = Color.FromArgb(44, 62, 80);
            lblDescription.Location = new Point(0, 225);
            lblDescription.Size = new Size(550, 50);
            lblDescription.TextAlign = ContentAlignment.MiddleCenter;
            lblDescription.BackColor = Color.Transparent;
            this.Controls.Add(lblDescription);

            // ========== РАЗРАБОТЧИК ==========
            Label lblDeveloper = new Label();
            lblDeveloper.Text = "Разработчик: Сыцевич Мария\n" +
                                "Группа: ИС-31\n" +
                                "Преподаватель: Гиршберг Г.Г.\n" +
                                "© 2026 Все права защищены";
            lblDeveloper.Font = new Font("Segoe UI", 10);
            lblDeveloper.ForeColor = Color.FromArgb(93, 109, 126);
            lblDeveloper.Location = new Point(0, 285);
            lblDeveloper.Size = new Size(550, 90);
            lblDeveloper.TextAlign = ContentAlignment.MiddleCenter;
            lblDeveloper.BackColor = Color.Transparent;
            this.Controls.Add(lblDeveloper);

            // ========== КНОПКА ЗАКРЫТИЯ ==========
            Button btnClose = new Button();
            btnClose.Text = "Закрыть";
            btnClose.Size = new Size(100, 35);
            btnClose.Location = new Point(225, 385);
            btnClose.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.BackColor = Color.FromArgb(154, 75, 255);
            btnClose.ForeColor = Color.White;
            btnClose.Cursor = Cursors.Hand;

            MakeButtonRounded(btnClose, 15);

            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
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

        // ========== МЕТОД ДЛЯ ЗАПАСНОГО ЛОГОТИПА ==========
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
        }

        // ========== ОБРАБОТЧИК КЛИКА ПО ЛОГОТИПУ ==========
        private void LogoBox_Click(object sender, EventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                if ((DateTime.Now - lastClickTime).TotalSeconds > 1)
                {
                    secretClickCount = 0;
                }

                secretClickCount++;
                lastClickTime = DateTime.Now;

                PictureBox logoBox = (PictureBox)sender;
                logoBox.BackColor = Color.FromArgb(50, 255, 255, 255);
                Timer timer = new Timer();
                timer.Interval = 100;
                timer.Tick += (s, args) => { logoBox.BackColor = Color.Transparent; timer.Stop(); };
                timer.Start();

                if (secretClickCount >= 5)
                {
                    UnlockSecretSkin();
                    secretClickCount = 0;
                }
            }
        }

        // ========== РАЗБЛОКИРОВКА СЕКРЕТНОГО СКИНА ==========
        private void UnlockSecretSkin()
        {
            MessageBox.Show("🎉 СЕКРЕТНЫЙ СКИН РАЗБЛОКИРОВАН!\n\n" +
                          "Золотой дракон теперь доступен в галерее!\n" +
                          "Этот скин имеет уникальные футуристичные обои\n" +
                          "и особую анимацию.",
                          "Секрет разблокирован!",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Information);
        }
    }
}