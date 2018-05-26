using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace YourCommonTools
{

	/******************************************
	 * 
	 * ThumbnailRequestedInfo
	 * 
	 * Auxilary class to help to load images in the background (for filesystem)
	 * 
	 * @author Esteban Gallardo
	 */
	public class ThumbnailRequestedInfo
	{
		public const float DEFAULT_ICON_HEIGHT = 256;
		public const float DEFAULT_SCREEN_HEIGHT = 2560;

		private GUIContent m_contentImg;
		private string m_fullName;
		private int m_height;
		private bool m_loaded = false;

		public ThumbnailRequestedInfo(GUIContent _contentImg, string _fullName, int _height)
		{
			m_contentImg = _contentImg;
			m_fullName = _fullName;
			m_height = _height;
		}

		public void LoadTexture()
		{
			if (m_loaded) return;

			m_contentImg.image = ImageUtils.LoadTexture2D(m_fullName, m_height);
			m_loaded = true;
		}

		public void Destroy()
		{
			m_contentImg = null;
			m_loaded = true;
		}
	}
}