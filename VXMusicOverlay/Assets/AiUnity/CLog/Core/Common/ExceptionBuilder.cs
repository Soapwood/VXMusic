#region License

/*
 * Copyright © 2002-2013 the original author or authors.
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AiUnity.CLog.Core.Common
{
    /// <summary>
    /// </summary>
    public class ExceptionBuilder
    {
        #region Fields
        private string assemblyModulePath;

        private string assemblyName;

        private string exceptionDelimiter = string.Empty;

        private string methodName;

        private string stackTrace;

        private string typeName;
        #endregion

        #region Properties
        public IFormatProvider FormatProvider { get; private set; }

        private StringBuilder ExceptionText { get; set; }
        #endregion

        #region Constructors
        public ExceptionBuilder(StringBuilder stringBuilder = null, IFormatProvider formatProvider = null)
        {
            //Exception = exception;
            FormatProvider = formatProvider ?? CultureInfo.InvariantCulture;
            //ExceptionText = new StringBuilder();
            ExceptionText = stringBuilder ?? new StringBuilder();
        }
        #endregion

        #region Methods
        public void Format(Exception exception)
        {
            if (exception == null)
                return;

            // push all inner exceptions onto stack
            //var exceptionStack = new Stack<Exception>();
            var currentException = exception;
            while (currentException != null)
            {
                //exceptionStack.Push(currentException);
                FormatSingleException(currentException);
                currentException = currentException.InnerException;
            }

            // go through inner exceptions in reverse order
            //var sb = new StringBuilder();
            /*for (int i = 1; exceptionStack.Count > 0; i++) {
                currentException = exceptionStack.Pop();
                FormatSingleException(currentException, i);
            }*/

            //return ExceptionText.ToString();
        }

        public override string ToString()
        {
            return ExceptionText.ToString();
        }

        private void AddData(Exception exception)
        {
            if (exception.Data.Count > 0)
            {
                ExceptionText.AppendFormat("{1}{0}{1}", wrapLabel("Data:"), Environment.NewLine);
                foreach (DictionaryEntry entry in exception.Data)
                {
                    ExceptionText.AppendFormat(FormatProvider,
                        "{0} = \"{1}\"{2}",
                        entry.Key, entry.Value, Environment.NewLine);
                }
            }
        }

        private void AddFooter()
        {
            ExceptionText.Append(this.exceptionDelimiter);
        }

        private void AddHeader(Exception exception)
        {
            string exceptionHeader = string.Format("Exception: {0}{1}", exception.GetType().FullName, Environment.NewLine);
            this.exceptionDelimiter = new String('*', exceptionHeader.Length) + Environment.NewLine;
            ExceptionText.Append(Environment.NewLine);
            ExceptionText.Append(this.exceptionDelimiter);
            ExceptionText.AppendFormat(FormatProvider, exceptionHeader);
            ExceptionText.Append(this.exceptionDelimiter);
        }

        private void AddMessage(Exception exception)
        {
            ExceptionText.AppendFormat(FormatProvider, "{0,-11} {1}{2}", wrapLabel("Message:"), exception.Message, Environment.NewLine);
        }

        private void AddOverview(Exception exception)
        {
            ExceptionText.AppendFormat(FormatProvider, "{0,-12} {1}{2}", wrapLabel("Method:"), this.methodName, Environment.NewLine);
            ExceptionText.AppendFormat(FormatProvider, "{0,-13} {1}{2}", wrapLabel("Type:"), this.typeName, Environment.NewLine);
            ExceptionText.AppendFormat(FormatProvider, "{0,-10} {1}{2}", wrapLabel("Assembly:"), this.assemblyName, Environment.NewLine);
            ExceptionText.AppendFormat(FormatProvider, "{0,-12} {1}{2}", wrapLabel("Location:"), this.assemblyModulePath, Environment.NewLine);
            if (!string.IsNullOrEmpty(exception.HelpLink))
            {
                ExceptionText.AppendFormat(FormatProvider, "{0,-13}{1}", wrapLabel("Help:"), exception.HelpLink, Environment.NewLine);
            }
        }

        private void AddProperties(Exception exception)
        {
            var properties = exception.GetType().GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public);

            Boolean first = true;
            foreach (PropertyInfo property in properties)
            {
                if (property.DeclaringType == typeof(Exception))
                    continue;
                if (property.Name == "Message")
                    continue;

                if (first)
                {
                    first = false;
                    ExceptionText.AppendFormat("{1}{0}{1}", wrapLabel("Properties:"), Environment.NewLine);
                }

                Object propertyValue = "<unavailable>";
                if (property.CanRead)
                    propertyValue = property.GetValue(exception, null);

                var enumerableValue = propertyValue as IEnumerable;

                if (enumerableValue == null || propertyValue is String)
                {
#if UNITY_WSA
                    ExceptionText.AppendFormat(FormatProvider, "  {1} = \"{2}\"{3}",
                        property.Name, propertyValue, Environment.NewLine);
#else
                    ExceptionText.AppendFormat(FormatProvider, "  {0}.{1} = \"{2}\"{3}",
                        property.ReflectedType.Name, property.Name, propertyValue, Environment.NewLine);
#endif
                }
                else
                {
#if UNITY_WSA
                    ExceptionText.AppendFormat(FormatProvider, "  {1} = {{{2}",
                        property.Name, Environment.NewLine);
#else
                    ExceptionText.AppendFormat(FormatProvider, "  {0}.{1} = {{{2}",
                        property.ReflectedType.Name, property.Name, Environment.NewLine);
#endif

                    foreach (var item in enumerableValue)
                        ExceptionText.AppendFormat("    \"{0}\",{1}", item != null ? item.ToString() : "<null>", Environment.NewLine);

                    ExceptionText.AppendFormat("  }}{0}", Environment.NewLine);
                }
            }
        }

        private void AddStackTrace(Exception exception)
        {
            ExceptionText.AppendFormat(FormatProvider, "{2}{0}{2}{1}{2}", wrapLabel("Stack Trace:"), this.stackTrace, Environment.NewLine);
        }

        private void AnalyzeException(Exception exception)
        {
            MethodBase targetSite = exception.TargetSite;

            if (targetSite != null)
            {
                this.methodName = targetSite.Name;
                Type type = targetSite.ReflectedType;
                this.typeName = type.FullName;
                Assembly assembly = type.Assembly;
                this.assemblyName = assembly.FullName;
                Module assemblyModule = assembly.ManifestModule;
                this.assemblyModulePath = assemblyModule.FullyQualifiedName;
                this.stackTrace = exception.StackTrace;
            }
            else
            {
                StackTrace st = new StackTrace(true);
                if (st != null)
                {
                    List<StackFrame> stackFrames = st.GetFrames().ToList();
                    int lastCoreIndex = stackFrames.FindLastIndex(s => s.GetMethod().DeclaringType.Namespace.StartsWith("AiUnity.CLog.Core"));
                    var externalStackFrames = stackFrames.Where((s, i) => i > lastCoreIndex).ToList();
                    StackFrame stackFrame = externalStackFrames.FirstOrDefault();
                    this.methodName = stackFrame.GetMethod().Name;
                    this.typeName = stackFrame.GetMethod().DeclaringType.FullName;
                    this.assemblyName = stackFrame.GetMethod().DeclaringType.AssemblyQualifiedName;
                    this.assemblyModulePath = stackFrame.GetMethod().DeclaringType.Assembly.ManifestModule.FullyQualifiedName;
                    this.stackTrace = string.Join(Environment.NewLine, externalStackFrames.Select(f => f.ToString()).ToArray());
                }
            }

        }

        private void FormatSingleException(Exception exception)
        {
            AnalyzeException(exception);

            AddHeader(exception);
            AddMessage(exception);
            AddOverview(exception);
            AddProperties(exception);
            AddData(exception);
            AddStackTrace(exception);
            AddFooter();
        }

        private string wrapLabel(string label)
        {
            return string.Format("{0}", label);
        }
#endregion
    }
}
#endif
