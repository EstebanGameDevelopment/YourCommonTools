using UnityEngine;

namespace YourCommonTools
{

	/******************************************
	 * 
	 * KeyEventInputController
	 * 
	 * Class used to process the system game input's
	 * 
	 * @author Esteban Gallardo
	 */
	public class KeysEventInputController : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string ACTION_BUTTON_DOWN = "ACTION_BUTTON";
		public const string ACTION_BUTTON_UP = "ACTION_BUTTON_UP";
		public const string ACTION_SECONDARY_BUTTON_DOWN = "ACTION_SECONDARY_BUTTON";
		public const string ACTION_SECONDARY_BUTTON_UP = "ACTION_SECONDARY_BUTTON_UP";
		public const string ACTION_CANCEL_BUTTON = "ACTION_CANCEL_BUTTON";
		public const string ACTION_SET_ANCHOR_POSITION = "ACTION_SET_ANCHOR_POSITION";
		public const string ACTION_BACK_BUTTON = "ACTION_BACK_BUTTON";

		public const string ACTION_KEY_UP_PRESSED = "ACTION_KEY_UP_PRESSED";
		public const string ACTION_KEY_DOWN_PRESSED = "ACTION_KEY_DOWN_PRESSED";
		public const string ACTION_KEY_LEFT_PRESSED = "ACTION_KEY_LEFT_PRESSED";
		public const string ACTION_KEY_RIGHT_PRESSED = "ACTION_KEY_RIGHT_PRESSED";

		public const string ACTION_KEY_UP_RELEASED = "ACTION_KEY_UP_RELEASED";
		public const string ACTION_KEY_DOWN_RELEASED = "ACTION_KEY_DOWN_RELEASED";
		public const string ACTION_KEY_LEFT_RELEASED = "ACTION_KEY_LEFT_RELEASED";
		public const string ACTION_KEY_RIGHT_RELEASED = "ACTION_KEY_RIGHT_RELEASED";

		private int DIRECTION_NONE = -1;
		private int DIRECTION_LEFT = 1;
		private int DIRECTION_RIGHT = 2;
		private int DIRECTION_DOWN = 3;
		private int DIRECTION_UP = 4;

		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	
		private const int AXIS_KEY_NONE = -1;
		private const int AXIS_KEY_DOWN_EVENT = 0;
		private const int AXIS_KEY_UP_EVENT = 1;
		private const int AXIS_KEY_DOWN_STILL_PRESSED_CODE = 2;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static KeysEventInputController _instance;

		public static KeysEventInputController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(KeysEventInputController)) as KeysEventInputController;
					if (!_instance)
					{
						GameObject container = new GameObject();
						container.name = "KeysEventInputController";
						_instance = container.AddComponent(typeof(KeysEventInputController)) as KeysEventInputController;
					}
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private int m_currentDirection = -1;
		private bool m_enableActionOnMouseDown = true;
		private float m_timeAcumInventory = 0;

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	
		public bool EnableActionOnMouseDown
		{
			get { return m_enableActionOnMouseDown; }
			set { m_enableActionOnMouseDown = value; }
		}

		// -------------------------------------------
		/* 
		 * Initialization
		 */
		public void Initialization()
		{
		}

		// -------------------------------------------
		/* 
		 * Destroy all references
		 */
		public void Destroy()
		{
			Destroy(_instance.gameObject);
			_instance = null;
		}


		// -------------------------------------------
		/* 
		 * KeyInputManagment
		 */
		private void KeyInputManagment()
		{
			if (Input.GetKeyDown(KeyCode.LeftControl) 
				|| Input.GetKeyDown(KeyCode.JoystickButton0)
				|| Input.GetButtonDown("Fire1"))
			{
				UIEventController.Instance.DispatchUIEvent(ACTION_BUTTON_DOWN);
			}

			if (Input.GetKeyUp(KeyCode.LeftControl)
				|| Input.GetKeyUp(KeyCode.JoystickButton0)
				|| Input.GetButtonUp("Fire1"))
			{
				UIEventController.Instance.DispatchUIEvent(ACTION_BUTTON_UP);
			}

			// CANCEL BUTTON
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				UIEventController.Instance.DispatchUIEvent(ACTION_CANCEL_BUTTON);
			}
			// BACK BUTTON
			if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Joystick1Button1))
			{
				UIEventController.Instance.DispatchUIEvent(ACTION_BACK_BUTTON);
			}

			// ARROWS KEYPAD
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				m_currentDirection = DIRECTION_LEFT;
				UIEventController.Instance.DispatchUIEvent(ACTION_KEY_LEFT_PRESSED);
			}
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				m_currentDirection = DIRECTION_RIGHT;
				UIEventController.Instance.DispatchUIEvent(ACTION_KEY_RIGHT_PRESSED);
			}
			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				m_currentDirection = DIRECTION_DOWN;
				UIEventController.Instance.DispatchUIEvent(ACTION_KEY_UP_PRESSED);
			}
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				m_currentDirection = DIRECTION_UP;
				UIEventController.Instance.DispatchUIEvent(ACTION_KEY_DOWN_PRESSED);
			}

			// ARROW KEYS UP
			if (Input.GetKeyUp(KeyCode.LeftArrow))
			{
				UIEventController.Instance.DispatchUIEvent(ACTION_KEY_LEFT_RELEASED);
			}
			if (Input.GetKeyUp(KeyCode.RightArrow))
			{
				UIEventController.Instance.DispatchUIEvent(ACTION_KEY_RIGHT_RELEASED);
			}
			if (Input.GetKeyUp(KeyCode.UpArrow))
			{
				UIEventController.Instance.DispatchUIEvent(ACTION_KEY_UP_RELEASED);
			}
			if (Input.GetKeyUp(KeyCode.DownArrow))
			{
				UIEventController.Instance.DispatchUIEvent(ACTION_KEY_DOWN_RELEASED);
			}
		}

		// -------------------------------------------
		/* 
		 * Update
		 */
		void Update()
		{
			KeyInputManagment();
		}
	}

}