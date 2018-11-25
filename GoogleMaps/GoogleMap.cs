using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using YourCommonTools;
using System;

namespace YourCommonTools
{

	/******************************************
	 * 
	 * GoogleMap
	 * 
	 * Uses the Google maps API to access to it to retrieve
	 * an aproximately position to create the request that works as
	 * a reference for the provider to look for work in their
	 * area.
	 * 
	 * @author Esteban Gallardo
	 */
	public class GoogleMap : MonoBehaviour
	{
		public enum MapType
		{
			RoadMap,
			Satellite,
			Terrain,
			Hybrid
		}

		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	
		public const float ZOOM_BASE = 10;
		public const float DEFAULT_HEIGHT = 637f;
        public const int ZOOM_MAXIMUM = 17;
        public const int ZOOM_MINIMUM = 3;

        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_GOOGLEMAP_INIT_POSITION   = "EVENT_GOOGLEMAP_INIT_POSITION";
		public const string EVENT_GOOGLEMAP_SHIFT_POSITION  = "EVENT_GOOGLEMAP_SHIFT_POSITION";
        public const string EVENT_GOOGLEMAP_ZOOM            = "EVENT_GOOGLEMAP_ZOOM";
        public const string EVENT_GOOGLEMAP_SIGNAL          = "EVENT_GOOGLEMAP_SIGNAL";
        public const string EVENT_GOOGLEMAP_GPS_COORDINATES = "EVENT_GOOGLEMAP_GPS_COORDINATES";

        public const string EVENT_GOOGLEMAP_SELECTED_LOCATION = "EVENT_GOOGLEMAP_SELECTED_LOCATION";

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------	
        public GameObject Signal;
        public bool LoadOnStart = true;
		public bool AutoLocateCenter = true;
		public GoogleMapLocation CenterLocation;
		public int Zoom = 13;
		public MapType mapType;
		public int Size = 512;
		public bool DoubleResolution = false;
		public GoogleMapMarker[] Markers;
		public GoogleMapPath[] Paths;
        public string GoogleAPICredentials;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private bool m_calculateLocationWithGPS = true;
		private bool m_firstTimeLoad = true;

		// -------------------------------------------
		/* 
		 * Initialization
		 */
		public void Initialization(string _coordinateData)
		{
			BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);
            UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);

            // SET INITIAL POSITION
            if (_coordinateData.IndexOf(",") > 0)
			{
				string[] coordinates = _coordinateData.Split(',');
				CenterLocation.latitude = float.Parse(coordinates[0]);
				CenterLocation.longitude = float.Parse(coordinates[1]);
				m_calculateLocationWithGPS = false;
			}

#if UNITY_EDITOR
            CenterLocation.latitude = 41.4771073f;
            CenterLocation.longitude = 2.082185f;
            AutoLocateCenter = false;
            Zoom = 11;

            BasicSystemEventController.Instance.DelayBasicSystemEvent(GoogleMap.EVENT_GOOGLEMAP_INIT_POSITION, 1f, CenterLocation.latitude, CenterLocation.longitude);
#else
            StartCoroutine(GetPositionDevice());
#endif
        }

        // -------------------------------------------
        /* 
		 * Initialization
		 */
        public void Initialization(Vector2 _coordinateData, int _zoom)
        {
            BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);
            UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);

            // SET INITIAL POSITION
            CenterLocation.latitude = _coordinateData.x;
            CenterLocation.longitude = _coordinateData.y;
            Zoom = _zoom;

            m_calculateLocationWithGPS = false;
            LoadOnStart = false;
            AutoLocateCenter = false;

            BasicSystemEventController.Instance.DispatchBasicSystemEvent(GoogleMap.EVENT_GOOGLEMAP_INIT_POSITION, _coordinateData.x, _coordinateData.y);
        }

        // -------------------------------------------
        /* 
         * OnBasicEvent
         */
        public static float GetZoomFactor(int _zoom, float _defaultHeight = DEFAULT_HEIGHT)
        {
            float zoomFactor = 0.17f * (Screen.height / _defaultHeight);
            if (_zoom == 2) zoomFactor = 144f * (Screen.height / _defaultHeight);
            if (_zoom == 3) zoomFactor = 72f * (Screen.height / _defaultHeight);
            if (_zoom == 4) zoomFactor = 36f * (Screen.height / _defaultHeight);
            if (_zoom == 5) zoomFactor = 18f * (Screen.height / _defaultHeight);
            if (_zoom == 6) zoomFactor = 9f * (Screen.height / _defaultHeight);
            if (_zoom == 7) zoomFactor = 4.5f * (Screen.height / _defaultHeight);
            if (_zoom == 8) zoomFactor = 2.4f * (Screen.height / _defaultHeight);
            if (_zoom == 9) zoomFactor = 1.12f * (Screen.height / _defaultHeight);
            if (_zoom == 10) zoomFactor = 0.6f * (Screen.height / _defaultHeight);
            if (_zoom == 11) zoomFactor = 0.3f * (Screen.height / _defaultHeight);
            if (_zoom == 12) zoomFactor = 0.15f * (Screen.height / _defaultHeight);
            if (_zoom == 13) zoomFactor = 0.07f * (Screen.height / _defaultHeight);
            if (_zoom == 14) zoomFactor = 0.04f * (Screen.height / _defaultHeight);
            if (_zoom == 15) zoomFactor = 0.02f * (Screen.height / _defaultHeight);
            if (_zoom == 16) zoomFactor = 0.01f * (Screen.height / _defaultHeight);
            if (_zoom == 17) zoomFactor = 0.005f * (Screen.height / _defaultHeight);

            return zoomFactor;
        }


        // -------------------------------------------
        /* 
		 * Destroy
		 */
        public void Destroy()
		{
            UIEventController.Instance.UIEvent -= OnUIEvent;
            BasicSystemEventController.Instance.BasicSystemEvent -= OnBasicSystemEvent;
			GameObject.Destroy(this.gameObject);
		}

        // -------------------------------------------
        /* 
		 * GetPositionDevice
		 */
        IEnumerator GetPositionDevice()
		{
			if ((!Input.location.isEnabledByUser) || !m_calculateLocationWithGPS)
			{
				BasicSystemEventController.Instance.DispatchBasicSystemEvent(GoogleMap.EVENT_GOOGLEMAP_INIT_POSITION, CenterLocation.latitude, CenterLocation.longitude);
				yield break;
			}

			Input.location.Start();

			int maxWait = 20;
			while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
			{
				yield return new WaitForSeconds(1);
				maxWait--;
			}

			if (maxWait < 1)
			{
				print("Timed out");
				yield break;
			}

			if (Input.location.status == LocationServiceStatus.Failed)
			{
				print("Unable to determine device location");
				yield break;
			}
			else
			{
				print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
			}

			Input.location.Stop();

			BasicSystemEventController.Instance.DispatchBasicSystemEvent(GoogleMap.EVENT_GOOGLEMAP_INIT_POSITION, Input.location.lastData.latitude, Input.location.lastData.longitude);
		}

	
		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnBasicSystemEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == EVENT_GOOGLEMAP_INIT_POSITION)
			{
				CenterLocation.latitude = (float)_list[0];
				CenterLocation.longitude = (float)_list[1];
				Refresh();
				if (m_firstTimeLoad)
				{
					m_firstTimeLoad = false;
					BasicSystemEventController.Instance.DelayBasicSystemEvent(EVENT_GOOGLEMAP_INIT_POSITION, 0.1f, CenterLocation.latitude, CenterLocation.longitude);
				}
			}
        }

        // -------------------------------------------
        /* 
		 * OnUIEvent
		 */
        private void OnUIEvent(string _nameEvent, object[] _list)
        {
            if (_nameEvent == EVENT_GOOGLEMAP_SHIFT_POSITION)
            {
                Vector2 shift = (Vector2)_list[0];
                float zoomFactor = GetZoomFactor(Zoom);
                CenterLocation.longitude -= shift.x * zoomFactor;
                CenterLocation.latitude -= shift.y * zoomFactor;
                Refresh();
            }
            if (_nameEvent == EVENT_GOOGLEMAP_ZOOM)
            {
                if ((bool)_list[0])
                {
                    Zoom += 1;
                }
                else
                {
                    Zoom -= 1;
                }
                if (Zoom > ZOOM_MAXIMUM)
                {
                    Zoom = ZOOM_MAXIMUM;
                }
                if (Zoom < ZOOM_MINIMUM)
                {
                    Zoom = ZOOM_MINIMUM;
                }
#if DEBUG_MODE_DISPLAY_LOG
            Debug.LogError("EVENT_GOOGLEMAP_ZOOM::Zoom=" + Zoom);
#endif
                Refresh();
            }
            if (_nameEvent == EVENT_GOOGLEMAP_SIGNAL)
            {
                Vector3 vectorSignal = (Vector3)_list[0];
                float distanceSignal = (float)_list[1];
                
                Vector2 pos = vectorSignal * distanceSignal;
                Vector2 shift = vectorSignal * (distanceSignal / this.transform.GetComponent<RectTransform>().sizeDelta.x);

                float zoomFactor = GetZoomFactor(Zoom, Screen.height);
                float finalLongitude = CenterLocation.longitude + shift.x * zoomFactor;
                float finalLatitude = CenterLocation.latitude + shift.y * zoomFactor;
                // CenterLocation.longitude = finalLongitude;
                // CenterLocation.latitude = finalLatitude;
                // Refresh();

                UIEventController.Instance.DispatchUIEvent(EVENT_GOOGLEMAP_GPS_COORDINATES, finalLatitude, finalLongitude, pos);
            }
        }

        // -------------------------------------------
        /* 
		 * Refresh
		 */
        public void Refresh()
		{
			if (AutoLocateCenter && (Markers.Length == 0 && Paths.Length == 0))
			{
				Debug.Log("Auto Center will only work if paths or markers are used.");
			}
			GetComponent<Image>().color = new Color(1, 1, 1, 0);
			StartCoroutine(RefreshingInformationWithGoogleMaps());
		}

		// -------------------------------------------
		/* 
		 * RefreshingInformationWithGoogleMaps
		 */
		IEnumerator RefreshingInformationWithGoogleMaps()
		{
			var url = "https://maps.googleapis.com/maps/api/staticmap";
			var qs = "";
			if (!AutoLocateCenter)
			{
				if (CenterLocation.address != "")
				{
					qs += "center=" + WWW.UnEscapeURL(CenterLocation.address);
				}
				else
				{
					qs += "center=" + WWW.UnEscapeURL(string.Format("{0},{1}", CenterLocation.latitude, CenterLocation.longitude));
				}

				qs += "&zoom=" + Zoom.ToString();
			}
			qs += "&size=" + WWW.UnEscapeURL(string.Format("{0}x{0}", Size));
			qs += "&scale=" + (DoubleResolution ? "2" : "1");
			qs += "&maptype=" + mapType.ToString().ToLower();
			var usingSensor = false;
#if UNITY_IPHONE
		usingSensor = Input.location.isEnabledByUser && Input.location.status == LocationServiceStatus.Running;
#endif
			qs += "&sensor=" + (usingSensor ? "true" : "false");
            qs += "&key=" + GoogleAPICredentials;

            foreach (var i in Markers)
			{
				qs += "&markers=" + string.Format("size:{0}|color:{1}|label:{2}", i.size.ToString().ToLower(), i.color, i.label);
				foreach (var loc in i.locations)
				{
					if (loc.address != "")
						qs += "|" + WWW.UnEscapeURL(loc.address);
					else
						qs += "|" + WWW.UnEscapeURL(string.Format("{0},{1}", loc.latitude, loc.longitude));
				}
			}

			foreach (var i in Paths)
			{
				qs += "&path=" + string.Format("weight:{0}|color:{1}", i.weight, i.color);
				if (i.fill) qs += "|fillcolor:" + i.fillColor;
				foreach (var loc in i.locations)
				{
					if (loc.address != "")
						qs += "|" + WWW.UnEscapeURL(loc.address);
					else
						qs += "|" + WWW.UnEscapeURL(string.Format("{0},{1}", loc.latitude, loc.longitude));
				}
			}


			// REQUEST IMAGE
			WWW req = new WWW(url + "?" + qs);
			yield return req;
			GetComponent<CanvasRenderer>().SetTexture(req.texture);
			GetComponent<Image>().color = Color.white;

			// REQUEST INFO
			var urlInfo = "https://maps.googleapis.com/maps/api/geocode/json";
			string parametersLatLong = "latlng=" + CenterLocation.latitude + "," + CenterLocation.longitude;
            string finalURL = urlInfo + "?" + parametersLatLong + "&key=" + GoogleAPICredentials;
            WWW reqInfo = new WWW(finalURL);
			yield return reqInfo;

			bool foundCity = false;
			JSONNode jsonData = JSON.Parse(reqInfo.text);
			for (int j = 0; j < jsonData["results"].Count; j++)
			{
				JSONNode element = jsonData["results"][j];
				for (int i = 0; i < element["address_components"].Count; i++)
				{
					JSONNode addressTypes = element["address_components"][i]["types"];
					string shortName = element["address_components"][i]["short_name"];
					for (int k = 0; k < addressTypes.Count; k++)
					{
						if (addressTypes[k].Value.Equals("locality"))
						{
							if (!foundCity)
							{
								foundCity = true;
								BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_GOOGLEMAP_SELECTED_LOCATION, shortName, CenterLocation.latitude + "," + CenterLocation.longitude);
							}
						}
					}
				}
			}
		}
	}


	public enum GoogleMapColor
	{
		black,
		brown,
		green,
		purple,
		yellow,
		blue,
		gray,
		orange,
		red,
		white
	}

	[System.Serializable]
	public class GoogleMapLocation
	{
		public string address;
		public float latitude;
		public float longitude;
	}

	[System.Serializable]
	public class GoogleMapMarker
	{
		public enum GoogleMapMarkerSize
		{
			Tiny,
			Small,
			Mid
		}
		public GoogleMapMarkerSize size;
		public GoogleMapColor color;
		public string label;
		public GoogleMapLocation[] locations;

	}

	[System.Serializable]
	public class GoogleMapPath
	{
		public int weight = 5;
		public GoogleMapColor color;
		public bool fill = false;
		public GoogleMapColor fillColor;
		public GoogleMapLocation[] locations;
	}
}