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
        private List<Vector2Int> _pythonCoords = new List<Vector2Int>();
    
        /// <summary>
        /// Скорость движения питона.
        /// </summary>
        private static float _pythonSpeed = 100;
    
        /// <summary>
        /// Время, прошедшее с момента последнего движения питона.
        /// </summary>
        private static float _pythonDelta;
        
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
        private static float _borderPeriod = 150;
    
        /// <summary>
        /// Время, прошедшее с момента последнего движения красных квадратиков.
        /// </summary>
        private static float _borderDelta;

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
                    switch (Console.ReadKey().Key)
                    {
                        case ConsoleKey.Escape:
                            //нажатие на кнопку выхода из игры
                            _game = false;
                            break;
                        case ConsoleKey.LeftArrow:
                        case ConsoleKey.NumPad4:
                            //команда движения питона влево
                            if (_pythonNextHeadDirection != HeadDirection.Right) _pythonNextHeadDirection = HeadDirection.Left;
                            break;
                        case ConsoleKey.RightArrow:
                        case ConsoleKey.NumPad6:
                            //команда движения питона вправо
                            if (_pythonNextHeadDirection != HeadDirection.Left) _pythonNextHeadDirection = HeadDirection.Right;
                            break;
                        case ConsoleKey.UpArrow:
                        case ConsoleKey.NumPad8:
                            //команда движения питона вверх
                            if (_pythonNextHeadDirection != HeadDirection.Down) _pythonNextHeadDirection = HeadDirection.Up;
                            break;
                        case ConsoleKey.DownArrow:
                        case ConsoleKey.NumPad2:
                            //команда движения питона вниз
                            if (_pythonNextHeadDirection != HeadDirection.Up) _pythonNextHeadDirection = HeadDirection.Down;
                            break;
                    }

                }


                //игровая логика
                switch (_mode)
                {
                    case Mode.StartScreen:
                        //начальный экран игры
                        //if (GetKey(Keyboard.Key.Return)) GameStart();
                        BorderIdle();
                        break;
                    case Mode.Game:
                        //игра
                        break;
                    case Mode.GamoverScreen:
                        //экран окончания игры
                        //if (GetKey(Keyboard.Key.Return)) _mode = Mode.StartScreen;
                        break;
                }

                //Перерисовка экрана
                ////стереть квадраты
                //foreach (KeyValuePair<Vector2Int, ConsoleColor> pair in _squaresToClear)
                //    WriteXY(pair.Key * ConsoleMultiplier, ClearSquare);
                //_squaresToClear.Clear();
                //нарисовать квадраты
                foreach (KeyValuePair<Vector2Int, ConsoleColor> pair in _squaresToDraw)
                    WriteXY(pair.Key * ConsoleMultiplier, Square, pair.Value);
                _squaresToDraw.Clear();
                //нарисовать всё остальное, что есть на экране
                switch (_mode)
                {
                    case Mode.StartScreen:
                        //ничего рисовать не надо
                        break;
                    case Mode.Game:
                        //нарисовать текущие очки игрока
                        break;
                    case Mode.GamoverScreen:
                        //ничего рисовать не надо
                        break;
                }

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
        }

        private static void GamoverScreenInit()
        {
            //запустить музыку
            //PlayMusic("_nongameMusic");
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
            WriteXY(new Vector2Int(17,16), "  ████████", color);
            WriteXY(new Vector2Int(17,17), "██", color);
            WriteXY(new Vector2Int(17,18), "██", color);
            WriteXY(new Vector2Int(17,19), "██", color);
            WriteXY(new Vector2Int(17,20), "██    ████", color);
            WriteXY(new Vector2Int(17,21), "██      ██", color);
            WriteXY(new Vector2Int(17,22), "  ████████", color);
            //A
            colorIndex = _random.Next(colors.Count);
            color = colors[colorIndex];
            colors.RemoveAt(colorIndex);
            WriteXY(new Vector2Int(29,16), "    ██", color);
            WriteXY(new Vector2Int(29,17), "  ██  ██", color);
            WriteXY(new Vector2Int(29,18), "██      ██", color);
            WriteXY(new Vector2Int(29,19), "██      ██", color);
            WriteXY(new Vector2Int(29,20), "██████████", color);
            WriteXY(new Vector2Int(29,21), "██      ██", color);
            WriteXY(new Vector2Int(29,22), "██      ██", color);
            //M
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
            //E
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
        }

        /// <summary>
        /// Расчёт координат квадратиков рамки неигрового экрана.
        /// </summary>
        private static List<Vector2Int> CalculateBorder(int start)
        {
            //очистить экран
            Console.Clear();

            //нарисовать рамку
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
            WriteXY(new Vector2Int(5,5), _gameDelta + "  ");
            _borderDelta += _gameDelta;
            if(_borderDelta < _borderPeriod) return;
            _borderDelta -= _borderPeriod;

            //почистить текущие клетки
            foreach (KeyValuePair<ConsoleColor, List<Vector2Int>> pair in _colorSquares)
            {
                foreach (Vector2Int square in pair.Value)
                {
                    if(!_squaresToClear.ContainsKey(square))
                        _squaresToClear.Add(square, ConsoleColor.Black);
                }
            }
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
            //очистить экран
            Console.Clear();

            //проиницализировать и нарисовать удава

            //разместить еду

            //отрисовать текущие очки

            //переключиться в игровой режим
            _mode = Mode.Game;
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
