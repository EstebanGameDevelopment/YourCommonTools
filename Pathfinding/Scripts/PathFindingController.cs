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
        private bool m_isPrecalculated = false;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	
        public bool IsPrecalculated
        {
            get { return m_isPrecalculated; }
        }

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
		 * Set the height of the waypoints
		 */
        public void SetWaypointHeight(float _waypointHeight, int _layer = -1)
        {
            if (_layer == -1)
            {
                m_pathfindingInstances[m_pathfindingInstances.Count - 1].WaypointHeight = _waypointHeight;
            }
            else
            {
                m_pathfindingInstances[_layer].WaypointHeight = _waypointHeight;
            }
        }

        // ---------------------------------------------------
        /**
		 * Get the content of the cell in the asked position
		 */
        public bool CheckOutsideBoard(float _x, float _y, float _z, int _layer = -1)
        {
            if (_layer == -1)
            {
                return m_pathfindingInstances[m_pathfindingInstances.Count - 1].CheckOutsideBoard(_x, _y, _z);
            }
            else
            {
                return m_pathfindingInstances[_layer].CheckOutsideBoard(_x, _y, _z);
            }
        }        

        // ---------------------------------------------------
        /**
		 * Get the cell of the current position
		 */
        public Vector3 GetCellPositionInMatrix(float _x, float _y, float _z, int _layer = -1)
        {
            if (_layer == -1)
            {
                return m_pathfindingInstances[m_pathfindingInstances.Count - 1].GetCellPositionInMatrix(_x, _y, _z);
            }
            else
            {
                return m_pathfindingInstances[_layer].GetCellPositionInMatrix(_x, _y, _z);
            }
        }

        // ---------------------------------------------------
        /**
		 * Get the content of the cell in the asked position
		 */
        public int GetCellContentByRealPosition(float _x, float _y, float _z, int _layer = -1)
        {
            if (_layer == -1)
            {
                return m_pathfindingInstances[m_pathfindingInstances.Count - 1].GetCellContentByRealPosition(_x, _y, _z);
            }
            else
            {
                return m_pathfindingInstances[_layer].GetCellContentByRealPosition(_x, _y, _z);
            }
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
		 * CreateSingleDot
		 */
        public GameObject CreateSingleDot(Vector3 _position, float _size, int _type, int _layer = -1)
        {
            if (_layer == -1)
            {
                return m_pathfindingInstances[m_pathfindingInstances.Count - 1].CreateSingleDot(_position, _size, _type);
            }
            else
            {
                return m_pathfindingInstances[_layer].CreateSingleDot(_position, _size, _type);
            }
        }


        // ---------------------------------------------------
        /**
		 * RenderDebugMatrixConstruction
		 */
        public void RenderDebugMatrixConstruction(int _layer = -1)
        {
            if (DEBUG_MATRIX_CONSTRUCTION)
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
        public Vector3 GetPath(Vector3 _origin,
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
        public Vector3 GetPathLayer(int _layer,
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

        // ---------------------------------------------------
        /**
		* Check if the position is a free one
		*/
        public Vector3 IsPositionInFreeNode(Vector3 _position, int _layer = -1)
        {
            if (_layer == -1)
            {
                return m_pathfindingInstances[m_pathfindingInstances.Count - 1].IsPositionInFreeNode(_position);
            }
            else
            {
                return m_pathfindingInstances[_layer].IsPositionInFreeNode(_position);
            }
        }

        // ---------------------------------------------------
        /**
		* Gets the path between 2 positions
		*/
        public Vector3 GetClosestFreeNode(Vector3 _position, int _layer = -1)
        {
            if (_layer == -1)
            {
                return m_pathfindingInstances[m_pathfindingInstances.Count - 1].GetClosestFreeNode(_position);
            }
            else
            {
                return m_pathfindingInstances[_layer].GetClosestFreeNode(_position);
            }
        }

        // ---------------------------------------------------
        /**
		* Gets the size of the cell
		*/
        public float GetCellSize(int _layer = -1)
        {
            if (_layer == -1)
            {
                return m_pathfindingInstances[m_pathfindingInstances.Count - 1].CellSize;
            }
            else
            {
                return m_pathfindingInstances[_layer].CellSize;
            }
        }


        // ---------------------------------------------------
        /**
		* GetRandomFreeCellBorder
		*/
        public Vector3 GetRandomFreeCellBorder(int _layer = -1)
        {
            if (_layer == -1)
            {
                return m_pathfindingInstances[m_pathfindingInstances.Count - 1].GetRandomFreeCellBorder();
            }
            else
            {
                return m_pathfindingInstances[_layer].GetRandomFreeCellBorder();
            }
        }

        // ---------------------------------------------------
        /**
		* CheckOutsideBoard
		*/
        public bool CheckOutsideBoard(Vector3 _position, int _layer = -1)
        {
            if (_layer == -1)
            {
                return m_pathfindingInstances[m_pathfindingInstances.Count - 1].CheckOutsideBoard(_position.x, _position.y, _position.z);
            }
            else
            {
                return m_pathfindingInstances[_layer].CheckOutsideBoard(_position.x, _position.y, _position.z);
            }
        }

        // ---------------------------------------------------
        /**
		* Precalculate all the paths
		*/
        public void CalculateAll(string _filenamePath, int _layer = -1, bool _raycastFilter = false, params string[] _masksToIgnore)
        {
            if (_layer == -1)
            {
                m_pathfindingInstances[m_pathfindingInstances.Count - 1].CalculateAll(_filenamePath, _raycastFilter, _masksToIgnore);
            }
            else
            {
                m_pathfindingInstances[_layer].CalculateAll(_filenamePath, _raycastFilter, _masksToIgnore);
            }
        }

        // ---------------------------------------------------
        /**
		 * Load data of pathfinding
		*/
        public void LoadFile(string _filenamePath, int _layer = -1)
        {
            m_isPrecalculated = true;
            if (_layer == -1)
            {
                m_pathfindingInstances[m_pathfindingInstances.Count - 1].LoadFile(_filenamePath);
            }
            else
            {
                m_pathfindingInstances[_layer].LoadFile(_filenamePath);
            }
        }

        // ---------------------------------------------------
        /**
		 * Load data of pathfinding
		*/
        public void LoadAsset(TextAsset _textAsset, int _layer = -1)
        {
            m_isPrecalculated = true;
            if (_layer == -1)
            {
                m_pathfindingInstances[m_pathfindingInstances.Count - 1].LoadAsset(_textAsset);
            }
            else
            {
                m_pathfindingInstances[_layer].LoadAsset(_textAsset);
            }
        }
    }
}