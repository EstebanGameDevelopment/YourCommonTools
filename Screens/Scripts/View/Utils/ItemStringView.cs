using UnityEngine.UI;

namespace YourCommonTools
{

	/******************************************
	 * 
	 * ItemStringView
	 * 
	 * Display an item that only contains a textfield
	 * 
	 * @author Esteban Gallardo
	 */
	public class ItemStringView : BaseItemView, IBasicView
	{
		// ----------------------
		// PRIVATE MEMBERS
		// ----------------------
		private string m_data = "";

		// ----------------------
		// GETTERS/SETTERS
		// ----------------------
		public string Data
		{
			get { return m_data; }
		}

		// -------------------------------------------
		/* 
		 * Initialitzation of all the references to the graphic resources
		 */
		public override void Initialize(params object[] _list)
		{
			base.Initialize();
			m_data = (string)_list[0];
			this.gameObject.transform.Find("Text").GetComponent<Text>().text = m_data;
		}
	}
}