// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 07-09-2016
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

using UnityEngine;

namespace AiUnity.Common.Attributes
{
    /// <summary>
    /// Creates a divider line in the inspector.
    /// </summary>
    /// <seealso cref="UnityEngine.PropertyAttribute" />
    public class DividerAttribute : UnityEngine.PropertyAttribute
    {
        #region Fields
        public readonly string col = "grey";
        public readonly float space = 0;
        public readonly float thickness = 1;
        public float widthPct = 1.0f;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DividerAttribute"/> class.
        /// </summary>
        public DividerAttribute()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DividerAttribute"/> class.
        /// </summary>
        /// <param name="col">The col.</param>
        public DividerAttribute(string col)
        {
            this.col = col;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DividerAttribute"/> class.
        /// </summary>
        /// <param name="widthPct">The width PCT.</param>
        public DividerAttribute(float widthPct)
        {
            this.widthPct = widthPct;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DividerAttribute"/> class.
        /// </summary>
        /// <param name="col">The col.</param>
        /// <param name="thickness">The thickness.</param>
        public DividerAttribute(string col, float thickness)
        {
            this.col = col;
            this.thickness = thickness;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DividerAttribute"/> class.
        /// </summary>
        /// <param name="widthPct">The width PCT.</param>
        /// <param name="thickness">The thickness.</param>
        public DividerAttribute(float widthPct, float thickness)
        {
            this.widthPct = widthPct;
            this.thickness = thickness;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DividerAttribute"/> class.
        /// </summary>
        /// <param name="col">The col.</param>
        /// <param name="thickness">The thickness.</param>
        /// <param name="widthPct">The width PCT.</param>
        public DividerAttribute(string col, float thickness, float widthPct)
        {
            this.col = col;
            this.thickness = thickness;
            this.widthPct = widthPct;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DividerAttribute"/> class.
        /// </summary>
        /// <param name="widthPct">The width PCT.</param>
        /// <param name="thickness">The thickness.</param>
        /// <param name="space">The space.</param>
        public DividerAttribute(float widthPct, float thickness, float space)
        {
            this.widthPct = widthPct;
            this.thickness = thickness;
            this.space = space;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DividerAttribute"/> class.
        /// </summary>
        /// <param name="col">The col.</param>
        /// <param name="thickness">The thickness.</param>
        /// <param name="widthPct">The width PCT.</param>
        /// <param name="space">The space.</param>
        public DividerAttribute(string col, float thickness, float widthPct, float space)
        {
            this.col = col;
            this.thickness = thickness;
            this.widthPct = widthPct;
            this.space = space;
        }
        #endregion
    }
}

#endif