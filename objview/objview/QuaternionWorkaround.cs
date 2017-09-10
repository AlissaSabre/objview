using System;
using System.Collections.Generic;
using System.Text;

using OpenGL;

namespace objview
{
    // OpenGL.Quaternion.operator * in OpenGL.NET 0.5.2 has an error in the math.
    // https://github.com/luca-piccioni/OpenGL.Net/issues/63
    public static class QuaternionWorkaround
    {
        public static Quaternion Multiply(this Quaternion q1, Quaternion q2)
        {
            double x1 = q1.X, y1 = q1.Y, z1 = q1.Z, w1 = q1.W;
            double x2 = q2.X, y2 = q2.Y, z2 = q2.Z, w2 = q2.W;

            double _q1 = w1 * x2 + x1 * w2 + y1 * z2 - z1 * y2;
            double _q2 = w1 * y2 + y1 * w2 + z1 * x2 - x1 * z2;
            // double _q2 = w1 * y2 + y1 * w2 + x1 * z2 - z1 * x2;
            double _q3 = w1 * z2 + z1 * w2 + x1 * y2 - y1 * x2;
            double _q4 = w1 * w2 - x1 * x2 - y1 * y2 - z1 * z2;

            return (new Quaternion(_q1, _q2, _q3, _q4));
        }
    }
}
