using System.Collections.Generic;

namespace NoMoreMath.Utils
{
    public sealed class UnityObjectComparer : IEqualityComparer<UnityEngine.Object>
    {
        public static readonly UnityObjectComparer Instance = new UnityObjectComparer();

        UnityObjectComparer()
        {
        }

        public bool Equals(UnityEngine.Object x, UnityEngine.Object y)
        {
            return x == y;
        }

        public int GetHashCode(UnityEngine.Object obj)
        {
            return obj.GetHashCode();
        }
    }
}
