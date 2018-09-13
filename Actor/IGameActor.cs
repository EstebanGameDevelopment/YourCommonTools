using UnityEngine;

namespace YourCommonTools
{
	/******************************************
	* 
	* IGameActor
	* 
	* Interface that should be implemented by the game actors
	* 
	* @author Esteban Gallardo
	*/
	public interface IGameActor
	{
		// FUNCTIONS
		void Initialize(params object[] _list);
		bool Destroy();
		void Logic();
		GameObject GetGameObject();

        float Life { get; set; }
        string Name { get; set; }
        string ModelState { get; set; }
    }
}
