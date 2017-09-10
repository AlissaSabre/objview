using System;
using System.Collections.Generic;
using System.Text;

using OpenGL;

namespace objview
{
    /// <summary>
    /// A port/reimplementation of an Android Java code:
    /// com.cocolog_nifty.alissa_sabre.android.hachune_roller.Roller
    /// in hachune-roller-109-src.zip
    /// https://1drv.ms/u/s!AoKnrlLAi4WIgUx2ysZJ6tDKSOeb
    /// </summary>
    public class Roller
    {
        // Adjustable constants.
        private const float ROTATION_SCALE = 100.0f;
        private const float INERTIAL_ROTATION_DECAY = 0.25f;
        private const int RESET_PERIOD = 1000; // in milliseconds

        //// Variables to control decaying inertial rotation after the user's action
        //// (scrolling).
        //private float mInertialRotationSpeed = 0.0f;
        //private float mInertialRotationAxisX = 0.0f;
        //private float mInertialRotationAxisY = 0.0f;

        // Variables to control reset animation
        private int mResetMillis = 0;
        private Quaternion mResetFrom;
        private OvershootInterpolator mResetInterporator = new OvershootInterpolator(0.8f);

        // A variable to handle user's rolling.
        private Quaternion mPinnedRotation;

        // The major Roller states.
        private bool mRotationChanged = true;
        private Quaternion mRotation = Quaternion.Identity;
        private Matrix4x4 mRotationMatrix;

        /// <summary>
        /// Stops any animations and prepare for a manual rotation.
        /// </summary>
        public void Pin()
        {
            //mInertialRotationSpeed = 0.0f;
            mResetMillis = 0;
            mPinnedRotation = mRotation;
        }

        /// <summary>
        /// Rotate an object to follow a displacement vector (x, y).
        /// The rotation is updated immediately.
        /// This method should be called after <see cref="Pin"/>.
        /// This method may be called multiple times after a single call to <see cref="Pin"/>.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Rotate(float x, float y)
        {
            var angle = (float)Math.Sqrt(x * x + y * y) * ROTATION_SCALE;
            mRotation = new Quaternion(new Vertex3f(-y, x, 0.0f), angle) * mPinnedRotation;
            mRotation.Normalize();
            mRotationChanged = true;
        }

        ///// <summary>
        ///// Start an inertial rotation to follow a velocity vector (x, y).
        ///// The rotation continues for a while then stops.
        ///// </summary>
        ///// <param name="x"></param>
        ///// <param name="y"></param>
        //public void RotateInertially(float x, float y)
        //{
        //    mInertialRotationSpeed = (float)Math.Sqrt(x * x + y * y) * ROTATION_SCALE;
        //    mInertialRotationAxisX = -y;
        //    mInertialRotationAxisY = x;
        //}

        /// <summary>
        /// Start an automatic reset rotation.
        /// The reset takes some time (i.e., <see cref="RESET_PERIOD"/> milliseconds) to complete.
        /// </summary>
        public void StartReset()
        {
            mResetFrom = mRotation;
            mResetMillis = RESET_PERIOD;
        }

        /// <summary>
        /// Reset to the initial rotation (identity) immediately.
        /// </summary>
        public void Reset()
        {
            mRotation = Quaternion.Identity;
            //mInertialRotationSpeed = 0.0f;
            mResetMillis = 0;
            mRotationChanged = true;
        }

        /// <summary>
        /// Indicates whether an animation is ongoing.
        /// </summary>
        /// <returns>True if any animation is updating the rotation.</returns>
        /// <remarks>
        /// The UI thread should keep calling <see cref="Update(int)"/> on every frame
        /// as long as <see cref="IsAnimating"/> is true.
        /// </remarks>
        public bool IsAnimating { get { return mResetMillis > 0; } }
        //public bool IsAnimating { get { return mInertialRotationSpeed >= 1.0f || mResetMillis > 0; } }

        /// <summary>
        /// Updates animations.
        /// </summary>
        /// <remarks>
        /// The UI thread should keep calling this method at some reasonable interval as long as <see cref="IsAnimating"/> is true.
        /// The UI thread may call this method when <see cref="IsAnimating"/> is false, though it is not necessary.
        /// </remarks>
        public void Update(int millis)
        {

            //// Take care of inertial rotation.
            //if (mInertialRotationSpeed >= 1.0f)
            //{
            //    mRotation = new Quaternion(new Vertex3f(mInertialRotationAxisX, mInertialRotationAxisY, 0.0f), mInertialRotationSpeed * millis) * mRotation;
            //    mRotation.Normalize();
            //    mInertialRotationSpeed *= (float)Math.Pow(INERTIAL_ROTATION_DECAY, millis / 1000.0f);
            //    mRotationChanged = true;
            //}

            // Take care of reset animation.
            if (mResetMillis > 0)
            {
                if (mResetMillis <= millis)
                {
                    mRotation = Quaternion.Identity;
                    mResetMillis = 0;
                }
                else
                {
                    var s = (RESET_PERIOD - mResetMillis) / (float)RESET_PERIOD;
                    var t = mResetInterporator.getInterpolation(s);
                    mRotation = Lerp(mResetFrom, Quaternion.Identity, t);
                    mRotation.Normalize();
                    mResetMillis -= millis;
                }
                mRotationChanged = true;
            }
        }

        /// <summary>
        /// Return a 4x4 rotation matrix representing the current rotation.
        /// </summary>
        /// <returns>Rotation matrix.</returns>
        /// <remarks>
        /// The returned matrix rotates the model accordingly
        /// when multiplied to the GL model-view matrix.
        /// </remarks>
        public Matrix4x4 getMatrix()
        {
            // Update the cached matrix if any of the roller parameter have been changed.
            if (mRotationChanged)
            {
                mRotationMatrix = (Matrix4x4)mRotation;
                mRotationChanged = false;
            }
            return mRotationMatrix;
        }

        /// <summary>
        /// Interporates between two Quaternions with a mixing factor.
        /// </summary>
        /// <param name="a">Initial quaternion.</param>
        /// <param name="b">Final quaternion.</param>
        /// <param name="t">Mixing facotor in range 0.0 to 1.0.</param>
        /// <returns>Interpolated quaternion, which is not normalized.</returns>
        /// <remarks>
        /// This should be a part of <see cref="Quaternion"/>...
        /// </remarks>
        private static Quaternion Lerp(Quaternion a, Quaternion b, float t)
        {
            var s = 1.0f - t;
            return new Quaternion(
                a.X * s + b.X * t,
                a.Y * s + b.Y * t,
                a.Z * s + b.Z * t,
                a.W * s + b.W * t);
        }
    }
}
