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
        private static readonly float TIME_SCALE = 20;
        public static readonly float GRAVITY = 9.81f * 0.1f;

        private float m_angleRad = Mathf.Deg2Rad * 45;
        private float m_power = 5;

        private Vector2 m_origin = new Vector2(50, 50);
        private Vector2 m_target = new Vector2(500, 100);

        private Projectile m_projectile = new Projectile();

        public void SetOrigin(Vector2 _origin)
        {
            m_origin = _origin;
        }

        public Vector2 GetOrigin()
        {
            return m_origin;
        }

        public void SetTarget(Vector2 _target)
        {
            m_target = _target;
        }

        public Vector2 GetTarget()
        {
            return m_target;
        }

        public void SetAngle(float _angleRad)
        {
            m_angleRad = _angleRad;
        }

        public float GetAngle()
        {
            return m_angleRad;
        }

        public void SetPower(float _power)
        {
            m_power = _power;
        }

        public float GetPower()
        {
            return m_power;
        }

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

        Projectile GetProjectile()
        {
            return m_projectile;
        }
    }
}