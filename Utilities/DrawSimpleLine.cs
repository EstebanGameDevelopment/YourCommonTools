
using UnityEngine;

public class DrawSimpleLine : MonoBehaviour
{
    // ----------------------------------------------
    // SINGLETON
    // ----------------------------------------------	
    private static DrawSimpleLine _instance;

    public static DrawSimpleLine Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = GameObject.FindObjectOfType(typeof(DrawSimpleLine)) as DrawSimpleLine;
                if (!_instance)
                {
                    GameObject container = new GameObject();
                    container.name = "DrawSimpleLine";
                    _instance = container.AddComponent(typeof(DrawSimpleLine)) as DrawSimpleLine;
                }
            }
            return _instance;
        }
    }

    // When added to an object, draws colored rays from the
    // transform position.
    public int m_lineCount = 100;
    public float m_radius = 3.0f;
    private Material m_lineMaterial;

    public Vector3 Origin;
    public Vector3 End;

    public void DrawLine(Vector3 _origin, Vector3 _end)
    {
        Origin = _origin;
        End = _end;

        if (!m_lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            m_lineMaterial = new Material(shader);
            m_lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            m_lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m_lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            m_lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            m_lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    // Will be called after all regular rendering is done
    public void OnRenderObject()
    {
        GL.PushMatrix();
        m_lineMaterial.SetPass(0);
        GL.LoadOrtho();

        GL.Begin(GL.LINES);
        GL.Color(Color.red);
        GL.Vertex(Origin);
        GL.Vertex(End);
        GL.End();

        GL.PopMatrix();
    }
}