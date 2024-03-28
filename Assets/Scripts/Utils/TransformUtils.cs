using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    static class TransformUtils
    {
        public static void ForEachChild(this Transform t, Action<Transform> action)
        {
            foreach (var child in t.GetAllChildren())
                action.Invoke(child);
        }

        public static Transform[] GetAllChildren(this Transform t)
        {
            var children = new Transform[t.childCount];
            for (var i = 0; i < children.Length; i++)
                children[i] = t.GetChild(i);
            return children;
        }
    }
}
