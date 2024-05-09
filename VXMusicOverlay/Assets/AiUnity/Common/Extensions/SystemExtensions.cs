// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 06-20-2016
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

using System;
using System.Collections.Generic;
using System.Linq;

namespace AiUnity.Common.Extensions
{
    /// <summary>
    /// System Extensions.
    /// </summary>
    public static class SystemExtensions
    {
        #region Methods
        /// <summary>
        /// Get full type name with full namespace names
        /// </summary>
        /// <param name="baseType">The type to get the C# name for (may be a generic type or a nullable type).</param>
        /// <param name="fullName">if set to <c>true</c> [full name].</param>
        /// <returns>Full type name, fully qualified namespaces</returns>
        // http://stackoverflow.com/questions/2579734/get-the-type-name
        // http://stackoverflow.com/questions/6402864/c-pretty-type-name-function
        public static string GetCSharpName(this Type type, bool fullName = false)
        {
            if (type == null || type.Equals(typeof(void)))
            {
                return "void";
            }

            Type nullableType = Nullable.GetUnderlyingType(type);
            Type baseType = nullableType != null ? nullableType : type;
            string nullableSuffix = nullableType != null ? "?" : string.Empty;

            if (baseType.IsGenericType)
            {
                return string.Format("{0}<{1}>{2}",
                    baseType.Name.Substring(0, baseType.Name.IndexOf('`')),
                    string.Join(", ", baseType.GetGenericArguments().Select(ga => ga.GetCSharpName()).ToArray()),
                    nullableSuffix);
            }

            switch (Type.GetTypeCode(baseType))
            {
                case TypeCode.Boolean:
                    return "bool";
                case TypeCode.Byte:
                    return "byte";
                case TypeCode.SByte:
                    return "sbyte";
                case TypeCode.Char:
                    return "char";
                case TypeCode.Decimal:
                    return "decimal" + nullableSuffix;
                case TypeCode.Double:
                    return "double" + nullableSuffix;
                case TypeCode.Single:
                    return "float" + nullableSuffix;
                case TypeCode.Int32:
                    return "int" + nullableSuffix;
                case TypeCode.UInt32:
                    return "uint" + nullableSuffix;
                case TypeCode.Int64:
                    return "long" + nullableSuffix;
                case TypeCode.UInt64:
                    return "ulong" + nullableSuffix;
                case TypeCode.Int16:
                    return "short" + nullableSuffix;
                case TypeCode.UInt16:
                    return "ushort" + nullableSuffix;
                case TypeCode.String:
                    return "string";
                case TypeCode.Object:
                    return (fullName ? baseType.FullName : baseType.Name) + nullableSuffix;
                default:
                    return null;
            }

        }

        /// <summary>
        /// Gets the inheritance depth of a type.
        /// </summary>
        /// <param name="type">The type to analyze</param>
        /// <returns>Inheritance depth</returns>
        public static int GetInheritanceDepth(this Type type)
        {
            int count = 0;

            for (var current = type; current != null; current = current.BaseType)
                count++;
            return count;
        }

        #endregion
    }
}
#endif