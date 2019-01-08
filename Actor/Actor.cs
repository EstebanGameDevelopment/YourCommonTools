using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourCommonTools
{

	[RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(Rigidbody))]
	/******************************************
	* 
	* Actor
	* 
	* Base class of the common properties of a game's actor
	* 
	* @author Esteban Gallardo
	*/
	public class Actor : StateManager
	{
        // ----------------------------------------------
        // PROTECTED MEMBERS
        // ----------------------------------------------
        protected int m_id;
        protected GameObject m_model;
        protected float m_life;
		protected float m_speed;
		protected float m_yaw;
		protected float m_directionLeft;
		protected bool m_applyGravity;
        protected bool m_ignoreRigidBody = false;

		protected int m_animation = 0;
		protected int m_previousAnimation = 0;
		protected List<String> m_animationStates = null;

		protected Color m_c1 = Color.yellow;
		protected Color m_c2 = Color.red;
		protected LineRenderer m_lineRenderer;
		protected GameObject m_planeAreaVisionDetection = null;
		protected float m_rangeDetectionView;
		protected float m_angleDetectionView;

		protected bool m_initializationCommon = false;
        private bool m_hasBeenDestroyed = false;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------
        public int Id
		{
			get { return m_id; }
		}
        public float Speed
		{
			get { return m_speed; }
			set { m_speed = value; }
		}
		public virtual float Life
		{
			get { return m_life; }
			set { m_life = value; }
		}
        public int Animation
        {
            get { return m_animation; }
        }        
        public float Yaw
		{
			get { return m_yaw; }
			set
			{
				m_yaw = value;
				if (!m_ignoreRigidBody && (this.gameObject.GetComponent<Rigidbody>() != null) && (this.gameObject.GetComponent<Rigidbody>().isKinematic))
				{
					Quaternion deltaRotation = Quaternion.Euler(new Vector3(0, -m_yaw, 0));
					this.gameObject.GetComponent<Rigidbody>().MoveRotation(deltaRotation);
                }
				else
				{
					this.gameObject.transform.eulerAngles = new Vector3(0f, -m_yaw, 0f);
				}
			}
		}
		public float DirectionLeft
		{
			get { return m_directionLeft; }
			set { m_directionLeft = value; }
		}
		public bool ApplyGravity
		{
			get { return m_applyGravity; }
			set { m_applyGravity = value; }
		}

        // -------------------------------------------
        /* 
		 * Initialization of the element
		 */
        public virtual void Start()
        {
        }

        // -------------------------------------------
        /* 
		 * Destroy the current object
		 */
        public virtual bool Destroy()
        {
            if (m_hasBeenDestroyed) return true;
            m_hasBeenDestroyed = true;

            return false;
        }

        // -------------------------------------------
        /* 
		 * Initialization of the element
		 */
        public virtual void Initialize(params object[] _list)
		{
			m_id = (int)_list[0];
		}

		// ---------------------------------------------------
		/**
		* Returns the gameobject of the actor
		*/
		public GameObject GetGameObject()
		{
			return this.gameObject;
		}

		// ---------------------------------------------------
		/**
		 * Will link to the internal 3D model
		 */
		public virtual Transform GetModel()
		{
			if (m_model == null)
			{
				if (transform.Find("Model") != null)
				{
					m_model = transform.Find("Model").gameObject;
                }				
			}
			if (m_model != null)
			{
				return m_model.transform;
			}
			else
			{
				return null;
			}			
		}

		// ---------------------------------------------------
		/**
		 * Set up the animation states
		 */
		public void CreateAnimationStates(params string[] _animations)
		{
			if (m_animationStates != null)
			{
				m_animationStates.Clear();
				m_animationStates = null;
			}
			m_animationStates = new List<String>();
			for (int i = 0; i < _animations.Length; i++)
			{
				m_animationStates.Add(_animations[i]);
			}
		}

		// ---------------------------------------------------
		/**
		 * Rotate the actor to align to goal position
		 */
		public void MoveToTarget(Vector3 _goal, float _speedMovement, float _speedRotation)
		{
			LogicAlineation(new Vector3(_goal.x, _goal.y, _goal.z), _speedMovement, _speedRotation);
		}

		// -------------------------------------------
		/* 
		 * Return the collider of the player
		 */
		public Collider GetPlayerCharacterCollider()
		{
			return this.gameObject.GetComponent<Collider>();
		}

		// ---------------------------------------------------
		/**
		* Use a 2D plane math to move forward the target
		*/
		public Vector2 LogicAlineation(Vector3 _goal, float _speedMovement, float _speedRotation)
		{
			float yaw = Yaw * Mathf.Deg2Rad;
			Vector3 pos = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y, this.gameObject.transform.position.z);

			Vector3 normalVector = new Vector3(_goal.x, _goal.y, _goal.z) - pos;
			normalVector.Normalize();

			Vector2 v1 = new Vector2((float)Mathf.Cos(yaw), (float)Mathf.Sin(yaw));
			Vector2 v2 = new Vector2(_goal.x - pos.x, _goal.z - pos.z);

			float moduloV2 = v2.magnitude;
			if (moduloV2 == 0)
			{
				v2.x = 0.0f;
				v2.y = 0.0f;
			}
			else
			{
				v2.x = v2.x / moduloV2;
				v2.y = v2.y / moduloV2;
			}
			float angulo = (v1.x * v2.x) + (v1.y * v2.y);

			float increment = _speedRotation;
			if (angulo > 0.95) increment = (1 - angulo);

			// ASK DIRECTION OF THE ROTATION TO REACH THE GOAL
			float directionLeft = Utilities.AskDirectionPoint(new Vector2(pos.x, pos.z), yaw, new Vector2(_goal.x, _goal.z));
			float yawGoal = yaw;
			if (directionLeft > 0)
			{
				yawGoal += increment;
			}
			else
			{
				yawGoal -= increment;
			}
			Vector2 vf = new Vector2((float)Mathf.Cos(yawGoal), (float)Mathf.Sin(yawGoal));
			vf.Normalize();
			// Debug.DrawLine(new Vector3(pos.x, 1, pos.y), new Vector3(pos.x + vf.x, 1, pos.y + vf.y), Color.yellow);			

			// MOVE AND ROTATE
			yawGoal = yawGoal * Mathf.Rad2Deg;
			if ((_speedMovement != -1) && (_speedMovement != 0))
			{
				CharacterController controller = this.GetComponent<CharacterController>();
				if (controller == null)
				{
					this.GetComponent<Rigidbody>().MovePosition(new Vector3(this.GetComponent<Rigidbody>().position.x + (vf.x * _speedMovement * Time.deltaTime)
																			 , this.GetComponent<Rigidbody>().position.y
																			 , this.GetComponent<Rigidbody>().position.z + (vf.y * _speedMovement * Time.deltaTime)));
				}
				else
				{
					Vector3 movement = new Vector3((vf.x * _speedMovement * Time.deltaTime),
													0,
													(vf.y * _speedMovement * Time.deltaTime)) + ((normalVector.z != 0) ? Vector3.zero : (ApplyGravity ? (Physics.gravity * Time.deltaTime) : Vector3.zero));
					controller.Move(movement);
				}
			}
			DirectionLeft = directionLeft;
			Yaw = yawGoal;
            return vf;
        }

        // ---------------------------------------------------
        /**
		* Use a 2D plane math to move forward the target
		*/
        public Vector2 LogicLocalAlineation(Vector3 _goal, float _speedMovement, float _speedRotation)
        {
            float yaw = Yaw * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(this.gameObject.transform.localPosition.x, this.gameObject.transform.localPosition.y, this.gameObject.transform.localPosition.z);

            Vector3 normalVector = new Vector3(_goal.x, _goal.y, _goal.z) - pos;
            normalVector.Normalize();

            Vector2 v1 = new Vector2((float)Mathf.Cos(yaw), (float)Mathf.Sin(yaw));
            Vector2 v2 = new Vector2(_goal.x - pos.x, _goal.z - pos.z);

            float moduloV2 = v2.magnitude;
            if (moduloV2 == 0)
            {
                v2.x = 0.0f;
                v2.y = 0.0f;
            }
            else
            {
                v2.x = v2.x / moduloV2;
                v2.y = v2.y / moduloV2;
            }
            float angulo = (v1.x * v2.x) + (v1.y * v2.y);

            float increment = _speedRotation;
            if (angulo > 0.95) increment = (1 - angulo);

            // ASK DIRECTION OF THE ROTATION TO REACH THE GOAL
            float directionLeft = Utilities.AskDirectionPoint(new Vector2(pos.x, pos.z), yaw, new Vector2(_goal.x, _goal.z));
            float yawGoal = yaw;
            if (directionLeft > 0)
            {
                yawGoal += increment;
            }
            else
            {
                yawGoal -= increment;
            }
            Vector2 vf = new Vector2((float)Mathf.Cos(yawGoal), (float)Mathf.Sin(yawGoal));
            vf.Normalize();
            // Debug.DrawLine(new Vector3(pos.x, 1, pos.y), new Vector3(pos.x + vf.x, 1, pos.y + vf.y), Color.yellow);			

            // MOVE AND ROTATE
            yawGoal = yawGoal * Mathf.Rad2Deg;
            if ((_speedMovement != -1) && (_speedMovement != 0))
            {
                Vector3 movement = new Vector3((vf.x * _speedMovement * Time.deltaTime),
                                                0,
                                                (vf.y * _speedMovement * Time.deltaTime)) + ((normalVector.z != 0) ? Vector3.zero : (ApplyGravity ? (Physics.gravity * Time.deltaTime) : Vector3.zero));

                this.gameObject.transform.localPosition += movement;
            }
            DirectionLeft = directionLeft;
            Yaw = yawGoal;
            return vf;
        }


        // -------------------------------------------
        /* 
		 * Change the animation
		 */
        public virtual void ChangeAnimation(int _animation, bool _isLoop)
		{
			if ((m_animation != _animation) || (!_isLoop))
			{
				m_previousAnimation = m_animation;
				m_animation = _animation;
				if (m_animationStates != null)
				{
					if (m_animationStates.Count > 0)
					{
						if (_animation < m_animationStates.Count)
						{
							if (GetModel() != null)
							{
								Animator sAnimatorModel = GetModel().GetComponent<Animator>();
								if (sAnimatorModel != null)
								{
									string buf = m_animationStates[m_animation];
									string[] animationState = buf.Split(',');
									sAnimatorModel.SetInteger(animationState[0], int.Parse(animationState[1]));
								}
								else
								{
									if (GetModel().GetComponent<Animation>() != null)
									{
										GetModel().GetComponent<Animation>()[m_animationStates[m_animation]].wrapMode = (_isLoop ? WrapMode.Loop : WrapMode.Once);
										GetModel().GetComponent<Animation>().Play(m_animationStates[m_animation], PlayMode.StopAll);
									}
								}
							}
						}
					}
				}
			}
		}

		// ---------------------------------------------------
		/**
		 * Will move the actor by the forward vector in the desired speed
		 */
		public void MoveForward()
		{
			CharacterController controller = this.GetComponent<CharacterController>();
			if (controller == null)
			{
				if (this.GetComponent<Rigidbody>() != null)
				{
					Vector3 newPosition = this.GetComponent<Rigidbody>().position + (this.gameObject.transform.forward * Speed * Time.deltaTime);
					this.GetComponent<Rigidbody>().MovePosition(newPosition);
				}
			}
			else
			{
				controller.Move(this.gameObject.transform.forward * Speed);
			}
		}

		// -------------------------------------------
		/* 
		 * Will draw the vision of the actor
		 */
		public void UpdateVision(GameObject _planeAreaVision, Material _material, float _viewDistance, float _angleView, float _shift)
		{
			int checkRadiusInstances = 10;
			if (m_planeAreaVisionDetection == null)
			{
				m_planeAreaVisionDetection = (GameObject)Instantiate(_planeAreaVision, Vector3.zero, new Quaternion(0, 0, 0, 0));
			}

			DrawAreaVision(m_planeAreaVisionDetection, checkRadiusInstances, _viewDistance, _angleView, -1, _material, _shift);
		}

		// -------------------------------------------
		/* 
		 * Will draw the vision of the actor
		 */
		public void SetLinePosition(LineRenderer _line, int _index, Vector3 _position)
		{
			if ((_index >= 0) && (_index < 20))
			{
				_line.SetPosition(_index, _position);
			}
		}

		// -------------------------------------------
		/* 
		 * DrawAreaVision		
		 */
		private void DrawAreaVision(GameObject _planeAreaVision, int _checkRadiusInstances, float _viewDistance, float _angleView, int _layerMask, Material _material, float _shift)
		{
			bool renderLineRenderer = false;

			List<Vector3> areaDetection = new List<Vector3>();
			int counter = 0;
			Vector3 posOrigin = Utilities.ClonePoint(this.gameObject.transform.position);
			posOrigin.y += _shift;
			areaDetection.Add(posOrigin);

			float totalAngle = 2 * _angleView * Mathf.Deg2Rad;
			float entryAngle = (Yaw + _angleView) * Mathf.Deg2Rad;
			float x = _viewDistance * Mathf.Cos(entryAngle);
			float z = _viewDistance * Mathf.Sin(entryAngle);

			if (renderLineRenderer) SetLinePosition(m_lineRenderer, counter++, posOrigin);
			Vector3 posTarget = new Vector3(posOrigin.x + x, posOrigin.y, posOrigin.z + z);
			if (renderLineRenderer) SetLinePosition(m_lineRenderer, counter++, posTarget);
			areaDetection.Add(posTarget);

			float thetaScale = totalAngle / _checkRadiusInstances;
			for (int i = 0; i < _checkRadiusInstances; i++)
			{
				entryAngle -= thetaScale;
				x = _viewDistance * Mathf.Cos(entryAngle);
				z = _viewDistance * Mathf.Sin(entryAngle);

				Vector3 posTargetRadius = new Vector3(posOrigin.x + x, posOrigin.y, posOrigin.z + z);
				if (renderLineRenderer) SetLinePosition(m_lineRenderer, counter++, posTargetRadius);
				areaDetection.Add(posTargetRadius);
			}

			float endAngle = (Yaw - _angleView) * Mathf.Deg2Rad;
			x = _viewDistance * Mathf.Cos(endAngle);
			z = _viewDistance * Mathf.Sin(endAngle);
			Vector3 posTargetEnd = new Vector3(posOrigin.x + x, posOrigin.y, posOrigin.z + z);
			if (renderLineRenderer) SetLinePosition(m_lineRenderer, counter++, posTargetEnd);
			areaDetection.Add(posTargetEnd);

			if (renderLineRenderer) SetLinePosition(m_lineRenderer, counter++, posOrigin);
			areaDetection.Add(posOrigin);

			_planeAreaVision.GetComponent<PlaneFromPoly>().Init(areaDetection.ToArray(), _material);
			_planeAreaVision.GetComponent<PlaneFromPoly>().Logic(new Vector3(posOrigin.x, posOrigin.y, posOrigin.z), posOrigin.y);
		}
    }
}