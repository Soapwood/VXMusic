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
    /// Constraint that allows selection of classes that implement a specific interface
    /// when selecting a <see cref="ClassTypeReference" /> with the Unity inspector.
    /// </summary>
    /// <seealso cref="AiUnity.Common.Attributes.ClassTypeConstraintAttribute" />
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ClassImplementsAttribute : ClassTypeConstraintAttribute
    {
        #region Properties
        /// <summary>
        /// Gets the type of interface that selectable classes must implement.
        /// </summary>
        public Type InterfaceType { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassImplementsAttribute" /> class.
        /// </summary>
        /// <param name="labelName">Name of the label.</param>
        public ClassImplementsAttribute(string labelName = null)
                    : base(labelName)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassImplementsAttribute" /> class.
        /// </summary>
        /// <param name="interfaceType">Type of interface that selectable classes must implement.</param>
        /// <param name="labelName">Name of the label.</param>
        public ClassImplementsAttribute(Type interfaceType, string labelName = null)
            : base(labelName)
        {
            InterfaceType = interfaceType;
        }
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
            if (base.IsConstraintSatisfied(type)) foreach (var interfaceType in type.GetInterfaces()) if (interfaceType == InterfaceType) return true;
            return false;
        }
        #endregion
    }
}

#endif