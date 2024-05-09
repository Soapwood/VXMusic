// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 07-24-2016
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

using System;

namespace AiUnity.Common.Attributes
{
    /// <summary>
    /// Custom editor windows can use this attribute to hide properties. This is automatically
    /// done by HideInInspector, but properties are also hidden when assets viewed in inspector.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    public class HideInEditorAttribute : Attribute
    {
    }
}

#endif