using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace YourCommonTools
{
    /******************************************
     * 
     * ScrollRectSnap
     */
    public class ScrollRectSnap : MonoBehaviour
    {
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------
        public const string EVENT_SCROLLRECTSNAPE_PAGE_SELECTED = "EVENT_SCROLLRECTSNAPE_PAGE_SELECTED";

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------
        [Tooltip("how many screens or pages are there within the content (steps)")]
        public int Screens = 1;

        [Tooltip("Snap horizontally")]
        public bool SnapInH = true;

        [Tooltip("Snap vertically")]
        public bool SnapInV = true;

        [Tooltip("Speed to auto-adjust")]
        public float Speed = 100f;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------
        private float[] m_points;
        private float m_stepSize;

        private ScrollRect m_scroll;
        private bool m_lerpH;
        private float m_targetH;

        private bool m_lerpV;
        private float m_targetV;

        // -------------------------------------------
        /* 
		 * Start
		 */
        void Start()
        {
            m_scroll = gameObject.GetComponent<ScrollRect>();
            m_scroll.inertia = false;

            if (Screens > 0)
            {
                m_points = new float[Screens];
                m_stepSize = 1 / (float)(Screens - 1);

                for (int i = 0; i < Screens; i++)
                {
                    m_points[i] = i * m_stepSize;
                }
            }
            else
            {
                m_points[0] = 0;
            }
        }

        // -------------------------------------------
        /* 
		 * Update
		 */
        void Update()
        {
            if (m_lerpH)
            {
                m_scroll.horizontalNormalizedPosition = Mathf.Lerp(m_scroll.horizontalNormalizedPosition, m_targetH, Speed * m_scroll.elasticity * Time.deltaTime);
                if (Mathf.Approximately(m_scroll.horizontalNormalizedPosition, m_targetH)) m_lerpH = false;
            }
            if (m_lerpV)
            {
                m_scroll.verticalNormalizedPosition = Mathf.Lerp(m_scroll.verticalNormalizedPosition, m_targetV, Speed * m_scroll.elasticity * Time.deltaTime);
                if (Mathf.Approximately(m_scroll.verticalNormalizedPosition, m_targetV)) m_lerpV = false;
            }
        }

        // -------------------------------------------
        /* 
		 * DragEnd
		 */
        public void DragEnd()
        {
            int pageSelected = -1;
            if (m_scroll.horizontal && SnapInH)
            {
                pageSelected = FindNearest(m_scroll.horizontalNormalizedPosition, m_points);
                m_targetH = m_points[pageSelected];
                m_lerpH = true;
            }
            if (m_scroll.vertical && SnapInV)
            {
                pageSelected = FindNearest(m_scroll.verticalNormalizedPosition, m_points);
                m_targetH = m_points[pageSelected];
                m_lerpH = true;
            }
            UIEventController.Instance.DispatchUIEvent(EVENT_SCROLLRECTSNAPE_PAGE_SELECTED, pageSelected);            
        }

        // -------------------------------------------
        /* 
		 * OnDrag
		 */
        public void OnDrag()
        {
            m_lerpH = false;
            m_lerpV = false;
        }

        // -------------------------------------------
        /* 
		 * FindNearest
		 */
        private int FindNearest(float _f, float[] _array)
        {
            float distance = Mathf.Infinity;
            int output = 0;
            for (int index = 0; index < _array.Length; index++)
            {
                if (Mathf.Abs(_array[index] - _f) < distance)
                {
                    distance = Mathf.Abs(_array[index] - _f);
                    output = index;
                }
            }
            return output;
        }
    }
}
