using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace CursorCompanionFinish
{
    static class Program
    {
        private static Icon _applicationIcon;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Загружаем иконку приложения
            LoadApplicationIcon();

            // ВАЖНО: Устанавливаем правильную рабочую директорию
            string exePath = Assembly.GetExecutingAssembly().Location;
            string exeDirectory = Path.GetDirectoryName(exePath);

            System.Diagnostics.Debug.WriteLine("ExecutingAssembly Location: " + exePath);
            System.Diagnostics.Debug.WriteLine("Setting CurrentDirectory to: " + exeDirectory);

            // Устанавливаем текущую директорию туда, где находится exe файл
            Directory.SetCurrentDirectory(exeDirectory);

            // Создаем папки если их нет
            CreateFolders();

            // Подписываемся на создание всех форм
            Application.Idle += Application_Idle;

            // Запускаем окно входа
            Application.Run(new SplashForm());
        }

        private static void LoadApplicationIcon()
        {
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CCicon.ico");

                if (File.Exists(iconPath))
                {
                    _applicationIcon = new Icon(iconPath);
                    System.Diagnostics.Debug.WriteLine($"✓ Иконка загружена из: {iconPath}");
                }
                else
                {
                    // Пробуем альтернативные пути
                    string[] altPaths = {
                        Path.Combine(Application.StartupPath, "CCicon.ico"),
                        Path.Combine(Directory.GetCurrentDirectory(), "CCicon.ico"),
                        Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CCicon.ico"),
                        "CCicon.ico"
                    };

                    foreach (string altPath in altPaths)
                    {
                        if (File.Exists(altPath))
                        {
                            _applicationIcon = new Icon(altPath);
                            System.Diagnostics.Debug.WriteLine($"✓ Иконка загружена из альтернативного пути: {altPath}");
                            break;
                        }
                    }
                }

                if (_applicationIcon == null)
                {
                    System.Diagnostics.Debug.WriteLine("✗ Не удалось загрузить иконку, используется системная");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("✗ Ошибка при загрузке иконки: " + ex.Message);
                _applicationIcon = null;
            }
        }

        private static void Application_Idle(object sender, EventArgs e)
        {
            // Убираем обработчик, чтобы не вызывать постоянно
            Application.Idle -= Application_Idle;

            // Устанавливаем иконку для всех открытых форм
            SetIconForAllForms();
        }

        private static void SetIconForAllForms()
        {
            if (_applicationIcon == null) return;

            try
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form != null && !form.IsDisposed)
                    {
                        // Сохраняем текущее состояние
                        bool wasVisible = form.Visible;

                        // Устанавливаем иконку
                        if (form.Icon == null || form.Icon.Handle != _applicationIcon.Handle)
                        {
                            form.Icon = (Icon)_applicationIcon.Clone();
                            System.Diagnostics.Debug.WriteLine($"✓ Иконка установлена для формы: {form.GetType().Name}");
                        }

                        // Восстанавливаем состояние
                        if (!wasVisible && form.Visible)
                        {
                            form.Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("✗ Ошибка при установке иконки для форм: " + ex.Message);
            }
        }

        // Вспомогательный метод для принудительной установки иконки на любую форму
        public static void SetIconForForm(Form form)
        {
            if (form == null || form.IsDisposed || _applicationIcon == null) return;

            try
            {
                form.Icon = (Icon)_applicationIcon.Clone();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Ошибка при установке иконки для {form.GetType().Name}: " + ex.Message);
            }
        }

        private static void CreateFolders()
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
                        System.Diagnostics.Debug.WriteLine("Создана папка: " + fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Ошибка создания папок: " + ex.Message);
            }
        }
    }
}