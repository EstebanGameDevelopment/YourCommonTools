using System;
using UnityEngine;
using System.Collections;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace YourCommonTools
{
    /******************************************
	 * 
	 * UISpriteAnimation
	 * 
	 * @author Esteban Gallardo
	 */
     [RequireComponent(typeof(Image))]
    public class UISpriteAnimation : AnimationBase2D, IAnimation2D
    {
        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------	
        [SerializeField]
        private Sprite[] m_frames;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private Image m_source;

        // -------------------------------------------
        /* 
		 * Constructor		
		 */
        void OnEnable()
        {
            base.Initialize(RunAnimation, TimeFrameAnimation, LoopAnimation);
            m_source = GetComponent<Image>();
            m_source.overrideSprite = m_frames[m_currentFrame];
        }

        // -------------------------------------------
        /* 
		 * IsValid		
		 */
        public override bool IsValid()
        {
            return (m_source != null);
        }

        // -------------------------------------------
        /* 
		 * RenderNewFrame		
		 */
        public override void RenderNewFrame()
        {
            if (m_currentFrame >= m_frames.Length)
            {
                if (m_loop)
                {
                    m_currentFrame = 0;
                }
                else
                {
                    m_currentFrame = m_frames.Length - 1;
                    m_run = false;
                }
            }
            m_currentFrame = m_currentFrame % m_frames.Length;
            m_source.overrideSprite = m_frames[m_currentFrame];
        }
    }
}