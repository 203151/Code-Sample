using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PolyhedraAngles
{
    public static class ExtensionMethods
    {
        private static System.Random random = new System.Random();

        public static T GetRandomElement<T>(this IEnumerable<T> list)
        {
            if (list.Count() == 0)
                return default(T);

            return list.ElementAt(random.Next(list.Count()));
        }

        public static string Repeat(this string text, int n)
        {
            return string.Concat(System.Linq.Enumerable.Repeat(text, n));
        }

        public static Transform AddChild(this Transform transform, string childName)
        {
            Transform child = new GameObject(childName).transform;
            child.SetParent(transform);
            child.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            child.localScale = Vector3.one;
            return child;
        }
    }
}
