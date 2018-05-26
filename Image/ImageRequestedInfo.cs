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
	 * ImageRequestedInfo
	 * 
	 * Auxilary class to help to load images in the background
	 * 
	 * @author Esteban Gallardo
	 */
	public class ImageRequestedInfo
	{
		private GameObject m_origin;
		private long m_id;
		private Image m_image;
		private int m_height;
		private bool m_showLoadingMessage = false;

		public GameObject Origin
		{
			get { return m_origin; }
		}
		public long Id
		{
			get { return m_id; }
		}
		public Image Image
		{
			get { return m_image; }
		}
		public int Height
		{
			get { return m_height; }
		}
		public bool ShowLoadingMessage
		{
			get { return m_showLoadingMessage; }
		}


		public ImageRequestedInfo(GameObject _origin, long _id, Image _image, int _height, bool _showLoadingMessage)
		{
			m_origin = _origin;
			m_id = _id;
			m_image = _image;
			m_height = _height;
			m_showLoadingMessage = _showLoadingMessage;
		}
	}
}