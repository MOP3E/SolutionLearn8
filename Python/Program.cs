using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Python
{
    internal class Program
    {
        /// <summary>
        /// Размер игрового поля в логических единицах.
        /// </summary>
        private static readonly Vector2Int GameFieldSize = new Vector2Int { X = 40, Y = 40 };

        /// <summary>
        /// Множитель для пересчёта игровых координат в координаты знакомест консоли.
        /// </summary>
        private static readonly Vector2Int ConsoleMultiplier = new Vector2Int { X = 2, Y = 1 };

        /// <summary>
        /// Квадратный пиксель для вывода в консоль.
        /// </summary>
        private const string Square = "██";

        /// <summary>
        /// Квадратный пиксель для стриания в консоли.
        /// </summary>
        private const string ClearSquare = "  ";

        /// <summary>
        /// Игровой бог.
        /// </summary>
        private static Random _random;

        /// <summary>
        /// Координаты питона.
        /// </summary>
        private static List<Vector2Int> _pythonCoords = new List<Vector2Int>();
    
        /// <summary>
        /// Периотд движения питона, мс.
        /// </summary>
        private static int _pythonPeriod;
    
        /// <summary>
        /// Время, прошедшее с момента последнего движения питона, мс.
        /// </summary>
        private static long _pythonDelta;

        /// <summary>
        /// Количество клеток, на которые должен вырасти питон.
        /// </summary>
        private static int _pythonGrow;

        /// <summary>
        /// Текущее направление движения головы питона.
        /// </summary>
        private static HeadDirection _pythonHeadDirection = HeadDirection.Right;
        
        
        /// <summary>
        /// Следующее направление движения головы питона.
        /// </summary>
        private static HeadDirection _pythonNextHeadDirection = HeadDirection.Right;

        /// <summary>
        /// Очки игрока.
        /// </summary>
        private static int _playerScore = 0;

        /// <summary>
        /// Рекорд игрока.
        /// </summary>
        private static int _playerRecord = 0;
        
        /// <summary>
        /// Звук съедания еды питоном.
        /// </summary>
        private static string _eatSound;

        /// <summary>
        /// Звук столкновения питона с самим собой.
        /// </summary>
        private static string _crashSound;

        /// <summary>
        /// Координаты еды.
        /// </summary>
        private static Vector2Int _foodCoord;

        /// <summary>
        /// Текущий режим игры.
        /// </summary>
        private static Mode _mode = Mode.StartScreen;

        /// <summary>
        /// Список красных квадратов на неигровом экране.
        /// </summary>
        private static Dictionary<ConsoleColor, List<Vector2Int>> _colorSquares = new Dictionary<ConsoleColor, List<Vector2Int>>();
            
        /// <summary>
        /// Период движения крастных квадратиков, мс.
        /// </summary>
        private static int _borderPeriod = 150;
    
        /// <summary>
        /// Время, прошедшее с момента последнего движения красных квадратиков.
        /// </summary>
        private static long _borderDelta;

        /// <summary>
        /// Музыка в неигровых экранах.
        /// </summary>
        private static string _nongameMusic;

        /// <summary>
        /// Квадраты, которые нужно стереть в консоли.
        /// </summary>
        private static Dictionary<Vector2Int, ConsoleColor> _squaresToClear = new Dictionary<Vector2Int, ConsoleColor>();

        /// <summary>
        /// Квадраты, которые нужно нарисовать в консоли.
        /// </summary>
        private static Dictionary<Vector2Int, ConsoleColor> _squaresToDraw = new Dictionary<Vector2Int, ConsoleColor>();

        /// <summary>
        /// Разрешить выполнение главного цикла игры.
        /// </summary>
        private static bool _game = true;

        /// <summary>
        /// Таймер для измерения времени игры.
        /// </summary>
        private static Stopwatch _timer = new Stopwatch();

        /// <summary>
        /// Время, зафиксированное в начале цикла, мс.
        /// </summary>
        private static long _startTime;

        /// <summary>
        /// Время, прошедшее с начала предыдущего цикла, мс.
        /// </summary>
        private static long _gameDelta;

        /// <summary>
        /// Фикисированный период игрового цикла, мс.
        /// </summary>
        private const int Period = 50;

        static void Main(string[] args)
        {
            //установить заголовок окна консоли
            Console.Title = "Python";
            //установить для консоли требуемый шрифт
            ConsoleFont.SetFont("Consolas");
            //изменить размер консоли
            Console.SetWindowSize(80, 41);
            Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
            //отключить курсор
            Console.CursorVisible = false;

            //проинициализировать игрового бога
            _random = new Random((int)(DateTime.Now.Ticks & 0xFFFFFFFF));

            //рассчитать положение квадратиков рамки неигрового экрана
            _colorSquares.Add(ConsoleColor.Magenta, CalculateBorder(0));
            _colorSquares.Add(ConsoleColor.Blue, CalculateBorder(2));
            _colorSquares.Add(ConsoleColor.Cyan, CalculateBorder(4));
            _colorSquares.Add(ConsoleColor.Green, CalculateBorder(6));
            _colorSquares.Add(ConsoleColor.Yellow, CalculateBorder(8));
            _colorSquares.Add(ConsoleColor.Red, CalculateBorder(10));

            //задать режим стартового экрана и нарисовать стартовый экран
            _mode = Mode.StartScreen;
            StartScreenInit();

            //запустить игровой таймер
            _timer.Start();
            _startTime = _timer.ElapsedMilliseconds;

            //главный цикл игры
            while (_game)
            {
                //рассчитать время, прошедшее с начала предыдущего игрового цикла и зафиксировать текущее время
                _gameDelta = _timer.ElapsedMilliseconds - _startTime;
                _startTime = _timer.ElapsedMilliseconds;
                
                //обработка нажатий на кнопки
                while (Console.KeyAvailable)
                {
                    switch (Console.ReadKey(true).Key)
                    {
                        case ConsoleKey.Escape:
                            //нажатие на кнопку выхода из игры
                            switch (_mode)
                            {
                                case Mode.StartScreen:
                                case Mode.GamoverScreen:
                                    //игра не идёт - выйти из игры
                                    _game = false;
                                    break;
                                case Mode.Game:
                                    //идёт игра - выйти в начальный экран
                                    _mode = Mode.StartScreen;
                                    StartScreenInit();
                                    break;
                            }
                            break;
                        case ConsoleKey.Enter:
                            //нажатие на кнопку перехода в другой игровой режим
                            switch (_mode)
                            {
                                case Mode.StartScreen:
                                    _mode = Mode.Game;
                                    GameStart();
                                    break;
                                case Mode.GamoverScreen:
                                    _mode = Mode.StartScreen;
                                    StartScreenInit();
                                    break;
                            }
                            break;
                        case ConsoleKey.LeftArrow:
                        case ConsoleKey.NumPad4:
                            //команда движения питона влево
                            if (_pythonHeadDirection != HeadDirection.Right) _pythonNextHeadDirection = HeadDirection.Left;
                            break;
                        case ConsoleKey.RightArrow:
                        case ConsoleKey.NumPad6:
                            //команда движения питона вправо
                            if (_pythonHeadDirection != HeadDirection.Left) _pythonNextHeadDirection = HeadDirection.Right;
                            break;
                        case ConsoleKey.UpArrow:
                        case ConsoleKey.NumPad8:
                            //команда движения питона вверх
                            if (_pythonHeadDirection != HeadDirection.Down) _pythonNextHeadDirection = HeadDirection.Up;
                            break;
                        case ConsoleKey.DownArrow:
                        case ConsoleKey.NumPad2:
                            //команда движения питона вниз
                            if (_pythonHeadDirection != HeadDirection.Up) _pythonNextHeadDirection = HeadDirection.Down;
                            break;
                    }

                }

                //игровая логика
                switch (_mode)
                {
                    case Mode.StartScreen:
                        //начальный экран игры
                        BorderIdle();
                        break;
                    case Mode.Game:
                        //игра
                        PythonMove();
                        break;
                    case Mode.GamoverScreen:
                        //экран окончания игры
                        BorderIdle();
                        break;
                }

                //Перерисовка экрана
                //стереть квадраты
                foreach (KeyValuePair<Vector2Int, ConsoleColor> pair in _squaresToClear)
                    WriteXY(pair.Key * ConsoleMultiplier, ClearSquare);
                _squaresToClear.Clear();
                //нарисовать квадраты
                foreach (KeyValuePair<Vector2Int, ConsoleColor> pair in _squaresToDraw)
                    WriteXY(pair.Key * ConsoleMultiplier, Square, pair.Value);
                _squaresToDraw.Clear();

                //ожидание до конца периода
                long timeRemains = Period - (_timer.ElapsedMilliseconds - _startTime) - 5;
                if (timeRemains > 0 && timeRemains < int.MaxValue) Thread.Sleep((int)timeRemains);
            }
        }

        /// <summary>
        /// Действия по иницализации начального экрана игры
        /// </summary>
        private static void StartScreenInit()
        {
            //запустить музыку
            //PlayMusic("_nongameMusic");
            
            //очистить экран и списки отрисовки квадратов
            Console.Clear();
            _squaresToDraw.Clear();
            _squaresToClear.Clear();
            
            //нарисовать рамку окна
            _borderDelta = 0;
            foreach (KeyValuePair<ConsoleColor, List<Vector2Int>> pair in _colorSquares)
            {
                foreach (Vector2Int square in pair.Value)
                {
                    if (_squaresToDraw.ContainsKey(square))
                        _squaresToDraw[square] = pair.Key;
                    else
                        _squaresToDraw.Add(square, pair.Key);
                }
            }

            //нарисовать заголовок окна и справочную информацию
            List<ConsoleColor> colors = new List<ConsoleColor>
                { ConsoleColor.Red, ConsoleColor.Yellow, ConsoleColor.Green, ConsoleColor.Cyan, ConsoleColor.Blue, ConsoleColor.Magenta, ConsoleColor.White };
            //P
            int colorIndex = _random.Next(colors.Count);
            ConsoleColor color = colors[colorIndex];
            colors.RemoveAt(colorIndex);
            WriteXY(new Vector2Int(5,16), "████████", color);
            WriteXY(new Vector2Int(5,17), "██      ██", color);
            WriteXY(new Vector2Int(5,18), "██      ██", color);
            WriteXY(new Vector2Int(5,19), "████████", color);
            WriteXY(new Vector2Int(5,20), "██", color);
            WriteXY(new Vector2Int(5,21), "██", color);
            WriteXY(new Vector2Int(5,22), "██", color);
            //Y
            colorIndex = _random.Next(colors.Count);
            color = colors[colorIndex];
            colors.RemoveAt(colorIndex);
            WriteXY(new Vector2Int(17,16), "██      ██", color);
            WriteXY(new Vector2Int(17,17), "██      ██", color);
            WriteXY(new Vector2Int(17,18), "  ██  ██", color);
            WriteXY(new Vector2Int(17,19), "    ██", color);
            WriteXY(new Vector2Int(17,20), "    ██", color);
            WriteXY(new Vector2Int(17,21), "    ██", color);
            WriteXY(new Vector2Int(17,22), "    ██", color);
            //T
            colorIndex = _random.Next(colors.Count);
            color = colors[colorIndex];
            colors.RemoveAt(colorIndex);
            WriteXY(new Vector2Int(29,16), "██████████", color);
            WriteXY(new Vector2Int(29,17), "    ██", color);
            WriteXY(new Vector2Int(29,18), "    ██", color);
            WriteXY(new Vector2Int(29,19), "    ██", color);
            WriteXY(new Vector2Int(29,20), "    ██", color);
            WriteXY(new Vector2Int(29,21), "    ██", color);
            WriteXY(new Vector2Int(29,22), "    ██", color);
            //H
            colorIndex = _random.Next(colors.Count);
            color = colors[colorIndex];
            colors.RemoveAt(colorIndex);
            WriteXY(new Vector2Int(41,16), "██      ██", color);
            WriteXY(new Vector2Int(41,17), "██      ██", color);
            WriteXY(new Vector2Int(41,18), "██      ██", color);
            WriteXY(new Vector2Int(41,19), "██████████", color);
            WriteXY(new Vector2Int(41,20), "██      ██", color);
            WriteXY(new Vector2Int(41,21), "██      ██", color);
            WriteXY(new Vector2Int(41,22), "██      ██", color);
            //O
            colorIndex = _random.Next(colors.Count);
            color = colors[colorIndex];
            colors.RemoveAt(colorIndex);
            WriteXY(new Vector2Int(53,16), "  ██████", color);
            WriteXY(new Vector2Int(53,17), "██      ██", color);
            WriteXY(new Vector2Int(53,18), "██      ██", color);
            WriteXY(new Vector2Int(53,19), "██      ██", color);
            WriteXY(new Vector2Int(53,20), "██      ██", color);
            WriteXY(new Vector2Int(53,21), "██      ██", color);
            WriteXY(new Vector2Int(53,22), "  ██████", color);
            //N
            colorIndex = _random.Next(colors.Count);
            color = colors[colorIndex];
            colors.RemoveAt(colorIndex);
            WriteXY(new Vector2Int(65,16), "██      ██", color);
            WriteXY(new Vector2Int(65,17), "██      ██", color);
            WriteXY(new Vector2Int(65,18), "████    ██", color);
            WriteXY(new Vector2Int(65,19), "██  ██  ██", color);
            WriteXY(new Vector2Int(65,20), "██    ████", color);
            WriteXY(new Vector2Int(65,21), "██      ██", color);
            WriteXY(new Vector2Int(65,22), "██      ██", color);

            Vector2Int pos = new Vector2Int(0, 13);
            string text = $"YOUR SCORE {_playerScore}";
            int left = (Console.BufferWidth - text.Length) / 2;
            pos.X = left;
            WriteXY(pos, "YOUR SCORE", ConsoleColor.Cyan);
            pos.X += 11;
            WriteXY(pos, _playerScore.ToString(), ConsoleColor.Magenta);
            pos.Y = 11;
            text = $"RECORD {_playerRecord}";
            left = (Console.BufferWidth - text.Length) / 2;
            pos.X = left;
            WriteXY(pos, "RECORD", ConsoleColor.Cyan);
            pos.X += 7;
            WriteXY(pos, _playerRecord.ToString(), ConsoleColor.Magenta);

            pos.Y = 25;
            text = "PRESS [ENTER] TO START";
            left = (Console.BufferWidth - text.Length) / 2;
            pos.X = left;
            WriteXY(pos, text, ConsoleColor.Cyan);
            pos.Y = 27;
            text = "PRESS [ESCAPE] TO EXIT";
            left = (Console.BufferWidth - text.Length) / 2;
            pos.X = left;
            WriteXY(pos, text, ConsoleColor.Cyan);
        }

        private static void GamoverScreenInit()
        {
            //запустить музыку
            //PlayMusic("_nongameMusic");
            
            //очистить экран и списки отрисовки квадратов
            Console.Clear();
            _squaresToDraw.Clear();
            _squaresToClear.Clear();
            
            //нарисовать рамку окна
            _borderDelta = 0;
            foreach (KeyValuePair<ConsoleColor, List<Vector2Int>> pair in _colorSquares)
            {
                foreach (Vector2Int square in pair.Value)
                {
                    if (_squaresToDraw.ContainsKey(square))
                        _squaresToDraw[square] = pair.Key;
                    else
                        _squaresToDraw.Add(square, pair.Key);
                }
            }
            
            //нарисовать заголовок окна и справочную информацию
            List<ConsoleColor> colors = new List<ConsoleColor>
                { ConsoleColor.Red, ConsoleColor.Yellow, ConsoleColor.Green, ConsoleColor.Cyan, ConsoleColor.Blue, ConsoleColor.Magenta, ConsoleColor.White };
            //в слове "GAME OVER" 8 букв, ярких цветов только 7, поэтому добавляю ещё один цвет случайным образом
            int colorIndex = _random.Next(colors.Count);
            colors.Add(colors[colorIndex]);

            //G
            colorIndex = _random.Next(colors.Count);
            ConsoleColor color = colors[colorIndex];
            colors.RemoveAt(colorIndex);
            WriteXY(new Vector2Int(17,10), "  ████████", color);
            WriteXY(new Vector2Int(17,11), "██", color);
            WriteXY(new Vector2Int(17,12), "██", color);
            WriteXY(new Vector2Int(17,13), "██", color);
            WriteXY(new Vector2Int(17,14), "██    ████", color);
            WriteXY(new Vector2Int(17,15), "██      ██", color);
            WriteXY(new Vector2Int(17,16), "  ████████", color);
            //A
            colorIndex = _random.Next(colors.Count);
            color = colors[colorIndex];
            colors.RemoveAt(colorIndex);
            WriteXY(new Vector2Int(29,10), "    ██", color);
            WriteXY(new Vector2Int(29,11), "  ██  ██", color);
            WriteXY(new Vector2Int(29,12), "██      ██", color);
            WriteXY(new Vector2Int(29,13), "██      ██", color);
            WriteXY(new Vector2Int(29,14), "██████████", color);
            WriteXY(new Vector2Int(29,15), "██      ██", color);
            WriteXY(new Vector2Int(29,16), "██      ██", color);
            //M
            colorIndex = _random.Next(colors.Count);
            color = colors[colorIndex];
            colors.RemoveAt(colorIndex);
            WriteXY(new Vector2Int(41,10), "██      ██", color);
            WriteXY(new Vector2Int(41,11), "████  ████", color);
            WriteXY(new Vector2Int(41,12), "██  ██  ██", color);
            WriteXY(new Vector2Int(41,13), "██  ██  ██", color);
            WriteXY(new Vector2Int(41,14), "██      ██", color);
            WriteXY(new Vector2Int(41,15), "██      ██", color);
            WriteXY(new Vector2Int(41,16), "██      ██", color);
            //E
            colorIndex = _random.Next(colors.Count);
            color = colors[colorIndex];
            colors.RemoveAt(colorIndex);
            WriteXY(new Vector2Int(53,10), "██████████", color);
            WriteXY(new Vector2Int(53,11), "██", color);
            WriteXY(new Vector2Int(53,12), "██", color);
            WriteXY(new Vector2Int(53,13), "████████", color);
            WriteXY(new Vector2Int(53,14), "██", color);
            WriteXY(new Vector2Int(53,15), "██", color);
            WriteXY(new Vector2Int(53,16), "██████████", color);
            //O
            colorIndex = _random.Next(colors.Count);
            color = colors[colorIndex];
            colors.RemoveAt(colorIndex);
            WriteXY(new Vector2Int(17,20), "  ██████", color);
            WriteXY(new Vector2Int(17,21), "██      ██", color);
            WriteXY(new Vector2Int(17,22), "██      ██", color);
            WriteXY(new Vector2Int(17,23), "██      ██", color);
            WriteXY(new Vector2Int(17,24), "██      ██", color);
            WriteXY(new Vector2Int(17,25), "██      ██", color);
            WriteXY(new Vector2Int(17,26), "  ██████", color);
            //V
            colorIndex = _random.Next(colors.Count);
            color = colors[colorIndex];
            colors.RemoveAt(colorIndex);
            WriteXY(new Vector2Int(29,20), "██      ██", color);
            WriteXY(new Vector2Int(29,21), "██      ██", color);
            WriteXY(new Vector2Int(29,22), "██      ██", color);
            WriteXY(new Vector2Int(29,23), "██      ██", color);
            WriteXY(new Vector2Int(29,24), "██      ██", color);
            WriteXY(new Vector2Int(29,25), "  ██  ██", color);
            WriteXY(new Vector2Int(29,26), "    ██", color);
            //E
            colorIndex = _random.Next(colors.Count);
            color = colors[colorIndex];
            colors.RemoveAt(colorIndex);
            WriteXY(new Vector2Int(41,20), "██████████", color);
            WriteXY(new Vector2Int(41,21), "██", color);
            WriteXY(new Vector2Int(41,22), "██", color);
            WriteXY(new Vector2Int(41,23), "████████", color);
            WriteXY(new Vector2Int(41,24), "██", color);
            WriteXY(new Vector2Int(41,25), "██", color);
            WriteXY(new Vector2Int(41,26), "██████████", color);
            //R
            color = colors[0];
            WriteXY(new Vector2Int(53,20), "████████", color);
            WriteXY(new Vector2Int(53,21), "██      ██", color);
            WriteXY(new Vector2Int(53,22), "██      ██", color);
            WriteXY(new Vector2Int(53,23), "████████", color);
            WriteXY(new Vector2Int(53,24), "██  ██", color);
            WriteXY(new Vector2Int(53,25), "██    ██", color);
            WriteXY(new Vector2Int(53,26), "██      ██", color);
      
            Vector2Int pos = new Vector2Int(0, 7);
            string text = $"YOUR SCORE {_playerScore}";
            int left = (Console.BufferWidth - text.Length) / 2;
            pos.X = left;
            WriteXY(pos, "YOUR SCORE", ConsoleColor.Cyan);
            pos.X += 11;
            WriteXY(pos, _playerScore.ToString(), ConsoleColor.Magenta);
            pos.Y = 5;
            text = $"RECORD {_playerRecord}";
            left = (Console.BufferWidth - text.Length) / 2;
            pos.X = left;
            WriteXY(pos, "RECORD", ConsoleColor.Cyan);
            pos.X += 7;
            WriteXY(pos, _playerRecord.ToString(), ConsoleColor.Magenta);

            pos.Y = 29;
            text = "PRESS [ENTER] TO START SCREEN";
            left = (Console.BufferWidth - text.Length) / 2;
            pos.X = left;
            WriteXY(pos, text, ConsoleColor.Cyan);
            pos.Y = 31;
            text = "PRESS [ESCAPE] TO EXIT";
            left = (Console.BufferWidth - text.Length) / 2;
            pos.X = left;
            WriteXY(pos, text, ConsoleColor.Cyan);
        }

        /// <summary>
        /// Расчёт координат квадратиков рамки неигрового экрана.
        /// </summary>
        private static List<Vector2Int> CalculateBorder(int start)
        {
            //квадраты рамки
            List<Vector2Int> squares = new List<Vector2Int>();
            int c = 0;
            const int step = 12;
            //верхняя линия
            for (int i = start; i < GameFieldSize.X; i++)
            {
                Vector2Int coord = new Vector2Int(i, 0);
                if (c % step == 0)
                {
                    //сохранить координаты цветного квадратика
                    squares.Add(coord);
                }
                c++;
            }
            //правая линия
            for (int i = 1; i < GameFieldSize.Y; i++)
            {
                Vector2Int coord = new Vector2Int(GameFieldSize.X - 1, i);
                if (c % step == 0)
                {
                    //сохранить координаты цветного квадратика
                    squares.Add(coord);
                }
                c++;
            }
            //нижняя линия
            for (int i = GameFieldSize.X - 2; i >= 0; i--)
            {
                Vector2Int coord = new Vector2Int(i, GameFieldSize.Y - 1);
                if (c % step == 0)
                {
                    //сохранить координаты цветного квадратика
                    squares.Add(coord);
                }
                c++;
            }
            //левая линия
            for (int i = GameFieldSize.Y - 2; i >= 1; i--)
            {
                Vector2Int coord = new Vector2Int(0, i);
                if (c % step == 0)
                {
                    //сохранить координаты цветного квадратика
                    squares.Add(coord);
                }
                c++;
            }

            return squares;
        }

        /// <summary>
        /// Движение элементов рамки неигрового окна.
        /// </summary>
        private static void BorderIdle()
        {
            //оценить потраченное время, и не пора ли двигать дальше квадратики
            _borderDelta += _gameDelta;
            if(_borderDelta < _borderPeriod) return;
            _borderDelta -= _borderPeriod;

            //переместиться на одну позицию дальше по рамке
            foreach (KeyValuePair<ConsoleColor, List<Vector2Int>> pair in _colorSquares)
            {
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    //красные клетки движутся по рамке по часовой стрелке
                    if (pair.Value[i].X == 0)
                    {
                        //движение по вертикали, снизу вверх
                        Vector2Int newCoord = new Vector2Int(pair.Value[i].X, pair.Value[i].Y - 1);
                        if (newCoord.Y < 0)
                        {
                            newCoord.Y = 0;
                            newCoord.X++;
                        }

                        pair.Value[i] = newCoord;
                        if (_squaresToDraw.ContainsKey(newCoord))
                            _squaresToDraw[newCoord] = pair.Key;
                        else
                            _squaresToDraw.Add(newCoord, pair.Key);
                    }
                    else if (pair.Value[i].X == GameFieldSize.X - 1)
                    {
                        //движение по вертикали, сверху вниз
                        Vector2Int newCoord = new Vector2Int(pair.Value[i].X, pair.Value[i].Y + 1);
                        if (newCoord.Y > GameFieldSize.Y - 1)
                        {
                            newCoord.Y = GameFieldSize.Y - 1;
                            newCoord.X--;
                        }

                        pair.Value[i] = newCoord;
                        if (_squaresToDraw.ContainsKey(newCoord))
                            _squaresToDraw[newCoord] = pair.Key;
                        else
                            _squaresToDraw.Add(newCoord, pair.Key);
                    }
                    else if (pair.Value[i].Y == 0)
                    {
                        //движение по горизонтали, слева направо
                        Vector2Int newCoord = new Vector2Int(pair.Value[i].X + 1, pair.Value[i].Y);
                        if (newCoord.X > GameFieldSize.X - 1)
                        {
                            newCoord.X = GameFieldSize.X - 1;
                            newCoord.Y++;
                        }

                        pair.Value[i] = newCoord;
                        if (_squaresToDraw.ContainsKey(newCoord))
                            _squaresToDraw[newCoord] = pair.Key;
                        else
                            _squaresToDraw.Add(newCoord, pair.Key);
                    }
                    else if (pair.Value[i].Y == GameFieldSize.Y - 1)
                    {
                        //движение по горизонтали, справа налево
                        Vector2Int newCoord = new Vector2Int(pair.Value[i].X - 1, pair.Value[i].Y);
                        if (newCoord.X < 0)
                        {
                            newCoord.X = 0;
                            newCoord.Y--;
                        }

                        pair.Value[i] = newCoord;
                        if (_squaresToDraw.ContainsKey(newCoord))
                            _squaresToDraw[newCoord] = pair.Key;
                        else
                            _squaresToDraw.Add(newCoord, pair.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Действия по инициализации игры.
        /// </summary>
        private static void GameStart()
        {
            //остановить музыку
            //StopMusic("_nongameMusic");
            //очистить экран и списки отрисовки квадратов
            Console.Clear();
            _squaresToDraw.Clear();
            _squaresToClear.Clear();

            //проиницализировать и нарисовать питона
            _pythonPeriod = 250;
            _pythonDelta = 0;
            _pythonGrow = 0;
            _pythonHeadDirection = HeadDirection.Up;
            _pythonNextHeadDirection = HeadDirection.Up;
            _pythonCoords.Clear();
            _pythonCoords.AddRange(new []
            {
                new Vector2Int(20, 30),
                new Vector2Int(20, 31),
                new Vector2Int(20, 32),
                new Vector2Int(20, 33),
                new Vector2Int(20, 34)
            });
            foreach (Vector2Int coord in _pythonCoords)
            {
                if (_squaresToDraw.ContainsKey(coord))
                    _squaresToDraw[coord] = ConsoleColor.Yellow;
                else
                    _squaresToDraw.Add(coord, ConsoleColor.Yellow);
  
            }

            //сбросить очки игрока
            _playerScore = 0;

            //разместить еду
            PlaceFood();

            //отрисовать текущие очки
            WriteXY(new Vector2Int(0, 40), "SCORE:", ConsoleColor.Cyan);
            WriteXY(new Vector2Int(7, 40), "0", ConsoleColor.Magenta);

            //переключиться в игровой режим
            _mode = Mode.Game;
        }

        /// <summary>
        /// Перемещение питона
        /// </summary>
        private static void PythonMove()
        {
            //оценить потраченное время, и не пора ли двигать дальше питона
            _pythonDelta += _gameDelta;
            if(_pythonDelta < _pythonPeriod) return;
            _pythonDelta -= _pythonPeriod;

            //движение питона
            //стереть клетку хвоста
            if (_pythonGrow > 0)
                //питон растёт - не стирать клетку хвоста, а вместо этого уменьшить счётчик роста на 1
                _pythonGrow--;
            else
            {
                //питон ползёт - стереть клетку хвоста
                if(!_squaresToClear.ContainsKey(_pythonCoords[_pythonCoords.Count-1]))
                    _squaresToClear.Add(_pythonCoords[_pythonCoords.Count - 1], ConsoleColor.Black);
                _pythonCoords.RemoveAt(_pythonCoords.Count - 1);
            }

            //нарисовать новую клетку головы
            Vector2Int newCoord = _pythonCoords[0];
            switch (_pythonNextHeadDirection)
            {
                case HeadDirection.Left:
                    newCoord.X--;
                    if (newCoord.X < 0) newCoord.X += GameFieldSize.X;
                    break;
                case HeadDirection.Up:
                    newCoord.Y--;
                    if (newCoord.Y < 0) newCoord.Y += GameFieldSize.Y;
                    break;
                case HeadDirection.Right:
                    newCoord.X++;
                    if (newCoord.X > GameFieldSize.X - 1) newCoord.X -= GameFieldSize.X;
                    break;
                case HeadDirection.Down:
                    newCoord.Y++;
                    if (newCoord.Y > GameFieldSize.Y - 1) newCoord.Y -= GameFieldSize.Y;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            //сохранить направление движения питона и нарисовать голову питона
            _pythonHeadDirection = _pythonNextHeadDirection;

            //если питон достиг еды - увеличить счётчик очков, скорость питона и создать новую еду
            if (newCoord == _foodCoord)
            {
                _playerScore++;
                _pythonGrow += 2;
                //период обновления игры порядка 50 мс, поэтому меньше 50 мс период питона делать нельзя
                if(_pythonPeriod > 50) _pythonPeriod -= 10;
                WriteXY(new Vector2Int(7, 40), _playerScore.ToString(), ConsoleColor.Magenta);
                PlaceFood();
            }
            else
            {
                //если питон сожрал сам себя - игра заканчивается
                foreach (Vector2Int coord in _pythonCoords)
                {
                    if (coord == newCoord)
                    {
                        _mode = Mode.GamoverScreen;
                        if (_playerScore > _playerRecord) _playerRecord = _playerScore;
                        GamoverScreenInit();
                        return;
                    }
                }
            }
            //добавить новую координату головы питона в начало списка координат питона
            _pythonCoords.Insert(0, newCoord);
            //нарисовать голову питона
            if (_squaresToDraw.ContainsKey(newCoord))
                _squaresToDraw[newCoord] = ConsoleColor.Yellow;
            else
                _squaresToDraw.Add(newCoord, ConsoleColor.Yellow);
        }
        
        /// <summary>
        /// Размещение еды.
        /// </summary>
        private static void PlaceFood()
        {
            while (true)
            {
                //разместить новую еду
                _foodCoord.X = _random.Next(0, GameFieldSize.X);
                _foodCoord.Y = _random.Next(0, GameFieldSize.Y);
                //проверить, не совпадает ли положение еды с телом питона
                bool founded = false;
                foreach (Vector2Int coord in _pythonCoords)
                {
                    if (coord == _foodCoord)
                    {
                        founded = true;
                        break;
                    }
                }

                //еду можно располагать на расстоянии не ближе двух клеток в направлении движения головы питона
                if (!founded)
                {
                    Vector2Int foodTest = _pythonCoords[0];
                    for (int i = 0; i < 2; i++)
                    {
                        switch (_pythonNextHeadDirection)
                        {
                            case HeadDirection.Left:
                                foodTest.X--;
                                if (foodTest.X < 0) foodTest.X += GameFieldSize.X;
                                break;
                            case HeadDirection.Up:
                                foodTest.Y--;
                                if (foodTest.Y < 0) foodTest.Y += GameFieldSize.Y;
                                break;
                            case HeadDirection.Right:
                                foodTest.X++;
                                if (foodTest.X > GameFieldSize.X - 1) foodTest.X -= GameFieldSize.X;
                                break;
                            case HeadDirection.Down:
                                foodTest.Y++;
                                if (foodTest.Y > GameFieldSize.Y - 1) foodTest.Y -= GameFieldSize.Y;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        if (foodTest == _foodCoord)
                        {
                            founded = true;
                            break;
                        }
                    }
                }

                //если еда в разрешённом месте - закончить работу
                if(!founded) break;
            }

            //нарисовать еду
            if (_squaresToDraw.ContainsKey(_foodCoord))
                _squaresToDraw[_foodCoord] = ConsoleColor.Red;
            else
                _squaresToDraw.Add(_foodCoord, ConsoleColor.Red);
        }

        /// <summary>
        /// Вывод в консоли текста в заданной позиции.
        /// </summary>
        /// <param name="coord">Координаты, в которых нужно выводить текст.</param>
        /// <param name="s">Выводимый текст.</param>
        /// <param name="fore">Цвет переднего плана.</param>
        /// <param name="back">Цвет заднего плана.</param>
        private static void WriteXY(Vector2Int coord, string s = "", ConsoleColor fore = ConsoleColor.White, ConsoleColor back = ConsoleColor.Black)
        {
            Console.CursorLeft = coord.X;
            Console.CursorTop = coord.Y;
            Console.ForegroundColor = fore;
            Console.BackgroundColor = back;
            Console.Write(s);
        }
    }
}
