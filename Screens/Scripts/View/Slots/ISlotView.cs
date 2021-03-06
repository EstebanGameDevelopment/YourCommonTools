﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourCommonTools
{
	/******************************************
	 * 
	 * ISlotView
	 * 
	 * Interface for the slot component
	 * 
	 * @author Esteban Gallardo
	 */
	public interface ISlotView : ICustomButton
    {
		// FUNCTIONS
		void Initialize(params object[] _list);
		bool Destroy();
	}
}