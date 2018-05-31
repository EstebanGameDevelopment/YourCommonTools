using UnityEngine;

namespace YourCommonTools
{

	/******************************************
	 * 
	 * ScreenLoadingView
	 * 
	 * Loading screen
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenLoadingView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_LOAD";

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
			UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;

			UIEventController.Instance.UIEvent -= OnUIEvent;
			if ((this != null) && (this.gameObject != null))
			{
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
			}			

			return false;
		}

		// -------------------------------------------
		/* 
		 * OnUIEvent
		 */
		private void OnUIEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == ScreenController.EVENT_FORCE_DESTRUCTION_POPUP)
			{
				Destroy();
			}
		}
	}
}