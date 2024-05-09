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
using System.IO;
using System.Xml;
using System.Collections.Generic;
using UnityEngine;
using AiUnity.Common.InternalLog;
using AiUnity.Common;

namespace AiUnity.CLog.Core.Configuration
{
	/// <summary>
	/// Implementation of <see cref="IConfigurationReader"/> that uses the standard .NET 
	/// configuration APIs, ConfigurationSettings in 1.x and ConfigurationManager in 2.0
	/// </summary>
	public class DefaultConfigurationReader : IConfigurationReader
	{
		private const string DefaultConfig = @"<?xml version=""1.0"" encoding=""utf-8""?>
<clog>
  <targets>
	<target name=""UnityConsole"" type=""UnityConsole"" logLevels=""Fatal, Error, Warn"" />
  </targets>
</clog>";

		// Internal logger singleton
		private static IInternalLogger Logger { get { return CLogInternalLogger.Instance; } }

		/// <summary>
		/// Parses the configuration section and returns the resulting object.
		/// </summary>
		/// <param name="sectionName">Name of the configuration section.</param>
		/// <returns>
		/// Object created by a corresponding <c>IConfigurationSectionHandler"</c>
		/// </returns>
		public object GetSection (string sectionName)
		{
			string fileConfigText = CLogConfigFile.Instance.GetConfigText();
			string configText = fileConfigText ?? DefaultConfig;
			XmlDocument configXml = new XmlDocument();

			try {
				configXml.LoadXml(configText);
			}
			catch (Exception e) {
				Logger.Error(e, "Failed to parse config file = {0}", CLogConfigFile.Instance.FileInfo.FullName);                
			}

			XmlNode loggingNode = configXml.DocumentElement;
			if (loggingNode == null) {
				Logger.Error("Failed to find section={0} in config file={0}", sectionName, CLogConfigFile.Instance.FileInfo.FullName);
				return null;
			}

			// Retrieve default configuration handler primarily intended for for simplified CLog XML
			IConfigurationSectionHandler configurationSectionHandler = new ConfigurationSectionHandler();
			object configObject = configurationSectionHandler.Create(null, null, loggingNode);

			if (fileConfigText != null) {
				Logger.Debug("Initialize CLog based upon {0}", CLogConfigFile.Instance.RelativeName);
			}
			else {
				//Logger.Info("Using default configuration because no CLog.xml found in a Resources directory.");
                Logger.Info("Using default configuration because unable to locate {0}.", CLogConfigFile.Instance.RelativeName);
            }

            return configObject;
		}
	}
}
#endif
