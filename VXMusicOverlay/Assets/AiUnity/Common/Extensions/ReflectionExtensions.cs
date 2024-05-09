// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 07-09-2016
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AiUnity.Common.Extensions
{
    /// <summary>
    /// Reflection Extensions.
    /// </summary>
    public static class ReflectionExtensions
    {
        #region Methods
        /// <summary>
        /// Gets first attribute of type T on propertyInfo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyInfo">Used to find property attributes</param>
        /// <returns>T.</returns>
        public static T GetAttribute<T>(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetAttributes<T>().FirstOrDefault();
        }

        /// <summary>
        /// Gets first attribute of type T on fieldInfo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldInfo">Used to find property attributes</param>
        /// <returns>T.</returns>
        public static T GetAttribute<T>(this FieldInfo fieldInfo)
        {
            return fieldInfo.GetAttributes<T>().FirstOrDefault();
        }

        /// <summary>
        /// Gets attributes of type T on propertyInfo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyInfo">Used to find property attributes</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        public static IEnumerable<T> GetAttributes<T>(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(typeof(T), true).Cast<T>();
        }

        /// <summary>
        /// Gets attributes of type T on fieldInfo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyInfo">Used to find property attributes</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        public static IEnumerable<T> GetAttributes<T>(this FieldInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(typeof(T), true).Cast<T>();
        }

        /// <summary>
        /// Determines whether the specified method has override keyword.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <returns><c>true</c> if the specified method information is overriding; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static bool IsOverriding(this MethodInfo methodInfo)
        {
            if (methodInfo == null) throw new ArgumentNullException();
            return methodInfo.DeclaringType != methodInfo.GetBaseDefinition().DeclaringType;
        }
        #endregion
    }
}

#endif