using UnityEngine;
using UnityEngine.UI;

namespace YourCommonTools
{

	/******************************************
	 * 
	 * ScreenLoadingView
	 * 
	 * Loading screen
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenLoadingView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_LOAD";

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        public const string EVENT_SCREENLOADING_LOAD_OR_JOIN_GAME = "EVENT_SCREENLOADING_LOAD_OR_JOIN_GAME";

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private GameObject m_root;
        private Transform m_container;
        private bool m_loadingFinished = false;
        private bool m_enabledAssetBundle = false;

        // -------------------------------------------
        /* 
		 * Constructor
		 */
        public override void Initialize(params object[] _list)
		{
            base.Initialize(_list);

            m_root = this.gameObject;
            m_container = m_root.transform.Find("Content");

            if (m_container != null)
            {
                if (m_container.Find("Text") != null) m_container.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("message.loading");
                if (m_container.Find("Info") != null) m_container.Find("Info").GetComponent<Text>().text = "";

                UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);
                if ((UIEventController.Instance.URLAssetBundle.Length > 0) && (UIEventController.Instance.VersionAssetBundle != -1))
                {
                    m_enabledAssetBundle = true;
                    AssetbundleController.Instance.AssetBundleEvent += new AssetBundleEventHandler(OnAssetBundleEvent);
                    string assetBundleURL = UIEventController.Instance.URLAssetBundle;
                    int assetBundleVersion = UIEventController.Instance.VersionAssetBundle;
                    AssetbundleController.Instance.LoadAssetBundle(assetBundleURL, assetBundleVersion);
                }
                else
                {
                    Invoke("LoadGameScene", 0.2f);
                }
            }
        }

        // -------------------------------------------
        /* 
		 * Destroy
		 */
        public override bool Destroy()
		{
			if (base.Destroy()) return true;

			UIEventController.Instance.UIEvent -= OnUIEvent;
            if (m_enabledAssetBundle) AssetbundleController.Instance.AssetBundleEvent -= OnAssetBundleEvent;
            if ((this != null) && (this.gameObject != null))
			{
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
			}			

			return false;
		}

        // -------------------------------------------
        /* 
		 * LoadGameScene
		 */
        public void LoadGameScene()
        {
            UIEventController.Instance.DispatchUIEvent(EVENT_SCREENLOADING_LOAD_OR_JOIN_GAME, false);
        }

        // -------------------------------------------
        /* 
		 * OnAssetBundleEvent
		 */
        private void OnAssetBundleEvent(string _nameEvent, object[] _list)
        {
            if (_nameEvent == AssetbundleController.EVENT_ASSETBUNDLE_ASSETS_LOADED)
            {
                if (!m_loadingFinished)
                {
                    m_loadingFinished = true;
                    m_container.Find("Info").GetComponent<Text>().text = LanguageController.Instance.GetText("message.loading.game");
                    Invoke("LoadGameScene", 0.2f);
                }
            }
            if (_nameEvent == AssetbundleController.EVENT_ASSETBUNDLE_ASSETS_PROGRESS)
            {
                if (!m_loadingFinished)
                {
                    m_container.Find("Info").GetComponent<Text>().text = LanguageController.Instance.GetText("message.download.progress") + " " + ((int)(100 * (float)_list[0])) + "%";
                }
            }
        }

        // -------------------------------------------
        /* 
		 * OnUIEvent
		 */
        private void OnUIEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == ScreenController.EVENT_FORCE_DESTRUCTION_POPUP)
			{
				Destroy();
			}
		}
	}
}