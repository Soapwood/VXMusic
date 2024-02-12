using System;
using UnityEngine.UIElements;

namespace UnityEditor.Purchasing
{
    internal abstract class AbstractObfuscatorSection
    {
        const string k_ErrorLabelClass = "warning-message";
        const string k_ObfuscateKeysBtn = "ObfuscateKeysButton";
        const string k_VerificationSection = "verification";
        const string k_ErrorSection = "error-message";

        VisualElement m_ObfuscatorBlock;

        protected string m_ErrorMessage;

        internal AbstractObfuscatorSection()
        {
        }

        internal void SetupObfuscatorBlock(VisualElement parentBlock)
        {
            m_ObfuscatorBlock = parentBlock;

            PopulateConfigBlock();
        }

        void PopulateConfigBlock()
        {
            SetupButtonActions();
            SetupVerification();
            SetupErrorMessages();
        }

        void SetupButtonActions()
        {
            m_ObfuscatorBlock.Q<Button>(k_ObfuscateKeysBtn).clicked += OnObfuscateClicked;
        }

        void OnObfuscateClicked()
        {
            ObfuscateKeys();
            UpdateVerificationSection();
        }

        protected abstract void ObfuscateKeys();

        void SetupVerification()
        {
            UpdateVerificationSection();
        }

        void UpdateVerificationSection()
        {
            var verificationSection = m_ObfuscatorBlock.Q(k_VerificationSection);

            verificationSection.style.display = DoesTangleFileExist()
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        protected abstract bool DoesTangleFileExist();

        void SetupErrorMessages()
        {
            HandleErrorSection(m_ErrorMessage);
        }

        void HandleErrorSection(string errorMessage)
        {
            var errorSection = m_ObfuscatorBlock.Q(k_ErrorSection);

            errorSection.style.display = (string.IsNullOrEmpty(errorMessage))
                ? DisplayStyle.None
                : DisplayStyle.Flex;

            var errorText = errorSection.Q<Label>(className: k_ErrorLabelClass);
            errorText.text = errorMessage;
        }
    }
}
