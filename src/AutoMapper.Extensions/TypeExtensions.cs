using System;

namespace AutoMapper.Extensions
{
    internal static class TypeExtensions
    {
        public static bool IsSystemType(this Type type) 
        {
            return type.Namespace == "System";
        }
    }
}