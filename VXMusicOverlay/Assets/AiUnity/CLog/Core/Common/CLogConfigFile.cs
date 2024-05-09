#if AIUNITY_CODE

using UnityEngine;
using AiUnity.Common.IO;
using AiUnity.Common.Patterns;
using System.IO;
using System.Linq;

namespace AiUnity.CLog.Core.Common
{
    public class CLogConfigFile : UnityFileInfo<CLogConfigFile>
    {
        //private CLogConfigFile()
        public CLogConfigFile()
        {
            string configFullFileName = PlayerPrefs.GetString("AiUnityCLogConfigFullFileName");

            if (string.IsNullOrEmpty(configFullFileName)) {
                string CLoggerFile = Directory.GetFiles(Application.dataPath, "CLogger.cs", SearchOption.AllDirectories).
                    Select(s => s.Replace('\\', '/')).FirstOrDefault(s => s.Contains(@"/CLog/Core/"));
                string aiUnityPath = string.IsNullOrEmpty(CLoggerFile) ? Application.dataPath : CLoggerFile.Substring(0, CLoggerFile.IndexOf("/CLog/Core/"));
                string configPath = aiUnityPath + @"/UserData/CLog/Resources";
                Directory.CreateDirectory(configPath);

                configFullFileName = configPath + "/CLog.xml";
            }
            FileInfo = new FileInfo(configFullFileName);
        }

        public void SetConfigFileName(string configFullFileName)
        {
            PlayerPrefs.SetString("AiUnityCLogConfigFullFileName", configFullFileName);
            FileInfo = new FileInfo(configFullFileName);
        }

        public string GetConfigText()
        {
            TextAsset textAsset = Resources.Load<TextAsset>(NameWithoutExtension);

            if (textAsset != null) {
                return textAsset.text;
            }
            else if (FileInfo.Exists) {
                return File.ReadAllText(FileInfo.FullName);
            }
            else {
                return null;
            }
        }
    }
}

#endif