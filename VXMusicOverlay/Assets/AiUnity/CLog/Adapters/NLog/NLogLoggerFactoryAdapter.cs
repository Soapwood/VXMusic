// Resource - https://github.com/net-commons/common-logging/blob/master/src/Common.Logging.NLog10/Logging/NLog/NLogLoggerFactoryAdapter.cs

#if AIUNITY_CODE

using System;
using AiUnity.CLog.Core.Common;
using AiUnity.CLog.Core;
using System.Collections.Generic;
using AiUnity.Common.Log;
using AiUnity.CLog.Core.Loggers;

namespace AiUnity.CLog.Adapters.NLog
{
    [Adapter("NLogLogger", typeof(NLogLogger))]
    public class NLogLoggerFactoryAdapter : ExternalLoggerFactoryAdapter
    {
        #region Constructors
        public NLogLoggerFactoryAdapter(Dictionary<string, string> properties) : base(properties, "NLogAdapter", "NLog")
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get a Logger instance by type name 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected override CLogger CreateLogger(string name, UnityEngine.Object context, IFormatProvider formatProvider)
        {

            if (nlogManager != null) {
                ILogger nlogLogger = nlogManager.GetLogger(name, context);
                //IVariablesContext nlogVariablesContext = nlogVariablesContextType != null ? (IVariablesContext)Activator.CreateInstance(nlogVariablesContextType) : null;
                //return new NLogLogger(global::AiUnity.NLog.Core.LogManager.Instance.GetLogger(name, context));
                return new NLogLogger(name, this, nlogLogger, context);
            }
            return new NullLogger(this);
        }
        #endregion
    }
}

#endif
