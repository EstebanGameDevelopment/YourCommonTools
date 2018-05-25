using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace YourCommonTools
{
	public class PlaneFromPoly : MonoBehaviour
	{

		private Material mat;
		private Vector3[] poly;  // Initialized in the inspector

		private Vector3 m_origin;
		private Vector3 m_center;

		public Vector3 Origin
		{
			get { return m_origin; }
		}
		public Vector3 Center
		{
			get { return m_center; }
		}

		public PlaneFromPoly()
		{

		}

		public void Init(Vector3[] _points, Material _mat)
		{
			mat = _mat;
			poly = _points;
		}

		public void Logic(Vector3 _porActor, float _yPos)
		{
			if (poly == null || poly.Length < 3)
			{
				Debug.Log("Define 2D polygon in 'poly' in the the Inspector");
				return;
			}

			MeshFilter mf = gameObject.GetComponent<MeshFilter>();
			if (mf == null)
			{
				mf = gameObject.AddComponent<MeshFilter>();
			}

			Mesh mesh = new Mesh();
			mf.mesh = mesh;

			Renderer rend = gameObject.GetComponent<MeshRenderer>();
			if (rend == null)
			{
				rend = gameObject.AddComponent<MeshRenderer>();
			}
			rend.material = mat;

			m_center = FindCenter();

			Vector3[] vertices = new Vector3[poly.Length + 1];
			vertices[0] = Vector3.zero;

			for (int i = 0; i < poly.Length; i++)
			{
				// poly[i].y = 0;
				if (i == 0)
				{
					m_origin = Utilities.ClonePoint(poly[i]);
				}
				vertices[i + 1] = poly[i] - m_center;
			}

			mesh.vertices = vertices;

			int[] triangles = new int[poly.Length * 3];

			for (int i = 0; i < poly.Length - 1; i++)
			{
				triangles[i * 3] = i + 2;
				triangles[i * 3 + 1] = 0;
				triangles[i * 3 + 2] = i + 1;
			}

			triangles[(poly.Length - 1) * 3] = 1;
			triangles[(poly.Length - 1) * 3 + 1] = 0;
			triangles[(poly.Length - 1) * 3 + 2] = poly.Length;

			mesh.triangles = triangles = triangles;
			mesh.uv = BuildUVs(vertices);

			mesh.RecalculateBounds();
			mesh.RecalculateNormals();

			gameObject.transform.position =  new Vector3(_porActor.x - (m_origin.x - m_center.x), _porActor.y, _porActor.z - (m_origin.z - m_center.z));
		}

		Vector3 FindCenter()
		{
			Vector3 center = Vector3.zero;
			foreach (Vector3 v3 in poly)
			{
				center += v3;
			}
			return center / poly.Length;
		}

		Vector2[] BuildUVs(Vector3[] vertices)
		{

			float xMin = Mathf.Infinity;
			float zMin = Mathf.Infinity;
			float xMax = -Mathf.Infinity;
			float zMax = -Mathf.Infinity;

			foreach (Vector3 v3 in vertices)
			{
				if (v3.x < xMin)
					xMin = v3.x;
				if (v3.z < zMin)
					zMin = v3.z;
				if (v3.x > xMax)
					xMax = v3.x;
				if (v3.z > zMax)
					zMax = v3.z;
			}

			float xRange = xMax - xMin;
			float zRange = zMax - zMin;

			Vector2[] uvs = new Vector2[vertices.Length];
			for (int i = 0; i < vertices.Length; i++)
			{
				uvs[i].x = (vertices[i].x - xMin) / xRange;
				uvs[i].y = (vertices[i].z - zMin) / zRange;
			}
			return uvs;
		}
	}	
}
