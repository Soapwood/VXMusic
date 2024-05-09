// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 06-20-2016
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

using System;
using AiUnity.Common.Attributes;
using System.Linq;
using System.ComponentModel;

namespace AiUnity.Common.Attributes
{
    /// <summary>
    /// Determines how a property is displayed in the Unity inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class DisplayAttribute : DisplayNameAttribute
    {
        #region Properties
        /// <summary>
        /// Indicate if item should be shown.
        /// </summary>
        public bool Advanced { get; private set; }

        /// <summary>
        /// Indicate if item should be shown.
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// Indicate if item should be shown.
        /// </summary>
        public string Tooltip { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayAttribute" /> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <param name="toolTip">The tool tip.</param>
        /// <param name="advanced">if set to <c>true</c> [advanced].</param>
        /// <param name="order">The order.</param>
        public DisplayAttribute(string name, string toolTip, bool advanced = false, int order = 0) : base(name)
        {
            Advanced = advanced;
            Tooltip = toolTip;
            Order = order;
        }
        #endregion
    }
}

#endif