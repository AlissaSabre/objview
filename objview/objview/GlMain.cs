using System;
using System.Collections.Generic;
using System.Text;

using OpenGL;
using System.Runtime.InteropServices;

namespace objview
{
    public static class GlMain
    {
        private static IModel _Model;

        public static IModel Model
        {
            get { return _Model; }
            set
            {
                _Model?.Dispose();
                _Model = value;
                ViewportIsDirty = true;
                Roller.Reset();
            }
        }

        public static bool Animating { get { return Roller.IsAnimating; } }

        private static int ViewportWidth;

        private static int ViewportHeight;

        private static bool ViewportIsDirty;

        private static float ViewportCanonicalScale;

        private static float CameraDistance;

        public static void Initialize()
        {

        }

        public static void Destroy()
        {
            _Model = null;
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
            if (Model == null) return;

            Roller.Update();

            if (ViewportIsDirty)
            {
                Gl.Viewport(0, 0, ViewportWidth, ViewportHeight);

                CameraDistance = CAMERA_DISTANCE_RATIO * Model.BoundingRadius;

                ViewportCanonicalScale = 2f / Math.Min(ViewportWidth, ViewportHeight);
                var s = (double)Model.BoundingRadius * ViewportCanonicalScale / 2;

                Gl.MatrixMode(MatrixMode.Projection);
                Gl.LoadIdentity();
                Gl.Frustum(-ViewportWidth * s, ViewportWidth * s, -ViewportHeight * s, ViewportHeight * s, CameraDistance - Model.BoundingRadius, CameraDistance + Model.BoundingRadius);
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

            Gl.Enable(EnableCap.Lighting);
            Gl.Enable(EnableCap.Light0);
            Gl.Light(LightName.Light0, LightParameter.Ambient, new[] { 0.8f, 0.8f, 0.8f, 1.0f });
            Gl.Light(LightName.Light0, LightParameter.Specular, new[] { 0.5f, 0.5f, 0.5f, 1.0f });
            Gl.Light(LightName.Light0, LightParameter.Diffuse, new[] { 1.0f, 1.0f, 1.0f, 1.0f });
            Gl.Light(LightName.Light0, LightParameter.Position, new[] { 0.3f, 0.5f, 1.0f, 0.0f });

            Gl.MultMatrix(Roller.getMatrix().ToArray());
            Model.Draw();
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
