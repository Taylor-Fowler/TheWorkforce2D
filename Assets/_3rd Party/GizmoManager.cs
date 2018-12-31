// Source: https://itch.io/t/162915/drawing-unity-gizmos-in-builds
//by popcron.itch.io

using UnityEngine;
using System.Collections.Generic;

//Must be attached to a camera.
public class GizmoManager : MonoBehaviour
{
    public struct GizmoLine
    {
        public Vector3 a;
        public Vector3 b;
        public Color color;

        public GizmoLine(Vector3 a, Vector3 b, Color color)
        {
            this.a = a;
            this.b = b;
            this.color = color;
        }
    }

    public Material material;
    internal static List<GizmoLine> lines = new List<GizmoLine>();

    public static bool Show = false;

    void OnPostRender()
    {
        if(Show)
        {
            material.SetPass(0);
            GL.Begin(GL.LINES);

            for (int i = 0; i < lines.Count; i++)
            {
                GL.Color(lines[i].color);
                GL.Vertex(lines[i].a);
                GL.Vertex(lines[i].b);
            }

            GL.End();
            // Modification...
            //lines.Clear();
        }
    }
}