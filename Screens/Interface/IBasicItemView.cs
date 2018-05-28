using UnityEngine;

namespace YourCommonTools
{

	/******************************************
	 * 
	 * IBasicItemView
	 * 
	 * Interface of a basic item
	 * 
	 * @author Esteban Gallardo
	 */
	public interface IBasicItemView : IBasicView
	{
		// FUNCTIONS
		GameObject ContainerParent { get; set; }
	}
}