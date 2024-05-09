// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 07-24-2016
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

using UnityEngine;

namespace AiUnity.Common.Attributes
{
    /// <summary>
    /// Determines the order of properties in the inspector.
    /// </summary>
    /// <seealso cref="UnityEngine.PropertyAttribute" />
    public class OrderAttribute : PropertyAttribute
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderAttribute"/> class.
        /// </summary>
        /// <param name="order">The order.</param>
        public OrderAttribute(int order)
        {
            base.order = order;
        }
        #endregion
    }
}

#endif
