// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 07-09-2016
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

using AiUnity.Common.InternalLog;
using System;
using UnityEngine;

namespace AiUnity.Common.Serialization.ClassTypeReference
{
    /// <summary>
    /// Reference to a class <see cref="System.Type" /> with support for Unity serialization.
    /// </summary>
    /// <seealso cref="UnityEngine.ISerializationCallbackReceiver" />
    /// <seealso cref="AiUnity.Common.Serialization.IValidateType" />
    [Serializable]
    public sealed class ClassTypeReference : ISerializationCallbackReceiver, IValidateTypeReference
    {
        #region Fields
        [SerializeField]
        [HideInInspector]
        private string _classRef;
        private Type _type;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets type of class reference.
        /// </summary>
        /// <exception cref="System.ArgumentException">If <paramref name="value" /> is not a class type.</exception>
        public Type Type
        {
            get {
                return this._type;
            }
            set {
                if (value != null && !value.IsClass)
                {
                    throw new ArgumentException(string.Format("'{0}' is not a class type.", value.FullName), "value");
                }
                this._type = value;
                this._classRef = GetClassRef(value);
            }
        }

        // Internal logger singleton
        /// <summary>
        /// Gets the logger.
        /// </summary>
        private static IInternalLogger Logger { get { return CommonInternalLogger.Instance; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassTypeReference" /> class.
        /// </summary>
        public ClassTypeReference()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassTypeReference" /> class.
        /// </summary>
        /// <param name="assemblyQualifiedClassName">Assembly qualified class name.</param>
        public ClassTypeReference(string assemblyQualifiedClassName)
        {
            Type = !string.IsNullOrEmpty(assemblyQualifiedClassName)
                ? Type.GetType(assemblyQualifiedClassName)
                : null;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassTypeReference" /> class.
        /// </summary>
        /// <param name="type">Class type.</param>
        /// <exception cref="System.ArgumentException">If <paramref name="type" /> is not a class type.</exception>
        public ClassTypeReference(Type type)
        {

            Type = type;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the class reference.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>System.String.</returns>
        public static string GetClassRef(Type type)
        {
            return type != null
                ? type.FullName + ", " + type.Assembly.GetName().Name
                : string.Empty;
        }

        /// <summary>
        /// Determines whether [is valid type].
        /// </summary>
        /// <returns><c>true</c> if [is valid type]; otherwise, <c>false</c>.</returns>
        public bool IsValidType()
        {
            return Type != null;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Type != null ? Type.FullName : "(None)";
        }

        /// <summary>
        /// Implement this method to receive a callback after Unity deserialized your object.
        /// </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(this._classRef))
            {
                this._type = Type.GetType(this._classRef);

                if (this._type == null)
                {
                    Debug.LogWarning(string.Format("'{0}' was referenced but class type was not found.", this._classRef));
                }
            }
            else
            {
                this._type = null;
            }
        }

        /// <summary>
        /// Implement this method to receive a callback before Unity serializes your object.
        /// </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }
        #endregion

        /// <summary>
        /// Performs an implicit conversion from <see cref="Type"/> to <see cref="ClassTypeReference"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator ClassTypeReference(Type type)
        {
            return new ClassTypeReference(type);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="ClassTypeReference"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="typeReference">The type reference.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator string(ClassTypeReference typeReference)
        {
            return typeReference._classRef;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="ClassTypeReference"/> to <see cref="Type"/>.
        /// </summary>
        /// <param name="typeReference">The type reference.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Type(ClassTypeReference typeReference)
        {
            return typeReference.Type;
        }
    }
}

#endif