using System;
using System.Collections.Generic;
using System.Text;

using OpenGL;

namespace objview
{
    public class MeshModel : IModel
    {
        protected MeshVertex[] Vertices;

        protected int[] Faces;

        protected int[] Wires;

        protected uint VertexBufferName;

        protected uint IndexBufferName;

        protected uint WireIndexBufferName;

        protected int FaceIndexBufferCount;

        protected int WireIndexBufferCount;

        protected Vertex3f Center;

        protected float _BoundingRadious;

        public float BoundingRadius { get { return _BoundingRadious; } }

        public MeshModel(MeshVertex[] vertices, int[] faces, int[] wires)
        {
            Vertices = vertices;
            Faces = faces;
            Wires = wires;

            FaceIndexBufferCount = faces.Length;
            WireIndexBufferCount = wires.Length;

            EstimateBoundingSphare(vertices, out Center, out _BoundingRadious);
        }

        public void Dispose()
        {
            Gl.DeleteBuffers(VertexBufferName, IndexBufferName, WireIndexBufferName);
            VertexBufferName = 0;
            IndexBufferName = 0;
            WireIndexBufferName = 0;
        }

        public void Draw()
        {
            if (VertexBufferName == 0)
            {
                if (Vertices == null) throw new Exception();

                var names = new uint[3];
                Gl.GenBuffers(names);
                VertexBufferName = names[0];
                IndexBufferName = names[1];
                WireIndexBufferName = names[2];

                Gl.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferName);
                Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Vertices.Length * MeshVertex.Size), Vertices, BufferUsage.StaticDraw);
                Gl.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferName);
                Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint)(Faces.Length * sizeof(int)), Faces, BufferUsage.StaticDraw);
                Gl.BindBuffer(BufferTarget.ElementArrayBuffer, WireIndexBufferName);
                Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint)(Wires.Length * sizeof(int)), Wires, BufferUsage.StaticDraw);

                Vertices = null;
                Faces = null;
                Wires = null;
            }

            Gl.ShadeModel(ShadingModel.Smooth);

            Gl.Disable(EnableCap.CullFace);
            // Gl.CullFace(CullFaceMode.Back);
            Gl.Enable(EnableCap.DepthTest);

            Gl.Translate(-Center.x, -Center.y, -Center.z);

            Gl.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferName);
            Gl.EnableClientState(EnableCap.VertexArray);
            Gl.VertexPointer(3, VertexPointerType.Float, MeshVertex.Size, (IntPtr)MeshVertex.CoordOffset);
            Gl.EnableClientState(EnableCap.NormalArray);
            Gl.NormalPointer(NormalPointerType.Float, MeshVertex.Size, (IntPtr)MeshVertex.NormalOffset);

            Gl.Enable(EnableCap.PolygonOffsetFill);
            Gl.PolygonOffset(1f, 1f);
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferName);
            Gl.DrawElements(PrimitiveType.Triangles, FaceIndexBufferCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
            Gl.Disable(EnableCap.PolygonOffsetFill);

            Gl.Disable(EnableCap.Lighting);
            Gl.Color3(0f, 0.5f, 0f);
            Gl.DisableClientState(EnableCap.NormalArray);
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, WireIndexBufferName);
            Gl.DrawElements(PrimitiveType.Lines, WireIndexBufferCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        protected static void EstimateBoundingSphare(MeshVertex[] vertices, out Vertex3f center, out float radious)
        {
            float x0 = float.MaxValue, y0 = float.MaxValue, z0 = float.MaxValue;
            float x9 = float.MinValue, y9 = float.MinValue, z9 = float.MinValue;
            for (int i = 0; i < vertices.Length; i++)
            {
                var v = vertices[i].Coord;
                if (v.x < x0) x0 = v.x;
                if (v.x > x9) x9 = v.x;
                if (v.y < y0) y0 = v.y;
                if (v.y > y9) y9 = v.y;
                if (v.z < z0) z0 = v.z;
                if (v.z > z9) z9 = v.z;
            }
            var p = center = new Vertex3f((x0 + x9) / 2, (y0 + y9) / 2, (z0 + z9) / 2);
            float r9 = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                var r = (vertices[i].Coord - p).ModuleSquared();
                if (r > r9) r9 = r;
            }
            radious = (float)Math.Sqrt(r9);
        }
    }
}
