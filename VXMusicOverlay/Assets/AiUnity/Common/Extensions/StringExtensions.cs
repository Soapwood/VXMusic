// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 06-20-2016
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

//using System;

namespace AiUnity.Common.Extensions
{
    /// <summary>
    /// String Extensions.
    /// </summary>
    public static class StringExtensions
    {
        #region Methods
        public static string After(this string value, char a, bool last = true)
        {
            int index = last ? value.LastIndexOf(a) : value.IndexOf(a);
            return value.Substring(index + 1);
        }

    
        /// <summary>
        /// Get string value after [last] a.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="a">a.</param>
        /// <returns>System.String.</returns>
        public static string After(this string value, string a, bool last = true)
        {
            int index = last ? value.LastIndexOf(a) : value.IndexOf(a);
            return value.Substring(index + 1);
        }

        /// <summary>
        /// Get string value after [first] a.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="a">a.</param>
        /// <returns>System.String.</returns>
        public static string Before(this string value, char a, bool last = true)
        {
            int posA = last ? value.LastIndexOf(a) : value.IndexOf(a);
            //return posA == -1 ? string.Empty : value.Substring(0, posA);
            return posA == -1 ? string.Empty : value.Remove(posA);
        }

        /// <summary>
        /// Get string value after [first] a.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="a">a.</param>
        /// <returns>System.String.</returns>
        public static string Before(this string value, string a, bool last = true)
        {
            int posA = last ? value.LastIndexOf(a) : value.IndexOf(a);
            return posA == -1 ? string.Empty : value.Remove(posA);
        }

        public static string TrimEnd(this string source, string value)
        {
            if (!source.EndsWith(value))
                return source;

            return source.Remove(source.LastIndexOf(value));
        }

        /// <summary>
        /// Determines whether [contains] [the specified to check].
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="toCheck">To check.</param>
        /// <param name="comp">The comp.</param>
        /// <returns><c>true</c> if [contains] [the specified to check]; otherwise, <c>false</c>.</returns>
        //public static bool Contains(this string source, string toCheck, StringComparison comp)
        //{
            //return source.IndexOf(toCheck, comp) >= 0;
        //}
        //xxx

        /// <summary>
        /// Lowercases the letter.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="index">The index.</param>
        /// <returns>System.String.</returns>
        public static string LowercaseLetter(this string s, int index = 0)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            var a = s.ToCharArray();
            a[index] = char.ToLower(a[index]);
            return new string(a);
        }

        /// <summary>
        /// Uppercases the letter.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="index">The index.</param>
        /// <returns>System.String.</returns>
        public static string UppercaseLetter(this string s, int index = 0)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            var a = s.ToCharArray();
            a[index] = char.ToUpper(a[index]);
            return new string(a);
        }
        #endregion
    }
}

#endif