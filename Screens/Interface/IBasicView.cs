namespace YourCommonTools
{

	/******************************************
	 * 
	 * IBasicView
	 * 
	 * Basic interface that force the programmer to 
	 * initialize and free resources to avoid memory leaks
	 * 
	 * @author Esteban Gallardo
	 */
	public interface IBasicView
	{
		// GETTERS/SETTERS
		string NameOfScreen { get; set; }

		// FUNCTIONS
		void Initialize(params object[] _list);
		bool Destroy();
		void SetActivation(bool _activation);
	}
}