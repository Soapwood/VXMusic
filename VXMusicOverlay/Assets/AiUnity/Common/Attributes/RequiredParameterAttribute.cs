// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 07-23-2016
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

namespace AiUnity.Common.Attributes
{
    using System;
using AiUnity.Common.Attributes;

    /// <summary>
    /// Attribute used to mark the required parameters for targets,
    /// layout targets and filters.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RequiredParameterAttribute : Attribute
    {
    }
}
#endif
