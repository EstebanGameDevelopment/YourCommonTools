
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

        private Dictionary<string, Object> m_loadedObjects = new Dictionary<string, Object>();

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
                m_loadedObjects.Clear();
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
#if UNITY_EDITOR
            Debug.LogError("AssetbundleController::CreateGameObject::_name=" + _name);
#endif
            if (!m_loadedObjects.ContainsKey(_name))
            {
                m_loadedObjects.Add(_name, m_assetBundle.LoadAsset(_name));
            }

            return Instantiate(m_loadedObjects[_name]) as GameObject;
        }

        // -------------------------------------------
        /* 
		 * CreateSprite
		 */
        public Sprite CreateSprite(string _name)
        {
#if UNITY_EDITOR
            Debug.LogError("AssetbundleController::CreateSprite::_name=" + _name);
#endif
            if (!m_loadedObjects.ContainsKey(_name))
            {
                m_loadedObjects.Add(_name, m_assetBundle.LoadAsset(_name));
            }

            return Instantiate(m_loadedObjects[_name]) as Sprite;
        }

        // -------------------------------------------
        /* 
		 * CreateTexture
		 */
        public Texture2D CreateTexture(string _name)
        {
#if UNITY_EDITOR
            Debug.LogError("AssetbundleController::CreateTexture::_name=" + _name);
#endif
            if (!m_loadedObjects.ContainsKey(_name))
            {
                m_loadedObjects.Add(_name, m_assetBundle.LoadAsset(_name));
            }

            return Instantiate(m_loadedObjects[_name]) as Texture2D;
        }

        // -------------------------------------------
        /* 
		 * Create a game audioclip
		 */
        public AudioClip CreateAudioclip(string _name)
        {
            if (!m_loadedObjects.ContainsKey(_name))
            {
                m_loadedObjects.Add(_name, m_assetBundle.LoadAsset(_name));
            }

            return Instantiate(m_loadedObjects[_name]) as AudioClip;
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