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
using AiUnity.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AiUnity.CLog.Core.Loggers
{
    /// <summary>
    /// Base factory implementation for creating simple <see cref="CLogger" /> instances.
    /// </summary>
    /// <author>Gilles Bayon</author>
    /// <author>Mark Pollack</author>
    /// <author>Erich Eichinger</author>
    [Serializable]
    public abstract class SimpleLoggerFactoryAdapter : CacheLoggerFactoryAdapter, ISimpleLoggerSettings
    {
        #region Properties
        /// <summary>
        /// Add Newline separator after primary log message
        /// </summary>
        [DefaultValue(true)]
        [Display("Show newline", "Add newline after message headers.")]
        public bool HeaderNewline { get; set; }

        /// <summary>
        /// Add call method to message
        /// </summary>
        [DefaultValue(true)]
        [Display("Show callsite", "Display callsite (namespace/method) in log message.")]
        public bool ShowCallsite { get; set; }

        /// <summary>
        /// Add level to message
        /// </summary>
        [DefaultValue(true)]
        [Display("Show log level", "Display log level in log message.")]
        public bool ShowLogLevel { get; set; }

        /// <summary>
        /// Add call name to message
        /// </summary>
        [DefaultValue(false)]
        [Display("Show logger", "Display logger name in log message.")]
        public bool ShowLoggerName { get; set; }

        /// <summary>
        /// Add timestamp to message
        /// </summary>
        [DefaultValue(false)]
        [Display("Show timestamp", "Display timestamp in log message.")]
        public bool ShowTimeStamp { get; set; }

        /// <summary>
        /// Determines the stack index relative to logging statement
        /// </summary>
        [DefaultValue(0)]
        [Display("Stack offset", "Determines the stack index relative to logging statement.  Used to calculate the callsite of each log statement.")]
        public int StackIndex { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractSimpleLoggerFactoryAdapter"/> class.
        /// </summary>
        /// <param name="properties">Configuration data (XML) Used to configure logger properties.</param>
        protected SimpleLoggerFactoryAdapter(Dictionary<string, string> properties = null) : base(properties)
        {
        }
        #endregion
    }
}
#endif
