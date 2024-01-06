using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TitanFall2Emotes;
using UnityEngine;

internal static class Assets
{
    internal static readonly List<AssetBundle> AssetBundles = new List<AssetBundle>();
    private static readonly Dictionary<string, int> AssetIndices = new Dictionary<string, int>();

    internal static void LoadAssetBundlesFromFolder(string folderName)
    {
        folderName = Path.Combine(Path.GetDirectoryName(TF2Plugin.PInfo.Location), folderName);
        foreach (var file in Directory.GetFiles(folderName))
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(file);

            int index = AssetBundles.Count;
            AssetBundles.Add(assetBundle);

            foreach (var assetName in assetBundle.GetAllAssetNames())
            {
                string path = assetName.ToLowerInvariant();
                if (path.StartsWith("assets/"))
                    path = path.Remove(0, "assets/".Length);

                //DebugClass.Log($"paring [{path}] with [{index}]");
                AssetIndices[path] = index;
            }

            DebugClass.Log($"Loaded AssetBundle: {Path.GetFileName(file)}");
        }
    }

    internal static T Load<T>(string assetName) where T : UnityEngine.Object
    {
        try
        {
            assetName = assetName.ToLowerInvariant();
            if (assetName.Contains(":"))
            {
                string[] path = assetName.Split(':');

                assetName = path[1].ToLowerInvariant();
            }
            if (assetName.StartsWith("assets/"))
                assetName = assetName.Remove(0, "assets/".Length);
            int index = AssetIndices[assetName];
            T asset = AssetBundles[index].LoadAsset<T>($"assets/{assetName}");
            return asset;
        }
        catch (Exception e)
        {
            DebugClass.Log($"Couldn't load asset [{assetName}] reason: {e}");
            return null;
        }

    }
}
