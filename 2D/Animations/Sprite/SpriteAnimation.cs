using System;
using UnityEngine;
using System.Collections;
using UnityEngine.U2D;

namespace YourCommonTools
{
    /******************************************
	 * 
	 * SpriteAnimation
	 * 
	 * @author Esteban Gallardo
	 */
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteAnimation : AnimationBase2D, IAnimation2D
    {
        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------	
        [SerializeField]
        private SpriteAtlas m_atlas;

        [SerializeField]
        private string[] m_frames;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private SpriteRenderer m_renderer;
		
		// -------------------------------------------
		/* 
		 * Constructor		
		 */
		void OnEnable()
		{
            base.Initialize(RunAnimation, TimeFrameAnimation, LoopAnimation);
            m_renderer = this.GetComponent<SpriteRenderer>();
        }

        // -------------------------------------------
        /* 
		 * IsValid		
		 */
        public override bool IsValid()
        {
            return (m_renderer != null);
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
            string nameFrame = m_frames[m_currentFrame];
            Sprite frameSprite = m_atlas.GetSprite(nameFrame);
            m_renderer.sprite = frameSprite;
        }
	}
}