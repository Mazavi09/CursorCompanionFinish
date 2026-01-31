using System;
using System.Drawing;
using System.Windows.Forms;

namespace CursorCompanionFinish
{
    public partial class AboutForm : Form
    {
        private int secretClickCount = 0;
        private DateTime lastClickTime = DateTime.Now;

        public AboutForm()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "О программе - Cursor Companion";
            this.Size = new Size(550, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Логотип - центрируем
            PictureBox logoBox = new PictureBox();
            logoBox.Size = new Size(120, 120);
            logoBox.Location = new Point(215, 20);
            logoBox.BackColor = Color.Transparent;
            logoBox.Cursor = Cursors.Hand;

            // Рисуем логотип (код без изменений)
            Bitmap logo = new Bitmap(120, 120);
            using (Graphics g = Graphics.FromImage(logo))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (SolidBrush brush = new SolidBrush(Color.FromArgb(0, 150, 136)))
                {
                    g.FillEllipse(brush, 0, 0, 120, 120);
                }

                using (SolidBrush brush = new SolidBrush(Color.FromArgb(30, 30, 30)))
                {
                    g.FillEllipse(brush, 10, 10, 100, 100);
                }

                Point[] lizard = {
            new Point(30, 50),
            new Point(60, 30),
            new Point(90, 50),
            new Point(70, 80),
            new Point(50, 70),
            new Point(30, 50)
        };

                using (SolidBrush brush = new SolidBrush(Color.FromArgb(100, 255, 100)))
                {
                    g.FillClosedCurve(brush, lizard);
                }

                using (Font font = new Font("Arial", 24, FontStyle.Bold))
                using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
                {
                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    g.DrawString("CC", font, textBrush, new RectangleF(0, 0, 120, 120), sf);
                }
            }

            logoBox.Image = logo;
            logoBox.Click += LogoBox_Click;

            // Информация о программе - центрируем
            Label lblTitle = new Label();
            lblTitle.Text = "Cursor Companion";
            lblTitle.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(0, 150);
            lblTitle.Size = new Size(550, 40);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblTitle);

            Label lblVersion = new Label();
            lblVersion.Text = "Версия 1.0.0";
            lblVersion.Font = new Font("Segoe UI", 11);
            lblVersion.ForeColor = Color.LightGray;
            lblVersion.Location = new Point(0, 195);
            lblVersion.Size = new Size(550, 25);
            lblVersion.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblVersion);

            Label lblDescription = new Label();
            lblDescription.Text = "Интерактивный компаньон для курсора\n" +
                                  "Дипломный проект по производственной практике";
            lblDescription.Font = new Font("Segoe UI", 10);
            lblDescription.ForeColor = Color.White;
            lblDescription.Location = new Point(0, 225);
            lblDescription.Size = new Size(550, 50);
            lblDescription.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblDescription);

            // Разработчик - центрируем
            Label lblDeveloper = new Label();
            lblDeveloper.Text = "Разработчик: Сыцевич Мария\n" +
                                "Группа: ИС-31\n" +
                                "Преподаватель: Гиршберг Г.Г.\n" +
                                "© 2025 Все права защищены";
            lblDeveloper.Font = new Font("Segoe UI", 10);
            lblDeveloper.ForeColor = Color.LightGray;
            lblDeveloper.Location = new Point(0, 285);
            lblDeveloper.Size = new Size(550, 90);
            lblDeveloper.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblDeveloper);

            // Секретная надпись (скрытая) - УДАЛЯЕМ или делаем НЕВИДИМОЙ
            // Вместо этого можно просто не добавлять или сделать:
            Label lblSecret = new Label();
            lblSecret.Text = "Зажмите Ctrl и кликните по логотипу 5 раз для секрета!";
            lblSecret.Font = new Font("Segoe UI", 8);
            lblSecret.ForeColor = Color.FromArgb(30, 30, 30); // Цвет фона - невидимо
            lblSecret.Location = new Point(130, 385);
            lblSecret.AutoSize = true;
            // ИЛИ ВООБЩЕ НЕ ДОБАВЛЯТЬ: this.Controls.Add(lblSecret);
            // Но для сохранения функционала оставим невидимой

            // Кнопка закрытия - центрируем (ПОДНИМАЕМ ВЫШЕ)
            Button btnClose = new Button();
            btnClose.Text = "Закрыть";
            btnClose.Size = new Size(100, 35); // Немного увеличили высоту
            btnClose.Location = new Point(225, 385); // Подняли с 380 до 370
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.BackColor = Color.FromArgb(64, 64, 64);
            btnClose.ForeColor = Color.White;
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);

            this.Controls.Add(logoBox);
        }

        private void LogoBox_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;

            // Проверяем зажат ли Ctrl
            if (Control.ModifierKeys == Keys.Control)
            {
                // Проверяем время между кликами (не больше 1 секунды)
                if ((DateTime.Now - lastClickTime).TotalSeconds > 1)
                {
                    secretClickCount = 0;
                }

                secretClickCount++;
                lastClickTime = DateTime.Now;

                // Визуальная обратная связь
                PictureBox logoBox = (PictureBox)sender;
                logoBox.BackColor = Color.FromArgb(50, 255, 255, 255);
                Timer timer = new Timer();
                timer.Interval = 100;
                timer.Tick += (s, args) => { logoBox.BackColor = Color.Transparent; timer.Stop(); };
                timer.Start();

                // После 5 кликов
                if (secretClickCount >= 5)
                {
                    UnlockSecretSkin();
                    secretClickCount = 0;
                }
            }
        }

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