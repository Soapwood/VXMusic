// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 06-20-2016
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

using System.IO;
using UnityEngine;
using AiUnity.Common.Patterns;

namespace AiUnity.Common.IO
{
    /// <summary>
    /// Unity file access based off <seealso cref="System.IO.FileInfo" />.
    /// </summary>
    //public abstract class UnityFileInfo<T> : Singleton<T> where T : class
    public abstract class UnityFileInfo<T> : Singleton<T> where T : new()
    {
        #region Properties
        /// <summary>
        /// Gets or sets the file information.
        /// </summary>
        public FileInfo FileInfo { get; set; }

        /// <summary>
        /// Gets the name without extension.
        /// </summary>
        public string NameWithoutExtension {
            get { return Path.GetFileNameWithoutExtension(FileInfo.Name); }
        }

        /// <summary>
        /// Gets the name of the relative.
        /// </summary>
        public string RelativeName {
            get { return string.IsNullOrEmpty(FileInfo.FullName) ? null : FileInfo.FullName.Substring(Application.dataPath.Length - 6); }
        }

        /// <summary>
        /// Gets the relative name without extension.
        /// </summary>
        public string RelativeNameWithoutExtension {
            get { return Path.Combine(Path.GetDirectoryName(RelativeName), NameWithoutExtension); }
        }
        #endregion
    }
}

#endif