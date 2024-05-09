// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 07-09-2016
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

using System;

namespace AiUnity.Common.Patterns
{
    /// <summary>
    /// A class implementing the singleton design pattern.
    /// </summary>
    /// <typeparam name="T">Class type of the singleton</typeparam>
    //public abstract class Singleton<T> where T : class
    public abstract class Singleton<T> where T : new()
    {
        //private static T instance = null;
        private static T instance = default(T);

        #region Properties
        /// <summary>
        /// Gets the instance of this singleton.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    try
                    {
                        //instance = Activator.CreateInstance(typeof(T), true) as T;
                        instance = new T();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Failed to create Singleton instance.", ex);
                    }
                }
                return instance;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Gets a value indicating whether singleton instance exists.
        /// </summary>
        public static bool InstanceExists { get { return instance != null; } }
        #endregion
    }
}
#endif