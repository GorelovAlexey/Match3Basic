using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Utils
{
    static class ArrayUtils
    {
        public static T[,] SetEach<T>(this T[,] arr, Func<int, int, T, T> setter)
        {
            for (var x = 0; x < arr.GetLength(0); x++)
            {
                for (var y = 0; y < arr.GetLength(1); y++)
                {
                    arr[x, y] = setter(x, y, arr[x, y]);
                }
            }

            return arr;
        }
    }
}
