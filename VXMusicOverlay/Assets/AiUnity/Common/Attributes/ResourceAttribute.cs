// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 06-20-2016
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AiUnity.Common.Attributes;
using System.ComponentModel;

namespace AiUnity.Common.Attributes
{
    /// <summary>
    /// Determines how a property is displayed in the Unity inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class ResourceAttribute : DescriptionAttribute
    {
        /// <summary>
        /// Help link for Resource
        /// </summary>
        public Uri HelpUri { get; private set; }

        public ResourceAttribute(string description, string helpLink=null) : base(description)
        {
            HelpUri = helpLink != null ? new Uri(helpLink) : null;
        }
    }
}

#endif