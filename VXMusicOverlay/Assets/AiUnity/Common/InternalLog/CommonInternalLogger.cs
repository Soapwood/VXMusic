// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 12-07-2016
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

using AiUnity.Common.Extensions;
using AiUnity.Common.Log;

namespace AiUnity.Common.InternalLog
{
    /// <summary>
    /// AiUnity Common internal logger.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public class CommonInternalLogger : InternalLogger<CommonInternalLogger>
    {
        static CommonInternalLogger()
        {
            Instance.Assert(true, "This log statement is executed prior to unity editor serialization due to InitializeOnLoad attribute.  The allows Common logger to work in all phases of Unity Editor compile (ie. serialization).");
        }
    }
}

#endif