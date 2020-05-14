using UnityEngine;

namespace YourCommonTools
{
    /******************************************
     * 
     * ProjectileShooter
     * 
     * @author Esteban Gallardo
     */
    public class ProjectileShooter
    {
        // ----------------------------------------------
        // PUBLIC CONSTANTS
        // ----------------------------------------------	
        public const string EVENT_PROJECTILESHOOTER_DESTROY = "EVENT_PROJECTILESHOOTER_DESTROY";

        // ----------------------------------------------
        // PUBLIC CONSTANTS
        // ----------------------------------------------	
        public const float TOTAL_TIME = 0.6f;
        public const float TIME_SCALE = 20;
        public const float GRAVITY = 9.81f * 0.1f;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private float m_angleRad = Mathf.Deg2Rad * 45;
        private float m_power = 5;

        private Vector2 m_origin = new Vector2(50, 50);
        private Vector2 m_target = new Vector2(500, 100);

        private GameObject m_ball;
        private Vector3 m_originSource;
        private Vector3 m_direction;
        private float m_totalDistance;
        private float m_totalTrajectory = 0;
        private float m_x0;
        private float m_x1;
        private float m_timeAcum = 0;
        private float m_delayToRun = 1;
        private float m_totalTimeWithTrajectory = -1;
        private GameObject m_ballContainer;

        // -------------------------------------------
        /* 
		 * Initialitzation
		 */
        public void Initialitzation(GameObject _ball, GameObject _prefabReference, Vector3 _origin, Vector3 _target, bool _calculatePower, float _power = 2, float _delay = 0)
        {
            m_ball = _ball;
            m_originSource = _origin;
            m_delayToRun = _delay;

            m_direction = _target - _origin;
            m_totalDistance = m_direction.magnitude;
            m_direction.Normalize();

            SetOrigin(new Vector2(0, _origin.y));
            SetTarget(new Vector2(m_totalDistance, _target.y));

            float angleDeg = Mathf.Rad2Deg * GetAngle();
            float power = GetPower();
            Vector3 origin = GetOrigin();
            Vector3 target = GetTarget();
            float range = ComputeRange(origin.y);
            float impactX = range + origin.x;

            if (_calculatePower)
            {
                float requiredPower = ComputeRequiredPower();
                SetPower(requiredPower);
                /// Debug.LogError("ProjectileShooter::SET POWER TO[" + requiredPower + "]!!!!!!!!!!!!!!!!!!");
            }
            else
            {
                SetPower(_power);

                Vector2 requiredAngles = ComputeRequiredAngles();
                float requiredAngleDeg0 = Mathf.Rad2Deg * requiredAngles.x;
                float requiredAngleDeg1 = Mathf.Rad2Deg * requiredAngles.y;

                SetAngle(requiredAngles.y);
                // Debug.LogError("ProjectileShooter::SET ANGLE Y[" + requiredAngles.y + "]!!!!!!!!!!!!!!!!!!");
            }

            m_x0 = Mathf.Min(origin.x, impactX);
            m_x1 = Mathf.Max(origin.x, impactX);

            RenderPath(_prefabReference);
        }

        // -------------------------------------------
        /* 
		 * Destroy
		 */
        public void Destroy()
        {
            if (m_ballContainer != null)
            {
                GameObject.Destroy(m_ballContainer);
                m_ballContainer = null;
            }
            m_ball = null;
        }

        // -------------------------------------------
        /* 
		 * SetOrigin
		 */
        public void SetOrigin(Vector2 _origin)
        {
            m_origin = _origin;
        }

        // -------------------------------------------
        /* 
		 * GetOrigin
		 */
        public Vector2 GetOrigin()
        {
            return m_origin;
        }

        // -------------------------------------------
        /* 
		 * SetTarget
		 */
        public void SetTarget(Vector2 _target)
        {
            m_target = _target;
        }

        // -------------------------------------------
        /* 
		 * GetTarget
		 */
        public Vector2 GetTarget()
        {
            return m_target;
        }

        // -------------------------------------------
        /* 
		 * SetAngle
		 */
        public void SetAngle(float _angleRad)
        {
            m_angleRad = _angleRad;
        }

        // -------------------------------------------
        /* 
		 * GetAngle
		 */
        public float GetAngle()
        {
            return m_angleRad;
        }

        // -------------------------------------------
        /* 
		 * SetPower
		 */
        public void SetPower(float _power)
        {
            m_power = _power;
        }

        // -------------------------------------------
        /* 
		 * GetPower
		 */
        public float GetPower()
        {
            return m_power;
        }

        // -------------------------------------------
        /* 
		 * ComputeY
		 */
        public float ComputeY(float x)
        {
            // http://de.wikipedia.org/wiki/Wurfparabel
            //  #Mathematische_Beschreibung
            float g = GRAVITY;
            float b = m_angleRad;
            float v0 = m_power;
            if (b > Mathf.PI / 2)
            {
                b = Mathf.PI - b;
            }
            float cb = Mathf.Cos(b);
            return x * Mathf.Tan(b) - g / (2 * v0 * v0 * cb * cb) * x * x;
        }

        // -------------------------------------------
        /* 
		 * ComputeRange
		 */
        public float ComputeRange(float _h0)
        {
            // http://de.wikipedia.org/wiki/Wurfparabel
            //  #Reichweite_bei_von_Null_verschiedener_Anfangsh.C3.B6he
            float g = GRAVITY;
            float b = m_angleRad;
            float v0 = m_power;
            float sb = Mathf.Sin(b);
            float f0 = (v0 * v0) / (g + g) * Mathf.Sin(b + b);
            float i = 1.0f + (2 * g * _h0) / (v0 * v0 * sb * sb);
            float f1 = 1.0f + Mathf.Sqrt(i);
            return f0 * f1;
        }

        // -------------------------------------------
        /* 
		 * ComputeRequiredAngles
		 */
        public Vector2 ComputeRequiredAngles()
        {
            // http://en.wikipedia.org/wiki/Trajectory_of_a_projectile
            //  #Angle_required_to_hit_coordinate_.28x.2Cy.29
            float v0 = m_power;
            float g = GRAVITY;
            float vv = v0 * v0;
            float dx = m_target.x - m_origin.x;
            float dy = m_target.y - m_origin.y;
            float radicand = vv * vv - g * (g * dx * dx + 2 * dy * vv);
            float numerator0 = vv + Mathf.Sqrt(radicand);
            float numerator1 = vv - Mathf.Sqrt(radicand);
            float angle0 = Mathf.Atan(numerator0 / (g * dx));
            float angle1 = Mathf.Atan(numerator1 / (g * dx));
            return new Vector2(angle0, angle1);
        }

        // -------------------------------------------
        /* 
		 * ComputeRequiredPower
		 */
        public float ComputeRequiredPower()
        {
            // WolframAlpha told me so...
            float R = m_target.x - m_origin.x;
            float h0 = m_origin.y - m_target.y;
            float g = GRAVITY;
            float b = m_angleRad;
            float sb = Mathf.Sin(b);
            float isb = 1.0f / sb;
            float v0 =
                Mathf.Sqrt(2) * Mathf.Sqrt(g) * R *
                Mathf.Sqrt(1 / Mathf.Sin(2 * b)) /
                Mathf.Sqrt(h0 * Mathf.Sin(2 * b) * isb * isb + 2 * R);
            return v0;
        }

        // -------------------------------------------
        /* 
		 * PolarToCartesian
		 */
        private static Vector2 PolarToCartesian(Vector2 _polar, Vector2 _cartesian)
        {
            float x = Mathf.Cos(_polar.x) * _polar.y;
            float y = Mathf.Sin(_polar.x) * _polar.y;
            if (_cartesian == null)
            {
                _cartesian = new Vector2();
            }
            Vector2 cartesian = new Vector2(x, y);
            return cartesian;
        }

        // -------------------------------------------
        /* 
		 * RenderPath
		 */
        private void RenderPath(GameObject _prefabReference)
        {
            m_ballContainer = new GameObject();
            m_ballContainer.name = "BALL_CONTAINER";
            Vector2 originRender = GetOrigin();
            m_totalTrajectory = 0;
            Vector3 previousPosition = Vector3.zero;
            float previousAngleTangent = -1;
            Vector2 current2DPosition = Vector2.zero;
            Vector2 previous2DPosition = Vector2.zero;
            for (float x = m_x0; x <= m_totalDistance; x += 0.5f)
            {
                float y = ComputeY(Mathf.Abs(originRender.x - x));
                current2DPosition = new Vector2(x, y);
                Vector3 currentPosition = m_originSource + (m_direction * x);                
                currentPosition = new Vector3(currentPosition.x, y + m_originSource.y, currentPosition.z);
                if (_prefabReference != null)
                {
                    GameObject ballPath = GameObject.Instantiate(_prefabReference, m_ballContainer.transform);
                    ballPath.transform.position = currentPosition;
                }
                if (previousPosition != Vector3.zero)
                {
                    m_totalTrajectory += Vector3.Distance(currentPosition, previousPosition);

                    // TANGENT
                    if (previous2DPosition != Vector2.zero)
                    {
                        Vector2 directionTangent = current2DPosition - previous2DPosition;
                        float angleTangent = Mathf.Atan2(directionTangent.y, directionTangent.x);
                        if (previousAngleTangent != -1)
                        {
                            float differenceTangents = Mathf.Abs(Mathf.Abs(angleTangent) - Mathf.Abs(previousAngleTangent));
                            m_totalTrajectory += 16 * differenceTangents;
                        }
                        previousAngleTangent = angleTangent;
                    }
                }
                previousPosition = currentPosition;
                previous2DPosition = current2DPosition;
            }
            m_totalTimeWithTrajectory = (TOTAL_TIME * m_totalTrajectory) / m_totalDistance;
        }

        // -------------------------------------------
        /* 
		 * Update
		 */
        public void Update()
        {
            if (m_ball != null)
            {
                m_delayToRun -= Time.deltaTime;
                if (m_delayToRun < 0)
                {
                    m_timeAcum += Time.deltaTime;
                    float totalTimeBallDone = (m_timeAcum / m_totalTimeWithTrajectory);
                    float xUpdate = (m_x0 + totalTimeBallDone * m_totalDistance);
                    float yUpdate = ComputeY(Mathf.Abs(GetOrigin().x - xUpdate));
                    Vector3 currentPosition = m_originSource + (m_direction * xUpdate);
                    currentPosition = new Vector3(currentPosition.x, yUpdate + m_originSource.y, currentPosition.z);
                    m_ball.transform.position = currentPosition;
                    if (totalTimeBallDone > 1.2f)
                    {
                        BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_PROJECTILESHOOTER_DESTROY, this, m_ball);
                        Destroy();
                    }
                }
            }
            else
            {
                BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_PROJECTILESHOOTER_DESTROY, this, m_ball);
                Destroy();
            }
        }
    }
}