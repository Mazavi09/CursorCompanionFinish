using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CursorCompanionFinish.Services
{
    public class WallpaperService
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private const int SPI_SETDESKWALLPAPER = 0x0014;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        private string _originalWallpaperPath;
        private bool _isWallpaperChanged = false;

        public WallpaperService()
        {
            // Сохраняем оригинальные обои при создании
            SaveOriginalWallpaperPath();
        }

        private void SaveOriginalWallpaperPath()
        {
            // Читаем текущие обои из реестра
            using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop"))
            {
                if (key != null)
                {
                    _originalWallpaperPath = key.GetValue("Wallpaper") as string;
                    System.Diagnostics.Debug.WriteLine($"Оригинальные обои сохранены: {_originalWallpaperPath}");
                }
            }
        }

        public bool ChangeWallpaper(string imagePath)
        {
            try
            {
                if (!File.Exists(imagePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Файл обоев не существует: {imagePath}");
                    return false;
                }

                // Меняем обои
                int result = SystemParametersInfo(
                    SPI_SETDESKWALLPAPER,
                    0,
                    imagePath,
                    SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE
                );

                if (result != 0)
                {
                    _isWallpaperChanged = true;
                    System.Diagnostics.Debug.WriteLine($"Обои успешно изменены на: {Path.GetFileName(imagePath)}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка смены обоев: {ex.Message}");
                return false;
            }
        }

        public void RestoreOriginalWallpaper()
        {
            if (!_isWallpaperChanged || string.IsNullOrEmpty(_originalWallpaperPath))
            {
                System.Diagnostics.Debug.WriteLine("Не нужно восстанавливать обои (не меняли или нет сохраненного пути)");
                return;
            }

            try
            {
                // Проверяем, существует ли файл оригинальных обоев
                bool useOriginal = File.Exists(_originalWallpaperPath);

                if (!useOriginal)
                {
                    // Если оригинальный файл не найден, пробуем стандартный путь Windows
                    string windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                    string defaultWallpaper = Path.Combine(windowsPath, "Web", "Wallpaper", "Windows", "img0.jpg");

                    if (File.Exists(defaultWallpaper))
                    {
                        _originalWallpaperPath = defaultWallpaper;
                        useOriginal = true;
                    }
                }

                if (useOriginal)
                {
                    // Восстанавливаем оригинальные обои
                    int result = SystemParametersInfo(
                        SPI_SETDESKWALLPAPER,
                        0,
                        _originalWallpaperPath,
                        SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE
                    );

                    if (result != 0)
                    {
                        _isWallpaperChanged = false;
                        System.Diagnostics.Debug.WriteLine($"Обои восстановлены на: {Path.GetFileName(_originalWallpaperPath)}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Не удалось восстановить обои");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Файл оригинальных обоев не найден");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка восстановления обоев: {ex.Message}");
            }
        }
    }
}