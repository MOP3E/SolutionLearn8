using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Dictionary<Vector2Int, string> dictionary = new Dictionary<Vector2Int, string>();
            dictionary.Add(new Vector2Int(25, 1), "");
            dictionary.Add(new Vector2Int(25, 2), "");
            Vector2Int vector = new Vector2Int(25, 1);
            Console.WriteLine(dictionary.ContainsKey(vector));
            Console.ReadLine();
        }
    }
}
