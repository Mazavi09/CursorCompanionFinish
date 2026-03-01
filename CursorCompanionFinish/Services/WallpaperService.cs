using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace CursorCompanionFinish.Services
{
    public class WallpaperService
    {
        // Поле для хранения пути к оригинальным обоям пользователя
        private string _userWallpaperPath;

        // WinAPI функции
        private static class NativeMethods
        {
            public const uint SPI_SETDESKWALLPAPER = 0x0014;
            public const uint SPIF_UPDATEINIFILE = 0x01;
            public const uint SPIF_SENDCHANGE = 0x02;

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern bool SystemParametersInfo(
                uint uiAction,
                uint uiParam,
                string pvParam,
                uint fWinIni
            );
        }

        public WallpaperService()
        {
            Debug.WriteLine("WallpaperService инициализирован");
            _userWallpaperPath = null;
        }

        /// <summary>
        /// Сохраняет путь к текущим обоям пользователя (вызывать перед первым изменением)
        /// </summary>
        public void SaveCurrentUserWallpaper()
        {
            try
            {
                _userWallpaperPath = GetCurrentWallpaperPathFromRegistry();
                Debug.WriteLine($"Сохранён путь к пользовательским обоям: {_userWallpaperPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка сохранения пути к обоям: {ex.Message}");
            }
        }

        /// <summary>
        /// Меняет обои рабочего стола на указанное изображение
        /// </summary>
        /// <param name="imagePath">Путь к файлу изображения</param>
        /// <returns>True если успешно, False если ошибка</returns>
        public bool ChangeWallpaper(string imagePath)
        {
            try
            {
                Debug.WriteLine($"=== СМЕНА ОБОЕВ ===");
                Debug.WriteLine($"Путь к изображению: {imagePath}");

                if (string.IsNullOrEmpty(imagePath))
                {
                    Debug.WriteLine("Ошибка: путь к изображению пуст");
                    return false;
                }

                if (!File.Exists(imagePath))
                {
                    Debug.WriteLine($"Ошибка: файл не существует по пути {imagePath}");
                    return false;
                }

                // Проверяем размер файла
                FileInfo fileInfo = new FileInfo(imagePath);
                Debug.WriteLine($"Размер файла: {fileInfo.Length} байт");

                // Устанавливаем обои через WinAPI
                bool result = NativeMethods.SystemParametersInfo(
                    NativeMethods.SPI_SETDESKWALLPAPER,
                    0,
                    imagePath,
                    NativeMethods.SPIF_UPDATEINIFILE | NativeMethods.SPIF_SENDCHANGE
                );

                Debug.WriteLine($"Результат смены обоев: {result}");

                if (result)
                {
                    Debug.WriteLine($"✓ Обои успешно изменены на: {Path.GetFileName(imagePath)}");
                }
                else
                {
                    Debug.WriteLine("✗ Не удалось изменить обои через WinAPI");
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✗ Ошибка при смене обоев: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Восстанавливает оригинальные обои пользователя
        /// </summary>
        public void RestoreOriginalWallpaper()
        {
            try
            {
                Debug.WriteLine("=== ВОССТАНОВЛЕНИЕ ОБОЕВ ПОЛЬЗОВАТЕЛЯ ===");

                string wallpaperToRestore = null;

                // Сначала пробуем сохранённый путь (если есть)
                if (!string.IsNullOrEmpty(_userWallpaperPath))
                {
                    Debug.WriteLine($"Проверяем сохранённый путь: {_userWallpaperPath}");

                    if (File.Exists(_userWallpaperPath))
                    {
                        wallpaperToRestore = _userWallpaperPath;
                        Debug.WriteLine("Используем сохранённый путь к обоям");
                    }
                    else
                    {
                        Debug.WriteLine("Сохранённый файл не существует");
                    }
                }

                // Если нет сохранённого пути или файл не существует - читаем из реестра
                if (string.IsNullOrEmpty(wallpaperToRestore))
                {
                    string registryPath = GetCurrentWallpaperPathFromRegistry();
                    Debug.WriteLine($"Проверяем путь из реестра: {registryPath}");

                    if (!string.IsNullOrEmpty(registryPath) && File.Exists(registryPath))
                    {
                        wallpaperToRestore = registryPath;
                        Debug.WriteLine("Используем путь из реестра");
                    }
                }

                // Восстанавливаем обои
                if (!string.IsNullOrEmpty(wallpaperToRestore))
                {
                    bool result = NativeMethods.SystemParametersInfo(
                        NativeMethods.SPI_SETDESKWALLPAPER,
                        0,
                        wallpaperToRestore,
                        NativeMethods.SPIF_UPDATEINIFILE | NativeMethods.SPIF_SENDCHANGE
                    );

                    Debug.WriteLine($"Восстанавливаем обои: {wallpaperToRestore}. Результат: {result}");

                    if (result)
                    {
                        Debug.WriteLine("✓ Обои пользователя успешно восстановлены");
                    }
                    else
                    {
                        Debug.WriteLine("✗ Не удалось восстановить обои пользователя");
                    }
                }
                else
                {
                    Debug.WriteLine("Не удалось найти обои для восстановления");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✗ Ошибка при восстановлении обоев: {ex.Message}");
            }
        }

        /// <summary>
        /// Читает путь к текущим обоям из реестра Windows
        /// </summary>
        private string GetCurrentWallpaperPathFromRegistry()
        {
            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop"))
                {
                    if (key != null)
                    {
                        // В реестре путь хранится в значении "Wallpaper"
                        string wallpaperPath = key.GetValue("Wallpaper") as string;

                        if (!string.IsNullOrEmpty(wallpaperPath))
                        {
                            Debug.WriteLine($"Прочитан путь из реестра: {wallpaperPath}");
                            return wallpaperPath;
                        }
                        else
                        {
                            Debug.WriteLine("Значение Wallpaper в реестре пустое");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Не удалось открыть ключ реестра Control Panel\\Desktop");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка чтения реестра: {ex.Message}");
            }

            // Если не удалось прочитать из реестра, пробуем альтернативный метод
            return GetWallpaperFromIniFile();
        }

        /// <summary>
        /// Альтернативный метод получения обоев из system.ini (на всякий случай)
        /// </summary>
        private string GetWallpaperFromIniFile()
        {
            try
            {
                string systemIniPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "system.ini");
                if (File.Exists(systemIniPath))
                {
                    string[] lines = File.ReadAllLines(systemIniPath);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("Wallpaper=", StringComparison.OrdinalIgnoreCase))
                        {
                            string wallpaperPath = line.Substring("Wallpaper=".Length).Trim();
                            Debug.WriteLine($"Прочитан путь из system.ini: {wallpaperPath}");
                            return wallpaperPath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка чтения system.ini: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Проверяет, существуют ли обои по указанному пути
        /// </summary>
        public bool IsWallpaperExists(string imagePath)
        {
            return !string.IsNullOrEmpty(imagePath) && File.Exists(imagePath);
        }

        /// <summary>
        /// Возвращает сохранённый путь к пользовательским обоям
        /// </summary>
        public string GetSavedUserWallpaperPath()
        {
            return _userWallpaperPath;
        }
    }
}