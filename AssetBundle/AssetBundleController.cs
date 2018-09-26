
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YourCommonTools
{
    public delegate void AssetBundleEventHandler(string _nameEvent, params object[] _list);

    /******************************************
	 * 
	 * AssetbundleController
	 * 
	 * Manager of the asset bundle
	 * 
	 * @author Esteban Gallardo
	 */
    public class AssetbundleController : MonoBehaviour
    {
        public event AssetBundleEventHandler AssetBundleEvent;

        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_ASSETBUNDLE_ASSETS_LOADED     = "EVENT_ASSETBUNDLE_ASSETS_LOADED";
        public const string EVENT_ASSETBUNDLE_ASSETS_PROGRESS   = "EVENT_ASSETBUNDLE_ASSETS_PROGRESS";

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static AssetbundleController _instance;

        public static AssetbundleController Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType(typeof(AssetbundleController)) as AssetbundleController;
                    if (!_instance)
                    {
                        GameObject container = new GameObject();
                        DontDestroyOnLoad(container);
                        container.name = "AssetbundleController";
                        _instance = container.AddComponent(typeof(AssetbundleController)) as AssetbundleController;
                    }
                }
                return _instance;
            }
        }

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------
        private AssetBundle m_assetBundle;
        private List<TimedEventData> listEvents = new List<TimedEventData>();

        // -------------------------------------------
        /* 
		 * Constructor
		 */
        private AssetbundleController()
        {
        }

        // -------------------------------------------
        /* 
		 * OnDestroy
		 */
        void OnDestroy()
        {
            Destroy();
        }

        // -------------------------------------------
        /* 
		 * Destroy
		 */
        public void Destroy()
        {
            if (_instance != null)
            {
                Destroy(_instance.gameObject);
                _instance = null;
            }
        }

        // -------------------------------------------
        /* 
		 * Will dispatch a basic system event
		 */
        public void DispatchAssetBundleEvent(string _nameEvent, params object[] _list)
        {
            if (AssetBundleEvent != null) AssetBundleEvent(_nameEvent, _list);
        }

        // -------------------------------------------
        /* 
		 * Will add a new delayed local event to the queue
		 */
        public void DelayBasicSystemEvent(string _nameEvent, float _time, params object[] _list)
        {
            listEvents.Add(new TimedEventData(_nameEvent, _time, _list));
        }

        // -------------------------------------------
        /* 
		 * Load the asset bundle
		 */
        public bool LoadAssetBundle(string _url, int _version)
        {
            if (m_assetBundle == null)
            {
                StartCoroutine(DownloadAssetBundle(WWW.LoadFromCacheOrDownload(_url, _version)));
                // StartCoroutine(DownloadAssetBundle(new WWW(_url)));
                return false;
            }
            else
            {
                return true;
            }            
        }

        // -------------------------------------------
        /* 
		 * Couroutine to load the asset bundle
		 */
        public IEnumerator DownloadAssetBundle(WWW _www)
        {
            while (!_www.isDone)
            {
                DispatchAssetBundleEvent(EVENT_ASSETBUNDLE_ASSETS_PROGRESS, _www.progress);
                yield return new WaitForSeconds(.1f);
            }
            m_assetBundle = _www.assetBundle;
            DispatchAssetBundleEvent(EVENT_ASSETBUNDLE_ASSETS_LOADED);
        }

        // -------------------------------------------
        /* 
		 * Create a game object
		 */
        public GameObject CreateGameObject(string _name)
        {
            return Instantiate(m_assetBundle.LoadAsset(_name)) as GameObject;
        }

        // -------------------------------------------
        /* 
		 * Create a game audioclip
		 */
        public AudioClip CreateAudioclip(string _name)
        {
            return Instantiate(m_assetBundle.LoadAsset(_name)) as AudioClip;
        }

        // -------------------------------------------
        /* 
		 * Will process the queue of delayed events 
		 */
        void Update()
        {
            // DELAYED EVENTS
            for (int i = 0; i < listEvents.Count; i++)
            {
                TimedEventData eventData = listEvents[i];
                eventData.Time -= Time.deltaTime;
                if (eventData.Time <= 0)
                {
                    if ((eventData != null) && (AssetBundleEvent != null))
                    {
                        AssetBundleEvent(eventData.NameEvent, eventData.List);
                        eventData.Destroy();
                    }
                    listEvents.RemoveAt(i);
                    break;
                }
            }
        }
    }
}