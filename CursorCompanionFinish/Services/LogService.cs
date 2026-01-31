using System;
using System.IO;
using System.Linq;

namespace CursorCompanionFinish.Services
{
    /// <summary>
    /// Сервис для логирования активности
    /// </summary>
    public class LogService
    {
        private readonly string _logDirectory;

        /// <summary>
        /// Конструктор - создает папку для логов
        /// </summary>
        public LogService()
        {
            _logDirectory = "Logs";
            Directory.CreateDirectory(_logDirectory);
        }

        /// <summary>
        /// Логирует активность скина
        /// </summary>
        /// <param name="skinId">ID скина</param>
        /// <param name="activity">Описание активности</param>
        public void LogActivity(int skinId, string activity)
        {
            try
            {
                string logFile = Path.Combine(_logDirectory, $"skin_{skinId}.log");
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logEntry = $"[{timestamp}] {activity}\r\n";

                File.AppendAllText(logFile, logEntry);
            }
            catch
            {
                // Игнорируем ошибки логирования
            }
        }

        /// <summary>
        /// Получает статистику по скину
        /// </summary>
        /// <param name="skinId">ID скина</param>
        /// <returns>Строка со статистикой</returns>
        public string GetStatistics(int skinId)
        {
            try
            {
                string logFile = Path.Combine(_logDirectory, $"skin_{skinId}.log");

                if (!File.Exists(logFile))
                    return $"Статистика для скина {skinId} отсутствует\nФайл лога не найден";

                string[] lines = File.ReadAllLines(logFile);
                int activations = lines.Count(l => l.Contains("активирован"));
                int sleepCount = lines.Count(l => l.Contains("сон") || l.Contains("Сон") || l.Contains("sleep"));
                int roamCount = lines.Count(l => l.Contains("исследование") || l.Contains("Исследование") || l.Contains("roam"));
                int returnCount = lines.Count(l => l.Contains("базу") || l.Contains("Базу") || l.Contains("base"));

                return $"Статистика для скина {skinId}:\n\n" +
                       $"Активаций: {activations}\n" +
                       $"Раз уснул: {sleepCount}\n" +
                       $"Исследований: {roamCount}\n" +
                       $"Возвратов на базу: {returnCount}\n" +
                       $"Всего записей: {lines.Length}\n\n" +
                       $"Последняя активность:\n" +
                       $"{(lines.Length > 0 ? lines.Last() : "нет данных")}";
            }
            catch
            {
                return "Ошибка чтения статистики";
            }
        }
    }
}