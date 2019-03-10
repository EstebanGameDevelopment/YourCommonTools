using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace YourCommonTools
{

    /******************************************
	* 
	* PathFindingController
	* 
	* Run A* to search a path between to cells of a matrix
	* 
	* @author Esteban Gallardo
	*/
    public class PathFindingController : MonoBehaviour
    {
        // ----------------------------------------------
        // PUBLIC CONSTANTS
        // ----------------------------------------------
        public const bool DEBUG_MATRIX_CONSTRUCTION = false;
        public const bool DEBUG_PATHFINDING = false;
        public const bool DEBUG_DOTPATHS = false;

        public const string TAG_FLOOR = "FLOOR";
        public const string TAG_PATH = "PATH";

        // CELLS
        public const int CELL_EMPTY = 0;
        public const int CELL_COLLISION = 1;

        // CONSTANTS DIRECTIONS
        public const int DIRECTION_LEFT = 1;
        public const int DIRECTION_RIGHT = 2;
        public const int DIRECTION_UP = 100;
        public const int DIRECTION_DOWN = 200;
        public const int DIRECTION_NONE = -1;

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static PathFindingController _instance;

        public static PathFindingController Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType(typeof(PathFindingController)) as PathFindingController;
                }
                return _instance;
            }
        }

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------
        public GameObject PathFindingPrefab;
        public GameObject DotReference;
        public GameObject DotReferenceEmtpy;
        public GameObject DotReferenceWay;

        public bool DebugPathPoints;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private List<PathFindingInstance> m_pathfindingInstances = new List<PathFindingInstance>();

        // ---------------------------------------------------
        /**
		 * Constructor of cPathFinding
		 */
        public void Initialize()
        {
        }

        // ---------------------------------------------------
        /**
		 * Destroy
		 */
        public void Destroy()
        {
            if (_instance == null) return;
            _instance = null;

            // ClearDotPaths();
        }

        // ---------------------------------------------------
        /**
		 * Will initialize the structure to be able to use it
		 */
        public void AllocateMemoryMatrix(int _cols,
                                        int _rows,
                                        int _layers,
                                        float _cellSize,
                                        float _xIni,
                                        float _yIni,
                                        float _zIni,
                                        int[][][] _initContent = null)
        {
            GameObject newPathfindingInstance = Instantiate(PathFindingPrefab);
            newPathfindingInstance.GetComponent<PathFindingInstance>().AllocateMemoryMatrix(_cols, _rows, _layers, _cellSize, _xIni, _yIni, _zIni, _initContent);
            m_pathfindingInstances.Add(newPathfindingInstance.GetComponent<PathFindingInstance>());
        }

        // ---------------------------------------------------
        /**
		 * Will dynamically calculate the collisions
		 */
        public void CalculateCollisions(int _layerToCheck = 0, params string[] _layersToIgnore)
        {
            foreach (PathFindingInstance pathInstance in m_pathfindingInstances)
            {
                pathInstance.CalculateCollisions(_layerToCheck, _layersToIgnore);
            }
        }

        // ---------------------------------------------------
        /**
		 * ClearDotPaths
		 */
        public void ClearDotPaths()
        {
            foreach (PathFindingInstance pathInstance in m_pathfindingInstances)
            {
                pathInstance.ClearDotPaths();
            }
        }

        // ---------------------------------------------------
        /**
		 * RenderDebugMatrixConstruction
		 */
        public void RenderDebugMatrixConstruction(int _layer = -1)
        {
            if (_layer == -1)
            {
                // RENDER ALL LAYERS
                for (int i = 0; i < m_pathfindingInstances.Count; i++)
                {
                    m_pathfindingInstances[i].RenderDebugMatrixConstruction(0, m_pathfindingInstances.Count - 1 - i);
                }
            }
            else
            {
                m_pathfindingInstances[_layer].RenderDebugMatrixConstruction(_layer);
            }
        }

        // ---------------------------------------------------
        /**
		 * CheckBlockedPath
		 */
        public bool CheckBlockedPath(Vector3 _origin, Vector3 _target, float _dotSize = 3, params string[] _masksToIgnore)
        {
            return (Utilities.GetCollidedObjectBySegmentTargetIgnore(_target, _origin, _masksToIgnore));
        }

        // ---------------------------------------------------
        /**
		* Gets the path between 2 positions
		*/
        public int GetPath(Vector3 _origin,
                                Vector3 _destination,
                                List<Vector3> _waypoints,
                                bool _oneLayer,
                                bool _raycastFilter,
                                int _limitSearch = -1,
                                params string[] _masksToIgnore)
        {
            // USE THE LAST PATH
            return m_pathfindingInstances[m_pathfindingInstances.Count - 1].GetPath(_origin, _destination, _waypoints, _oneLayer, _raycastFilter, _limitSearch, _masksToIgnore);
        }

        // ---------------------------------------------------
        /**
		* Gets the path between 2 positions
		*/
        public int GetPathLayer(int _layer,
                                Vector3 _origin,
                                Vector3 _destination,
                                List<Vector3> _waypoints,
                                bool _oneLayer,
                                bool _raycastFilter,
                                int _limitSearch = -1,
                                params string[] _masksToIgnore)
        {
            return m_pathfindingInstances[_layer].GetPath(_origin, _destination, _waypoints, _oneLayer, _raycastFilter, _limitSearch, _masksToIgnore);
        }

    }
}