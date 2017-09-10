using System;
using System.Collections.Generic;
using System.Text;

using OpenGL;
using System.Runtime.InteropServices;

namespace objview
{
    public static class GlMain
    {
        private static IMesh _Mesh;

        private static bool MeshIsDirty;

        public static IMesh Mesh
        {
            get { return _Mesh; }
            set
            {
                _Mesh = value;
                MeshIsDirty = true;
            }
        }

        public static bool Animating { get { return Roller.IsAnimating; } }

        private static uint VertexBufferName;

        private static uint IndexBufferName;

        private static int ViewportWidth;

        private static int ViewportHeight;

        private static bool ViewportIsDirty;

        private static float ViewportCanonicalScale;

        private static float CameraDistance;

        private static Vertex3f MeshCenter;

        private static float MeshRadious;

        public static void Initialize()
        {
            var names = new uint[2];
            Gl.GenBuffers(names);
            VertexBufferName = names[0];
            IndexBufferName = names[1];
        }

        public static void Destroy()
        {
            Gl.DeleteBuffers(VertexBufferName, IndexBufferName);
            VertexBufferName = 0;
            IndexBufferName = 0;
        }

        public static void Resize(int width, int height)
        {
            ViewportWidth = width;
            ViewportHeight = height;
            ViewportIsDirty = true;
        }

        private const float CAMERA_DISTANCE_RATIO = 4.0f;

        private static DateTime LastDraw = DateTime.UtcNow;

        public static void Draw()
        {
            if (Mesh == null) return;

            Roller.Update();

            if (MeshIsDirty)
            {
                Gl.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferName);
                Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Mesh.Vertices.Length * MeshVertex.Size), Mesh.Vertices, BufferUsage.StaticDraw);
                Gl.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferName);
                Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint)(Mesh.Faces.Length * sizeof(int)), Mesh.Faces, BufferUsage.StaticDraw);

                EstimateBoundingSphare(Mesh.Vertices, out MeshCenter, out MeshRadious);
                ViewportIsDirty = true;

                Roller.Reset();

                MeshIsDirty = false;
            }

            if (ViewportIsDirty)
            {
                Gl.Viewport(0, 0, ViewportWidth, ViewportHeight);

                CameraDistance = CAMERA_DISTANCE_RATIO * MeshRadious;

                ViewportCanonicalScale = 2f / Math.Min(ViewportWidth, ViewportHeight);
                var s = (double)MeshRadious * ViewportCanonicalScale / 2;

                Gl.MatrixMode(MatrixMode.Projection);
                Gl.LoadIdentity();
                Gl.Frustum(-ViewportWidth * s, ViewportWidth * s, -ViewportHeight * s, ViewportHeight * s, CameraDistance - MeshRadious, CameraDistance + MeshRadious);
                //Gl.Ortho(-ViewportWidth * s, ViewportWidth * s, -ViewportHeight * s, ViewportHeight * s, distance - MeshRadious, distance + MeshRadious);
                Gl.Translate(0f, 0f, -CameraDistance);
            }

            Gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Gl.MatrixMode(MatrixMode.Modelview);
            Gl.LoadIdentity();

            Gl.Disable(EnableCap.Normalize);

            Gl.Enable(EnableCap.Multisample);

            Gl.Disable(EnableCap.Blend);
            Gl.Disable(EnableCap.Dither);
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Gl.ShadeModel(ShadingModel.Smooth);

            Gl.Disable(EnableCap.CullFace);
            // Gl.CullFace(CullFaceMode.Back);
            Gl.Enable(EnableCap.DepthTest);

            Gl.Enable(EnableCap.Lighting);
            Gl.Enable(EnableCap.Light0);
            Gl.Light(LightName.Light0, LightParameter.Ambient, new[] { 0.8f, 0.8f, 0.8f, 1.0f });
            Gl.Light(LightName.Light0, LightParameter.Specular, new[] { 0.5f, 0.5f, 0.5f, 1.0f });
            Gl.Light(LightName.Light0, LightParameter.Diffuse, new[] { 1.0f, 1.0f, 1.0f, 1.0f });
            Gl.Light(LightName.Light0, LightParameter.Position, new[] { 0.3f, 0.5f, 1.0f, 0.0f });

            Gl.MultMatrix(Roller.getMatrix().ToArray());
            Gl.Translate(-MeshCenter.x, -MeshCenter.y, -MeshCenter.z);

            Gl.EnableClientState(EnableCap.VertexArray);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferName);
            Gl.VertexPointer(3, VertexPointerType.Float, MeshVertex.Size, (IntPtr)MeshVertex.CoordOffset);
            Gl.NormalPointer(NormalPointerType.Float, MeshVertex.Size, (IntPtr)MeshVertex.NormalOffset);

            Gl.EnableClientState(EnableCap.NormalArray);
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferName);
            Gl.DrawElements(PrimitiveType.Triangles, Mesh.Faces.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        private static void EstimateBoundingSphare(MeshVertex[] vertices, out Vertex3f center, out float radious)
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

        private static readonly Roller Roller = new Roller();

        private static int LastRotateX, LastRotateY;

        public static void StartRotating(int x, int y)
        {
            LastRotateX = x;
            LastRotateY = y;
            Roller.Pin();
        }

        public static void Rotating(int x, int y)
        {
            Roller.Rotate((x - LastRotateX) * ViewportCanonicalScale, (y - LastRotateY) * ViewportCanonicalScale);
        }

        public static void EndRotating(int x, int y)
        {
            // Do nothing special.
        }

        public static void ResetRotation()
        {
            Roller.StartReset();
        }
    }
}
