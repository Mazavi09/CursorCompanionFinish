using System;
using System.IO;
using System.Windows.Forms;

namespace CursorCompanionFinish
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ВАЖНО: Устанавливаем правильную рабочую директорию
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string exeDirectory = Path.GetDirectoryName(exePath);

            System.Diagnostics.Debug.WriteLine("ExecutingAssembly Location: " + exePath);
            System.Diagnostics.Debug.WriteLine("Setting CurrentDirectory to: " + exeDirectory);

            // Устанавливаем текущую директорию туда, где находится exe файл
            Directory.SetCurrentDirectory(exeDirectory);

            // Создаем папки если их нет
            CreateFolders();

            // Запускаем окно входа вместо главного окна
            Application.Run(new SplashForm());
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