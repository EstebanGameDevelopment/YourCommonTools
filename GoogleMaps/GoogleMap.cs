using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using YourCommonTools;
using YourSharingEconomyApp;

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

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_GOOGLEMAP_INIT_POSITION = "EVENT_GOOGLEMAP_INIT_POSITION";
		public const string EVENT_GOOGLEMAP_SHIFT_POSITION = "EVENT_GOOGLEMAP_SHIFT_POSITION";
		public const string EVENT_GOOGLEMAP_USER_UPDATE_VILLAGE = "EVENT_GOOGLEMAP_USER_UPDATE_VILLAGE";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------	
		public bool LoadOnStart = true;
		public bool AutoLocateCenter = true;
		public GoogleMapLocation CenterLocation;
		public int Zoom = 13;
		public MapType mapType;
		public int Size = 512;
		public bool DoubleResolution = false;
		public GoogleMapMarker[] Markers;
		public GoogleMapPath[] Paths;

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

			// SET INITIAL POSITION
			if (_coordinateData.IndexOf(",") > 0)
			{
				string[] coordinates = _coordinateData.Split(',');
				CenterLocation.latitude = float.Parse(coordinates[0]);
				CenterLocation.longitude = float.Parse(coordinates[1]);
				m_calculateLocationWithGPS = false;
			}

#if ENABLED_FACEBOOK
        if (m_calculateLocationWithGPS)
        {
            string warning = LanguageController.Instance.GetText("message.warning");
            string description = LanguageController.Instance.GetText("message.map.not.available.in.facebook");
            ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, warning, description, null, "");
        }
        else
        {
            BasicEventController.Instance.DelayBasicEvent(GoogleMap.EVENT_GOOGLEMAP_INIT_POSITION, 0.2f, centerLocation.latitude, centerLocation.longitude);
        }        
#else
			StartCoroutine(GetPositionDevice());
#endif
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
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
			if (_nameEvent == EVENT_GOOGLEMAP_SHIFT_POSITION)
			{
				Vector2 shift = (Vector2)_list[0];
				float zoomFactor = 0.17f * (Screen.height / DEFAULT_HEIGHT);
				CenterLocation.longitude -= shift.x * zoomFactor;
				CenterLocation.latitude -= shift.y * zoomFactor;
				Refresh();
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
			WWW reqInfo = new WWW(urlInfo + "?" + parametersLatLong);
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
								BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_GOOGLEMAP_USER_UPDATE_VILLAGE, shortName, CenterLocation.latitude + "," + CenterLocation.longitude);
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