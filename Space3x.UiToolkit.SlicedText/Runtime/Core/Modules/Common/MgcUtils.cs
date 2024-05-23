using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.SlicedText
{
    public static class MgcUtils
    {
        static readonly Vertex[] k_Vertices = new Vertex[4];
        static readonly ushort[] k_Indices = { 0, 1, 2, 2, 3, 0 };

        public static void PaintRectangle(
            this MeshGenerationContext target,
            Rect rect,
            Color color)
        {
            target.PaintRectangle(rect.xMin, rect.yMin, rect.xMax, rect.yMax, color);
        }

        public static void PaintRectangle(
            this MeshGenerationContext target, 
            float xMin, 
            float yMin, 
            float xMax, 
            float yMax,
            Color color)
        {
            k_Vertices[0].tint = color;
            k_Vertices[1].tint = color;
            k_Vertices[2].tint = color;
            k_Vertices[3].tint = color;
            
            k_Vertices[0].position = new Vector3(xMin, yMax, Vertex.nearZ);
            k_Vertices[1].position = new Vector3(xMin, yMin, Vertex.nearZ);
            k_Vertices[2].position = new Vector3(xMax, yMin, Vertex.nearZ);
            k_Vertices[3].position = new Vector3(xMax, yMax, Vertex.nearZ);
            
            MeshWriteData mwd = target.Allocate(k_Vertices.Length, k_Indices.Length);
            mwd.SetAllVertices(k_Vertices);
            mwd.SetAllIndices(k_Indices);
        }
    }
}
