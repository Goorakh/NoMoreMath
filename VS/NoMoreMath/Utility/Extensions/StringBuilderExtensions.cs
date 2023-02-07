using System;
using System.Collections.Generic;
using System.Text;

namespace NoMoreMath.Utility.Extensions
{
    public static class StringBuilderExtensions
    {
        public static string GetAndReturnToPool(this StringBuilder stringBuilder)
        {
            string result = stringBuilder.ToString();
            HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
            return result;
        }
    }
}
