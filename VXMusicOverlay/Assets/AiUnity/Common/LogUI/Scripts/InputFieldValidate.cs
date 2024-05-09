// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 06-20-2016
// Modified         : 05-06-2017
// ***********************************************************************
using UnityEngine;
using UnityEngine.UI;

namespace AiUnity.Common.LogUI.Scripts
{
    /// <summary>
    /// Class InputFieldValidate.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class InputFieldValidate : MonoBehaviour
    {
        #region Fields
        public bool validateNumber = true;
        #endregion

        #region Methods
        /// <summary>
        /// Validates the input.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="charIndex">Index of the character.</param>
        /// <param name="addedChar">The added character.</param>
        /// <returns>System.Char.</returns>
        public char ValidateInput(string text, int charIndex, char addedChar)
        {
            // Change to empty character if not a number
            if (this.validateNumber && !char.IsNumber(addedChar))
            {
                addedChar = '\0';
            }
            return addedChar;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start()
        {
            InputField inputField = gameObject.GetComponent<InputField>();
            inputField.onValidateInput += ValidateInput;
        }
        #endregion
    }
}