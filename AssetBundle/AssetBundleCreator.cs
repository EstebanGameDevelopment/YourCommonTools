#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace YourCommonTools
{
    public class AssetBundleCreator : MonoBehaviour
    {
        [MenuItem("Assets/Build Asset Bundle Android")]
        static void BuildBundles()
        {
            BuildPipeline.BuildAssetBundles("AssetsBundles", BuildAssetBundleOptions.None, BuildTarget.Android);
        }

        [MenuItem("Assets/Build Asset Bundle IOS")]
        static void BuildBundlesIOS()
        {
            BuildPipeline.BuildAssetBundles("AssetsBundles", BuildAssetBundleOptions.None, BuildTarget.iOS);
        }

        [MenuItem("Assets/Build Asset Bundle WebGL")]
        static void BuildBundlesWebGL()
        {
            BuildPipeline.BuildAssetBundles("AssetsBundles", BuildAssetBundleOptions.None, BuildTarget.WebGL);
        }

        [MenuItem("Assets/Build Asset Bundle Windows")]
        static void BuildBundlesWindows()
        {
            BuildPipeline.BuildAssetBundles("AssetsBundles", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        }

        [MenuItem("Assets/Build Asset Bundle MacOS")]
        static void BuildBundlesMacOS()
        {
            BuildPipeline.BuildAssetBundles("AssetsBundles", BuildAssetBundleOptions.None, BuildTarget.StandaloneOSX);
        }
    }
}
#endif