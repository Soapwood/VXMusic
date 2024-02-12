using UnityEngine;
using UnityEditor;

using Codice.CM.Common;

using Unity.PlasticSCM.Editor.AssetMenu;
using Unity.PlasticSCM.Editor.AssetUtils.Processor;
using Unity.PlasticSCM.Editor.AssetsOverlays;
using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;
using Unity.PlasticSCM.Editor.CollabMigration;
using Unity.PlasticSCM.Editor.Inspector;
using Unity.PlasticSCM.Editor.ProjectDownloader;
using Unity.PlasticSCM.Editor.UI;

namespace Unity.PlasticSCM.Editor
{
    [InitializeOnLoad]
    internal static class PlasticPlugin
    {
        internal static IAssetStatusCache AssetStatusCache { get; private set; }

        static PlasticPlugin()
        {
            CloudProjectDownloader.Initialize();
            MigrateCollabProject.Initialize();
            EditorDispatcher.Initialize();

            CooldownWindowDelayer cooldownInitializeAction = new CooldownWindowDelayer(
                Enable, UnityConstants.PLUGIN_DELAYED_INITIALIZE_INTERVAL);
            cooldownInitializeAction.Ping();
        }

        internal static void Enable()
        {
            if (sIsEnabled)
                return;

            sIsEnabled = true;

            PlasticApp.InitializeIfNeeded();

            if (!FindWorkspace.HasWorkspace(Application.dataPath))
                return;

            EnableForWorkspace();
        }

        internal static void EnableForWorkspace()
        {
            if (sIsEnabledForWorkspace)
                return;

            WorkspaceInfo wkInfo = FindWorkspace.InfoForApplicationPath(
                Application.dataPath,
                PlasticApp.PlasticAPI);

            if (wkInfo == null)
                return;

            sIsEnabledForWorkspace = true;

            PlasticApp.SetWorkspace(wkInfo);

            AssetStatusCache = new AssetStatusCache(
                wkInfo,
                PlasticApp.PlasticAPI.IsGluonWorkspace(wkInfo));

            AssetMenuItems.Enable();
            AssetsProcessors.Enable();
            DrawAssetOverlay.Enable();
            DrawInspectorOperations.Enable();
        }

        internal static void Disable()
        {
            try
            {
                PlasticApp.Dispose();

                if (!sIsEnabledForWorkspace)
                    return;

                AssetsProcessors.Disable();
                AssetMenuItems.Disable();
                DrawAssetOverlay.Disable();
                DrawInspectorOperations.Disable();
            }
            finally
            {
                sIsEnabled = false;
                sIsEnabledForWorkspace = false;
            }
        }

        static bool sIsEnabled;
        static bool sIsEnabledForWorkspace;
    }
}
