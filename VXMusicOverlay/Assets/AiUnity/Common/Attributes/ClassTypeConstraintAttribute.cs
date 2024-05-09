// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 07-09-2016
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

using System;
using AiUnity.Common.Serialization.ClassTypeReference;
using UnityEngine;

namespace AiUnity.Common.Attributes
{
    /// <summary>
    /// Base class for class selection constraints that can be applied when selecting
    /// a <see cref="ClassTypeReference" /> with the Unity inspector.
    /// </summary>
    /// <seealso cref="UnityEngine.PropertyAttribute" />
    public abstract class ClassTypeConstraintAttribute : UnityEngine.PropertyAttribute
    {
        #region Fields
        public GUIContent LabelContent;
        private bool _allowAbstract = false;
        private ClassGrouping _grouping = ClassGrouping.ByNamespaceFlat;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets whether abstract classes can be selected from drop-down.
        /// Defaults to a value of <c>false</c> unless explicitly specified.
        /// </summary>
        public bool AllowAbstract
        {
            get { return _allowAbstract; }
            set { _allowAbstract = value; }
        }
        /// <summary>
        /// Gets or sets grouping of selectable classes. Defaults to <see cref="ClassGrouping.ByNamespaceFlat" />
        /// unless explicitly specified.
        /// </summary>
        public ClassGrouping Grouping
        {
            get { return _grouping; }
            set { _grouping = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassTypeConstraintAttribute"/> class.
        /// </summary>
        /// <param name="labelContent">Content of the label.</param>
        public ClassTypeConstraintAttribute(GUIContent labelContent)
        {
            LabelContent = labelContent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassTypeConstraintAttribute"/> class.
        /// </summary>
        /// <param name="labelName">Name of the label.</param>
        /// <param name="tooltip">The tooltip.</param>
        public ClassTypeConstraintAttribute(string labelName, string tooltip = "")
        {
            LabelContent = new GUIContent(labelName, tooltip);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Determines whether the specified <see cref="Type" /> satisfies filter constraint.
        /// </summary>
        /// <param name="type">Type to test.</param>
        /// <returns>A <see cref="bool" /> value indicating if the type specified by <paramref name="type" />
        /// satisfies this constraint and should thus be selectable.</returns>
        public virtual bool IsConstraintSatisfied(Type type)
        {
            return AllowAbstract || !type.IsAbstract;
        }
        #endregion
    }
}

#endif