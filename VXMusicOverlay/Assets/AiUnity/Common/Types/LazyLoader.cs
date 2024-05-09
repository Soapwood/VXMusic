// ***********************************************************************
// Assembly   : Assembly-CSharp
// Company    : AiUnity
// Author     : AiDesigner
//
// Created    : 07-07-2017
// Modified   : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE
using System;

namespace AiUnity.Common.Types
{
    /// <summary>
    /// A pre-.NET4 Lazy<T> implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LazyLoader<T> where T : class
    {
        #region Fields
        private readonly Func<T> function;
        private readonly object padlock;
        private bool hasRun;
        private T instance;
        #endregion

        #region Properties
        /// <summary> Gets the value. </summary>
        public T Value {
            get {
                lock (this.padlock)
                {
                    if (!this.hasRun)
                    {
                        this.instance = this.function.Invoke();
                        this.hasRun = true;
                    }
                }
                return this.instance;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LazyLoader{T}"/> class.
        /// </summary>
        /// <param name="function">The function.</param>
        public LazyLoader(Func<T> function)
        {
            this.hasRun = false;
            this.padlock = new object();
            this.function = function;
        }
        #endregion
    }
}
#endif