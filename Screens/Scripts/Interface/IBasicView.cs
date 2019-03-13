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
        int Layer { get; set; }
        bool IsMarkedToBeDestroyed { get; set; }
        bool MustBeDestroyed { get; }

        // FUNCTIONS
        void Initialize(params object[] _list);
		bool Destroy();
		void SetActivation(bool _activation);
	}
}