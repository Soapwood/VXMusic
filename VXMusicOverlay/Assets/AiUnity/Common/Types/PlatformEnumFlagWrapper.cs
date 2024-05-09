// ***********************************************************************
// Assembly   : Assembly-CSharp
// Company    : AiUnity
// Author     : AiDesigner
//
// Created    : 07-07-2017
// Modified   : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE
using AiUnity.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using AiUnity.Common.Utilities;

namespace AiUnity.Common.Types
{
    /// <summary>
    /// Provides the tools to convert Unity Platform enums into a one-hot flag enum.
    /// </summary>
    /// <typeparam name="TEnum">The type of the t enum.</typeparam>
    public class PlatformEnumFlagWrapper<TEnum> where TEnum : struct, IComparable, IFormattable, IConvertible
    {
        #region Fields
        public Dictionary<TEnum, int> EnumValueToFlag = new Dictionary<TEnum, int>() { { default(TEnum), 0 } };
        //private IEnumerable<string> legacyPlatforms = new List<string>() { "PS3", "XBOX360", "Metro", "WP8", "BB10Player", "BlackBerry", "NaCl", "FlashPlayer" };
        private IEnumerable<string> legacyPlatforms = new List<string>() { "PS3", "XBOX360", "WP8", "BB10Player", "BlackBerry", "NaCl", "FlashPlayer" };
        #endregion

        #region Properties
        /// <summary> Gets or sets the enum flags. </summary>
        public int EnumFlags { get; set; }

        /// <summary> Gets the enum names. </summary>
        public IEnumerable<string> EnumNames { get { return EnumValues.Select(b => b.ToString()); } }

        /// <summary> Gets or sets the enum values. </summary>
        public IEnumerable<TEnum> EnumValues { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformEnumFlagWrapper{TEnum}"/> class.
        /// </summary>
        /// <param name="tEnum">The t enum.</param>
        public PlatformEnumFlagWrapper(TEnum tEnum = default(TEnum))
        {
            EnumValues = EnumUtility.GetValues<TEnum>().Where(e => !this.legacyPlatforms.Any(p => e.ToString().Contains(p))).Distinct().ToList();

            foreach (var p in EnumValues.Select((e, i) => new { e, i }))
            {
                this.EnumValueToFlag[p.e] = this.EnumValueToFlag.GetValueOrDefault(p.e) | 1 << p.i;
            }
            EnumFlags = this.EnumValueToFlag[tEnum];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformEnumFlagWrapper{TEnum}"/> class.
        /// </summary>
        /// <param name="names">The names.</param>
        public PlatformEnumFlagWrapper(string names) : this()
        {
            if (names == "Everything")
            {
                EnumFlags = -1;
            }
            else if (!string.IsNullOrEmpty(names))
            {
                foreach (string name in names.Split(',').Select(n => n.Trim()))
                {
                    Add(name.ToEnum<TEnum>());
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            if (EnumFlags == -1)
            {
                return "Everything";
            }
            return string.Join(", ", GetFlags().Select(e => e.ToString()).ToArray());
        }

        /// <summary>
        /// Determines whether [has] [the specified e].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [has] [the specified e]; otherwise, <c>false</c>.</returns>
        public bool Has(TEnum e)
        {
            return (EnumFlags & this.EnumValueToFlag[e]) != 0;
        }

        /// <summary>
        /// Adds the specified e.
        /// </summary>
        /// <param name="e">The e.</param>
        public void Add(TEnum e)
        {
            EnumFlags |= this.EnumValueToFlag[e];
        }

        /// <summary>
        /// Removes the specified e.
        /// </summary>
        /// <param name="e">The e.</param>
        public void Remove(TEnum e)
        {
            EnumFlags &= ~this.EnumValueToFlag[e];
        }

        /// <summary>
        /// Gets the flags.
        /// </summary>
        /// <returns>IEnumerable&lt;TEnum&gt;.</returns>
        public IEnumerable<TEnum> GetFlags()
        {
            for (int currentPow = 1; currentPow != 0; currentPow <<= 1)
            {
                if ((currentPow & EnumFlags) != 0)
                {
                    yield return this.EnumValueToFlag.FirstOrDefault(x => x.Value == currentPow).Key;
                }
            }
        }
        #endregion

        /// <summary>
        /// Performs an implicit conversion from <see cref="TEnum"/> to <see cref="PlatformEnumFlagWrapper{TEnum}"/>.
        /// </summary>
        /// <param name="tEnum">The t enum.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator PlatformEnumFlagWrapper<TEnum>(TEnum tEnum)  // Implicit TEnum to EnumFlagWrapper<TEnum> conversion operator
        {
            return new PlatformEnumFlagWrapper<TEnum>(tEnum);
        }
        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="PlatformEnumFlagWrapper{TEnum}"/>.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator PlatformEnumFlagWrapper<TEnum>(string names)  // Implicit string to EnumFlagWrapper<TEnum> conversion operator
        {
            return new PlatformEnumFlagWrapper<TEnum>(names);
        }
    }
}
#endif