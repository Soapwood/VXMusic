// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 07-09-2016
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE
using System;

namespace AiUnity.Common.Attributes
{
    /// <summary>
    /// Indicates the date at which a type was generated.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    //[AttributeUsage(AttributeTargets.Enum)]
    public class EnumSymbolAttribute : Attribute
    {
#region Fields
        public readonly string EnumSymbol;
#endregion

#region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedTypeAttribute" /> class.
        /// </summary>
        /// <param name="enumSymbol">The creation date.</param>
        public EnumSymbolAttribute(string enumSymbol)
        {
            this.EnumSymbol = enumSymbol;
        }

        public override string ToString()
        {
            return EnumSymbol;
        }
#endregion
    }
}
#endif