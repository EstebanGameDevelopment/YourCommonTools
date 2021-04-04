
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
        public const string EVENT_ASSETBUNDLE_ASSETS_UNKNOW_PROGRESS = "EVENT_ASSETBUNDLE_ASSETS_UNKNOW_PROGRESS";
        public const string EVENT_ASSETBUNDLE_LEVEL_XML         = "EVENT_ASSETBUNDLE_LEVEL_XML";
        public const string EVENT_ASSETBUNDLE_ONE_TIME_LOADING_ASSETS = "EVENT_ASSETBUNDLE_ONE_TIME_LOADING_ASSETS";

        public const string COOCKIE_LOADED_ASSETBUNDLE = "COOCKIE_LOADED_ASSETBUNDLE";

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
        private bool m_isLoadinAnAssetBundle = false;
        private List<ItemMultiObjectEntry> m_loadBundles = new List<ItemMultiObjectEntry>();
        private List<string> m_urlsBundle = new List<string>();
        private List<AssetBundle> m_assetBundle = new List<AssetBundle>();
        private List<TimedEventData> listEvents = new List<TimedEventData>();
        private string m_levelsXML = "";

        private Dictionary<string, Object> m_loadedObjects = new Dictionary<string, Object>();

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------
        public string LevelsXML
        {
            get { return m_levelsXML; }
            set { m_levelsXML = value; }
        }

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
            if (!m_urlsBundle.Contains(_url))
            {
                m_urlsBundle.Add(_url);
#if UNITY_WEBGL
                DispatchAssetBundleEvent(EVENT_ASSETBUNDLE_ONE_TIME_LOADING_ASSETS);
                CachedAssetBundle cacheBundle = new CachedAssetBundle();
                StartCoroutine(WebRequestAssetBundle(UnityWebRequestAssetBundle.GetAssetBundle(_url, cacheBundle)));
#else
                StartCoroutine(DownloadAssetBundle(WWW.LoadFromCacheOrDownload(_url, _version)));
#endif
                return false;
            }
            else
            {
                Invoke("AllAssetsLoaded", 0.01f);
                return true;
            }            
        }

        // -------------------------------------------
        /* 
		 * Load the asset bundle
		 */
        public bool LoadAssetBundles(string[] _urls, int _version)
        {
            for (int i = 0; i < _urls.Length; i++)
            {
                if (!m_urlsBundle.Contains(_urls[i]))
                {
                    m_loadBundles.Add(new ItemMultiObjectEntry(_urls[i], _version));
                }
            }

            if (m_loadBundles.Count == 0)
            {
                Invoke("AllAssetsLoaded", 0.01f);
                return true;
            }
            else
            {
                return false;
            }
        }

        // -------------------------------------------
        /* 
		 * AllAssetsLoaded
		 */
        public bool CheckAssetsCached()
        {
            return PlayerPrefs.GetInt(COOCKIE_LOADED_ASSETBUNDLE, -1) == 1;
        }

        // -------------------------------------------
        /* 
		 * AllAssetsLoaded
		 */
        public void AllAssetsLoaded()
        {
            m_isLoadinAnAssetBundle = true;
            if (m_loadBundles.Count == 0)
            {
                PlayerPrefs.SetInt(COOCKIE_LOADED_ASSETBUNDLE, 1);
                DispatchAssetBundleEvent(EVENT_ASSETBUNDLE_ASSETS_LOADED);
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
            m_assetBundle.Add(_www.assetBundle);
            Invoke("AllAssetsLoaded", 0.01f);
        }

        // -------------------------------------------
        /* 
		 * Couroutine to load the asset bundle
		 */
        public IEnumerator WebRequestAssetBundle(UnityWebRequest _www)
        {
            DispatchAssetBundleEvent(EVENT_ASSETBUNDLE_ASSETS_UNKNOW_PROGRESS, 0);
            yield return _www.SendWebRequest();
            if (_www.isNetworkError || _www.isHttpError)
            {
                Debug.LogError(_www.error);
            }
            else
            {
                m_assetBundle.Add(DownloadHandlerAssetBundle.GetContent(_www));
            }            
            Invoke("AllAssetsLoaded", 0.01f);
        }

        // -------------------------------------------
        /* 
		 * Create a game object
		 */
        public GameObject CreateGameObject(string _name)
        {
#if UNITY_EDITOR
            Utilities.DebugLogError("AssetbundleController::CreateGameObject::_name=" + _name);
#endif
            if (m_assetBundle.Count == 0) return null;

            foreach (AssetBundle item in m_assetBundle)
            {
                if (item.Contains(_name))
                {
                    if (!m_loadedObjects.ContainsKey(_name))
                    {
                        m_loadedObjects.Add(_name, item.LoadAsset(_name));
                    }
                    return Instantiate(m_loadedObjects[_name]) as GameObject;
                }
            }
            return null;
        }

        // -------------------------------------------
        /* 
		 * CreateSprite
		 */
        public Sprite CreateSprite(string _name)
        {
#if UNITY_EDITOR
            Utilities.DebugLogError("AssetbundleController::CreateSprite::_name=" + _name);
#endif
            if (m_assetBundle.Count == 0) return null;

            foreach (AssetBundle item in m_assetBundle)
            {
                if (item.Contains(_name))
                {
                    if (!m_loadedObjects.ContainsKey(_name))
                    {
                        m_loadedObjects.Add(_name, item.LoadAsset(_name));
                    }
                    return Instantiate(m_loadedObjects[_name]) as Sprite;
                }
            }
            return null;
        }

        // -------------------------------------------
        /* 
		 * CreateTexture
		 */
        public Texture2D CreateTexture(string _name)
        {
#if UNITY_EDITOR
            Utilities.DebugLogError("AssetbundleController::CreateTexture::_name=" + _name);
#endif
            if (m_assetBundle.Count == 0) return null;

            foreach (AssetBundle item in m_assetBundle)
            {
                if (item.Contains(_name))
                {
                    if (!m_loadedObjects.ContainsKey(_name))
                    {
                        m_loadedObjects.Add(_name, item.LoadAsset(_name));
                    }
                    return Instantiate(m_loadedObjects[_name]) as Texture2D;
                }
            }

            return null;
        }

        // -------------------------------------------
        /* 
		 * CreateMaterial
		 */
        public Material CreateMaterial(string _name)
        {
#if UNITY_EDITOR
            Utilities.DebugLogError("AssetbundleController::CreateMaterial::_name=" + _name);
#endif
            if (m_assetBundle.Count == 0) return null;

            foreach (AssetBundle item in m_assetBundle)
            {
                if (item.Contains(_name))
                {
                    if (!m_loadedObjects.ContainsKey(_name))
                    {
                        m_loadedObjects.Add(_name, item.LoadAsset(_name));
                    }
                    return Instantiate(m_loadedObjects[_name]) as Material;
                }
            }

            return null;
        }

        // -------------------------------------------
        /* 
		 * Create a game audioclip
		 */
        public AudioClip CreateAudioclip(string _name)
        {
            if (m_assetBundle.Count == 0) return null;

            foreach (AssetBundle item in m_assetBundle)
            {
                if (item.Contains(_name))
                {
                    if (!m_loadedObjects.ContainsKey(_name))
                    {
                        m_loadedObjects.Add(_name, item.LoadAsset(_name));
                    }
                    return Instantiate(m_loadedObjects[_name]) as AudioClip;
                }
            }

            return null;
        }

        // -------------------------------------------
        /* 
		 * ClearAssetBundleEvents
		 */
        public void ClearAssetBundleEvents(string _nameEvent = "")
        {
            if (_nameEvent.Length == 0)
            {
                for (int i = 0; i < listEvents.Count; i++)
                {
                    listEvents[i].Time = -1000;
                }
            }
            else
            {
                for (int i = 0; i < listEvents.Count; i++)
                {
                    TimedEventData eventData = listEvents[i];
                    if (eventData.NameEvent == _nameEvent)
                    {
                        eventData.Time = -1000;
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		 * Will process the queue of delayed events 
		 */
        void Update()
        {
            if (!m_isLoadinAnAssetBundle)
            {
                if (m_loadBundles.Count > 0)
                {
                    string assetBundleURL = (string)m_loadBundles[0].Objects[0];
                    int assetBundleVersion = (int)m_loadBundles[0].Objects[1];
                    m_loadBundles.RemoveAt(0);
                    m_isLoadinAnAssetBundle = true;
                    if (LoadAssetBundle(assetBundleURL, assetBundleVersion))
                    {
                        m_isLoadinAnAssetBundle = false;
                    }
                }
            }

            // DELAYED EVENTS
            for (int i = 0; i < listEvents.Count; i++)
            {
                TimedEventData eventData = listEvents[i];
                if (eventData.Time == -1000)
                {
                    eventData.Destroy();
                    listEvents.RemoveAt(i);
                    break;
                }
                else
                {
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
}