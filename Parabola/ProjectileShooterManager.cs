using UnityEngine;

namespace YourCommonTools
{
    /******************************************
     * 
     * ProjectileShooterPanel
     * 
     * @author Esteban Gallardo
     */
    public class ProjectileShooterManager : MonoBehaviour
    {
        public GameObject BallReference;

        public void RenderParabola(Vector3 _origin, Vector3 _target)
        {
            Vector3 direction = _target - _origin;
            float totalDistance = direction.magnitude;
            direction.Normalize();

            ProjectileShooter projectileShooter = new ProjectileShooter();
            projectileShooter.SetOrigin(new Vector2(0, _origin.y));
            projectileShooter.SetTarget(new Vector2(totalDistance, _target.y));

            float angleDeg = Mathf.Rad2Deg * projectileShooter.GetAngle();
            float power = projectileShooter.GetPower();
            Vector3 origin = projectileShooter.GetOrigin();
            Vector3 target = projectileShooter.GetTarget();
            float range = projectileShooter.ComputeRange(origin.y);
            float impactX = range + origin.x;

            /*
            Debug.LogError("Angle: " + angleDeg);
            Debug.LogError("Power: " + power);
            Debug.LogError("Range: " + range);
            Debug.LogError("Origin: " + origin.ToString());
            Debug.LogError("Impact X: " + impactX);
            Debug.LogError("Target: " + target.ToString());
            */

            Vector2 requiredAngles = projectileShooter.ComputeRequiredAngles();
            float requiredAngleDeg0 = Mathf.Rad2Deg * requiredAngles.x;
            float requiredAngleDeg1 = Mathf.Rad2Deg * requiredAngles.y;
            Debug.LogError("Required angle 0: " + requiredAngleDeg0);
            Debug.LogError("Required angle 1: " + requiredAngleDeg1);

            float requiredPower = projectileShooter.ComputeRequiredPower();
            Debug.LogError("Required power: " + requiredPower);

            // projectileShooter.SetPower(requiredPower);
            projectileShooter.SetAngle(requiredAngles.y);

            RenderPath(projectileShooter, Mathf.Min(origin.x, impactX), Mathf.Max(origin.x, impactX), _origin, direction);
        }

        private void RenderPath(ProjectileShooter _projectileShooter, float _x0, float _x1, Vector3 _origin, Vector3 _direction)
        {
            GameObject ballContainer = new GameObject();
            ballContainer.name = "BALL_CONTAINER";
            Destroy(ballContainer, 10);
            Vector2 originRender = _projectileShooter.GetOrigin();
            double y0 = _projectileShooter.ComputeY(Mathf.Abs(originRender.x - _x0));
            for (float x = _x0; x <= _x1; x+=1)
            {
                float y = _projectileShooter.ComputeY(Mathf.Abs(originRender.x - x));

                Vector3 currentPosition = _origin + (_direction * x);
                currentPosition = new Vector3(currentPosition.x, y + _origin.y, currentPosition.z);
                GameObject ballPath = GameObject.Instantiate(BallReference, ballContainer.transform);
                ballPath.transform.position = currentPosition;
            }
        }
    }

}