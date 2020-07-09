using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourCommonTools
{
	[RequireComponent(typeof(Collider2D))]
	[RequireComponent(typeof(Rigidbody2D))]
    /******************************************
	* 
	* Actor2D
	* 
	* Base class of the common properties of a game's actor
	* 
	* @author Esteban Gallardo
	*/
    public class Actor2D : StateManager
	{
        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------
        public GameObject[] Animations;

        // ----------------------------------------------
        // PROTECTED MEMBERS
        // ----------------------------------------------
        protected int m_id;
        protected GameObject m_model;
        protected int m_currentAnimation = -1;

        // -------------------------------------------
        /* 
		 * Start		
		 */
        protected virtual void Start()
        {
            m_model = this.gameObject.transform.Find("Model").gameObject;
        }

        // -------------------------------------------
        /* 
		 * RenderNewFrame		
		 */
        public void UpdateAnimation()
        {
            if (Animations != null)
            {
                if (Animations.Length > 0)
                {
                    if ((m_currentAnimation >= 0) && (m_currentAnimation < Animations.Length))
                    {
                        Animations[m_currentAnimation].GetComponent<IAnimation2D>().Run = true;
                        Animations[m_currentAnimation].GetComponent<IAnimation2D>().UpdateAnimation();
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		 * ChangeAnimation		
		 */
        public void ChangeAnimation(int _newAnimation, bool _flip)
        {
            if (Animations != null)
            {
                if (Animations.Length > 0)
                {
                    if ((_newAnimation >= 0) && (_newAnimation < Animations.Length))
                    {
                        m_currentAnimation = _newAnimation;
                        for (int i = 0; i < Animations.Length; i++)
                        {
                            Animations[i].GetComponent<IAnimation2D>().Run = false;
                            Animations[i].SetActive(false);
                        }
                        Animations[m_currentAnimation].SetActive(true);
                        Animations[m_currentAnimation].GetComponent<IAnimation2D>().Run = true;
                        Animations[m_currentAnimation].GetComponent<IAnimation2D>().UpdateAnimation(true);
                        Animations[m_currentAnimation].GetComponent<IAnimation2D>().FlipAnimation(_flip);
                    }
                }
            }
        }
    }
}