using UnityEngine;
using System.Collections.Generic;
#if ENABLE_DRONECONTROLLER
using IronPython.Hosting;
using System.IO;
using Microsoft.Scripting.Hosting;
#endif

namespace YourCommonTools
{
	/******************************************
	 * 
	 * Runs a python script that allow to change the velocity vector
     * of the drone
	 * 
	 * @author Esteban Gallardo
	 */
	public class DroneKitPythonController : MonoBehaviour 
	{
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------
        public const string EVENT_DRONEKITCONTROLLER_START_VELOCITY     = "EVENT_DRONEKITCONTROLLER_START_VELOCITY";
        public const string EVENT_DRONEKITCONTROLLER_FINISHED_VELOCITY  = "EVENT_DRONEKITCONTROLLER_FINISHED_VELOCITY";

        // ----------------------------------------------
        // CONSTANTS
        // ----------------------------------------------
        public const string DRONEKIT_TAG_TOTALTIME = "$TOTALTIME";
        public const string DRONEKIT_TAG_TOTALSPEED = "$TOTALSPEED";
        public const string DRONEKIT_TAG_PARAMVX = "$PARAMVX";
        public const string DRONEKIT_TAG_PARAMVY = "$PARAMVY";
        public const string DRONEKIT_TAG_PARAMVZ = "$PARAMVZ";
        public const string DRONEKIT_TAG_RETURNHOME = "$RETURNHOME";

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------
        private static DroneKitPythonController _instance;

        public static DroneKitPythonController Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType<DroneKitPythonController>();
                }
                return _instance;
            }
        }

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------
        public TextAsset ScriptDroneKitVelocity;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------
#if ENABLE_DRONECONTROLLER
        private ScriptScope m_scope;
        private ScriptSource m_source;
#endif
        private string m_drokeKitScript;
        private float m_timeout = -1;
        private Vector3 m_nextVelocity = Vector3.zero;
        private bool m_returnHome = false;
        private int m_totalTime = 2;
        private int m_totalSpeed = 5;

        // -------------------------------------------
        /* 
		 * Start the drone with the desired vector velocity
		 */
        public void RunVelocity(float _vx, float _vy, float _vz, bool _returnHome = false, int _totalTime = 2, int _totalSpeed = 5)
		{
            if (m_timeout >= 0)
            {
                m_nextVelocity = new Vector3(_vx, _vy, _vz);
                m_returnHome = _returnHome;
                m_totalTime = _totalTime;
                m_totalSpeed = _totalSpeed;
                return;
            }

#if ENABLE_DRONECONTROLLER
            var engine = Python.CreateEngine();
            m_scope = engine.CreateScope();
            ICollection<string> sp = engine.GetSearchPaths();
            string path = Path.Combine(Path.GetDirectoryName(UnityEngine.Application.dataPath), "Assets/RemoteStreamController/Libraries/YourCommonTools/DroneKit/Resources");
            sp.Add(path);
            // path = Path.Combine(Path.GetDirectoryName(UnityEngine.Application.dataPath), "Assets/RemoteStreamController/Libraries/YourCommonTools/DroneKit/Resources/dronekit/util.py");
            engine.SetSearchPaths(sp);
            foreach (string item in sp)
            {
                Debug.LogError("+++++item=" + item);
            }
            engine.Runtime.LoadAssembly(System.Reflection.Assembly.GetAssembly(typeof(GameObject)));

            m_drokeKitScript = ScriptDroneKitVelocity.text;
            m_drokeKitScript = m_drokeKitScript.Replace(DRONEKIT_TAG_TOTALTIME, _totalTime.ToString());
            m_drokeKitScript = m_drokeKitScript.Replace(DRONEKIT_TAG_TOTALSPEED, _totalSpeed.ToString());
            m_drokeKitScript = m_drokeKitScript.Replace(DRONEKIT_TAG_PARAMVX, _vx.ToString());
            m_drokeKitScript = m_drokeKitScript.Replace(DRONEKIT_TAG_PARAMVY, _vz.ToString());
            m_drokeKitScript = m_drokeKitScript.Replace(DRONEKIT_TAG_PARAMVZ, _vy.ToString());
            m_drokeKitScript = m_drokeKitScript.Replace(DRONEKIT_TAG_RETURNHOME, (_returnHome?"1":"0"));

            m_timeout = 0;
            m_source = engine.CreateScriptSourceFromString(m_drokeKitScript);
            m_source.Execute(m_scope);

            BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_START_VELOCITY);
#endif
        }

        // -------------------------------------------
        /* 
		 * Check finished
		 */
        private void Update()
        {
#if ENABLE_DRONECONTROLLER
            if (m_timeout >= 0)
            {
                m_timeout += Time.deltaTime;
                if (m_timeout > 0.2f)
                {
                    m_timeout = 0;
                    if (m_scope.GetVariable<bool>("vehicleactivated"))
                    {
                        m_timeout = -1;
                        if ((m_nextVelocity != Vector3.zero) || m_returnHome)
                        {
                            RunVelocity(m_nextVelocity.x, m_nextVelocity.y, m_nextVelocity.z, m_returnHome, m_totalTime, m_totalSpeed);
                        }
                        else
                        {
                            BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_FINISHED_VELOCITY);
                        }                        
                    }
                }
            }
#endif
        }
    }
}