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
    }
}
#endif