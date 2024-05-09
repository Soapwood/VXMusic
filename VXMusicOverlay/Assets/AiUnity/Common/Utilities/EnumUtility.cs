// ***********************************************************************
// Assembly   : Assembly-CSharp
// Company    : AiUnity
// Author     : AiDesigner
//
// Created    : 08-29-2017
// Modified   : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AiUnity.Common.Utilities
{
    public static class EnumUtility
    {
        #region Methods
        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="allowCombinators">if set to <c>true</c> [allow combinators].</param>
        /// <param name="allowZero">if set to <c>true</c> [allow negative].</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        public static IEnumerable<T> GetValues<T>(bool allowZero = true, bool allowCombinators = false)
        {
            return ((T[])Enum.GetValues(typeof(T))).Where(t => (allowZero || Convert.ToInt32(t) > 0) && (allowCombinators || !typeof(T).IsDefined(typeof(FlagsAttribute), false) || (Convert.ToInt32(t) & (Convert.ToInt32(t) - 1)) == 0));
        }
        #endregion
    }
}
#endif