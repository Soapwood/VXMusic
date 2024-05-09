
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
//using System.Collections;
//using System.Configuration;
//using System.Runtime.CompilerServices;
using System.Xml;
//using AiUnity.CLog.Core.Configuration;
using System.Linq;
using System.Collections.Generic;
//using AiUnity.CLog.Core.Factory;
using UnityEngine;
using AiUnity.Common.Extensions;
using System.Reflection;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Log;
using AiUnity.CLog.Core.Loggers;
using AiUnity.Common.Types;

namespace AiUnity.CLog.Core.Configuration
{
    /// <summary>
    /// Used in an application's configuration file (App.Config or Web.Config) to configure the logging subsystem.
    /// </summary>
    public class ConfigurationSectionHandler : IConfigurationSectionHandler
    {

        #region Fields
        private static readonly string LOGFACTORYADAPTER_ELEMENT = "factoryAdapter";
        private static readonly string LOGFACTORYADAPTER_ELEMENT_TYPE_ATTRIB = "type";
        private static readonly string ARGUMENT_ELEMENT = "arg";
        private static readonly string ARGUMENT_ELEMENT_KEY_ATTRIB = "key";
        private static readonly string ARGUMENT_ELEMENT_VALUE_ATTRIB = "value";
        private Dictionary<AdapterAttribute, Type> targetTypeByAttribute;
        //private RuntimePlatforms runtimePlatforms;
        #endregion

        // Internal logger singleton
        private static IInternalLogger Logger { get { return CLogInternalLogger.Instance; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public ConfigurationSectionHandler()
        {
            ReflectTargets();
            //runtimePlatforms = Application.platform.ToString().ToEnum<RuntimePlatforms>();
        }

        private void ReflectTargets()
        {
            targetTypeByAttribute = new Dictionary<AdapterAttribute, Type>();

            List<string> searchAssemblyNames = new List<string>() { "Assembly-CSharp", Assembly.GetExecutingAssembly().GetName().Name };
            IEnumerable<Assembly> searchAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => searchAssemblyNames.Any(t => a.FullName.StartsWith(t)));

            IEnumerable<Type> targetTypes = searchAssemblies.SelectMany(a => a.GetTypes()).Where(t => typeof(ILoggerFactoryAdapter).IsAssignableFrom(t));
            //IEnumerable<Type> targetTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => t.IsSubclassOf(typeof(Logger)));
            foreach (Type targetType in targetTypes) {
                AdapterAttribute targetAttribute = targetType.GetCustomAttributes(typeof(AdapterAttribute), true).FirstOrDefault() as AdapterAttribute;
                if (targetAttribute != null) {
                    targetTypeByAttribute[targetAttribute] = targetType;
                }
            }
        }

        /// <summary>
        /// Retrieves the <see cref="Type" /> of the logger the use by looking at the logFactoryAdapter element
        /// of the logging configuration element.
        /// </summary>
        /// <param name="section"></param>
        /// <returns>
        /// A <see cref="LogSetting" /> object containing the specified type that implements 
        /// <see cref="ILoggerFactoryAdapter" /> along with zero or more properties that will be 
        /// passed to the logger factory adapter's constructor as an <see cref="IDictionary" />.
        /// </returns>
        private LogSetting ReadConfiguration(XmlNode section)
        {
            XmlAttribute internalLevelsAttribute = section.Attributes["internalLevels"];
            if (internalLevelsAttribute != null) {
                try {
                    Logger.InternalLogLevel = internalLevelsAttribute.Value.ToEnum<LogLevels>();
                }
                catch {
                    Logger.Warn("Unable to processing internalLevels values = {0}", internalLevelsAttribute.Value);
                }
            }

            bool result = false;
            XmlAttribute assertExceptionAttribute = section.Attributes["assertException"];
            CLogManager.Instance.AssertException = assertExceptionAttribute != null && bool.TryParse(assertExceptionAttribute.Value, out result) && result;

            XmlNode xmlTargetsNode = section.SelectSingleNode("targets");
            //XmlNodeList xmlTargetNodes = section.SelectNodes("target");
            XmlNodeList xmlTargetNodes = xmlTargetsNode.SelectNodes("target");
            XmlNode adapterNode = section.SelectSingleNode(LOGFACTORYADAPTER_ELEMENT);

            foreach (XmlNode xmlTargetNode in xmlTargetNodes) {

                XmlAttribute platformAttribute = xmlTargetNode != null ? xmlTargetNode.Attributes["platforms"] : null;

                //string platformValue = platformAttribute != null ? platformAttribute.Value : RuntimePlatforms.Everything.ToString();
                string platformValue = platformAttribute != null ? platformAttribute.Value : "Everything";
                //if (!string.IsNullOrEmpty(platformValue) && platformValue.ToEnum<RuntimePlatforms>().Has(this.runtimePlatforms)) {
                if (!string.IsNullOrEmpty(platformValue))
                {
                    PlatformEnumFlagWrapper<RuntimePlatform> runtimePlatform = platformValue;
                    if (runtimePlatform.Has(Application.platform))
                    {
                        adapterNode = xmlTargetNode;
                        break;
                    }
                }
            }

            Type factoryType = typeof(NullFactoryAdapter);
            Dictionary<string, string> properties = new Dictionary<string, string>();

            if (adapterNode == null) {
                Logger.Warn(string.Format("No Target Adapter exist for active platform {0}", Application.platform));
            }
            else {
                // Unity
                //XmlNode logFactoryElement = section.SelectSingleNode(LOGFACTORYADAPTER_ELEMENT);
                //XmlNode logFactoryElement = section.SelectSingleNode("target") ?? section.SelectSingleNode(LOGFACTORYADAPTER_ELEMENT);

                string factoryTypeString = string.Empty;
                if (adapterNode.Attributes[LOGFACTORYADAPTER_ELEMENT_TYPE_ATTRIB] != null)
                    factoryTypeString = adapterNode.Attributes[LOGFACTORYADAPTER_ELEMENT_TYPE_ATTRIB].Value;

                if (factoryTypeString == string.Empty) {
                    throw new ConfigurationException("Required Attribute '" + LOGFACTORYADAPTER_ELEMENT_TYPE_ATTRIB + "' not found in element '"
                      + LOGFACTORYADAPTER_ELEMENT + "'");
                }

                try {
                    /*if (String.Compare(factoryTypeString, "CONSOLE", true) == 0) {
                        // Unity - Console adapter requires Unity .NET Full or .NET Core Compiler
                        //factoryType = typeof(ConsoleOutLoggerFactoryAdapter);
                    }
                    else if (String.Compare(factoryTypeString, "TRACE", true) == 0) {
                        factoryType = typeof(TraceLoggerFactoryAdapter);
                    }
                    else if (String.Compare(factoryTypeString, "NOOP", true) == 0) {
                        factoryType = typeof(NoOpLoggerFactoryAdapter);
                    }
                    else {*/
                    // Unity
                    //factoryType = Type.GetType(factoryTypeString, true, false);
                    //factoryType = targetTypeByAttribute.Values.FirstOrDefault(t => t.FullName.EndsWith(factoryTypeString));
                    factoryType = targetTypeByAttribute.Where(p => p.Key.DisplayName.Equals(factoryTypeString)).Select(p => p.Value).FirstOrDefault();

                    if (factoryType == null) {
                        Logger.Error("Unable to locate FactoryAdapter = {0}", factoryTypeString);
                    }
                    //}
                }
                catch (Exception e) {
                    throw new ConfigurationException("Unable to create type '" + factoryTypeString + "'", e);
                }

                XmlNodeList propertyNodes = adapterNode.SelectNodes(ARGUMENT_ELEMENT);

                //NameValueCollection properties = null;
                //properties = new NameValueCollection(); // defaults to case-insensitive keys
                //Dictionary<string, string> properties = null;
                //properties = new Dictionary<string, string>(); // defaults to case-insensitive keys

                foreach (XmlNode propertyNode in propertyNodes) {
                    string key = string.Empty;
                    string itsValue = string.Empty;

                    XmlAttribute keyAttrib = propertyNode.Attributes[ARGUMENT_ELEMENT_KEY_ATTRIB];
                    XmlAttribute valueAttrib = propertyNode.Attributes[ARGUMENT_ELEMENT_VALUE_ATTRIB];

                    if (keyAttrib == null) {
                        throw new ConfigurationException
                          ("Required Attribute '"
                            + ARGUMENT_ELEMENT_KEY_ATTRIB
                            + "' not found in element '"
                            + ARGUMENT_ELEMENT
                            + "'"
                          );
                    }
                    else {
                        key = keyAttrib.Value;
                    }

                    if (valueAttrib != null) {
                        itsValue = valueAttrib.Value;
                    }

                    properties.Add(key, itsValue);
                }

                // Unity
                foreach (XmlAttribute attribute in adapterNode.Attributes.OfType<XmlAttribute>().Where(a => !a.LocalName.Equals("name") || !a.LocalName.Equals("type"))) {
                    properties.Add(attribute.LocalName, attribute.Value);
                }
            }

            return new LogSetting(factoryType, properties);
        }

        /// <summary>
        /// Verifies that the logFactoryAdapter element appears once in the configuration section.
        /// </summary>
        /// <param name="parent">settings of a parent section - atm this must always be null</param>
        /// <param name="configContext">Additional information about the configuration process.</param>
        /// <param name="section">The configuration section to apply an XPath query too.</param>
        /// <returns>
        /// A <see cref="LogSetting" /> object containing the specified logFactoryAdapter type
        /// along with user supplied configuration properties.
        /// </returns>
        public LogSetting Create(LogSetting parent, object configContext, XmlNode section)
        {
            if (parent != null) {
                throw new ConfigurationException("parent configuration sections are not allowed");
            }

            // Unity
            //int logFactoryElementsCount = section.SelectNodes(LOGFACTORYADAPTER_ELEMENT).Count;
            /*int targetElementsCount = section.SelectNodes("target").Count;
            int logFactoryElementsCount = targetElementsCount > 0 ? targetElementsCount : section.SelectNodes(LOGFACTORYADAPTER_ELEMENT).Count;

            if (logFactoryElementsCount > 1) {
                throw new ConfigurationException("Only one <factoryAdapter> element allowed");
            }
            else if (logFactoryElementsCount == 1) {
                return ReadConfiguration(section);
            }
            else {
                return null;
            }*/

            return ReadConfiguration(section);
        }

        #region IConfigurationSectionHandler Members

        /// <summary>
        /// Verifies that the logFactoryAdapter element appears once in the configuration section.
        /// </summary>
        /// <param name="parent">The parent of the current item.</param>
        /// <param name="configContext">Additional information about the configuration process.</param>
        /// <param name="section">The configuration section to apply an XPath query too.</param>
        /// <returns>
        /// A <see cref="LogSetting" /> object containing the specified logFactoryAdapter type
        /// along with user supplied configuration properties.
        /// </returns>
        object IConfigurationSectionHandler.Create(object parent, object configContext, XmlNode section)
        {
            return Create(parent as LogSetting, configContext, section);
        }

        #endregion
    }
}
#endif
