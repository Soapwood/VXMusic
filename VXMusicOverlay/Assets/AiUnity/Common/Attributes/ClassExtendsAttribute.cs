// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 07-09-2016
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

using System;

namespace AiUnity.Common.Attributes
{
    /// <summary>
    /// Constraint that allows selection of classes that extend a specific class when
    /// selecting a <see cref="ClassTypeReference" /> with the Unity inspector.
    /// </summary>
    /// <seealso cref="AiUnity.Common.Attributes.ClassTypeConstraintAttribute" />
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ClassExtendsAttribute : ClassTypeConstraintAttribute
    {
        #region Properties
        /// <summary>
        /// Gets the type of class that selectable classes must derive from.
        /// </summary>
        public Type BaseType { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Determines whether the specified <see cref="Type" /> satisfies filter constraint.
        /// </summary>
        /// <param name="type">Type to test.</param>
        /// <returns>A <see cref="bool" /> value indicating if the type specified by <paramref name="type" />
        /// satisfies this constraint and should thus be selectable.</returns>
        public override bool IsConstraintSatisfied(Type type)
        {
            return base.IsConstraintSatisfied(type)
                && BaseType.IsAssignableFrom(type) && type != BaseType;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassExtendsAttribute" /> class.
        /// </summary>
        /// <param name="baseType">Type of class that selectable classes must derive from.</param>
        /// <param name="labelName">Name of the label.</param>
        /// <param name="tooltip">The tooltip.</param>
        public ClassExtendsAttribute(Type baseType, string labelName = null, string tooltip = null)
            : base(labelName, tooltip)
        {
            BaseType = baseType;
        }
        #endregion

    }
}

#endif