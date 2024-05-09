// ***********************************************************************
// Assembly         : Assembly-CSharp-Editor
// Author           : AiDesigner
// Created          : 07-09-2016
// Modified         : 08-19-2018
// ***********************************************************************
#if AIUNITY_CODE

using AiUnity.Common.Extensions;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace AiUnity.Common.Services
{
    /// <summary>
    /// Variable service to manipulate variable names and values.
    /// </summary>
    /// <seealso cref="AiUnity.Common.Patterns.Singleton{AiUnity.ScriptBuilder.Editor.Services.VariableService}" />
    public class VariableService : Singleton<VariableService>
    {
#region Properties
        // Internal logger singleton
        /// <summary>
        /// Gets the logger.
        /// </summary>
        private static IInternalLogger Logger { get { return CommonInternalLogger.Instance; } }
#endregion

#region Methods
        /// <summary>
        /// Creates the camel variable.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <returns>System.String.</returns>
        public string CreateCamelVariable(params string[] names)
        {
            return CreatePascalVariable(names).LowercaseLetter();
        }

        /// <summary>
        /// Creates the pascal variable.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <returns>System.String.</returns>
        public string CreatePascalVariable(params string[] names)
        {
            if (names == null) {
                return null;
            }
            //.Replace("-", string.Empty)
            //IEnumerable<string> formatedVariable = names.SelectMany(s => s.Split()).Select(u => u.UppercaseLetter());
            IEnumerable<string> formatedVariable = names.Where(s => !string.IsNullOrEmpty(s)).SelectMany(s => s.Split(' ', '-', '.')).Select(u => u.UppercaseLetter());
            return string.Join(string.Empty, formatedVariable.ToArray());
        }

        /// <summary>
        /// Gets the formatted value.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        public string GetFormattedValue(string type, object value)
        {
            if (value is string && type != null)
            {
                //string typeName = type.Substring(type.LastIndexOf('.') + 1).Trim();
                string typeName = type.After(".").Trim();

                if (!typeName.Equals("string", StringComparison.OrdinalIgnoreCase))
                {
                    return GetFormattedValue(typeof(Object), value);
                }
            }

            return GetFormattedValue(value.GetType(), value);
        }

        /// <summary>
        /// Gets the formatted value.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <returns>System.String.</returns>
        public string GetFormattedValue(Type type, object value)
        {
            if (type == null || value == null)
            {
                return null;
            }

            if (type.Equals(typeof(string)))
            {
                if (value.ToString() == "null" && type.IsValueType)
                {
                    return null;
                }
                else
                {
                    return string.Format("\"{0}\"", value);
                }
            }

            // Detects type is either class or struct
            /*if ((value is string) && !type.IsEnum && !type.IsPrimitive)
            {
                return value.ToString();
            }
            
            if (type.IsGenericType)
            {
                return null;
            }*/

            if (value as IConvertible != null && TypeDescriptor.GetConverter(type).IsValid(value))
            {
                try
                {
                    string result = Convert.ChangeType(value, type).ToString();

                    if (type.Equals(typeof(bool)))
                    {
                        return result.ToLower();
                    }
                    if (type.Equals(typeof(float)))
                    {
                        return result + "f";
                    }
                    return result;
                }
                catch
                {
                    //Logger.Error("Invalid conversion of {0} to type {1}.", value, type);
                    //return null;
                }
            }

            return value.ToString();
        }
#endregion
    }
}
#endif