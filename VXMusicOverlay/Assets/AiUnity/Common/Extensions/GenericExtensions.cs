// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 01-29-2017
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

using System;
using System.Collections.Generic;
using System.Linq;

namespace AiUnity.Common.Extensions
{
    /// <summary>
    /// Generic Extensions.
    /// </summary>
    public static class GenericExtensions
    {
        #region Methods
        /// <summary>
        /// Adds the unique.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="item">The item.</param>
        public static bool AddUnique<T>(this List<T> list, T item)
        {
            if (!list.Exists(p => p.Equals(item)))
            {
                list.Add(item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the or add.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>V.</returns>
        public static V GetOrAdd<K, V>(this Dictionary<K, V> dictionary, K key, V defaultValue = default(V))
        {
            V value;

            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }
            dictionary.Add(key, defaultValue);
            return defaultValue;
        }

        /// <summary>
        /// Indicates whether the specified array is null or has a length of zero.
        /// </summary>
        /// <param name="array">The array to test.</param>
        /// <returns>true if the array parameter is null or has a length of zero; otherwise, false.</returns>
        public static bool IsNullOrEmpty(this Array array)
        {
            return (array == null || array.Length == 0);
        }
        #endregion
    }
}
#endif