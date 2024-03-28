using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class DebugExtender
    {
        public static string CreateDebugString(this string text, Color32 color)
        {
            var hexColor = $"{color.r:X2}{color.g:X2}{color.b:X2}";
            return $"<color=#{hexColor}>{text}</color>";
        }
    }
}
