using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace YourCommonTools
{

    /******************************************
	* 
	* PathFindingInstance
	* 
	* Run A* to search a path between to cells of a matrix
	* 
	* @author Esteban Gallardo
	*/
    public class PathFindingInstance : MonoBehaviour
	{
        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private int m_cols;                     //! Cols of the matrix
		private int m_rows;                     //! Rows of the matrix
		private int m_layers;                   //! Height of the matrix
		private int m_totalCells;               //! Total number of cells
		private float m_cellSize;               //! Size of the cell
		private float m_xIni;                   //! Initial shift X
		private float m_yIni;                   //! Initial shift Y
		private float m_zIni;                   //! Initial shift Z

		private int m_sizeMatrix;
		private int m_numCellsGenerated;

		private int[] m_floor;

		private int[][] m_cells;                //! List of cells to apply the pathfinding

		private List<NodePathMatrix> m_matrixAI;
		private List<GameObject> m_dotPaths = new List<GameObject>();

        private PrecalculatedData m_vectorPaths;

        // ----------------------------------------------
        // SETTERS/GETTERS
        // ----------------------------------------------	
        public int Cols
		{
			get { return m_cols; }
			set { m_cols = value; }
		}
		public int Rows
		{
			get { return m_rows; }
			set { m_rows = value; }
		}
		public int Height
		{
			get { return m_layers; }
			set { m_layers = value; }
		}
		public int TotalCells
		{
			get { return m_totalCells; }
			set { m_totalCells = value; }
		}
		public float CellSize
		{
			get { return m_cellSize; }
			set { m_cellSize = value; }
		}
		public float xIni
		{
			get { return m_xIni; }
			set { m_xIni = value; }
		}
		public float yIni
		{
			get { return m_yIni; }
			set { m_yIni = value; }
		}
		public float zIni
		{
			get { return m_zIni; }
			set { m_zIni = value; }
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
		 * ClearDotPaths
		 */
		public void ClearDotPaths()
		{
			foreach (GameObject dot in m_dotPaths)
			{
				Destroy(dot);
			}
			m_dotPaths.Clear();
		}

		// ---------------------------------------------------
		/**
		 * Will clear the allocated memory
		 */
		public void ClearMemoryAllocated()
		{
			if (m_matrixAI != null) m_matrixAI.Clear();
		}

		// ---------------------------------------------------
		/**
		 * Destroy
		 */
		public void Destroy()
		{
			ClearDotPaths();
		}

		// ---------------------------------------------------
		/**
		 * CreateDotPath
		 */
		private void CreateDotPath(Vector3 _position, int _totalDots)
		{
			if (PathFindingController.Instance.DebugPathPoints)
			{
				GameObject newdot = (GameObject)Instantiate(PathFindingController.Instance.DotReferenceWay, _position, new Quaternion());
                // float cellSize = (m_cellSize / 3) + (1.2f * (float)(m_dotPaths.Count + 1) / (float)_totalDots);
                float cellSize = (m_cellSize / 2);
                newdot.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
				m_dotPaths.Add(newdot);
			}
		}

        // ---------------------------------------------------
        /**
		 * CreateSingleDot
		 */
        public GameObject CreateSingleDot(Vector3 _position, float _size, int _type)
        {
            GameObject prefabDot = PathFindingController.Instance.DotReferenceWay;
            switch (_type)
            {
                case 1:
                    prefabDot = PathFindingController.Instance.DotReference;
                    break;

                case 2:
                    prefabDot = PathFindingController.Instance.DotReferenceEmtpy;
                    break;

                default:
                    prefabDot = PathFindingController.Instance.DotReferenceWay;
                    break;
            }

            GameObject newdot = (GameObject)Instantiate(prefabDot, _position, new Quaternion());
            newdot.transform.localScale = new Vector3(_size, _size, _size);
            return newdot;
        }

        // ---------------------------------------------------
        /**
		 * CreateDot
		 */
        private void CreateDot(Vector3 _position, bool _enableRenderer = true, float _scaleSize = 3)
        {
            GameObject newdot = (GameObject)Instantiate(PathFindingController.Instance.DotReferenceWay, _position, new Quaternion());
            if (!_enableRenderer)
            {
                newdot.GetComponent<Renderer>().enabled = true;
            }
            float cellSize = (m_cellSize / _scaleSize);
            newdot.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
            m_dotPaths.Add(newdot);
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
			m_cols = _rows;
			m_rows = _cols;
			m_layers = _layers;
			m_totalCells = m_cols * m_rows * m_layers;
			m_cellSize = _cellSize;
			m_xIni = _xIni;
			m_yIni = _yIni;
			m_zIni = _zIni;

			// INIT
			m_cells = new int[m_layers][];
			for (int z = 0; z < m_layers; z++)
			{
				m_cells[z] = new int[m_totalCells];
				for (int x = 0; x < m_rows; x++)
				{
					for (int y = 0; y < m_cols; y++)
					{
						m_cells[z][(x * m_cols) + y] = PathFindingController.CELL_EMPTY;
					}
				}
			}

            // COLLISION
            if (_initContent != null)
            {
                for (int z = 0; z < m_layers; z++)
                {
                    for (int x = 0; x < m_rows; x++)
                    {
                        for (int y = 0; y < m_cols; y++)
                        {
                            int cellContent = _initContent[z][x][y];
                            m_cells[z][(x * m_cols) + y] = ((cellContent != 0) ? PathFindingController.CELL_COLLISION : PathFindingController.CELL_EMPTY);
                        }
                    }
                }
            }

            m_matrixAI = new List<NodePathMatrix>();
			for (int i = 0; i < m_totalCells; i++)
			{
				m_matrixAI.Add(new NodePathMatrix());
			}

			if (PathFindingController.DEBUG_MATRIX_CONSTRUCTION)
			{
                if (_initContent == null)
                {
                    RenderDebugMatrixConstruction(0);
                }				
			}
		}


		// ---------------------------------------------------
		/**
		 * SetContentCollisionCell
		*/
		public void SetContentCollisionCell(Vector3 _posMatrix, int _content)
		{
			m_cells[(int)_posMatrix.z][(int)((_posMatrix.x * m_cols) + _posMatrix.y)] = _content;
		}

		// ---------------------------------------------------
		/**
		 * SetContentCollisionFloor
		*/
		public void SetContentCollisionFloor(Vector3 _posMatrix, int _content)
		{
			m_floor[(int)((_posMatrix.x * m_cols) + _posMatrix.y)] = _content;
        }

		// ---------------------------------------------------
		/**
		 * Render an sphere int the empty cells to check if the matrix was build right
		 */
		public void RenderDebugMatrixConstruction(int _layerToCheck = 0, int _heightLayer = -1)
		{
			if (m_dotPaths.Count > 0) return;

			for (int x = 0; x < m_rows; x++)
			{
				for (int y = 0; y < m_cols; y++)
				{
					int cellContent = m_cells[_layerToCheck][(x * m_cols) + y];
                    float finalHeightDot = 0f * m_cellSize + m_yIni;
                    if (_heightLayer != -1)
                    {
                        finalHeightDot += (_heightLayer * (m_cellSize * 2));
                    }
                    Vector3 pos = new Vector3((float)((x * m_cellSize)) + (m_cellSize / 2) + m_xIni, finalHeightDot, (float)((y * m_cellSize)) + (m_cellSize / 2) + m_zIni);
                    GameObject newdot;
                    if (cellContent == PathFindingController.CELL_EMPTY)
					{
						newdot = (GameObject)Instantiate(PathFindingController.Instance.DotReferenceEmtpy);
					}
                    else
                    {
                        newdot = (GameObject)Instantiate(PathFindingController.Instance.DotReference);
                    }
                    // newdot.transform.localScale = new Vector3(m_cellSize / 4, m_cellSize / 4, m_cellSize / 4);
                    newdot.transform.localScale = new Vector3(m_cellSize / 2, m_cellSize / 2, m_cellSize / 2);
                    newdot.transform.position = pos;
                    m_dotPaths.Add(newdot);
                }
			}
		}

        // ---------------------------------------------------
        /**
		 * Will dynamically calculate the collisions
		 */
        public void CalculateCollisions(int _layerToCheck = 0, params string[] _layersToIgnore)
        {
            m_cells = new int[m_layers][];
            for (int z = 0; z < m_layers; z++)
            {
                m_cells[z] = new int[m_totalCells];
                for (int x = 0; x < m_rows; x++)
                {
                    for (int y = 0; y < m_cols; y++)
                    {
                        m_cells[z][(x * m_cols) + y] = PathFindingController.CELL_EMPTY;
                    }
                }
            }

            for (int x = 0; x < m_rows; x++)
            {
                for (int y = 0; y < m_cols; y++)
                {
                    int cellContent = m_cells[_layerToCheck][(x * m_cols) + y];
                    Vector3 posAir = new Vector3((float)((x * m_cellSize)) + (m_cellSize / 2) + m_xIni, 1000, (float)((y * m_cellSize)) + (m_cellSize / 2) + m_zIni);
                    Vector3 posAir1 = new Vector3(posAir.x - (m_cellSize / 3), posAir.y, posAir.z - (m_cellSize / 3));
                    Vector3 posAir2 = new Vector3(posAir.x + (m_cellSize / 3), posAir.y, posAir.z - (m_cellSize / 3));
                    Vector3 posAir3 = new Vector3(posAir.x - (m_cellSize / 3), posAir.y, posAir.z + (m_cellSize / 3));
                    Vector3 posAir4 = new Vector3(posAir.x + (m_cellSize / 3), posAir.y, posAir.z + (m_cellSize / 3));

                    
                    RaycastHit raycastHit = new RaycastHit();
                    if (Utilities.GetRaycastHitInfoByRay(posAir1, new Vector3(0, -1, 0), ref raycastHit, _layersToIgnore))
                    {
                        if (raycastHit.collider.gameObject.tag != PathFindingController.TAG_FLOOR)
                        {
                            m_cells[_layerToCheck][(x * m_cols) + y] = PathFindingController.CELL_COLLISION;
                        }
                    }
                    if (Utilities.GetRaycastHitInfoByRay(posAir2, new Vector3(0, -1, 0), ref raycastHit, _layersToIgnore))
                    {
                        if (raycastHit.collider.gameObject.tag != PathFindingController.TAG_FLOOR)
                        {
                            m_cells[_layerToCheck][(x * m_cols) + y] = PathFindingController.CELL_COLLISION;
                        }
                    }
                    if (Utilities.GetRaycastHitInfoByRay(posAir3, new Vector3(0, -1, 0), ref raycastHit, _layersToIgnore))
                    {
                        if (raycastHit.collider.gameObject.tag != PathFindingController.TAG_FLOOR)
                        {
                            m_cells[_layerToCheck][(x * m_cols) + y] = PathFindingController.CELL_COLLISION;
                        }
                    }
                    if (Utilities.GetRaycastHitInfoByRay(posAir4, new Vector3(0, -1, 0), ref raycastHit, _layersToIgnore))
                    {
                        if (raycastHit.collider.gameObject.tag != PathFindingController.TAG_FLOOR)
                        {
                            m_cells[_layerToCheck][(x * m_cols) + y] = PathFindingController.CELL_COLLISION;
                        }
                    }
                }
            }
        }

        // ---------------------------------------------------
        /**
		 * Gets the direction to go from two points
		*/
        private int GetDirectionByPosition(int _xOrigin, int _yOrigin, int _xDestination, int _yDestination)
		{
			if (_yOrigin > _yDestination) return (PathFindingController.DIRECTION_UP);
			if (_yOrigin < _yDestination) return (PathFindingController.DIRECTION_DOWN);
			if (_xOrigin < _xDestination) return (PathFindingController.DIRECTION_RIGHT);
			if (_xOrigin > _xDestination) return (PathFindingController.DIRECTION_LEFT);

			return (PathFindingController.DIRECTION_NONE);
		}

		// ---------------------------------------------------
		/**
		 * Get the content of the cell in the asked position
		 */
		public bool CheckOutsideBoard(float _x, float _y, float _z)
		{
			int x = (int)((_x + (0.0f * m_cellSize)) / m_cellSize);
			int z = (int)((_z + (0.0f * m_cellSize)) / m_cellSize);
			if (z < 0) return true;
			if (x < 0) return true;
			if (x >= m_rows) return true;
			if (z >= m_cols) return true;
			return false;
		}

        // ---------------------------------------------------
        /**
		 * Get the cell of the current position
		 */
        public Vector3 GetCellPositionInMatrix(float _x, float _y, float _z)
        {
            int x = (int)((_x - m_xIni) / m_cellSize);
            int z = (int)((_z - m_zIni) / m_cellSize);
            return new Vector3(x, z, 0);
        }

        // ---------------------------------------------------
        /**
		 * Get the content of the cell in the asked position
		 */
        public int GetCellContentByRealPosition(float _x, float _y, float _z)
		{
			int x = (int)((_x - m_xIni) / m_cellSize);
			int z = (int)((_z - m_zIni) / m_cellSize);
			return GetCellContent(x, z, 0);
		}

		// ---------------------------------------------------
		/**
		 * Get the content of the cell in the asked position
		 */
		public int GetCellContent(int _x, int _y, int _z)
		{
			if (_y < 0) return PathFindingController.CELL_COLLISION;
			if (_x < 0) return PathFindingController.CELL_COLLISION;
			if (_z < 0) return PathFindingController.CELL_COLLISION;
			if (_y >= m_cols) return PathFindingController.CELL_COLLISION;
			if (_x >= m_rows) return PathFindingController.CELL_COLLISION;
			if (_z >= m_layers) return PathFindingController.CELL_COLLISION;
			return (int)(m_cells[_z][(_x * m_cols) + _y]);
		}

		// ---------------------------------------------------
		/**
		 * Get the content of the cell in the asked position
		 */
		public bool OutOfBoundaries(int _x, int _y, int _z)
		{
			if (_y < 0) return true;
			if (_x < 0) return true;
			if (_z < 0) return true;
			if (_y >= m_cols) return true;
			if (_x >= m_rows) return true;
			if (_z >= m_layers) return true;
			return false;
		}

		// ---------------------------------------------------
		/**
		 * Distance between two points
		*/
		private float GetDistance(int _xOrigin, int _yOrigin, int _zOrigin, int _xDestination, int _yDestination, int _zDestination)
		{
			return (Mathf.Abs(_xOrigin - _xDestination) + Math.Abs(_yOrigin - _yDestination) + Math.Abs(_zOrigin - _zDestination));
		}

		// ---------------------------------------------------
		/**
		* CheckCollidedContent
		*/
		public bool CheckCollidedContent(int _content)
		{
			return (_content != PathFindingController.CELL_EMPTY);
		}

		// ---------------------------------------------------
		/*
		 * GetHops
		*/
		private int GetHops(int _current)
		{
			int curIndexBack = _current;
			int hops = 0;
			do
			{
				curIndexBack = m_matrixAI[curIndexBack].PreviousCell;
				hops++;
			} while ((curIndexBack != 0) && (curIndexBack != -1));
			return hops;
		}

        // ---------------------------------------------------
        /**
		* Check if the position is in a free postion
		*/
        public Vector3 IsPositionInFreeNode(Vector3 _position)
        {
            Vector3 basePosition = GetCellPositionInMatrix(_position.x, _position.y, _position.z);
            if (GetCellContent((int)basePosition.x, (int)basePosition.y, 0) == PathFindingController.CELL_EMPTY)
            {
                return new Vector3((basePosition.x * m_cellSize) + m_xIni, (m_cellSize / 10), (basePosition.y * m_cellSize) + m_zIni);
            }
            else
            {
                return Vector3.down;
            }
        }

        // ---------------------------------------------------
        /**
        * Get the closest free node to a position
        */
        public Vector3 GetClosestFreeNode(Vector3 _position)
        {
            Vector3 basePosition = GetCellPositionInMatrix(_position.x, _position.y, _position.z);
            if (GetCellContent((int)basePosition.x, (int)basePosition.y, 0) == PathFindingController.CELL_EMPTY)
            {
                return new Vector3((basePosition.x * m_cellSize) + m_xIni, (m_cellSize / 10), (basePosition.y * m_cellSize) + m_zIni);
            }
            else
            {
                Vector3 closestFreeCell = Utilities.Clone(Vector3.down);
                float minimumDistance = 1000000f;
                for (int i = (int)basePosition.x - 5; i < (int)basePosition.x + 5; i++)
                {
                    for (int j = (int)basePosition.y - 5; j < (int)basePosition.y + 5; j++)
                    {
                        if (!OutOfBoundaries(i,j,0))
                        {
                            if (GetCellContent(i, j, 0) == PathFindingController.CELL_EMPTY)
                            {
                                Vector3 currentPosition = new Vector3((i * m_cellSize) + m_xIni, (m_cellSize / 10), (j * m_cellSize) + m_zIni);
                                float currentDistance = Vector3.Distance(_position, currentPosition);
                                if (currentDistance < minimumDistance)
                                {
                                    minimumDistance = currentDistance;
                                    closestFreeCell = Utilities.Clone(currentPosition);
                                }
                            }
                        }
                    }
                }
                return closestFreeCell;
            }
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
            Vector3 origin = new Vector3();
            origin.x = (int)((_origin.x - m_xIni) / m_cellSize);
            origin.y = (int)((_origin.z - m_zIni) / m_cellSize);
            origin.z = (_oneLayer?0:((int)((_origin.y - m_yIni) / m_cellSize)));

            Vector3 destination = new Vector3();
            destination.x = (int)((_destination.x - m_xIni) / m_cellSize);
            destination.y = (int)((_destination.z - m_zIni) / m_cellSize);
            destination.z = (_oneLayer ? 0 : ((int)((_destination.y - m_yIni) / m_cellSize)));

            // Debug.LogError("GetPath::origin[" + origin.ToString() + "]::destination[" + destination.ToString() + "]");

            int limitSearch = ((_limitSearch == -1) ? m_totalCells - 5 : _limitSearch);

            if (!m_hasBeenFileLoaded)
            {
                return SearchAStar(origin, destination, _origin, _destination, _waypoints, _oneLayer, limitSearch, _raycastFilter, _masksToIgnore);
            }
            else
            {
                int cellOrigin = (int)((origin.x * m_cols) + origin.y);
                int cellDestination = (int)((destination.x * m_cols) + destination.y);
                if (m_vectorPaths.Data[cellOrigin] == null) return Vector3.zero;
                if (m_vectorPaths.Data[cellOrigin][cellDestination] == null) return Vector3.zero;
                return m_vectorPaths.Data[cellOrigin][cellDestination].GetVector3();
            }
        }

        // ---------------------------------------------------
        /**
		* Do the search A* in the matrix to search a type or a position
		* @param x_ori	Initial position X
		* @param y_ori	Initial position Y
		* @param x_des	Final position X
		* @param y_des	Final position Y
		*/
        public Vector3 SearchAStar(Vector3 _origin,
								Vector3 _destination,
                                Vector3 _realOrigin,
                                Vector3 _realDestination,
                                List<Vector3> _waypoints,
								bool _oneLayer,
                                int _limitSearch,
                                bool _raycastFilter = false,
                                params string[] _masksToIgnore)
		{
			int i;
			int j;
			float minimalValue;
			int currentNodeEvaluated;

			m_sizeMatrix = 0;
			m_numCellsGenerated = 0;

			if (PathFindingController.DEBUG_PATHFINDING)
			{
				Debug.Log("cPathFinding.as::SearchAStar:: ORIGIN(" + _origin.x + "," + _origin.y + "," + _origin.z + "); DESTINATION(" + _destination.x + "," + _origin.y + "," + _origin.z + "); COLUMNS=" + m_cols + ";ROWS=" + m_rows + ";HEIGHT=" + m_layers);
				Debug.Log("CONTENT=" + m_cells);
			}

			// SAME POSITION
			if ((_origin.x == _destination.x) && (_origin.y == _destination.y) && (_origin.z == _destination.z))
			{
				return Vector3.zero;
			}

			// RESET MATRIX
			for (i = 0; i < m_totalCells; i++)
			{
				m_matrixAI[i].Reset();
			}

			// INITIALIZE FIRST POSITION
			m_sizeMatrix = 0;
			m_matrixAI[m_sizeMatrix].X = (int)_origin.x;
			m_matrixAI[m_sizeMatrix].Y = (int)_origin.y;
			m_matrixAI[m_sizeMatrix].Z = (int)_origin.z;
			m_matrixAI[m_sizeMatrix].HasBeenVisited = NodePathMatrix.NODE_VISITED;
			m_matrixAI[m_sizeMatrix].DirectionInitial = PathFindingController.DIRECTION_NONE;
			if ((_destination.x == -1) && (_destination.y == -1) && (_destination.z == -1))
			{
				m_matrixAI[m_sizeMatrix].ValueSearch = 0;
			}
			else
			{
				m_matrixAI[m_sizeMatrix].ValueSearch = GetDistance((int)_origin.x, (int)_origin.y, (int)_origin.z,
																(int)_destination.x, (int)_destination.y, (int)_destination.z);
			}
			m_matrixAI[m_sizeMatrix].PreviousCell = -1;

			// ++ START SEARCH ++
			i = 0;
			do
			{
				m_numCellsGenerated = 0;

				// CHECK OVERFLOW
				if (m_sizeMatrix > _limitSearch)
				{
                    if (PathFindingController.DEBUG_PATHFINDING) Debug.Log("cPathFinding.as::SearchAStar:: RETURN 1");
                    return Vector3.zero;
				}

				// ++ LOOK FOR THE FIRST BEST NODE TO CONTINUE ++
				minimalValue = 100000000;
				i = -1;
				for (j = 0; j <= m_sizeMatrix; j++)
				{
					if (m_matrixAI[j].HasBeenVisited == NodePathMatrix.NODE_VISITED) // CHECKED
					{
						if (m_matrixAI[j].ValueSearch < minimalValue)
						{
							i = j;
							minimalValue = m_matrixAI[j].ValueSearch;
						}
					}
				}

				if (i == -1)
				{
					if (PathFindingController.DEBUG_PATHFINDING) Debug.Log("cPathFinding.as::SearchAStar:: RETURN 2");
					return Vector3.zero;
				}

				// ++ SELECT NODE ++
				currentNodeEvaluated = i;
				if ((m_matrixAI[i].X == _destination.x) && (m_matrixAI[i].Y == _destination.y) && (m_matrixAI[i].Z == _destination.z))
				{
					// CREATE THE LIST OF CELLS BETWEEN DESTINATION-ORIGIN
					List<Vector3> way = new List<Vector3>();
					if (i == -1)
					{
						return Vector3.zero;
					}
					else
					{
						int curIndexBack = i;
						Vector3 sGoalNext = new Vector3(-1f, -1f, -1f);
						Vector3 sGoalCurrent = new Vector3(0f, 0f, 0f);
                        Vector3 pivotReference = Vector3.zero;
                        Vector3 currentChecked = Vector3.zero;
                        Vector3 previousChecked = Vector3.zero;
						do
						{

							sGoalCurrent.x = m_matrixAI[curIndexBack].X;
							sGoalCurrent.y = m_matrixAI[curIndexBack].Y;
							sGoalCurrent.z = m_matrixAI[curIndexBack].Z;

							sGoalNext.x = sGoalCurrent.x;
							sGoalNext.y = sGoalCurrent.y;
							sGoalNext.z = sGoalCurrent.z;

                            curIndexBack = m_matrixAI[curIndexBack].PreviousCell;

                            // INSERT WAYPOINT
                            if (!_raycastFilter)
                            {
                                if (_oneLayer)
                                {
                                    way.Insert(0, new Vector3((sGoalNext.x * m_cellSize) + m_xIni, (m_cellSize / 10), (sGoalNext.y * m_cellSize) + m_zIni));
                                }
                                else
                                {
                                    way.Insert(0, new Vector3((sGoalNext.x * m_cellSize) + m_xIni, sGoalNext.z - (m_cellSize / 10) + m_yIni, (sGoalNext.y * m_cellSize) + m_zIni));
                                }
                            }
                            else
                            {
                                if (_oneLayer)
                                {
                                    if (pivotReference == Vector3.zero)
                                    {
                                        // Debug.LogError("INSERT INITIAL POINT[" + sGoalNext.ToString() + "]");
                                        currentChecked = new Vector3(_realDestination.x, (m_cellSize / 10), _realDestination.z);
                                        way.Insert(0, currentChecked);
                                        pivotReference = Utilities.Clone(currentChecked);
                                    }
                                    else
                                    {
                                        Vector3 lastValidChecked = Utilities.Clone(previousChecked);
                                        previousChecked = Utilities.Clone(currentChecked);
                                        currentChecked = new Vector3((sGoalNext.x * m_cellSize) + m_xIni, (m_cellSize / 10), (sGoalNext.y * m_cellSize) + m_zIni);
                                        if ((curIndexBack == 0) || (curIndexBack == -1))
                                        {
                                            currentChecked = new Vector3(_realOrigin.x, (m_cellSize / 10), _realOrigin.z);
                                        }
                                        if (CheckBlockedPath(currentChecked, pivotReference, 3, _masksToIgnore))
                                        {
                                            // Debug.LogError("INSERT["+ currentChecked.ToString() + "] BECAUSE BLOCKED PATH");
                                            // way.Insert(0, previousChecked);
                                            // pivotReference = Utilities.Clone(previousChecked);
                                            way.Insert(0, lastValidChecked);
                                            pivotReference = Utilities.Clone(previousChecked);                                            
                                        }
                                    }
                                }
                                else
                                {
                                    way.Insert(0, new Vector3((sGoalNext.x * m_cellSize) + m_xIni, sGoalNext.z - (m_cellSize / 10) + m_yIni, (sGoalNext.y * m_cellSize) + m_zIni));
                                }
                            }

                            

						} while ((curIndexBack != 0) && (curIndexBack != -1));

						ClearDotPaths();

                        // DRAW DEBUG BALLS
                        if (_waypoints != null)
                        {
                            for (int o = 0; o < way.Count; o++)
                            {
                                Vector3 sway = way[o];
                                _waypoints.Add(new Vector3(sway.x, sway.y, sway.z));
                                CreateDotPath(sway, way.Count);
                            }
                        }

                        if (way.Count > 0)
                        {
                            return way[0];
                        }
                        else
                        {
                            return Vector3.zero;
                        }                        
                    }
				}

				// SET AS VISITED NODE
				m_matrixAI[currentNodeEvaluated].HasBeenVisited = 0;

				if (PathFindingController.DEBUG_PATHFINDING) Debug.Log("cPathFinding.as::SearchAStar::ANALIZING(" + m_matrixAI[i].X + "," + m_matrixAI[i].Y + "," + m_matrixAI[i].Z + ")");

				// CHILD UP
				ChildGeneration(i, currentNodeEvaluated, (int)(m_matrixAI[i].X), (int)(m_matrixAI[i].Y + 1), (int)(m_matrixAI[i].Z), (int)_destination.x, (int)_destination.y, (int)_destination.z, PathFindingController.DIRECTION_DOWN, _oneLayer);
				if (!_oneLayer) ChildGeneration(i, currentNodeEvaluated, (int)(m_matrixAI[i].X), (int)(m_matrixAI[i].Y + 1), (int)(m_matrixAI[i].Z - 1), (int)_destination.x, (int)_destination.y, (int)_destination.z, PathFindingController.DIRECTION_DOWN, _oneLayer);
				if (!_oneLayer) ChildGeneration(i, currentNodeEvaluated, (int)(m_matrixAI[i].X), (int)(m_matrixAI[i].Y + 1), (int)(m_matrixAI[i].Z + 1), (int)_destination.x, (int)_destination.y, (int)_destination.z, PathFindingController.DIRECTION_DOWN, _oneLayer);

				// Child DOWN
				ChildGeneration(i, currentNodeEvaluated, (int)(m_matrixAI[i].X), (int)(m_matrixAI[i].Y - 1), (int)(m_matrixAI[i].Z), (int)_destination.x, (int)_destination.y, (int)_destination.z, PathFindingController.DIRECTION_UP, _oneLayer);
				if (!_oneLayer) ChildGeneration(i, currentNodeEvaluated, (int)(m_matrixAI[i].X), (int)(m_matrixAI[i].Y - 1), (int)(m_matrixAI[i].Z - 1), (int)_destination.x, (int)_destination.y, (int)_destination.z, PathFindingController.DIRECTION_UP, _oneLayer);
				if (!_oneLayer) ChildGeneration(i, currentNodeEvaluated, (int)(m_matrixAI[i].X), (int)(m_matrixAI[i].Y - 1), (int)(m_matrixAI[i].Z + 1), (int)_destination.x, (int)_destination.y, (int)_destination.z, PathFindingController.DIRECTION_UP, _oneLayer);

				// Child LEFT
				ChildGeneration(i, currentNodeEvaluated, (int)(m_matrixAI[i].X - 1), (int)(m_matrixAI[i].Y), (int)(m_matrixAI[i].Z), (int)_destination.x, (int)_destination.y, (int)_destination.z, PathFindingController.DIRECTION_LEFT, _oneLayer);
				if (!_oneLayer) ChildGeneration(i, currentNodeEvaluated, (int)(m_matrixAI[i].X - 1), (int)(m_matrixAI[i].Y), (int)(m_matrixAI[i].Z - 1), (int)_destination.x, (int)_destination.y, (int)_destination.z, PathFindingController.DIRECTION_LEFT, _oneLayer);
				if (!_oneLayer) ChildGeneration(i, currentNodeEvaluated, (int)(m_matrixAI[i].X - 1), (int)(m_matrixAI[i].Y), (int)(m_matrixAI[i].Z + 1), (int)_destination.x, (int)_destination.y, (int)_destination.z, PathFindingController.DIRECTION_LEFT, _oneLayer);

				//  Child RIGHT
				ChildGeneration(i, currentNodeEvaluated, (int)(m_matrixAI[i].X + 1), (int)(m_matrixAI[i].Y), (int)(m_matrixAI[i].Z), (int)_destination.x, (int)_destination.y, (int)_destination.z, PathFindingController.DIRECTION_RIGHT, _oneLayer);
				if (!_oneLayer) ChildGeneration(i, currentNodeEvaluated, (int)(m_matrixAI[i].X + 1), (int)(m_matrixAI[i].Y), (int)(m_matrixAI[i].Z - 1), (int)_destination.x, (int)_destination.y, (int)_destination.z, PathFindingController.DIRECTION_RIGHT, _oneLayer);
				if (!_oneLayer) ChildGeneration(i, currentNodeEvaluated, (int)(m_matrixAI[i].X + 1), (int)(m_matrixAI[i].Y), (int)(m_matrixAI[i].Z + 1), (int)_destination.x, (int)_destination.y, (int)_destination.z, PathFindingController.DIRECTION_RIGHT, _oneLayer);

			} while (true);
		}

		// ---------------------------------------------------
		/**
		 * Test if the child generated is correct
		*/
		private int GetCorrectChild(int _xPosition, int _yPosition, int _zPosition, int _sizeMatrix, bool _oneLayer)
		{
			int sCell;
			int i;

			// Position outside the bounds
			if ((_xPosition < 0) || (_xPosition >= m_rows)) return (0);
			if ((_yPosition < 0) || (_yPosition >= m_cols)) return (0);
			if ((_zPosition < 0) || (_zPosition >= m_layers)) return (0);

			// Collision control
			sCell = GetCellContent(_xPosition, _yPosition, _zPosition);
			if (!CheckCollidedContent(sCell))
			{
				// Check if the cell has been evaluated
				for (i = 0; i <= _sizeMatrix; i++)
				{
					if ((m_matrixAI[i].X == _xPosition) && (m_matrixAI[i].Y == _yPosition) && (m_matrixAI[i].Z == _zPosition))
						return (0);
				}
				return 1;
			}
			return 0;
		}

		// ---------------------------------------------------
		/**
		 * Generation of a new child
		*/
		private void ChildGeneration(int _index,
									int _searched,
									int _xOrigin, int _yOrigin, int _zOrigin,
									int _xDestination, int _yDestination, int _zDestination,
									int _initialDirection,
									bool _oneLayer)
		{
			// Generation of Childs 
			int posx = _xOrigin;
			int posy = _yOrigin;
			int posz = _zOrigin;
			int directionInitial = -1;
			if (GetCorrectChild(posx, posy, posz, m_sizeMatrix, _oneLayer) == 1)
			{
				if (m_matrixAI[_searched].DirectionInitial == PathFindingController.DIRECTION_NONE)
				{
					directionInitial = _initialDirection;
				}
				else
				{
					directionInitial = m_matrixAI[_searched].DirectionInitial;
				}

				m_sizeMatrix++;
				m_matrixAI[m_sizeMatrix].X = posx;
				m_matrixAI[m_sizeMatrix].Y = posy;
				m_matrixAI[m_sizeMatrix].Z = posz;
				m_matrixAI[m_sizeMatrix].HasBeenVisited = NodePathMatrix.NODE_VISITED;
				m_matrixAI[m_sizeMatrix].DirectionInitial = directionInitial;
				if ((_xDestination == PathFindingController.DIRECTION_NONE) && (_yDestination == PathFindingController.DIRECTION_NONE) && (_zDestination == PathFindingController.DIRECTION_NONE))
				{
					m_matrixAI[m_sizeMatrix].ValueSearch = 0;
				}
				else
				{
                    // m_matrixAI[m_sizeMatrix].ValueSearch = GetDistance(posx, posy, posz, _xDestination, _yDestination, _zDestination);
                    m_matrixAI[m_sizeMatrix].ValueSearch = (float)GetHops(_index); // hops
                }
				m_matrixAI[m_sizeMatrix].PreviousCell = _index;
				m_numCellsGenerated++;
			}
		}

        private int m_xIterator = -1;
        private int m_yIterator = 0;
        private bool m_calculationCompleted = false;

        private int m_iIterator = -1;
        private int m_jIterator = 0;
        private bool m_iterationCompleted = true;

        private string m_filenamePath = "Assets/pathfinding.dat";
        private bool m_hasBeenFileLoaded = false;

        // ---------------------------------------------------
        /**
		 * Save data of pathfinding
		*/
        private void SavePathfindingData()
        {
            FileStream file;

            if (File.Exists(m_filenamePath)) file = File.OpenWrite(m_filenamePath);
            else file = File.Create(m_filenamePath);

            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, m_vectorPaths);
            file.Close();
        }

        // ---------------------------------------------------
        /**
		 * Load data of pathfinding
		*/
        public void LoadFile(string _filenamePath)
        {
            if (m_hasBeenFileLoaded) return;
            m_hasBeenFileLoaded = true;

            FileStream file;

            m_filenamePath = _filenamePath;

            if (File.Exists(m_filenamePath)) file = File.OpenRead(m_filenamePath);
            else
            {
                Debug.LogError("File not found");
                return;
            }

            BinaryFormatter bf = new BinaryFormatter();
            m_vectorPaths = (PrecalculatedData)bf.Deserialize(file);
            // m_vectorPaths.DebugLog();
            file.Close();
        }

        // ---------------------------------------------------
        /**
		 * Generation of a new child
		*/
        public void CalculateAll(string _filenamePath, bool _raycastFilter = false, params string[] _masksToIgnore)
        {
            if (m_calculationCompleted) return;

            m_filenamePath = _filenamePath;

            if (m_xIterator == -1)
            {
                m_vectorPaths = new PrecalculatedData(new CustomVector3[m_rows * m_cols][]);
            }

            PathFindingController.Instance.DebugPathPoints = false;
            if (m_iterationCompleted)
            {
                m_iterationCompleted = false;
                m_xIterator++;
                if (m_xIterator >= m_rows)
                {
                    m_xIterator = 0;
                    m_yIterator++;
                    if (m_yIterator >= m_cols)
                    {
                        m_calculationCompleted = true;
                        Debug.LogError("*********************** CALCULATION COMPLETED ***************************");
                        SavePathfindingData();
                        return;
                    }
                }
                int originatorCell = (m_xIterator * m_cols) + m_yIterator;
                m_vectorPaths.Data[originatorCell] = new CustomVector3[m_rows * m_cols];
                Debug.LogError("++++++++++++ NEW CALCULATION[" + originatorCell + "/" + (m_rows * m_cols) + "]");
            }

            int originCell = (m_xIterator * m_cols) + m_yIterator;
            if (m_cells[0][originCell] != PathFindingController.CELL_EMPTY)
            {
                m_iterationCompleted = true;             
            }
            else
            {
                m_iIterator++;
                if (m_iIterator >= m_rows)
                {
                    m_iIterator = 0;
                    m_jIterator++;
                    if (m_jIterator >= m_cols)
                    {
                        m_iIterator = -1;
                        m_jIterator = 0;
                        m_iterationCompleted = true;
                        return;
                    }
                }
                int targetCell = (m_iIterator * m_cols) + m_jIterator;
                m_vectorPaths.Data[originCell][targetCell] = new CustomVector3();
                // Debug.LogError("PROGRESS [" + targetCell + "/" + (m_rows * m_cols) + "]");
                if (!((m_xIterator == m_iIterator) && (m_yIterator == m_jIterator)))
                {
                    if (m_cells[0][targetCell] == PathFindingController.CELL_EMPTY)
                    {
                        Vector3 origin = new Vector3(m_xIterator, m_yIterator, 0);
                        Vector3 destination = new Vector3(m_iIterator, m_jIterator, 0);
                        int limitSearch = m_totalCells - 1;                        
                        m_vectorPaths.Data[originCell][targetCell].SetVector3( SearchAStar(origin, destination, Vector3.zero, Vector3.one, null, true, limitSearch, _raycastFilter, _masksToIgnore));
                        // Debug.LogError("VALUE[" + m_vectorPaths.Data[originCell][targetCell].ToString() + "]");
                        // Debug.LogError("...");
                    }
                }
            }
        }
    }

}