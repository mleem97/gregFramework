#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FFM.Plugin.PlayerModels.Editor;

/// <summary>
/// Unity editor helper for exporting a compliant player model bundle in one click.
/// </summary>
public static class ModelBundleExporter
{
    [MenuItem("FrikaMF/PlayerModels/Build Selected PlayerModel Bundle")]
    public static void BuildSelectedBundle()
    {
        GameObject selected = Selection.activeObject as GameObject;
        if (selected == null)
        {
            Debug.LogError("Select a prefab asset in Project view before exporting.");
            return;
        }

        if (selected.name != "PlayerModel_Root")
        {
            Debug.LogError("Selected prefab must be named 'PlayerModel_Root'.");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(selected);
        if (string.IsNullOrWhiteSpace(assetPath))
        {
            Debug.LogError("Could not resolve selected prefab asset path.");
            return;
        }

        string outputFolder = EditorUtility.SaveFolderPanel("Select Bundle Output Folder", "", "");
        if (string.IsNullOrWhiteSpace(outputFolder))
            return;

        string bundleName = selected.name.ToLowerInvariant() + ".bundle";

        AssetImporter importer = AssetImporter.GetAtPath(assetPath);
        importer.assetBundleName = bundleName;

        BuildPipeline.BuildAssetBundles(
            outputFolder,
            BuildAssetBundleOptions.None,
            EditorUserBuildSettings.activeBuildTarget);

        importer.assetBundleName = string.Empty;
        AssetDatabase.RemoveUnusedAssetBundleNames();

        Debug.Log($"Built player model bundle '{bundleName}' to '{outputFolder}'.");
    }
}
#endif
