using System;
using System.Runtime.InteropServices;

namespace Python
{
    /// <summary>
    /// Класс для замены шрифта в консоли.
    /// </summary>
    internal static class ConsoleFont
    {
        /// <summary>
        /// Информация о шрифте.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal unsafe struct CONSOLE_FONT_INFOEX
        {
            internal uint cbSize;
            internal uint nFont;
            internal COORD dwFontSize;
            internal int FontFamily;
            internal int FontWeight;
            internal fixed char FaceName[LF_FACESIZE];
        }

        /// <summary>
        /// Координаты.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct COORD
        {
            internal short X;
            internal short Y;

            internal COORD(short x, short y)
            {
                X = x;
                Y = y;
            }
        }

        //константы из мира Windows
        private const int STD_OUTPUT_HANDLE = -11;
        private const int TMPF_TRUETYPE = 4;
        private const int LF_FACESIZE = 32;
        private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        /// <summary>
        /// Установка шрифта в консоли.
        /// </summary>
        /// <param name="consoleOutput">Указатель на консоль.</param>
        /// <param name="maximumWindow">Устанавливать ли после смены шрифта максимальный размер окна консоли.</param>
        /// <param name="consoleCurrentFontEx">Указатель на описание устанавливаемого шрифта.</param>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetCurrentConsoleFontEx(
            IntPtr consoleOutput,
            bool maximumWindow,
            ref CONSOLE_FONT_INFOEX consoleCurrentFontEx);
        
        /// <summary>
        /// Получить информацию о текущем шрифте консоли.
        /// </summary>
        /// <param name="consoleOutput">Указатель на консоль.</param>
        /// <param name="maximumWindow">Получать информацию о шрифте для максимального размера окна консоли.</param>
        /// <param name="consoleCurrentFontEx">Указатель на описание шрифта, в которое будут записана информация о текущем шрифте консоли.</param>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool GetCurrentConsoleFontEx(
            IntPtr consoleOutput,
            bool maximumWindow,
            ref CONSOLE_FONT_INFOEX consoleCurrentFontEx);

        /// <summary>
        /// Получить указатель на стандартный объект консоли (поток ввода, поток вывода или поток ошибок).
        /// </summary>
        /// <param name="dwType">Идентификатор объекта.</param>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int dwType);

        /// <summary>
        /// Вывод на экран информации о текущем шрифте консоли.
        /// </summary>
        public static unsafe bool PrintFontInfo()
        {
            //получить ссылку на консоль
            IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
            if (hnd == INVALID_HANDLE_VALUE) return false;
            
            //получить данные етекущего шрифта
            CONSOLE_FONT_INFOEX info = new CONSOLE_FONT_INFOEX();
            info.cbSize = (uint)Marshal.SizeOf(info);
            if (!GetCurrentConsoleFontEx(hnd, false, ref info)) return false;

            Console.WriteLine("nFont: " + info.nFont);
            Console.WriteLine("dwFontSize: " + info.dwFontSize.X + ", " + info.dwFontSize.Y);
            Console.WriteLine("FontFamily: " + info.FontFamily);
            Console.WriteLine("FontWeight: " + info.FontWeight);
            IntPtr ptr = new IntPtr(info.FaceName);
            char[] faceNameChars = new char[LF_FACESIZE];
            Marshal.Copy(ptr, faceNameChars, 0, LF_FACESIZE);
            string faceName = new string(faceNameChars);
            Console.WriteLine("FaceName: " + faceName);

            return true;
        }

        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;
        public const int SC_MINIMIZE = 0xF020;
        public const int SC_MAXIMIZE = 0xF030;
        public const int SC_SIZE = 0xF000;//resize

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        /// <summary>
        /// Установка шрифта в консоли.
        /// </summary>
        /// <param name="fontName">Имя шрифта.</param>
        /// <param name="x">Размер символа по X.</param>
        /// <param name="y">Размер символа по Y.</param>
        /// <param name="fontWeight">Жирность шрифта.</param>
        public static void SetFont(string fontName = "Lucida Console", short x = -1, short y = -1, int fontWeight = 400)
        {
            unsafe
            {
                IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
                if (hnd != INVALID_HANDLE_VALUE)
                {
                    CONSOLE_FONT_INFOEX info = new CONSOLE_FONT_INFOEX();
                    info.cbSize = (uint)Marshal.SizeOf(info);

                    if (GetCurrentConsoleFontEx(hnd, false, ref info))
                    {
                        //установить заданный шрифт
                        CONSOLE_FONT_INFOEX newInfo = new CONSOLE_FONT_INFOEX();
                        newInfo.cbSize = (uint)Marshal.SizeOf(newInfo);
                        newInfo.FontFamily = TMPF_TRUETYPE;
                        IntPtr ptr = new IntPtr(newInfo.FaceName);
                        Marshal.Copy(fontName.ToCharArray(), 0, ptr, fontName.Length);

                        //часть настроек шрифта взять из текущего шрифта консоли
                        newInfo.dwFontSize = new COORD(x == -1 ? info.dwFontSize.X : x, y == -1 ? info.dwFontSize.Y : y);
                        newInfo.FontWeight = fontWeight;
                        SetCurrentConsoleFontEx(hnd, false, ref newInfo);
                    }
                }

                IntPtr handle = GetConsoleWindow();
                IntPtr sysMenu = GetSystemMenu(handle, false);

                if (handle != IntPtr.Zero)
                {
                    //DeleteMenu(sysMenu, SC_CLOSE, MF_BYCOMMAND);
                    //DeleteMenu(sysMenu, SC_MINIMIZE, MF_BYCOMMAND);
                    //DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND);
                    DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND);//resize
                }
            }
        }
    }
}