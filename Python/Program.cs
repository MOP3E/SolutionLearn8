using SFML.Learning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using SFML.Window;

namespace Python
{
    internal class Program : Game
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
        /// Направление движения головы питона
        /// 0 - вверх, 1 - вправо, 2 - вниз, 3 - влево.
        /// </summary>
        private static int _pythonHeadDirection = 1;
        
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
        private static List<Vector2Int> _redSquares = new List<Vector2Int>();
            
        /// <summary>
        /// Скорость движения красных квадратиков.
        /// </summary>
        private static float _borderSpeed = 100;
    
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


        static void Main(string[] args)
        {
            Game.InitWindow(800, 600);

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

            _mode = Mode.StartScreen;
            StartScreenInit();

            //главный цикл игры
            while (true)
            {
                //Расчёт.
                DispatchEvents();

                //выход из игры
                if (GetKey(Keyboard.Key.Escape)) break;

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


                ClearWindow(100, 149, 237);

                //Перерисовка экрана
                //стереть квадраты
                foreach (KeyValuePair<Vector2Int, ConsoleColor> pair in _squaresToClear)
                    WriteXY(pair.Key * ConsoleMultiplier, ClearSquare);
                _squaresToClear.Clear();
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

                //Ожидание
                Delay(1);
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
            DrawBorder();

            int i = _redSquares.Count;

            //нарисовать заголовок окна и справочную информацию
            //TODO
        }

        /// <summary>
        /// Первоначальная отрисовка рамки неигрового экрана.
        /// </summary>
        private static void DrawBorder()
        {
            //очистить экран
            Console.Clear();

            //нарисовать рамку
            _redSquares.Clear();
            int c = 0;
            const int step = 12;
            //верхняя линия
            for (int i = 0; i < GameFieldSize.X; i++)
            {
                Vector2Int coord = new Vector2Int(i, 0);
                if (c % step == 0)
                {
                    //каждый 12 квадратик - красного цвета
                    if (_squaresToDraw.ContainsKey(coord))
                        _squaresToDraw[coord] = ConsoleColor.Red;
                    else
                        _squaresToDraw.Add(coord, ConsoleColor.Red);
                    //сохранить координаты красного квадратика
                    _redSquares.Add(coord);
                }
                else
                {
                    if (_squaresToDraw.ContainsKey(coord))
                        _squaresToDraw[coord] = ConsoleColor.White;
                    else
                        _squaresToDraw.Add(coord, ConsoleColor.White);
                }
                c++;
            }
            //правая линия
            for (int i = 1; i < GameFieldSize.Y; i++)
            {
                Vector2Int coord = new Vector2Int(GameFieldSize.X - 1, i);
                if (c % step == 0)
                {
                    //каждый 12 квадратик - красного цвета
                    if (_squaresToDraw.ContainsKey(coord))
                        _squaresToDraw[coord] = ConsoleColor.Red;
                    else
                        _squaresToDraw.Add(coord, ConsoleColor.Red);
                    _redSquares.Add(coord);
                }
                else
                {
                    if (_squaresToDraw.ContainsKey(coord))
                        _squaresToDraw[coord] = ConsoleColor.White;
                    else
                        _squaresToDraw.Add(coord, ConsoleColor.White);
                }
                c++;
            }
            //нижняя линия
            for (int i = GameFieldSize.X - 2; i >= 0; i--)
            {
                Vector2Int coord = new Vector2Int(i, GameFieldSize.Y - 1);
                if (c % step == 0)
                {
                    //каждый 12 квадратик - красного цвета
                    if (_squaresToDraw.ContainsKey(coord))
                        _squaresToDraw[coord] = ConsoleColor.Red;
                    else
                        _squaresToDraw.Add(coord, ConsoleColor.Red);
                    //сохранить координаты красного квадратика
                    _redSquares.Add(coord);
                }
                else
                {
                    if (_squaresToDraw.ContainsKey(coord))
                        _squaresToDraw[coord] = ConsoleColor.White;
                    else
                        _squaresToDraw.Add(coord, ConsoleColor.White);
                }
                c++;
            }
            //левая линия
            for (int i = GameFieldSize.Y - 2; i >= 1; i--)
            {
                Vector2Int coord = new Vector2Int(0, i);
                if (c % step == 0)
                {
                    //каждый 12 квадратик - красного цвета
                    if (_squaresToDraw.ContainsKey(coord))
                        _squaresToDraw[coord] = ConsoleColor.Red;
                    else
                        _squaresToDraw.Add(coord, ConsoleColor.Red);
                    //сохранить координаты красного квадратика
                    _redSquares.Add(coord);
                }
                else
                {
                    if (_squaresToDraw.ContainsKey(coord))
                        _squaresToDraw[coord] = ConsoleColor.White;
                    else
                        _squaresToDraw.Add(coord, ConsoleColor.White);
                }
                c++;
            }
            int qqq = _redSquares.Count;
        }

        /// <summary>
        /// Движение элементов рамки неигрового окна.
        /// </summary>
        private static void BorderIdle()
        {
            //TODO: оценить потраченное время, и не пора ли двигать дальше квадратики
            WriteXY(new Vector2Int(5,5), DeltaTime.ToString());
            _borderDelta += 1000 * DeltaTime;
            if(_borderDelta < _borderSpeed) return;
            _borderDelta -= _borderSpeed;

            //покрасить текущие клетки белым
            foreach (Vector2Int square in _redSquares)
            {
                if (_squaresToDraw.ContainsKey(square))
                    _squaresToDraw[square] = ConsoleColor.White;
                else
                    _squaresToDraw.Add(square, ConsoleColor.White);
            }
            //переместиться на одну позицию дальше по рамке
            for (int i = 0; i < _redSquares.Count; i++)
            {
                //красные клетки движутся по рамке по часовой стрелке
                if (_redSquares[i].X == 0)
                {
                    //движение по вертикали, снизу вверх
                    Vector2Int newCoord = new Vector2Int(_redSquares[i].X, _redSquares[i].Y - 1);
                    if (newCoord.Y < 0)
                    {
                        newCoord.Y = 0;
                        newCoord.X++;
                    }
                    _redSquares[i] = newCoord;
                    if (_squaresToDraw.ContainsKey(newCoord))
                        _squaresToDraw[newCoord] = ConsoleColor.Red;
                    else
                        _squaresToDraw.Add(newCoord, ConsoleColor.Red);
                }
                else if(_redSquares[i].X == GameFieldSize.X - 1)
                {
                    //движение по вертикали, сверху вниз
                    Vector2Int newCoord = new Vector2Int(_redSquares[i].X, _redSquares[i].Y + 1);
                    if (newCoord.Y > GameFieldSize.Y - 1)
                    {
                        newCoord.Y = GameFieldSize.Y - 1;
                        newCoord.X--;
                    }
                    _redSquares[i] = newCoord;
                    if (_squaresToDraw.ContainsKey(newCoord))
                        _squaresToDraw[newCoord] = ConsoleColor.Red;
                    else
                        _squaresToDraw.Add(newCoord, ConsoleColor.Red);
                }
                else if (_redSquares[i].Y == 0)
                {
                    //движение по горизонтали, слева направо
                    Vector2Int newCoord = new Vector2Int(_redSquares[i].X + 1, _redSquares[i].Y);
                    if (newCoord.X > GameFieldSize.X - 1)
                    {
                        newCoord.X = GameFieldSize.X - 1;
                        newCoord.Y++;
                    }
                    _redSquares[i] = newCoord;
                    if (_squaresToDraw.ContainsKey(newCoord))
                        _squaresToDraw[newCoord] = ConsoleColor.Red;
                    else
                        _squaresToDraw.Add(newCoord, ConsoleColor.Red);
                }
                else if(_redSquares[i].Y == GameFieldSize.Y - 1)
                {
                    //движение по горизонтали, справа налево
                    Vector2Int newCoord = new Vector2Int(_redSquares[i].X - 1, _redSquares[i].Y);
                    if (newCoord.X < 0)
                    {
                        newCoord.X = 0;
                        newCoord.Y--;
                    }
                    _redSquares[i] = newCoord;
                    if (_squaresToDraw.ContainsKey(newCoord))
                        _squaresToDraw[newCoord] = ConsoleColor.Red;
                    else
                        _squaresToDraw.Add(newCoord, ConsoleColor.Red);
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
