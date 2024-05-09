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

using AiUnity.CLog.Core.Common;
using AiUnity.Common.Attributes;
using AiUnity.Common.Extensions;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Log;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace AiUnity.CLog.Core.Loggers
{
    /// <summary>
    /// An implementation of <see cref="ILoggerFactoryAdapter"/> that caches loggers handed out by this factory.
    /// </summary>
    /// <remarks>
    /// Implementors just need to override <see cref="CreateLogger"/>.
    /// </remarks>
    /// <author>Erich Eichinger</author>
    public abstract class CacheLoggerFactoryAdapter : ILoggerFactoryAdapter, ILoggerSettings
    {
        #region Fields
        private readonly Dictionary<string, CLogger> _cachedLoggers;
        #endregion

        #region Properties
        /// <summary>
        /// The <see cref="AiUnity.CLog.Core.Common.LogLevels"/> for this logger.
        /// </summary>
        [DefaultValue(LogLevels.Everything)]
        [Display("Log levels", "Log levels that will be logged by this target.", false, -100)]
        public LogLevels LogLevels { get; set; }

        // Internal logger singleton
        private static IInternalLogger Logger { get { return CLogInternalLogger.Instance; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance, the logger cache being case-sensitive.
        /// <param name="properties">Configuration data (XML) Used to configure logger properties.</param>
        /// </summary>
        protected CacheLoggerFactoryAdapter(Dictionary<string, string> properties) : this(true)
        {
            if (properties != null)
            {

                IEnumerable<PropertyInfo> propertyInfos = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty).Where(p => p.CanWrite).ToList();

                foreach (PropertyInfo propertyInfo in propertyInfos)
                {

                    try
                    {
                        string propertyValue;
                        properties.TryGetValue(propertyInfo.Name.LowercaseLetter(), out propertyValue);

                        // If property not present in configuration file set to value specified by DefaultValue Attribute
                        if (propertyValue == null && Attribute.IsDefined(propertyInfo, typeof(DefaultValueAttribute)))
                        {
                            propertyValue = propertyInfo.GetAttributes<DefaultValueAttribute>().Select(v => v.Value.ToString()).FirstOrDefault();
                        }

                        if (propertyValue != null)
                        {
                            if (propertyInfo.PropertyType.IsEnum)
                            {
                                object setValue = Enum.Parse(propertyInfo.PropertyType, propertyValue);
                                propertyInfo.SetValue(this, setValue, null);
                            }
                            else if (propertyInfo.PropertyType == typeof(UnityEngine.Color))
                            {
                                UnityEngine.Color color;
                                UnityEngine.ColorUtility.TryParseHtmlString(propertyValue, out color);
                                propertyInfo.SetValue(this, color, null);
                            }
                            else if (propertyInfo.PropertyType.IsValueType || propertyInfo.PropertyType.Equals(typeof(string)))
                            {
                                object setValue = Convert.ChangeType(propertyValue, propertyInfo.PropertyType);
                                propertyInfo.SetValue(this, setValue, null);
                            }
                            else
                            {
                                Logger.Warn("Unable to set adapter property = {0} due to unsupported type = {1}", propertyInfo.Name, propertyInfo.PropertyType);
                            }
                        }
                    }
                    catch
                    {
                        Logger.Warn("Unable to find adapter property = {0} on adapter = {1}", propertyInfo.Name, GetType());
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new instance, the logger cache being <paramref key="caseSensitiveLoggerCache"/>.
        /// </summary>
        /// <param name="caseSensitiveLoggerCache"></param>
        protected CacheLoggerFactoryAdapter(bool caseSensitiveLoggerCache)
        {
            this._cachedLoggers = (caseSensitiveLoggerCache)
                                 ? new Dictionary<string, CLogger>()
                                 : new Dictionary<string, CLogger>(StringComparer.OrdinalIgnoreCase);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get a Logger instance by key.
        /// </summary>
        /// <param name="name">Usually a <see cref="Type" />'s Name or FullName property.</param>
        /// <returns>
        /// An Logger instance either obtained from the internal cache or created by a call to <see cref="CreateLogger"/>.
        /// </returns>
        public CLogger GetLogger(string name, UnityEngine.Object context, IFormatProvider formatProvider)
        {
            //return GetLoggerInternal(key);
            return GetLoggerInternal(name, context, formatProvider);
        }

        /// <summary>
        /// Purges all loggers from cache
        /// </summary>
        protected void ClearLoggerCache()
        {
            lock (this._cachedLoggers)
            {
                this._cachedLoggers.Clear();
            }
        }

        /// <summary>
        /// Create the specified named logger instance
        /// </summary>
        /// <remarks>
        /// Derived factories need to implement this method to create the
        /// actual logger instance.
        /// </remarks>
        protected abstract CLogger CreateLogger(string name, UnityEngine.Object context, IFormatProvider formatProvider);

        /// <summary>
        /// Get or create a Logger instance by key.
        /// </summary>
        /// <param name="name">Usually a <see cref="Type" />'s Name or FullName property.</param>
        /// <param name="context">Usually the reference <see cref="Object" /> of the current class.</param>
        /// <returns>
        /// An Logger instance either obtained from the internal cache or created by a call to <see cref="CreateLogger"/>.
        /// </returns>
        private CLogger GetLoggerInternal(string name, UnityEngine.Object context, IFormatProvider formatProvider)
        {
            CLogger log;
            if (!this._cachedLoggers.TryGetValue(name, out log))
            {
                lock (this._cachedLoggers)
                {
                    if (!this._cachedLoggers.TryGetValue(name, out log))
                    {
                        log = CreateLogger(name, context, formatProvider);
                        if (log == null)
                        {
                            throw new ArgumentException(string.Format("{0} returned null on creating logger instance for key {1}", GetType().FullName, name));
                        }
                        this._cachedLoggers.Add(name, log);
                    }
                }
            }
            return log;
        }
        #endregion
    }
}
#endif
