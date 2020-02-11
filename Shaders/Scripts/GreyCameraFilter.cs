using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YourCommonTools
{
    /******************************************
	 * 
	 * GrayscaleFilter
	 * 
	 * @author Esteban Gallardo
	 */
    [ExecuteInEditMode]
    public class GreyCameraFilter : MonoBehaviour
    {
        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static GreyCameraFilter instance;

        public static GreyCameraFilter Instance
        {
            get
            {
                if (!instance)
                {
                    instance = GameObject.FindObjectOfType(typeof(GreyCameraFilter)) as GreyCameraFilter;
                }
                return instance;
            }
        }

        private Material m_mat;
        public Texture m_overlay;
        public bool m_activated = false;

        private void Awake()
        {
            this.enabled = m_activated;
        }

        private void OnRenderImage(RenderTexture _src, RenderTexture _dest)
        {
            if (!m_activated)
            {
                DestroyMaterial();
                return;
            }

            if (!m_mat)
                m_mat = new Material(Shader.Find("Hidden/GSF"));

            m_mat.SetTexture("_Mask", m_overlay);

            Graphics.Blit(_src, _dest, m_mat);
        }

        private void OnDisable()
        {
            DestroyMaterial();
        }

        private void DestroyMaterial()
        {
            if (m_mat)
            {
                DestroyImmediate(m_mat);
                m_mat = null;
            }
        }

        public void SetActivation(bool _enable)
        {
            m_activated = _enable;
            this.enabled = m_activated;
        }
    }
}