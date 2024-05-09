// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 11-30-2016
// Modified         : 08-29-2017
// ***********************************************************************
#if AIUNITY_CODE

using AiUnity.Common.InternalLog;
using AiUnity.Common.Extensions;
using AiUnity.Common.Log;

namespace AiUnity.CLog.Core.Common
{
    /// <summary>
    /// CLog internal logger.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public class CLogInternalLogger : InternalLogger<CLogInternalLogger>
    {
        static CLogInternalLogger()
        {
            Instance.Assert(true, "This log statement is executed prior to unity editor serialization due to InitializeOnLoad attribute.  The allows CLog logger to work in all phases of Unity Editor compile (ie. serialization).");
            CommonInternalLogger.Instance.Assert(true, "This log statement is executed prior to unity editor serialization due to InitializeOnLoad attribute.  The allows Common logger to work in all phases of Unity Editor compile (ie. serialization).");
        }
    }
}

#endif