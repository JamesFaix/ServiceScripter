using System;

namespace ServiceScripter.Extensions
{
    public static class StringExtensions
    {
        public static TEnum ParseEnum<TEnum>(this string @this)
            where TEnum : struct, IConvertible
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException(nameof(TEnum), "Must be enum type.");

            return (TEnum)Enum.Parse(typeof(TEnum), @this);
        }
    }
}
