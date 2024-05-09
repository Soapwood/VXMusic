// ***********************************************************************
// Assembly   : Assembly-CSharp
// Company    : AiUnity
// Author     : AiDesigner
//
// Created    : 06-20-2016
// Modified   : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AiUnity.Common.Attributes;
using System.ComponentModel;

namespace AiUnity.Common.Extensions
{
    /// <summary>
    /// Enum Extensions.
    /// </summary>
    public static class EnumExtensions
    {
        #region Methods
        /// <summary>
        /// Adds the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        public static T Add<T>(this Enum type, T value)
        {
            try
            {
                return (T)(object)(((int)(object)type | (int)(object)value));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format(
                    "Could not append value from enumerated type '{0}'.",
                    typeof(T).Name
                    ), ex);
            }
        }

        /// <summary>
        /// Gets the flags.
        /// </summary>
        /// <typeparam name="TEnum">The type of the t enum.</typeparam>
        /// <param name="input">The input.</param>
        /// <param name="checkZero">if set to <c>true</c> [check zero].</param>
        /// <param name="checkCombinators">if set to <c>true</c> [check combinators].</param>
        /// <exception cref="ArgumentException">T must be an enumerated type</exception>
        /// <exception cref="System.ArgumentException">T must be an enumerated type</exception>
        public static IEnumerable<TEnum> GetFlags<TEnum>(this TEnum input, bool checkZero = false, bool checkCombinators = false) where TEnum : struct, IComparable, IFormattable
        {
#if !UNITY_WSA
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }
#endif

            long setBits = Convert.ToInt32(input);
            if (!checkZero && (setBits == 0))
            {
                yield break;
            }

            foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
            {
                long valMask = Convert.ToInt32(value);

                if (valMask != 0 && (setBits & valMask) == valMask)
                {
                    if (checkCombinators || ((valMask & (valMask - 1)) == 0))
                    {
                        yield return value;
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether [has] [the specified value].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        public static bool Has<T>(this Enum type, T value)
        {
            try
            {
                return (((int)(object)type & (int)(object)value) == (int)(object)value);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether [is] [the specified value].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        public static bool Is<T>(this Enum type, T value)
        {
            try
            {
                return (int)(object)type == (int)(object)value;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether the specified s is enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s">The s.</param>
        public static bool IsEnum<T>(this string s)
        {
            return s.IsEnum(typeof(T));
        }

        /// <summary>
        /// Determines whether the specified type is enum.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="type">The type.</param>
        public static bool IsEnum(this string s, Type type)
        {
            bool result = Enum.IsDefined(type, s);
            
            if (!result && !string.IsNullOrEmpty(s) && type.IsDefined(typeof(FlagsAttribute), false))
            {
                string[] names = Enum.GetNames(type);
                result = s.Replace(" ", string.Empty).Split(',').All(e => names.Any(n => n.Equals(e)));
            }
            return result;
        }

        /// <summary>
        /// Removes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        public static T Remove<T>(this Enum type, T value)
        {
            try
            {
                return (T)(object)(((int)(object)type & ~(int)(object)value));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format(
                    "Could not remove value from enumerated type '{0}'.",
                    typeof(T).Name
                    ), ex);
            }
        }

        /// <summary>
        /// To the enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="i">The i.</param>
        public static T ToEnum<T>(this int i) where T : struct, IComparable, IFormattable, IConvertible
        {
            return (T)(object)i;
        }

        /// <summary>
        /// To the enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s">The s.</param>
        /// <param name="defaultValue">The default value.</param>
        public static T ToEnum<T>(this string s, T defaultValue = default(T)) where T : struct, IComparable, IFormattable, IConvertible
        {
            try
            {
                if (IsEnum<T>(s))
                {
                    return (T)Enum.Parse(typeof(T), s);
                }
            }
            catch
            {
            }
            return defaultValue;
        }

        /// <summary>
        /// To the enum.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="type">The type.</param>
        public static Enum ToEnum(this string s, Type type)
        {
            try
            {
                int tryInt;
                if (Int32.TryParse(s, out tryInt))
                {
                    return tryInt.ToEnum(type);
                }
                if (IsEnum(s, type))
                {
                    return Enum.Parse(type, s) as Enum;
                }
            }
            catch
            {
                return Activator.CreateInstance(type) as Enum;
            }
            return Activator.CreateInstance(type) as Enum;
        }

        /// <summary>
        /// To the enum.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="type">The type.</param>
        public static Enum ToEnum(this int i, Type type)
        {
            return Convert.ChangeType(i, type) as Enum;
        }

        /// <summary>
        /// To the enum safe.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s">The s.</param>
        public static T? ToEnumSafe<T>(this string s) where T : struct
        {
            return (IsEnum<T>(s) ? (T?)Enum.Parse(typeof(T), s) : null);
        }

        /// <summary>
        /// Determines whether [is flag defined] [the specified e].
        /// </summary>
        /// <param name="e">The e.</param>
        static bool IsFlagDefined(Enum e)
        {
            decimal d;
            return !decimal.TryParse(e.ToString(), out d);
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <param name="value">The value.</param>
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            //return value.ToString();
            return string.Empty;
        }

        /// <summary>
        /// Gets the symbol.
        /// </summary>
        /// <param name="value">The value.</param>
        public static string GetSymbol(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    EnumSymbolAttribute attr = Attribute.GetCustomAttribute(field, typeof(EnumSymbolAttribute)) as EnumSymbolAttribute;
                    if (attr != null)
                    {
                        return attr.EnumSymbol;
                    }
                }
            }
            return value.ToString();
        }

        /// <summary>
        /// Gets the type of the attribute string of.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumVal">The enum value.</param>
        public static string GetAttributeStringOfType<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? ((T)attributes[0]).ToString() : enumVal.ToString();
        }
#endregion
    }
}

#endif