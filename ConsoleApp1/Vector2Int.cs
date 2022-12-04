using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApp1
{
    /// <summary>
    /// Целочисленный вектор.
    /// </summary>
    internal struct Vector2Int : IComparable
    {
        public int X;

        public int Y;

        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Vector2Int operator *(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.X * b.X, a.Y * b.Y);
        }

        public static bool operator ==(Vector2Int a, Vector2Int b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Vector2Int a, Vector2Int b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public static bool operator <(Vector2Int a, Vector2Int b)
        {
            if (a.X == b.X) return a.Y < b.Y;
            return a.X < b.X;
        }

        public static bool operator >(Vector2Int a, Vector2Int b)
        {
            if (a.X == b.X) return a.Y > b.Y;
            return a.X > b.X;
        }

        public static bool operator <=(Vector2Int a, Vector2Int b)
        {
            if (a.X == b.X) return a.Y <= b.Y;
            return a.X <= b.X;
        }

        public static bool operator >=(Vector2Int a, Vector2Int b)
        {
            if (a.X == b.X) return a.Y >= b.Y;
            return a.X >= b.X;
        }

        /// <summary>
        /// Сортировка по умолчанию, по имени.
        /// </summary>
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            if (obj.GetType() == typeof(Vector2Int))
            {
                if (X == ((Vector2Int)obj).X) return Y.CompareTo(((Vector2Int)obj).Y);
                return X.CompareTo(((Vector2Int)obj).X);
            }
            throw new ArgumentException("Ожидался объект типа Vector2Int.");
        }
    }
}
