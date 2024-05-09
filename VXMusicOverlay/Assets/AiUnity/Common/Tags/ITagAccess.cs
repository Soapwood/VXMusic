// ***********************************************************************
// Assembly   : Assembly-CSharp
// Company    : AiUnity
// Author     : AiDesigner
//
// Created    : 07-07-2017
// Modified   : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE
using System.Collections.Generic;

namespace AiUnity.Common.Tags
{
    /// <summary>
    /// Interface that provides a list of all Unity tags.
    /// </summary>
    public interface ITagAccess
    {
        #region Properties
        /// <summary> Gets the tag paths. </summary>
        IEnumerable<string> TagPaths { get; }
        #endregion
    }
}
#endif