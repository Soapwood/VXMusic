using System;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace UnityEditor.Purchasing
{
    class BuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            switch (report.summary.platform)
            {
                case BuildTarget.WSAPlayer:
                    WinRTPatcher.PatchWinRTBuild();
                    break;
                case BuildTarget.Android:
                    UnityPurchasingEditor.OnPostProcessScene();
                    break;
            }
        }
    }
}
