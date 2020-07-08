using System;
using UnityEngine;
using System.Collections;
using UnityEngine.U2D;

namespace YourCommonTools
{
    /******************************************
	 * 
	 * AnimationBase2D
	 * 
	 * @author Esteban Gallardo
	 */
    public class AnimationBase2D : StateManager
	{
        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------	
        public bool RunAnimation = false;
        public float TimeFrameAnimation = -1;
        public bool LoopAnimation = false;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        protected float m_frameTime;
        protected bool m_loop;
        protected int m_currentFrame = 0;

        protected bool m_run = false;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	
        public bool Run
        {
            get { return m_run; }
            set { m_run = value; }
        }
		
		// -------------------------------------------
		/* 
		 * Constructor		
		 */
		public virtual void Initialize(bool _run, float _time, bool _loop)
		{
            m_run = _run;
            m_frameTime = _time;
            m_loop = _loop;
            m_currentFrame = 0;
        }

        // -------------------------------------------
        /* 
		 * IsValid		
		 */
        public virtual bool IsValid()
        {
            return false;
        }

        // -------------------------------------------
        /* 
		 * RenderNewFrame		
		 */
        public virtual void RenderNewFrame()
        {
        }

        // -------------------------------------------
        /* 
		 * Update		
		 */
        public virtual void UpdateAnimation()
		{
            if (m_run)
            {
                if (IsValid())
                {
                    m_timeAcum += Time.deltaTime;
                    if (m_timeAcum >= m_frameTime)
                    {
                        m_timeAcum = 0;
                        m_currentFrame++;
                        RenderNewFrame();
                    }
                }
            }
        }
	}
}