using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

namespace YourCommonTools
{
    /******************************************
	 * 
	 * Class with a collection of generic functionalities
	 * 
	 * @author Esteban Gallardo
	 */
    public static class Utilities
    {
		private static readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		// CONSTANTS DIRECTIONS
		public const int DIRECTION_LEFT = 1;
		public const int DIRECTION_RIGHT = 2;
		public const int DIRECTION_UP = 100;
		public const int DIRECTION_DOWN = 200;
		public const int DIRECTION_NONE = -1;

		public static string[] IgnoreLayersForDebug = new string[] { "UI" };

		// -------------------------------------------
		/* 
		 * Get distance between points in axis X,Z
		 */
		public static float DistanceXZ(Vector3 _positionA, Vector3 _positionB)
		{
			return Mathf.Abs(_positionA.x - _positionB.x) + Mathf.Abs(_positionA.z - _positionB.z);
		}

		// -------------------------------------------
		/* 
		 * Get distance between points in axis X,Z
		 */
		public static float DistanceSqrtXZ(Vector3 _positionA, Vector3 _positionB)
		{
			float x = Mathf.Abs(_positionA.x - _positionB.x);
			float y = Mathf.Abs(_positionA.z - _positionB.z);
			return Mathf.Sqrt((x * x) + (y * y));
		}

		// -------------------------------------------
		/* 
		 * Adds a child to the parent
		 */
		public static GameObject AddChild(Transform _parent, GameObject _prefab)
		{
			GameObject newObj = GameObject.Instantiate(_prefab);
			newObj.transform.SetParent(_parent, false);
			return newObj;
		}

        // -------------------------------------------
        /* 
		 * Attach a child to the parent
		 */
        public static GameObject AttachChild(Transform _parent, GameObject _prefab)
        {
            _prefab.transform.SetParent(_parent, false);
            return _prefab;
        }

        // -------------------------------------------
        /* 
		 * SetChild
		 */
        public static void SetChild(Transform _parent, GameObject _go)
		{
			_go.transform.SetParent(_parent, false);
		}

		// -------------------------------------------
		/* 
		 * Adds a sprite component to the object.
		 * It's used to create the visual selectors.
		 */
		public static Sprite AddSprite(GameObject _parent, Sprite _prefab, Rect _rect, Rect _rectTarget, Vector2 _pivot)
		{
			RectTransform newTransform = _parent.AddComponent<RectTransform>();
			_parent.AddComponent<CanvasRenderer>();
			Image srcImage = _parent.AddComponent<Image>() as Image;
			Sprite sprite = Sprite.Create(_prefab.texture, _rect, _pivot);
			if ((_rectTarget.width != 0) && (_rectTarget.height != 0))
			{
				newTransform.sizeDelta = new Vector2(_rectTarget.width, _rectTarget.height);
			}
			srcImage.sprite = sprite;
			return sprite;
		}

		// ---------------------------------------------------
		/**
		 @brief isInsideCone: Will test if the game player is inside the cone of vision
		 */
		public static float IsInsideCone(GameObject _source, float _yaw, GameObject _objective, float _rangeDetection, float _angleDetection)
		{
			float distance = Vector3.Distance(new Vector3(_source.transform.position.x, 0, _source.transform.position.z),
											 new Vector3(_objective.transform.position.x, 0, _objective.transform.position.z));
			if (distance < _rangeDetection)
			{
				float yaw = _yaw * Mathf.Deg2Rad;
				Vector2 pos = new Vector2(_source.transform.position.x, _source.transform.position.z);

				Vector2 v1 = new Vector2((float)Mathf.Cos(yaw), (float)Mathf.Sin(yaw));
				Vector2 v2 = new Vector2(_objective.transform.position.x - pos.x, _objective.transform.position.z - pos.y);

				// Angle detection
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
				float angleCreated = (v1.x * v2.x) + (v1.y * v2.y);
				float angleResult = Mathf.Cos(_angleDetection * Mathf.Deg2Rad);

				if (angleCreated > angleResult)
				{
					return (distance);
				}
				else
				{
					return (-1);
				}
			}
			else
			{
				return (-1);
			}
		}

		// ---------------------------------------------------
		/**
		 @brief Check if between two points there is no obstacle
		 */
		public static bool CheckFreePath(Vector3 _goalPosition, Vector3 _originPosition, params int[] _masks)
		{
			Vector3 fwd = new Vector3(_goalPosition.x - _originPosition.x, _goalPosition.y - _originPosition.y, _goalPosition.z - _originPosition.z);
			float distanceTotal = fwd.sqrMagnitude;
			fwd.Normalize();
			Ray ray = new Ray();
			ray.direction = fwd;
			RaycastHit hitCollision = new RaycastHit();

			int layerMask = Physics.DefaultRaycastLayers;
			for (int i = 0; i < _masks.Length; i++)
			{
				layerMask = layerMask & _masks[i];
			}
			if (Physics.Raycast(_originPosition, fwd, out hitCollision, Mathf.Infinity, layerMask))
			{
				// var distanceToGround = hitCollision.distance;
				if (Vector3.Distance(hitCollision.point, _originPosition) < distanceTotal)
				{
					return false;
				}
			}

			return true;
		}

		// ---------------------------------------------------
		/**
		 @brief Gets the collision point between two positions, zero if there is no collision
		 */
		public static Vector3 GetCollisionPoint(Vector3 _goalPosition, Vector3 _originPosition, params int[] _masks)
		{
			Vector3 fwd = new Vector3(_goalPosition.x - _originPosition.x, _goalPosition.y - _originPosition.y, _goalPosition.z - _originPosition.z);
			float distanceTotal = fwd.magnitude;
			fwd.Normalize();
			Ray ray = new Ray();
			ray.direction = fwd;
			RaycastHit hitCollision = new RaycastHit();

			int layerMask = Physics.DefaultRaycastLayers;
			for (int i = 0; i < _masks.Length; i++)
			{
				layerMask = layerMask & _masks[i];
			}
			if (Physics.Raycast(_originPosition, fwd, out hitCollision, Mathf.Infinity, layerMask))
			{
				if (Vector3.Distance(hitCollision.point, _originPosition) < distanceTotal)
				{
					return hitCollision.point;
				}
			}

			return Vector3.zero;
		}

		// ---------------------------------------------------
		/**
		 @brief ClonePoint
		 */
		public static Vector3 ClonePoint(Vector3 _position)
		{
			return new Vector3(_position.x, _position.y, _position.z);
		}

		// ---------------------------------------------------
		/**
		 @brief ClonePoint
		 */
		public static Vector2 ClonePoint(Vector2 _position)
		{
			return new Vector2(_position.x, _position.y);
		}

		// ---------------------------------------------------
		/**
		 @brief We get the collided object through the forward vector
		 */
		public static bool GetCollidedInfoByRay(Vector3 _origin, Vector3 _forward, ref RaycastHit _hitCollision)
		{
			Vector3 fwd = ClonePoint(_forward);
			fwd.Normalize();
			Ray ray = new Ray();
			ray.direction = _forward;

			if (Physics.Raycast(_origin, fwd, out _hitCollision))
			{
				return true;
			}

			return false;
		}

        // ---------------------------------------------------
        /**
		 @brief We get the whole RaycastHit information of the collision, with the mask to consider
		 */
        public static bool GetRaycastHitInfoByRayWithMask(Vector3 _origin, Vector3 _forward, ref RaycastHit _hitCollision, params string[] _masksToConsider)
        {
            Vector3 fwd = ClonePoint(_forward);
            fwd.Normalize();

            int layerMask = 0;
            if (_masksToConsider != null)
            {
                for (int i = 0; i < _masksToConsider.Length; i++)
                {
                    layerMask |= (1 << LayerMask.NameToLayer(_masksToConsider[i]));
                }
            }
            bool result = false;
            if (layerMask == 0)
            {
                result = Physics.Raycast(_origin, fwd, out _hitCollision, Mathf.Infinity);
            }
            else
            {
                result = Physics.Raycast(_origin, fwd, out _hitCollision, Mathf.Infinity, layerMask);
            }
            return result;
        }

        // ---------------------------------------------------
        /**
		 @brief We get the collided object through the forward vector
		 */
        public static GameObject GetCollidedObjectByRay(Vector3 _origin, Vector3 _forward)
		{
			Vector3 fwd = ClonePoint(_forward);
			fwd.Normalize();
			Ray ray = new Ray();
			ray.direction = _forward;
			RaycastHit hitCollision = new RaycastHit();

			if (Physics.Raycast(_origin, fwd, out hitCollision))
			{
				return hitCollision.collider.gameObject;
			}

			return null;
		}


		// ---------------------------------------------------
		/**
		 @brief We get the whole RaycastHit information of the collision, with the mask to ignore
		 */
		public static RaycastHit GetRaycastHitInfoByRay(Vector3 _origin, Vector3 _forward, params string[] _masksToIgnore)
		{
			Vector3 fwd = ClonePoint(_forward);
			fwd.Normalize();
			RaycastHit hitCollision = new RaycastHit();

			int layerMask = Physics.IgnoreRaycastLayer;
            if (_masksToIgnore != null)
			{
				for (int i = 0; i < _masksToIgnore.Length; i++)
				{
					layerMask |= (1 << LayerMask.NameToLayer(_masksToIgnore[i]));
				}
                layerMask = ~layerMask;
            }
			Physics.Raycast(_origin, fwd, out hitCollision, Mathf.Infinity, layerMask);
			return hitCollision;
		}

        // ---------------------------------------------------
        /**
		 @brief We get the whole RaycastHit information of the collision, with the mask to ignore
		 */
        public static bool GetRaycastHitInfoByRay(Vector3 _origin, Vector3 _forward, ref RaycastHit _hitCollision, params string[] _masksToIgnore)
        {
            Vector3 fwd = ClonePoint(_forward);
            fwd.Normalize();            

            int layerMask = Physics.IgnoreRaycastLayer;
            if (_masksToIgnore != null)
            {
                for (int i = 0; i < _masksToIgnore.Length; i++)
                {
                    layerMask |= (1 << LayerMask.NameToLayer(_masksToIgnore[i]));
                }
                layerMask = ~layerMask;
            }
            if (Physics.Raycast(_origin, fwd, out _hitCollision, Mathf.Infinity, layerMask))
            {
                return true;
            }
            return false;
        }

        // ---------------------------------------------------
        /**
		 @brief We get the whole RaycastHit information of the collision, with the mask to consider
		 */
        public static RaycastHit GetRaycastHitInfoByRayWithMask(Vector3 _origin, Vector3 _forward, params string[] _masksToConsider)
		{
			Vector3 fwd = ClonePoint(_forward);
			fwd.Normalize();
			RaycastHit hitCollision = new RaycastHit();

			int layerMask = 0;
			if (_masksToConsider != null)
			{
				for (int i = 0; i < _masksToConsider.Length; i++)
				{
					layerMask |= (1 << LayerMask.NameToLayer(_masksToConsider[i]));
				}
			}
			if (layerMask == 0)
			{
				Physics.Raycast(_origin, fwd, out hitCollision, Mathf.Infinity);
			}
			else
			{
				Physics.Raycast(_origin, fwd, out hitCollision, Mathf.Infinity, layerMask);
			}
			return hitCollision;
		}

		// ---------------------------------------------------
		/**
		 @brief We get the collided object between 2 points
		 */
		public static GameObject GetCollidedObjectByRayTarget(Vector3 _goalPosition, Vector3 _originPosition, params int[] _masks)
		{
			Vector3 fwd = new Vector3(_goalPosition.x - _originPosition.x, _goalPosition.y - _originPosition.y, _goalPosition.z - _originPosition.z);
			float distanceTotal = fwd.sqrMagnitude;
			fwd.Normalize();
			Ray ray = new Ray();
			ray.direction = fwd;
			RaycastHit hitCollision = new RaycastHit();

			int layerMask = 0;
			if (_masks.Length == 0)
			{
				layerMask = Physics.DefaultRaycastLayers;
			}
			else
			{
				layerMask = Physics.DefaultRaycastLayers;
				for (int i = 0; i < _masks.Length; i++)
				{
					layerMask = layerMask | _masks[i];
				}
			}
			if (Physics.Raycast(_originPosition, fwd, out hitCollision, Mathf.Infinity, layerMask))
			{
				return hitCollision.collider.gameObject;
			}

			return null;
		}

        // ---------------------------------------------------
        /**
		 @brief We get the collided object between 2 points
		 */
        public static GameObject GetCollidedObjectByRayTargetIgnore(Vector3 _goalPosition, Vector3 _originPosition, params string[] _masksToIgnore)
        {
            Vector3 fwd = new Vector3(_goalPosition.x - _originPosition.x, _goalPosition.y - _originPosition.y, _goalPosition.z - _originPosition.z);
            float distanceTotal = fwd.sqrMagnitude;
            fwd.Normalize();
            Ray ray = new Ray();
            ray.direction = fwd;
            RaycastHit hitCollision = new RaycastHit();

            int layerMask = Physics.IgnoreRaycastLayer;
            if (_masksToIgnore != null)
            {
                for (int i = 0; i < _masksToIgnore.Length; i++)
                {
                    layerMask |= (1 << LayerMask.NameToLayer(_masksToIgnore[i]));
                }
                layerMask = ~layerMask;
            }
            if (Physics.Raycast(_originPosition, fwd, out hitCollision, Mathf.Infinity, layerMask))
            {
                return hitCollision.collider.gameObject;
            }

            return null;
        }


        // ---------------------------------------------------
        /**
		 @brief We get the collided object between 2 points
		 */
        public static bool GetCollidedObjectBySegmentTargetIgnore(Vector3 _goalPosition, Vector3 _originPosition, params string[] _masksToIgnore)
        {
            Vector3 fwd = new Vector3(_goalPosition.x - _originPosition.x, _goalPosition.y - _originPosition.y, _goalPosition.z - _originPosition.z);
            float distanceTotal = Vector3.Distance(_goalPosition, _originPosition);
            fwd.Normalize();
            Ray ray = new Ray();
            ray.direction = fwd;
            RaycastHit hitCollision = new RaycastHit();

            int layerMask = Physics.IgnoreRaycastLayer;
            if (_masksToIgnore != null)
            {
                for (int i = 0; i < _masksToIgnore.Length; i++)
                {
                    layerMask |= (1 << LayerMask.NameToLayer(_masksToIgnore[i]));
                }
                layerMask = ~layerMask;
            }
            if (layerMask == 0)
            {
                if (Physics.Raycast(_originPosition, fwd, out hitCollision, distanceTotal))
                {
                    // Debug.LogError("[--NO FILTER--]::NAME["+ hitCollision.collider.gameObject.name + "]::LAYER["+ LayerMask.LayerToName(hitCollision.collider.gameObject.layer) + "]");
                    return true;
                }
            }
            else
            {
                if (Physics.Raycast(_originPosition, fwd, out hitCollision, distanceTotal, layerMask))
                {
                    // Debug.LogError("[++WITH FILTER++]::NAME[" + hitCollision.collider.gameObject.name + "]::LAYER[" + LayerMask.LayerToName(hitCollision.collider.gameObject.layer) + "]::DISTANCE CHECK["+ distanceTotal + "]");
                    return true;
                }
            }

            return false;
        }

        // ---------------------------------------------------
        /**
		 @brief We get the collided object for a forward vector
		 */
        public static GameObject GetCollidedObjectByRayForward(Vector3 _origin, Vector3 _forward, params int[] _masks)
		{
			Vector3 fwd = ClonePoint(_forward);
			fwd.Normalize();
			Ray ray = new Ray();
			ray.direction = _forward;
			RaycastHit hitCollision = new RaycastHit();

			int layerMask = 0;
			if (_masks.Length == 0)
			{
				layerMask = Physics.DefaultRaycastLayers;
			}
			else
			{
				layerMask = Physics.DefaultRaycastLayers;
				for (int i = 0; i < _masks.Length; i++)
				{
					layerMask = layerMask | _masks[i];
				}
			}
			if (Physics.Raycast(_origin, fwd, out hitCollision, Mathf.Infinity, layerMask))
			{
				return hitCollision.collider.gameObject;
			}

			return null;
		}

		// ---------------------------------------------------
		/**
		 @brief We get the collision point for a forward vector
		 */
		public static Vector3 GetCollidedPointByRayForward(Vector3 _origin, Vector3 _forward, params int[] _masks)
		{
			Vector3 fwd = ClonePoint(_forward);
			fwd.Normalize();
			Ray ray = new Ray();
			ray.direction = _forward;
			RaycastHit hitCollision = new RaycastHit();

			int layerMask = 0;
			if (_masks.Length == 0)
			{
				layerMask = Physics.DefaultRaycastLayers;
			}
			else
			{
				layerMask = Physics.DefaultRaycastLayers;
				for (int i = 0; i < _masks.Length; i++)
				{
					layerMask = layerMask | _masks[i];
				}
			}
			if (Physics.Raycast(_origin, fwd, out hitCollision, Mathf.Infinity, layerMask))
			{
				return hitCollision.point;
			}

			return Vector3.zero;
		}

        // -------------------------------------------
        /* 
		 * We apply a material on all the hirarquy of objects
		 */
        public static void SetAllChildCollidersTrigger(GameObject _go, bool _trigger)
        {
            foreach (Transform child in _go.transform)
            {
                SetAllChildCollidersTrigger(child.gameObject, _trigger);
            }
            if (_go.GetComponent<Collider>() != null)
            {
                if (_go.GetComponent<MeshCollider>()!=null) _go.GetComponent<MeshCollider>().convex = _trigger;
                _go.GetComponent<Collider>().isTrigger = _trigger;
            }
        }

        // -------------------------------------------
        /* 
		 * We apply a box collider trigger on all the hirarquy of objects
		 */
        public static void SetAllChildBoxCollidersTrigger(GameObject _go, bool _trigger)
        {
            foreach (Transform child in _go.transform)
            {
                SetAllChildBoxCollidersTrigger(child.gameObject, _trigger);
            }
            if (_go.GetComponent<BoxCollider>() != null)
            {
                _go.GetComponent<BoxCollider>().isTrigger = _trigger;
            }
        }

        // -------------------------------------------
        /* 
		 * We apply a material on all the hirarquy of objects
		 */
        public static void ApplyMaterialOnImages(GameObject _go, Material _material)
		{
			foreach (Transform child in _go.transform)
			{
				ApplyMaterialOnImages(child.gameObject, _material);
			}
			if (_go.GetComponent<Image>() != null)
			{
				_go.GetComponent<Image>().material = _material;
			}

			if (_go.GetComponent<Text>() != null)
			{
				_go.GetComponent<Text>().material = _material;
			}
		}


		// -------------------------------------------
		/* 
		 * We apply a material on all the hirarquy of objects
		 */
		public static void ApplyLayerOnGameObject(GameObject _go, LayerMask _layer)
		{
			foreach (Transform child in _go.transform)
			{
				ApplyLayerOnGameObject(child.gameObject, _layer);
			}
			if (_go != null)
			{
				_go.layer = _layer;
			}
		}

		// -------------------------------------------
		/* 
		 * We apply a material on all the hirarquy of objects
		 */
		public static void ApplyMaterialOnObjects(GameObject _go, Material _material)
		{
			foreach (Transform child in _go.transform)
			{
                ApplyMaterialOnObjects(child.gameObject, _material);
			}
			if (_go.GetComponent<Renderer>() != null)
			{
				_go.GetComponent<Renderer>().material = _material;
			}
		}

        // -------------------------------------------
        /* 
		 * We apply a material on all the hirarquy of objects
		 */
        public static void ApplyMaterialOnMeshes(GameObject _go, Material _material)
        {
            foreach (Transform child in _go.transform)
            {
                ApplyMaterialOnMeshes(child.gameObject, _material);
            }
            if (_go.GetComponent<SkinnedMeshRenderer>() != null)
            {
                _go.GetComponent<SkinnedMeshRenderer>().materials = new Material[0];
                _go.GetComponent<SkinnedMeshRenderer>().material = _material;
            }
        }

        // -------------------------------------------
        /* 
		 * Check if the objects is visible in the camera's frustum
		 */
        public static bool IsVisibleFrom(Bounds _bounds, Camera _camera)
		{
			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_camera);
			return GeometryUtility.TestPlanesAABB(planes, _bounds);
		}

		// -------------------------------------------
		/* 
		 * Move with control keys, with rigid body if there is one available or forcing the position
		 */
		public static void MoveAroundOrientedRigidbody(Transform _target, Vector3 _forward, Vector3 _right, float _speedMovement)
		{
			float moveForward = _speedMovement * Time.smoothDeltaTime * Input.GetAxis("Vertical");
			float moveLeft = _speedMovement * Time.smoothDeltaTime * Input.GetAxis("Horizontal");

			Vector3 newPosition = _target.position + (_forward * moveForward) + (_right * moveLeft);
			Vector3 normal = newPosition - _target.position;
			normal.Normalize();
			Vector3 newVelocity = normal * _speedMovement;
			if (!_target.GetComponent<Rigidbody>().isKinematic)
			{
				_target.GetComponent<Rigidbody>().velocity = new Vector3(newVelocity.x, _target.GetComponent<Rigidbody>().velocity.y, newVelocity.z);
			}
			else
			{
				_target.position = newPosition;
			}
		}

		// -------------------------------------------
		/* 
		 * Get the bounds of game object
		 */
		public static Bounds CalculateBounds(GameObject _gameObject)
		{
			Bounds bounds = CalculateBoundsThroughCollider(_gameObject);
			if (bounds.size == Vector3.zero)
			{
				bounds = CalculateBoundsThroughRenderer(_gameObject);
			}
			if (bounds.size == Vector3.zero)
			{
				bounds = CalculateBoundsThroughMesh(_gameObject);
			}
			return bounds;
		}

		// -------------------------------------------
		/* 
		 * Get the bounds through renderer
		 */
		public static Bounds CalculateBoundsThroughRenderer(GameObject _gameObject)
		{
			Renderer[] meshRenderers = _gameObject.GetComponentsInChildren<Renderer>();

			Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

			foreach (Renderer bc in meshRenderers)
			{
				bounds.Encapsulate(bc.bounds);
			}

			return bounds;
		}

		// -------------------------------------------
		/* 
		 * Get the bounds through collider
		 */
		public static Bounds CalculateBoundsThroughCollider(GameObject _gameObject)
		{
			Collider[] colliders = _gameObject.GetComponentsInChildren<Collider>();

			Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

			foreach (Collider bc in colliders)
			{
				bounds.Encapsulate(bc.bounds);
			}

			return bounds;
		}

		// -------------------------------------------
		/* 
		 * Get the bounds through mesh
		 */
		public static Bounds CalculateBoundsThroughMesh(GameObject _gameObject)
		{
			MeshRenderer[] meshRenderers = _gameObject.GetComponentsInChildren<MeshRenderer>();

			Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

			foreach (MeshRenderer bc in meshRenderers)
			{
				bounds.Encapsulate(bc.bounds);
			}

			return bounds;
		}

        // -------------------------------------------
        /* 
		 * Disaable all the mesh renderers
		 */
        public static void EnableMeshRenderers(GameObject _gameObject, bool _value)
        {
            Renderer[] meshRenderers = _gameObject.GetComponentsInChildren<Renderer>();

            foreach (Renderer bc in meshRenderers)
            {
                bc.enabled = _value;
            }
        }

        // -------------------------------------------
        /* 
		 * Check if there is a collider
		 */
        public static bool IsThereABoxCollider(GameObject _gameObject)
		{
			Collider[] colliders = _gameObject.GetComponentsInChildren<BoxCollider>();

			return colliders.Length > 0;
		}


		// -------------------------------------------
		/* 
		 * Return the rect container
		 */
		public static Rect GetCornersRectTransform(RectTransform _rectTransform)
		{
			Vector3[] corners = new Vector3[4];
			_rectTransform.GetWorldCorners(corners);
			Rect rec = new Rect(corners[0].x, corners[0].y, corners[2].x - corners[0].x, corners[2].y - corners[0].y);
			return rec;
		}



		// -------------------------------------------
		/* 
		 * Will look fot the gameobject in the childs
		 */
		public static bool FindGameObjectInChilds(GameObject _go, GameObject _target)
		{
			if (_go == _target)
			{
				return true;
			}
			bool output = false;
			foreach (Transform child in _go.transform)
			{
				output = output || FindGameObjectInChilds(child.gameObject, _target);
			}
			return output;
		}

        // -------------------------------------------
        /* 
		 * Will look fot the gameobject in the parent
		 */
        public static bool FindGameObjectInParent(GameObject _go, GameObject _target)
        {
            if (_go == _target)
            {
                return true;
            }
            bool output = false;
            if (_go.transform.parent != null)
            {
                output = output || FindGameObjectInParent(_go.transform.parent.gameObject, _target);
            }
            return output;
        }

        // -------------------------------------------
        /* 
		 * Copy to the clipboard
		 */
        public static string Clipboard
		{
			get { return GUIUtility.systemCopyBuffer; }
			set { GUIUtility.systemCopyBuffer = value; }
		}

		// -------------------------------------------
		/* 
		 * Copy to the clipboard
		 */
		public static string GetFormatNumber3Digits(int _number)
		{
			string sNumber = _number.ToString();
			int counter = 0;
			while (sNumber.Length > 3)
			{
				sNumber = sNumber.Substring(0, 3);
				counter++;
			}
			switch (counter)
			{
				case 1:
					sNumber += "K";
					break;
				case 2:
					sNumber += "M";
					break;
			}
			return sNumber;
		}

		// -------------------------------------------
		/* 
		 * GetMegabytesSize
		 */
		public static int GetMegabytesSize(long _size)
		{
			int sizeMegas = (int)(_size / (1024 * 1024));
			return sizeMegas;
		}

		// -------------------------------------------
		/* 
		 * GetFilesize
		 */
		public static long GetFilesize(string _filename)
		{
			try
			{
				FileStream fileInputStream = File.OpenRead(_filename);
				return fileInputStream.Length;
			}
			catch (Exception err)
			{
				Debug.LogError("ERROR TRYING TO OPEN THE FILE[" + _filename + "]=" + err.Message);
				return -1;
			}
		}

		// -------------------------------------------
		/* 
		 * ReplaceComaForDot
		 */
		public static string ReplaceComaForDot(string _data)
		{
			return _data.Replace(',', '.');
		}


		// ---------------------------------------------------
		/**
		 @brief Creation of test plane to know which direction to follow
		 */
		public static float AskDirectionPoint(Vector2 pos, float yaw, Vector2 objetive)
		{
			// Create Plane
			Vector3 p1 = new Vector3(pos.x, 0.0f, pos.y);
			Vector3 p2 = new Vector3((float)(pos.x + Mathf.Cos(yaw)), 0.0f, (float)(pos.y + Mathf.Sin(yaw)));
			Vector3 p3 = new Vector3(pos.x, 1.0f, pos.y);

			Debug.DrawLine(new Vector3(pos.x, 1, pos.y), new Vector3(p2.x, 1, p2.z), Color.red);
			Debug.DrawLine(new Vector3(pos.x, 1, pos.y), new Vector3(p3.x, 2, p3.z), Color.blue);

			Vector3 p = new Vector3(0.0f, 0.0f, 0.0f);
			Vector3 q = new Vector3(0.0f, 0.0f, 0.0f);
			Vector3 r = new Vector3(0.0f, 0.0f, 0.0f);

			p = p2 - p1;
			q = p3 - p1;

			r.x = (p.y * q.z) - (p.z * q.y);
			r.y = (p.z * q.x) - (p.x * q.z);
			r.z = (p.x * q.y) - (p.y * q.x);

			float moduloR = r.magnitude;
			if (moduloR == 0)
			{
				r.x = 0.0f;
				r.y = 0.0f;
				r.z = 0.0f;
			}
			else
			{
				r.x = r.x / moduloR;
				r.y = r.y / moduloR;
				r.z = r.z / moduloR;
			}
			float d = -((r.x * p1.x) + (r.y * p1.y) + (r.z * p1.z));

			// Check if point objective is in one side or another of the planeppos si centro del plano
			return (((objetive.x * r.x) + (objetive.y * r.z)) + d);
		}


		// ---------------------------------------------------
		/**
		 @brief Get distance from plane to point
		 */
		public static float GetDistancePlanePoint(Vector2 pos, float yaw, Vector2 objetive)
		{
			// Create Plane
			Vector3 p1 = new Vector3(pos.x, 0.0f, pos.y);
			Vector3 p2 = new Vector3((float)(pos.x + Mathf.Cos(yaw + (Mathf.PI / 2))), 0.0f, (float)(pos.y + Mathf.Sin(yaw + (Mathf.PI / 2))));
			Vector3 p3 = new Vector3(pos.x, 1.0f, pos.y);

			Vector3 p = new Vector3(0.0f, 0.0f, 0.0f);
			Vector3 q = new Vector3(0.0f, 0.0f, 0.0f);
			Vector3 r = new Vector3(0.0f, 0.0f, 0.0f);

			p = p2 - p1;
			q = p3 - p1;

			r.x = (p.y * q.z) - (p.z * q.y);
			r.y = (p.z * q.x) - (p.x * q.z);
			r.z = (p.x * q.y) - (p.y * q.x);

			float moduloR = r.magnitude;
			if (moduloR == 0)
			{
				r.x = 0.0f;
				r.y = 0.0f;
				r.z = 0.0f;
			}
			else
			{
				r.x = r.x / moduloR;
				r.y = r.y / moduloR;
				r.z = r.z / moduloR;
			}

			float d = -((r.x * p1.x) + (r.y * p1.y) + (r.z * p1.z));

			return Mathf.Abs((((objetive.x * r.x) + (objetive.y * r.z)) + d));
		}

		// ---------------------------------------------------
		/**
		 * Converst from string to Vector3
		 */
		public static Vector3 StringToVector3(string _data)
		{
			string[] values = _data.Split(',');
			return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
		}

		// ---------------------------------------------------
		/**
		 * Converst from string to Vector3
		 */
		public static string Vector3ToString(Vector3 _data)
		{
			return _data.x + "," + _data.y + "," + _data.z;
        }

        // ---------------------------------------------------
        /**
		 * Converst from string to Quaternion
		 */
        public static Quaternion StringToQuaternion(string _data)
        {
            string[] values = _data.Split(',');
            return new Quaternion(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
        }

        // ---------------------------------------------------
        /**
		 * Converst from Quaternion to string
		 */
        public static string QuaternionToString(Quaternion _data)
        {
            return _data.x + "," + _data.y + "," + _data.z + "," + _data.w;
        }

        // -------------------------------------------
        /* 
		 * GetTimestamp
		 */
        public static long GetTimestamp()
		{
			return (long)((DateTime.UtcNow - Jan1St1970).TotalMilliseconds);
		}

		// -------------------------------------------
		/* 
		 * GetTimestamp
		 */
		public static long GetTimestampSeconds()
		{
			return (long)(((DateTime.UtcNow - Jan1St1970).TotalMilliseconds) / 1000);
		}

        // -------------------------------------------
        /* 
		 * GetTimestampDays
		 */
        public static long GetTimestampDays()
        {
            return (long)((((((DateTime.UtcNow - Jan1St1970).TotalMilliseconds) / 1000) / 60) / 60) / 24);
        }

        // -------------------------------------------
        /* 
		 * GetDaysFromSeconds
		 */
        public static int GetDaysFromSeconds(long _seconds)
		{
			return (int)(((_seconds / 60) / 60) / 24);
		}

		// -------------------------------------------
		/* 
		 * CheckTextForbiddenCharacters
		 */
		public static bool CheckTextForbiddenCharacters(string _text, char[] _forbidden)
		{
			for (int i = 0; i < _forbidden.Length; i++)
			{
				if (_text.IndexOf(_forbidden[i]) != -1)
				{
					return true;
				}
			}
			return false;
		}

        // -------------------------------------------
        /* 
		 * Clone
		 */
        public static bool Clone(ref string[] _output, string[] _data)
        {
            if (_data != null)
            {
                if (_data.Length > 0)
                {
                    _output = new string[_data.Length];
                    for (int i = 0; i < _data.Length; i++)
                    {
                        _output[i] = _data[i];
                    }
                    return true;
                }
            };
            return false;
        }

        // -------------------------------------------
        /* 
		 * Clone
		 */
        public static Quaternion Clone(Quaternion _quaternion)
		{
			Quaternion output = new Quaternion();
			output.x = _quaternion.x;
			output.y = _quaternion.y;
			output.z = _quaternion.z;
			output.w = _quaternion.w;
			return output;
		}

        // -------------------------------------------
        /* 
		 * Clone
		 */
        public static Rect Clone(Rect _rect)
        {
            Rect output = new Rect();
            output.x = _rect.x;
            output.y = _rect.y;
            output.width = _rect.width;
            output.height = _rect.height;
            return output;
        }

        // -------------------------------------------
        /* 
		 * Clone
		 */
        public static Vector3 Clone(Vector3 _vector)
		{
			Vector3 output = new Vector3();
			output.x = _vector.x;
			output.y = _vector.y;
			output.z = _vector.z;
			return output;
		}


		// -------------------------------------------
		/* 
		 * Clone
		 */
		public static Vector2 Clone(Vector2 _vector)
		{
			Vector2 output = new Vector2();
			output.x = _vector.x;
			output.y = _vector.y;
			return output;
		}

		// -------------------------------------------
		/* 
		 * GetFormattedTimeSeconds
		 */
		public static string GetFormattedTimeMinutes(long _timestamp)
		{
			int totalSeconds = (int)_timestamp;
			int totalMinutes = (int)Math.Floor((double)(totalSeconds / 60));
			int totalHours = (int)Math.Floor((double)(totalMinutes / 60));
			int restSeconds = (int)(totalSeconds - (totalMinutes * 60));
			int restMinutes = (int)(totalMinutes - (totalHours * 60));

			// SECONDS
			String seconds;
			if (restSeconds < 10)
			{
				seconds = "0" + restSeconds;
			}
			else
			{
				seconds = "" + restSeconds;
			}

			// MINUTES
			String minutes;
			if (restMinutes < 10)
			{
				minutes = "0" + restMinutes;
			}
			else
			{
				minutes = "" + restMinutes;
			}

			return (minutes + ":" + seconds);
		}


        // -------------------------------------------
        /* 
		 * GetFormattedTimeSeconds
		 */
        public static string GetFormattedTimeDays(long _timestamp)
        {
            int totalSeconds = (int)_timestamp;
            int totalMinutes = (int)Math.Floor((double)(totalSeconds / 60));
            int totalHours = (int)Math.Floor((double)(totalMinutes / 60));
            int totalDays = (int)Math.Floor((double)(totalHours / 24));
            int restSeconds = (int)(totalSeconds - (totalMinutes * 60));
            int restMinutes = (int)(totalMinutes - (totalHours * 60));
            int restHours = (int)(totalHours - (totalDays * 24));

            // SECONDS
            String seconds;
            if (restSeconds < 10)
            {
                seconds = "0" + restSeconds;
            }
            else
            {
                seconds = "" + restSeconds;
            }

            // MINUTES
            String minutes;
            if (restMinutes < 10)
            {
                minutes = "0" + restMinutes;
            }
            else
            {
                minutes = "" + restMinutes;
            }

            // HOURS
            String hours;
            if (restHours < 10)
            {
                hours = "0" + restHours;
            }
            else
            {
                hours = "" + restHours;
            }

            if (totalDays > 0)
            {
                return (totalDays + " DAYS AND " + hours + " HOURS");
            }
            else
            {
                if (totalHours > 0)
                {
                    return (hours + ":" + minutes + ":" + seconds);
                }
                else
                {
                    return (minutes + ":" + seconds);
                }
            }
        }

        // -------------------------------------------
        /* 
		 * LoadPNG
		 */
        public static Texture2D LoadPNG(string _filePath)
		{
			Texture2D tex = null;
			byte[] fileData;

			if (File.Exists(_filePath))
			{
				fileData = File.ReadAllBytes(_filePath);
				tex = new Texture2D(2, 2);
				tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
			}
			return tex;
		}


		// -------------------------------------------
		/* 
		 * Replace the sprite with the content in the file
		 */
		public static void SetPictureByPath(Image _image, string _imagefilePath)
		{
			Texture2D newTexture = Utilities.LoadPNG(_imagefilePath);
			_image.sprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), Vector2.zero);
		}

		// -------------------------------------------
		/* 
		 * Find out if the object is a number
		 */
		public static bool IsNumber(object _value)
		{
			return (_value is int) || (_value is float) || (_value is double);
		}

		// -------------------------------------------
		/* 
		 * Get the number as an integer
		 */
		public static int GetInteger(object _value)
		{
			if (_value is int)
			{
				return (int)_value;
			}
			if (_value is float)
			{
				return (int)((float)_value);
			}
			if (_value is double)
			{
				return (int)((double)_value);
			}
			return -1;
		}

		// -------------------------------------------
		/* 
		 * Get the number as an float
		 */
		public static float GetFloat(object _value)
		{
			if (_value is int)
			{
				return (int)_value;
			}
			if (_value is float)
			{
				return (float)_value;
			}
			if (_value is double)
			{
				return (float)((double)_value);
			}
			return -1;
		}

		// -------------------------------------------
		/* 
		 * Get the number as an float
		 */
		public static double GetDouble(object _value)
		{
			if (_value is int)
			{
				return (int)_value;
			}
			if (_value is float)
			{
				return (float)_value;
			}
			if (_value is double)
			{
				return (double)_value;
			}
			return -1;
		}

		// -------------------------------------------
		/* 
		 * Check if the string can be converted to a number
		 */
		public static bool IsStringInteger(string _value)
		{
			int valueInteger = -1;
			if (int.TryParse(_value, out valueInteger))
			{
				return true;
			}

			return false;
		}


		// -------------------------------------------
		/* 
		 * Check if the string can be converted to a number
		 */
		public static bool IsStringFloat(string _value)
		{
			float valueFloat = -1;
			if (float.TryParse(_value, out valueFloat))
			{
				return true;
			}

			return false;
		}

		// -------------------------------------------
		/* 
		 * Check if the string can be converted to a number
		 */
		public static bool IsStringDouble(string _value)
		{
			float valueDouble = -1;
			if (float.TryParse(_value, out valueDouble))
			{
				return true;
			}

			return false;
		}

		// -------------------------------------------
		/* 
		 * Check if the string can be converted to a vector3
		 */
		public static bool IsStringVector3(string _value)
		{
			float valueFloat = -1;
			string[] vector = _value.Split(',');
			if (vector.Length == 3)
			{
				for (int i = 0; i < vector.Length; i++)
				{
					if (!float.TryParse(vector[i], out valueFloat))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		// -------------------------------------------
		/* 
		 * Will trim a string to fit a maximum number of characters
		 */
		public static string Trim(string _value, int _maxChars = 8)
		{
			if (_value.Length > _maxChars)
			{
				return _value.Substring(0, _maxChars);
			}
			else
			{
				return _value;
			}
		}

		// -------------------------------------------
		/* 
		 * GetBytesPNG
		 */
		public static byte[] GetBytesPNG(Sprite _image)
		{
			return _image.texture.EncodeToPNG();
		}

		// -------------------------------------------
		/* 
		 * GetBytesPNG
		 */
		public static byte[] GetBytesPNG(Texture2D _image)
		{
			return _image.EncodeToPNG();
		}

		// -------------------------------------------
		/* 
		* ComputeHashCode
		*/
		public static string ComputeHashCode(byte[] _bytes)
		{
			SHA256Managed shaEncryptor = new SHA256Managed();
			byte[] hash = shaEncryptor.ComputeHash(_bytes);
			return Convert.ToBase64String(hash);
		}

		// -------------------------------------------
		/* 
		 * Will generate a random string
		 */
		public static string RandomCodeIV(int _size)
		{
			string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789#!@+=-*";
			var stringChars = new char[_size];
			var random = new System.Random();

			for (int i = 0; i < _size; i++)
			{
				stringChars[i] = chars[random.Next(chars.Length)];
			}

			string finalString = new String(stringChars);
			return finalString;
		}

		// -------------------------------------------
		/* 
		 * GetFiles
		 */
		public static string[] GetFiles(string _path, string _searchPattern, SearchOption _searchOption)
		{
			string[] searchPatterns = _searchPattern.Split('|');
			List<string> files = new List<string>();
			foreach (string sp in searchPatterns)
			{
				files.AddRange(System.IO.Directory.GetFiles(_path, sp, _searchOption));
			}
			files.Sort();
			return files.ToArray();
		}

		// -------------------------------------------
		/* 
		 * GetFiles
		 */
		public static FileInfo[] GetFiles(DirectoryInfo _path, string _searchPattern, SearchOption _searchOption)
		{
			List<FileInfo> files = new List<FileInfo>();
			if (_searchPattern.Length > 0)
			{
				string[] searchPatterns = _searchPattern.Split('|');			
				foreach (string sp in searchPatterns)
				{
					files.AddRange(_path.GetFiles(sp, _searchOption));
				}
			}
			else
			{
				files.AddRange(_path.GetFiles());
			}
			return files.ToArray();
		}

		// -------------------------------------------
		/* 
		 * Will generate a random string
		 */
		public static string RandomCodeGeneration(string _idUser)
		{
			string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			var stringChars = new char[8];
			var random = new System.Random();

			for (int i = 0; i < stringChars.Length; i++)
			{
				stringChars[i] = chars[random.Next(chars.Length)];
			}

			string finalString = new String(stringChars) + "_" + _idUser;
			return finalString;
		}

		// -------------------------------------------
		/* 
		 * LoadAllByteData
		 */
		public static byte[] LoadAllByteData(string _pathFile)
		{
			if (System.IO.File.Exists(_pathFile))
			{
				return System.IO.File.ReadAllBytes(_pathFile);
			}
			else
			{
				return null;
			}
		}

		// -------------------------------------------
		/* 
		 * DebugLogError
		 */
		public static void DebugLogError(string _message)
		{
			Debug.Log("<color=red>" + _message + "</color>");
		}


		// -------------------------------------------
		/* 
		 * DebugLogWarning
		 */
		public static void DebugLogWarning(string _message)
		{
			Debug.Log("<color=yellow>" + _message + "</color>");
		}

		// -------------------------------------------
		/* 
		 * Get visible corners from camera
		 */
		private static int CountCornersVisibleFrom(ScrollRectVR _container, RectTransform _rectTransform)
		{
			Rect screenBounds = new Rect(_container.RectTransformObject.localPosition.x, _container.RectTransformObject.localPosition.y,
										 _container.RectTransformObject.sizeDelta.x, _container.RectTransformObject.sizeDelta.y);
			Vector2[] objectCorners = new Vector2[4];
			float shiftX = _container.RectTransformObject.localPosition.x;
			float shiftY = _container.RectTransformObject.localPosition.y;
			if (_container.IsVerticalGrid())
			{
				GameObject sVerticalGrid = _container.VerticalGrid.gameObject;
				shiftY += -sVerticalGrid.GetComponent<RectTransform>().localPosition.y + _container.InitialPositionVerticalGrid;
			}
			if (_container.IsHorizontalGrid())
			{
				GameObject sHorizontalGrid = _container.HorizontalGrid.gameObject;
				shiftX += sHorizontalGrid.GetComponent<RectTransform>().localPosition.x - _container.InitialPositionHorizontalGrid;
			}
			Vector2 p = new Vector2(_rectTransform.gameObject.transform.localPosition.x - (_rectTransform.sizeDelta.x / 2) + shiftX,
									-(_rectTransform.gameObject.transform.localPosition.y + (_rectTransform.sizeDelta.y / 2)) + shiftY);
			objectCorners[0] = new Vector2(p.x, p.y);
			objectCorners[1] = new Vector2(p.x, p.y + _rectTransform.sizeDelta.y);
			objectCorners[2] = new Vector2(p.x + _rectTransform.sizeDelta.x, p.y);
			objectCorners[3] = new Vector2(p.x + _rectTransform.sizeDelta.x, p.y + _rectTransform.sizeDelta.y);

			int visibleCorners = 0;
			for (var i = 0; i < objectCorners.Length; i++)
			{
				if (screenBounds.Contains(objectCorners[i]))
				{
					visibleCorners++;
				}
			}
			return visibleCorners;
		}

		// -------------------------------------------
		/* 
		 * Check if it's fully visible from camera
		 */
		public static bool IsFullyVisibleFrom(ScrollRectVR _container, RectTransform rectTransform)
		{
			return CountCornersVisibleFrom(_container, rectTransform) == 4;
		}

		// -------------------------------------------
		/* 
		 * Check if it's partially visible from camera
		 */
		public static bool IsVisibleFrom(ScrollRectVR _container, RectTransform rectTransform)
		{
			return CountCornersVisibleFrom(_container, rectTransform) > 0;
		}

		// -------------------------------------------
		/* 
		 * Move the scroll with the siblings information
		 */
		public static void MoveScrollWithSiblings(ScrollRectVR _container, GameObject _targetObject)
		{
			if (!IsFullyVisibleFrom(_container, _targetObject.GetComponent<RectTransform>()))
			{
				_container.LastSibling = _container.CurrentSibling;
				_container.CurrentSibling = _targetObject.transform.GetSiblingIndex();

				// ITEM IS NOT VISIBLE, THEN SCROLL PAGE
				float direction = 1;
				int sizePage = _container.VisibleItemsPerPage;
				float normalizePosition = 0;
				if (_container.ScrollRectObject.vertical)
				{
					if (_container.LastSibling < _container.CurrentSibling)
					{
						direction = 1;
					}
					else
					{
						direction = -1;
					}
					if (_container.CurrentSibling < _container.ScrollRectObject.content.transform.childCount - 3)
					{
						normalizePosition = (float)(_container.CurrentSibling + (sizePage * direction)) / (float)_container.ScrollRectObject.content.transform.childCount;
					}
					else
					{
						normalizePosition = 1;
					}
					if (normalizePosition < 0) normalizePosition = 0;
					if (normalizePosition > 1) normalizePosition = 1;
					_container.ScrollRectObject.verticalNormalizedPosition = 1 - normalizePosition;
				}
				else
				{
					if (_container.LastSibling < _container.CurrentSibling)
					{
						direction = 1;
					}
					else
					{
						direction = -1;
					}
					if (_container.CurrentSibling < _container.ScrollRectObject.content.transform.childCount - 3)
					{
						normalizePosition = (float)(_container.CurrentSibling + (sizePage * direction)) / (float)_container.ScrollRectObject.content.transform.childCount;
					}
					else
					{
						normalizePosition = 1;
					}
					if (normalizePosition < 0) normalizePosition = 0;
					if (normalizePosition > 1) normalizePosition = 1;

					_container.ScrollRectObject.horizontalNormalizedPosition = normalizePosition;
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Get the bounds of the scroll rect
		 */
		public static void CalculateScrollRect(GameObject _gameObject, ref List<ScrollRectVR> _scrollRects)
		{
			ScrollRect[] scrollRects = _gameObject.GetComponentsInChildren<ScrollRect>();
			_scrollRects = new List<ScrollRectVR>();

			if (scrollRects.Length > 0)
			{
				foreach (ScrollRect bc in scrollRects)
				{
					_scrollRects.Add(new ScrollRectVR(bc.gameObject));
				}

				if (_scrollRects.Count > 0)
				{
					CalculateVisibleItemsForeachScrollRect(_scrollRects);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * We calculate the number of visible elements that the scrollrect has
		 */
		public static bool CalculateVisibleItemsForeachScrollRect(List<ScrollRectVR> _scrollRectsVR)
		{
			for (int i = 0; i < _scrollRectsVR.Count; i++)
			{
				ButtonVRView[] buttonsVR = _scrollRectsVR[i].BaseObject.GetComponentsInChildren<ButtonVRView>();
				int counter = 0;
				foreach (ButtonVRView btnVR in buttonsVR)
				{
					if (IsFullyVisibleFrom(_scrollRectsVR[i], btnVR.gameObject.GetComponent<RectTransform>()))
					{
						counter++;
					}
				}
				_scrollRectsVR[i].VisibleItemsPerPage = counter;
			}

			return true;
		}

		// -------------------------------------------
		/* 
		 * We check the sibling visibility
		 */
		private static bool CheckSiblingVisibility(ScrollRectVR _container, Transform _parent, int _indexSibling)
		{
			if ((_indexSibling < 0) || (_indexSibling >= _parent.transform.childCount)) return false;

			Transform childTransform = _parent.transform.GetChild(_indexSibling);
			if (childTransform != null)
			{
				if (childTransform.GetComponent<RectTransform>() != null)
				{
					return IsFullyVisibleFrom(_container, childTransform.GetComponent<RectTransform>());
				}
			}
			return false;
		}


        // -------------------------------------------
        /* 
		 * DebugLogByteArray
		 */
        public static void DebugLogByteArray(byte[] _bytes)
        {
            int acumlator = 0;
            for (int i = 0; i < _bytes.Length; i++)
            {
                if ((i < 3) || (i >= _bytes.Length - 3))
                {
                    Debug.Log("_bytes[" + i + "]=" + _bytes[i]);
                }
                acumlator += (int)_bytes[i];
            }
            Debug.LogError("ACUMULATOR BYTES=" + acumlator);
        }

        // -------------------------------------------
        /* 
		 * DebugLogFloatArray
		 */
        public static void DebugLogFloatArray(float[] _floats)
        {
            int acumlator = 0;
            for (int i = 0; i < _floats.Length; i++)
            {
                if ((i < 3) || (i >= _floats.Length - 3))
                {
                    Debug.Log("_floats[" + i + "]=" + _floats[i]);
                }
                acumlator += (int)_floats[i];
            }
            Debug.LogError("ACUMULATOR FLOATS=" + acumlator);
        }

        // -------------------------------------------
        /* 
        * CastAsInteger
        */
        public static int CastAsInteger(object _data)
        {
            if (_data is string)
            {
                return int.Parse(_data.ToString());
            }
            else if (_data is float)
            {
                return (int)((float)_data);
            }
            else if (_data is double)
            {
                return (int)((double)_data);
            }
            else if (_data is int)
            {
                return (int)_data;
            }
            else if (_data is long)
            {
                return (int)((long)_data);
            }
            return -1;
        }

        // -------------------------------------------
        /* 
        * CastAsFloat
        */
        public static float CastAsFloat(object _data)
        {
            if (_data is string)
            {
                return float.Parse(_data.ToString());
            }
            else if (_data is float)
            {
                return (float)_data;
            }
            else if (_data is double)
            {
                return (float)((double)_data);
            }
            else if (_data is int)
            {
                return (float)_data;
            }
            else if (_data is long)
            {
                return (float)((long)_data);
            }
            return -1;
        }

        // -------------------------------------------
        /* 
        * CastAsLong
        */
        public static long CastAsLong(object _data)
        {
            if (_data is string)
            {
                return long.Parse(_data.ToString());
            }
            else if (_data is float)
            {
                return (long)((float)_data);
            }
            else if (_data is double)
            {
                return (long)((double)_data);
            }
            else if (_data is int)
            {
                return (long)((int)_data);
            }
            else if (_data is long)
            {
                return (long)_data;
            }
            return -1;
        }

        // ---------------------------------------------------
        /**
		* Use a 2D plane math to move forward the target
		*/
        public static Vector2 LogicAlineation(Actor _target, Vector3 _goal, float _speedMovement, float _speedRotation, bool _applyGravity = false)
        {
            float yaw = _target.Yaw * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(_target.gameObject.transform.position.x, _target.gameObject.transform.position.y, _target.gameObject.transform.position.z);

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
                CharacterController controller = _target.GetComponent<CharacterController>();
                if (controller == null)
                {
                    _target.gameObject.GetComponent<Rigidbody>().MovePosition(new Vector3(_target.gameObject.GetComponent<Rigidbody>().position.x + (vf.x * _speedMovement * Time.deltaTime)
                                                                             , _target.gameObject.GetComponent<Rigidbody>().position.y
                                                                             , _target.gameObject.GetComponent<Rigidbody>().position.z + (vf.y * _speedMovement * Time.deltaTime)));
                }
                else
                {
                    Vector3 movement = new Vector3((vf.x * _speedMovement * Time.deltaTime),
                                                    0,
                                                    (vf.y * _speedMovement * Time.deltaTime)) + ((normalVector.z != 0) ? Vector3.zero : (_applyGravity ? (Physics.gravity * Time.deltaTime) : Vector3.zero));
                    controller.Move(movement);
                }
            }
            _target.DirectionLeft = directionLeft;
            _target.Yaw = yawGoal;
            return vf;
        }

        // ---------------------------------------------------
        /**
		* Use a 2D plane math to move forward the target
		*/
        public static Vector2 LogicLocalAlineation(Actor _target, Vector3 _goal, float _speedMovement, float _speedRotation, bool _applyGravity = false)
        {
            float yaw = _target.Yaw * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(_target.gameObject.transform.localPosition.x, _target.gameObject.transform.localPosition.y, _target.gameObject.transform.localPosition.z);

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
                                                (vf.y * _speedMovement * Time.deltaTime)) + ((normalVector.z != 0) ? Vector3.zero : (_applyGravity ? (Physics.gravity * Time.deltaTime) : Vector3.zero));

                _target.gameObject.transform.localPosition += movement;
            }
            _target.DirectionLeft = directionLeft;
            _target.Yaw = yawGoal;
            return vf;
        }


        // ---------------------------------------------------
        /**
		* GetMacAddress
		*/
        public static string GetMacAddress()
        {
            var macAdress = "";
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            var i = 0;
            foreach (NetworkInterface adapter in nics)
            {
                PhysicalAddress address = adapter.GetPhysicalAddress();
                if (address.ToString() != "")
                {
                    macAdress = address.ToString();
                    return macAdress;
                }
            }
            return null;
        }

        // ---------------------------------------------------
        /**
        * GetRandom
        */
        public static T GetRandom<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
                return default(T);

            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        // ---------------------------------------------------
        /**
        * GetRandom
        */
        public static T GetRandom<T>(this T[] list)
        {
            if (list == null || list.Length == 0)
                return default(T);

            return list[UnityEngine.Random.Range(0, list.Length)];
        }

        // -------------------------------------------
        /* 
		 * CreateBinaryFile
		 */
        public static string CreatePathForFile(string _name, string _folder)
        {
            string folderPath = Path.Combine(Application.persistentDataPath, _folder);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            return Path.Combine(folderPath, _name);
        }

        // -------------------------------------------
        /* 
		 * SaveBinaryData
		 */
        public static void SaveBinaryData(byte[] _data, string _path)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream fileStream = File.Open(_path, FileMode.OpenOrCreate))
            {
                binaryFormatter.Serialize(fileStream, _data);
            }
        }

        // -------------------------------------------
        /* 
		 * LoadBinaryData
		 */
        public static byte[] LoadBinaryData(string _path)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (FileStream fileStream = File.Open(_path, FileMode.Open))
            {
                return (byte[])binaryFormatter.Deserialize(fileStream);
            }
        }

        // -------------------------------------------
        /* 
		 * GetFilePaths
		 */
        public static string[] GetFilePath(string _folderName, string _name)
        {
            string folderPath = Path.Combine(Application.persistentDataPath, _folderName);

            return Directory.GetFiles(folderPath, _name);
        }
    }
}