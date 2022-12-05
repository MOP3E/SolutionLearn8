using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Python
{
    /// <summary>
    /// Текущее состояние игры.
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// Начальный экран игры.
        /// </summary>
        StartScreen,
        /// <summary>
        /// Игра.
        /// </summary>
        Game,
        /// <summary>
        /// Экран завершения игры.
        /// </summary>
        GamoverScreen
    }
}
