using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace YourCommonTools
{
	public class ImageURLLoader: MonoBehaviour
	{
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_IMAGEURLLOADER_COMPLETED_LOADED = "EVENT_IMAGEURLLOADER_COMPLETED_LOADED";

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	

        private static ImageURLLoader _instance;

		public static ImageURLLoader Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(ImageURLLoader)) as ImageURLLoader;
					if (!_instance)
					{
						GameObject container = new GameObject();
						container.name = "ImageURLLoader";
						_instance = container.AddComponent(typeof(ImageURLLoader)) as ImageURLLoader;
					}
				}
				return _instance;
			}
		}

		private string m_URL;
		private Image m_image;
		private int m_height;
		private int m_maximumHeightAllowed;

		public void LoadURLImage(string _URL, Image _image, int _height, int _maximumHeightAllowed)
		{
			m_URL = _URL;
			m_image = _image;
			m_height = _height;
			m_maximumHeightAllowed = _maximumHeightAllowed;
			StartCoroutine(DownloadImageURL(m_URL));
		}

		IEnumerator DownloadImageURL(string _url)
		{
			// Start a download of the given URL
			WWW www = new WWW(_url);

			// Wait for download to complete
			yield return www;

			ImageUtils.TransformTexture(www.texture, m_image, m_height, m_URL, m_maximumHeightAllowed);

            UIEventController.Instance.DispatchUIEvent(EVENT_IMAGEURLLOADER_COMPLETED_LOADED, m_URL, www.texture);
		}
	}
	
}
