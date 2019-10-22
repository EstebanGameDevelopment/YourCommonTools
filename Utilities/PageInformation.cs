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
	 * PageInformation
	 * 
	 * It keeps information to be displayed by the information page
	 * 
	 * @author Esteban Gallardo
	 */
	[System.Serializable]
	public class PageInformation
	{
		public string MyTitle;
		public string MyText;
		public Sprite MySprite;
		public string EventData;
		public GameObject Reference;
		public string OkButtonText = "";
		public string CancelButtonText = "";
        public Dictionary<string, PageInformation> Responses;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public PageInformation(string _title, string _text, Sprite _sprite, string _eventData, GameObject _reference, string _okButtonText = "", string _cancelButtonText = "", Dictionary<string, PageInformation> _responses = null)
		{
			MyTitle = _title;
			MyText = _text;
			MySprite = _sprite;
			EventData = _eventData;
			Reference = _reference;
			OkButtonText = _okButtonText;
			CancelButtonText = _cancelButtonText;
            Responses = _responses;
        }

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public PageInformation(string _title, string _text, Sprite _sprite, string _eventData)
		{
			MyTitle = _title;
			MyText = _text;
			MySprite = _sprite;
			EventData = _eventData;
			Reference = null;
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public PageInformation(string _title, string _text, Sprite _sprite, string _eventData, string _okButtonText = "", string _cancelButtonText = "")
		{
			MyTitle = _title;
			MyText = _text;
			MySprite = _sprite;
			EventData = _eventData;
			Reference = null;
			OkButtonText = _okButtonText;
			CancelButtonText = _cancelButtonText;
		}

		// -------------------------------------------
		/* 
		 * Clone
		 */
		public PageInformation Clone()
		{
			return new PageInformation(MyTitle, MyText, MySprite, EventData, Reference, OkButtonText, CancelButtonText);
		}
	}
}