using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
#if IL2CPP
using AssetBundle = UnityEngine.Il2CppAssetBundle;
#elif MONO
using AssetBundle = UnityEngine.AssetBundle;
#endif

namespace Hoverboard.TemplateUtils
{
    public static class AssetBundleUtils
    {
        static Core mod;
        static MelonAssembly melonAssembly;
        static Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>();

#if IL2CPP
        private static string EnsureBundleCacheFile(string bundleFileName, byte[] bundleData)
        {
            string cacheDir = Path.Combine(MelonEnvironment.UserDataDirectory, "Hoverboard", "AssetBundles");
            Directory.CreateDirectory(cacheDir);

            string bundlePath = Path.Combine(cacheDir, bundleFileName);
            bool needsWrite = true;
            if (File.Exists(bundlePath))
            {
                try
                {
                    FileInfo fi = new FileInfo(bundlePath);
                    needsWrite = fi.Length != bundleData.LongLength;
                }
                catch
                {
                    needsWrite = true;
                }
            }

            if (needsWrite)
            {
                File.WriteAllBytes(bundlePath, bundleData);
            }

            return bundlePath;
        }
#endif

        public static void Initialize(Core coreMod)
        {
            mod = coreMod;
            melonAssembly = mod.MelonAssembly;
        }

        public static AssetBundle LoadAssetBundle(string bundleFileName)
        {
            if (assetBundles.ContainsKey(bundleFileName)) { return assetBundles[bundleFileName]; }
            try
            {
                string streamPath = $"{typeof(Core).Namespace}.Assets.{bundleFileName}";
                Stream bundleStream = melonAssembly.Assembly.GetManifestResourceStream($"{streamPath}");
                if (bundleStream == null)
                {
                    MelonLogger.Error($"AssetBundle resource '{streamPath}' not found. Check EmbeddedResource entry for '{bundleFileName}'.");
                    return null;
                }

                byte[] bundleData;
                using (bundleStream)
                using (MemoryStream ms = new MemoryStream())
                {
                    bundleStream.CopyTo(ms);
                    bundleData = ms.ToArray();
                }

                if (bundleData.Length == 0)
                {
                    MelonLogger.Error($"AssetBundle '{bundleFileName}' is empty.");
                    return null;
                }

#if IL2CPP
                AssetBundle ab = null;
                try
                {
                    // Prefer file-based loading for IL2CPP because stream handles can return a bundle with an invalid native pointer.
                    string bundlePath = EnsureBundleCacheFile(bundleFileName, bundleData);
                    ab = Il2CppAssetBundleManager.LoadFromFile(bundlePath);
                }
                catch (Exception fileEx)
                {
                    MelonLogger.Warning($"LoadFromFile failed for bundle '{bundleFileName}', trying memory fallback: {fileEx.Message}");
                }

                if (ab == null)
                {
                    try
                    {
                        ab = Il2CppAssetBundleManager.LoadFromMemory(bundleData);
                    }
                    catch (Exception memoryEx)
                    {
                        MelonLogger.Warning($"LoadFromMemory failed for bundle '{bundleFileName}', trying stream fallback: {memoryEx.Message}");
                    }
                }

                if (ab == null)
                {
                    try
                    {
                        Il2CppSystem.IO.Stream stream = new Il2CppSystem.IO.MemoryStream(bundleData);
                        ab = Il2CppAssetBundleManager.LoadFromStream(stream);
                    }
                    catch (Exception streamEx)
                    {
                        MelonLogger.Error($"LoadFromStream failed for bundle '{bundleFileName}': {streamEx}");
                    }
                }
#elif MONO
                AssetBundle ab = AssetBundle.LoadFromMemory(bundleData);
#endif

                if (ab == null)
                {
                    MelonLogger.Error($"Failed to load AssetBundle '{bundleFileName}' (loader returned null).");
                    return null;
                }

                assetBundles.Add(bundleFileName, ab);
                return ab;
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Failed to load AssetBundle '{bundleFileName}': {e}");
                return null;
            }
        }

        public static AssetBundle GetLoadedAssetBundle(string bundleName)
        {
            if (assetBundles.ContainsKey(bundleName))
            {
                return assetBundles[bundleName];
            }
            else
            {
                MelonLogger.Warning($"Asset bundle '{bundleName}' is not loaded.");
                return null;
            }
        }

        public static T LoadAssetFromBundle<T>(string assetName, string bundleName) where T : UnityEngine.Object
        {
            var bundle = GetLoadedAssetBundle(bundleName);
            if (bundle == null)
            {
                MelonLogger.Error($"Couldn't find loaded bundle '{bundleName}'.");
                return null;
            }

            T asset = null;
            try
            {
                asset = bundle.LoadAsset<T>(assetName);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Failed loading asset '{assetName}' from bundle '{bundleName}'. The bundle may be invalid or have a zero internal pointer. {ex}");
                return null;
            }

            if (asset == null)
            {
                MelonLogger.Error($"Asset '{assetName}' not found in bundle '{bundleName}'.");
                return null;
            }

            return asset;
        }
    }
}