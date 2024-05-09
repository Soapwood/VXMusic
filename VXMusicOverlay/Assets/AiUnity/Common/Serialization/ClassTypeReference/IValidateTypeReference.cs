// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 07-23-2016
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

namespace AiUnity.Common.Serialization.ClassTypeReference
{
    /// <summary>
    /// Indicates class can validate if it represents a valid <seealso cref="System.Type" />
    /// </summary>
    public interface IValidateTypeReference {

        /// <summary>
        /// Determines whether the class type is valid.
        /// </summary>
        /// <returns><c>true</c> if [is valid type]; otherwise, <c>false</c>.</returns>
        bool IsValidType();
    }
}

#endif