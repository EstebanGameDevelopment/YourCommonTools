using UnityEngine;

namespace YourCommonTools
{
    /******************************************
     * 
     * Projectile
     * 
     * @author Esteban Gallardo
     */
    public class Projectile
    {
        private static Vector2 ACCELERATION = new Vector3(0, -ProjectileShooter.GRAVITY);

        private Vector2 m_position = Vector2.zero;
        private Vector2 m_velocity = Vector2.zero;

        public Vector2 GetPosition()
        {
            return m_position;
        }
        public void SetPosition(Vector2 _point)
        {
            m_position = _point;
        }

        public void SetVelocity(Vector2 _velocity)
        {
            m_velocity = _velocity;
        }

        public void PerformTimeStep(float _dt)
        {
            m_velocity = ScaleAddAssign(m_velocity, _dt, ACCELERATION);
            m_position = ScaleAddAssign(m_position, _dt, m_velocity);

            //System.out.println("Now at "+position+" with "+velocity);
        }

        private Vector2 ScaleAddAssign(Vector2 _result, float _factor, Vector2 _addend)
        {
            float x = _result.x + _factor * _addend.x;
            float y = _result.y + _factor * _addend.y;
            return new Vector2(x, y);
        }

    }
}