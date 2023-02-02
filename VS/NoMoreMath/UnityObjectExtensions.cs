using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NoMoreMath
{
    public static class UnityObjectExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.TryGetComponent(out T component) ? component : gameObject.AddComponent<T>();
        }
    }
}
