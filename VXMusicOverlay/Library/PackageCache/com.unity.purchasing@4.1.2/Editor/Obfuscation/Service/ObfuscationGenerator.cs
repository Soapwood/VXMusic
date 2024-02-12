using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityEditor.Purchasing
{
    class ObfuscationGenerator
    {
        const string m_GeneratedCredentialsTemplateFilename = "IAPGeneratedCredentials.cs.template";
        const string m_GeneratedCredentialsTemplateFilenameNoExtension = "IAPGeneratedCredentials.cs";

        const string k_AppleCertPath = "Packages/com.unity.purchasing/Editor/AppleIncRootCertificate.cer";
        const string k_AppleStoreKitTestCertPath = "Packages/com.unity.purchasing/Editor/StoreKitTestCertificate.cer";

        const string k_AppleClassIncompleteErr = "Invalid Apple Root Certificate";
        const string k_AppleStoreKitTestClassIncompleteErr = "Invalid Apple StoreKit Test Certificate";

        internal static string ObfuscateAppleSecrets()
        {
            var appleError = WriteObfuscatedAppleClassAsAsset();

            AssetDatabase.Refresh();

            return appleError;
        }

        internal static string ObfuscateGoogleSecrets(string googlePlayPublicKey)
        {
            var googleError = WriteObfuscatedGooglePlayClassAsAsset(googlePlayPublicKey);

            AssetDatabase.Refresh();

            return googleError;
        }

        /// <summary>
        /// Generates specified obfuscated class files.
        /// </summary>
        internal static void ObfuscateSecrets(bool includeGoogle, ref string appleError, ref string googleError, string googlePlayPublicKey)
        {
            try
            {
                // First things first! Obfuscate! XHTLOA!
                appleError = WriteObfuscatedAppleClassAsAsset();

                if (includeGoogle)
                {
                    googleError = WriteObfuscatedGooglePlayClassAsAsset(googlePlayPublicKey);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.StackTrace);
            }

            // Ensure all the Tangle classes exist, even if they were not generated at this time.
            if (!DoesGooglePlayTangleClassExist())
            {
                try
                {
                    WriteObfuscatedClassAsAsset(TangleFileConsts.k_GooglePlayClassPrefix, 0, new int[0], new byte[0], false);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.StackTrace);
                }
            }

            AssetDatabase.Refresh();
        }

        static string WriteObfuscatedAppleClassAsAsset()
        {
            var err = WriteObfuscatedAppleClassAsAsset(k_AppleCertPath, k_AppleClassIncompleteErr, TangleFileConsts.k_AppleClassPrefix);

            if (err == null)
            {
                err = WriteObfuscatedAppleClassAsAsset(k_AppleStoreKitTestCertPath, k_AppleStoreKitTestClassIncompleteErr, TangleFileConsts.k_AppleStoreKitTestClassPrefix);
            }

            return err;
        }

        static string WriteObfuscatedAppleClassAsAsset(string certPath, string classIncompleteErr, string classPrefix)
        {
            string appleError = null;
            int key = 0;
            int[] order = new int[0];
            byte[] tangled = new byte[0];
            try
            {
                byte[] bytes = File.ReadAllBytes(certPath);
                order = new int[bytes.Length / 20 + 1];

                // TODO: Integrate with upgraded Tangle!

                tangled = TangleObfuscator.Obfuscate(bytes, order, out key);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{classIncompleteErr}. Generating incomplete credentials file. " + e);
                appleError = $"  {classIncompleteErr}";
            }

            WriteObfuscatedClassAsAsset(classPrefix, key, order, tangled, tangled.Length != 0);

            return appleError;
        }

        static string WriteObfuscatedGooglePlayClassAsAsset(string googlePlayPublicKey)
        {
            string googleError = null;
            int key = 0;
            int[] order = new int[0];
            byte[] tangled = new byte[0];
            try
            {
                var bytes = Convert.FromBase64String(googlePlayPublicKey);
                order = new int[bytes.Length / 20 + 1];

                tangled = TangleObfuscator.Obfuscate(bytes, order, out key);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Invalid Google Play Public Key. Generating incomplete credentials file. " + e);
                googleError =
                    "  The Google Play License Key is invalid. GooglePlayTangle was generated with incomplete credentials.";
            }
            WriteObfuscatedClassAsAsset(TangleFileConsts.k_GooglePlayClassPrefix, key, order, tangled, tangled.Length != 0);

            return googleError;
        }

        static string FullPathForTangleClass(string classnamePrefix)
        {
            return Path.Combine(TangleFileConsts.k_OutputPath, string.Format($"{classnamePrefix}{TangleFileConsts.k_ObfuscationClassSuffix}"));
        }

        internal static bool DoesAppleTangleClassExist()
        {
            return ObfuscatedClassExists(TangleFileConsts.k_AppleClassPrefix) &&
                ObfuscatedClassExists(TangleFileConsts.k_AppleStoreKitTestClassPrefix);
        }

        internal static bool DoesGooglePlayTangleClassExist()
        {
            return ObfuscatedClassExists(TangleFileConsts.k_GooglePlayClassPrefix);
        }

        static bool ObfuscatedClassExists(string classnamePrefix)
        {
            return File.Exists(FullPathForTangleClass(classnamePrefix));
        }

        static void WriteObfuscatedClassAsAsset(string classnamePrefix, int key, int[] order, byte[] data, bool populated)
        {
            Dictionary<string, string> substitutionDictionary = new Dictionary<string, string>()
            {
                {"{NAME}", classnamePrefix.ToString()},
                {"{KEY}", key.ToString()},
                {"{ORDER}", String.Format("{0}",String.Join(",", Array.ConvertAll(order, i => i.ToString())))},
                {"{DATA}", Convert.ToBase64String(data)},
                {"{POPULATED}", populated.ToString().ToLowerInvariant()} // Defaults to XML-friendly values
            };

            string templateRelativePath = null;
            string templateText = LoadTemplateText(out templateRelativePath);

            if (templateText != null)
            {
                string outfileText = templateText;

                // Apply the parameters to the template
                foreach (var pair in substitutionDictionary)
                {
                    outfileText = outfileText.Replace(pair.Key, pair.Value);
                }
                Directory.CreateDirectory (TangleFileConsts.k_OutputPath);
                File.WriteAllText(FullPathForTangleClass(classnamePrefix), outfileText);
            }
        }

        /// <summary>
        /// Loads the template file.
        /// </summary>
        /// <returns>The template file's text.</returns>
        /// <param name="templateRelativePath">Relative Assets/ path to template file.</param>
        static string LoadTemplateText(out string templateRelativePath)
        {
            string[] assetGUIDs =
                AssetDatabase.FindAssets(m_GeneratedCredentialsTemplateFilenameNoExtension);
            string templateGUID = null;
            templateRelativePath = null;

            if (assetGUIDs.Length > 0)
            {
                templateGUID = assetGUIDs[0];
            }
            else
            {
                Debug.LogError(String.Format("Could not find template \"{0}\"",
                    m_GeneratedCredentialsTemplateFilename));
            }

            string templateText = null;

            if (templateGUID != null)
            {
                templateRelativePath = AssetDatabase.GUIDToAssetPath(templateGUID);

                string templateAbsolutePath =
                    Path.GetDirectoryName(Application.dataPath)
                    + Path.DirectorySeparatorChar
                    + templateRelativePath;

                templateText = File.ReadAllText(templateAbsolutePath);
            }

            return templateText;
        }
    }
}
