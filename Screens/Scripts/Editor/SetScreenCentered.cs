using UnityEngine;
using UnityEditor;

namespace YourCommonTools
{

    /******************************************
	 * 
	 * SetScreenCentered
	 * 
	 * @author Esteban Gallardo
	 */

    public class SetScreenCentered
    {
        [MenuItem("Assets/Center Screens")]
        static void CenterScreens()
        {
            GameObject assetRoot = Selection.activeObject as GameObject;
            string assetPath = AssetDatabase.GetAssetPath(assetRoot);
            string finalPath = assetPath.Substring(0, assetPath.LastIndexOf('/'));
            Debug.LogError("+++++++++++++CenterScreens::finalPath=" + finalPath);

            string[] guids2 = AssetDatabase.FindAssets("SCREEN_", new[] { finalPath });
            foreach (string guid2 in guids2)
            {
                string screenPath = AssetDatabase.GUIDToAssetPath(guid2);
                GameObject contentsScreen = PrefabUtility.LoadPrefabContents(screenPath);
                if (contentsScreen.GetComponent<ScreenBaseView>() != null)
                {
                    Transform contentToReparent = contentsScreen.transform.Find("Content");
                    if (contentToReparent != null)
                    {
                        string currScr = contentsScreen.name;
                        contentToReparent.GetComponent<RectTransform>().SetCenteredToParentSize(contentToReparent.parent.GetComponent<RectTransform>());
                        contentToReparent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                        contentToReparent.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 720);
                        PrefabUtility.SaveAsPrefabAsset(contentsScreen, screenPath);
                        PrefabUtility.UnloadPrefabContents(contentsScreen);
                        Debug.LogError("CONTENT REPARENTED=" + currScr);
                    }
                }
            }
        }
    }
}