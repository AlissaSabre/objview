using System;
using System.Collections.Generic;
using System.Text;

namespace objview
{
    /// <summary>
    /// A minimal imitation of
    /// android.view.animation.OvershootInterpolator 
    /// </summary>
    public class OvershootInterpolator
    {
        public OvershootInterpolator(float tension)
        {
            K = tension;
        }

        private float K;

        public float getInterpolation(float t)
        {
            var x = t - 1.0f;
            return x * x * (x * (K + 1.0f) + K) + 1.0f;
        }
    }
}
