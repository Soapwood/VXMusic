namespace UnityEditor.Purchasing
{
    internal class GoogleConfigService
    {
        GoogleConfigurationData m_GoogleConfigData;

        static GoogleConfigService m_Instance;

        internal GoogleConfigurationData GoogleConfigData => m_GoogleConfigData;

        GoogleConfigService()
        {
            m_GoogleConfigData = new GoogleConfigurationData();
        }

        internal static GoogleConfigService Instance()
        {
            if (m_Instance == null)
            {
                m_Instance = new GoogleConfigService();
            }

            return m_Instance;
        }
    }
}
