#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace YourCommonTools
{
    public class ssetBundleCreator : MonoBehaviour
    {
        [MenuItem("Assets/Build Asset Bundle")]
        static void BuildBundles()
        {
            BuildPipeline.BuildAssetBundles("AssetsBundles", BuildAssetBundleOptions.None, BuildTarget.Android);
        }
    }
}
#endif