
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
using AiUnity.CLog.Core.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Attributes;
using AiUnity.Common.Log;

namespace AiUnity.CLog.Core.Loggers
{
    /// <summary>
    /// Factory for creating <see cref="CLogger" /> instances that silently ignores
    /// logging requests.
    /// </summary>
    /// <author>Gilles Bayon</author>
    [Adapter("Null", typeof(NullLogger))]
    public sealed class NullFactoryAdapter : ILoggerFactoryAdapter, ILoggerSettings
    {
        private CLogger nullLogger;

        /// <summary>
        /// The <see cref="AiUnity.CLog.Core.Common.LogLevels"/> for this logger.
        /// </summary>
        [Display("Log levels", "Log levels that will be logged by this target.", false, -100)]
        [DefaultValue(0)]
        public LogLevels LogLevels { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public NullFactoryAdapter(Dictionary<string, string> properties = null)
        {
            nullLogger = new NullLogger(this);
        }

        #region ILoggerFactoryAdapter Members

        /// <summary>
        /// Get a Logger instance by type key 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public CLogger GetLogger(string name, UnityEngine.Object context, IFormatProvider formatProvider)
        {
            return nullLogger;
        }

        #endregion
    }
}
#endif
