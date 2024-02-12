namespace UnityEditor.Purchasing
{
    class AppleObfuscatorSection : AbstractObfuscatorSection
    {
        protected override void ObfuscateKeys()
        {
            m_ErrorMessage = ObfuscationGenerator.ObfuscateAppleSecrets();
        }

        protected override bool DoesTangleFileExist()
        {
            return ObfuscationGenerator.DoesAppleTangleClassExist();
        }
    }
}
