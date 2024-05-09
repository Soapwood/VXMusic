
#region License

/*
 * Copyright � 2002-2009 the original author or authors.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

#if AIUNITY_CODE

using System;
using AiUnity.CLog.Core.Common;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Log;

namespace AiUnity.CLog.Core.Loggers
{
    /// <summary>
    /// Silently ignores all log messages.
    /// </summary>
    /// <author>Gilles Bayon</author>
    /// <author>Erich Eichinger</author>
    [Serializable]
    public sealed class NullLogger : CLogger
    {
        public NullLogger(ILoggerSettings loggerSettings) : base ("NullLogger", loggerSettings, null, null)
        {
        }

        protected override void WriteInternal(LogLevels level, UnityEngine.Object context, object message, Exception exception)
        {
            return;
        }

    }
}
#endif
