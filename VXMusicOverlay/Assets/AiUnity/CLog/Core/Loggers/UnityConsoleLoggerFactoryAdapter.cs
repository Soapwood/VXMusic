#if AIUNITY_CODE

using System;
using AiUnity.CLog.Core.Common;
using System.Collections.Generic;
using AiUnity.Common.Attributes;
using System.ComponentModel;

namespace AiUnity.CLog.Core.Loggers
{
    [Adapter("UnityConsole", typeof(UnityConsoleLogger))]
    public class UnityConsoleLoggerFactoryAdapter : SimpleLoggerFactoryAdapter, IUnityConsoleLoggerSettings
    {
        #region Properties
        /// <summary>
        /// Enable colors in message header and message.
        /// </summary>
        [DefaultValue(true)]
        [Display("Enable Colors", "Is colors enabled for message header and content.")]
        public bool EnableColors { get; set; }
        
        /// <summary>
        /// The color of the message header.
        /// </summary>
        [DefaultValue("olive")]
        [Display("Header Color", "The color of message header.")]
        public UnityEngine.Color HeaderColor { get; set; }


        /// <summary>
        /// The color of the message content.
        /// </summary>
        [DefaultValue("black")]
        [Display("Message Color", "The color of message content.")]
        public UnityEngine.Color MessageColor { get; set; }
#endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityConsoleLoggerFactoryAdapter" /> class.
        /// </summary>
        /// <param name="properties">Configuration data (XML) used to configure logger properties.</param>
        public UnityConsoleLoggerFactoryAdapter(Dictionary<string, string> properties = null) : base(properties)
        {
        }

        /// <summary>
        /// Creates a new <see cref="UnityConsoleLogger"/> instance.
        /// </summary>
        /// <param name="name">Name associated with the logger</param>
        /// <param name="context">GameObject associated with the logger</param>
        /// <param name="formatProvider">IFormatProvider to used to display log messages</param>
        protected override CLogger CreateLogger(string name, UnityEngine.Object context, IFormatProvider formatProvider)
        {
            return new UnityConsoleLogger(name, this, context, formatProvider);
        }
    }
}

#endif
