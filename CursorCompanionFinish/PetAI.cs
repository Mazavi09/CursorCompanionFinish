using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using Models = CursorCompanionFinish.Models;

namespace CursorCompanionFinish
{
    public class PetAI : IDisposable
    {
        #region WinAPI Imports
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Keys vKey);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        #endregion

        private Models.Settings _settings;

        #region Константы
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_TOOLWINDOW = 0x80;

        private const float FOLLOW_SPEED = 0.15f;
        private const int SEGMENT_DISTANCE = 15;
        private const int BASE_RADIUS = 50;
        #endregion

        #region Перечисления
        public enum PetState
        {
            Following,
            Idle,
            Sleeping,
            Exploring,
            SentToBase
        }
        #endregion

        #region Поля
        private Thread _logicThread;
        private bool _isRunning = false;

        private Point[] _segments = new Point[7];
        private Point _targetPosition;
        private Point _basePosition;
        private Size _baseFormSize;
        private Bitmap[] _sprites = new Bitmap[7];

        private Form _overlayForm;
        private Form _baseForm;

        private PetState _currentState = PetState.Following;
        private int _idleTimeout = 10;
        private DateTime _lastMouseMove = DateTime.Now;
        private DateTime _lastStateChange = DateTime.Now;

        private bool _freeRoamEnabled = false;
        private Point _roamTarget;
        private Random _random = new Random();

        private int _skinId;
        private Color _themeColor;

        private Queue<string> _activityLog = new Queue<string>();

        // ДЛЯ РЕЖИМА ИССЛЕДОВАНИЯ:
        private DateTime _reachedTargetTime = DateTime.Now;
        private bool _isWaitingAtTarget = false;
        private const int ROAM_WAIT_TIME = 3000; // 3 секунды ожидания на точке
        private int _explorePointCount = 0;

        // ФИКСИРОВАННЫЙ МАРШРУТ (20 точек)
        private List<Point> _explorationRoute = new List<Point>();
        private int _currentRouteIndex = 0;
        // Для расчета маршрута
        private int _screenWidth;
        private int _screenHeight;
        #endregion

        #region Свойства
        public PetState CurrentState
        {
            get { return _currentState; }
        }

        public int SkinId
        {
            get { return _skinId; }
        }

        public bool IsRunning
        {
            get { return _isRunning; }
        }
        #endregion

        #region Конструктор
        public PetAI(int skinId, Models.Settings settings)
        {
            _skinId = skinId;
            _settings = settings;

            // Получаем размеры экрана
            Screen screen = Screen.PrimaryScreen;
            _screenWidth = screen.Bounds.Width;
            _screenHeight = screen.Bounds.Height;

            // Используем настройки из settings
            _basePosition = new Point(
                settings.RestZoneX,
                settings.RestZoneY
            );

            _baseFormSize = new Size(
                settings.RestZoneWidth,
                settings.RestZoneHeight
            );

            // Используем настройки времени бездействия
            _idleTimeout = settings.IdleTimeout;
            _freeRoamEnabled = settings.FreeRoamEnabled;

            _themeColor = GetThemeColor(skinId);

            // Инициализируем переменные для исследования
            _roamTarget = Point.Empty;
            _isWaitingAtTarget = false;
            _explorePointCount = 0;
            _reachedTargetTime = DateTime.Now;
            _currentRouteIndex = 0;

            // СОЗДАЕМ ФИКСИРОВАННЫЙ МАРШРУТ
            CreateExplorationRoute();

            InitializeOverlayForm();
            InitializeBaseForm();
            LoadSprites();
            InitializeSegments();
        }
        #endregion

        #region Инициализация
        private void InitializeOverlayForm()
        {
            _overlayForm = new Form();
            _overlayForm.FormBorderStyle = FormBorderStyle.None;
            _overlayForm.WindowState = FormWindowState.Maximized;
            _overlayForm.TopMost = true;
            _overlayForm.ShowInTaskbar = false;
            _overlayForm.BackColor = Color.Magenta;
            _overlayForm.TransparencyKey = Color.Magenta;
            _overlayForm.AllowTransparency = true;

            // Делаем окно полностью прозрачным для мыши
            int style = GetWindowLong(_overlayForm.Handle, GWL_EXSTYLE);
            SetWindowLong(_overlayForm.Handle, GWL_EXSTYLE,
                style | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);

            _overlayForm.Paint += OverlayForm_Paint;
        }

        private void InitializeBaseForm()
        {
            _baseForm = new Form();
            _baseForm.Text = "Место отдыха";
            _baseForm.FormBorderStyle = FormBorderStyle.None;
            _baseForm.Size = _baseFormSize;
            _baseForm.TopMost = true;
            _baseForm.ShowInTaskbar = false;

            // ДЕЛАЕМ ПОЛНОСТЬЮ ПРОЗРАЧНОЙ
            _baseForm.BackColor = Color.Magenta;
            _baseForm.TransparencyKey = Color.Magenta;
            _baseForm.AllowTransparency = true;
            _baseForm.Opacity = 0.0; // ПОЛНАЯ ПРОЗРАЧНОСТЬ

            _baseForm.StartPosition = FormStartPosition.Manual;
            _baseForm.Location = new Point(_basePosition.X, _basePosition.Y);

            int style = GetWindowLong(_baseForm.Handle, GWL_EXSTYLE);
            SetWindowLong(_baseForm.Handle, GWL_EXSTYLE,
                style | WS_EX_LAYERED | WS_EX_TOOLWINDOW);

            _baseForm.MouseDoubleClick += BaseForm_MouseDoubleClick;
        }

        private Color GetThemeColor(int skinId)
        {
            switch (skinId)
            {
                case 1: return Color.FromArgb(76, 175, 80);     // Ящерица - зеленый
                case 2: return Color.FromArgb(121, 85, 72);     // Крыса - коричневый
                case 3: return Color.FromArgb(139, 195, 74);    // Змея - салатовый
                case 4: return Color.FromArgb(33, 150, 243);    // Аксолотль - синий
                case 5: return Color.FromArgb(158, 158, 158);   // Кот - серый
                case 6: return Color.FromArgb(255, 152, 0);     // Хорек - оранжевый
                case 7: return Color.FromArgb(233, 30, 99);     // Дракон - розовый
                default: return Color.Purple;
            }
        }

        private void LoadSprites()
        {
            // Пробуем несколько возможных путей
            string[] possiblePaths = {
                "Skins/skin_" + _skinId,
                Path.Combine(Directory.GetCurrentDirectory(), "Skins", "skin_" + _skinId),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Skins", "skin_" + _skinId),
                Path.Combine(Application.StartupPath, "Skins", "skin_" + _skinId),
                @"C:\Users\user\Desktop\Cursor Companion\CursorCompanionFinish\CursorCompanionFinish\bin\Debug\Skins\skin_" + _skinId
            };

            string skinPath = null;
            foreach (string path in possiblePaths)
            {
                if (Directory.Exists(path))
                {
                    skinPath = path;
                    Debug.WriteLine($"Найден путь к скину: {path}");
                    break;
                }
            }

            Debug.WriteLine("=== ЗАГРУЗКА СПРАЙТОВ ===");
            Debug.WriteLine("Текущая директория: " + Directory.GetCurrentDirectory());
            Debug.WriteLine("Путь к скину: " + skinPath);

            if (skinPath == null || !Directory.Exists(skinPath))
            {
                Debug.WriteLine("✗ Папка не существует! Создаем тестовые спрайты");
                CreateTestSprites();
                return;
            }

            string[] files = Directory.GetFiles(skinPath, "*.png");
            Debug.WriteLine("Найдено PNG файлов: " + files.Length);

            // Загружаем спрайты для 7 сегментов
            for (int i = 0; i < 7; i++)
            {
                string spriteFile = Path.Combine(skinPath, "sprite_" + i.ToString() + ".png");

                if (File.Exists(spriteFile))
                {
                    try
                    {
                        _sprites[i] = new Bitmap(spriteFile);
                        MakeTransparent(_sprites[i]);
                        Debug.WriteLine("✓ Загружен спрайт " + i.ToString() + ": " + spriteFile);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("✗ Ошибка загрузки " + spriteFile + ": " + ex.Message);
                        _sprites[i] = CreateTestSprite(i, _themeColor);
                    }
                }
                else
                {
                    Debug.WriteLine("✗ Файл не найден: " + spriteFile);
                    _sprites[i] = CreateTestSprite(i, _themeColor);
                }
            }

            Debug.WriteLine("=== КОНЕЦ ЗАГРУЗКИ ===");
        }

        private Bitmap CreateTestSprite(int segmentIndex, Color color)
        {
            int size = (48 - segmentIndex * 4) / 2;
            Bitmap bmp = new Bitmap(size, size);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (SolidBrush brush = new SolidBrush(color))
                {
                    g.FillEllipse(brush, 0, 0, size, size);
                }

                Color darkerColor = Color.FromArgb(
                    Math.Max(color.R - 40, 0),
                    Math.Max(color.G - 40, 0),
                    Math.Max(color.B - 40, 0));

                using (Pen pen = new Pen(darkerColor, 3))
                {
                    g.DrawEllipse(pen, 2, 2, size - 4, size - 4);
                }

                using (Font font = new Font("Arial", 10, FontStyle.Bold))
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    g.DrawString(segmentIndex.ToString(), font, textBrush,
                        new RectangleF(0, 0, size, size), sf);
                }
            }

            return bmp;
        }

        private void CreateTestSprites()
        {
            for (int i = 0; i < 7; i++)
            {
                _sprites[i] = CreateTestSprite(i, _themeColor);
            }
            Debug.WriteLine("Созданы тестовые спрайты для скина " + _skinId.ToString());
        }

        private void MakeTransparent(Bitmap bitmap)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    if (pixel.R > 200 && pixel.B > 200 && pixel.G < 100)
                    {
                        bitmap.SetPixel(x, y, Color.Transparent);
                    }
                }
            }
        }

        private void InitializeSegments()
        {
            Point mousePos = new Point();
            GetCursorPos(ref mousePos);
            _targetPosition = mousePos;
            _lastMouseMove = DateTime.Now;

            for (int i = 0; i < 7; i++)
            {
                _segments[i] = new Point(
                    mousePos.X,
                    mousePos.Y + i * SEGMENT_DISTANCE);
            }
        }

        private void CreateExplorationRoute()
        {
            _explorationRoute.Clear();

            Screen screen = Screen.PrimaryScreen;
            Rectangle screenBounds = screen.Bounds;

            int screenWidth = screenBounds.Width;
            int screenHeight = screen.Bounds.Height;
            int margin = 100;

            // Создаем 30 случайных точек вместо 20
            Random rand = new Random();
            for (int i = 0; i < 30; i++)
            {
                int x = rand.Next(margin, screenWidth - margin);
                int y = rand.Next(margin, screenHeight - margin);
                _explorationRoute.Add(new Point(x, y));
            }

            // Добавляем несколько фиксированных точек для разнообразия
            _explorationRoute.Add(new Point(margin, margin)); // Верхний левый
            _explorationRoute.Add(new Point(screenWidth - margin, margin)); // Верхний правый
            _explorationRoute.Add(new Point(screenWidth - margin, screenHeight - margin)); // Нижний правый
            _explorationRoute.Add(new Point(margin, screenHeight - margin)); // Нижний левый
            _explorationRoute.Add(new Point(screenWidth / 2, screenHeight / 2)); // Центр

            // Перемешиваем точки
            ShuffleRoute();

            Debug.WriteLine($"Создан перемешанный маршрут из {_explorationRoute.Count} точек");
        }

        private void ShuffleRoute()
        {
            Random rand = new Random();
            int n = _explorationRoute.Count;

            for (int i = n - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                // Меняем местами
                Point temp = _explorationRoute[i];
                _explorationRoute[i] = _explorationRoute[j];
                _explorationRoute[j] = temp;
            }
        }
        #endregion

        #region Основной цикл
        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;

            Debug.WriteLine($"=== PetAI Start ===");
            Debug.WriteLine($"SkinId: {_skinId}");
            Debug.WriteLine($"FreeRoamEnabled: {_settings.FreeRoamEnabled}");
            Debug.WriteLine($"ExploreWaitTime: {_settings.ExploreWaitTime}");
            Debug.WriteLine($"=== ===");

            _logicThread = new Thread(PetLogic);
            _logicThread.IsBackground = true;
            _logicThread.Start();

            _overlayForm.Show();
            _baseForm.Show();

            LogActivity("Старт");
        }

        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;
            if (_logicThread != null)
            {
                _logicThread.Join(1000);
            }

            if (_overlayForm != null && !_overlayForm.IsDisposed)
            {
                _overlayForm.Invoke(new Action(delegate {
                    _overlayForm.Hide();
                    _overlayForm.Close();
                }));
            }

            if (_baseForm != null && !_baseForm.IsDisposed)
            {
                _baseForm.Invoke(new Action(delegate {
                    _baseForm.Hide();
                    _baseForm.Close();
                }));
            }

            LogActivity("Остановка");
        }

        private void PetLogic()
        {
            Debug.WriteLine("PetLogic начал работу");

            while (_isRunning)
            {
                try
                {
                    UpdateMousePosition();
                    UpdateState();
                    UpdatePosition();
                    UpdateDrawing();

                    Thread.Sleep(16);

                    // Отладочная печать каждую секунду
                    if (DateTime.Now.Second % 1 == 0 && _currentState == PetState.Exploring)
                    {
                        Debug.WriteLine($"[{DateTime.Now:HH:mm:ss}] Explore логика работает...");
                    }
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("PetLogic error: " + ex.Message);
                }
            }

            Debug.WriteLine("PetLogic завершил работу");
        }

        private void UpdateMousePosition()
        {
            Point mousePos = new Point();
            if (GetCursorPos(ref mousePos))
            {
                if (mousePos != _targetPosition)
                {
                    _lastMouseMove = DateTime.Now;
                    _targetPosition = mousePos;

                    if (_currentState == PetState.Sleeping || _currentState == PetState.Idle)
                    {
                        ChangeState(PetState.Following);
                    }
                }
            }
        }
        #endregion

        #region Управление состоянием
        private void UpdateState()
        {
            TimeSpan idleTime = DateTime.Now - _lastMouseMove;

            switch (_currentState)
            {
                case PetState.Following:
                    if (idleTime.TotalSeconds > _settings.ExploreWaitTime)
                    {
                        if (_settings.FreeRoamEnabled)
                        {
                            ChangeState(PetState.Exploring);
                            Debug.WriteLine($"Переход в исследование: {idleTime.TotalSeconds:F1} > {_settings.ExploreWaitTime} сек");
                        }
                        else if (_settings.AutoReturnToBase)
                        {
                            ChangeState(PetState.Idle);
                        }
                    }
                    break;

                case PetState.Idle:
                    MoveToBase();
                    if (IsAtBase())
                    {
                        ChangeState(PetState.Sleeping);
                    }
                    break;

                case PetState.Sleeping:
                    break;

                case PetState.Exploring:
                    if (idleTime.TotalSeconds < 0.5)
                    {
                        ChangeState(PetState.Following);
                        _isWaitingAtTarget = false;
                        _explorePointCount = 0;
                        Debug.WriteLine("Выход из исследования: курсор движется");
                    }
                    else
                    {
                        ExploreScreen();
                    }
                    break;

                case PetState.SentToBase:
                    MoveToBase();
                    if (IsAtBase())
                    {
                        ChangeState(PetState.Sleeping);
                    }
                    break;
            }
        }

        private void ChangeState(PetState newState)
        {
            if (_currentState == newState) return;

            string oldState = _currentState.ToString();
            _currentState = newState;
            _lastStateChange = DateTime.Now;

            string stateName = "";
            switch (newState)
            {
                case PetState.Following: stateName = "Слежение за курсором"; break;
                case PetState.Idle: stateName = "Ожидание"; break;
                case PetState.Sleeping: stateName = "Сон на базе"; break;
                case PetState.Exploring:
                    stateName = "Свободное исследование";
                    _explorePointCount = 0;
                    _isWaitingAtTarget = false;
                    _currentRouteIndex = 0;

                    if (_explorationRoute.Count > 0)
                    {
                        _roamTarget = _explorationRoute[_currentRouteIndex];
                        Debug.WriteLine($"Начинаем исследование с точки 1: {_roamTarget.X}, {_roamTarget.Y}");
                    }
                    break;
                case PetState.SentToBase: stateName = "Возврат на базу"; break;
                default: stateName = "Неизвестно"; break;
            }

            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss}] Смена состояния: {oldState} -> {newState} ({stateName})");
            LogActivity($"Смена состояния: {stateName}");
        }
        #endregion

        #region Движение
        private void UpdatePosition()
        {
            switch (_currentState)
            {
                case PetState.Following:
                    MoveToCursor();
                    break;

                case PetState.Idle:
                    MoveToBase();
                    break;

                case PetState.Sleeping:
                    StayAtBase();
                    break;

                case PetState.Exploring:
                    Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ExploreScreen вызывается");
                    ExploreScreen();
                    break;

                case PetState.SentToBase:
                    MoveToBase();
                    break;
            }
        }

        private void MoveToCursor()
        {
            _segments[0].X += (int)((_targetPosition.X - _segments[0].X) * FOLLOW_SPEED);
            _segments[0].Y += (int)((_targetPosition.Y - _segments[0].Y) * FOLLOW_SPEED);

            for (int i = 1; i < 7; i++)
            {
                Point previous = _segments[i - 1];
                Point current = _segments[i];

                double dx = previous.X - current.X;
                double dy = previous.Y - current.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance > 0)
                {
                    double ratio = SEGMENT_DISTANCE / distance;
                    _segments[i].X = previous.X - (int)(dx * ratio);
                    _segments[i].Y = previous.Y - (int)(dy * ratio);
                }
            }
        }

        private void StayAtBase()
        {
            int centerX = _basePosition.X + (_baseFormSize.Width / 2);
            int centerY = _basePosition.Y + (_baseFormSize.Height / 2);

            for (int i = 0; i < 7; i++)
            {
                double angle = (i * Math.PI / 6);
                int offsetX = (int)(Math.Cos(angle) * 20);
                int offsetY = (int)(Math.Sin(angle) * 20);

                _segments[i].X = centerX + offsetX - (i * 3);
                _segments[i].Y = centerY + offsetY - (i * 3);
            }
        }

        private void MoveToBase()
        {
            int centerX = _basePosition.X + (_baseFormSize.Width / 2);
            int centerY = _basePosition.Y + (_baseFormSize.Height / 2);

            float speed = 0.05f;
            if (_currentState == PetState.SentToBase)
            {
                speed = 0.1f;
            }

            _segments[0].X += (int)((centerX - _segments[0].X) * speed);
            _segments[0].Y += (int)((centerY - _segments[0].Y) * speed);

            for (int i = 1; i < 7; i++)
            {
                Point previous = _segments[i - 1];
                Point current = _segments[i];

                double dx = previous.X - current.X;
                double dy = previous.Y - current.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance > 0)
                {
                    double ratio = SEGMENT_DISTANCE / distance;
                    _segments[i].X = previous.X - (int)(dx * ratio);
                    _segments[i].Y = previous.Y - (int)(dy * ratio);
                }
            }
        }

        private void ExploreScreen()
        {
            // Если нет цели или достигли цели и прошло время ожидания
            if (_roamTarget == Point.Empty ||
                (IsAtTarget() && (DateTime.Now - _reachedTargetTime).TotalMilliseconds > ROAM_WAIT_TIME))
            {
                if (IsAtTarget())
                {
                    // Время ожидания прошло - выбираем следующую точку
                    _isWaitingAtTarget = false;
                    GetNextRoutePoint();
                    _explorePointCount++;
                }
                else if (_roamTarget == Point.Empty)
                {
                    // Первая точка исследования
                    GetNextRoutePoint();
                    _explorePointCount = 1;
                }
            }

            // Если достигли цели, начинаем отсчет ожидания
            if (IsAtTarget() && !_isWaitingAtTarget)
            {
                _isWaitingAtTarget = true;
                _reachedTargetTime = DateTime.Now;
                Debug.WriteLine($"Достигнута точка #{_explorePointCount}, ожидание 3 сек");
                return;
            }

            // Если ждем на точке - не двигаемся
            if (_isWaitingAtTarget)
            {
                TimeSpan waitTime = DateTime.Now - _reachedTargetTime;
                if (waitTime.TotalSeconds % 1 < 0.1) // Логируем каждую секунду
                {
                    Debug.WriteLine($"Ждем: {waitTime.TotalSeconds:F1}/3 сек");
                }
                return;
            }

            // Двигаемся к цели
            MoveToRoamTarget();
        }

        private float GetDistanceToTarget()
        {
            if (_roamTarget == Point.Empty) return 0;

            return (float)Math.Sqrt(
                Math.Pow(_segments[0].X - _roamTarget.X, 2) +
                Math.Pow(_segments[0].Y - _roamTarget.Y, 2));
        }

        private void MoveToRoamTarget()
        {
            float exploreSpeed = 0.03f; 

            _segments[0].X += (int)((_roamTarget.X - _segments[0].X) * exploreSpeed);
            _segments[0].Y += (int)((_roamTarget.Y - _segments[0].Y) * exploreSpeed);

            // Обновляем сегменты
            for (int i = 1; i < 7; i++)
            {
                Point previous = _segments[i - 1];
                Point current = _segments[i];

                double dx = previous.X - current.X;
                double dy = previous.Y - current.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance > 0)
                {
                    double ratio = SEGMENT_DISTANCE / distance;
                    _segments[i].X = previous.X - (int)(dx * ratio);
                    _segments[i].Y = previous.Y - (int)(dy * ratio);
                }
            }

            // Отладочная информация о расстоянии
            if (DateTime.Now.Millisecond % 100 == 0)
            {
                double distance = Math.Sqrt(
                    Math.Pow(_segments[0].X - _roamTarget.X, 2) +
                    Math.Pow(_segments[0].Y - _roamTarget.Y, 2));
                Debug.WriteLine($"Расстояние до цели: {distance:F1}px, скорость: {exploreSpeed}");
            }
        }

        private void GetNextRoutePoint()
        {
            if (_explorationRoute.Count == 0) return;

            Point currentPos = _segments[0];
            int bestIndex = -1;
            double bestDistance = double.MaxValue;

            Random rand = new Random();

            // Предпочитаем точки на разумном расстоянии (не более 1/3 экрана)
            int maxReasonableDistance = Math.Min(_screenWidth, _screenHeight) / 3;

            // Проверяем несколько случайных точек
            for (int attempt = 0; attempt < 10; attempt++)
            {
                int randomIndex = rand.Next(_explorationRoute.Count);
                Point candidate = _explorationRoute[randomIndex];

                double distance = Math.Sqrt(
                    Math.Pow(currentPos.X - candidate.X, 2) +
                    Math.Pow(currentPos.Y - candidate.Y, 2));

                // Предпочитаем точки на разумном расстоянии
                if (distance < maxReasonableDistance && distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = randomIndex;
                }
            }

            // Если не нашли подходящую точку, берем случайную
            if (bestIndex == -1)
            {
                bestIndex = rand.Next(_explorationRoute.Count);
                Point candidate = _explorationRoute[bestIndex];
                bestDistance = Math.Sqrt(
                    Math.Pow(currentPos.X - candidate.X, 2) +
                    Math.Pow(currentPos.Y - candidate.Y, 2));
            }

            _currentRouteIndex = bestIndex;
            _roamTarget = _explorationRoute[bestIndex];

            Debug.WriteLine($"Следующая точка #{_explorePointCount + 1}: {_roamTarget.X}, {_roamTarget.Y} " +
                           $"(расстояние: {bestDistance:F1}px, разумный максимум: {maxReasonableDistance}px)");
        }

        private bool IsAtTarget()
        {
            if (_roamTarget == Point.Empty) return false;

            double distance = Math.Sqrt(
                Math.Pow(_segments[0].X - _roamTarget.X, 2) +
                Math.Pow(_segments[0].Y - _roamTarget.Y, 2));

            return distance < 50; // Увеличили с 30 до 50
        }

        private bool IsAtBase()
        {
            int centerX = _basePosition.X + (_baseFormSize.Width / 2);
            int centerY = _basePosition.Y + (_baseFormSize.Height / 2);

            double distance = Math.Sqrt(
                Math.Pow(_segments[0].X - centerX, 2) +
                Math.Pow(_segments[0].Y - centerY, 2));
            return distance < 50;
        }
        #endregion

        #region Отрисовка
        private void UpdateDrawing()
        {
            if (_overlayForm != null && !_overlayForm.IsDisposed)
            {
                _overlayForm.Invoke(new Action(delegate {
                    _overlayForm.Invalidate();
                }));
            }

            if (_baseForm != null && !_baseForm.IsDisposed)
            {
                _baseForm.Invoke(new Action(delegate {
                    _baseForm.Invalidate();
                }));
            }
        }

        private void OverlayForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            // Рисуем сегменты питомца
            for (int i = 0; i < 7; i++)
            {
                DrawSegment(g, _segments[i], i);
            }
        }

        private void DrawSegment(Graphics g, Point position, int segmentIndex)
        {
            if (_sprites[segmentIndex] != null)
            {
                Bitmap sprite = _sprites[segmentIndex];

                float angle = CalculateRotationAngle(segmentIndex);

                var originalTransform = g.Transform;

                g.TranslateTransform(position.X, position.Y);
                g.RotateTransform(angle);

                int newWidth = sprite.Width / 2;
                int newHeight = sprite.Height / 2;

                g.DrawImage(sprite,
                    -newWidth / 2,
                    -newHeight / 2,
                    newWidth,
                    newHeight);

                g.Transform = originalTransform;
            }
            else
            {
                int size = (30 - segmentIndex * 3) / 2;
                using (SolidBrush brush = new SolidBrush(_themeColor))
                {
                    g.FillEllipse(brush,
                        position.X - size / 2,
                        position.Y - size / 2,
                        size, size);
                }
            }
        }

        private float CalculateRotationAngle(int segmentIndex)
        {
            if (segmentIndex == 0)
            {
                if (_currentState == PetState.Sleeping || _currentState == PetState.Idle)
                {
                    // В режиме сна смотреть вперед (вправо)
                    return 0f;
                }
                else if (_currentState == PetState.Exploring && _roamTarget != Point.Empty)
                {
                    return CalculateAngle(_segments[0], _roamTarget) + 90;
                }
                else
                {
                    return CalculateAngle(_segments[0], _targetPosition) + 90;
                }
            }
            else
            {
                return CalculateAngle(_segments[segmentIndex], _segments[segmentIndex - 1]) + 90;
            }
        }

        private float CalculateAngle(Point from, Point to)
        {
            float dx = to.X - from.X;
            float dy = to.Y - from.Y;
            return (float)(Math.Atan2(dy, dx) * (180 / Math.PI));
        }
        #endregion

        #region События
        private void BaseForm_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_currentState != PetState.Sleeping && _currentState != PetState.SentToBase)
            {
                ChangeState(PetState.SentToBase);
                LogActivity("Двойной клик по месту отдыха - отправка спать");
            }
        }
        #endregion

        #region Настройки
        public void SetIdleTimeout(int seconds)
        {
            _idleTimeout = Math.Max(5, Math.Min(60, seconds));
        }

        public void SetFreeRoam(bool enabled)
        {
            _freeRoamEnabled = enabled;
            if (!enabled && _currentState == PetState.Exploring)
            {
                ChangeState(PetState.Following);
            }
        }
        #endregion

        #region Логирование
        private void LogActivity(string message)
        {
            string logEntry = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message;
            _activityLog.Enqueue(logEntry);

            try
            {
                string logsDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }

                string logFile = Path.Combine(logsDir, "pet_" + _skinId.ToString() + "_activity.log");
                File.AppendAllText(logFile, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка записи лога: " + ex.Message);
            }

            if (_activityLog.Count > 100)
            {
                _activityLog.Dequeue();
            }
        }

        public string[] GetRecentActivity()
        {
            return _activityLog.ToArray();
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Stop();

            foreach (Bitmap sprite in _sprites)
            {
                if (sprite != null)
                {
                    sprite.Dispose();
                }
            }

            if (_overlayForm != null)
            {
                _overlayForm.Dispose();
            }

            if (_baseForm != null)
            {
                _baseForm.Dispose();
            }
        }
        #endregion
    }
}