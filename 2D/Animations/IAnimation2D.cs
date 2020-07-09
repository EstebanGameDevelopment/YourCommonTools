using System;
using UnityEngine;
using System.Collections;
using UnityEngine.U2D;

namespace YourCommonTools
{
    /******************************************
	 * 
	 * IAnimation2D
	 * 
	 * @author Esteban Gallardo
	 */
    public interface IAnimation2D
	{
        bool Run { get; set; }

        void Initialize(bool _run, float _time, bool _loop);
        bool IsValid();
        void RenderNewFrame();
        void UpdateAnimation(bool _force = false);
        void FlipAnimation(bool _flip);

    }
}